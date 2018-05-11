using System;
using UnityEngine;

[Serializable]
public struct ForceInfo
{
    [SerializeField]
    private float _force;
    [SerializeField]
    private bool _enableRagdoll;

    public float Force => _force;
    public bool EnableRagdoll => _enableRagdoll;
}

