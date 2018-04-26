using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TimeController))]
public abstract class Projectile : MonoBehaviour
{
    private TimeController _timeController;
    private LayerMask _ignoreMask;
    private RangeAttackInfo _attackInfo;
    private Transform _attacker;

    private void Awake()
    {
        _timeController = GetComponent<TimeController>();
    }

    private void Update()
    {
        RaycastHit hitInfo;
        Vector3 newPosition = transform.position + transform.forward * (_attackInfo.FireForce * _timeController.DeltaTime);
        if (Physics.Linecast(transform.position, newPosition, out hitInfo, ~_ignoreMask))
        {
            Transform target = hitInfo.transform;
            var damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
                damageable.ApplyDamage(_attacker, _attackInfo.Damage, _attackInfo.ReactionID);
            
            OnCollideWith(target);

            // Vfx & Sfx
            var sceneObj = target.GetComponent<SceneObject>();
            if (sceneObj != null)
            {
                string textureName = sceneObj.TextureName;
                List<EffectPair> effectList = null;
                if (_attackInfo.HitEffectDict.TryGetValue(textureName, out effectList))
                {
                    EffectPair effectInfo = effectList[Random.Range(0, effectList.Count)];

                    // Vfx
                    GameObject vfx = PoolManager.Instance[effectInfo.Particle].Spawn();
                    vfx.transform.position = hitInfo.point;

                    // Sfx
                    FMODUnity.RuntimeManager.PlayOneShot(effectInfo.Sound, hitInfo.point);
                }
            }

            PoolManager.Instance[_attackInfo.Projectile].Despawn(gameObject);
            transform.position = hitInfo.point;
        }
        else
        {
            transform.position = newPosition;
        }
    }

    protected virtual void OnCollideWith(Transform target) { }

    protected void Set(RangeAttackInfo attackInfo, Transform attacker, Vector3 startPosition, Vector3 direction)
    {
        _attacker = attacker;
        _attackInfo = attackInfo;
        _ignoreMask = (1 << attacker.gameObject.layer | 1 << gameObject.layer);
        transform.position = startPosition;
        transform.forward = direction;
    }

    protected AttackInfo AttackInfo => _attackInfo;
}