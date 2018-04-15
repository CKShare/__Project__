using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Character))]
public abstract class ControllerBase : MonoBehaviour
{
    private Character _character;
    private Transform _transform;
    private Rigidbody _rigidbody;
    private Animator _animator;

    protected virtual void Awake()
    {
        _transform = transform;
        _character = GetComponent<Character>();
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        _rigidbody.useGravity = false;
    }

    protected virtual void OnEnable()
    {
        _character.OnDamaged += OnDamaged;
        _character.OnDeath += OnDeath;
    }

    protected virtual void OnDisable()
    {
        _character.OnDamaged -= OnDamaged;
        _character.OnDeath -= OnDeath;
    }

    protected virtual void OnAnimatorMove()
    {
        Vector3 velocity = _animator.deltaPosition / Time.deltaTime;
        velocity.y = Physics.gravity.y;
        _rigidbody.velocity = velocity;
        //_transform.rotation = _animator.rootRotation;
    }

    protected virtual void OnDamaged(Transform attacker, int damage, int reactionID) { }
    protected virtual void OnDeath() { }

    public Transform Transform => _transform;
    public Character Character => _character;
    public Rigidbody Rigidbody => _rigidbody;
    public Animator Animator => _animator;
}