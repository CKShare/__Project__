using UnityEngine;
using Sirenix.OdinInspector;

public class IntRandomizer : StateMachineBehaviour
{
    [SerializeField, Required]
    private string _parameter = "Random";
    [SerializeField, OnValueChanged("OnMinValueChanged")]
    private int _minValue;
    [SerializeField, OnValueChanged("OnMaxValueChanged")]
    private int _maxValue;

    private int _hash;

    public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
    {
        if (_hash == 0)
            _hash = Animator.StringToHash(_parameter);
        animator.SetInteger(_hash, (int)Random.Range(_minValue, _maxValue + 1));
    }

    private void OnMinValueChanged(int minValue)
    {
        if (_minValue > _maxValue)
            _minValue = _maxValue;
    }

    private void OnMaxValueChanged(int maxValue)
    {
        if (_maxValue < _minValue)
            _maxValue = _minValue;
    }
}
