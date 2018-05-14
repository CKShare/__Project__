using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using RootMotion.FinalIK;

[RequireComponent(typeof(GrounderFBBIK))]
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
    }

    [SerializeField, TitleGroup("Stats")]
    private float _healthRegenUnit = 5F;

    [SerializeField, TitleGroup("Movement")]
    private float _moveSpeed = 7.5F;
    [SerializeField, TitleGroup("Movement")]
    private float _moveLerpSpeed = 7.5F;
    [SerializeField, TitleGroup("Movement")]
    private float _moveRotateSpeed = 10F;

    [SerializeField, DisableContextMenu, TitleGroup("Combat")]
    private Dictionary<int, HitInfo> _attackDict = new Dictionary<int, HitInfo>();
    [SerializeField, TitleGroup("Combat")]
    private int[] _maxCombos = new int[0];
    [SerializeField, TitleGroup("Combat")]
    private float _attackRotateSpeed = 10F;

    [SerializeField, Required, TitleGroup("Input")]
    private string _horizontalAxisName = "Horizontal";
    [SerializeField, Required, TitleGroup("Input")]
    private string _verticalAxisName = "Vertical";
    [SerializeField, Required, TitleGroup("Input")]
    private string _attackButtonName = "Attack";

    private Camera _camera;
    private Transform _cameraTr;

    private Vector3 _velocity;
    private Quaternion _rotation;
    private bool _applyRootMotion;

    private Vector2 _axisValue;
    private float _axisSqrMagnitude;
    private bool _lockMove;
    
    private bool _attackPressed;
    private Vector3 _attackDirection;
    private PatternType _currentPatternType = PatternType.Front;
    private int _currentComboNumber;
    private bool _isAttacking;
    private bool _comboSaved;
    private bool _comboInputEnabled;
    private bool _comboTransitionEnabled;
    
    protected override void Awake()
    {
        base.Awake();
        
        _camera = Camera.main;
        _cameraTr = _camera.transform;

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
            Rigidbody.rotation = Animator.rootRotation;
        }
    }

    private void FixedUpdate()
    {
        UpdateRigidbody();
    }

    private void UpdateRigidbody()
    {
        Rigidbody.velocity = _velocity;
        Rigidbody.rotation = _rotation;
    }

    private void Update()
    {
        UpdateInput();
        UpdateMovement();
        UpdateAttack();

        RegenHealth();
    }

    private void UpdateInput()
    {
        _axisValue = new Vector2(Input.GetAxisRaw(_horizontalAxisName), Input.GetAxisRaw(_verticalAxisName));
        _axisSqrMagnitude = _axisValue.sqrMagnitude;
        _attackPressed = Input.GetButtonDown(_attackButtonName);
    }
    
    private void UpdateMovement()
    {
        bool isControlling = _axisSqrMagnitude > 0F && !_lockMove;
        if (isControlling)
        {
            Vector3 controlDir = _cameraTr.forward * _axisValue.y + _cameraTr.right * _axisValue.x;
            controlDir.Normalize();
            controlDir.y = 0F;

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

    private void UpdateAttack()
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
        }

        if (_isAttacking && !_comboTransitionEnabled)
        {
            _rotation = Quaternion.Slerp(Transform.rotation, Quaternion.LookRotation(_attackDirection), Time.deltaTime * _attackRotateSpeed);
        }
    }

    private void RegenHealth()
    {
        CurrentHealth += _healthRegenUnit * Time.deltaTime;
    }

    #region Animation Events

    private void EnableComboInput()
    {
        _comboInputEnabled = true;
    }

    private void EnableComboTransition()
    {
        _comboTransitionEnabled = true;
    }

    private void CheckHit(int attackID)
    {
        var hitInfo = _attackDict[attackID];
        
    }

    #endregion

    #region Animator Events

    private void OnAttackEnter()
    {
        _lockMove = true;
    }

    private void OnAttackExit()
    {
        _lockMove = false;
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

    #endregion
}
