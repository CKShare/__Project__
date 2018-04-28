using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TimeController))]
public abstract class Projectile<TProjectileInfo> : MonoBehaviour where TProjectileInfo : ProjectileInfo
{
    [SerializeField]
    private TProjectileInfo _projectileInfo;

    private Pool<GameObject> _pool;
    private TimeController _timeController;
    private Transform _attacker;
    private LayerMask _ignoreMask;
    private float _fireForce;
    private HitInfo _hitInfo;

    private void Awake()
    {
        _timeController = GetComponent<TimeController>();
        _pool = PoolManager.Instance[_projectileInfo.Pool];
    }

    private void Update()
    {
        RaycastHit hitInfo;
        Vector3 newPosition = transform.position + transform.forward * (_fireForce * _timeController.DeltaTime);
        if (Physics.Linecast(transform.position, newPosition, out hitInfo, ~_ignoreMask))
        {
            Transform target = hitInfo.transform;
            var damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
                damageable.ApplyDamage(_attacker, _hitInfo);

            OnCollideWith(target);

            // Vfx & Sfx
            var sceneObj = target.GetComponent<SceneObject>();
            if (sceneObj != null)
            {
                string textureName = sceneObj.TextureName;
                EffectPair pair;
                if (_projectileInfo.HitEffectSettings.TryGetEffectPair(textureName, out pair))
                {
                    // Vfx
                    string particlePool = pair.ParticlePools[Random.Range(0, pair.ParticlePools.Count)];
                    GameObject vfx = PoolManager.Instance[particlePool].Spawn();
                    vfx.transform.position = hitInfo.point;

                    // Sfx
                    FMODUnity.RuntimeManager.PlayOneShot(pair.Sound, hitInfo.point);
                }
            }

            _pool.Despawn(gameObject);
            transform.position = hitInfo.point;
        }
        else
        {
            transform.position = newPosition;
        }
    }

    protected virtual void OnCollideWith(Transform target) { }
    protected TProjectileInfo ProjectileInfo => _projectileInfo;

    public void Set(Transform attacker, float fireForce, Vector3 startPosition, Vector3 direction)
    {
        _attacker = attacker;
        _ignoreMask = (1 << attacker.gameObject.layer | 1 << gameObject.layer);
        _fireForce = fireForce;
        transform.position = startPosition;
        transform.forward = direction;
    }
}