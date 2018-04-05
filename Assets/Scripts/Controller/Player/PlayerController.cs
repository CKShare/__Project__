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
    private string _aimButton = "Aim";
    [SerializeField, BoxGroup("Input")]
    private string _lookHoriztonalAxis = "Mouse X";
    [SerializeField, BoxGroup("Input")]
    private string _lookVerticalAxis = "Mouse Y";

    private Transform _cameraTransform;
    private LookAtIK _lookAtIK;

    private bool _isAiming = false;

    protected override void Awake()
    {
        base.Awake();

        _cameraTransform = _mainCamera.transform;
        _lookAtIK = GetComponent<LookAtIK>();

        // Cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        CheckAim();
        UpdateLook();
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

    private void UpdateLook()
    {
        if (_isAiming)
        {
            Vector3 dir = (Transform.position - _cameraTransform.position).normalized;
            Vector3 selfForward = Transform.forward;
            
            float yaw = Vector3.SignedAngle(selfForward, new Vector3(dir.x, 0F, dir.z), Vector3.up);
            float pitch = (_cameraTransform.eulerAngles - Transform.eulerAngles).x;
            if (pitch > 180F)
                pitch -= 360F;
            
            float value = _aimVCam.m_XAxis.Value;
            if (value > 90F && value < 270F)
            {
                if (value < 180F)
                    value = 90F;
                else
                    value = 270F;
            }

            _aimVCam.m_XAxis.Value = value;
            
            Animator.SetFloat(Hash.Yaw, yaw, 0.2F, Time.deltaTime);
            Animator.SetFloat(Hash.Pitch, pitch, 0.2F, Time.deltaTime);
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

    #endregion
}
