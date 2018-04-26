using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu]
public class FootEffectSettings : SerializedScriptableObject
{
    [SerializeField]
    private LayerMask _rayMask;
    [SerializeField]
    private float _rayHeightOffset;
    [SerializeField]
    private Dictionary<string, EffectPair> _effectDict = new Dictionary<string, EffectPair>();

    public bool TryGetEffectPair(string textureName, out EffectPair effectPair)
    {
        return _effectDict.TryGetValue(textureName, out effectPair);
    }

    public LayerMask RayMask => _rayMask;
    public float RayHeightOffset => _rayHeightOffset;
}