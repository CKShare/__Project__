using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(TimeController))]
public class Projectile : MonoBehaviour
{
    [SerializeField, InlineEditor]
    private EffectSettings _hitEffectSettings;
    
    private TimeController _timeController;
    private Transform _attacker;
    private Vector3 _force;
    private HitInfo _hitInfo;
    private Pool<GameObject> _pool;

    private void Awake()
    {
        _timeController = GetComponent<TimeController>();
    }

    private void Update()
    {
        RaycastHit hitInfo;
        Vector3 newPosition = transform.position + _force * _timeController.DeltaTime;
        if (Physics.Linecast(transform.position, newPosition, out hitInfo, ~(1 << _attacker.gameObject.layer)))
        {
            Transform target = hitInfo.transform;
            var damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
                damageable.ApplyDamage(_attacker, _hitInfo.Damage, _hitInfo.ReactionID);

            if (_hitEffectSettings != null)
            {
                var sceneObj = target.GetComponent<SceneObject>();
                if (sceneObj != null)
                {
                    TextureType textureType = sceneObj.TextureType;
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

            OnCollideWith(target, hitInfo.point);
            _pool.Despawn(gameObject);
            transform.position = hitInfo.point;
        }
        else
        {
            transform.position = newPosition;
        }
    }

    protected virtual void OnCollideWith(Transform target, Vector3 position) { }

    public void Set(Transform attacker, HitInfo hitInfo, Vector3 startPosition, Vector3 force, Pool<GameObject> pool)
    {
        _attacker = attacker;
        _pool = pool;
        _hitInfo = hitInfo;
        transform.position = startPosition;
        transform.forward = force.normalized;
    }
}