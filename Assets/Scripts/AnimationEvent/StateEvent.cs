using System;
using UnityEngine;
using Sirenix.OdinInspector;

public class StateEvent : StateMachineBehaviour
{
    [Serializable]
    private struct EventInfo
    {
        [Tooltip("Function name to trigger.")]
        [SerializeField]
        private string _function;
        [Tooltip("Parameter to be sent when function is triggered.")]
        [SerializeField, ShowInInspector]
        private IEventParameter _parameter;

        public string Function => _function;
        public IEventParameter Parameter => _parameter;
    }

    [Tooltip("These events are called when a state enter.")]
    [SerializeField, HideReferenceObjectPicker, DisableContextMenu]
    private EventInfo[] _enterEvents = new EventInfo[0];
    [Tooltip("These events are called when a state exit.")]
    [SerializeField, HideReferenceObjectPicker, DisableContextMenu]
    private EventInfo[] _exitEvents = new EventInfo[0];

    private bool _isTransitioningIn;
    private bool _isTransitioningOut;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        foreach (var eventInfo in _enterEvents)
            animator.SendMessage(eventInfo.Function, eventInfo.Parameter);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!_isTransitioningOut)
        {
            if (animator.IsInTransition(layerIndex))
            {
                if (!_isTransitioningIn)
                {
                    OnStatePreExit(animator, stateInfo, layerIndex);
                    _isTransitioningOut = true;
                }
            }
            else if (_isTransitioningIn)
            {
                _isTransitioningIn = false;
            }
        }
    }

    private void OnStatePreExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        foreach (var eventInfo in _exitEvents)
            animator.SendMessage(eventInfo.Function, eventInfo.Parameter);
    }
}