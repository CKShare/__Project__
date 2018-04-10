using System;
using UnityEngine;

public class Character : SceneObject
{
    [SerializeField]
    private int _maxHealth = 100;

    private int _currentHealth;
    private event Action<int> _onDamaged;
    private event Action _onDeath;

    public override void ApplyDamage(int damage, int reactionID = -1)
    {
        base.ApplyDamage(damage, reactionID);

        CurrentHealth -= damage;
        _onDamaged(damage);
    }

    public int MaxHealth => _maxHealth;

    public int CurrentHealth
    {
        get
        {
            return _currentHealth;
        }

        private set
        {
            _currentHealth = value;
            if (_currentHealth <= 0)
            {
                _onDeath();
                _currentHealth = 0;
            }
        }
    }

    public event Action<int> OnDamaged
    {
        add { _onDamaged += value; }
        remove { _onDamaged -= value; }
    }

    public event Action OnDeath
    {
        add { _onDeath += value; }
        remove { _onDeath -= value; }
    }
}