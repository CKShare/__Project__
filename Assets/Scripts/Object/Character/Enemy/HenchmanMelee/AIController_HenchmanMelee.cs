using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using Pathfinding.RVO;
using Sirenix.OdinInspector;

[RequireComponent(typeof(TimeController))]
[RequireComponent(typeof(RichAI))]
public class AIController_HenchmanMelee : ControllerBase
{
    public static class Hash
    {
        public static int Detect = Animator.StringToHash("Detect");
        public static int IsWalking = Animator.StringToHash("IsWalking");
        public static int IsRunning = Animator.StringToHash("IsRunning");
        public static int Alert = Animator.StringToHash("Alert");
        public static int Attack = Animator.StringToHash("Attack");
        public static int Hit = Animator.StringToHash("Hit");
        public static int HitDirection = Animator.StringToHash("HitDirection");
        public static int ReactionID = Animator.StringToHash("ReactionID");
        public static int IsDead = Animator.StringToHash("IsDead");
    }

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
    private bool _patrolOnAwake;
    [SerializeField]
    private Transform _patrolContainer;
    [SerializeField]
    private bool _patrolToNearestPoint;
    [SerializeField]
    private bool _patrolReversely;
    [SerializeField]
    private float _patrolSpeed;
    [SerializeField]
    private float _patrolDistanceError;
    [SerializeField]
    private float _chaseSpeed;
    [SerializeField]
    private float _chaseKeepDistance;
    [SerializeField]
    private float _approachSpeed;
    [SerializeField]
    private float _approachKeepDistance;
    [SerializeField]
    private float _followDelay;
    [SerializeField]
    private float _missingTimeThreshold;
    [SerializeField]
    private float _alertToPatrolDelay;
    [SerializeField]
    private float _attackDelay;
    [SerializeField]
    private MeleeAttacker _meleeAttacker = new MeleeAttacker();

    private Seeker _seeker;
    private RichAI _pathfinder;
    private TimeController _timeController;
    private Collider _collider;
    private Collider _targetCollider;
    
    private AIState_HenchmanMelee _currentState = AIState_HenchmanMelee.None;
    private int _currentPointIdx = 0;
    private float _followElapsedTime = 0F;
    private float _attackElapsedTime = 0F;
    private float _missingElapsedTime = 0F;
    private bool _isTargetDetected = false;
    private bool _isAttacking = false; 
    private bool _isHitting = false;
    private int _reactionID = -1;
    private Vector3[] _patrolPoints;
    private bool _delayFrame = false;

    protected override void Awake()
    {
        base.Awake();

        _seeker = GetComponent<Seeker>();
        _pathfinder = GetComponent<RichAI>();
        _timeController = GetComponent<TimeController>();
        _collider = GetComponent<Collider>();
        _targetCollider = _target.GetComponent<Collider>();

        _patrolPoints = new Vector3[_patrolContainer.childCount];
        for (int i = 0; i < _patrolPoints.Length; i++)
            _patrolPoints[i] = _patrolContainer.GetChild(i).position;
    }

    protected override void Start()
    {
        base.Start();

        ChangeState(AIState_HenchmanMelee.Idle);
    }

    private void Update()
    {
        if (!_delayFrame)
        {
            OnStateUpdate(_currentState);
        }
        else
        {
            _delayFrame = false;
        }
    }

    //private void OnAnimatorMove()
    //{
    //    RaycastHit hitInfo;
    //    Vector3 newPosition = Transform.position + Animator.deltaPosition + Physics.gravity * _timeController.DeltaTime;
    //    if (Physics.Linecast(Transform.position + new Vector3(0F, FootRayOffset, 0F), newPosition, out hitInfo, FootRayMask))
    //    {
    //        newPosition = hitInfo.point; // Clamp feet to the ground.
    //        Rigidbody.position = newPosition;
    //    }
    //}

    private void ChangeState(AIState_HenchmanMelee state)
    {
        if (_currentState != AIState_HenchmanMelee.None)
            OnStateExit(_currentState);
        _currentState = state;
        OnStateEnter(_currentState);

        _delayFrame = true;
    }

    private void OnStateEnter(AIState_HenchmanMelee state)
    {
        switch (state)
        {
            case AIState_HenchmanMelee.Idle:
                {
                    if (_patrolOnAwake)
                    {
                        ChangeState(AIState_HenchmanMelee.Patrol);
                    }
                }
                break;

            case AIState_HenchmanMelee.Patrol:
                {
                    _pathfinder.endReachedDistance = _patrolDistanceError;
                    _pathfinder.maxSpeed = _patrolSpeed;

                    if (_patrolToNearestPoint)
                    {
                        _seeker.StartMultiTargetPath(Transform.position, _patrolPoints, false, OnPathComplete);
                        _pathfinder.canSearch = false;
                        _patrolToNearestPoint = false;
                    }
                    else
                    {
                        _pathfinder.destination = _patrolPoints[_currentPointIdx];
                        _pathfinder.SearchPath();
                    }

                    Animator.SetBool(Hash.IsWalking, true);
                }
                break;

            case AIState_HenchmanMelee.Combat:
                {
                    _attackElapsedTime = 0F;
                    _pathfinder.canMove = false;
                    Animator.SetTrigger(Hash.Detect);
                }
                break;

            case AIState_HenchmanMelee.Attack:
                {
                    _pathfinder.canMove = false;
                    _isAttacking = true;
                    Animator.SetTrigger(Hash.Attack);
                }
                break;

            case AIState_HenchmanMelee.Chase:
                {
                    _missingElapsedTime = 0F;
                    _pathfinder.endReachedDistance = _chaseKeepDistance;
                    _pathfinder.maxSpeed = _chaseSpeed;
                    _pathfinder.destination = _target.position;
                    _pathfinder.SearchPath();
                    Animator.SetBool(Hash.IsRunning, true);
                }
                break;

            case AIState_HenchmanMelee.Approach:
                {
                    _pathfinder.endReachedDistance = _approachKeepDistance;
                    _pathfinder.maxSpeed = _approachSpeed;
                    _pathfinder.destination = _target.position;
                    _pathfinder.SearchPath();
                    Animator.SetBool(Hash.IsWalking, true);
                }
                break;

            case AIState_HenchmanMelee.Alert:
                {
                    _followElapsedTime = 0F;
                    _missingElapsedTime = 0F;
                    _pathfinder.canMove = _pathfinder.canSearch = false;

                    // Look around animation
                    Animator.SetTrigger(Hash.Alert);
                }
                break;

            case AIState_HenchmanMelee.Hit:
                {
                    _pathfinder.canMove = _pathfinder.canSearch = false;
                    _isTargetDetected = true;
                    _isHitting = true;

                    Animator.SetInteger(Hash.ReactionID, _reactionID);
                    Animator.SetTrigger(Hash.Hit);
                    Transform.LookAt(_target, Vector3.up);
                }
                break;

            case AIState_HenchmanMelee.Dead:
                {
                    _pathfinder.canMove = _pathfinder.canSearch = false;
                    GetComponent<RVOController>().enabled = false;

                    Animator.SetBool(Hash.IsDead, true);
                }
                break;
        }
    }

    private void OnStateUpdate(AIState_HenchmanMelee state)
    {
        switch (state)
        {
            case AIState_HenchmanMelee.Idle:
                {
                    if (_isTargetDetected || IsTargetInView())
                    {
                        ChangeState(AIState_HenchmanMelee.Combat);
                    }
                }
                break;

            case AIState_HenchmanMelee.Patrol:
                {
                    if (_isTargetDetected || IsTargetInView())
                    {
                        ChangeState(AIState_HenchmanMelee.Combat);
                    }
                    else
                    {
                        if (!_pathfinder.pathPending && _pathfinder.reachedEndOfPath)
                        {
                            if (_currentPointIdx == 0)
                                _patrolReversely = false;
                            else if (_currentPointIdx == _patrolPoints.Length - 1)
                                _patrolReversely = true;

                            _currentPointIdx += _patrolReversely ? -1 : 1;

                            ChangeState(AIState_HenchmanMelee.Alert);
                        }
                    }
                }
                break;

            case AIState_HenchmanMelee.Combat:
                {
                    Vector3 diff = _target.position - Transform.position;
                    diff.y = 0F;

                    _followElapsedTime += Time.deltaTime;

                    float distance = diff.sqrMagnitude;
                    if (distance > _chaseKeepDistance * _chaseKeepDistance || !IsTargetInView())
                    {
                        if (_followElapsedTime > _followDelay)
                            ChangeState(AIState_HenchmanMelee.Chase);
                    }
                    else if (distance > _approachKeepDistance * _approachKeepDistance)
                    {
                        if (_followElapsedTime > _followDelay)
                            ChangeState(AIState_HenchmanMelee.Approach);
                    }
                    else
                    {
                        _attackElapsedTime += _timeController.DeltaTime;
                        if (_attackElapsedTime >= _attackDelay)
                        {
                            ChangeState(AIState_HenchmanMelee.Attack);
                        }
                    }

                    Transform.rotation = Quaternion.Slerp(Transform.rotation, Quaternion.LookRotation(diff), _timeController.DeltaTime * _targetLookRotateSpeed);
                }
                break;

            case AIState_HenchmanMelee.Attack:
                {
                    if (!_isAttacking)
                        ChangeState(AIState_HenchmanMelee.Combat);
                }
                break;

            case AIState_HenchmanMelee.Chase:
                {
                    _missingElapsedTime = !IsTargetInView() ? _missingElapsedTime + Time.deltaTime : 0F;
                    if (_missingElapsedTime < _missingTimeThreshold)
                    {
                        _pathfinder.destination = _target.position;

                        if (!_pathfinder.pathPending && _pathfinder.reachedEndOfPath)
                            ChangeState(AIState_HenchmanMelee.Approach);
                    }
                    else // Target Missing
                    {
                        _patrolToNearestPoint = true;
                        _isTargetDetected = false;
                        ChangeState(AIState_HenchmanMelee.Alert);
                    }
                }
                break;

            case AIState_HenchmanMelee.Approach:
                {
                    Vector3 diff = _target.position - Transform.position;
                    diff.y = 0F;

                    if (diff.sqrMagnitude > _approachKeepDistance * _approachKeepDistance || !IsTargetInView())
                    {
                        ChangeState(AIState_HenchmanMelee.Chase);
                    }
                    else
                    {
                        _pathfinder.destination = _target.position;

                        if (!_pathfinder.pathPending && _pathfinder.reachedEndOfPath)
                            ChangeState(AIState_HenchmanMelee.Combat);
                    }
                }
                break;

            case AIState_HenchmanMelee.Alert:
                {
                    if (_isTargetDetected || IsTargetInView())
                    {
                        _patrolToNearestPoint = false;
                        _seeker.CancelCurrentPathRequest();
                        ChangeState(AIState_HenchmanMelee.Combat);
                    }
                    else
                    {
                        _missingElapsedTime += Time.deltaTime;
                        if (_missingElapsedTime > _alertToPatrolDelay)
                        {
                            ChangeState(AIState_HenchmanMelee.Patrol);
                        }
                    }
                }
                break;

            case AIState_HenchmanMelee.Hit:
                {
                    if (!_isHitting)
                        ChangeState(AIState_HenchmanMelee.Combat);
                }
                break;
        }
    }

    private void OnStateExit(AIState_HenchmanMelee state)
    {
        switch (state)
        {
            case AIState_HenchmanMelee.Patrol:
                {
                    Animator.SetBool(Hash.IsWalking, false);
                }
                break;

            case AIState_HenchmanMelee.Combat:
                {
                    _pathfinder.canMove = true;
                    Animator.ResetTrigger(Hash.Detect);
                }
                break;

            case AIState_HenchmanMelee.Attack:
                {
                    _pathfinder.canMove = true;
                }
                break;

            case AIState_HenchmanMelee.Chase:
                {
                    Animator.SetBool(Hash.IsRunning, false);
                }
                break;

            case AIState_HenchmanMelee.Approach:
                {
                    Animator.SetBool(Hash.IsWalking, false);
                }
                break;

            case AIState_HenchmanMelee.Alert:
                {
                    _pathfinder.canMove = _pathfinder.canSearch = true;
                }
                break;

            case AIState_HenchmanMelee.Hit:
                {
                    _pathfinder.canMove = _pathfinder.canSearch = true;
                }
                break;
        }
    }

    private void OnPathComplete(Path p)
    {
        if (p.error)
        {
            // Log Error
            return;
        }

        // Set path to the shortest patrol point.
        MultiTargetPath mp = p as MultiTargetPath;
        _currentPointIdx = mp.chosenTarget;
        _pathfinder.canSearch = true;
        _pathfinder.destination = _patrolPoints[_currentPointIdx];
    }

    public override void ApplyDamage(Transform attacker, int damage, int reactionID)
    {
        _reactionID = reactionID;
        ChangeState(AIState_HenchmanMelee.Hit);

        base.ApplyDamage(attacker, damage, reactionID);
    }

    protected override void OnDeath()
    {
        base.OnDeath();

        ChangeState(AIState_HenchmanMelee.Dead);
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
                if (Physics.Raycast(origin, diff + new Vector3(0F, offset, 0F), out hitInfo, _targetDetectMaxDistance, ~(1 << gameObject.layer)))
                    return hitInfo.transform.gameObject.layer == _target.gameObject.layer;
            }
        }

        return false;
    }

    #region Animator Events

    private void ExitAttacking()
    {
        _isAttacking = false;
    }

    private void ExitHitting()
    {
        _isHitting = false;
    }

    private void SetColliderActive(BoolParameter active)
    {
        _collider.enabled = active.Value;
    }

    private void CheckHit(IntParameter attackID)
    {
        _meleeAttacker.CheckHit(Transform, _target, attackID.Value);
    }

    #endregion
}