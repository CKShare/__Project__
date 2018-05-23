using System;
using UnityEngine;

[Serializable]
public class TimeControlAudioSource : ITimeControl
{
    [SerializeField]
    private AudioSource _audioSource;
    
    private float _pitch;

    public void Initialize()
    {
        _pitch = _audioSource.pitch;
    }

    public void AdjustTimeScale(float timeScale)
    {
        _audioSource.pitch = _pitch * timeScale;
    }
}