using System;
using UnityEngine;
using Sirenix.OdinInspector;
using RootMotion.FinalIK;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(RagdollUtility))]
[RequireComponent(typeof(HitReaction))]
[RequireComponent(typeof(FullBodyBipedIK))]
public abstract class CharacterControllerBase : SerializedMonoBehaviour, IHitReactive
{
    [SerializeField, TitleGroup("Stats")]
    private float _maxHealth = 100F;
    [SerializeField, HideLabel, HideReferenceObjectPicker, TitleGroup("Weapon")]
    private WeaponInventory _weaponInventory = new WeaponInventory();
    [SerializeField, TitleGroup("Etc")]
    private float _minForceToEnableRagdoll;

    private Transform _transform;
    private Rigidbody _rigidbody;
    private Animator _animator;
    private HitReaction _hitReaction;
    private RagdollUtility _ragdoll;

    private event Action<float> _onHealthChanged;
    private event Action _onDeath;
    private float _currentHealth;
    private bool _isDead;

    protected virtual void Awake()
    {
        _transform = transform;
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        _hitReaction = GetComponent<HitReaction>();
        _ragdoll = GetComponent<RagdollUtility>();
    }

    protected virtual void Start()
    {
        _currentHealth = _maxHealth;
    }

    protected virtual void OnDeathInternal()
    {
        _isDead = true;
        this.enabled = false; // Disable Controller

        _onDeath?.Invoke();
    }

    public virtual void ApplyDamage(Transform attacker, int damage)
    {
        CurrentHealth -= damage;
    }

    public virtual void ReactToHit(Collider collider, Vector3 point, Vector3 force)
    {
        // If force is over minimum force to enable ragdoll, Enable it
        if (force.sqrMagnitude >= _minForceToEnableRagdoll * _minForceToEnableRagdoll)
        {
            _ragdoll.EnableRagdoll();
        }

        // React to hit using IK.
        _hitReaction.Hit(collider, force, point);
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

    public float MaxHealth => _maxHealth;
    public bool IsDead => _isDead;

    protected Transform Transform => _transform;
    protected Rigidbody Rigidbody => _rigidbody;
    protected Animator Animator => _animator;
}
