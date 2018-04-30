using System;
using UnityEngine;
using Pathfinding;
using Sirenix.OdinInspector;

[RequireComponent(typeof(MeleeAttacker))]
public class HenchMeleeController : EnemyController<HenchMeleeState>
{
    [SerializeField]
    private float _detectToChaseDelay = 1F;
    [SerializeField]
    private float _chaseKeepDistance = 2F;
    [SerializeField]
    private float _chaseFastToSlowThreshold = 5F;
    [SerializeField]
    private float _chaseFastSpeed = 4F;
    [SerializeField]
    private float _chaseSlowSpeed = 2F;
    [SerializeField, Required]
    private Weapon _meleeWeapon;
    [SerializeField]
    private float _attackDelay = 1F;

    private MeleeAttacker _attacker;
    private float _detectElapsedTime;
    private float _attackElapsedTime;

    protected override void Awake()
    {
        base.Awake();

        _attacker = GetComponent<MeleeAttacker>();

        Initialize(HenchMeleeState.Idle);
    }

    protected override void OnStateEnter(HenchMeleeState state)
    {
        switch (state)
        {
            case HenchMeleeState.Idle:
                {
                    RichAI.canSearch = false;
                    RichAI.isStopped = true;

                    if (PatrolOnAwake)
                        ChangeState(HenchMeleeState.Patrol);
                }
                break;

            case HenchMeleeState.Patrol:
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

            case HenchMeleeState.Detected:
                {
                    RichAI.isStopped = true;
                    _detectElapsedTime = 0F;
                    Animator.SetTrigger(Hash.Detect);
                }
                break;

            case HenchMeleeState.Chase:
                {
                    RichAI.endReachedDistance = _chaseKeepDistance;
                    RichAI.destination = Target.position;
                    RichAI.SearchPath();
                    Animator.SetFloat(Hash.Speed, 0F);
                }
                break;

            case HenchMeleeState.Combat:
                {
                    RichAI.canSearch = false;
                    RichAI.isStopped = true;
                    _attackElapsedTime = 0F;
                }
                break;

            case HenchMeleeState.Attack:
                {
                    RichAI.canSearch = false;
                    RichAI.isStopped = true;

                    int attackID = UnityEngine.Random.Range(1, _attacker.AttackCount + 1);
                    Animator.SetInteger(Hash.Random, attackID);
                    Animator.SetTrigger(Hash.Attack);
                    _attacker.Attack(Target.gameObject, attackID);
                }
                break;

            case HenchMeleeState.Hit:
                {
                    RichAI.canSearch = false;
                    RichAI.isStopped = true;
                    Animator.SetTrigger(Hash.Hit);
                }
                break;

            case HenchMeleeState.Dead:
                {
                    _meleeWeapon.Drop();

                    RichAI.canSearch = false;
                    RichAI.isStopped = true;
                    Animator.SetBool(Hash.IsDead, true);
                }
                break;
        }
    }

    protected override void OnStateUpdate(HenchMeleeState state)
    {
        switch (state)
        {
            case HenchMeleeState.Idle:
                {
                    if (IsTargetInView())
                    {
                        ChangeState(HenchMeleeState.Detected);
                        return;
                    }
                }
                break;

            case HenchMeleeState.Patrol:
                {
                    if (IsTargetInView())
                    {
                        ChangeState(HenchMeleeState.Detected);
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

            case HenchMeleeState.Detected:
                {
                    _detectElapsedTime += TimeController.DeltaTime;
                    if (_detectElapsedTime >= _detectToChaseDelay)
                    {
                        ChangeState(HenchMeleeState.Chase);
                        return;
                    }

                    Vector3 dir = Target.position - Transform.position;
                    dir.y = 0F;
                    Transform.rotation = Quaternion.Slerp(Transform.rotation, Quaternion.LookRotation(dir), LookRotateSpeed * TimeController.DeltaTime);
                }
                break;

            case HenchMeleeState.Chase:
                {
                    if (!RichAI.pathPending && RichAI.reachedEndOfPath)
                    {
                        ChangeState(HenchMeleeState.Combat);
                        return;
                    }

                    bool isFast = RichAI.remainingDistance > _chaseFastToSlowThreshold;
                    RichAI.maxSpeed = isFast ? _chaseFastSpeed : _chaseSlowSpeed;
                    RichAI.destination = Target.position;
                    Animator.SetFloat(Hash.Speed, isFast ? 2F : 1F, 0.1F, TimeController.DeltaTime);
                }
                break;

            case HenchMeleeState.Combat:
                {
                    _attackElapsedTime += TimeController.DeltaTime;
                    if (_attackElapsedTime >= _attackDelay)
                    {
                        ChangeState(HenchMeleeState.Attack);
                        return;
                    }

                    Vector3 diff = Target.position - Transform.position;
                    diff.y = 0F;
                    if (diff.sqrMagnitude > _chaseKeepDistance * _chaseKeepDistance || !IsTargetInView())
                    {
                        ChangeState(HenchMeleeState.Chase);
                        return;
                    }

                    Transform.rotation = Quaternion.Slerp(Transform.rotation, Quaternion.LookRotation(diff), LookRotateSpeed * TimeController.DeltaTime);
                    Animator.SetFloat(Hash.Speed, 0F, 0.1F, TimeController.DeltaTime);
                }
                break;
        }
    }

    protected override void OnStateExit(HenchMeleeState state)
    {
        switch (state)
        {
            case HenchMeleeState.Idle:
                {
                    RichAI.canSearch = true;
                    RichAI.isStopped = false;
                }
                break;

            case HenchMeleeState.Patrol:
                {
                    Seeker.CancelCurrentPathRequest();
                }
                break;

            case HenchMeleeState.Detected:
                {
                    RichAI.isStopped = false;
                }
                break;

            case HenchMeleeState.Combat:
                {
                    RichAI.canSearch = true;
                    RichAI.isStopped = false;
                }
                break;

            case HenchMeleeState.Attack:
                {
                    RichAI.canSearch = true;
                    RichAI.isStopped = false;
                }
                break;

            case HenchMeleeState.Hit:
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
            ChangeState(HenchMeleeState.Hit);

            base.ApplyDamage(attacker, hitInfo);
        }
    }

    protected override void OnDeath()
    {
        base.OnDeath();

        ChangeState(HenchMeleeState.Dead);
    }

    #region Animator Events

    private void OnAttackFinish()
    {
        ChangeState(HenchMeleeState.Combat);
    }

    private void OnHitFinish()
    {
        if (!IsDead)
            ChangeState(HenchMeleeState.Combat);
    }

    #endregion
}