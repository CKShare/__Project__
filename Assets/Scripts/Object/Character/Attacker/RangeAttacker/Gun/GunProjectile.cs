using UnityEngine;

public class GunProjectile : Projectile
{
    public void Set(GunAttackInfo attackInfo, Transform attacker, Vector3 startPosition, Vector3 direction)
    {
        base.Set(attackInfo, attacker, startPosition, direction);
        transform.forward += transform.TransformVector(attackInfo.HorizontalError, attackInfo.VerticalError, 0F);
    }
}