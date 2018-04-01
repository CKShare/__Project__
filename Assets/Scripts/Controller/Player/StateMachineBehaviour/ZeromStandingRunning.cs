using UnityEngine;

public class ZeromStandingRunning : StateMachineBehaviour
{
    [SerializeField]
    private AnimationCurve _accelCurve;

    private Transform _transform;
    private Rigidbody _rigidbody;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_transform == null)
            _transform = animator.transform;
        if (_rigidbody == null)
            _rigidbody = animator.GetComponent<Rigidbody>();
    }

    public override void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Vector3 accel = _transform.TransformVector(new Vector3(0F, 0F, _accelCurve.Evaluate(stateInfo.normalizedTime)));
        _rigidbody.velocity += accel;
    }
}
