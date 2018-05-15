using UnityEngine;

public class TimeControlAnimator : ITimeControl
{
    [SerializeField]
    private Animator _animator;

    public void AdjustTimeScale(float ratio)
    {
        _animator.speed *= ratio;
    }
}