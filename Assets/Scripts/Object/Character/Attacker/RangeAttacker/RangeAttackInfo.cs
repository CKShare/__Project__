using UnityEngine;

public abstract class RangeAttackInfo : AttackInfo
{
    [SerializeField]
    private string _projectile;
    [SerializeField, FMODUnity.EventRef]
    private string _fireSound;
    [SerializeField]
    private float _fireForce;

    public string Projectile => _projectile;
    public string FireSound => _fireSound;
    public float FireForce => _fireForce;
}