using UnityEngine;
using Sirenix.OdinInspector;

public class CharacterDatabase : SerializedScriptableObject
{
    [SerializeField]
    private float _maxHealth;
    [SerializeField, Required]
    private FootEffectSettings _footEffectSettings;

    public float MaxHealth => _maxHealth;
    public FootEffectSettings FootEffectSettings => _footEffectSettings;
}