using System.Collections;
using UnityEngine;
using Pathfinding;
using static EnemyAnimatorInfo;

[RequireComponent(typeof(TimeController))]
public class AIController : ControllerBase
{
    [SerializeField]
    private Transform _target;
    [SerializeField]
    private float _targetDetectMaxDistance;
    [SerializeField]
    private float _targetDetectMaxAngle;

    private RichAI _ai;

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

    public void SetDestination(Transform destination)
    {
        
    }
}