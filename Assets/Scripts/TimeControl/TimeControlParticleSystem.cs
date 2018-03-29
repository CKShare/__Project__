using UnityEngine;

public class TimeControlParticleSystem : ITimeControl
{
    [SerializeField]
    private ParticleSystem _particleSystem;
    private float _simulationSpeed;

    public void Initialize()
    {
        _simulationSpeed = _particleSystem.main.simulationSpeed;
    }

    public void AdjustTimeScale(float timeScale)
    {
        var module = _particleSystem.main;
        module.simulationSpeed = _simulationSpeed * timeScale;
    }
}