﻿using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class MeleeWeapon : Weapon
{
    [SerializeField]
    private float _hitDistanceThreshold;
    [SerializeField]
    private float _hitAngleThreshold;
    [ShowInInspector, ReadOnly]
    private Dictionary<int, MeleeHitPoint> _hitPointDict = new Dictionary<int, MeleeHitPoint>();

    [Button]
    private void ShowPoints()
    {
        _hitPointDict.Clear();
        var points = GetComponentsInChildren<MeleeHitPoint>();
        foreach (var point in points)
            _hitPointDict[point.AttackID] = point;
    }

    private void Awake()
    {
        var points = GetComponentsInChildren<MeleeHitPoint>();
        foreach (var point in points)
            _hitPointDict[point.AttackID] = point;
    }

    public void Hit(GameObject target, int attackID)
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
                    var hitPoint = _hitPointDict[attackID];

                    // Effect
                    if (hitPoint.HitEffect != null)
                    {
                        TextureType textureType = sceneObj.TextureType;
                        EffectInfo effectInfo;
                        if (hitPoint.HitEffect.TryGetEffectInfo(textureType, out effectInfo))
                        {
                            // Vfx
                            GameObject vfx = effectInfo.ParticlePool.Spawn();
                            vfx.transform.position = hitPoint.Point;

                            // Sfx
                            FMODUnity.RuntimeManager.PlayOneShot(effectInfo.Sound, hitPoint.Point);
                        }
                    }

                    // Damageable
                    var damageable = sceneObj as IDamageable;
                    if (damageable != null)
                    {
                        damageable.ApplyDamage(Owner, hitPoint.Damage);

                        var hitReactive = damageable as IHitReactive;
                        if (hitReactive != null)
                        {
                            if (hitReactive.PhysiqueType == PhysiqueType.Light)
                            {
                                var info = hitPoint.LightReactionInfo;
                                hitReactive.ReactToHit(info.BoneType, hitPoint.Point, hitPoint.Direction * info.Force, info.EnableRagdoll);
                            }
                            else if (hitReactive.PhysiqueType == PhysiqueType.Heavy)
                            {
                                var info = hitPoint.HeavyReactionInfo;
                                hitReactive.ReactToHit(info.BoneType, hitPoint.Point, hitPoint.Direction * info.Force, info.EnableRagdoll);
                            }
                        }
                    }
                }
            }
        }
    }
}