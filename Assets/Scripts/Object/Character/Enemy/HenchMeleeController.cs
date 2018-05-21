using System;
using UnityEngine;
using Pathfinding;
using Sirenix.OdinInspector;

public enum HenchMeleeState
{
    Idle,
    Patrol,
    Detected,
    Combat,
    Chase,
    Attack,
    Hit,
    Faint,
    Dead
}

public class HenchMeleeController : EnemyController<HenchMeleeState>
{
    private static class Hash
    {
        public static readonly int Speed = Animator.StringToHash("Speed");
        public static readonly int IsDetected = Animator.StringToHash("IsDetected");
        public static readonly int Attack = Animator.StringToHash("Attack");
        public static readonly int GetUpFront = Animator.StringToHash("GetUpFront");
        public static readonly int GetUpBack = Animator.StringToHash("GetUpBack");
        public static readonly int IsDead = Animator.StringToHash("IsDead");
    }

    [SerializeField]
    private bool _patrolOnAwake = false;
    [SerializeField, Required]
    private Transform _patrolContainer;
    [SerializeField]
    private float _patrolSpeed = 1.5F;
    [SerializeField]
    private float _patrolEndReachedDistance = 0.5F;
    [SerializeField]
    private float _detectDelay = 1.5F;
    [SerializeField]
    private float _lookRotateSpeed = 10F;
    [SerializeField]
    private float _chaseSpeed = 3F;
    [SerializeField]
    private float _chaseKeepDistance = 5F;
    [SerializeField]
    private float _combatStrafeSpeed = 1.5F;
    [SerializeField]
    private float _combatKeepDistance = 2F;
    [SerializeField, Required]
    private MeleeWeapon _meleeWeapon;
    [SerializeField]
    private float _attackDelay = 1F;

    private Vector3[] _patrolPoints;
    private int _currentPatrolPointIdx;
    private bool _patrolToNearest;

    private float _detectElapsedTime;
    private float _attackElapsedTime;

    protected override void Awake()
    {
        base.Awake();

        // Patrol Points
        _patrolPoints = new Vector3[_patrolContainer.childCount];
        for (int i = 0; i < _patrolPoints.Length; i++)
            _patrolPoints[i] = _patrolContainer.GetChild(i).position;

        _meleeWeapon.Owner = gameObject;

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
                    
                    if (_patrolOnAwake)
                        ChangeState(HenchMeleeState.Patrol);
                }
                break;

            case HenchMeleeState.Patrol:
                {
                    RichAI.endReachedDistance = _patrolEndReachedDistance;
                    RichAI.maxSpeed = _patrolSpeed;

                    if (_patrolToNearest)
                    {
                        Seeker.StartMultiTargetPath(Transform.position, _patrolPoints, false, OnMultiPathComplete);
                        RichAI.canSearch = false; // Block finding new path to the current patrol point.
                        RichAI.isStopped = true;
                        _patrolToNearest = false;
                    }

                    Animator.SetFloat(Hash.Speed, 0F);
                }
                break;

            case HenchMeleeState.Detected:
                {
                    RichAI.isStopped = true;
                    _detectElapsedTime = 0F;
                    Animator.SetBool(Hash.IsDetected, true);
                }
                break;

            case HenchMeleeState.Chase:
                {
                    RichAI.endReachedDistance = _combatKeepDistance;
                    RichAI.destination = Target.position;
                    RichAI.SearchPath();
                    Animator.SetFloat(Hash.Speed, 0F);
                }
                break;

            case HenchMeleeState.Combat:
                {
                    RichAI.isStopped = true;
                    RichAI.canSearch = false;
                    _attackElapsedTime = 0F;
                }
                break;

            case HenchMeleeState.Attack:
                {
                    RichAI.canSearch = false;
                    RichAI.isStopped = true;
                    
                    Animator.SetTrigger(Hash.Attack);
                }
                break;

            case HenchMeleeState.Hit:
                {
                    RichAI.isStopped = true;
                    RichAI.canSearch = false;
                }
                break;

            case HenchMeleeState.Faint:
                {
                    RichAI.isStopped = true;
                    RichAI.canSearch = false;
                }
                break;

            case HenchMeleeState.Dead:
                {

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
                    if (IsTargetInView(DetectMaxDistance, DetectMaxAngle))
                        ChangeState(HenchMeleeState.Detected);
                }
                break;

            case HenchMeleeState.Patrol:
                {
                    if (IsTargetInView(DetectMaxDistance, DetectMaxAngle))
                    {
                        ChangeState(HenchMeleeState.Detected);
                        return;
                    }

                    RichAI.destination = _patrolPoints[_currentPatrolPointIdx];
                    Animator.SetFloat(Hash.Speed, 1F, 0.1F, TimeController.DeltaTime);

                    if (!RichAI.pathPending && RichAI.reachedEndOfPath)
                    {
                        _currentPatrolPointIdx = (_currentPatrolPointIdx + 1) % _patrolPoints.Length;
                        RichAI.destination = _patrolPoints[_currentPatrolPointIdx];
                        RichAI.SearchPath();
                    }
                }
                break;

            case HenchMeleeState.Detected:
                {
                    _detectElapsedTime += TimeController.DeltaTime;
                    if (_detectElapsedTime >= _detectDelay)
                    {
                        ChangeState(HenchMeleeState.Combat);
                        return;
                    }

                    Vector3 dir = Target.position - Transform.position;
                    dir.y = 0F;
                    Transform.rotation = Quaternion.Slerp(Transform.rotation, Quaternion.LookRotation(dir), _lookRotateSpeed * TimeController.DeltaTime);
                }
                break;

            case HenchMeleeState.Chase:
                {
                    if (!RichAI.pathPending && RichAI.reachedEndOfPath)
                    {
                        ChangeState(HenchMeleeState.Combat);
                        return;
                    }

                    bool isFast = RichAI.remainingDistance > _chaseKeepDistance;
                    RichAI.maxSpeed = isFast ? _chaseSpeed : _combatStrafeSpeed;
                    RichAI.destination = Target.position;
                    Animator.SetFloat(Hash.Speed, isFast ? 2F : 1F, 0.1F, TimeController.DeltaTime);
                }
                break;

            case HenchMeleeState.Combat:
                {
                    Vector3 diff = Target.position - Transform.position;
                    diff.y = 0F;
                    if (diff.sqrMagnitude > _combatKeepDistance * _combatKeepDistance || !IsTargetInView(_combatKeepDistance, DetectMaxAngle))
                    {
                        ChangeState(HenchMeleeState.Chase);
                        return;
                    }

                    Transform.rotation = Quaternion.Slerp(Transform.rotation, Quaternion.LookRotation(diff), _lookRotateSpeed * TimeController.DeltaTime);
                    Animator.SetFloat(Hash.Speed, 0F, 0.1F, TimeController.DeltaTime);

                    _attackElapsedTime += TimeController.DeltaTime;
                    if (_attackElapsedTime >= _attackDelay)
                    {
                        ChangeState(HenchMeleeState.Attack);
                        return;
                    }

                }
                break;

            case HenchMeleeState.Hit:
                {
                    if (!HitReaction.inProgress)
                        ChangeState(Animator.GetBool(Hash.IsDetected) ? HenchMeleeState.Combat : HenchMeleeState.Detected);
                }
                break;

            case HenchMeleeState.Faint:
                {
                 
                }
                break;

            case HenchMeleeState.Dead:
                {

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
                    RichAI.isStopped = false;
                    RichAI.canSearch = true;
                }
                break;

            case HenchMeleeState.Faint:
                {
                    RichAI.isStopped = false;
                    RichAI.canSearch = true;
                }
                break;

            case HenchMeleeState.Dead:
                {

                }
                break;
        }
    }

    private void OnMultiPathComplete(Path p)
    {
        if (p.error)
        {
            // Log error.
            return;
        }

        int targetIndex = (p as MultiTargetPath).chosenTarget;
        _currentPatrolPointIdx = targetIndex;
        RichAI.canSearch = true;
        RichAI.isStopped = false;
    }

    public override void ReactToHit(BoneType boneType, Vector3 point, Vector3 force, bool enableRagdoll)
    {
        base.ReactToHit(boneType, point, force, enableRagdoll);

        if (!IsDead)
            ChangeState(enableRagdoll ? HenchMeleeState.Faint : HenchMeleeState.Hit);
    }

    public override void ReactToHit(Collider collider, Vector3 point, Vector3 force)
    {
        base.ReactToHit(collider, point, force);

        if (!IsDead)
            ChangeState(HenchMeleeState.Hit);
    }

    protected override void OnFaint()
    {
        base.OnFaint();

        Animator.ResetTrigger(Hash.Attack);
        ChangeState(HenchMeleeState.Faint);
    }

    protected override void OnGetUp(bool isFront)
    {
        base.OnGetUp(isFront);

        Animator.SetTrigger(isFront ? Hash.GetUpFront : Hash.GetUpBack);
    }

    public override PhysiqueType PhysiqueType => PhysiqueType.Light;

    #region Animation Events

    private void OnAttackExit()
    {
        ChangeState(HenchMeleeState.Combat);
    }

    private void CheckHit(int attackID)
    {
        _meleeWeapon.Hit(Target.gameObject, attackID);
    }

    private void OnGetUpExit()
    {
        ChangeState(Animator.GetBool(Hash.IsDetected) ? HenchMeleeState.Combat : HenchMeleeState.Detected);
    }

    #endregion
}