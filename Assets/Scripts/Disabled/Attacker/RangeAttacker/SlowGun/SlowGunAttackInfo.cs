using UnityEngine;

[CreateAssetMenu]
public class SlowGunAttackInfo : RangeAttackInfo
{
    [SerializeField]
    private float _applyDelay;
    [SerializeField]
    private float _slowDuration;
    [SerializeField]
    private AnimationCurve _slowCurve;

    public float ApplyDelay => _applyDelay;
    public float SlowDuration => _slowDuration;
    public AnimationCurve SlowCurve => _slowCurve;
}