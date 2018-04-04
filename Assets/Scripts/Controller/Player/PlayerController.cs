using System.Collections;
using UnityEngine;
using RootMotion.FinalIK;
using Sirenix.OdinInspector;
using static PlayerAnimatorInfo;

[RequireComponent(typeof(AimIK))]
public class PlayerController : ControllerBase
{
    [SerializeField, Required]
    private Camera _camera;

    [SerializeField]
    private float _holsterTransitionSpeed = 0.3F;
    [SerializeField]
    private float _unholsterTransitionSpeed = 0.3F;

    [SerializeField, BoxGroup("Input")]
    private string _aimButton = "Aim";
    [SerializeField, BoxGroup("Input")]
    private string _lookHoriztonalAxis = "Mouse X";
    [SerializeField, BoxGroup("Input")]
    private string _lookVerticalAxis = "Mouse Y";

    private Transform _cameraTransform;
    private AimIK _aimIK;

    private bool _isAiming = false;
    private bool _isHolstering = false;
    private bool _isUnholstering = false;

    protected override void Awake()
    {
        base.Awake();

        _cameraTransform = _camera.transform;
        _aimIK = GetComponent<AimIK>();
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
            RaycastHit hitInfo;
            Vector3 aimPoint;
            Ray ray = _camera.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0F));
            if (Physics.Raycast(ray, out hitInfo, 100F))
            {
                aimPoint = hitInfo.point;
            }
            else
            {
                aimPoint = ray.GetPoint(100F);
            }

            _aimIK.solver.IKPosition = aimPoint;

            if (_isUnholstering)
                _aimIK.solver.IKPositionWeight = Mathf.Lerp(_aimIK.solver.IKPositionWeight, 1F, _unholsterTransitionSpeed);
        }
        else
        {
            _aimIK.solver.IKPositionWeight = Mathf.Lerp(_aimIK.solver.IKPositionWeight, 0F, _holsterTransitionSpeed);
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

    private void SetHolstering(EventParameter param)
    {
        _isHolstering = param.BoolParameter;
    }

    private void SetUnholstering(EventParameter param)
    {
        _isUnholstering = param.BoolParameter;
    }

    #endregion
}
