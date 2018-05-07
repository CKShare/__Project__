using System;
using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public abstract class CharacterControllerBase<TDatabase> : MonoBehaviour, IDamageable where TDatabase : CharacterDatabase
{
    [SerializeField, Required]
    private TDatabase _database;

    private Transform _transform;
    private Rigidbody _rigidbody;
    private Animator _animator;

    private event Action<float> _onHealthChanged;
    private event Action _onDeath;
    private float _currentHealth;
    private bool _isDead;

    protected virtual void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
    }

    protected virtual void Start()
    {
        _currentHealth = _database.MaxHealth;
    }

    public void ApplyDamage(Transform attacker, int damage, int reactionID = -1)
    {
        if (_isDead)
            return;

        CurrentHealth -= damage;
    }

    protected virtual void OnDeathInternal()
    {
        _isDead = true;
        this.enabled = false; // Disable Controller

        _onDeath?.Invoke();
    }

    public event Action<float> OnHealthChanged
    {
        add { _onHealthChanged += value; }
        remove { _onHealthChanged -= value; }
    }

    public event Action OnDeath
    {
        add { _onDeath += value; }
        remove { _onDeath -= value; }
    }

    public float CurrentHealth
    {
        get
        {
            return _currentHealth;
        }

        protected set
        {
            _currentHealth = value;
            if (_currentHealth <= 0F)
            {
                OnDeathInternal();
                _currentHealth = 0F;
            }

            _onHealthChanged?.Invoke(_currentHealth);
        }
    }

    public bool IsDead => _isDead;

    protected TDatabase Database => _database;
    protected Transform Transform => _transform;
    protected Rigidbody Rigidbody => _rigidbody;
    protected Animator Animator => _animator;
}
