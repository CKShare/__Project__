using UnityEngine;
using Pathfinding;
using Pathfinding.RVO;
using Sirenix.OdinInspector;

[RequireComponent(typeof(TimeController))]
[RequireComponent(typeof(Seeker))]
[RequireComponent(typeof(RichAI))]
[RequireComponent(typeof(RVOController))]
public abstract class EnemyController<TState> : ControllerBase
{
    public static class Hash
    {
        public static readonly int TypeID = Animator.StringToHash("TypeID");
        public static readonly int Random = Animator.StringToHash("Random");
        public static readonly int Speed = Animator.StringToHash("Speed");
        public static readonly int Detect = Animator.StringToHash("Detect");
        public static readonly int Attack = Animator.StringToHash("Attack");
        public static readonly int ReactionID = Animator.StringToHash("ReactionID");
        public static readonly int Hit = Animator.StringToHash("Hit");
        public static readonly int IsDead = Animator.StringToHash("IsDead");
    }

    [SerializeField]
    private int _typeID = 0;
    [SerializeField, Required]
    private string _targetTag = "Player";
    [SerializeField]
    private float _detectMaxDistance = 20F;
    [SerializeField]
    private float _detectMaxAngle = 95F;
    [SerializeField]
    private float _detectHeightOffset = 1.5F;
    [SerializeField]
    private float _lookRotateSpeed = 15F;
    [SerializeField]
    private float _patrolSpeed = 2F;
    [SerializeField]
    private float _patrolDistanceError = 0.5F;
    [SerializeField, Required]
    private Transform _patrolContainer;
    [SerializeField]
    private bool _patrolOnAwake;
    [SerializeField]
    private bool _patrolToNearestPoint;
    [SerializeField]
    private bool _patrolReversely;

    private Transform _target;
    private Collider _targetCollider;
    private TimeController _timeController;
    private RichAI _richAI;
    private Seeker _seeker;

    private TState _currentState;
    private bool _delayFrame;

    private int _currentPatrolIndex;
    private Vector3[] _patrolPoints;

    protected override void Awake()
    {
        base.Awake();

        _target = GameObject.FindGameObjectWithTag(_targetTag).transform;
        _targetCollider = _target.GetComponent<Collider>();
        _timeController = GetComponent<TimeController>();
        _richAI = GetComponent<RichAI>();
        _seeker = GetComponent<Seeker>();

        // Convert all the child into the position of them.
        _patrolPoints = new Vector3[_patrolContainer.childCount];
        for (int i = 0; i < _patrolPoints.Length; i++)
            _patrolPoints[i] = _patrolContainer.GetChild(i).position;

        //
        Animator.SetInteger(Hash.TypeID, _typeID);
    }

    protected void Initialize(TState initialState)
    {
        _currentState = initialState;
        OnStateEnter(_currentState);
    }

    protected void ChangeState(TState newState)
    {
        OnStateExit(_currentState);
        _currentState = newState;
        OnStateEnter(_currentState);

        _delayFrame = true;
    }

    protected virtual void Update()
    {
        if (_delayFrame)
        {
            _delayFrame = false;
            return;
        }

        OnStateUpdate(_currentState);
    }

    protected override void OnDeath()
    {
        base.OnDeath();

        GetComponent<Collider>().enabled = false;
        GetComponent<RVOController>().enabled = false;
    }

    protected abstract void OnStateEnter(TState state);
    protected abstract void OnStateUpdate(TState state);
    protected abstract void OnStateExit(TState state);

    protected bool IsTargetInView()
    {
        Vector3 origin = Transform.position + new Vector3(0F, _detectHeightOffset, 0F);
        Vector3 diff = _target.position - origin;

        float sqrDist = diff.sqrMagnitude;
        if (sqrDist < _detectMaxDistance * _detectMaxDistance)
        {
            float angle = Vector3.Angle(Transform.forward, diff);
            if (angle < _detectMaxAngle)
            {
                float offset = _targetCollider.bounds.size.y * 0.9F;
                RaycastHit hitInfo;
                if (Physics.Raycast(origin, diff + new Vector3(0F, offset, 0F), out hitInfo, _detectMaxDistance, ~(1 << gameObject.layer)))
                    return hitInfo.transform.gameObject.layer == _target.gameObject.layer;
            }
        }

        return false;
    }

    protected Transform Target => _target;
    protected Collider TargetCollider => _targetCollider;
    protected TimeController TimeController => _timeController;
    protected RichAI RichAI => _richAI;
    protected Seeker Seeker => _seeker;

    protected float LookRotateSpeed => _lookRotateSpeed;
    protected Vector3[] PatrolPoints => _patrolPoints;
    protected bool PatrolOnAwake => _patrolOnAwake;
    protected float PatrolDistanceError => _patrolDistanceError;
    protected float PatrolSpeed => _patrolSpeed;

    protected bool PatrolToNearestPoint
    {
        get { return _patrolToNearestPoint; }
        set { _patrolToNearestPoint = value; }
    }

    protected bool PatrolToReversely
    {
        get { return _patrolReversely; }
        set { _patrolReversely = value; }
    }

    protected int CurrentPatrolIndex
    {
        get { return _currentPatrolIndex; }
        set { _currentPatrolIndex = value; }
    }

    #region Animator Events

    private void SetColliderActive(BoolParameter active)
    {
        GetComponent<Collider>().enabled = active.Value;
    }

    #endregion
}