using UnityEngine;
using Sirenix.OdinInspector;

public abstract class Weapon : SerializedMonoBehaviour
{
    [SerializeField, PropertyOrder(2)]
    private bool _isDroppable = true;

    public void Drop()
    {
        if (!_isDroppable)
            return;

        // Now, this weapon can be affected by physics.
        var collider = GetComponent<Collider>();
        var rigid = GetComponent<Rigidbody>();
        if (rigid != null && collider != null)
        {
            collider.isTrigger = false;
            rigid.isKinematic = false;
        }

        transform.SetParent(null, true);
        Owner = null;
    }

    public GameObject Owner { get; set; }
}