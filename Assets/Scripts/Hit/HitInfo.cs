using System;
using UnityEngine;
using Sirenix.OdinInspector;

[Serializable]
public class MeleeHitInfo
{
    [SerializeField]
    private Transform _hitPoint;
    [SerializeField]
    private int _damage;
    [SerializeField]
    private float _force;
    [SerializeField]
    private int _reactionID;
    [SerializeField]
    private bool _enableRagdoll;
    [SerializeField]
    private EffectSettings _hitEffect;

    public Vector3 HitPoint => _hitPoint.position;
    public int Damage => _damage;
    public Vector3 Force => _hitPoint.forward * _force;
    public int ReactionID => _reactionID;
    public bool EnableRagdoll => _enableRagdoll;
    public EffectSettings HitEffect => _hitEffect;
}

[Serializable]
public class RangeHitInfo
{
    [SerializeField]
    private int _damage;
    [SerializeField]
    private float _force;
    [SerializeField]
    private bool _enableRagdoll;
    [SerializeField]
    private EffectSettings _hitEffect;

    public int Damage => _damage;
    public float Force => _force;
    public bool EnableRagdoll => _enableRagdoll;
    public EffectSettings HitEffect => _hitEffect;
}