using System;
using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public abstract class ControllerBase : MonoBehaviour, IDamageable
{
    [SerializeField, BoxGroup("Base")]
    private int _maxHealth = 100;
    [SerializeField, BoxGroup("Etc")]
    private Transform _leftFoot, _rightFoot;
    [SerializeField, BoxGroup("Etc")]
    private LayerMask _footRayMask;
    [SerializeField, BoxGroup("Etc")]
    private float _footRayOffset;
    [SerializeField, BoxGroup("Etc")]
    private FootEffectCollection _footEffectCollection;

    private event Action<int> _onHealthChanged = null;
    private event Action<Transform, int> _onDamaged = null;
    private Transform _transform = null;
    private Rigidbody _rigidbody = null;
    private Animator _animator = null;

    private int _currentHealth = 0;

    protected virtual void Awake()
    {
        _transform = transform;
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
    }

    protected virtual void Start()
    {
        CurrentHealth = _maxHealth;
    }

    public virtual void ApplyDamage(Transform attacker, int damage, int reactionID = -1)
    {
        if (_currentHealth > 0)
        {
            CurrentHealth -= damage;
            _onDamaged?.Invoke(attacker, damage);
        }
    }

    protected virtual void OnDeath()
    {
        _rigidbody.isKinematic = true;
        GetComponent<Collider>().enabled = false;
    }

    private void OnFootPlant(Vector3 origin)
    {
        RaycastHit hitInfo;
        if (Physics.Raycast(origin + new Vector3(0F, _footRayOffset, 0F), Vector3.down, out hitInfo, _footRayOffset + 0.5F, _footRayMask))
        {
            var sceneObj = hitInfo.transform.GetComponent<SceneObject>();
            if (sceneObj != null)
            {
                string textureName = sceneObj.TextureName;
                EffectPair pair;
                if (_footEffectCollection.FootEffectDict.TryGetValue(textureName, out pair))
                {
                    GameObject particle = PoolManager.Instance[pair.Particle].Spawn();
                    particle.transform.position = hitInfo.point;
                    FMODUnity.RuntimeManager.PlayOneShot(pair.Sound, hitInfo.point);
                }
            }
        }
    }

    public int MaxHealth => _maxHealth;
    public int CurrentHealth
    {
        get
        {
            return _currentHealth;
        }

        protected set
        {
            _currentHealth = Mathf.Clamp(value, 0, _maxHealth);
            if (_currentHealth <= 0)
            {
                OnDeath();
                _currentHealth = 0;
            }

            _onHealthChanged?.Invoke(_currentHealth);
        }
    }

    public event Action<int> OnHealthChanged
    {
        add { _onHealthChanged += value; }
        remove { _onHealthChanged -= value; }
    }

    public event Action<Transform, int> OnDamaged
    {
        add { _onDamaged += value; }
        remove { _onDamaged -= value; }
    }

    protected LayerMask FootRayMask => _footRayMask;
    protected float FootRayOffset => _footRayOffset;

    public Transform Transform => _transform;
    public Rigidbody Rigidbody => _rigidbody;
    public Animator Animator => _animator;

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