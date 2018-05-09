using UnityEngine;
using Sirenix.OdinInspector;

public class PlayerController : CharacterControllerBase
{
    private static class Hash
    {
        public static readonly int IsMoving = Animator.StringToHash("IsMoving");
        public static readonly int Accel = Animator.StringToHash("Accel");
    }

    [SerializeField, TitleGroup("Stats")]
    private float _healthRegenUnit = 5F;
    [SerializeField, TitleGroup("Stats")]
    private float _moveSpeedMultiplier = 10F;
    [SerializeField, TitleGroup("Stats")]
    private float _moveSpeedLerpMultiplier = 10F;
    [SerializeField, TitleGroup("Stats")]
    private float _moveRotateLerpMultiplier = 10F;

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
        _axisValue = new Vector2(Input.GetAxisRaw(_horizontalAxisName), Input.GetAxisRaw(_verticalAxisName));
        _axisSqrMagnitude = _axisValue.sqrMagnitude;
        _attackPressed = Input.GetButtonDown(_attackButtonName);
    }
    
    private void UpdateTransform()
    {
        bool isControlling = _axisSqrMagnitude > 0F && !_lockMove;
        if (isControlling)
        {
            Vector3 controlDir = _cameraTr.forward * _axisValue.y + _cameraTr.right * _axisValue.x;
            controlDir.Normalize();
            controlDir.y = 0F;

            _velocity = Vector3.Lerp(Rigidbody.velocity, controlDir * _moveSpeedMultiplier, Time.fixedDeltaTime * _moveSpeedLerpMultiplier);
            _rotation = Quaternion.Slerp(Rigidbody.rotation, Quaternion.LookRotation(controlDir), Time.fixedDeltaTime * _moveRotateLerpMultiplier);
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
        CurrentHealth += _healthRegenUnit * Time.deltaTime;
    }
}
