using UnityEngine;
using Sirenix.OdinInspector;

public class RangeWeapon : Weapon
{
    [SerializeField]
    private PoolInfo _projectile;
    [SerializeField]
    private RangeHitInfo _hitInfo;
    [SerializeField, Required]
    private Transform _muzzle;
    [SerializeField]
    private ParticleSystem _muzzleEffect;
    [SerializeField, FMODUnity.EventRef]
    private string _fireSound;
    [SerializeField]
    private int _fireForce;
    [SerializeField]
    private Vector2 _horizontalError, _verticalError;

    public void Trigger(Vector3 direction)
    {
        // Vfx
        if (_muzzleEffect != null)
            _muzzleEffect.Play(true);

        // Sfx
        if (!string.IsNullOrEmpty(_fireSound))
            FMODUnity.RuntimeManager.PlayOneShot(_fireSound, _muzzle.position);

        // Projectile
        var prjObj = _projectile.Pool.Spawn();
        var prj = prjObj.GetComponent<Projectile>();
        prj.Set(Owner, _hitInfo, _fireForce, _muzzle.position, direction + _muzzle.TransformDirection(new Vector2(Random.Range(_horizontalError.x, _horizontalError.y), Random.Range(_verticalError.x, _verticalError.y))), _projectile.Pool);
    }

    public Vector3 MuzzlePosition => _muzzle.position;
}