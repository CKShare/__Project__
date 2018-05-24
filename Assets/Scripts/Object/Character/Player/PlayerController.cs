using System;
using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;
using RootMotion.FinalIK;
using Pathfinding;

[RequireComponent(typeof(Seeker))]
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
        public static readonly int IsAiming = Animator.StringToHash("IsAiming");
        public static readonly int Aim = Animator.StringToHash("Aim");
        public static readonly int Pitch = Animator.StringToHash("Pitch");
        public static readonly int Trigger = Animator.StringToHash("Trigger");
        public static readonly int Dash = Animator.StringToHash("Dash");
        public static readonly int CancelDash = Animator.StringToHash("CancelDash");
        public static readonly int GetUpFront = Animator.StringToHash("GetUpFront");
        public static readonly int GetUpBack = Animator.StringToHash("GetUpBack");
        public static readonly int Crouch = Animator.StringToHash("Crouch");
        public static readonly int IsCrouching = Animator.StringToHash("IsCrouching");
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
    private string _crouchButtonName = "Crouch";
    [SerializeField, Required, TitleGroup("Input")]
    private string _dashButtonName = "Dash";

    private Camera _camera;
    private Transform _cameraTr;
    private Seeker _seeker;

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

    private event Action<bool> _onAimActiveChanged;
    private bool _aimPressing;
    private bool _lockSlowGun;
    private float _slowGunRemainingTime;

    private bool _dashPressed;
    private float _dashRemainingTime;
    private Coroutine _dashCrt;
    private WaitForFixedUpdate _waitForFixedUpdate = new WaitForFixedUpdate();
    private bool _lockDash;
    
    private bool _crouchPressed;
    private Transform _crouchPoint;
    private Coroutine _crouchCrt;
    private bool _cancelCrouch;
    private bool _lockCrouch;

    protected override void Awake()
    {
        base.Awake();
        
        _camera = Camera.main;
        //_camera.clearStencilAfterLightingPass = true;
        _cameraTr = _camera.transform;
        _seeker = GetComponent<Seeker>();

        foreach (var weapon in GetComponentsInChildren<Weapon>())
            weapon.Owner = gameObject;

        // Cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnEnable()
    {
        _seeker.pathCallback += OnPathComplete;
    }

    private void OnDisable()
    {
        _seeker.pathCallback -= OnPathComplete;
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

    private void OnTriggerEnter(Collider col)
    {
        int layer = LayerMask.NameToLayer("CrouchPoint");
        if (col.gameObject.layer == layer)
            _crouchPoint = col.transform;
    }

    private void OnTriggerExit(Collider col)
    {
        int layer = LayerMask.NameToLayer("CrouchPoint");
        if (col.gameObject.layer == layer)
            _crouchPoint = null;
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
        Crouch();

        RegenHealth();
    }

    private void UpdateInput()
    {
        _axisValue = new Vector2(Input.GetAxisRaw(_horizontalAxisName), Input.GetAxisRaw(_verticalAxisName));
        _axisSqrMagnitude = _axisValue.sqrMagnitude;
        _attackPressed = Input.GetButtonDown(_attackButtonName);
        _crouchPressed = Input.GetButtonDown(_crouchButtonName);
        _aimPressing = Input.GetButton(_aimButtonName);
        _dashPressed = Input.GetButtonDown(_dashButtonName);
    }
    
    private void Movement()
    {
        if (!LockMove)
        {
            bool isControlling = _axisSqrMagnitude > 0F;
            if (isControlling)
            {
                Vector3 controlDir = _cameraTr.forward * _axisValue.y + _cameraTr.right * _axisValue.x;
                controlDir.y = 0F;
                controlDir.Normalize();

                _velocity = Vector3.Lerp(Rigidbody.velocity, controlDir * _moveSpeed, DeltaTime * _moveLerpSpeed);
                _rotation = Quaternion.Slerp(Rigidbody.rotation, Quaternion.LookRotation(controlDir), DeltaTime * _moveRotateSpeed);
            }
            else
            {
                _velocity = Vector3.zero;
                _rotation = Quaternion.LookRotation(Transform.forward);
            }

            // Animator
            Animator.SetFloat(Hash.Accel, Mathf.Clamp01(_axisSqrMagnitude), 0.1F, DeltaTime);
            Animator.SetBool(Hash.IsMoving, isControlling);
        }
    }

    private void MeleeAttack()
    {
        if (LockMeleeAttack)
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
        }

        // Rotate towards the target or control-direction.
        if (_isAttacking && !_comboTransitionEnabled)
        {
            if (_target != null)
            {
                _attackDirection = _target.position - Transform.position;
                _attackDirection.y = 0F;
            }

            _rotation = Quaternion.Slerp(Transform.rotation, Quaternion.LookRotation(_attackDirection), DeltaTime * _attackRotateSpeed);
        }
    }

    private void Crouch()
    {
        if (_crouchPressed)
        {
            if (_crouchCrt == null)
            {
                if (!LockCrouch && _crouchPoint != null)
                {
                    _crouchCrt = StartCoroutine(CrouchCrt());
                }
            }
            else
            {
                _cancelCrouch = true;
            }
        }
        else if (_crouchCrt != null)
        {
            if (_axisSqrMagnitude > 0F || HitReaction.inProgress)
            {
                _cancelCrouch = true;
            }
        }
    }

    private IEnumerator CrouchCrt()
    {
        _updateRigidbody = false;
        LockMove = true;
        LockDash = true;
        LockSlowGun = true;
        LockMeleeAttack = true;

        Path p = _seeker.StartPath(Transform.position, _crouchPoint.position);
        while (true)
        {
            if (p.CompleteState == PathCompleteState.Error)
            {
                _cancelCrouch = true;
                break;
            }
            else if (p.CompleteState == PathCompleteState.Complete)
            {
                break;
            }
            else
            {
                yield return null;
            }
        }

        // First, Move to the crouch point.
        Animator.SetBool(Hash.IsMoving, true);
        int current = 0;
        var points = p.vectorPath;
        while (current < points.Count && !_cancelCrouch)
        {
            Vector3 diff = points[current] - Transform.position;
            diff.y = 0F;
            float sqrDist = diff.sqrMagnitude;
            if (sqrDist < 0.01F)
            {
                current++;
                continue;
            }

            Rigidbody.velocity = Vector3.Lerp(Rigidbody.velocity, diff.normalized * _moveSpeed, DeltaTime * _moveLerpSpeed);
            Rigidbody.rotation = Quaternion.Slerp(Rigidbody.rotation, Quaternion.LookRotation(diff), DeltaTime * _moveRotateSpeed);
            yield return _waitForFixedUpdate;
        }
        Rigidbody.velocity = Vector3.zero;
        if (p != null && !p.error)
            p.Release(this);
        Animator.SetBool(Hash.IsMoving, false);

        // Second, Crouch down and Rotate towards the facing direction of the crouch point.
        Animator.SetTrigger(Hash.Crouch);
        Animator.SetBool(Hash.IsCrouching, true);
        // Scale Collider.
        Collider.height *= 0.5F;
        Collider.center = Vector3.Scale(Collider.center, new Vector3(1F, 0.5F, 1F));
        // Enable Shader.

        while (!_cancelCrouch)
        {
            Vector3 forward = _crouchPoint.forward;
            forward.y = 0F;
            Rigidbody.rotation = Quaternion.Slerp(Rigidbody.rotation, Quaternion.LookRotation(forward), DeltaTime * _moveRotateSpeed);
            yield return _waitForFixedUpdate;
        }
        _rotation = Rigidbody.rotation;

        Animator.ResetTrigger(Hash.Crouch);
        Animator.SetBool(Hash.IsCrouching, false);
        // Scale Collider.
        Collider.height *= 2F;
        Collider.center = Vector3.Scale(Collider.center, new Vector3(1F, 2F, 1F));
        // Disable Shader.

        _updateRigidbody = true;
        LockMove = false;
        LockDash = false;
        LockSlowGun = false;
        LockMeleeAttack = false;

        _cancelCrouch = false;
        _crouchCrt = null;
    }

    private void OnPathComplete(Path p)
    {
        if (!p.error)
            p.Claim(this);
    }

    private void Dash()
    {
        if (_dashRemainingTime > 0F)
        {
            _dashRemainingTime -= DeltaTime;
            if (_dashRemainingTime <= 0F)
                _dashRemainingTime = 0F;
        }

        if (!LockDash && _dashPressed && _dashRemainingTime <= 0F)
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
        LockMove = true;
        LockMeleeAttack = true;
        LockSlowGun = true;
        Animator.SetTrigger(Hash.Dash);

        int layer = 1 << LayerMask.NameToLayer("Obstacle") | 1 << LayerMask.NameToLayer("Enemy");
        float h = (Collider.height - Collider.radius * 2F) * 0.5F;
        Vector3 offset = Vector3.up * h;
        float distance = 0F;
        while (distance < _dashDistance && !LockDash)
        {
            Vector3 nextPosition = Rigidbody.position + dashDirection * _dashSpeed;
            Vector3 diff = nextPosition - Rigidbody.position;
            Vector3 velocity = diff / DeltaTime;
            velocity = Vector3.Lerp(Rigidbody.velocity, velocity, DeltaTime * _dashLerpSpeed);
            velocity.y = 0F;
            float d = (velocity * DeltaTime).magnitude;

            // Stop If collides with obstacle.
            Vector3 center = Collider.bounds.center;
            Vector3 point1 = center + offset;
            Vector3 point2 = center - offset;
            if (Physics.CapsuleCast(point1, point2, Collider.radius - 0.01F, diff, d, layer))
            {
                break;
            }

            Rigidbody.velocity = velocity;
            Rigidbody.rotation = Quaternion.Slerp(Rigidbody.rotation, Quaternion.LookRotation(dashDirection), DeltaTime * _dashRotateSpeed);

            distance += d;
            yield return _waitForFixedUpdate;
        }

        Animator.SetTrigger(Hash.CancelDash);
        yield return _waitForFixedUpdate;

        Animator.ResetTrigger(Hash.CancelDash);
        _updateRigidbody = true;
        LockMove = false;
        LockMeleeAttack = false;
        LockSlowGun = false;
        
        // Reset velocity not to move abnormally fast.
        Rigidbody.velocity = Vector3.zero;

        _dashCrt = null;
    }

    private void SlowGun()
    {
        if (_slowGunRemainingTime > 0F)
        {
            _slowGunRemainingTime -= DeltaTime;
            if (_slowGunRemainingTime <= 0F)
                _slowGunRemainingTime = 0F;
        }

        if (!LockSlowGun)
        {
            bool isAiming = _aimPressing;
            if (isAiming)
            {
                if (!Animator.GetBool(Hash.IsAiming))
                {
                    _onAimActiveChanged?.Invoke(true);
                    LockMove = true;
                    LockMeleeAttack = true;
                    Animator.SetTrigger(Hash.Aim);
                }

                // Rotate towards aiming direction.
                Ray ray = new Ray(_cameraTr.position, _cameraTr.forward);
                Vector3 lookPoint = ray.GetPoint(30F);
                Vector3 dir = lookPoint - _cameraTr.position;

                Vector3 xzDir = dir;
                xzDir.y = 0F;
                Quaternion look = Quaternion.LookRotation(xzDir);
                _rotation = Quaternion.Slerp(Rigidbody.rotation, look, _aimRotateSpeed * DeltaTime);

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
                LockMove = false;
                LockMeleeAttack = false;
            }

            Animator.SetBool(Hash.IsAiming, isAiming);
        }
    }
    
    private void RegenHealth()
    {
        CurrentHealth += _healthRegenUnit * DeltaTime;
    }

    protected override void SetRagdollActive(bool active)
    {
        base.SetRagdollActive(active);

        Rigidbody.isKinematic = active;
    }

    protected override void OnGetUp(bool isFront)
    {
        base.OnGetUp(isFront);

        Animator.SetTrigger(isFront ? Hash.GetUpFront : Hash.GetUpBack);
    }

    public event Action<bool> OnAimActiveChanged
    {
        add { _onAimActiveChanged += value; }
        remove { _onAimActiveChanged -= value; }
    }

    private bool LockMove
    {
        get
        {
            return _lockMove;
        }

        set
        {
            _lockMove = value;
            if (value)
            {
                _velocity = Vector3.zero;
                _rotation = Quaternion.LookRotation(Transform.forward);
                Animator.SetFloat(Hash.Accel, 0F);
                Animator.SetBool(Hash.IsMoving, false);
            }
        }
    }

    private bool LockMeleeAttack
    {
        get
        {
            return _lockMeleeAttack;
        }

        set
        {
            _lockMeleeAttack = value;
            if (value)
            {
                _rotation = Quaternion.LookRotation(Transform.forward);
                ResetCombo();
                Animator.ResetTrigger(Hash.Attack);
            }
        }
    }

    private bool LockSlowGun
    {
        get
        {
            return _lockSlowGun;
        }

        set
        {
            _lockSlowGun = value;
            if (value)
            {
                HolsterGun();
                Animator.ResetTrigger(Hash.Aim);
                Animator.SetBool(Hash.IsAiming, false);
            }
        }
    }

    private bool LockDash
    {
        get
        {
            return _lockDash;
        }

        set
        {
            _lockDash = value;
            if (value)
            {
                Animator.ResetTrigger(Hash.Dash);
                Animator.ResetTrigger(Hash.CancelDash);
            }
        }
    }

    private bool LockCrouch
    {
        get
        {
            return _lockCrouch;
        }

        set
        {
            _lockCrouch = value;
            if (value)
            {
                Animator.ResetTrigger(Hash.Crouch);
                Animator.SetBool(Hash.IsCrouching, false);

                if (_crouchCrt != null)
                    _cancelCrouch = true;
            }
        }
    }

    public float DashCoolTime => _dashCoolTime;
    public float DashRemainingTime => _dashRemainingTime;
    public float SlowGunCoolTime => _slowGunCoolTime;
    public float SlowGunRemainingTime => _slowGunRemainingTime;

    public override PhysiqueType PhysiqueType => PhysiqueType.Light;
    public override float DeltaTime => Time.deltaTime;

    #region Animation Events
    
    private void EnableComboInput()
    {
        _comboInputEnabled = true;
    }

    private void EnableComboTransition()
    {
        _comboTransitionEnabled = true;
        LockSlowGun = false;
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
        LockMove = true;
        LockSlowGun = true;
    }

    private void OnAttackExit()
    {
        //if (_comboTransitionEnabled)
        {
            _applyRootMotion = false;
            LockMove = false;
            LockSlowGun = false;
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
        if (!LockSlowGun)
            _slowGun.transform.SetParent(_grip, false);
    }

    #endregion
}
