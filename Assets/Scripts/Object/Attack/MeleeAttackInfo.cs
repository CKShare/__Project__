using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu]
public class MeleeAttackInfo : SerializedScriptableObject
{
    [SerializeField]
    private float _hitRadius;
    [SerializeField, InlineEditor, Required]
    private EffectSettings _hitEffectSettings;

    public float HitRadius => _hitRadius;
    public EffectSettings HitEffectSettings => _hitEffectSettings;
}