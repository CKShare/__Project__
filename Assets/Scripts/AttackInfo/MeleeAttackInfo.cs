using UnityEngine;

[CreateAssetMenu]
public class MeleeAttackInfo : AttackInfo
{
    [SerializeField]
    private float _detectMaxDistance;
    [SerializeField]
    private float _detectMaxAngle;

    public float DetectMaxDistance => _detectMaxDistance;
    public float DetectMaxAngle => _detectMaxAngle;
}