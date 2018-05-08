using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public abstract class Weapon : SerializedMonoBehaviour
{
    [SerializeField]
    private bool _isDroppable = true;
    
    private Rigidbody _rigidbody;
    private Collider _collider;

    protected virtual void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Drop Sfx
    }

    public void Drop()
    {
        if (!_isDroppable)
            return;

        _rigidbody.isKinematic = false;
        _collider.isTrigger = false;
        transform.SetParent(null, true);
    }

    public GameObject Owner { get; set; }
}



