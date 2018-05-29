using System;
using UnityEngine;

public class TimeControlAnimator : MonoBehaviour, ITimeControl
{
    private Animator _animator;
    private float _speed;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _speed = _animator.speed;
    }

    public void AdjustTimeScale(float timeScale)
    {
        _animator.speed = _speed * timeScale;
    }
}