using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public abstract class Weapon : SerializedMonoBehaviour
{
    [SerializeField, DisableContextMenu, PropertyOrder(1)]
    private Dictionary<string, HitPoint> _castPointDict = new Dictionary<string, HitPoint>();
    [SerializeField, PropertyOrder(2)]
    private bool _isDroppable = true;

    private HitPoint _currentPoint;

    public void EnableHit(CastHitInfo hitInfo)
    {
        _currentPoint = _castPointDict[hitInfo.PointName];
        _currentPoint.Set(Owner, hitInfo);
        _currentPoint.enabled = true;
    }

    private void DisableHit()
    {
        if (_currentPoint == null)
            return;

        _currentPoint.enabled = false;
        _currentPoint = null;
    }

    public void Drop()
    {
        if (!_isDroppable)
            return;

        // Now, this weapon can be affected by physics. 
        var rigidbody = GetComponent<Rigidbody>();
        var collider = GetComponent<Collider>();
        if (rigidbody != null && collider != null)
        {
            rigidbody.isKinematic = false;
            collider.isTrigger = false;
        }

        transform.SetParent(null, true);
        Owner = null;
    }

    public GameObject Owner { get; set; }
}