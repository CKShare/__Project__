using System;
using UnityEngine;
using Pathfinding;
using Sirenix.OdinInspector;

public enum HenchRangeState
{
    Idle,
    Patrol,
    Detected,
    Combat,
    Chase,
    Hit,
    Dead
}

public class HenchRangeController : EnemyController<HenchRangeState>
{
    private static class Hash
    {
        public static readonly int Speed = Animator.StringToHash("Speed");
        public static readonly int IsDetected = Animator.StringToHash("IsDetected");
        public static readonly int Trigger = Animator.StringToHash("Trigger");
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
    private float _chaseKeepDistance = 15F;
    [SerializeField]
    private float _combatStrafeSpeed = 1.5F;
    [SerializeField]
    private float _combatKeepDistance = 10F;
    [SerializeField]
    private float _combatDistanceError = 1F;
    [SerializeField, Required]
    private RangeWeapon _rangeWeapon;
    [SerializeField]
    private float _minFireDelay = 0.25F, _maxFireDelay = 1F;

    private Vector3[] _patrolPoints;
    private int _currentPatrolPointIdx;
    private bool _patrolToNearest;

    private float _detectElapsedTime;
    private float _fireDelay;
    private float _fireElapsedTime;

    protected override void Awake()
    {
        base.Awake();

        // Patrol Points
        _patrolPoints = new Vector3[_patrolContainer.childCount];
        for (int i = 0; i < _patrolPoints.Length; i++)
            _patrolPoints[i] = _patrolContainer.GetChild(i).position;

        _rangeWeapon.Owner = gameObject;

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
                    
                    if (_patrolOnAwake)
                        ChangeState(HenchRangeState.Patrol);
                }
                break;

            case HenchRangeState.Patrol:
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

            case HenchRangeState.Detected:
                {
                    RichAI.isStopped = true;
                    _detectElapsedTime = 0F;
                    Animator.SetBool(Hash.IsDetected, true);
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
                    _fireDelay = UnityEngine.Random.Range(_minFireDelay, _maxFireDelay);
                    _fireElapsedTime = 0F;
                }
                break;

            case HenchRangeState.Hit:
                {
                    RichAI.isStopped = true;
                    RichAI.canSearch = false;
                }
                break;

            case HenchRangeState.Dead:
                {

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
                    if (IsTargetInView(DetectMaxDistance, DetectMaxAngle))
                        ChangeState(HenchRangeState.Detected);
                }
                break;

            case HenchRangeState.Patrol:
                {
                    if (IsTargetInView(DetectMaxDistance, DetectMaxAngle))
                    {
                        ChangeState(HenchRangeState.Detected);
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

            case HenchRangeState.Detected:
                {
                    _detectElapsedTime += TimeController.DeltaTime;
                    if (_detectElapsedTime >= _detectDelay)
                    {
                        ChangeState(HenchRangeState.Combat);
                        return;
                    }

                    Vector3 dir = Target.position - Transform.position;
                    dir.y = 0F;
                    Transform.rotation = Quaternion.Slerp(Transform.rotation, Quaternion.LookRotation(dir), _lookRotateSpeed * TimeController.DeltaTime);
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

                    bool isBackwards = false;
                    bool isBlocked = false;
                    float targetSpeed = 0F;
                    float sqrDist = diff.sqrMagnitude;
                    if (sqrDist > _chaseKeepDistance * _chaseKeepDistance || !IsTargetInView(_chaseKeepDistance, DetectMaxAngle))
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

                    _fireElapsedTime += TimeController.DeltaTime;
                    if (_fireElapsedTime >= _fireDelay)
                    {
                        Vector3 targetPos = Target.position + new Vector3(0F, TargetCollider.bounds.size.y * 0.6F, 0F);
                        Vector3 dir = (targetPos - _rangeWeapon.MuzzlePosition).normalized;

                        _rangeWeapon.Trigger(dir);
                        Animator.SetTrigger(Hash.Trigger);
                        _fireElapsedTime = 0F;
                        _fireDelay = UnityEngine.Random.Range(_minFireDelay, _maxFireDelay);
                    }
                }
                break;

            case HenchRangeState.Hit:
                {
                    if (!HitReaction.inProgress)
                    {
                        if (Animator.GetBool(Hash.IsDetected))
                        {
                            ChangeState(HenchRangeState.Combat);
                        }
                        else
                        {
                            ChangeState(HenchRangeState.Detected);
                        }
                    }
                }
                break;

            case HenchRangeState.Dead:
                {

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

            case HenchRangeState.Chase:
                {
                    Animator.SetFloat(Hash.Speed, 0F);
                }
                break;
                
            case HenchRangeState.Combat:
                {
                    RichAI.isStopped = false;
                    RichAI.canSearch = true;
                    RichAI.updateRotation = true;
                    Animator.SetFloat(Hash.Speed, 0F);
                }
                break;

            case HenchRangeState.Hit:
                {
                    RichAI.isStopped = false;
                    RichAI.canSearch = true;
                }
                break;

            case HenchRangeState.Dead:
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
            ChangeState(HenchRangeState.Hit);
    }

    public override void ReactToHit(Collider collider, Vector3 point, Vector3 force)
    {
        base.ReactToHit(collider, point, force);

        if (!IsDead)
            ChangeState(HenchRangeState.Hit);
    }

    public override PhysiqueType PhysiqueType => PhysiqueType.Light;
}