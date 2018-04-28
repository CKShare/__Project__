using UnityEngine;

public interface IDamageable
{
    void ApplyDamage(Transform attacker, HitInfo hitInfo);
}
