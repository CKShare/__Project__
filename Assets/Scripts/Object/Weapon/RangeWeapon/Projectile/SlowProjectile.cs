using UnityEngine;
using Sirenix.OdinInspector;

public class SlowProjectile : Projectile
{
    [SerializeField]
    private PoolInfo _slowArea;

    protected override void OnCollideWith(GameObject target, Vector3 position)
    {
        base.OnCollideWith(target, position);

        var area = _slowArea.Pool.Spawn();
        area.transform.position = position;
    }
}