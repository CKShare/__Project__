using System;
using UnityEngine;

public abstract class Character : MonoBehaviour, IDamageable
{
    [SerializeField]
    private int _maxHealth = 100;

    private int _currentHealth;
    private event Action<Transform, int, int> _onDamaged;
    private event Action _onDeath;

    void Awake()
    {
        CurrentHealth = _maxHealth;
    }

    public void ApplyDamage(Transform attacker, int damage, int reactionID = -1)
    {
        if (_currentHealth > 0)
        {
            CurrentHealth -= damage;
            _onDamaged?.Invoke(attacker, damage, reactionID);
        }
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
                _onDeath?.Invoke();
                _currentHealth = 0;
            }
        }
    }

    public event Action<Transform, int, int> OnDamaged
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