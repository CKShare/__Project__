using System;
using UnityEngine;

[Serializable]
public class TimeControlAudioSource : MonoBehaviour, ITimeControl
{
    private AudioSource _audioSource;
    private float _pitch;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _pitch = _audioSource.pitch;
    }

    public void AdjustTimeScale(float timeScale)
    {
        _audioSource.pitch = _pitch * timeScale;
    }
}