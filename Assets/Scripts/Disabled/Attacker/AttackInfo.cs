using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public abstract class AttackInfo : SerializedScriptableObject 
{
    [SerializeField]
    private int _damage;
    [SerializeField]
    private int _reactionID;
    [SerializeField]
    private Dictionary<string, List<EffectPair>> _hitEffectDict = new Dictionary<string, List<EffectPair>>();

    public int Damage => _damage;
    public int ReactionID => _reactionID;
    public Dictionary<string, List<EffectPair>> HitEffectDict => _hitEffectDict;
}