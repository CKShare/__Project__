using UnityEngine;
using Cinemachine;

public class MeleeHitPoint : MonoBehaviour
{
    [SerializeField]
    private int _attackID;
    [SerializeField]
    private int _damage;
    [SerializeField]
    private ReactionInfo _lightReactionInfo;
    [SerializeField]
    private ReactionInfo _heavyReactionInfo;
    [SerializeField]
    private EffectSettings _hitEffect;
    [SerializeField]
    private NoiseSettings _hitNoise;
    [SerializeField]
    private float _noiseDuration = 0F;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, 0.1F);
        Gizmos.DrawRay(transform.position, transform.forward);
    }

    public int AttackID => _attackID;
    public Vector3 Point => transform.position;
    public Vector3 Direction => transform.forward;
    public int Damage => _damage;
    public ReactionInfo LightReactionInfo => _lightReactionInfo;
    public ReactionInfo HeavyReactionInfo => _heavyReactionInfo;
    public EffectSettings HitEffect => _hitEffect;
    public NoiseSettings HitNoise => _hitNoise;
    public float NoiseDuration => _noiseDuration;
}