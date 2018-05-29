using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

[DefaultExecutionOrder(-32000)]
public class TimeController : SerializedMonoBehaviour
{
    [ShowInInspector, ReadOnly]
    private float _timeScale = 1F;

    private ITimeControl[] _components = new ITimeControl[0];
    private Dictionary<Type, ITimeControl> _compDict = new Dictionary<Type, ITimeControl>();

    private void Awake()
    {
        _components = GetComponentsInChildren<ITimeControl>();
        foreach (var comp in _components)
        {
            _compDict[comp.GetType()] = comp;
        }
    }

    public T GetTimeControlComponent<T>() where T : class, ITimeControl
    {
        ITimeControl comp;
        return _compDict.TryGetValue(typeof(T), out comp) ? (T)comp: null;
    }
    
    public float TimeScale
    {
        get
        {
            return _timeScale;
        }

        set
        {
            _timeScale = value;
            foreach (var comp in _components)
                comp.AdjustTimeScale(value);
        }
    }
    
    public float DeltaTime => Time.deltaTime * TimeScale;
}