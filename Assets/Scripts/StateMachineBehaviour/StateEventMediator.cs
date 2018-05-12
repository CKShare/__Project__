﻿using UnityEngine;
using Sirenix.OdinInspector;

public class StateEventMediator : MonoBehaviour
{
    [SerializeField, InlineEditor, Required]
    private StateEventCollection _collection;

    public bool TryGetEventInfo(int stateHash, out StateEventInfo eventInfo)
    {
        return _collection.TryGetEventInfo(stateHash, out eventInfo);
    }
}