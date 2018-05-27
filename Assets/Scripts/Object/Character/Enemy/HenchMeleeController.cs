using UnityEngine;
using Pathfinding;
using Sirenix.OdinInspector;

public enum HenchMeleeState
{
    Idle,
    Patrol,
    Detect,
    Chase,
    Combat,
    Retreat,
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
        public static readonly int AttackNumber = Animator.StringToHash("AttackNumber");
        public static readonly int GetUpFront = Animator.StringToHash("GetUpFront");
        public static readonly int GetUpBack = Animator.StringToHash("GetUpBack");
        public static readonly int IsDead = Animator.StringToHash("IsDead");
    }

    [SerializeField, Tooltip("참이면, 씬이 시작됬을 때 바로 정찰을 시작함")]
    private bool _patrolOnAwake = false;
    [SerializeField, Required, Tooltip("정찰 포인트들을 담고있는 컨테이너 오브젝트")]
    private Transform _patrolContainer;
    [SerializeField, Tooltip("정찰 속도")]
    private float _patrolSpeed = 1.5F;
    [SerializeField, Tooltip("정찰 포인트에 도달했다고 가정할 거리")]
    private float _patrolEndReachedDistance = 0.5F;
    [SerializeField, Tooltip("적 감지후 몇초후에 다음 상태로 이어지는가")]
    private float _detectDelay = 0.5F;
    [SerializeField, Tooltip("적을 바라보는 회전 속도")]
    private float _lookRotateSpeed = 10F;
    [SerializeField, Tooltip("적 추적 속도")]
    private float _chaseSpeed = 3F;
    [SerializeField, Tooltip("적 추적 후 유지할 거리")]
    private float _chaseKeepDistance = 1.5F;
    [SerializeField, Tooltip("후퇴 속도")]
    private float _retreatSpeed = 1.5F;
    [SerializeField, Tooltip("후퇴할 때 유지 거리")]
    private float _retreatKeepDistance = 4F;
    [SerializeField, Tooltip("이 값이 작으면 적이 살짝만 움직여도 후퇴를 함")]
    private float _retreatKeepDistanceError = 0.5F;
    [SerializeField, MinValue(0F), HorizontalGroup("Pattern"), Tooltip("한 패턴 이후 다음 패턴까지의 최소/최대 시간 (랜덤)")]
    private float _patternMinDelay, _patternMaxDelay;
    [SerializeField, MinValue(0F), HorizontalGroup("Attack"), Tooltip("공격간의 최소/최대 시간 (랜덤)")]
    private float _attackMinDelay, _attackMaxDelay;
    [SerializeField, Required]
    private MeleeWeapon _meleeWeapon;

    private Vector3[] _patrolPoints;
    private int _currentPatrolPointIdx;
    private bool _patrolToNearest;

    private float _detectElapsedTime;
    private float _patternDelay;
    private float _patternElapsedTime;
    private float _attackDelay;
    private float _attackElapsedTime;
    private int _attackPattern;
    private int _attackNumber;
    private int _maxAttackNumber;
    private bool _isAttacking;

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

            case HenchMeleeState.Detect:
                {
                    RichAI.isStopped = true;
                    _detectElapsedTime = 0F;
                    Animator.SetBool(Hash.IsDetected, true);
                    ShowDetectUI();
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
                    _patternDelay = UnityEngine.Random.Range(_patternMinDelay, _patternMaxDelay);
                    _patternElapsedTime = 0F;
                    Animator.SetBool(Hash.IsDetected, true);
                }
                break;

            case HenchMeleeState.Retreat:
                {
                    RichAI.isStopped = true;
                    RichAI.canSearch = false;
                }
                break;

            case HenchMeleeState.Attack:
                {
                    RichAI.canSearch = false;
                    RichAI.isStopped = true;
                    Animator.SetFloat(Hash.Speed, 0F);
                    _attackPattern = UnityEngine.Random.Range(1, 4 + 1);
                    _attackDelay = UnityEngine.Random.Range(_attackMinDelay, _attackMaxDelay);
                    _attackElapsedTime = _attackDelay;
                    _attackNumber = _attackPattern == 1 || _attackPattern == 3 ? 0 : 3;
                    
                    switch (_attackPattern)
                    {
                        case 1:
                            _maxAttackNumber = 3;
                            break;
                        case 2:
                            _maxAttackNumber = 0;
                            break;
                        case 3:
                            _maxAttackNumber = 2;
                            break;
                        case 4:
                            _maxAttackNumber = 1;
                            break;
                    }
                }
                break;

            case HenchMeleeState.Hit:
                {
                    RichAI.isStopped = true;
                    RichAI.canSearch = false;
                    Animator.SetFloat(Hash.Speed, 0F);
                }
                break;

            case HenchMeleeState.Faint:
                {
                    RichAI.isStopped = true;
                    RichAI.canSearch = false;
                    ShowFaintUI();
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
                        ChangeState(HenchMeleeState.Detect);
                }
                break;

            case HenchMeleeState.Patrol:
                {
                    if (IsTargetInView(DetectMaxDistance, DetectMaxAngle))
                    {
                        ChangeState(HenchMeleeState.Detect);
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

            case HenchMeleeState.Detect:
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
                    if (TargetController.IsAttacking && RichAI.remainingDistance <= _retreatKeepDistance)
                    {
                        ChangeState(HenchMeleeState.Retreat);
                        return;
                    }

                    if (!RichAI.pathPending && RichAI.reachedEndOfPath)
                    {
                        ChangeState(HenchMeleeState.Combat);
                        return;
                    }
                    
                    Animator.SetFloat(Hash.Speed, 2F, 0.1F, TimeController.DeltaTime);
                }
                break;

            case HenchMeleeState.Combat:
                {
                    if (!IsTargetInView(_chaseKeepDistance, DetectMaxAngle))
                    {
                        ChangeState(HenchMeleeState.Chase);
                        return;
                    }
                    else if (TargetController.IsAttacking) // If player is attacking,
                    {
                        ChangeState(HenchMeleeState.Retreat);
                        return;
                    }

                    Vector3 diff = Target.position - Transform.position;
                    diff.y = 0F;
                    Transform.rotation = Quaternion.Slerp(Transform.rotation, Quaternion.LookRotation(diff), _lookRotateSpeed * TimeController.DeltaTime);
                    Animator.SetFloat(Hash.Speed, 0F, 0.1F, TimeController.DeltaTime);

                    _patternElapsedTime += TimeController.DeltaTime;
                    if (_patternElapsedTime >= _patternDelay)
                    {
                        ChangeState(HenchMeleeState.Attack);
                        return;
                    }
                }
                break;

            case HenchMeleeState.Attack:
                {
                    if (!_isAttacking)
                    {
                        Vector3 diff = Target.position - Transform.position;
                        int nextNumber = _attackNumber + (_attackPattern == 1 || _attackPattern == 3 ? 1 : -1);
                        if (nextNumber == _maxAttackNumber || !IsTargetInView(_chaseKeepDistance, DetectMaxAngle))
                        {
                            ChangeState(HenchMeleeState.Combat);
                            return;
                        }

                        _attackElapsedTime += TimeController.DeltaTime;
                        if (_attackElapsedTime >= _attackDelay)
                        {
                            Animator.SetInteger(Hash.AttackNumber, nextNumber);
                            Animator.SetTrigger(Hash.Attack);
                            _attackNumber = nextNumber;
                            _isAttacking = true;
                            _attackElapsedTime = 0F;
                        }
                    }
                }
                break;

            case HenchMeleeState.Retreat:
                {
                    if (!TargetController.IsAttacking)
                    {
                        ChangeState(HenchMeleeState.Combat);
                        return;
                    }

                    Vector3 diff = Target.position - Transform.position;
                    diff.y = 0F;

                    bool isBlocked = false;
                    bool isBackwards = false;
                    float targetSpeed = 0;
                    float sqrDist = diff.sqrMagnitude;
                    if (sqrDist > _retreatKeepDistance * _retreatKeepDistance)
                    {
                        targetSpeed = 1F;
                    }
                    else if (sqrDist < (_retreatKeepDistance - _retreatKeepDistanceError) * (_retreatKeepDistance - _retreatKeepDistanceError))
                    {
                        targetSpeed = -1F;
                        isBackwards = true;
                    }

                    diff.Normalize();

                    Vector3 dv = diff * (targetSpeed * _retreatSpeed * TimeController.DeltaTime);
                    if (isBackwards)
                    {
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
                    }

                    if (!isBlocked)
                    {
                        RichAI.Move(dv);
                    }
                    Animator.SetFloat(Hash.Speed, targetSpeed, 0.1F, TimeController.DeltaTime);
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

            case HenchMeleeState.Detect:
                {
                    RichAI.isStopped = false;
                }
                break;

            case HenchMeleeState.Combat:
                {
                    RichAI.isStopped = false;
                    RichAI.canSearch = true;
                }
                break;

            case HenchMeleeState.Attack:
                {
                    RichAI.canSearch = true;
                    RichAI.isStopped = false;
                    _isAttacking = false;
                }
                break;

            case HenchMeleeState.Retreat:
                {
                    RichAI.isStopped = false;
                    RichAI.canSearch = true;
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
        _isAttacking = false;
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