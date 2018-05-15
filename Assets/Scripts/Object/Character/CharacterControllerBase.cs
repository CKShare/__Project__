using System;
using System.Collections.Generic;
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
    private LayerMask _groundLayer;
    [SerializeField, TitleGroup("Foot")]
    private EffectSettings _footstepEffect;

    [SerializeField, TitleGroup("Hit")]
    private Dictionary<int, Collider> _hitColliderDict = new Dictionary<int, Collider>();


    private Transform _transform;
    private Rigidbody _rigidbody;
    private Animator _animator;
    private RagdollUtility _ragdoll;
    private HitReaction _hitReaction;
    private FullBodyBipedIK _fbbik;
    private Material[][] _materials;

    private event Action<float> _onHealthChanged;
    private event Action _onDeath;
    private float _currentHealth;
    private bool _isDead;

    protected virtual void Awake()
    {
        _transform = transform;
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        _ragdoll = GetComponent<RagdollUtility>();
        _hitReaction = GetComponent<HitReaction>();
        _fbbik = GetComponent<FullBodyBipedIK>();

        var renderers = GetComponentsInChildren<Renderer>();
        _materials = new Material[renderers.Length][];
        for (int i = 0; i < renderers.Length; i++)
            _materials[i] = renderers[i].materials;
    }

    protected virtual void Start()
    {
        _currentHealth = _maxHealth;
    }

    private bool TryFootRaycast(Vector3 origin, out RaycastHit hitInfo)
    {
        return Physics.Raycast(origin + new Vector3(0F, 1F, 0F), Vector3.down, out hitInfo, 1F + 0.5F, _groundLayer);
    }

    protected virtual void OnDeathInternal()
    {
        _isDead = true;
        this.enabled = false; // Disable Controller
        GetComponent<Collider>().enabled = false;
        Rigidbody.useGravity = false;

        // Ragdoll
        var colliders = GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
        {
            if (col.gameObject.layer == LayerMask.NameToLayer("HitCollider"))
            {
                col.isTrigger = false;
            }
        }
        _ragdoll.EnableRagdoll();

        _onDeath?.Invoke();
    }

    public virtual void ApplyDamage(GameObject attacker, int damage)
    {
        CurrentHealth -= damage;
    }

    public void ReactToHit(int reactionID, Vector3 point, Vector3 force, bool enableRagdoll)
    {
        if (enableRagdoll)
            _ragdoll.EnableRagdoll();

        Collider collider;
        if (_hitColliderDict.TryGetValue(reactionID, out collider))
        {
            _hitReaction.Hit(collider, force, point);
        }
    }

    public void ReactToHit(Collider collider, Vector3 point, Vector3 force, bool enableRagdoll)
    {
        if (enableRagdoll)
            _ragdoll.EnableRagdoll();

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
            _currentHealth = Mathf.Clamp(value, 0F, _maxHealth);
            if (_currentHealth <= 0F)
            {
                OnDeathInternal();
                _currentHealth = 0F;
            }
            
            _onHealthChanged?.Invoke(_currentHealth);
        }
    }

    protected LayerMask GroundLayer => _groundLayer;

    public float MaxHealth => _maxHealth;
    public bool IsDead => _isDead;

    public Transform Transform => _transform;
    public Rigidbody Rigidbody => _rigidbody;
    public Animator Animator => _animator;
    public Material[][] Materials => _materials;

    private void OnFootPlant(Vector3 origin)
    {
        RaycastHit hitInfo;
        if (TryFootRaycast(origin, out hitInfo))
        {

            var sceneObj = hitInfo.transform.GetComponent<SceneObject>();
            if (sceneObj != null)
            {
                EffectInfo effectInfo;
                if (_footstepEffect.TryGetEffectInfo(sceneObj.TextureType, out effectInfo))
                {
                    // Vfx
                    GameObject particle = effectInfo.ParticlePool.Spawn();
                    particle.transform.position = hitInfo.point;

                    // Sfx
                    FMODUnity.RuntimeManager.PlayOneShot(effectInfo.Sound, hitInfo.point);
                }
            }
        }
    }

    #region Animation Events

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
