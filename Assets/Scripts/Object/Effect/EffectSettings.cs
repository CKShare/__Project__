using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu]
public class EffectSettings : SerializedScriptableObject
{
    [SerializeField]
    private Dictionary<string, EffectPair> _effectDict = new Dictionary<string, EffectPair>();

    public bool TryGetEffectPair(string key, out EffectPair effectPair)
    {
        return _effectDict.TryGetValue(key, out effectPair);
    }
}