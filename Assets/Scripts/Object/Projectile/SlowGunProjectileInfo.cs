using UnityEngine;

[CreateAssetMenu]
public class SlowGunProjectileInfo : ProjectileInfo
{
    [SerializeField]
    private AnimationCurve _slowCurve;
    [SerializeField]
    private float _duration;
    [SerializeField]
    private float _applyDelay;

    public AnimationCurve SlowCurve => _slowCurve;
    public float Duration => _duration;
    public float ApplyDelay => _applyDelay;
}