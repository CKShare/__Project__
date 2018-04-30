using System;
using UnityEngine;
using Pathfinding;
using Sirenix.OdinInspector;

public class HenchRangeController : EnemyController<HenchRangeState>
{
    [SerializeField]
    private float _detectToChaseDelay = 0.5F;
    [SerializeField]
    private float _chaseKeepDistance = 15F;
    [SerializeField]
    private float _chaseSpeed = 4F;
    [SerializeField]
    private float _combatKeepDistance = 10F;
    [SerializeField]
    private float _combatDistanceError = 1F;
    [SerializeField]
    private float _combatSpeed = 1.5F;
    [SerializeField, Required]
    private RangeWeapon _rangeWeapon;
    [SerializeField]
    private float _attackDelay = 0.5F;

    private float _detectElapsedTime;
    private float _attackElapsedTime;

    protected override void Awake()
    {
        base.Awake();

        Initialize(HenchRangeState.Idle);
    }

    protected override void OnStateEnter(HenchRangeState state)
    {
        switch (state)
        {
            case HenchRangeState.Idle:
                {
                    RichAI.canSearch = false;
                    RichAI.isStopped = true;

                    if (PatrolOnAwake)
                        ChangeState(HenchRangeState.Patrol);
                }
                break;

            case HenchRangeState.Patrol:
                {
                    RichAI.endReachedDistance = PatrolDistanceError;
                    RichAI.maxSpeed = PatrolSpeed;

                    if (PatrolToNearestPoint)
                    {
                        Seeker.StartMultiTargetPath(Transform.position, PatrolPoints, false, OnPathComplete);
                        RichAI.canSearch = false; // Block finding new path to the current patrol point.
                        RichAI.isStopped = true;
                        PatrolToNearestPoint = false;
                    }

                    Animator.SetFloat(Hash.Speed, 0F);
                }
                break;

            case HenchRangeState.Detected:
                {
                    RichAI.isStopped = true;
                    _detectElapsedTime = 0F;
                    Animator.SetTrigger(Hash.Detect);
                }
                break;

            case HenchRangeState.Chase:
                {
                    RichAI.maxSpeed = _chaseSpeed;
                    RichAI.endReachedDistance = _chaseKeepDistance;
                    RichAI.destination = Target.position;
                    RichAI.SearchPath();
                    Animator.SetFloat(Hash.Speed, 0F);
                }
                break;

            case HenchRangeState.Combat:
                {
                    RichAI.isStopped = true;
                    RichAI.canSearch = false;
                    RichAI.updateRotation = false;
                    //RichAI.updatePosition = RichAI.updateRotation = false;
                    _attackElapsedTime = 0F;
                }
                break;

            case HenchRangeState.Hit:
                {
                    RichAI.canSearch = false;
                    RichAI.isStopped = true;
                    Animator.SetTrigger(Hash.Hit);
                }
                break;

            case HenchRangeState.Dead:
                {
                    _rangeWeapon.Drop();

                    RichAI.canSearch = false;
                    RichAI.isStopped = true;
                    Animator.SetBool(Hash.IsDead, true);
                }
                break;
        }
    }

    protected override void OnStateUpdate(HenchRangeState state)
    {
        switch (state)
        {
            case HenchRangeState.Idle:
                {
                    if (IsTargetInView())
                    {
                        ChangeState(HenchRangeState.Detected);
                        return;
                    }
                }
                break;

            case HenchRangeState.Patrol:
                {
                    if (IsTargetInView())
                    {
                        ChangeState(HenchRangeState.Detected);
                        return;
                    }

                    RichAI.destination = PatrolPoints[CurrentPatrolIndex];
                    Animator.SetFloat(Hash.Speed, 1F, 0.1F, TimeController.DeltaTime);

                    if (!RichAI.pathPending && RichAI.reachedEndOfPath)
                    {
                        if (CurrentPatrolIndex == 0)
                            PatrolToReversely = false;
                        else if (CurrentPatrolIndex == PatrolPoints.Length - 1)
                            PatrolToReversely = true;
                        CurrentPatrolIndex += PatrolToReversely ? -1 : 1;

                        RichAI.destination = PatrolPoints[CurrentPatrolIndex];
                        RichAI.SearchPath();
                    }
                }
                break;

            case HenchRangeState.Detected:
                {
                    _detectElapsedTime += TimeController.DeltaTime;
                    if (_detectElapsedTime >= _detectToChaseDelay)
                    {
                        ChangeState(HenchRangeState.Chase);
                        return;
                    }

                    Vector3 dir = Target.position - Transform.position;
                    dir.y = 0F;
                    Transform.rotation = Quaternion.Slerp(Transform.rotation, Quaternion.LookRotation(dir), LookRotateSpeed * TimeController.DeltaTime);
                }
                break;

            case HenchRangeState.Chase:
                {
                    if (!RichAI.pathPending && RichAI.reachedEndOfPath)
                    {
                        ChangeState(HenchRangeState.Combat);
                        return;
                    }

                    RichAI.destination = Target.position;
                    Animator.SetFloat(Hash.Speed, 2F, 0.1F, TimeController.DeltaTime);
                }
                break;

            case HenchRangeState.Combat:
                {
                    Vector3 diff = Target.position - Transform.position;
                    diff.y = 0F;

                    float targetSpeed = 0F;
                    float sqrDist = diff.sqrMagnitude;
                    if (sqrDist > _chaseKeepDistance * _chaseKeepDistance || !IsTargetInView())
                    {
                        ChangeState(HenchRangeState.Chase);
                        return;
                    }
                    else if (sqrDist > _combatKeepDistance * _combatKeepDistance)
                    {
                        targetSpeed = 1F;
                    }
                    else if (sqrDist < (_combatKeepDistance - _combatDistanceError) * (_combatKeepDistance - _combatDistanceError))
                    {
                        targetSpeed = -1F;
                    }

                    Transform.rotation = Quaternion.Slerp(Transform.rotation, Quaternion.LookRotation(diff), LookRotateSpeed * TimeController.DeltaTime);
                    Animator.SetFloat(Hash.Speed, targetSpeed, 0.1F, TimeController.DeltaTime);
                    RichAI.Move(diff.normalized * (targetSpeed * _combatSpeed * TimeController.DeltaTime));

                    _attackElapsedTime += TimeController.DeltaTime;
                    if (_attackElapsedTime >= _attackDelay)
                    {
                        Vector3 targetPos = Target.position + new Vector3(0F, TargetCollider.bounds.size.y * 0.9F, 0F);
                        _rangeWeapon.Attack(Transform, targetPos);
                        Animator.SetTrigger(Hash.Attack);
                        _attackElapsedTime -= _attackDelay;
                    }
                }
                break;
        }
    }

    protected override void OnStateExit(HenchRangeState state)
    {
        switch (state)
        {
            case HenchRangeState.Idle:
                {
                    RichAI.canSearch = true;
                    RichAI.isStopped = false;
                }
                break;

            case HenchRangeState.Patrol:
                {
                    Seeker.CancelCurrentPathRequest();
                }
                break;

            case HenchRangeState.Detected:
                {
                    RichAI.isStopped = false;
                }
                break;

            case HenchRangeState.Combat:
                {
                    RichAI.isStopped = false;
                    RichAI.canSearch = true;
                    RichAI.updateRotation = true;
                }
                break;

            case HenchRangeState.Hit:
                {
                    RichAI.canSearch = true;
                    RichAI.isStopped = false;
                }
                break;
        }
    }

    private void OnPathComplete(Path p)
    {
        if (p.error)
        {
            // Log error.
            return;
        }

        int targetIndex = (p as MultiTargetPath).chosenTarget;
        CurrentPatrolIndex = targetIndex;
        RichAI.canSearch = true;
        RichAI.isStopped = false;
    }

    public override void ApplyDamage(Transform attacker, HitInfo hitInfo)
    {
        if (!IsDead)
        {
            Animator.SetInteger(Hash.ReactionID, hitInfo.ReactionID);
            ChangeState(HenchRangeState.Hit);

            base.ApplyDamage(attacker, hitInfo);
        }
    }

    protected override void OnDeath()
    {
        base.OnDeath();

        ChangeState(HenchRangeState.Dead);
    }

    #region Animator Events

    private void OnHitFinish()
    {
        if (!IsDead)
            ChangeState(HenchRangeState.Combat);
    }

    #endregion
}