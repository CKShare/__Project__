using System;
using UnityEngine;

public abstract class HitPoint : MonoBehaviour
{
    private GameObject _attacker;
    private CastHitInfo _hitInfo;
    private event Action<RaycastHit> _onHit;

    protected void NotifyHit(Vector3 rayDirection, RaycastHit rayHitInfo)
    {
        var rootLinker = rayHitInfo.transform.GetComponent<RootLinker>();
        var root = rootLinker == null ? rayHitInfo.transform.gameObject : rootLinker.Root;
        
        var sceneObj = root.GetComponent<SceneObject>();
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

                // HitReactive
                IHitReactive hitReactive = damageable as IHitReactive;
                if (hitReactive != null)
                {
                    ForceInfo forceInfo;
                    if (_hitInfo.TryGetForceInfo(hitReactive.PhysiqueType, out forceInfo))
                    {
                        hitReactive.ReactToHit(rayHitInfo.collider, rayHitInfo.point, rayDirection * forceInfo.Force, forceInfo.EnableRagdoll);
                    }
                }
            }
        }
        _onHit?.Invoke(rayHitInfo);
    }

    protected int IgnoreLayer => 1 << _attacker.layer;

    public void Set(GameObject attacker, CastHitInfo hitInfo)
    {
        _attacker = attacker;
        _hitInfo = hitInfo;
    }

    public event Action<RaycastHit> OnHit
    {
        add { _onHit += value; }
        remove { _onHit -= value; }
    }
}