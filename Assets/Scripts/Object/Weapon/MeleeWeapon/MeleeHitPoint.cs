using UnityEngine;

public class MeleeHitPoint : MonoBehaviour
{
    [SerializeField]
    private int _attackID;
    [SerializeField]
    private int _damage;
    [SerializeField]
    private int _reactionID;
    [SerializeField]
    private EffectSettings _hitEffect;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, 0.1F);
    }

    public int AttackID => _attackID;
    public Vector3 Point => transform.position;
    public int Damage => _damage;
    public int ReactionID => _reactionID;
    public EffectSettings HitEffect => _hitEffect;
}