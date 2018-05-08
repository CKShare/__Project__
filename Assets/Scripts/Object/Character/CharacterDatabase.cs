using UnityEngine;
using Sirenix.OdinInspector;

public class CharacterDatabase : SerializedScriptableObject
{
    [SerializeField, Tooltip("최대 체력")]
    private float _maxHealth = 100F;

    public float MaxHealth => _maxHealth;
}