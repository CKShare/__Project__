using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu]
public class ProjectileInfo : SerializedScriptableObject
{
    [SerializeField]
    private string _pool;
    [SerializeField, InlineEditor, Required]
    private EffectSettings _hitEffectSettings;

    public string Pool => _pool;
    public EffectSettings HitEffectSettings => _hitEffectSettings;
}