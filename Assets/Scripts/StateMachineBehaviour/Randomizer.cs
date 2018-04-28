using UnityEngine;
using Sirenix.OdinInspector;

public class Randomizer : StateMachineBehaviour
{
    [SerializeField]
    private string _parameter = "Random";
    [SerializeField, MinMaxSlider(int.MinValue, int.MaxValue, true)]
    private Vector2 _randomRange;

    private int _hash;

    public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
    {
        if (_hash == 0)
            _hash = Animator.StringToHash(_parameter);
        animator.SetInteger(_hash, (int)Random.Range(_randomRange.x, _randomRange.y + 1));
    }

}
