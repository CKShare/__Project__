using System.Collections;
using UnityEngine;
using Pathfinding;
using static EnemyAnimatorInfo;

[RequireComponent(typeof(TimeController))]
public class AIController : ControllerBase
{
    [SerializeField]
    private float _patrolSpeed;
    [SerializeField]
    private float _chaseSpeed;
    [SerializeField]
    private Transform _target;
    [SerializeField]
    private float _targetDetectMaxDistance;
    [SerializeField]
    private float _targetDetectMaxAngle;
    [SerializeField]
    private float _targetKeepDistance;
    [SerializeField]
    private Transform[] _patrolPoints;

    private RichAI _ai;
    private int _currentPointIdx = -1;
    private bool _isInversed;

    protected override void Awake()
    {
        base.Awake();

        _ai = GetComponent<RichAI>();
    }

    public bool IsTargetDetected()
    {
        Vector3 diff = _target.position - Transform.position;
        diff.y = 0F;

        float sqrDist = diff.sqrMagnitude;
        if (sqrDist <= _targetDetectMaxDistance)
        {
            float angle = Vector3.Angle(Transform.forward, diff);
            if (angle <= _targetDetectMaxAngle)
            {
                RaycastHit hitInfo;
                Physics.Raycast(Transform.position, diff, out hitInfo, _targetDetectMaxDistance);
                return hitInfo.transform.gameObject.layer == _target.gameObject.layer;
            }
        }

        return false;
    }

    public bool IsPatrolPointReached()
    {
        return _ai.reachedEndOfPath;
    }

    public void GoToNextPatrolPoint()
    {
        if (_currentPointIdx == _patrolPoints.Length - 1)
            _isInversed = true;
        else if (_currentPointIdx == 0)
            _isInversed = false;

        _currentPointIdx += _isInversed ? -1 : 1;
        _ai.destination = _patrolPoints[_currentPointIdx].position;
    }
}