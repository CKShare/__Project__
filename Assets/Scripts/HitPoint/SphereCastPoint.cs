using UnityEngine;

public class SphereCastPoint : HitPoint
{
    [SerializeField]
    private float _radius = 0.5F;
    [SerializeField]
    private float _distanceMultiplier = 1F;

    private Vector3 _prevPosition;

    private void OnEnable()
    {
        _prevPosition = transform.position;
    }

    private void Update()
    {
        RaycastHit hitInfo;
        Vector3 diff = transform.position - _prevPosition;
        if (Physics.SphereCast(_prevPosition, _radius, diff, out hitInfo, diff.magnitude * _distanceMultiplier, ~IgnoreLayer))
        {
            NotifyHit(diff.normalized, hitInfo);
        }

        _prevPosition = transform.position;
    }

    private void OnDrawGizmosSelected()
    {
        var prevColor = Gizmos.color;
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _radius);

        // Restore previous settings.
        Gizmos.color = prevColor;
    }
}
