using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor.Animations;
#endif

[CreateAssetMenu]
public class StateEventCollection : SerializedScriptableObject
{
#if UNITY_EDITOR
    [SerializeField, Required]
    private AnimatorController _animatorController;
#endif
    [SerializeField, DisableContextMenu]
    private Dictionary<StateInfo, StateEventInfo> _eventDict = new Dictionary<StateInfo, StateEventInfo>(new StateInfoEqualityComparer());

    public bool TryGetEventInfo(int stateHash, out StateEventInfo eventInfo)
    {
        return _eventDict.TryGetValue(stateHash, out eventInfo);
    }


#if UNITY_EDITOR
    public AnimatorController AnimatorController => _animatorController;
#endif
}

