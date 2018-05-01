﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class MeleeAttacker : MonoBehaviour
{
    [Serializable]
    private struct MeleeAttackElement
    {
        [SerializeField]
        private HitInfo _hitInfo;
        [SerializeField]
        private float _hitRadius;
        [SerializeField, InlineEditor]
        private EffectSettings _hitEffectSettings;
        [SerializeField, Required]
        private Transform _hitOrigin;
        [SerializeField]
        private Cinemachine.NoiseSettings _noiseSettings;
        [SerializeField]
        private float _noiseDuration;
        [SerializeField]
        private float _timeScaleAmount;
        [SerializeField]
        private float _timeScaleDuration;

        public HitInfo HitInfo => _hitInfo;
        public float HitRadius => _hitRadius;
        public EffectSettings HitEffectSettings => _hitEffectSettings;
        public Transform HitOrigin => _hitOrigin;
        public Cinemachine.NoiseSettings NoiseSettings => _noiseSettings;
        public float NoiseDuration => _noiseDuration;
        public float TimeScaleAmount => _timeScaleAmount;
        public float TimeScaleDuration => _timeScaleDuration;
    }

    [SerializeField]
    private MeleeAttackElement[] _attacks = new MeleeAttackElement[0];

    private event Action<int, Vector3> _onHit;
    private GameObject _target;
    private int _attackID;

    public void Attack(GameObject target, int attackID)
    {
        _target = target;
        _attackID = attackID;
    }

    public int AttackCount => _attacks.Length;

    public event Action<int, Vector3> OnHit
    {
        add { _onHit += value; }
        remove { _onHit -= value; }
    }

    #region Animator Events

    private void CheckHit()
    {
        if (_target == null)
            return;

        MeleeAttackElement elem = _attacks[_attackID - 1];
        Vector3 originPos = elem.HitOrigin.position;
        if (Physics.CheckSphere(originPos, elem.HitRadius, 1 << _target.layer))
        {
            var damageable = _target.GetComponent<IDamageable>();
            if (damageable != null)
                damageable.ApplyDamage(transform, elem.HitInfo);

            if (elem.HitEffectSettings != null)
            {
                var sceneObj = _target.GetComponent<SceneObject>();
                if (sceneObj != null)
                {
                    string textureName = sceneObj.TextureName;
                    EffectPair pair;
                    if (elem.HitEffectSettings.TryGetEffectPair(textureName, out pair))
                    {
                        // Vfx
                        string particlePool = pair.ParticlePools[UnityEngine.Random.Range(0, pair.ParticlePools.Count)];
                        var vfx = PoolManager.Instance[particlePool].Spawn();
                        vfx.transform.position = originPos;

                        // Sfx
                        FMODUnity.RuntimeManager.PlayOneShot(pair.Sound, originPos);
                    }
                }
            }

            if (elem.NoiseSettings != null)
                GameUtility.ShakeCamera(elem.NoiseSettings, elem.NoiseDuration);
            GameUtility.SetTimeScale(elem.TimeScaleAmount, elem.TimeScaleDuration);
        }
    }

    #endregion
}