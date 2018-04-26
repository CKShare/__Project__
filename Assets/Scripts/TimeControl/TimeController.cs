using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[DefaultExecutionOrder(-32000)]
public class TimeController : SerializedMonoBehaviour
{
    [ShowInInspector, ReadOnly]
    private float _timeScale = 1F;
    [SerializeField, HideReferenceObjectPicker]
    private ITimeControl[] _components = new ITimeControl[0];

    private Dictionary<object, float> _effectors = new Dictionary<object, float>();
    private bool _isChanged = false;

    private void FixedUpdate()
    {
        DeltaTime = Time.fixedDeltaTime * TimeScale;
    }

    private void Update()
    {
        DeltaTime = Time.deltaTime * TimeScale;
    }

    public void AddOrSetEffector(object key, float value)
    {
        _effectors[key] = value;
        _isChanged = true;
    }

    public void RemoveEffector(object key)
    {
        _effectors.Remove(key);
        _isChanged = true;
    }

    private void Calculate()
    {
        float timeScale = 1F;
        foreach (var effector in _effectors)
            timeScale *= effector.Value;

        TimeScale = timeScale;
    }
    
    public float TimeScale
    {
        get
        {
            if (_isChanged)
            {
                Calculate();
                _isChanged = false;
            }

            return _timeScale;
        }

        private set
        {
            float prev = _timeScale;
            _timeScale = value;

            foreach (var comp in _components)
                comp.AdjustTimeScale(_timeScale / prev);
        }
    }

    public float DeltaTime { get; private set; }
}