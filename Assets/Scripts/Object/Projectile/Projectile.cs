﻿using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(TimeController))]
public class Projectile : MonoBehaviour
{
    [SerializeField]
    private string _pool;
    [SerializeField, InlineEditor]
    private EffectSettings _hitEffectSettings;

    private Pool<GameObject> _poolRef;
    private TimeController _timeController;
    private Transform _attacker;
    private LayerMask _ignoreMask;
    private float _fireForce;
    private HitInfo _hitInfo;

    private void Awake()
    {
        _timeController = GetComponent<TimeController>();
    }

    private void Start()
    {
        _poolRef = PoolManager.Instance[_pool];
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

            if (_hitEffectSettings != null)
            {
                var sceneObj = target.GetComponent<SceneObject>();
                if (sceneObj != null)
                {
                    string textureName = sceneObj.TextureName;
                    EffectPair pair;
                    if (_hitEffectSettings.TryGetEffectPair(textureName, out pair))
                    {
                        // Vfx
                        string particlePool = pair.ParticlePools[Random.Range(0, pair.ParticlePools.Count)];
                        GameObject vfx = PoolManager.Instance[particlePool].Spawn();
                        vfx.transform.position = hitInfo.point;

                        // Sfx
                        FMODUnity.RuntimeManager.PlayOneShot(pair.Sound, hitInfo.point);
                    }
                }
            }

            OnCollideWith(target);
            _poolRef.Despawn(gameObject);
            transform.position = hitInfo.point;
        }
        else
        {
            transform.position = newPosition;
        }
    }

    protected virtual void OnCollideWith(Transform target) { }

    public void Set(Transform attacker, HitInfo hitInfo, float fireForce, Vector3 startPosition, Vector3 direction)
    {
        _attacker = attacker;
        _ignoreMask = (1 << attacker.gameObject.layer | 1 << gameObject.layer);
        _hitInfo = hitInfo;
        _fireForce = fireForce;
        transform.position = startPosition;
        transform.forward = direction;
    }
}