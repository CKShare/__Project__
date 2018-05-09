using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif

public class StateInfoEqualityComparer : IEqualityComparer<StateInfo>
{
    public bool Equals(StateInfo x, StateInfo y)
    {
        return x.StateHash == y.StateHash;
    }

    public int GetHashCode(StateInfo obj)
    {
        return obj.StateHash;
    }
}

[Serializable]
public struct StateInfo
{
    [SerializeField, HideInInspector]
    private int _stateHash;

    public static implicit operator StateInfo(int stateHash)
    {
        StateInfo info = new StateInfo();
        info._stateHash = stateHash;

        return info;
    }

    public int StateHash => _stateHash;

#if UNITY_EDITOR
    [SerializeField, ValueDropdown("StateNames"), OnValueChanged("OnValueChanged"), Required]
    private string _stateName;

    private List<string> StateNames
    {
        get
        {
            List<string> stateNames = new List<string>();
            AnimatorController controller = Selection.activeGameObject.GetComponent<Animator>().runtimeAnimatorController as AnimatorController;
            foreach (var layer in controller.layers)
            {
                AddStateNameRecursively(stateNames, layer.name, layer.stateMachine);
            }

            return stateNames;
        }
    }

    private void AddStateNameRecursively(List<string> names, string layerName, AnimatorStateMachine stateMachine)
    {
        foreach (var childState in stateMachine.states)
        {
            string name = string.CompareOrdinal(layerName, stateMachine.name) == 0 ? $"{layerName}.{childState.state.name}" : $"{layerName}.{stateMachine.name}.{childState.state.name}";
            names.Add(name);
        }

        foreach (var childStateMachine in stateMachine.stateMachines)
        {
            AddStateNameRecursively(names, layerName, childStateMachine.stateMachine);
        }
    }

    private void OnValueChanged(string stateName)
    {
        _stateHash = Animator.StringToHash(stateName);
    }
#endif
}