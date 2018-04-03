using System;
using UnityEngine;

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
    [SerializeField]
    private object _objectParameter;

    public int IntParameter => _intParameter;
    public float FloatParameter => _floatParameter;
    public string StringParameter => _stringParameter;
    public bool BoolParameter => _boolParameter;
    public object ObjectParameter => _objectParameter;
}
