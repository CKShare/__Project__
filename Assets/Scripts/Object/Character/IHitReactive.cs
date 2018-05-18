using UnityEngine;

public interface IHitReactive : IDamageable
{
    void ReactToHit(int reactionID);
    void ReactToHit(Collider collider, Vector3 point, Vector3 force);
}