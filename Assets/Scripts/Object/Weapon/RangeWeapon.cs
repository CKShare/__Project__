using UnityEngine;
using Sirenix.OdinInspector;

public class RangeWeapon : Weapon
{
    [SerializeField, HideLabel]
    private PoolInfo _projectile;
    [SerializeField, Required]
    private Transform _muzzle;
    [SerializeField]
    private ParticleSystem _muzzleEffect;
    [SerializeField]
    private string _fireSound;
    [SerializeField]
    private int _fireForce;
    [SerializeField]
    private Vector2 _horizontalError, verticalError;

    public void Fire(Vector3 targetPosition)
    {
        // Vfx
        if (_muzzleEffect != null)
            _muzzleEffect.Play(true);

        // Sfx
        if (!string.IsNullOrEmpty(_fireSound))
            FMODUnity.RuntimeManager.PlayOneShot(_fireSound, _muzzle.position);

        // Projectile
        var prjObj = _projectile.Pool.Spawn();
        
    }
}