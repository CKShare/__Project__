using UnityEngine;

public class ESPlayerDash : EventScope
{
    [SerializeField]
    private AnimationCurve _dashCurve = new AnimationCurve();

    private PlayerController _controller;

    public override void OnScopeEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_controller == null)
            _controller = animator.GetComponent<PlayerController>();
    }

    public override void OnScopeUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Vector3 direction;
        if (!_controller.TryGetControlDirection(out direction))
            direction = _controller.Transform.forward;
        Quaternion look = Quaternion.LookRotation(direction);

        _controller.Rigidbody.AddForce(direction * _dashCurve.Evaluate(stateInfo.normalizedTime), ForceMode.Acceleration);
        _controller.Rigidbody.rotation = Quaternion.Slerp(_controller.Rigidbody.rotation, look, Time.fixedDeltaTime * 30F);
    }
}