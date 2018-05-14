using System;
using UnityEngine;
using Sirenix.OdinInspector;

public class StateEvent : SerializedStateMachineBehaviour
{
    [Tooltip("These events are called when a state enter.")]
    [SerializeField, HideReferenceObjectPicker, DisableContextMenu]
    private SMBEventInfo[] _enterEvents = new SMBEventInfo[0];
    [Tooltip("These events are called when a state pre-exit.")]
    [SerializeField, HideReferenceObjectPicker, DisableContextMenu]
    private SMBEventInfo[] _preExitEvents = new SMBEventInfo[0];
    [Tooltip("These events are called when a state exit.")]
    [SerializeField, HideReferenceObjectPicker, DisableContextMenu]
    private SMBEventInfo[] _exitEvents = new SMBEventInfo[0];

    private bool _isTransitioningIn;
    private bool _isTransitioningOut;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _isTransitioningIn = animator.IsInTransition(layerIndex);
        _isTransitioningOut = false;

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
        foreach (var eventInfo in _preExitEvents)
            animator.SendMessage(eventInfo.Function, eventInfo.Parameter);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!_isTransitioningOut)
            OnStatePreExit(animator, stateInfo, layerIndex);

        foreach (var eventInfo in _exitEvents)
            animator.SendMessage(eventInfo.Function, eventInfo.Parameter);
    }
}