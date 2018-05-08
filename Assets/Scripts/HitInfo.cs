using System;
using UnityEngine;

[Serializable]
public class HitInfo
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
