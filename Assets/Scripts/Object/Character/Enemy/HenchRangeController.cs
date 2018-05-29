using System;
using UnityEngine;
using Pathfinding;
using Sirenix.OdinInspector;

public enum HenchRangeState
{
    Idle,
    Patrol,
    Detect,
    RunToCover,
    Cover,
    Combat,
    Hit,
    Faint,
    Dead
}

public class HenchRangeController : EnemyController<HenchRangeState>
{
    private static class Hash
    {
        public static readonly int Speed = Animator.StringToHash("Speed");
        public static readonly int IsDetected = Animator.StringToHash("IsDetected");
        public static readonly int IsCrouching = Animator.StringToHash("IsCrouching");
        public static readonly int Trigger = Animator.StringToHash("Trigger");
        public static readonly int GetUpFront = Animator.StringToHash("GetUpFront");
        public static readonly int GetUpBack = Animator.StringToHash("GetUpBack");
        public static readonly int IsDead = Animator.StringToHash("IsDead");
    }

    [SerializeField, Tooltip("참이면, 씬이 시작됬을 때 바로 정찰을 시작함")]
    private bool _patrolOnAwake = false;
    [SerializeField, Tooltip("참이면, 가장 가까운 위치부터 정찰을 시작함")]
    private bool _patrolToNearest = false;
    [SerializeField, Required, Tooltip("정찰 포인트들을 담고있는 컨테이너 오브젝트")]
    private Transform _patrolContainer;
    [SerializeField, Tooltip("정찰 속도")]
    private float _patrolSpeed = 1.5F;
    [SerializeField, Tooltip("정찰 포인트에 도달했다고 가정할 거리")]
    private float _patrolEndReachedDistance = 0.5F;
    [SerializeField, Tooltip("적 감지후 몇초후에 다음 상태로 이어지는가")]
    private float _detectDelay = 1.5F;
    [SerializeField, Tooltip("적을 바라보는 회전 속도")]
    private float _lookRotateSpeed = 10F;
    [SerializeField, Required, Tooltip("엄폐 포인트 오브젝트")]
    private Transform _coverPoint;
    [SerializeField, Tooltip("엄폐 하기위해 달려가는 속도")]
    private float _coverSpeed = 3F;
    [SerializeField, HorizontalGroup("Hide"), Tooltip("엄폐하고있는 최소/최대 시간 (랜덤)")]
    private float _minHideTime, _maxHideTime;
    [SerializeField, HorizontalGroup("FireTime"), Tooltip("엄폐 전 공격하는 최소/최대 시간 (랜덤)")]
    private float _minFireTime, _maxFireTime;
    [SerializeField, HorizontalGroup("FireDelay"), Tooltip("공격간의 최소/최대 시간 (랜덤)")]
    private float _minFireDelay = 0.25F, _maxFireDelay = 1F;
    [SerializeField, Tooltip("후퇴할 때 유지 거리")]
    private float _retreatKeepDistance = 5F;
    [SerializeField, Tooltip("후퇴 속도")]
    private float _retreatSpeed = 1.5F;
    [SerializeField, Required]
    private RangeWeapon _rangeWeapon;

    private Vector3[] _patrolPoints;
    private int _currentPatrolPointIdx;

    private float _detectElapsedTime;
    private float _hideTime;
    private float _hideElapsedTime;
    private float _fireTime;
    private float _fireElapsedTime;
    private float _fireDelay;
    private float _fireElapsedDelay;
    private bool _isStanding;

    protected override void Awake()
    {
        base.Awake();

        // Patrol Points
        _patrolPoints = new Vector3[_patrolContainer.childCount];
        for (int i = 0; i < _patrolPoints.Length; i++)
            _patrolPoints[i] = _patrolContainer.GetChild(i).position;

        _rangeWeapon.Owner = gameObject;

    }

    protected override void Start()
    {
        base.Start();

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
                    TimeControlRichAI.MaxSpeed = _patrolSpeed;
                    
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

            case HenchRangeState.Detect:
                {
                    RichAI.isStopped = true;
                    _detectElapsedTime = 0F;
                    Animator.SetBool(Hash.IsDetected, true);
                    ShowDetectUI();
                }
                break;

            case HenchRangeState.RunToCover:
                {
                    TimeControlRichAI.MaxSpeed = _coverSpeed;
                    RichAI.destination = _coverPoint.position;
                    RichAI.endReachedDistance = 0.1F;
                    RichAI.SearchPath();
                }
                break;

            case HenchRangeState.Cover:
                {
                    if (Vector3.Angle(_coverPoint.forward, Target.position - _coverPoint.position) >= 75F)
                    {
                        ChangeState(HenchRangeState.Combat);
                        return;
                    }

                    RichAI.isStopped = true;
                    RichAI.canSearch = false;
                    Animator.SetBool(Hash.IsCrouching, true);
                    _hideTime = UnityEngine.Random.Range(_minHideTime, _maxHideTime);
                    _hideElapsedTime = 0F;
                    _isStanding = false;
                }
                break;

            case HenchRangeState.Combat:
                {
                    RichAI.isStopped = true;
                    RichAI.canSearch = false;
                    _fireTime = UnityEngine.Random.Range(_minFireTime, _maxFireTime);
                    _fireElapsedTime = 0F;
                    _fireDelay = UnityEngine.Random.Range(_minFireDelay, _maxFireDelay);
                    _fireElapsedDelay = 0F;
                }
                break;

            case HenchRangeState.Hit:
                {
                    RichAI.isStopped = true;
                    RichAI.canSearch = false;
                }
                break;

            case HenchRangeState.Faint:
                {
                    RichAI.isStopped = true;
                    RichAI.canSearch = false;
                    ShowFaintUI();
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
                        ChangeState(HenchRangeState.Detect);
                }
                break;

            case HenchRangeState.Patrol:
                {
                    if (IsTargetInView(DetectMaxDistance, DetectMaxAngle))
                    {
                        ChangeState(HenchRangeState.Detect);
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

            case HenchRangeState.Detect:
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

            case HenchRangeState.RunToCover:
                {
                    float sqrDist = (Target.position - Transform.position).sqrMagnitude;
                    if (sqrDist <= _retreatKeepDistance)
                    {
                        ChangeState(HenchRangeState.Combat);
                        return;
                    }

                    if (!RichAI.pathPending && RichAI.reachedEndOfPath)
                    {
                        ChangeState(HenchRangeState.Cover);
                        return;
                    }
                    
                    Animator.SetFloat(Hash.Speed, 2F * Mathf.Clamp01(RichAI.remainingDistance * 2F), 0.1F, TimeController.DeltaTime);
                }
                break;

            case HenchRangeState.Cover:
                {
                    Vector3 forward = _coverPoint.forward;
                    forward.y = 0F;
                    Transform.rotation = Quaternion.Slerp(Transform.rotation, Quaternion.LookRotation(forward), _lookRotateSpeed * TimeController.DeltaTime);

                    _hideElapsedTime += TimeController.DeltaTime;
                    if ((Target.position - Transform.position).sqrMagnitude <= _retreatKeepDistance || _hideElapsedTime >= _hideTime && IsTargetInView(DetectMaxDistance, DetectMaxAngle))
                    {
                        ChangeState(HenchRangeState.Combat);
                        return;
                    }
                }
                break;

            case HenchRangeState.Combat:
                {
                    Vector3 diff = Target.position - Transform.position;
                    diff.y = 0F;

                    bool isRetreating = false;
                    float targetSpeed = 0;
                    float sqrDist = diff.sqrMagnitude;
                    if (sqrDist < (_retreatKeepDistance * _retreatKeepDistance))
                    {
                        bool isBlocked = false;

                        isRetreating = true;
                        targetSpeed = -1F;
                        diff.Normalize();
                        Vector3 dv = diff * (targetSpeed * _retreatSpeed * TimeController.DeltaTime);
                        float d = dv.magnitude;
                        LayerMask layer = 1 << LayerMask.NameToLayer("Obstacle") | 1 << gameObject.layer;
                        float h = (Collider.height - Collider.radius * 2F) * 0.5F;
                        Vector3 offset = Vector3.up * h;
                        Vector3 center = Collider.bounds.center;
                        Vector3 point1 = center + offset;
                        Vector3 point2 = center - offset;
                        RaycastHit hitInfo;
                        if (Physics.CapsuleCast(point1, point2, Collider.radius, -diff, out hitInfo, d, layer))
                        {
                            if (hitInfo.transform.gameObject != gameObject)
                            {
                                targetSpeed = 0F;
                                dv = Vector3.zero;
                                isBlocked = true;
                            }
                        }

                        if (!isBlocked)
                        {
                            RichAI.Move(dv);
                        }
                    }
                    Transform.rotation = Quaternion.Slerp(Transform.rotation, Quaternion.LookRotation(diff), _lookRotateSpeed * TimeController.DeltaTime);
                    Animator.SetFloat(Hash.Speed, targetSpeed, 0.1F, TimeController.DeltaTime);

                    _fireElapsedTime += TimeController.DeltaTime;
                    if (_fireElapsedTime >= _fireTime && !isRetreating && Vector3.Angle(_coverPoint.forward, Target.position - _coverPoint.position) < 75F)
                    {
                        ChangeState(HenchRangeState.RunToCover);
                        return;

                    }

                    _fireElapsedDelay += TimeController.DeltaTime;
                    if (_fireElapsedDelay >= _fireDelay)
                    {
                        Vector3 targetPos = Target.position + new Vector3(0F, TargetCollider.bounds.size.y * 0.6F, 0F);
                        Vector3 dir = (targetPos - _rangeWeapon.MuzzlePosition).normalized;

                        _rangeWeapon.Trigger(dir);
                        Animator.SetTrigger(Hash.Trigger);
                        _fireElapsedDelay = 0F;
                        _fireDelay = UnityEngine.Random.Range(_minFireDelay, _maxFireDelay);
                    }
                }
                break;

            case HenchRangeState.Hit:
                {
                    if (!HitReaction.inProgress)
                        ChangeState(Animator.GetBool(Hash.IsDetected) ? HenchRangeState.Combat : HenchRangeState.Detect);
                }
                break;

            case HenchRangeState.Faint:
                {

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

            case HenchRangeState.Detect:
                {
                    RichAI.isStopped = false;
                }
                break;

            case HenchRangeState.RunToCover:
                {

                }
                break;

            case HenchRangeState.Cover:
                {
                    RichAI.isStopped = false;
                    RichAI.canSearch = true;
                    Animator.SetBool(Hash.IsCrouching, false);
                }
                break;
                
            case HenchRangeState.Combat:
                {
                    RichAI.isStopped = false;
                    RichAI.canSearch = true;
                }
                break;

            case HenchRangeState.Hit:
                {
                    RichAI.isStopped = false;
                    RichAI.canSearch = true;
                }
                break;

            case HenchRangeState.Faint:
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
            ChangeState(enableRagdoll ? HenchRangeState.Faint : HenchRangeState.Hit);
    }

    public override void ReactToHit(Collider collider, Vector3 point, Vector3 force)
    {
        base.ReactToHit(collider, point, force);

        if (!IsDead)
            ChangeState(HenchRangeState.Hit);
    }

    protected override void OnFaint()
    {
        base.OnFaint();

        Animator.ResetTrigger(Hash.Trigger);
        ChangeState(HenchRangeState.Faint);
    }

    protected override void OnGetUp(bool isFront)
    {
        base.OnGetUp(isFront);

        Animator.SetTrigger(isFront ? Hash.GetUpFront : Hash.GetUpBack);
    }

    public override PhysiqueType PhysiqueType => PhysiqueType.Light;

    #region Animation Events

    private void OnGetUpExit()
    {
        ChangeState(Animator.GetBool(Hash.IsDetected) ? HenchRangeState.Combat : HenchRangeState.Detect);
    }

    private void OnStandUpExit()
    {
        _isStanding = true;
    }

    #endregion
}