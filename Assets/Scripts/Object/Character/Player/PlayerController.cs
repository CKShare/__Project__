using System;
using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;
using RootMotion.FinalIK;

public class PlayerController : ControllerBase
{
    public static class Hash
    {
        public static readonly int Random = Animator.StringToHash("Random");
        public static readonly int IsMoving = Animator.StringToHash("IsMoving");
        public static readonly int Accel = Animator.StringToHash("Accel");
        public static readonly int IsCrouching = Animator.StringToHash("IsCrouching");
        public static readonly int Dash = Animator.StringToHash("Dash");
        public static readonly int Attack = Animator.StringToHash("Attack");
        public static readonly int AttackType = Animator.StringToHash("AttackType");
        public static readonly int ComboNumber = Animator.StringToHash("ComboNumber");
        public static readonly int Aim = Animator.StringToHash("Aim");
        public static readonly int IsAiming = Animator.StringToHash("IsAiming");
        public static readonly int Pitch = Animator.StringToHash("Pitch");
    }

    [Serializable]
    private struct ComboSet
    {
        [SerializeField, Required]
        MeleeAttacker _attacker;
        [SerializeField]
        private int _maxCombo;

        public MeleeAttacker Attacker => _attacker;
        public int MaxCombo => _maxCombo;
    }

    private static readonly float TurnningAttackAngleThreshold = 75F;
    private static readonly float AttackRotateSpeed = 30F;
    private static readonly float AimingRotateSpeed = 30F;

    [SerializeField]
    private float _healthRegenUnit;
    [SerializeField]
    private AnimationCurve _moveCurve;
    [SerializeField]
    private float _moveLerpTime;
    [SerializeField]
    private float _rotateSpeed;
    [SerializeField]
    private AnimationCurve _dashCurve;
    [SerializeField]
    private float _dashCoolTime;
    [SerializeField]
    private LayerMask _targetLayer;
    [SerializeField]
    private float _targetingMaxDistance;
    [SerializeField]
    private float _targetingMaxAngle;
    [SerializeField]
    private ComboSet[] _comboSets = new ComboSet[0];
    [SerializeField, Required]
    private RangeWeapon _rangeWeapon;
    [SerializeField, Required]
    private Transform _weaponGrip;
    [SerializeField, Required]
    private Transform _weaponHolster;

    [SerializeField]
    private string _inputHorizontal = "Horizontal";
    [SerializeField]
    private string _inputVertical = "Vertical";
    [SerializeField]
    private string _inputAttack = "Attack";
    [SerializeField]
    private string _inputDash = "Dash";
    [SerializeField]
    private string _inputAim = "Aim";

    private Camera _camera;
    private Transform _cameraTr;

    private Vector3 _velocity;
    private Quaternion _rotation;

    private Vector2 _axisValue;
    private float _axisSqrMag;
    private float _movingElapsedTime;
 
    private bool _dashPressed;
    private Coroutine _dashCrt;
    private readonly WaitForFixedUpdate _waitForFixedUpdate = new WaitForFixedUpdate();
    private bool _dashCooldown;
    private float _dashElapsedCoolTime;

    private Transform _target;
    private Collider[] _targetPool = new Collider[5];
    private Vector3 _attackDirection;
    private bool _attackPressed;
    private bool _isAttacking;
    private bool _comboInputEnabled;
    private bool _comboTransitionEnabled;
    private bool _comboSaved;
    private int _comboNumber;
    private AttackType _attackType = AttackType.Front;

    private bool _aimPressing;
    private bool _isAiming;

    private bool _applyRootMotion = true;
    private bool _lockMove, _lockWeapon, _lockMelee;

    private event Action<bool> _onAimActiveChanged;
    private event Action<bool> _onDashActiveChanged;
    private event Action<bool> _onWeaponActiveChanged;

    protected override void Awake()
    {
        base.Awake();

        _camera = Camera.main;
        _cameraTr = _camera.transform;

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
        UpdateVelocity();
        UpdateRotation();
    }

    private void UpdateVelocity()
    {
        _velocity.y = Rigidbody.velocity.y;
        Rigidbody.velocity = _velocity;
    }

    private void UpdateRotation()
    {
        Rigidbody.rotation = _rotation;
    }

    private void Update()
    {
        UpdateHealth();
        UpdateInput();
        UpdateLocomotion();
        UpdateWeapon();
        UpdateMelee();
        UpdateDash();
    }

    private void UpdateHealth()
    {
        CurrentHealth += _healthRegenUnit * Time.deltaTime;
    }

    private void UpdateInput()
    {
        _axisValue = new Vector2(Input.GetAxisRaw(_inputHorizontal), Input.GetAxisRaw(_inputVertical));
        _axisSqrMag = _axisValue.sqrMagnitude;
        _dashPressed = Input.GetButtonDown(_inputDash);
        _attackPressed = Input.GetButtonDown(_inputAttack);
        _aimPressing = Input.GetButton(_inputAim);
    }

    private void UpdateLocomotion()
    {
        Vector3 controlDirection;
        bool isControlling = TryGetControlDirection(out controlDirection);
        bool isMoving = !_lockMove && isControlling;

        // Move
        if (isMoving)
        {
            _movingElapsedTime = _movingElapsedTime + Time.fixedDeltaTime;
            _velocity = controlDirection * _moveCurve.Evaluate(_movingElapsedTime);
            _velocity = Vector3.Lerp(Rigidbody.velocity, _velocity, Time.fixedDeltaTime * _moveLerpTime);
        }
        else
        {
            _velocity = Vector3.zero;
        }

        // Rotate
        if (isMoving)
        {
            Quaternion look = Quaternion.LookRotation(controlDirection);
            _rotation = Quaternion.Slerp(Rigidbody.rotation, look, _rotateSpeed * Time.fixedDeltaTime);
        }
        else
        {
            _rotation = Quaternion.LookRotation(Transform.forward);
        }

        // Animator
        Animator.SetFloat(Hash.Accel, Mathf.Clamp01(_axisSqrMag), 0.1F, Time.deltaTime);
        Animator.SetBool(Hash.IsMoving, isMoving);
    }

    private void UpdateDash()
    {
        if (_dashCooldown)
        {
            _dashElapsedCoolTime += Time.deltaTime;
            if (_dashElapsedCoolTime >= _dashCoolTime)
            {
                _dashCooldown = false;
                _dashElapsedCoolTime = 0F;
            }
        }   
        else if (_dashPressed)
        {
            _dashCooldown = true;

            if (_dashCrt != null)
                StopCoroutine(_dashCrt);
            _dashCrt = StartCoroutine(Dash());
        }
    }

    private IEnumerator Dash()
    {
        var curve = _dashCurve;
        float maxTime = curve.keys[curve.length - 1].time;
        float elapsedTime = 0F;
        Vector3 direction;
        if (!TryGetControlDirection(out direction))
            direction = Transform.forward;
        Quaternion look = Quaternion.LookRotation(direction);

        _onDashActiveChanged?.Invoke(true);
        Animator.SetTrigger(Hash.Dash);

        while (elapsedTime < maxTime)
        {
            Rigidbody.AddForce(direction * curve.Evaluate(elapsedTime), ForceMode.Acceleration);
            Rigidbody.rotation = Quaternion.Slerp(Rigidbody.rotation, look, Time.fixedDeltaTime * _rotateSpeed * 2F);

            elapsedTime += Time.fixedDeltaTime;
            yield return _waitForFixedUpdate;
        }

        _onDashActiveChanged?.Invoke(false);
        _dashCrt = null;
    }

    private void UpdateMelee()
    {
        if (_lockMelee)
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

        // Go to the next combo.
        if (_comboSaved && _comboTransitionEnabled)
        {
            if (!TryGetControlDirection(out _attackDirection))
            {
                _attackDirection = Transform.forward;
            }
            else
            {
                // Get AttackType.
                float angleDiff = Vector3.Angle(Transform.forward, _attackDirection);
                if (angleDiff > TurnningAttackAngleThreshold)
                {
                    if (_attackType != AttackType.Turn)
                        _comboNumber = 0;
                    _attackType = AttackType.Turn;
                }
            }

            _comboSaved = false;
            _comboTransitionEnabled = false;
            _comboNumber = _comboNumber % _comboSets[(int)_attackType - 1].MaxCombo + 1;
            Animator.SetInteger(Hash.AttackType, (int)_attackType);
            Animator.SetInteger(Hash.ComboNumber, _comboNumber);
            Animator.SetTrigger(Hash.Attack);

            // Get target for targeting and Attack him.
            _target = GameUtility.FindTargetInView(_targetPool, Transform.position, _attackDirection, _targetingMaxDistance, _targetingMaxAngle, _targetLayer);
            _comboSets[(int)_attackType - 1].Attacker.Attack(_target != null ? _target.gameObject : null, _comboNumber);
        }

        // Rotate towards attacking direction.
        if (_isAttacking && !_comboTransitionEnabled)
        {
            if (_target != null)
            {
                _attackDirection = _target.position - Transform.position;
                _attackDirection.y = 0F;
            }

            Quaternion look = Quaternion.LookRotation(_attackDirection);
            _rotation = Quaternion.Slerp(Rigidbody.rotation, look, AttackRotateSpeed * Time.fixedDeltaTime);
        }
    }

    private void UpdateWeapon()
    {
        bool isAiming = !_lockWeapon && _aimPressing;
        if (isAiming)
        {
            if (!Animator.GetBool(Hash.IsAiming))
            {
                _onAimActiveChanged?.Invoke(true);
                _lockMove = true;
                _lockMelee = true;
                Animator.SetTrigger(Hash.Aim);
            }
            else
            {
                // Rotate towards aiming direction.
                Vector3 camForward = _cameraTr.forward;
                camForward.y = 0F;

                Quaternion look = Quaternion.LookRotation(camForward);
                _rotation = Quaternion.Slerp(Rigidbody.rotation, look, AimingRotateSpeed * Time.fixedDeltaTime);

                // Pitch
                Vector3 targetPosition;
                RaycastHit hitInfo;
                Ray ray = _camera.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0F));
                targetPosition = Physics.Raycast(ray, out hitInfo, 100F, ~(1 << gameObject.layer)) ? hitInfo.point : ray.GetPoint(100F);

                float pitch = _cameraTr.eulerAngles.x;
                if (pitch > 180F)
                    pitch -= 360F;
                pitch *= -1F;
                Animator.SetFloat(Hash.Pitch, pitch);

                // Fire
                if (_isAiming && _attackPressed)
                {
                    _rangeWeapon.Attack(Transform, targetPosition);
                    //Animator.SetTrigger(Hash.Attack);
                }
            }
        }
        else
        {
            if (Animator.GetBool(Hash.IsAiming))
            {
                _onAimActiveChanged?.Invoke(false);
                _lockMove = false;
                _lockMelee = false;
                if (_lockWeapon)
                    HolsterWeapon();
            }
        }
        Animator.SetBool(Hash.IsAiming, isAiming);
    }

    private bool TryGetControlDirection(out Vector3 controlDirection)
    {
        if (_axisSqrMag > 0F)
        {
            controlDirection = _cameraTr.forward * _axisValue.y + _cameraTr.right * _axisValue.x;
            controlDirection.y = 0F;
            controlDirection.Normalize();
            return true;
        }

        controlDirection = Vector3.zero;
        return false;
    }

    public event Action<bool> OnDashActiveChanged
    {
        add { _onDashActiveChanged += value; }
        remove { _onDashActiveChanged -= value; }
    }

    public event Action<bool> OnAimActiveChanged
    {
        add { _onAimActiveChanged += value; }
        remove { _onAimActiveChanged -= value; }
    }

    public bool IsDashCooldown => _dashCooldown;
    public float DashCoolTime => _dashCoolTime;
    public float DashRemainingCoolTime => _dashCoolTime - _dashElapsedCoolTime;

    #region Animator Events

    private void OnRunEnter()
    {
        _applyRootMotion = false;
    }

    private void OnRunExit()
    {
        _applyRootMotion = true;
    }

    private void OnDashEnter()
    {
        _lockMove = true;
    }

    private void OnDashExit()
    {
        _lockMove = false;
    }

    private void OnAttackEnter()
    {
        _lockMove = true;
        _lockWeapon = true;
    }

    private void OnAttackExit()
    {
        _lockMove = false;
        _lockWeapon = false;
    }

    private void EnableComboInput()
    {
        _comboInputEnabled = true;
    }

    private void EnableComboTransition()
    {
        _comboTransitionEnabled = true;
        _lockWeapon = false;
    }

    private void ResetCombo()
    {
        _attackType = AttackType.Front;
        _comboNumber = 0;
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

    private void HolsterWeapon()
    {
        _rangeWeapon.transform.SetParent(_weaponHolster, false);
    }

    private void UnholsterWeapon()
    {
        _rangeWeapon.transform.SetParent(_weaponGrip, false);
    }

    private void OnAimEnter()
    {
        _isAiming = true;
    }

    private void OnAimExit()
    {
        _isAiming = false;
    }

    #endregion
}