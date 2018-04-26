using System;
using UnityEngine;

[Serializable]
public struct EffectPair
{
    [SerializeField]
    private string _particle;
    [SerializeField, FMODUnity.EventRef]
    private string _sound;

    public string Particle => _particle;
    public string Sound => _sound;
}