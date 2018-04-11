using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Sirenix.OdinInspector;

public abstract class AttackInfo : SerializedScriptableObject
{
    [SerializeField]
    private int _damage;
    [SerializeField]
    private int _reactionID;
    [SerializeField]
    private NoiseSettings _hitNoise;
    [SerializeField]
    private Dictionary<string, EffectInfo> _hitEffectDict = new Dictionary<string, EffectInfo>();

    public int Damage => _damage;
    public int ReactionID => _reactionID;
    public NoiseSettings HitNoise => _hitNoise;

    public EffectInfo GetHitEffectInfo(string textureName)
    {
        EffectInfo info;
        if (!_hitEffectDict.TryGetValue(textureName, out info))
            throw new KeyNotFoundException(textureName);

        return info; 
    }
}