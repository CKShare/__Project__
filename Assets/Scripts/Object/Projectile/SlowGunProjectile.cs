using UnityEngine;
using Sirenix.OdinInspector;

public class SlowGunProjectile : Projectile
{
    [SerializeField, Required]
    private string _slowAreaPool;

    private Pool<GameObject> _slowAreaPoolRef;

    protected override void Start()
    {
        base.Start();

        _slowAreaPoolRef = PoolManager.Instance[_slowAreaPool];
    }

    protected override void OnCollideWith(Transform target, Vector3 position)
    {
        base.OnCollideWith(target, position);

        GameObject slowArea = _slowAreaPoolRef.Spawn(); 
        slowArea.transform.position = position;
        gameObject.SetActive(false);
    }
}