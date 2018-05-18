using UnityEngine;

public class ReactionPoint : MonoBehaviour
{
    [SerializeField]
    private int _reactionID;
    [SerializeField]
    private float _reactionForce;
    [SerializeField]
    private bool _enableRagdoll;
    
    private Collider _collider;

    private void Awake()
    {
        _collider = GetComponentInParent<Collider>();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.1F);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward);
    }

    public int ReactionID => _reactionID;
    public Vector3 Point => transform.position;
    public Vector3 ReactionForce => transform.forward * _reactionForce;
    public bool EnableRagdoll => _enableRagdoll;
    public Collider Collider => _collider;
}