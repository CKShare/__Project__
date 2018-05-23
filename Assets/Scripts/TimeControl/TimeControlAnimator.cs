using System;
using UnityEngine;

[Serializable]
public class TimeControlAnimator : ITimeControl
{
    [SerializeField]
    private Animator _animator;
    
    private float _speed;

    public void Initialize()
    {
        _speed = _animator.speed;
    }

    public void AdjustTimeScale(float timeScale)
    {
        _animator.speed = _speed * timeScale;
    }
}