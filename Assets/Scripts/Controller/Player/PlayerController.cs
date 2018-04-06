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

    private Transform _cameraTransform;
    private LookAtIK _lookAtIK;

    private bool _isRunning = false;
    private bool _isAiming = false;
    private int _stopFrame = 0;

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
            float angle = Vector3.SignedAngle(Transform.forward, controlDirection, Vector3.up);

            if (_isRunning)
            {
                Quaternion controlQuaternion = Quaternion.LookRotation(controlDirection);
                Transform.rotation = Quaternion.Slerp(Transform.rotation, controlQuaternion, Time.deltaTime * 10F);

                Animator.SetFloat(Hash.Accel, sqrMagnitude, 1F, Time.deltaTime);
            }
            Animator.SetFloat(Hash.TurnAngle, angle);

            _stopFrame = 0;
        }
        else
        {
            _stopFrame++;
        }

        Animator.SetBool(Hash.IsMoving, isControlling || _stopFrame < 4);
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

    private void SetRunning(EventParameter param)
    {
        _isRunning = param.BoolParameter;
        if (!_isRunning)
            Animator.SetFloat(Hash.Accel, 0F);
    }

    #endregion
}
