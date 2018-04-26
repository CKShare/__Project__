using UnityEngine;
using Sirenix.OdinInspector;

public class GunAttacker : RangeAttacker
{
    [SerializeField, InlineEditor, Required]
    private GunAttackInfo _attackInfo;

    public override void Attack(Transform attacker, Vector3 targetPosition)
    {
        base.AttackInfo = _attackInfo;
        base.Attack(attacker, targetPosition);

        GameObject obj = PoolManager.Instance[_attackInfo.Projectile].Spawn();
        GunProjectile pj = obj.GetComponent<GunProjectile>();
        pj.Set(_attackInfo, attacker, MuzzlePosition, targetPosition - MuzzlePosition);
    }
}