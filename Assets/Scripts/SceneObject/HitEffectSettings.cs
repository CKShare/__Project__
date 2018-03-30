using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu]
public class HitEffectSettings : SerializedScriptableObject
{
    [SerializeField]
    private Dictionary<int, HitEffectInfo> _hitEffectDict = new Dictionary<int, HitEffectInfo>();

    public HitEffectInfo this[int weaponType]
    {
        get
        {
            HitEffectInfo info;
            if (!_hitEffectDict.TryGetValue(weaponType, out info))
                throw new KeyNotFoundException($"'{weaponType}' key is not found in the dictionary.");

            return info;
        }
    }
}

