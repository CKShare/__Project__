using System;
using UnityEngine;
using Sirenix.OdinInspector;
using RootMotion.FinalIK;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(RagdollUtility))]
[RequireComponent(typeof(HitReaction))]
[RequireComponent(typeof(FullBodyBipedIK))]
public abstract class CharacterControllerBase : SceneObject, IHitReactive
{
    [SerializeField, TitleGroup("Stats")]
    private float _maxHealth = 100F;
    [SerializeField, TitleGroup("Foot"), Required]
    private Transform _leftFoot, _rightFoot;
    [SerializeField, TitleGroup("Foot")]
    private LayerMask _footRayLayerMask;
    [SerializeField, TitleGroup("Foot")]
    private float _footRayHeightOffset = 1F;
    [SerializeField, TitleGroup("Foot")]
    private EffectSettings _footstepEffect;
    [SerializeField, TitleGroup("Etc")]
    private PhysiqueType _physiqueType = PhysiqueType.Normal;

    private Transform _transform;
    private Rigidbody _rigidbody;
    private Animator _animator;
    private HitReaction _hitReaction;
    private RagdollUtility _ragdoll;
    private FullBodyBipedIK _fbbik;

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
        _fbbik = GetComponent<FullBodyBipedIK>();
    }

    protected virtual void Start()
    {
        _currentHealth = _maxHealth;
        //_weaponInventory.Initialize(gameObject);
    }

    private bool TryFootRaycast(Vector3 origin, out RaycastHit hitInfo)
    {
        return Physics.Raycast(origin + new Vector3(0F, _footRayHeightOffset, 0F), Vector3.down, out hitInfo, _footRayHeightOffset + 0.5F, _footRayLayerMask);
    }

    protected virtual void OnDeathInternal()
    {
        _isDead = true;
        this.enabled = false; // Disable Controller

        _onDeath?.Invoke();
    }

    public virtual void ApplyDamage(GameObject attacker, int damage)
    {
        CurrentHealth -= damage;
    }

    public virtual void ReactToHit(Collider collider, Vector3 point, Vector3 force, bool enableRagdoll)
    {
        if (enableRagdoll)
            _ragdoll.EnableRagdoll();

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
    public PhysiqueType PhysiqueType => _physiqueType;

    public Transform Transform => _transform;
    public Rigidbody Rigidbody => _rigidbody;
    public Animator Animator => _animator;

    private void OnFootPlant(Vector3 origin)
    {
        RaycastHit hitInfo;
        if (TryFootRaycast(origin, out hitInfo))
        {
            EffectInfo effectInfo;
            if (_footstepEffect.TryGetEffectInfo(TextureType, out effectInfo))
            {
                // Vfx
                GameObject particle = effectInfo.ParticlePool.Spawn();
                particle.transform.position = hitInfo.point;

                // Sfx
                FMODUnity.RuntimeManager.PlayOneShot(effectInfo.Sound, hitInfo.point);
            }

        }
    }

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
