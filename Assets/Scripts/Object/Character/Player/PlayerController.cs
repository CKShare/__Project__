using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;

public class PlayerController : ControllerBase<PlayerDatabase>
{
    public static class Hash
    {
        public static int IsMoving = Animator.StringToHash("IsMoving");
        public static int Accel = Animator.StringToHash("Accel");
        public static int TurnAngle = Animator.StringToHash("TurnAngle");
        public static int IsCrouching = Animator.StringToHash("IsCrouching");
        public static int Dash = Animator.StringToHash("Dash");
        public static int Attack = Animator.StringToHash("Attack");
    }

    private Transform _cameraTr;

    private Vector2 _axisValue;
    private float _axisSqrMag;
    private Vector3 _controlDirection;
    private float _movingElapsedTime;
 
    private bool _dashPressed;
    //private Coroutine _dashCrt;
    //private WaitForFixedUpdate _waitForFixedUpdate = new WaitForFixedUpdate();
    private bool _dashCooldown;
    private float _dashElapsedCoolTime;

    private bool _attackPressed;

    protected override void Awake()
    {
        base.Awake();

        _cameraTr = Camera.main.transform;
    }

    protected override void Start()
    {
        base.Start();
    }

    private void OnAnimatorMove()
    {
        Vector3 velocity = Animator.deltaPosition / Time.fixedDeltaTime;
        velocity.y = Rigidbody.velocity.y;
        Rigidbody.velocity = velocity;
    }

    private void Update()
    {
        UpdateInput();
        UpdateLocomotion();
        CheckDash();
        CheckAttack();
    }

    private void UpdateInput()
    {
        _axisValue = new Vector2(Input.GetAxisRaw(Database.InputHorizontal), Input.GetAxisRaw(Database.InputVertical));
        _axisSqrMag = _axisValue.sqrMagnitude;
        _dashPressed = Input.GetButtonDown(Database.InputDash);
        //_attackPressed = Input.GetButtonDown(_database.InputAttack);
    }

    private void UpdateLocomotion()
    {
        //if (_axisSqrMag > 0F)
        //{
        //    Vector3 controlDirection;
        //    if (TryGetControlDirection(out controlDirection))
        //        _controlDirection = controlDirection;
        //}

        Vector3 controlDirection;
        _movingElapsedTime = TryGetControlDirection(out controlDirection) ? _movingElapsedTime + Time.fixedDeltaTime : 0F;
        Rigidbody.velocity = controlDirection * (Database. * Time.fixedDeltaTime);

        //Animator.SetFloat(Hash.TurnAngle, Vector3.SignedAngle(Transform.forward, _controlDirection, Vector3.up));
        Animator.SetFloat(Hash.Accel, Mathf.Clamp01(_axisSqrMag), 0.1F, Time.deltaTime);
        Animator.SetBool(Hash.IsMoving, _axisSqrMag > 0F);
    }

    private void CheckDash()
    {
        if (_dashCooldown)
        {
            _dashElapsedCoolTime += Time.deltaTime;
            if (_dashElapsedCoolTime >= Database.DashCoolTime)
            {
                _dashCooldown = false;
                _dashElapsedCoolTime = 0F;
            }
        }   
        else if (_dashPressed)
        {
            _dashCooldown = true;
            Animator.SetTrigger(Hash.Dash);
            //_dashCrt = StartCoroutine(Dash());
        }
    }

    //private IEnumerator Dash()
    //{
    //    var curve = _database.DashCurve;
    //    float maxTime = curve.keys[curve.length - 1].time;
    //    float elapsedTime = 0F;
    //    Vector3 direction = _axisSqrMag > 0F ? GetControlDirection() : Transform.forward;
    //    Quaternion look = Quaternion.LookRotation(direction);  

    //    Animator.SetTrigger(Hash.Dash);

    //    while (elapsedTime < maxTime)
    //    {
    //        Rigidbody.AddForce(direction * curve.Evaluate(elapsedTime), ForceMode.Acceleration);
    //        Transform.rotation = Quaternion.Slerp(Transform.rotation, look, Time.fixedDeltaTime * 30F);
            
    //        elapsedTime += Time.fixedDeltaTime;
    //        yield return _waitForFixedUpdate;
    //    }

    //    _dashCrt = null;
    //}

    private void CheckAttack()
    {
        if (_attackPressed)
        {

        }
    }

    public bool TryGetControlDirection(out Vector3 controlDirection)
    {
        if (_axisSqrMag > 0F)
        {
            controlDirection = _cameraTr.forward * _axisValue.y + _cameraTr.right * _axisValue.x;
            controlDirection.y = 0F;
            return true;
        }

        controlDirection = Vector3.zero;
        return false;
    }

    #region Animator Events


    #endregion
}