using System;
using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public abstract class ControllerBase<TDatabase> : MonoBehaviour, IDamageable where TDatabase : CharacterDatabase
{
    [SerializeField]
    private TDatabase _database;
    [SerializeField]
    private Transform _leftFoot, _rightFoot;

    private event Action<float> _onHealthChanged = null;
    private event Action<Transform, int> _onDamaged = null;
    private Transform _transform = null;
    private Rigidbody _rigidbody = null;
    private Animator _animator = null;

    private float _currentHealth = 0F;
    private bool _isDead = false;

    protected virtual void Awake()
    {
        _transform = transform;
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
    }

    protected virtual void Start()
    {
        _currentHealth = Database.MaxHealth;
    }

    public virtual void ApplyDamage(Transform attacker, int damage, int reactionID = -1)
    {
        if (!_isDead)
        {
            CurrentHealth -= damage;
            _onDamaged?.Invoke(attacker, damage);
        }
    }

    protected virtual void OnDeath()
    {
        _isDead = true;
    }

    private void OnFootPlant(Vector3 origin)
    {
        var footSettings = Database.FootEffectSettings;

        RaycastHit hitInfo;
        if (Physics.Raycast(origin + new Vector3(0F, footSettings.RayHeightOffset, 0F), Vector3.down, out hitInfo, footSettings.RayHeightOffset + 0.5F, footSettings.RayMask))
        {
            var sceneObj = hitInfo.transform.GetComponent<SceneObject>();
            if (sceneObj != null)
            {
                string textureName = sceneObj.TextureName;
                EffectPair pair;
                if (footSettings.TryGetEffectPair(textureName, out pair))
                {
                    GameObject particle = PoolManager.Instance[pair.Particle].Spawn();
                    particle.transform.position = hitInfo.point;
                    FMODUnity.RuntimeManager.PlayOneShot(pair.Sound, hitInfo.point);
                }
            }
        }
    }

    public float CurrentHealth
    {
        get
        {
            return _currentHealth;
        }

        protected set
        {
            _currentHealth = Mathf.Clamp(value, 0, Database.MaxHealth);
            if (_currentHealth <= 0)
            {
                OnDeath();
                _currentHealth = 0;
            }

            _onHealthChanged?.Invoke(_currentHealth);
        }
    }

    public event Action<float> OnHealthChanged
    {
        add { _onHealthChanged += value; }
        remove { _onHealthChanged -= value; }
    }

    public event Action<Transform, int> OnDamaged
    {
        add { _onDamaged += value; }
        remove { _onDamaged -= value; }
    }

    public Transform Transform => _transform;
    public Rigidbody Rigidbody => _rigidbody;
    public Animator Animator => _animator;

    public TDatabase Database => _database;
    public bool IsDead => _isDead;

    #region Animator Events

    private void OnLeftFootPlant()
    {
        OnFootPlant(_leftFoot.position);
    }

    private void OnRightFootPlant()
    {
        OnFootPlant(_rightFoot.position);
    }

    #endregion
}