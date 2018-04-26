using UnityEngine;

public class EventScope_Turnning : EventScope
{
    [SerializeField]
    private AnimationCurve _angleCurve;
    [SerializeField]
    private float targetAngle;

    private float _y;
    private float _error;

    //public override void OnScopeEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    float initialAngle = animator.GetFloat(PlayerAnimatorInfo.Hash.TurnAngle);
    //    _error = Mathf.Abs(initialAngle) / targetAngle;
    //    _y = animator.transform.eulerAngles.y;
    //}

    //public override void OnScopeMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    float deltaYaw = _angleCurve.Evaluate(stateInfo.normalizedTime);
    //    deltaYaw *= _error;
        
    //    animator.transform.eulerAngles = new Vector3(0F, _y + deltaYaw, 0F);
    //}
}
