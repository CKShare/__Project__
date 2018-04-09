using UnityEngine;
using Cinemachine;

public abstract class AttackInfo : ScriptableObject
{
    [SerializeField]
    private int _damage;
    [SerializeField]
    private int _reactionID;
    [SerializeField]
    private NoiseSettings _hitNoise;

    public int Damage => _damage;
    public int ReactionID => _reactionID;
    public NoiseSettings HitNoise => _hitNoise;
}