using System;
using UnityEngine;

[Serializable]
public class TimeControlAudioSource : ITimeControl
{
    [SerializeField]
    private AudioSource _audioSource;

    public void AdjustTimeScale(float ratio)
    {
        _audioSource.pitch *= ratio;
    }
}