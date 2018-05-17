using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(TimeController))]
public class Projectile : MonoBehaviour
{
    private LayerMask _hitLayer;
    private TimeController _timeController;
    private GameObject _attacker;
    private int _fireForce;
    private RangeHitInfo _hitInfo;
    private Pool<GameObject> _pool;

    private void Awake()
    {
        _timeController = GetComponent<TimeController>();
        _hitLayer = LayerMask.GetMask("Ground", "Obstacle", "HitCollider");
    }

    private void Update()
    {
        RaycastHit rayHitInfo;
        Vector3 newPosition = transform.position + transform.forward * (_fireForce * _timeController.DeltaTime);
        if (Physics.Linecast(transform.position, newPosition, out rayHitInfo, _hitLayer))
        {
            GameObject target = rayHitInfo.transform.gameObject;

            var sceneObj = target.GetComponent<SceneObject>();
            if (sceneObj == null)
            {
                var linker = target.GetComponent<SceneObjectLinker>();
                if (linker != null)
                    sceneObj = linker.SceneObject;
            }

            if (sceneObj != null)
            {
                // Effect
                if (_hitInfo.HitEffect != null)
                {
                    TextureType textureType = sceneObj.TextureType;
                    EffectInfo effectInfo;
                    if (_hitInfo.HitEffect.TryGetEffectInfo(textureType, out effectInfo))
                    {
                        // Vfx
                        GameObject vfx = effectInfo.ParticlePool.Spawn();
                        vfx.transform.position = rayHitInfo.point;

                        // Sfx
                        FMODUnity.RuntimeManager.PlayOneShot(effectInfo.Sound, rayHitInfo.point);
                    }
                }

                // Damageable
                var damageable = sceneObj as IDamageable;
                if (damageable != null)
                {
                    damageable.ApplyDamage(_attacker, _hitInfo.Damage);

                    var hitReactive = damageable as IHitReactive;
                    if (hitReactive != null)
                    {
                        hitReactive.ReactToHit(rayHitInfo.collider, rayHitInfo.point, transform.forward * _hitInfo.Force, _hitInfo.EnableRagdoll);
                    }
                }
            }

            OnCollideWith(target, rayHitInfo.point);
            _pool.Despawn(gameObject);
            transform.position = rayHitInfo.point;
        }
        else
        {
            transform.position = newPosition;
        }
    }

    protected virtual void OnCollideWith(GameObject target, Vector3 position) { }

    public void Set(GameObject attacker, RangeHitInfo hitInfo, int fireForce, Vector3 startPosition, Vector3 direction, Pool<GameObject> pool)
    {
        _attacker = attacker;
        _pool = pool;
        _fireForce = fireForce;
        _hitInfo = hitInfo;
        transform.position = startPosition;
        transform.forward = direction;
    }
}