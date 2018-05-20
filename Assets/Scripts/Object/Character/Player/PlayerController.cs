using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using RootMotion.FinalIK;

[RequireComponent(typeof(GrounderFBBIK))]
[RequireComponent(typeof(FullBodyBipedIK))]
public class PlayerController : CharacterControllerBase
{
    public static class Hash
    {
        public static readonly int Random = Animator.StringToHash("Random");
        public static readonly int IsMoving = Animator.StringToHash("IsMoving");
        public static readonly int Accel = Animator.StringToHash("Accel");
        public static readonly int ComboNumber = Animator.StringToHash("ComboNumber");
        public static readonly int PatternType = Animator.StringToHash("PatternType");
        public static readonly int Attack = Animator.StringToHash("Attack");
        public static readonly int ReactionID = Animator.StringToHash("ReactionID");
        public static readonly int IsAiming = Animator.StringToHash("IsAiming");
        public static readonly int Aim = Animator.StringToHash("Aim");
        public static readonly int Pitch = Animator.StringToHash("Pitch");
        public static readonly int Trigger = Animator.StringToHash("Trigger");
        public static readonly int IsDashing = Animator.StringToHash("IsDashing");
    }

    [SerializeField, TitleGroup("Stats")]
    private float _healthRegenUnit = 5F;

    [SerializeField, TitleGroup("Movement")]
    private float _moveSpeed = 7.5F;
    [SerializeField, TitleGroup("Movement")]
    private float _moveLerpSpeed = 7.5F;
    [SerializeField, TitleGroup("Movement")]
    private float _moveRotateSpeed = 10F;

    [SerializeField, InlineEditor, Required, TitleGroup("Melee Attack")]
    private MeleeWeapon _meleeWeapon;
    [SerializeField, TitleGroup("Melee Attack")]
    private int[] _maxCombos = new int[0];
    [SerializeField, TitleGroup("Melee Attack")]
    private float _attackRotateSpeed = 10F;
    [SerializeField, TitleGroup("Melee Attack")]
    private float _targetingMaxDistance = 2F;
    [SerializeField, TitleGroup("Melee Attack")]
    private float _targetingMaxAngle = 75F;

    [SerializeField, InlineEditor, Required, TitleGroup("Slow Gun")]
    private RangeWeapon _slowGun;
    [SerializeField, Required, TitleGroup("Slow Gun")]
    private Transform _holster;
    [SerializeField, Required, TitleGroup("Slow Gun")]
    private Transform _grip;
    [SerializeField, TitleGroup("Slow Gun")]
    private float _slowGunCoolTime = 5F;
    [SerializeField, TitleGroup("Slow Gun")]
    private float _aimRotateSpeed = 15F;

    [SerializeField, TitleGroup("Dash")]
    private float _dashCoolTime = 3F;
    [SerializeField, TitleGroup("Dash")]
    private float _dashDistance = 3F;
    [SerializeField, TitleGroup("Dash")]
    private float _dashSpeed = 10F;
    [SerializeField, TitleGroup("Dash")]
    private float _dashLerpSpeed = 10F;
    [SerializeField, TitleGroup("Dash")]
    private float _dashRotateSpeed = 10F;
    
    [SerializeField, Required, TitleGroup("Input")]
    private string _horizontalAxisName = "Horizontal";
    [SerializeField, Required, TitleGroup("Input")]
    private string _verticalAxisName = "Vertical";
    [SerializeField, Required, TitleGroup("Input")]
    private string _attackButtonName = "Attack";
    [SerializeField, Required, TitleGroup("Input")]
    private string _aimButtonName = "Aim";
    [SerializeField, Required, TitleGroup("Input")]
    private string _concentrateButtonName = "Concentrate";
    [SerializeField, Required, TitleGroup("Input")]
    private string _dashButtonName = "Dash";

    private Camera _camera;
    private Transform _cameraTr;

    private Vector3 _velocity;
    private Quaternion _rotation;
    private bool _updateRigidbody = true;
    private bool _applyRootMotion;

    private Vector2 _axisValue;
    private float _axisSqrMagnitude;
    private bool _lockMove;

    private Transform _target;
    private Collider[] _nearTargets = new Collider[5];
    private bool _attackPressed;
    private Vector3 _attackDirection;
    private PatternType _currentPatternType = PatternType.Front;
    private int _currentComboNumber;
    private bool _isAttacking;
    private bool _comboSaved;
    private bool _comboInputEnabled;
    private bool _comboTransitionEnabled;
    private bool _lockMeleeAttack;

    private bool _concentratePressed;

    private event Action<bool> _onAimActiveChanged;
    private bool _aimPressing;
    private bool _lockSlowGun;
    private float _slowGunRemainingTime;

    private bool _dashPressed;
    private float _dashRemainingTime;
    private Coroutine _dashCrt;
    private WaitForFixedUpdate _waitForFixedUpdate = new WaitForFixedUpdate();
    private bool _lockDash;
    
    private GameObject _concentrateCamera;
    private bool _isConcentrateEnabled;

    protected override void Awake()
    {
        base.Awake();
        
        _camera = Camera.main;
        _cameraTr = _camera.transform;
        var concentrateCamera = _cameraTr.GetChild(0).GetComponent<Camera>();
        concentrateCamera.SetReplacementShader(Shader.Find("Hidden/XRay"), "XRay");
        concentrateCamera.clearStencilAfterLightingPass = true;
        _concentrateCamera = concentrateCamera.gameObject;

        foreach (var weapon in GetComponentsInChildren<Weapon>())
            weapon.Owner = gameObject;

        // Cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    protected override void Start()
    {
        base.Start();
        
        _rotation = Quaternion.LookRotation(Transform.forward);
    }

    private void OnAnimatorMove()
    {
        if (_applyRootMotion)
        {
            Vector3 velocity = Animator.deltaPosition / Time.fixedDeltaTime;
            velocity.y = Rigidbody.velocity.y;
            Rigidbody.velocity = velocity;
        }
    }

    private void FixedUpdate()
    {
        UpdateRigidbody();
    }

    private void UpdateRigidbody()
    {
        if (!_updateRigidbody)
            return;

        _velocity.y = Rigidbody.velocity.y;
        Rigidbody.velocity = _velocity;
        Rigidbody.rotation = _rotation;
    }

    private void Update()
    {
        UpdateInput();
        Movement();
        MeleeAttack();
        Dash();
        SlowGun();
        Concentrate();

        RegenHealth();
    }

    private void UpdateInput()
    {
        _axisValue = new Vector2(Input.GetAxisRaw(_horizontalAxisName), Input.GetAxisRaw(_verticalAxisName));
        _axisSqrMagnitude = _axisValue.sqrMagnitude;
        _attackPressed = Input.GetButtonDown(_attackButtonName);
        _concentratePressed = Input.GetButtonDown(_concentrateButtonName);
        _aimPressing = Input.GetButton(_aimButtonName);
        _dashPressed = Input.GetButtonDown(_dashButtonName);
    }
    
    private void Movement()
    {
        bool isControlling = _axisSqrMagnitude > 0F && !_lockMove;
        if (isControlling)
        {
            Vector3 controlDir = _cameraTr.forward * _axisValue.y + _cameraTr.right * _axisValue.x;
            controlDir.y = 0F;
            controlDir.Normalize();

            _velocity = Vector3.Lerp(Rigidbody.velocity, controlDir * _moveSpeed, Time.deltaTime * _moveLerpSpeed);
            _rotation = Quaternion.Slerp(Rigidbody.rotation, Quaternion.LookRotation(controlDir), Time.deltaTime * _moveRotateSpeed);
        }
        else
        {
            _velocity = Vector3.zero;
            _rotation = Quaternion.LookRotation(Transform.forward);
        }

        // Animator
        Animator.SetFloat(Hash.Accel, Mathf.Clamp01(_axisSqrMagnitude), 0.1F, Time.deltaTime);
        Animator.SetBool(Hash.IsMoving, isControlling);
    }

    private void MeleeAttack()
    {
        if (_lockMeleeAttack)
            return;

        if (_attackPressed)
        {
            if (!_isAttacking)
            {
                _isAttacking = true;
                _comboSaved = true;
                _comboTransitionEnabled = true;
            }
            else if (_comboInputEnabled)
            {
                _comboSaved = true;
                _comboInputEnabled = false;
            }
        }

        if (_comboSaved && _comboTransitionEnabled)
        {
            _attackDirection = _axisSqrMagnitude > 0F ? _cameraTr.forward * _axisValue.y + _cameraTr.right * _axisValue.x : Transform.forward;
            _attackDirection.y = 0F;

            // Pattern
            PatternType newPatternType = _currentPatternType;

            float angleDiff = Vector3.Angle(Transform.forward, _attackDirection);
            if (angleDiff > 75F)
                newPatternType = PatternType.Turn;

            if (_currentPatternType != newPatternType)
                _currentComboNumber = 0;
            _currentPatternType = newPatternType;

            _comboSaved = false;
            _comboTransitionEnabled = false;
            _currentComboNumber = _currentComboNumber % _maxCombos[(int)_currentPatternType - 1] + 1;

            Animator.SetInteger(Hash.PatternType, (int)_currentPatternType);
            Animator.SetInteger(Hash.ComboNumber, _currentComboNumber);
            Animator.SetTrigger(Hash.Attack);

            _target = GameUtility.FindNearestTargetInView(_nearTargets, Transform.position, _attackDirection, _targetingMaxDistance, _targetingMaxAngle, LayerMask.NameToLayer("Enemy"));
            Debug.Log(_target == null);
        }

        // Rotate towards the target or control-direction.
        if (_isAttacking && !_comboTransitionEnabled)
        {
            if (_target != null)
            {
                _attackDirection = _target.position - Transform.position;
                _attackDirection.y = 0F;
            }

            _rotation = Quaternion.Slerp(Transform.rotation, Quaternion.LookRotation(_attackDirection), Time.deltaTime * _attackRotateSpeed);
        }
    }

    private void Concentrate()
    {
        if (_concentratePressed)
        {
            if (!_isConcentrateEnabled)
            {
                _concentrateCamera.SetActive(true);
                _isConcentrateEnabled = true;
            }
            else
            {
                _concentrateCamera.SetActive(false);
                _isConcentrateEnabled = false;
            }
        }
    }

    private void Dash()
    {
        if (_dashRemainingTime > 0F)
        {
            _dashRemainingTime -= Time.deltaTime;
            if (_dashRemainingTime <= 0F)
                _dashRemainingTime = 0F;
        }

        if (_dashPressed && _dashRemainingTime <= 0F)
        {
            if (_dashCrt != null)
                StopCoroutine(_dashCrt);
            _dashCrt = StartCoroutine(DashCrt());

            _dashRemainingTime = _dashCoolTime;
        }
    }

    private IEnumerator DashCrt()
    {
        Vector3 dashDirection = _axisSqrMagnitude > 0F ? _cameraTr.forward * _axisValue.y + _cameraTr.right * _axisValue.x : Transform.forward;
        dashDirection.y = 0F;
        dashDirection.Normalize();

        _updateRigidbody = false;
        _lockMove = true;
        _lockSlowGun = true;
        Animator.SetBool(Hash.IsDashing, true);

        LayerMask layer = LayerMask.NameToLayer("Obstacle");
        float h = (Collider.height - Collider.radius * 2F) * 0.5F;
        Vector3 offset = Vector3.up * h;
        float distance = 0F;
        while (distance < _dashDistance)
        {
            Vector3 nextPosition = Rigidbody.position + dashDirection * _dashSpeed;
            Vector3 diff = nextPosition - Rigidbody.position;
            Vector3 velocity = diff / Time.deltaTime;
            velocity = Vector3.Lerp(Rigidbody.velocity, velocity, Time.deltaTime * _dashLerpSpeed);
            velocity.y = 0F;
            float d = (velocity * Time.deltaTime).magnitude;

            // Stop If collides with obstacle.
            Vector3 center = Collider.bounds.center;
            Vector3 point1 = center + offset;
            Vector3 point2 = center - offset;
            if (Physics.CapsuleCast(point1, point2, Collider.radius, diff, d, 1 << layer))
            {
                break;
            }

            Rigidbody.velocity = velocity;
            Rigidbody.rotation = Quaternion.Slerp(Rigidbody.rotation, Quaternion.LookRotation(dashDirection), Time.deltaTime * _dashRotateSpeed);

            distance += d;
            yield return _waitForFixedUpdate;
        }

        _updateRigidbody = true;
        _lockMove = false;
        _lockSlowGun = false;
        Animator.SetBool(Hash.IsDashing, false);
        // Reset velocity not to move abnormally fast.
        Rigidbody.velocity = Vector3.zero;

        _dashCrt = null;
    }

    private void SlowGun()
    {
        if (_slowGunRemainingTime > 0F)
        {
            _slowGunRemainingTime -= Time.deltaTime;
            if (_slowGunRemainingTime <= 0F)
                _slowGunRemainingTime = 0F;
        }

        bool isAiming = _aimPressing && !_lockSlowGun;
        if (isAiming)
        {
            if (!Animator.GetBool(Hash.IsAiming))
            {
                _onAimActiveChanged?.Invoke(true);
                _lockMove = true;
                _lockMeleeAttack = true;
                _lockDash = true;
                Animator.SetTrigger(Hash.Aim);
            }

            // Rotate towards aiming direction.
            Ray ray = new Ray(_cameraTr.position, _cameraTr.forward);
            Vector3 lookPoint = ray.GetPoint(30F);
            Vector3 dir = lookPoint - _cameraTr.position;

            Vector3 xzDir = dir;
            xzDir.y = 0F;
            Quaternion look = Quaternion.LookRotation(xzDir);
            _rotation = Quaternion.Slerp(Rigidbody.rotation, look, _aimRotateSpeed * Time.deltaTime);

            float pitch = _cameraTr.eulerAngles.x;
            if (pitch > 180F)
                pitch -= 360F;
            pitch *= -1F;
            Animator.SetFloat(Hash.Pitch, pitch);

            // Fire
            if (_attackPressed && _slowGunRemainingTime <= 0F)
            {
                _slowGunRemainingTime = _slowGunCoolTime;

                _slowGun.Trigger(dir);
                Animator.SetTrigger(Hash.Trigger);
            }
        }
        else if (Animator.GetBool(Hash.IsAiming))
        {
            _onAimActiveChanged?.Invoke(false);
            _lockMove = false;
            _lockMeleeAttack = false;
            _lockDash = false;

            if (_lockSlowGun)
                HolsterGun();
        }

        Animator.SetBool(Hash.IsAiming, isAiming);
    }
    
    private void RegenHealth()
    {
        CurrentHealth += _healthRegenUnit * Time.deltaTime;
    }

    protected override void SetRagdollActive(bool active, LayerMask layer)
    {
        base.SetRagdollActive(active, layer);

        Rigidbody.isKinematic = active;
    }

    public event Action<bool> OnAimActiveChanged
    {
        add { _onAimActiveChanged += value; }
        remove { _onAimActiveChanged -= value; }
    }

    public float DashCoolTime => _dashCoolTime;
    public float DashRemainingTime => _dashRemainingTime;
    public float SlowGunCoolTime => _slowGunCoolTime;
    public float SlowGunRemainingTime => _slowGunRemainingTime;

    public override PhysiqueType PhysiqueType => PhysiqueType.Light; 

    #region Animation Events
    
    private void EnableComboInput()
    {
        _comboInputEnabled = true;
    }

    private void EnableComboTransition()
    {
        _comboTransitionEnabled = true;
        _lockSlowGun = false;
    }

    private void CheckHit(int attackID)
    {
        if (_target == null)
            return;

        _meleeWeapon.Hit(_target.gameObject, attackID);
    }

    private void OnAttackEnter()
    {
        _applyRootMotion = true;
        _lockMove = true;
        _lockSlowGun = true;
    }

    private void OnAttackExit()
    {
        //if (_comboTransitionEnabled)
        {
            _applyRootMotion = false;
            _lockMove = false;
            _lockSlowGun = false;
        }
    }
    
    private void ResetCombo()
    {
        _currentPatternType = PatternType.Front;
        _currentComboNumber = 0;
        _isAttacking = false;
        _comboSaved = false;
        _comboInputEnabled = false;
        _comboTransitionEnabled = false;
    }

    private void CheckResetCombo()
    {
        if (_comboTransitionEnabled)
            ResetCombo();
    }

    private void HolsterGun()
    {
        _slowGun.transform.SetParent(_holster, false);
    }

    private void UnholsterGun()
    {
        if (!_lockSlowGun)
            _slowGun.transform.SetParent(_grip, false);
    }

    #endregion
}
