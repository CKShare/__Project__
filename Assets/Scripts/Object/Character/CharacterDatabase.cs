using UnityEngine;
using Sirenix.OdinInspector;

public class CharacterDatabase : SerializedScriptableObject
{
    [SerializeField]
    private float _maxHealth;
    [SerializeField, Required]
    private FootSettings _footSettings;

    public float MaxHealth => _maxHealth;
    public FootSettings FootSettings => _footSettings;
}