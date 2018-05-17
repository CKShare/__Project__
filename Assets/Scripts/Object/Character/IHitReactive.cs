using UnityEngine;

public interface IHitReactive : IDamageable
{
    void ReactToHit(int reactionID, Vector3 point, Vector3 force, bool enableRagdoll);
    void ReactToHit(Collider collider, Vector3 point, Vector3 force, bool enableRagdoll);
}