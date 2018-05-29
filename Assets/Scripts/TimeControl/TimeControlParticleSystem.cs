using System;
using UnityEngine;

[Serializable]
public class TimeControlParticleSystem : MonoBehaviour, ITimeControl
{
    private ParticleSystem _particleSystem;
    private float _simulationSpeed;

    private void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();
        _simulationSpeed = _particleSystem.main.simulationSpeed;
    }

    public void AdjustTimeScale(float timeScale)
    {
        var module = _particleSystem.main;
        module.simulationSpeed = _simulationSpeed * timeScale;
    }
}