using UnityEngine;
using Sirenix.OdinInspector;

public class ESPlayerTurn : EventScope
{
    [SerializeField]
    private AnimationCurve _turnCurve = new AnimationCurve();
    [SerializeField, MinValue(-180F), MaxValue(180F), ValidateInput("CheckMaxAngle", "MaxAngle can't be zero.")]
    private float _maxAngle;

    private int _hash;
    private Rigidbody _rigidbody;
    private float _initialAngle;
    private float _targetAngle;
    private float _error;

    public override void OnScopeEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_hash == 0)
            _hash = Animator.StringToHash("TurnAngle");
        if (_rigidbody == null)
            _rigidbody = animator.GetComponent<Rigidbody>();
        _initialAngle = _rigidbody.rotation.eulerAngles.y;
        _targetAngle = animator.GetFloat(_hash);
        _error = _targetAngle / _maxAngle;
    }

    public override void OnScopeUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        float angle = _turnCurve.Evaluate(stateInfo.normalizedTime) * _error;
        _rigidbody.rotation = Quaternion.Euler(0F, _initialAngle + angle, 0F);
    }

    private bool CheckMaxAngle(float maxAngle)
    {
        return maxAngle != 0F;
    }
} 