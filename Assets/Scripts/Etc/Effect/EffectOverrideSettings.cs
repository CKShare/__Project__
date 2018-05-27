using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu]
public class EffectOverrideSettings : EffectSettings
{
    [SerializeField, Required, PropertyOrder(-1)]
    private EffectSettings _settings;

    public override bool TryGetEffectInfo(TextureType textureType, out EffectInfo effectInfo)
    {
        // If Effect has been overrided, return it. Or return the existing one.
        return base.TryGetEffectInfo(textureType, out effectInfo) || _settings.TryGetEffectInfo(textureType, out effectInfo);
    }
}