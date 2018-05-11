using UnityEngine;

public class BoxCastPoint : HitPoint
{
    [SerializeField]
    private Vector3 _halfExtents = Vector3.one;
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
        if (Physics.BoxCast(_prevPosition, _halfExtents, diff, out hitInfo, transform.rotation, diff.magnitude * _distanceMultiplier, ~IgnoreLayer))
        {
            NotifyHit(diff.normalized, hitInfo);
        }

        _prevPosition = transform.position;
    }
    
    private void OnDrawGizmosSelected()
    {
        var prevMatrix = Gizmos.matrix;
        var prevColor = Gizmos.color;

        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, _halfExtents);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);

        // Restore previous settings.
        Gizmos.matrix = prevMatrix;
        Gizmos.color = prevColor;
    }
}
