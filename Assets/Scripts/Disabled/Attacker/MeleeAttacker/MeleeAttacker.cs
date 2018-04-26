using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[Serializable]
public class MeleeAttacker
{
    [Serializable]
    private struct MeleeAttackElement
    {
        [SerializeField, InlineEditor, Required]
        private MeleeAttackInfo _attackInfo;
        [SerializeField, Required]
        private Transform _hitOrigin;

        public MeleeAttackInfo AttackInfo => _attackInfo;
        public Transform HitOrigin => _hitOrigin;
    }

    [SerializeField]
    private List<MeleeAttackElement> _attackList = new List<MeleeAttackElement>();
    
    public void CheckHit(Transform attacker, Transform target, int attackID)
    {
        MeleeAttackElement elem = _attackList[attackID - 1];
        MeleeAttackInfo attackInfo = elem.AttackInfo;
        
        if (target != null)
        {
            Vector3 diff = target.position - attacker.position;
            float sqrDist = diff.sqrMagnitude;
            if (sqrDist < attackInfo.HitMaxDistance * attackInfo.HitMaxDistance)
            {
                float angle = Vector3.Angle(attacker.forward, diff);
                if (angle < attackInfo.HitMaxAngle)
                {
                    target.GetComponent<IDamageable>().ApplyDamage(attacker, attackInfo.Damage, attackInfo.ReactionID);

                    // Vfx & Sfx
                    string textureName = target.GetComponent<SceneObject>().TextureName;
                    List<EffectPair> effectList = null;
                    if (attackInfo.HitEffectDict.TryGetValue(textureName, out effectList))
                    {
                        EffectPair effectInfo = effectList[UnityEngine.Random.Range(0, effectList.Count)];

                        // Vfx
                        GameObject vfx = PoolManager.Instance[effectInfo.Particle].Spawn();
                        vfx.transform.position = elem.HitOrigin.position;

                        // Sfx
                        FMODUnity.RuntimeManager.PlayOneShot(effectInfo.Sound, elem.HitOrigin.position);
                    }

                    // Noise & Slow
                    GameManager.Instance.ShakeCamera(attackInfo.HitNoiseSettings, attackInfo.HitNoiseDuration);
                    GameManager.Instance.SetTimeScale(attackInfo.HitSlowScale, attackInfo.HitSlowDuration);
                }
            }
        }
    }
}