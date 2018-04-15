using System.Collections;
using UnityEngine;
using Pathfinding;
using Sirenix.OdinInspector;
using static AIAnimatorInfo;

[RequireComponent(typeof(TimeController))]
[RequireComponent(typeof(AI))]
public class AIController : ControllerBase, IPerceivable
{
    [SerializeField, Required]
    private Transform _target;
    [SerializeField]
    private float _targetDetectMaxDistance;
    [SerializeField]
    private float _targetDetectMaxAngle;
    [SerializeField]
    private float _targetDetectRayOffset;
    [SerializeField]
    private float _targetLookRotateSpeed;
    [SerializeField]
    private float _patrolSpeed;
    [SerializeField]
    private float _patrolDistanceError;
    [SerializeField, MinMaxSlider(0F, int.MaxValue, true)]
    private Vector2 _patrolDelayMinMax;
    [SerializeField]
    private float _chaseSpeed;
    [SerializeField]
    private float _chaseKeepDistance;
    [SerializeField]
    private float _chaseDelay;
    [SerializeField]
    private float _attackDelay;
    [SerializeField]
    private Transform[] _patrolPoints;

    private AI _ai;
    private RichAI _pathfinder;
    private TimeController _timeController;
    private Collider _targetCollider;

    private AIState _currentState = AIState.None;
    private int _currentPointIdx = -1;
    private bool _isPatrolReversed = false;
    private float _patrolDelay;
    private float _patrolElapsedDelay;
    private float _chaseElapsedDelay;
    private float _attackElapsedDelay;
    private bool _isTargetDetected = false;
    private bool _isCombatIdle = false;
    private int _reactionID = -1;

    protected override void Awake()
    {
        base.Awake();

        _ai = Character as AI;
        _pathfinder = GetComponent<RichAI>();
        _timeController = GetComponent<TimeController>();
        _targetCollider = _target.GetComponent<Collider>();
    }

    private void Start()
    {
        ChangeState(AIState.Idle);
    }

    private void Update()
    {
        OnStateUpdate(_currentState);
    }

    private void ChangeState(AIState state)
    {
        if (_currentState != AIState.None)
            OnStateExit(_currentState);
        _currentState = state;
        OnStateEnter(_currentState);
    }

    private void OnStateEnter(AIState state)
    {
        switch (state)
        {
            case AIState.Idle:
                {
                    if (_patrolPoints.Length > 0)
                    {
                        _patrolDelay = Random.Range(_patrolDelayMinMax.x, _patrolDelayMinMax.y);
                        _patrolElapsedDelay = 0F;
                    }
                    break;
                }

            case AIState.Patrol:
                {
                    if (_currentPointIdx == 0)
                        _isPatrolReversed = false;
                    else if (_currentPointIdx == _patrolPoints.Length - 1)
                        _isPatrolReversed = true;

                    _currentPointIdx += _isPatrolReversed ? -1 : 1;
                    _pathfinder.destination = _patrolPoints[_currentPointIdx].position;
                    _pathfinder.endReachedDistance = _patrolDistanceError;
                    _pathfinder.maxSpeed = _patrolSpeed;
                    _pathfinder.SearchPath();
                    Animator.SetBool(Hash.IsWalking, true);
                    break;
                }

            case AIState.Combat:
                {
                    _attackElapsedDelay = _attackDelay * 0.5F;
                    _pathfinder.isStopped = true;
                    Animator.SetTrigger(Hash.CombatState);
                    break;
                }

            case AIState.Chase:
                {
                    _isTargetDetected = true;
                    _chaseElapsedDelay = 0F;
                    _pathfinder.isStopped = true;
                    _pathfinder.endReachedDistance = _chaseKeepDistance;
                    _pathfinder.maxSpeed = _chaseSpeed;
                    _pathfinder.destination = _target.position;
                    _pathfinder.SearchPath();
                    break;
                }

            case AIState.Hit:
                {
                    _pathfinder.canMove = _pathfinder.canSearch = false;
                    Animator.SetInteger(Hash.ReactionID, _reactionID);
                    Animator.SetTrigger(Hash.Hit);
                    Transform.LookAt(_target, Vector3.up);
                    break;
                }

            case AIState.Dead:
                {
                    _pathfinder.canMove = _pathfinder.canSearch = false;
                    Animator.SetBool(Hash.IsDead, true);
                    break;
                }
        }
    }

    private void OnStateUpdate(AIState state)
    {
        switch (state)
        {
            case AIState.Idle:
                {
                    if (_isTargetDetected || IsTargetInView())
                    {
                        ChangeState(AIState.Combat);
                    }
                    else if (_patrolPoints.Length > 0)
                    {
                        _patrolElapsedDelay += _timeController.DeltaTime;
                        if (_patrolElapsedDelay > _patrolDelay)
                            ChangeState(AIState.Patrol);
                    }
                    break;
                }

            case AIState.Patrol:
                {
                    if (_isTargetDetected || IsTargetInView())
                    {
                        ChangeState(AIState.Combat);
                    }
                    else
                    {
                        if (!_pathfinder.pathPending && _pathfinder.reachedEndOfPath)
                            ChangeState(AIState.Idle);
                    }
                    break;
                }

            case AIState.Combat:
                {
                    Vector3 diff = _target.position - Transform.position;
                    diff.y = 0F;

                    float distance = diff.sqrMagnitude;
                    if (distance > _chaseKeepDistance * _chaseKeepDistance || !IsTargetInView())
                    {
                        ChangeState(AIState.Chase);
                    }
                    else
                    {
                        if (_attackElapsedDelay >= _attackDelay)
                        {
                            Animator.SetTrigger(Hash.Attack);
                            _attackElapsedDelay -= _attackDelay;
                        }

                        _attackElapsedDelay += _timeController.DeltaTime;
                    }

                    Transform.rotation = Quaternion.Slerp(Transform.rotation, Quaternion.LookRotation(diff), _timeController.DeltaTime * _targetLookRotateSpeed);
                    break;
                }

            case AIState.Chase:
                {
                    _chaseElapsedDelay += _timeController.DeltaTime;
                    if (_chaseElapsedDelay > _chaseDelay)
                    {
                        if (_pathfinder.isStopped)
                        {
                            Animator.SetBool(Hash.IsRunning, true);
                            _pathfinder.isStopped = false;
                        }
                        _pathfinder.destination = _target.position;
                    }
                    else
                    {
                        // Rotate towards target when being delayed.
                        Vector3 diff = _target.position - Transform.position;
                        diff.y = 0F;
                        Transform.rotation = Quaternion.Slerp(Transform.rotation, Quaternion.LookRotation(diff), _timeController.DeltaTime * _targetLookRotateSpeed);
                    }

                    //
                    if (!_pathfinder.pathPending && _pathfinder.reachedEndOfPath)
                        ChangeState(AIState.Combat);
                    break;
                }

            case AIState.Hit:
                {
                    if (_isCombatIdle)
                        ChangeState(AIState.Combat);
                    break;
                }
        }
    }

    private void OnStateExit(AIState state)
    {
        switch (state)
        {
            case AIState.Patrol:
                {
                    Animator.SetBool(Hash.IsWalking, false);
                    break;
                }

            case AIState.Chase:
                {
                    _pathfinder.isStopped = false;
                    Animator.SetBool(Hash.IsRunning, false);
                    break;
                }

            case AIState.Hit:
                {
                    _pathfinder.canMove = _pathfinder.canSearch = true;
                    break;
                }
        }
    }

    protected override void OnDamaged(Transform attacker, int damage, int reactionID)
    {
        _isTargetDetected = true;
        _reactionID = reactionID;
        ChangeState(AIState.Hit);
    }

    protected override void OnDeath()
    {
        ChangeState(AIState.Dead);
    }

    public void Perceive()
    {
        _isTargetDetected = true;
    }

    private bool IsTargetInView()
    {
        Vector3 origin = Transform.position + new Vector3(0F, _targetDetectRayOffset, 0F);
        Vector3 diff = _target.position - origin;
        
        float sqrDist = diff.sqrMagnitude;
        if (sqrDist < _targetDetectMaxDistance * _targetDetectMaxDistance)
        {
            float angle = Vector3.Angle(Transform.forward, diff);
            if (angle < _targetDetectMaxAngle)
            {
                float offset = _targetCollider.bounds.size.y * 0.9F;
                RaycastHit hitInfo;
                if (Physics.Raycast(origin, diff + new Vector3(0F, offset, 0F), out hitInfo, _targetDetectMaxDistance))
                    return hitInfo.transform.gameObject.layer == _target.gameObject.layer;
            }
        }

        return false;
    }

    #region Animator Events

    private void SetCombatIdle(BoolParameter boolean)
    {
        _isCombatIdle = boolean.Value;
    }

    #endregion
}