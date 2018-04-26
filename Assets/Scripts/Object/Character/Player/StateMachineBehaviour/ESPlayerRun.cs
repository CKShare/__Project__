using UnityEngine;

public class ESPlayerRun : EventScope
{
    private PlayerController _controller;

    public override void OnScopeEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_controller == null)
            _controller = animator.GetComponent<PlayerController>();
    }

    public override void OnScopeUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Vector3 dir;
        _controller.TryGetControlDirection(out dir);

        Quaternion look = Quaternion.LookRotation(dir);
        _controller.Rigidbody.rotation = Quaternion.Slerp(_controller.Rigidbody.rotation, look, Time.fixedDeltaTime * 6F);
    }

    //public override void OnScopeMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
        
    //}
}