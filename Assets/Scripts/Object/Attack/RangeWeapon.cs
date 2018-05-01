using UnityEngine;
using Sirenix.OdinInspector;

public class RangeWeapon : Weapon
{
    [SerializeField, Required]
    private Transform _muzzle;
    [SerializeField]
    private ParticleSystem _muzzleEffect;
    [SerializeField]
    private HitInfo _hitInfo;
    [SerializeField]
    private string _projectilePool;
    [SerializeField, FMODUnity.EventRef]
    private string _fireSound;
    [SerializeField]
    private float _fireForce;
    [SerializeField, MinMaxSlider(-10F, 10F, true)]
    private Vector2 _horizontalError, _verticalError;

    private Pool<GameObject> _pool;

    protected override void Awake()
    {
        base.Awake();

        _pool = PoolManager.Instance[_projectilePool];
    }

    public void Attack(Transform attacker, Vector3 targetPosition)
    {
        // Vfx
        if (_muzzleEffect != null)
            _muzzleEffect.Play(true);

        // Sfx
        if (!string.IsNullOrEmpty(_fireSound))
            FMODUnity.RuntimeManager.PlayOneShot(_fireSound, _muzzle.position);

        // Projectile
        var obj = _pool.Spawn();
        var pj = obj.GetComponent<Projectile>();
        pj.Set(attacker, _hitInfo, _fireForce, _muzzle.position, (targetPosition - _muzzle.position) + _muzzle.TransformDirection(new Vector2(Random.Range(_horizontalError.x, _horizontalError.y), Random.Range(_verticalError.x, _verticalError.y))));
    }
}