using UnityEngine;
using Pathfinding;
using Pathfinding.RVO;
using Sirenix.OdinInspector;
using System;

[RequireComponent(typeof(TimeController))]
[RequireComponent(typeof(Seeker))]
[RequireComponent(typeof(RichAI))]
[RequireComponent(typeof(RVOController))]
public abstract class EnemyController<TState> : CharacterControllerBase
{
    [TitleGroup("AI")]
    [SerializeField]
    private Transform _target;
    [SerializeField]
    private Transform _head;
    [SerializeField]
    private float _detectMaxDistance = 10F;
    [SerializeField]
    private float _detectMaxAngle = 90F;

    private TimeController _timeController;
    private RichAI _richAI;
    private Seeker _seeker;
    private Collider _targetCollider;

    private TState _currentState;
    private bool _delayFrame;

    protected override void Awake()
    {
        base.Awake();

        _timeController = GetComponent<TimeController>();
        _richAI = GetComponent<RichAI>();
        _seeker = GetComponent<Seeker>();
        _targetCollider = _target.GetComponent<Collider>();
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

    protected bool IsTargetInView(float maxDistance, float maxAngle)
    {
        Vector3 diff = _target.position - _head.position;
        float sqrDist = diff.sqrMagnitude;
        if (sqrDist < _detectMaxDistance * _detectMaxDistance)
        {
            float angle = Vector3.Angle(Transform.forward, diff);
            if (angle < maxAngle)
            {
                float offset = _targetCollider.bounds.size.y * 0.9F;
                RaycastHit rayHitInfo;
                if (Physics.Raycast(_head.position, diff + new Vector3(0F, offset, 0F), out rayHitInfo, maxDistance, (1 << Target.gameObject.layer | 1 << LayerMask.NameToLayer("Obstacle"))))
                {
                    return rayHitInfo.transform.gameObject.layer == _target.gameObject.layer;
                }
            }
        }

        return false;
    }

    protected abstract void OnStateEnter(TState state);
    protected abstract void OnStateUpdate(TState state);
    protected abstract void OnStateExit(TState state);

    protected Transform Target => _target;
    protected Collider TargetCollider => _targetCollider;
    protected TimeController TimeController => _timeController;
    protected RichAI RichAI => _richAI;
    protected Seeker Seeker => _seeker;

    protected float DetectMaxDistance => _detectMaxDistance;
    protected float DetectMaxAngle => _detectMaxAngle;

    public override float DeltaTime => _timeController.DeltaTime;
}
