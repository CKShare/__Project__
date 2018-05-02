using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[Serializable]
public class StateEventInfo
{
    [SerializeField, HideReferenceObjectPicker, DisableContextMenu, ListDrawerSettings(AlwaysAddDefaultValue = true), TitleGroup("State Event")]
    private AnimationEventInfo[] _enterEvents = new AnimationEventInfo[0];
    [SerializeField, HideReferenceObjectPicker, DisableContextMenu, ListDrawerSettings(AlwaysAddDefaultValue = true), TitleGroup("State Event")]
    private AnimationEventInfo[] _exitEvents = new AnimationEventInfo[0];
    [SerializeField, HideReferenceObjectPicker, DisableContextMenu, ListDrawerSettings(AlwaysAddDefaultValue = true), TitleGroup("Time Event")]
    private AnimationTimeEventInfo[] _timeEvents = new AnimationTimeEventInfo[0];

    public IReadOnlyList<AnimationEventInfo> EnterEvents => _enterEvents;
    public IReadOnlyList<AnimationEventInfo> ExitEvents => _exitEvents;
    public IReadOnlyList<AnimationTimeEventInfo> TimeEvents => _timeEvents;
}
