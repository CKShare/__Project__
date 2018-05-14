using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu]
public class HitInfo : SerializedScriptableObject
{
    [SerializeField]
    private int _damage;
    [SerializeField]
    private int _reactionID;
    [SerializeField]
    private EffectSettings _hitEffect;
    
    public int Damage => _damage;
    public int ReactionID => _reactionID;
    public EffectSettings HitEffect => _hitEffect;
}