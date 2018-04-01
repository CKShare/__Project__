using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public abstract class ControllerBase : MonoBehaviour
{
    private Transform _transform;
    private Rigidbody _rigidbody;
    private Animator _animator;

    protected virtual void Awake()
    {
        _transform = transform;
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        _rigidbody.useGravity = false;
    }

    protected virtual void OnAnimatorMove()
    {
        Vector3 velocity = _animator.deltaPosition / Time.deltaTime;
        velocity.y = Physics.gravity.y;
        _rigidbody.velocity = velocity;
    }

    protected Transform Transform => _transform;
    protected Rigidbody Rigidbody => _rigidbody;
    protected Animator Animator => _animator;
}