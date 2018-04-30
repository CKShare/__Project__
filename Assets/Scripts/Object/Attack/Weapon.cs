using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Weapon : MonoBehaviour
{
    private Rigidbody _rigidbody;
    private Collider _collider;

    protected virtual void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
    }

    public void Drop()
    {
        _collider.isTrigger = false;
        _rigidbody.isKinematic = false;

        transform.SetParent(null, true);
    }
}