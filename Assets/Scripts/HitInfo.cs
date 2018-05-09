using System;
using UnityEngine;

[Serializable]
public class HitInfo
{
    [SerializeField]
    private int _damage;
    [SerializeField]
    private float _force;
    [SerializeField]
    private EffectSettings _hitEffect;

    public int Damage => _damage;
    public float Force => _force;
    public EffectSettings HitEffect => _hitEffect;
}