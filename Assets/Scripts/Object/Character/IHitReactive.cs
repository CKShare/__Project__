using UnityEngine;

public interface IHitReactive : IDamageable
{
    void ReactToHit(Collider collider, Vector3 point, Vector3 force, bool enableRagdoll);
    PhysiqueType PhysiqueType { get; }
}