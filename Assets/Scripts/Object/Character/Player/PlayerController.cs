using UnityEngine;
using Sirenix.OdinInspector;

public class PlayerController : CharacterControllerBase<PlayerDatabase>
{
    private static class Hash
    {
        public static readonly int IsMoving = Animator.StringToHash("IsMoving");
        public static readonly int Accel = Animator.StringToHash("Accel");
    }

    [SerializeField, Required]
    private InputSettings _inputSettings;

    private Camera _camera;
    private Transform _cameraTr;

    private Vector3 _velocity;
    private Quaternion _rotation;
    private bool _applyRootMotion;

    private Vector2 _axisValue;
    private float _axisSqrMagnitude;
    private bool _lockMove;
    
    private bool _attackPressed;
    
    protected override void Awake()
    {
        base.Awake();

        _camera = Camera.main;
        _cameraTr = _camera.transform;

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
        UpdateTransform();

        RegenHealth();
    }

    private void UpdateInput()
    {
        _axisValue = new Vector2(Input.GetAxisRaw(_inputSettings.HorizontalAxisName), Input.GetAxisRaw(_inputSettings.VerticalAxisName));
        _axisSqrMagnitude = _axisValue.sqrMagnitude;
        _attackPressed = Input.GetButtonDown(_inputSettings.AttackButtonName);
    }
    
    private void UpdateTransform()
    {
        bool isControlling = _axisSqrMagnitude > 0F && !_lockMove;
        if (isControlling)
        {
            Vector3 controlDir = _cameraTr.forward * _axisValue.y + _cameraTr.right * _axisValue.x;
            controlDir.Normalize();
            controlDir.y = 0F;

            _velocity = Vector3.Lerp(Rigidbody.velocity, controlDir * Database.MoveSpeedMultiplier, Time.fixedDeltaTime * Database.MoveSpeedLerpMultiplier);
            _rotation = Quaternion.Slerp(Rigidbody.rotation, Quaternion.LookRotation(controlDir), Time.fixedDeltaTime * Database.MoveRotateLerpMultiplier);
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

    private void RegenHealth()
    {
        CurrentHealth += Database.HealthRegenUnit * Time.deltaTime;
    }
}
