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

    private Vector2 _axisValue;
    private float _sqrAxis;
    
    private bool _isMoving = false;
    private bool _isAttacking = false;
    private bool _isAiming = false;
    private int _stopFrame = 0;

    private MeleeAttackInfo _attackInfo = null;
    private Collider[] _attackTargets = new Collider[5];
    private Transform _attackTarget = null;
    private Vector3 _attackDirection;
    private bool _comboInputEnabled = false;
    private bool _comboTransitionEnabled = false;

    protected override void Awake()
    {
        base.Awake();

        _cameraTransform = _mainCamera.transform;

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
        UpdateInput();
        UpdateLocomotion();
        CheckAttack();
        //CheckAim();
    }

    private void UpdateInput()
    {
        _axisValue = new Vector2(Input.GetAxisRaw(_moveHorizontalAxis), Input.GetAxisRaw(_moveVerticalAxis));
        _sqrAxis = Mathf.Clamp01(_axisValue.sqrMagnitude);
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
        if (_isAttacking)
        {
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
                diff.y = 0F;
                float angle = Mathf.Acos(Vector3.Dot(_attackDirection.normalized, diff.normalized)) * Mathf.Rad2Deg;

                if (Mathf.Abs(angle) <= _attackInfo.DetectMaxAngle)
                {
                    if (_attackTarget == null || angle < nearestAngle)
                    {
                        nearestAngle = angle;
                        _attackTarget = _attackTargets[i].transform;
                    }
                }
            }

            if (_attackTarget != null)
                Debug.Log($"Attack Target : {_attackTarget.name}");
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

    private void SetMoving(BoolParameter isMoving)
    {
        _isMoving = isMoving.Value;
        if (!_isMoving)
            Animator.SetFloat(Hash.Accel, 0F);
    }

    private void StartAttacking(ObjectParameter attackInfo)
    {
        _isAttacking = true;
        _attackTarget = null;

        _attackInfo = attackInfo.Value as MeleeAttackInfo;
        FindAttackTarget();
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
        if (_comboInputEnabled)
            ResetAttacking();

        _comboInputEnabled = false;
        _comboTransitionEnabled = false;
    }

    #endregion
}
