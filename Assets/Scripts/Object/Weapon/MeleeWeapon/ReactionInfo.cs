using System;
using UnityEngine;

[Serializable]
public struct ReactionInfo
{
    [SerializeField]
    private BoneType _boneType;
    [SerializeField]
    private float _force;
    [SerializeField]
    private bool _enableRagdoll;

    public BoneType BoneType => _boneType;
    public float Force => _force;
    public bool EnableRagdoll => _enableRagdoll;
}