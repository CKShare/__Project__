using UnityEngine;

public class SlowGunProjectile : Projectile
{
    protected override void OnCollideWith(Transform target)
    {
        base.OnCollideWith(target);

        SlowGunAttackInfo attackInfo = AttackInfo as SlowGunAttackInfo;
        int siblingIdx = transform.GetSiblingIndex();
        GameObject sibling = transform.GetChild(siblingIdx).gameObject;
        sibling.GetComponent<SlowArea>().Set(attackInfo.ApplyDelay, attackInfo.SlowDuration, attackInfo.SlowCurve);
        sibling.SetActive(true);
        gameObject.SetActive(false);
    }

    public void Set(SlowGunAttackInfo attackInfo, Transform attacker, Vector3 startPosition, Vector3 direction)
    {
        base.Set(attackInfo, attacker, startPosition, direction);
    }
}