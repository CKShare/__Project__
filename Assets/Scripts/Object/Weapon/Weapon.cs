using UnityEngine;
using Sirenix.OdinInspector;

public abstract class Weapon : SerializedMonoBehaviour
{
    [SerializeField, PropertyOrder(1)]
    private bool _isDroppable = true;
    
    private void OnCollisionEnter(Collision collision)
    {
        // Drop Sfx
    }

    public void Drop()
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
        }

        transform.SetParent(null, true);
        Owner = null;
    }

    public GameObject Owner { get; set; }
}



