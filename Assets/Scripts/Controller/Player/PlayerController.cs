using System.Collections;
using UnityEngine;
using Cinemachine;
using RootMotion.FinalIK;
using Sirenix.OdinInspector;
using static PlayerAnimatorInfo;

[RequireComponent(typeof(LookAtIK))]
public class PlayerController : ControllerBase
{
    [SerializeField, Required, BoxGroup("Camera")]
    private Camera _mainCamera;
    [SerializeField, Required, BoxGroup("Camera")]
    private CinemachineFreeLook _aimVCam;

    [SerializeField, BoxGroup("Locomotion")]
    private float _runningRotateSpeed = 10F;
    [SerializeField, BoxGroup("Locomotion")]
    private float _attackingRotateSpeed = 20F; // Should be high enough to rotate completely during about 0.1 ~ 0.2 seconds.

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

    private Transform _cameraTransform;
    private LookAtIK _lookAtIK;

    private bool _isMoving = false;
    private bool _isAttacking = false;
    private bool _isAiming = false;
    private int _controlFrame = 0;
    private int _stopFrame = 0;

    private Transform _attackTarget = null;
    private bool _rotateToAttack = false;
    private bool _comboInputEnabled = false;
    private bool _comboTransitionEnabled = false;

    protected override void Awake()
    {
        base.Awake();

        _cameraTransform = _mainCamera.transform;
        _lookAtIK = GetComponent<LookAtIK>();

        // Cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void FixedUpdate()
    {
        // If this is not in FixedUpdate, jitter will be occured when moving or clamped.
        //UpdateLook();
        //ClampLook();
    }

    private void Update()
    {
        //CheckAim();
        UpdateLocomotion();
        CheckAttack();
    }

    private void CheckAim()
    {
        if (Input.GetButtonDown(_aimButton))
        {
            OnAimEnabled();
        }
        else if (Input.GetButtonUp(_aimButton))
        {
            OnAimDisabled();
        }
    }

    // Update yaw and pitch when aiming.
    private void UpdateLook()
    {
        if (_isAiming)
        {
            Vector3 angleDiff = _cameraTransform.eulerAngles - Transform.eulerAngles;
            float yaw = angleDiff.y > 180F ? angleDiff.y - 360F : angleDiff.y;
            float pitch = angleDiff.x > 180F ? angleDiff.x - 360F : angleDiff.x;
            
            Animator.SetFloat(Hash.Yaw, yaw, 0.1F, Time.deltaTime);
            Animator.SetFloat(Hash.Pitch, pitch, 0.1F, Time.deltaTime);
        }
    }

    // Clamp yaw rotation range when aiming.
    private void ClampLook()
    {
        if (_isAiming)
        {
            float value = _aimVCam.m_XAxis.Value;
            if (value > 90F && value < 270F)
            {
                value = value < 180F ? 90F : 270F;
            }

            _aimVCam.m_XAxis.Value = value;
        }
    }

    private void UpdateLocomotion()
    {
        Vector2 axisValue = new Vector2(Input.GetAxisRaw(_moveHorizontalAxis), Input.GetAxisRaw(_moveVerticalAxis));
        float sqrMagnitude = Mathf.Clamp01(axisValue.sqrMagnitude);
        bool isControlling = sqrMagnitude > 0F;
        
        if (isControlling)
        {
            Vector3 controlDirection = _cameraTransform.forward * axisValue.y + _cameraTransform.right * axisValue.x;
            controlDirection.y = 0F;

            if (_isMoving || _rotateToAttack)
            {
                float rotateSpeed;
                if (_isMoving)
                {
                    rotateSpeed = _runningRotateSpeed;
                    Animator.SetFloat(Hash.Accel, sqrMagnitude, 1F, Time.deltaTime);
                }
                else
                {
                    rotateSpeed = _attackingRotateSpeed;
                }

                Quaternion controlQuaternion = Quaternion.LookRotation(controlDirection);
                Transform.rotation = Quaternion.Slerp(Transform.rotation, controlQuaternion, Time.deltaTime * rotateSpeed);
            }

            float angle = Vector3.SignedAngle(Transform.forward, controlDirection, Vector3.up);
            Animator.SetFloat(Hash.TurnAngle, angle);

            _controlFrame++;
            _stopFrame = 0;
        }
        else
        {
            _stopFrame++;
            _controlFrame = 0;
        }

        Animator.SetBool(Hash.IsMoving, _controlFrame > 3 || _stopFrame < 4);
    }

    private void CheckAttack()
    {
        if (Input.GetButtonDown(_attackButton))
        {
            if (!_isAttacking)
            {
                _comboTransitionEnabled = true;
                _isAttacking = true;
            }
            else if (_comboInputEnabled)
            {
                _comboInputEnabled = false;
            }
        }

        if (_comboTransitionEnabled && !_comboInputEnabled)
        {
            Animator.SetTrigger(Hash.Attack);
            _comboTransitionEnabled = false;
        }
    }

    private void OnAimEnabled()
    {
        _isAiming = true;
        Animator.SetBool(Hash.IsAiming, true);
    }

    private void OnAimDisabled()
    {
        _isAiming = false;
        Animator.SetBool(Hash.IsAiming, false);
    }

    #region Animator Events

    private void SetMoving(EventParameter param)
    {
        _isMoving = param.BoolParameter;
        if (!_isMoving)
            Animator.SetFloat(Hash.Accel, 0F);
    }

    private void SetRotateToAttack(EventParameter param)
    {
        _rotateToAttack = param.BoolParameter;
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
        if (_comboInputEnabled) // Not be connected to next combo.
            ResetAttacking();

        _comboInputEnabled = false;
        _comboTransitionEnabled = false;
    }

    private void ResetAttacking()
    {
        _isAttacking = false;
    }

    #endregion
}
