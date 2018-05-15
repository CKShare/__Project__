using System;
using UnityEngine;

[Serializable]
public struct EffectInfo
{
    [SerializeField, FMODUnity.EventRef]
    private string _sound;
    [SerializeField]
    private PoolInfo _particle;

    public string Sound => _sound;
    public Pool<GameObject> ParticlePool => _particle.Pool;
}
