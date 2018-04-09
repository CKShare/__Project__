using System.Collections;
using UnityEngine;
using Cinemachine;
using RootMotion.FinalIK;
using Sirenix.OdinInspector;
using static PlayerAnimatorInfo;

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

    [SerializeField, BoxGroup("Combat")]
    private LayerMask _attackTargetLayer;

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
    private Collider _collider;

    private bool _isMoving = false;
    private bool _isAttacking = false;
    private bool _isAiming = false;
    private int _stopFrame = 0;

    private MeleeAttackInfo _attackInfo = null;
    private Collider[] _attackTargets = new Collider[5];
    private Transform _attackTarget = null;
    private Vector3 _attackDirection;
    private bool _rotateToAttack = false;
    private bool _comboInputEnabled = false;
    private bool _comboTransitionEnabled = false;

    protected override void Awake()
    {
        base.Awake();

        _cameraTransform = _mainCamera.transform;
        _collider = GetComponent<Collider>();

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

            // Set attacking direction during a few frames.
            if (!_isAttacking || _rotateToAttack)
            {
                _attackDirection = controlDirection;
            }

            if (_isMoving)
            {
                Quaternion controlQuaternion = Quaternion.LookRotation(controlDirection);
                Transform.rotation = Quaternion.Slerp(Transform.rotation, controlQuaternion, Time.deltaTime * _runningRotateSpeed);
                Animator.SetFloat(Hash.Accel, sqrMagnitude, 0.5F, Time.deltaTime);
            }

            float angle = Vector3.SignedAngle(Transform.forward, controlDirection, Vector3.up);
            Animator.SetFloat(Hash.TurnAngle, angle);
            
            _stopFrame = 0;
        }
        else
        {
            _stopFrame++;
        }

        // Rotate during attacking using direction vector got above.
        if (_rotateToAttack)
        {
            if (_attackTarget == null)
                FindAttackTarget();
            
            if (_attackTarget != null)
            {
                _attackDirection = _attackTarget.position - Transform.position;
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
            if (!_isAttacking)
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
            Animator.SetTrigger(Hash.Attack);
            _comboTransitionEnabled = false;
        }
    }

    // Find attack target in nearest angle around attack direction.
    private void FindAttackTarget()
    {
        int count = 0;
        if ((count = Physics.OverlapSphereNonAlloc(Transform.position, _attackInfo.DetectMaxDistance, _attackTargets, _attackTargetLayer)) > 0)
        {
            float nearestAngle = _attackInfo.DetectMaxAngle;
            for (int i = 0; i < count; i++)
            {
                Vector3 diff = _attackTargets[i].transform.position - Transform.position;
                float angle = Mathf.Acos(Vector3.Dot(_attackDirection.normalized, diff.normalized)) * Mathf.Rad2Deg;

                if (Mathf.Abs(angle) <= _attackInfo.DetectMaxAngle)
                {
                    if (_attackTarget == null || angle < nearestAngle)
                    {
                        nearestAngle = angle;
                        _attackTarget = _attackTargets[i].transform;
                        Debug.Log(_attackTarget.name);
                    }
                }
            }
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

    private void StartAttacking(EventParameter param)
    {
        _attackInfo = param.ObjectParameter as MeleeAttackInfo;
        _isAttacking = true;
        _attackTarget = null;
    }

    private void ResetAttacking()
    {
        _isAttacking = false;
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

    #endregion
}
