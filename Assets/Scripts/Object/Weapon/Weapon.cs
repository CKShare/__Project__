using UnityEngine;
using Sirenix.OdinInspector;

public abstract class Weapon : SerializedMonoBehaviour
{
    [SerializeField, PropertyOrder(2)]
    private bool _isDroppable = true;

    public virtual void Trigger()
    {

    }

    public void Drop(Vector3 force)
    {
        if (!_isDroppable)
            return;

        // Now, this weapon can be affected by physics. 
        var rigidbody = GetComponent<Rigidbody>();
        var collider = GetComponent<Collider>();
        if (rigidbody != null && collider != null)
        {
            rigidbody.isKinematic = false;
            collider.isTrigger = false;

            rigidbody.AddForce(force, ForceMode.VelocityChange);
        }

        transform.SetParent(null, true);
        Owner = null;
    }

    public GameObject Owner { get; set; }
}