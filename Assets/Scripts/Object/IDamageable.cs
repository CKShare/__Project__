using UnityEngine;

public interface IDamageable
{
    void ApplyDamage(Transform attacker, int damage, int reactionID = -1);
}
