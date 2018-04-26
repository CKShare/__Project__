using UnityEngine;
using Sirenix.OdinInspector;

public class SlowGunAttacker : RangeAttacker
{
    [SerializeField, InlineEditor, Required]
    private SlowGunAttackInfo _attackInfo;

    public override void Attack(Transform attacker, Vector3 targetPosition)
    {
        base.AttackInfo = _attackInfo;
        base.Attack(attacker, targetPosition);

        GameObject obj = PoolManager.Instance[_attackInfo.Projectile].Spawn();
        SlowGunProjectile pj = obj.GetComponent<SlowGunProjectile>();
        pj.Set(_attackInfo, attacker, MuzzlePosition, targetPosition - MuzzlePosition);
    }
}
