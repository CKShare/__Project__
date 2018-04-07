using UnityEngine;

[SharedBetweenAnimators]
public class ZeromStandingRunningTurn_RotateCorrection : StateMachineBehaviour
{
    private float _error;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        float initialAngle = animator.GetFloat(PlayerAnimatorInfo.Hash.TurnAngle);
        _error = 1F - Mathf.Abs(initialAngle) / 180F;
    }

    public override void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        float deltaYaw = animator.deltaRotation.eulerAngles.y;
        if (deltaYaw > 180F)
            deltaYaw -= 360F;
        deltaYaw *= _error;
        
        animator.transform.eulerAngles -= new Vector3(0F, deltaYaw, 0F);
    }
}
