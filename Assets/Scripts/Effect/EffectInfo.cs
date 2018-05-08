using UnityEngine;

public struct EffectInfo
{
    [SerializeField]
    private string _sound;
    [SerializeField]
    private string _particle;

    public string Sound => _sound;
    public string Particle => _particle;
}
