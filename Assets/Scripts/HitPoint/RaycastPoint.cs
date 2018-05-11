﻿using UnityEngine;

public class RaycastPoint : HitPoint
{
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
        if (Physics.Raycast(_prevPosition, diff, out hitInfo, diff.magnitude * _distanceMultiplier, ~IgnoreLayer))
        {
            NotifyHit(diff.normalized, hitInfo);
        }

        _prevPosition = transform.position;
    }

    private void OnDrawGizmosSelected()
    {
        var prevColor = Gizmos.color;
        
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 0.1F);

        // Restore previous settings.
        Gizmos.color = prevColor;
    }
}