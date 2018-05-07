using UnityEngine;

public class CharacterDatabase : ScriptableObject
{
    [SerializeField]
    private float _maxHealth = 100F;


    public float MaxHealth => _maxHealth;
}