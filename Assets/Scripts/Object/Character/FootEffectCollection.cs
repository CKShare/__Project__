using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu]
public class FootEffectCollection : SerializedScriptableObject
{
    [SerializeField]
    private Dictionary<string, EffectPair> _footEffectDict = new Dictionary<string, EffectPair>();

    public Dictionary<string, EffectPair> FootEffectDict => _footEffectDict;
}