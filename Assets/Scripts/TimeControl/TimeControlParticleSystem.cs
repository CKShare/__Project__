using System;
using UnityEngine;

[Serializable]
public class TimeControlParticleSystem : ITimeControl
{
    [SerializeField]
    private ParticleSystem _particleSystem;

    public void AdjustTimeScale(float ratio)
    {
        var module = _particleSystem.main;
        module.simulationSpeed *= ratio;
    }
}