using System;
using System.Collections;
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
    private EffectSettings _footstepEffect;

    [SerializeField, TitleGroup("Etc")]
    private float _faintRecoveryTime = 2.5F;

    private Transform _transform;
    private Rigidbody _rigidbody;
    private CapsuleCollider _collider;
    private Animator _animator;
    private RagdollUtility _ragdoll;
    private HitReaction _hitReaction;
    private FullBodyBipedIK _fbbik;
    private List<Collider> _hitColliders = new List<UnityEngine.Collider>();

    private event Action<GameObject, GameObject, int> _onDamaged;
    private event Action<float> _onHealthChanged;
    private event Action _onDeath;
    private float _currentHealth;
    private bool _isDead;
    private Coroutine _getUpCrt;

    protected virtual void Awake()
    {
        _transform = transform;
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<CapsuleCollider>();
        _animator = GetComponent<Animator>();
        _ragdoll = GetComponent<RagdollUtility>();
        _hitReaction = GetComponent<HitReaction>();
        _fbbik = GetComponent<FullBodyBipedIK>();

        var colliders = GetComponentsInChildren<Collider>();
        foreach (var collider in colliders)
        {
            if (collider.gameObject.layer == LayerMask.NameToLayer("HitCollider"))
            {
                _hitColliders.Add(collider);
            }
        }
    }

    protected virtual void Start()
    {
        _currentHealth = _maxHealth;
    }

    private bool TryFootRaycast(Vector3 origin, out RaycastHit hitInfo)
    {
        return Physics.Raycast(origin + new Vector3(0F, 1F, 0F), Vector3.down, out hitInfo, 1F + 0.5F, 1 << LayerMask.NameToLayer("Ground"));
    }

    protected virtual void OnDeathInternal()
    {
        _isDead = true;
        this.enabled = false; // Disable Controller
        Rigidbody.useGravity = false;

        var weapon = GetComponentInChildren<Weapon>();
        weapon.Drop();

        // Ragdoll
        SetRagdollActive(true);

        _onDeath?.Invoke();
    }

    public virtual void ApplyDamage(GameObject attacker, int damage)
    {
        if (IsDead)
            return;

        CurrentHealth -= damage;
        _onDamaged?.Invoke(attacker, gameObject, damage);
    }

    public virtual void ReactToHit(BoneType boneType, Vector3 point, Vector3 force, bool enableRagdoll)
    {
        var refs = _fbbik.references;
        Collider collider = null;
        switch (boneType)
        {
            case BoneType.Head:
                collider = refs.head.GetComponent<Collider>();
                break;
            case BoneType.Pelvis:
                collider = refs.pelvis.GetComponent<Collider>();
                break;
            case BoneType.Spine:
                collider = refs.spine[1].GetComponent<Collider>();
                break;
            case BoneType.LeftUpperArm:
                collider = refs.leftUpperArm.GetComponent<Collider>();
                break;
            case BoneType.LeftForeArm:
                collider = refs.leftForearm.GetComponent<Collider>();
                break;
            case BoneType.RightUpperArm:
                collider = refs.rightUpperArm.GetComponent<Collider>();
                break;
            case BoneType.RightForeArm:
                collider = refs.rightForearm.GetComponent<Collider>();
                break;
            case BoneType.LeftThigh:
                collider = refs.leftThigh.GetComponent<Collider>();
                break;
            case BoneType.LeftCalf:
                collider = refs.leftCalf.GetComponent<Collider>();
                break;
            case BoneType.RightThigh:
                collider = refs.rightThigh.GetComponent<Collider>();
                break;
            case BoneType.RightCalf:
                collider = refs.rightCalf.GetComponent<Collider>();
                break;
        }

        if (!IsDead && enableRagdoll)
        {
            SetRagdollActive(true);
            if (_getUpCrt == null)
                _getUpCrt = StartCoroutine(GetUpCrt());
            OnFaint();
        }

        _hitReaction.Hit(collider, force, point);
    }

    public virtual void ReactToHit(Collider collider, Vector3 point, Vector3 force)
    {
        _hitReaction.Hit(collider, force, point);
    }

    protected virtual void OnFaint() { }
    protected virtual void OnGetUp(bool isFront) { }

    private IEnumerator GetUpCrt()
    {
        Transform pelvis = _fbbik.references.pelvis;

        // Check whether the body is on the ground or obstacle.
        int layer = 1 << LayerMask.NameToLayer("Ground") | 1 << LayerMask.NameToLayer("Obstacle");
        while (!Physics.Raycast(pelvis.position, Vector3.down, 1F, layer))
        {
            yield return null;
        }  

        // If the body is on the ground or obstacle, Make the body get up after a few seconds.
        float elpasedTime = 0F;
        while (elpasedTime < _faintRecoveryTime)
        {
            elpasedTime += DeltaTime;
            yield return null;
        }

        // Move root position to pelvis position without moving ragdoll.
        Vector3 toPelvisPosition = pelvis.position - Transform.position;
        Transform.position += toPelvisPosition;
        pelvis.position -= toPelvisPosition;

        //
        Vector3 toPelvisRotation = pelvis.eulerAngles - Transform.eulerAngles;
        Transform.eulerAngles -= new Vector3(0F, toPelvisRotation.y, 0F);
        pelvis.eulerAngles += new Vector3(0F, toPelvisRotation.y, 0F);

        // Disable ragdoll and Enable animator.
        SetRagdollActive(false);

        // 
        bool isFront = Vector3.Dot(pelvis.up, Vector3.up) > 0;
        OnGetUp(isFront);
    }

    protected virtual void SetRagdollActive(bool active)
    {
        Collider.enabled = !active;
        foreach (var col in _hitColliders)
            col.isTrigger = !active;
        if (active)
            _ragdoll.EnableRagdoll(1 << LayerMask.NameToLayer("HitCollider"));
        else
            _ragdoll.DisableRagdoll();
    }

    public event Action<GameObject, GameObject, int> OnDamaged
    {
        add { _onDamaged += value; }
        remove { _onDamaged -= value; }
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

    public float MaxHealth => _maxHealth;
    public bool IsDead => _isDead;

    public Transform Transform => _transform;
    public Rigidbody Rigidbody => _rigidbody;
    public CapsuleCollider Collider => _collider;
    public Animator Animator => _animator;
    public HitReaction HitReaction => _hitReaction;

    public abstract PhysiqueType PhysiqueType { get; }
    public abstract float DeltaTime { get; }

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
