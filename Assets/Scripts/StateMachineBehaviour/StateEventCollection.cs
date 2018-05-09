using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu]
public class StateEventCollection : SerializedScriptableObject
{
    [SerializeField]
    private Dictionary<StateInfo, StateEventInfo> _eventDict = new Dictionary<StateInfo, StateEventInfo>(new StateInfoEqualityComparer());

    public bool TryGetEventInfo(int stateHash, out StateEventInfo eventInfo)
    {
        return _eventDict.TryGetValue(stateHash, out eventInfo);
    }
}

