using UnityEngine;
using Sirenix.OdinInspector;
using Cinemachine;

[CreateAssetMenu]
public class MeleeAttackInfo : AttackInfo
{
    [SerializeField]
    private float _hitSlowScale = 1F;
    [SerializeField]
    private float _hitSlowDuration;
    [SerializeField, InlineEditor]
    private NoiseSettings _hitNoiseSettings;
    [SerializeField]
    private float _hitNoiseDuration;
    [SerializeField]
    private float _hitMaxDistance;
    [SerializeField]
    private float _hitMaxAngle;

    public float HitSlowScale => _hitSlowScale;
    public float HitSlowDuration => _hitSlowDuration;
    public NoiseSettings HitNoiseSettings => _hitNoiseSettings;
    public float HitNoiseDuration => _hitNoiseDuration;
    public float HitMaxDistance => _hitMaxDistance;
    public float HitMaxAngle => _hitMaxAngle;
}