using System;
using UnityEngine;
using Pathfinding;
using Sirenix.OdinInspector;

public enum HenchMeleeState
{
    Idle,
    Patrol,
    Chase,
    Combat,
    Raid,
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
    private float _lookRotateSpeed = 10F;
    [SerializeField]
    private float _chaseSpeed = 3F;
    [SerializeField]
    private float _chaseKeepDistance = 8F;
    [SerializeField]
    private float _combatStrafeSpeed = 1.5F;
    [SerializeField]
    private float _combatKeepDistance = 4F;
    [SerializeField]
    private float _combatDistanceError = 1F;
    [SerializeField, MinValue(0F)]
    private float _raidMinDelay, _raidMaxDelay;
    [SerializeField]
    private float _raidRunSpeed = 3F;
    [SerializeField]
    private float _raidKeepDistance = 1F;
    [SerializeField]
    private float _attackDelay = 1F;
    [SerializeField, Required]
    private MeleeWeapon _meleeWeapon;

    private Vector3[] _patrolPoints;
    private int _currentPatrolPointIdx;
    private bool _patrolToNearest;
    
    private float _raidDelay;
    private float _raidElapsedTime;
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
                    TimeControlRichAI.MaxSpeed = _patrolSpeed;
                    
                    if (_patrolToNearest)
                    {
                        Seeker.StartMultiTargetPath(Transform.position, _patrolPoints, false, OnMultiPathComplete);
                        RichAI.canSearch = false; // Block finding new path to the current patrol point.
                        RichAI.isStopped = true;
                        _patrolToNearest = false;
                    }
                }
                break;

            case HenchMeleeState.Chase:
                {
                    TimeControlRichAI.MaxSpeed = _chaseSpeed;
                    RichAI.endReachedDistance = _chaseKeepDistance;
                    RichAI.destination = Target.position;
                    RichAI.SearchPath();
                }
                break;

            case HenchMeleeState.Combat:
                {
                    RichAI.isStopped = true;
                    RichAI.canSearch = false;
                    RichAI.updateRotation = false;
                    Animator.SetBool(Hash.IsDetected, true);
                }
                break;

            case HenchMeleeState.Raid:
                {
                    TimeControlRichAI.MaxSpeed = _raidRunSpeed;
                    RichAI.endReachedDistance = _raidKeepDistance;
                    RichAI.destination = Target.position;
                    RichAI.SearchPath();
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
                        ChangeState(HenchMeleeState.Combat);
                }
                break;

            case HenchMeleeState.Patrol:
                {
                    if (IsTargetInView(DetectMaxDistance, DetectMaxAngle))
                    {
                        ChangeState(HenchMeleeState.Combat);
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

            case HenchMeleeState.Chase:
                {
                    if (!RichAI.pathPending && RichAI.reachedEndOfPath)
                    {
                        ChangeState(HenchMeleeState.Combat);
                        return;
                    }

                    RichAI.destination = Target.position;
                    Animator.SetFloat(Hash.Speed, 2F, 0.1F, TimeController.DeltaTime);
                }
                break;

            case HenchMeleeState.Combat:
                {
                    Vector3 diff = Target.position - Transform.position;
                    diff.y = 0F;

                    bool isBackwards = false;
                    bool isBlocked = false;
                    float targetSpeed = 0F;
                    float sqrDist = diff.sqrMagnitude;
                    if (sqrDist > _chaseKeepDistance * _chaseKeepDistance || !IsTargetInView(_chaseKeepDistance, DetectMaxAngle))
                    {
                        ChangeState(HenchMeleeState.Chase);
                        return;
                    }
                    else if (sqrDist > _combatKeepDistance * _combatKeepDistance)
                    {
                        targetSpeed = 1F;
                    }
                    else if (sqrDist < (_combatKeepDistance - _combatDistanceError) * (_combatKeepDistance - _combatDistanceError))
                    {
                        targetSpeed = -1F;
                        isBackwards = true;
                    }

                    Transform.rotation = Quaternion.Slerp(Transform.rotation, Quaternion.LookRotation(diff), _lookRotateSpeed * TimeController.DeltaTime);

                    Vector3 dv = diff.normalized * (targetSpeed * _combatStrafeSpeed * TimeController.DeltaTime);
                    if (isBackwards)
                    {
                        float d = dv.magnitude;
                        LayerMask layer = LayerMask.NameToLayer("Obstacle");
                        float h = (Collider.height - Collider.radius * 2F) * 0.5F;
                        Vector3 offset = Vector3.up * h;
                        Vector3 center = Collider.bounds.center;
                        Vector3 point1 = center + offset;
                        Vector3 point2 = center - offset;
                        if (Physics.CapsuleCast(point1, point2, Collider.radius, -diff, d, 1 << layer))
                        {
                            targetSpeed = 0F;
                            isBlocked = true;
                        }
                    }

                    if (!isBlocked)
                    {
                        RichAI.Move(diff.normalized * (targetSpeed * _combatStrafeSpeed * TimeController.DeltaTime));
                    }
                    Animator.SetFloat(Hash.Speed, targetSpeed, 0.1F, TimeController.DeltaTime);

                    _raidElapsedTime += TimeController.DeltaTime;
                    if (_raidElapsedTime >= _raidDelay)
                    {
                        //ChangeState(HenchMeleeState.Raid);
                    }
                }
                break;

            case HenchMeleeState.Raid:
                {
                    if (!RichAI.pathPending && RichAI.reachedEndOfPath)
                    {
                        ChangeState(HenchMeleeState.Attack);
                        return;
                    }

                    RichAI.destination = Target.position;
                    Animator.SetFloat(Hash.Speed, 2F, 0.1F, TimeController.DeltaTime);
                }
                break;

            case HenchMeleeState.Hit:
                {
                    if (!HitReaction.inProgress)
                        ChangeState(HenchMeleeState.Combat);
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

            case HenchMeleeState.Combat:
                {
                    RichAI.isStopped = false;
                    RichAI.canSearch = true;
                    RichAI.updateRotation = true;
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
        ChangeState(HenchMeleeState.Combat);
    }

    #endregion
}