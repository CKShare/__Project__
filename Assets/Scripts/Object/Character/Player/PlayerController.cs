using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using RootMotion.FinalIK;
using Sirenix.OdinInspector;

public class PlayerController : ControllerBase
{
    public static class Hash
    {
        public static int IsMoving = Animator.StringToHash("IsMoving");
        public static int IsCrouching = Animator.StringToHash("IsCrouching");
        public static int Accel = Animator.StringToHash("Accel");
        public static int TurnAngle = Animator.StringToHash("TurnAngle");
        public static int IsAiming = Animator.StringToHash("IsAiming");
        public static int Yaw = Animator.StringToHash("Yaw");
        public static int Pitch = Animator.StringToHash("Pitch");
        public static int Attack = Animator.StringToHash("Attack");
    }

    [SerializeField, BoxGroup("Base")]
    private float _healthRegenUnit = 5F;

    [SerializeField, BoxGroup("Locomotion")]
    private float _runningRotateSpeed = 10F;
    [SerializeField, BoxGroup("Locomotion")]
    private float _attackingRotateSpeed = 20F; // Should be high enough to rotate completely during about 0.1 ~ 0.2 seconds.

    [SerializeField, BoxGroup("Combat")]
    private LayerMask _attackTargetLayer;
    [SerializeField, BoxGroup("Combat")]
    private float _targetDetectMaxDistance = 1.5F;
    [SerializeField, BoxGroup("Combat")]
    private float _targetDetectMaxAngle = 70F;
    [SerializeField, BoxGroup("Combat")]
    private MeleeAttacker _meleeAttacker = new MeleeAttacker();
    [SerializeField, BoxGroup("Combat")]
    private SlowGunAttacker _gunAttacker = new SlowGunAttacker();

    [SerializeField, BoxGroup("Input")]
    private string _moveHorizontalAxis = "Horizontal";
    [SerializeField, BoxGroup("Input")]
    private string _moveVerticalAxis = "Vertical";
    [SerializeField, BoxGroup("Input")]
    private string _aimButton = "Aim";
    [SerializeField, BoxGroup("Input")]
    private string _lookHoriztonalAxis = "Mouse X";
    [SerializeField, BoxGroup("Input")]
    private string _lookVerticalAxis = "Mouse Y";
    [SerializeField, BoxGroup("Input")]
    private string _attackButton = "Fire1";
    [SerializeField, BoxGroup("Input")]
    private string _visionButton = "Vision";
    [SerializeField, BoxGroup("Input")]
    private string _crouchButton = "Crouch";
    [SerializeField, BoxGroup("Input")]
    private string _dashButton = "Dash";

    [SerializeField, BoxGroup("Etc")]
    private float _visionRadius = 10F;
    [SerializeField, BoxGroup("Etc")]
    private AnimationCurve _dashCurve;
    [SerializeField, BoxGroup("Etc")]
    private float _dashDelay = 1.5F;
    [SerializeField, BoxGroup("Etc")]
    private float _slowGunDelay = 5F;

    private Action<bool> _onDashActiveChanged = null;
    private Action<bool> _onSlowGunActiveChanged = null;

    private Camera _mainCamera;
    private Transform _cameraTransform;

    private Vector2 _axisValue;
    private float _sqrAxis;
    
    private bool _isMoving = false;
    private bool _isMeleeAttacking = false;
    private bool _isDashCooldown = false;
    private bool _isSlowGunCooldown = false;
    private int _stopFrame = 0;

    private float _healthRegenElapsed = 0F;
    private float _healthRegenAccum = 0F;
    private Collider[] _attackTargets = new Collider[5];
    private Transform _currentAttackTarget = null;
    private Vector3 _attackDirection;
    private bool _comboInputEnabled = false;
    private bool _comboTransitionEnabled = false;
    private Coroutine _dashCrt = null;
    private WaitForFixedUpdate _waitForFixedUpdate = new WaitForFixedUpdate();
    private float _dashElapsedTime = 0F;
    private float _slowGunElapsedTime = 0F;

    protected override void Awake()
    {
        base.Awake();

        _mainCamera = Camera.main;
        _cameraTransform = _mainCamera.transform;
    }

    protected override void Start()
    {
        base.Start();

        // Cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (CurrentHealth <= 0)
            return;

        UpdateHealth();
        UpdateInput();
        UpdateLocomotion();
        CheckDash();
        CheckVision();
        CheckCrouch();
        CheckAttack();
    }

    private void OnAnimatorMove()
    {
        Vector3 velocity = Animator.deltaPosition / Time.deltaTime;
        velocity.y = Rigidbody.velocity.y;
        Rigidbody.velocity = velocity;
    }

    private void UpdateHealth()
    {
        _healthRegenElapsed += Time.deltaTime;
        if (_healthRegenElapsed > 1F)
        {
            int m = Mathf.FloorToInt(_healthRegenUnit);
            _healthRegenAccum += _healthRegenUnit - m;
            int n = Mathf.FloorToInt(_healthRegenAccum);
            _healthRegenAccum -= n;

            CurrentHealth += m + n;
            _healthRegenElapsed -= 1F;
        }
    }

    private void UpdateInput()
    {
        _axisValue = new Vector2(Input.GetAxisRaw(_moveHorizontalAxis), Input.GetAxisRaw(_moveVerticalAxis));
        _sqrAxis = Mathf.Clamp01(_axisValue.sqrMagnitude);
    }

    private void CheckVision()
    {
        if (Input.GetButtonDown(_visionButton))
        {

        }
    }

    private void CheckCrouch()
    {
        if (Input.GetButtonDown(_crouchButton))
        {
            bool isCrouching = Animator.GetBool(Hash.IsCrouching);
            Animator.SetBool(Hash.IsCrouching, !isCrouching);
        }
    }

    private void CheckDash()
    {
        if (_isDashCooldown)
        {
            _dashElapsedTime += Time.deltaTime;
            if (DashRemainingDelay <= 0F)
            {
                _isDashCooldown = false;
                _dashElapsedTime = 0F;
                _onDashActiveChanged(false);
            }
        }

        if (Input.GetButtonDown(_dashButton) && !_isDashCooldown)
        {
            _isDashCooldown = true;
            _onDashActiveChanged(true);

            if (_dashCrt != null)
                StopCoroutine(_dashCrt);
            Vector3 dir = _sqrAxis > 0F ? _cameraTransform.forward * _axisValue.y + _cameraTransform.right * _axisValue.x : Transform.forward;
            dir.y = 0F;
            _dashCrt = StartCoroutine(Dash(dir));
        }
    }

    private IEnumerator Dash(Vector3 direction)
    {
        float maxTime = _dashCurve.keys[_dashCurve.length - 1].time;
        float elapsedTime = 0F;

        while (elapsedTime < maxTime)
        {
            Rigidbody.AddForce(direction * _dashCurve.Evaluate(elapsedTime), ForceMode.Acceleration);
            elapsedTime += Time.fixedDeltaTime;
            yield return _waitForFixedUpdate;
        }

        _dashCrt = null;
    }

    private void UpdateLocomotion()
    {
        bool isControlling = _sqrAxis > 0F;
        if (isControlling)
        {
            Vector3 controlDirection = _cameraTransform.forward * _axisValue.y + _cameraTransform.right * _axisValue.x;
            controlDirection.y = 0F;

            if (_isMoving)
            {
                Quaternion controlQuaternion = Quaternion.LookRotation(controlDirection);
                Transform.rotation = Quaternion.Slerp(Transform.rotation, controlQuaternion, Time.deltaTime * _runningRotateSpeed);
                Animator.SetFloat(Hash.Accel, _sqrAxis, 0.5F, Time.deltaTime);
            }

            float angle = Vector3.SignedAngle(Transform.forward, controlDirection, Vector3.up);
            Animator.SetFloat(Hash.TurnAngle, angle);
            
            _stopFrame = 0;
        }
        else
        {
            _stopFrame++;
        }

        // Rotate during attacking.
        if (_isMeleeAttacking)
        {
            if (_currentAttackTarget != null)
            {
                _attackDirection = _currentAttackTarget.position - Transform.position;
                _attackDirection.y = 0F;
            }

            Quaternion attackQuaternion = Quaternion.LookRotation(_attackDirection);
            Transform.rotation = Quaternion.Slerp(Transform.rotation, attackQuaternion, Time.deltaTime * _attackingRotateSpeed);
        }

        Animator.SetBool(Hash.IsMoving, isControlling || Animator.GetBool(Hash.IsMoving) && _stopFrame < 4);
    }

    private void CheckAttack()
    {
        if (Input.GetButtonDown(_attackButton))
        {
            if (!_isMeleeAttacking)
            {
                _comboTransitionEnabled = true;
            }
            else if (_comboInputEnabled)
            {
                _comboInputEnabled = false;
            }
        }

        if (_comboTransitionEnabled && !_comboInputEnabled)
        {
            if (_sqrAxis > 0F)
            {
                Vector3 dir = _cameraTransform.forward * _axisValue.y + _cameraTransform.right * _axisValue.x;
                dir.y = 0F;
                _attackDirection = dir;
            }
            else
            {
                _attackDirection = Transform.forward;
            }

            FindAttackTarget();

            Animator.SetTrigger(Hash.Attack);
            _comboTransitionEnabled = false;
        }
    }

    // Find attack target in nearest angle around attack direction.
    private void FindAttackTarget()
    {
        _currentAttackTarget = null;

        int count = 0;
        if ((count = Physics.OverlapSphereNonAlloc(Transform.position, _targetDetectMaxDistance, _attackTargets, _attackTargetLayer)) > 0)
        {
            float nearestAngle = _targetDetectMaxAngle;
            for (int i = 0; i < count; i++)
            {
                Vector3 diff = _attackTargets[i].transform.position - Transform.position;
                diff.y = 0F;
                float angle = Mathf.Acos(Vector3.Dot(_attackDirection.normalized, diff.normalized)) * Mathf.Rad2Deg;

                if (Mathf.Abs(angle) < _targetDetectMaxAngle)
                {
                    if (angle < nearestAngle)
                    {
                        nearestAngle = angle;
                        _currentAttackTarget = _attackTargets[i].transform;
                    }
                }
            }
        }
     }

    public override void ApplyDamage(Transform attacker, int damage, int reactionID = -1)
    {
        base.ApplyDamage(attacker, damage, reactionID);
    }

    protected override void OnDeath()
    {
        base.OnDeath();
        Animator.SetBool("IsDead", true);
    }

    public event Action<bool> OnDashActiveChanged
    {
        add { _onDashActiveChanged += value; }
        remove { _onDashActiveChanged -= value; }
    }

    public event Action<bool> OnSlowGunActiveChanged
    {
        add { _onSlowGunActiveChanged += value; }
        remove { _onSlowGunActiveChanged -= value; }
    }

    public float SlowGunDelay => _slowGunDelay;
    public float SlowGunRemainingDelay => _slowGunDelay - _slowGunElapsedTime;
    public bool IsSlowGunCooldown => _isSlowGunCooldown;
    public float DashDelay => _dashDelay;
    public float DashRemainingDelay => _dashDelay - _dashElapsedTime;
    public bool IsDashCooldown => _isDashCooldown;

    #region Animator Events

    private void SetMoving(BoolParameter isMoving)
    {
        _isMoving = isMoving.Value;
        if (!_isMoving)
            Animator.SetFloat(Hash.Accel, 0F);
    }

    private void SetAttacking(BoolParameter isAttacking)
    {
        _isMeleeAttacking = isAttacking.Value;
    }

    private void EnableComboInput()
    {
        _comboInputEnabled = true;
    }

    private void EnableComboTransition()
    {
        _comboTransitionEnabled = true;
    }

    private void DisableCombo()
    {
        if (_comboInputEnabled)
            _isMeleeAttacking = false;

        _comboInputEnabled = false;
        _comboTransitionEnabled = false;
    }

    private void CheckHit(IntParameter attackID)
    {
        _meleeAttacker.CheckHit(Transform, _currentAttackTarget, attackID.Value);
    }

    #endregion
}
