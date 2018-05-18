using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(TimeController))]
public class Projectile : MonoBehaviour
{
    private LayerMask _hitLayer;
    private TimeController _timeController;
    private RangeWeapon _weapon;

    private void Awake()
    {
        _timeController = GetComponent<TimeController>();
        _hitLayer = LayerMask.GetMask("Ground", "Obstacle", "HitCollider");
    }

    private void Update()
    {
        RaycastHit rayHitInfo;
        Vector3 newPosition = transform.position + transform.forward * (_weapon.FireForce * _timeController.DeltaTime);
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
                if (_weapon.HitEffect != null)
                {
                    TextureType textureType = sceneObj.TextureType;
                    EffectInfo effectInfo;
                    if (_weapon.HitEffect.TryGetEffectInfo(textureType, out effectInfo))
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
                    damageable.ApplyDamage(_weapon.Owner, _weapon.Damage);

                    var hitReactive = damageable as IHitReactive;
                    if (hitReactive != null)
                    {
                        hitReactive.ReactToHit(rayHitInfo.collider, rayHitInfo.point, transform.forward * _weapon.ReactionForce);
                    }
                }
            }

            OnCollideWith(target, rayHitInfo.point);
            _weapon.ProjectilePool.Despawn(gameObject);
            transform.position = rayHitInfo.point;
        }
        else
        {
            transform.position = newPosition;
        }
    }

    protected virtual void OnCollideWith(GameObject target, Vector3 position) { }

    public void Set(RangeWeapon weapon)
    {
        _weapon = weapon;
    }
}