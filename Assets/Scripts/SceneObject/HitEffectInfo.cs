using UnityEngine;

public class HitEffectInfo
{
    [SerializeField]
    private GameObject _particle;
    [SerializeField]
    private AudioClip _audioClip;

    public GameObject Particle => _particle;
    public AudioClip AudioClip => _audioClip;
}