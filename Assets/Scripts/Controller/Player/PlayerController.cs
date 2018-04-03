using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;
using static PlayerAnimatorInfo;

public class PlayerController : ControllerBase
{
    [SerializeField, Required, BoxGroup("Control")]
    private Camera _camera;
    [SerializeField, BoxGroup("Control")]
    private float _rotateSpeed = 10F;

    [SerializeField, BoxGroup("Combat")]
    private float _combatDuration = 4F;

    [SerializeField, BoxGroup("Input")]
    private string _horizontalAxis = "Horizontal";
    [SerializeField, BoxGroup("Input")]
    private string _verticalAxis = "Vertical";
    [SerializeField, BoxGroup("Input")]
    private string _attackButton = "Fire1";

    private Transform _camTransform;

    private WaitForSeconds _combatDurationRef;
    private Coroutine _combatCoroutine;
    
    private int _nextComboNumber = 0;
    private bool _activeComboInput = false;
    private bool _activeComboTransition = false;

    private bool _lockMove = false;

    protected override void Awake()
    {
        base.Awake();

        _camTransform = _camera.transform;
        _combatDurationRef = new WaitForSeconds(_combatDuration);
    }

    private void Update()
    {
        UpdateLocomotion();
        TryAttack();
    }

    private void UpdateLocomotion()
    {
        if (_lockMove)
            return;

        Vector2 axisValue = new Vector2(Input.GetAxisRaw(_horizontalAxis), Input.GetAxisRaw(_verticalAxis));
        bool isControlling = axisValue.sqrMagnitude > 0F;
        Vector3 controlDirection = _camTransform.forward * axisValue.y + _camTransform.right * axisValue.x;
        controlDirection.y = 0F;

        // Move
        Animator.SetBool(Hash.IsMoving, isControlling);

        // Turn
        if (isControlling)
        {
            Quaternion look = Quaternion.LookRotation(controlDirection);
            Transform.rotation = Quaternion.Slerp(Transform.rotation, look, Time.deltaTime * _rotateSpeed);
        }
    }

    private void TryAttack()
    {
        if (Input.GetButtonDown(_attackButton))
        {
            if (_activeComboInput)
            {
                if (_nextComboNumber == 0)
                    _activeComboTransition = true;
                
                _nextComboNumber = _nextComboNumber % Value.MaxCombo + 1;
                _activeComboInput = false;
            }
        }

        if (_activeComboTransition && !_activeComboInput)
        {
            Animator.SetInteger(Hash.ComboNumber, _nextComboNumber);
            Animator.SetTrigger(Hash.Attack);
            _activeComboTransition = false;
        }
    }
    
    private IEnumerator CombatToNormal()
    {
        yield return _combatDurationRef;
        Animator.SetBool(Hash.IsCombating, false);
    }

    #region Animation Events

    private void LockMove(EventParameter param)
    {
        _lockMove = param.BoolParameter;
        if (_lockMove)
            Animator.SetBool(Hash.IsMoving, false);
    }

    private void SetToCombating()
    {
        Animator.SetBool(Hash.IsCombating, true);
    }

    private void EnterCombat()
    {
        _combatCoroutine = StartCoroutine(CombatToNormal());
    }

    private void ExitCombat()
    {
        StopCoroutine(_combatCoroutine);
    }

    private void EnableComboInput()
    {
        _activeComboInput = true;
    }

    private void EnableComboTransition()
    {
        _activeComboTransition = true;
    }

    private void DisableCombo()
    {
        _activeComboInput = false;
        _activeComboTransition = false;
    }

    private void ResetComboNumber(EventParameter param)
    {
        if (param.IntParameter == _nextComboNumber)
        {
            _nextComboNumber = 0;
        }
    }

    #endregion
}