using UnityEngine;

public interface IHitReactive : IDamageable
{
    void ReactToHit(BoneType boneType, Vector3 point, Vector3 force, bool enableRagdoll);
    void ReactToHit(Collider collider, Vector3 point, Vector3 force);
    PhysiqueType PhysiqueType { get; }
}