using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct EffectPair
{
    [SerializeField, FMODUnity.EventRef]
    private string _sound;
    [SerializeField]
    private string[] _particlePools;

    public string Sound => _sound;
    public IReadOnlyList<string> ParticlePools => _particlePools;
}