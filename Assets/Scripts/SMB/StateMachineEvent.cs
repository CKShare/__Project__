using System;
using UnityEngine;
using Sirenix.OdinInspector;

public class StateMachineEvent : SerializedStateMachineBehaviour
{
    [Tooltip("These events are called when a state-machien enter.")]
    [SerializeField, HideReferenceObjectPicker, DisableContextMenu]
    private SMBEventInfo[] _enterEvents = new SMBEventInfo[0];
    [Tooltip("These events are called when a state-machine exit.")]
    [SerializeField, HideReferenceObjectPicker, DisableContextMenu]
    private SMBEventInfo[] _exitEvents = new SMBEventInfo[0];

    public override void OnStateMachineEnter(Animator animator, int hash)
    {
        foreach (var eventInfo in _enterEvents)
            animator.SendMessage(eventInfo.Function, eventInfo.Parameter);
    }

    public override void OnStateMachineExit(Animator animator, int hash)
    {
        foreach (var eventInfo in _exitEvents)
            animator.SendMessage(eventInfo.Function, eventInfo.Parameter);
    }
}