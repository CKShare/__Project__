using System;
using UnityEngine;

public interface IEventParameter { }

[Serializable]
public abstract class EventParameter<T> : IEventParameter
{
    [SerializeField]
    private T _value;
    public T Value => _value;
}

public class IntParameter : EventParameter<int> { }
public class FloatParameter : EventParameter<float> { }
public class BoolParameter : EventParameter<bool> { }
public class StringParameter : EventParameter<string> { }
public class ObjectParameter : EventParameter<UnityEngine.Object> { }