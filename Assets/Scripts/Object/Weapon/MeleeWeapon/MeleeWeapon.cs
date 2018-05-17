using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class MeleeWeapon : Weapon
{
    [SerializeField]
    private Dictionary<int, MeleeHitInfo> _hitInfoDict = new Dictionary<int, MeleeHitInfo>();
    [SerializeField]
    private float _hitDistanceThreshold;
    [SerializeField]
    private float _hitAngleThreshold;

    public void CheckHit(GameObject target, int attackID)
    {
        Transform tr = target.transform;
        if (Physics.CheckSphere(Owner.transform.position, _hitDistanceThreshold, 1 << target.layer))
        {
            Vector3 forward = Owner.transform.forward;
            Vector3 diff = tr.position - Owner.transform.position;
            diff.y = 0F;

            float angle = Vector3.Angle(forward, diff);
            if (angle <= _hitAngleThreshold)
            {
                var sceneObj = target.GetComponent<SceneObject>();
                if (sceneObj != null)
                {
                    MeleeHitInfo hitInfo = _hitInfoDict[attackID];

                    // Effect
                    if (hitInfo.HitEffect != null)
                    {
                        TextureType textureType = sceneObj.TextureType;
                        EffectInfo effectInfo;
                        if (hitInfo.HitEffect.TryGetEffectInfo(textureType, out effectInfo))
                        {
                            // Vfx
                            GameObject vfx = effectInfo.ParticlePool.Spawn();
                            vfx.transform.position = hitInfo.HitPoint;

                            // Sfx
                            FMODUnity.RuntimeManager.PlayOneShot(effectInfo.Sound, hitInfo.HitPoint);
                        }
                    }

                    // Damageable
                    var damageable = sceneObj as IDamageable;
                    if (damageable != null)
                    {
                        damageable.ApplyDamage(Owner, hitInfo.Damage);

                        var hitReactive = damageable as IHitReactive;
                        if (hitReactive != null)
                        {
                            hitReactive.ReactToHit(hitInfo.ReactionID, hitInfo.HitPoint, hitInfo.Force, hitInfo.EnableRagdoll);
                        }
                    }
                }
            }
        }
    }
}