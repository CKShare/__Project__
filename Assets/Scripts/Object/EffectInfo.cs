using System;
using UnityEngine;

[Serializable]
public class EffectInfo
{
    [SerializeField]
    private ParticleSystem _particle;
    [SerializeField]
    private AudioClip _sound;

    public ParticleSystem Particle => _particle;
    public AudioClip Sound => _sound;
}