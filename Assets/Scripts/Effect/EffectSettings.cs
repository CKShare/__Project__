using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu]
public class EffectSettings : SerializedScriptableObject
{
    [SerializeField]
    private Dictionary<TextureType, EffectInfo> _effectDict = new Dictionary<TextureType, EffectInfo>();

    public virtual bool TryGetEffectInfo(TextureType textureType, out EffectInfo effectInfo)
    {
        return _effectDict.TryGetValue(textureType, out effectInfo);
    }
}