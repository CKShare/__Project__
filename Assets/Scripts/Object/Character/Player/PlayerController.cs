using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;
using RootMotion.FinalIK;

[RequireComponent(typeof(AimIK))]
public class PlayerController : ControllerBase<PlayerDatabase>
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
    }

    private static readonly float TurnningAttackAngleThreshold = 75F;
    private static readonly float AttackRotateSpeed = 30F;
    private static readonly float AimingRotateSpeed = 30F;

    private Transform _cameraTr;
    private AimIK _aimIK;

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
    private bool _lockMove;

    protected override void Awake()
    {
        base.Awake();

        _cameraTr = Camera.main.transform;
        _aimIK = GetComponent<AimIK>();
    }

    protected override void Start()
    {
        base.Start();
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
        UpdateLocomotion();
    }

    private void Update()
    {
        UpdateInput();
        CheckDash();
        CheckAttack();
        CheckAim();
    }

    private void UpdateInput()
    {
        _axisValue = new Vector2(Input.GetAxisRaw(Database.InputHorizontal), Input.GetAxisRaw(Database.InputVertical));
        _axisSqrMag = _axisValue.sqrMagnitude;
        _dashPressed = Input.GetButtonDown(Database.InputDash);
        _attackPressed = Input.GetButtonDown(Database.InputAttack);
        _aimPressing = Input.GetButton(Database.InputAim);
    }

    private void UpdateLocomotion()
    {
        Vector3 controlDirection, velocity;
        bool isControlling = TryGetControlDirection(out controlDirection);
        bool isMoving = !_lockMove && isControlling;

        // Move
        if (isMoving)
        {
            _movingElapsedTime = _movingElapsedTime + Time.fixedDeltaTime;
            velocity = controlDirection * Database.MoveCurve.Evaluate(_movingElapsedTime);
            velocity = Vector3.Lerp(Rigidbody.velocity, velocity, Time.fixedDeltaTime * Database.MoveLerpTime);
        }
        else
        {
            velocity = Vector3.zero;
        }
        velocity.y = Rigidbody.velocity.y;
        Rigidbody.velocity = velocity;

        // Rotate
        if (isMoving) // Moving
        {
            Quaternion look = Quaternion.LookRotation(controlDirection);
            Rigidbody.rotation = Quaternion.Slerp(Rigidbody.rotation, look, Database.RotateSpeed * Time.fixedDeltaTime);
        }
        else if (_isAttacking && !_comboTransitionEnabled) // Attacking
        {
            if (_target != null)
            {
                _attackDirection = _target.position - Transform.position;
                _attackDirection.y = 0F;
            }
            
            Quaternion look = Quaternion.LookRotation(_attackDirection);
            Rigidbody.rotation = Quaternion.Slerp(Rigidbody.rotation, look, AttackRotateSpeed * Time.fixedDeltaTime);
        }
        else if (_aimPressing) // Aiming
        {
            Vector3 camForward = _cameraTr.forward;
            camForward.y = 0F;

            Quaternion look = Quaternion.LookRotation(camForward);
            Rigidbody.rotation = Quaternion.Slerp(Rigidbody.rotation, look, AimingRotateSpeed * Time.fixedDeltaTime);
        }

        // Animator
        Animator.SetFloat(Hash.Accel, Mathf.Clamp01(_axisSqrMag), 0.1F, Time.deltaTime);
        Animator.SetBool(Hash.IsMoving, isMoving);
    }

    private void CheckDash()
    {
        if (_dashCooldown)
        {
            _dashElapsedCoolTime += Time.deltaTime;
            if (_dashElapsedCoolTime >= Database.DashCoolTime)
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
        var curve = Database.DashCurve;
        float maxTime = curve.keys[curve.length - 1].time;
        float elapsedTime = 0F;
        Vector3 direction;
        if (!TryGetControlDirection(out direction))
            direction = Transform.forward;
        Quaternion look = Quaternion.LookRotation(direction);

        Animator.SetTrigger(Hash.Dash);

        while (elapsedTime < maxTime)
        {
            Rigidbody.AddForce(direction * curve.Evaluate(elapsedTime), ForceMode.Acceleration);
            Rigidbody.rotation = Quaternion.Slerp(Rigidbody.rotation, look, Time.fixedDeltaTime * Database.RotateSpeed * 2F);

            elapsedTime += Time.fixedDeltaTime;
            yield return _waitForFixedUpdate;
        }

        _dashCrt = null;
    }

    private void CheckAttack()
    {
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

            // Get target for targeting.
            _target = GameUtility.FindTargetInView(_targetPool, Transform.position, _attackDirection, Database.TargetingMaxDistance, Database.TargetingMaxAngle, Database.TargetLayer);

            _comboSaved = false;
            _comboTransitionEnabled = false;
            _comboNumber = _comboNumber % Database.MaxCombos[(int)_attackType - 1] + 1;
            Animator.SetInteger(Hash.AttackType, (int)_attackType);
            Animator.SetInteger(Hash.ComboNumber, _comboNumber);
            Animator.SetTrigger(Hash.Attack);
        }
    }

    private void CheckAim()
    {
        if (_aimPressing)
        {
            if (!Animator.GetBool(Hash.IsAiming))
            {
                _lockMove = true;
                Animator.SetTrigger(Hash.Aim);
            }
        }

        Animator.SetBool(Hash.IsAiming, _aimPressing);
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

    #region Animator Events

    private void OnRunStart()
    {
        _applyRootMotion = false;
    }

    private void OnRunFinish()
    {
        _applyRootMotion = true;
    }

    private void OnDashStart()
    {
        _lockMove = true;
    }

    private void OnDashFinish()
    {
        _lockMove = false;
    }

    private void OnAttackStart()
    {
        _lockMove = true;
    }

    private void OnAttackFinish()
    {
        _lockMove = false;
    }

    private void EnableComboInput()
    {
        _comboInputEnabled = true;
    }

    private void EnableComboTransition()
    {
        _comboTransitionEnabled = true;
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

    #endregion
}