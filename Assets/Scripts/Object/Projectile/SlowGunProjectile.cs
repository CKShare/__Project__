using UnityEngine;

public class SlowGunProjectile : Projectile<SlowGunProjectileInfo>
{
    [SerializeField]
    private GameObject _slowArea;

    protected override void OnCollideWith(Transform target)
    {
        base.OnCollideWith(target);
        
        _slowArea.GetComponent<SlowArea>().Set(ProjectileInfo.ApplyDelay, ProjectileInfo.Duration, ProjectileInfo.SlowCurve);
        _slowArea.SetActive(true);
        gameObject.SetActive(false);
    }
}