using System;
using UnityEngine;
using Pathfinding;

[Serializable]
public class TimeControlRichAI : MonoBehaviour, ITimeControl
{
    private RichAI _richAI;
    private float _timeScale = 1F;
    private float _maxSpeed;
    private float _rotationSpeed;
    private float _gravity;

    private void Awake()
    {
        _richAI = GetComponent<RichAI>();
        _timeScale = 1F;
        _maxSpeed = _richAI.maxSpeed;
        _rotationSpeed = _richAI.rotationSpeed;
        _gravity = _richAI.gravity.y;
    }

    public void AdjustTimeScale(float timeScale)
    {
        _timeScale = timeScale;
        _richAI.maxSpeed = _maxSpeed * timeScale;
        _richAI.rotationSpeed = _rotationSpeed * timeScale;
        _richAI.gravity.y = _gravity * timeScale;
    }

    public float MaxSpeed
    {
        get
        {
            return _maxSpeed;
        }
        
        set
        {
            _maxSpeed = value;
            _richAI.maxSpeed = value * _timeScale;
        }
    }

    public float RotationSpeed
    {
        get
        {
            return _rotationSpeed;
        }

        set
        {
            _rotationSpeed = value;
            _richAI.rotationSpeed = value * _timeScale;
        }
    }

    public float Gravity
    {
        get
        {
            return _gravity;
        }

        set
        {
            _gravity = value;
            _richAI.gravity.y = value * _timeScale;
        }
    }
}