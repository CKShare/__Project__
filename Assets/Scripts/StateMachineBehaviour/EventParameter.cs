using System;
using UnityEngine;
using Sirenix.OdinInspector;

[Serializable]
public class EventParameter
{
    [SerializeField]
    private int _intParameter;
    [SerializeField]
    private float _floatParameter;
    [SerializeField]
    private string _stringParameter;
    [SerializeField]
    private bool _boolParameter;
    [SerializeField, InlineEditor]
    private UnityEngine.Object _objectParameter;

    public int IntParameter => _intParameter;
    public float FloatParameter => _floatParameter;
    public string StringParameter => _stringParameter;
    public bool BoolParameter => _boolParameter;
    public UnityEngine.Object ObjectParameter => _objectParameter;
}
