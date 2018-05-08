using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu]
public class EffectSettings : SerializedScriptableObject
{
    private class TextureTypeComparer : IEqualityComparer<TextureType>
    {
        public bool Equals(TextureType x, TextureType y)
        {
            return x == y;
        }

        public int GetHashCode(TextureType obj)
        {
            return obj.GetHashCode();
        }
    }

    [SerializeField]
    private Dictionary<TextureType, EffectInfo> _effectDict = new Dictionary<TextureType, EffectInfo>(new TextureTypeComparer());

    public virtual bool TryGetEffectInfo(TextureType textureType, out EffectInfo effectInfo)
    {
        return _effectDict.TryGetValue(textureType, out effectInfo);
    }
}