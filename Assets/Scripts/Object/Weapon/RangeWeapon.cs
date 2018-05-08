using UnityEngine;
using Sirenix.OdinInspector;

public class RangeWeapon : Weapon
{
    [SerializeField, Required]
    private string _projectile;
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

    private Pool<GameObject> _prjPool;

    protected override void Awake()
    {
        base.Awake();
        
        _prjPool = PoolManager.Instance[_projectile];
    }

    public void Fire(Vector3 targetPosition)
    {
        // Vfx
        if (_muzzleEffect != null)
            _muzzleEffect.Play(true);

        // Sfx
        if (!string.IsNullOrEmpty(_fireSound))
            FMODUnity.RuntimeManager.PlayOneShot(_fireSound, _muzzle.position);

        // Projectile
        var prjObj = _prjPool.Spawn();
        
    }
}