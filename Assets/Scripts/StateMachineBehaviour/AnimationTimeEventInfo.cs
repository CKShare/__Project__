using System;
using UnityEngine;
using Sirenix.OdinInspector;

[Serializable]
public class AnimationTimeEventInfo : AnimationEventInfo
{
    [Tooltip("Normalized-time to trigger event.")]
    [SerializeField, MinValue(0F), MaxValue(1F)]
    private float _time = 0F;
    [Tooltip("Probability to trigger event.\n0 is never triggerd, and 1 is always triggered.")]
    [SerializeField, MinValue(0F), MaxValue(1F), HideIf("_triggerBeforeExiting", optionalValue: true)]
    private float _chance = 1F;
    [Tooltip("Layer weight threshold to trigger event.\nBase Layer is never affected by this.")]
    [SerializeField, MinValue(0F), MaxValue(1F)]
    private float _weightThreshold = 0F;
    [Tooltip("If true, this event will be necessarily triggered before exiting a state.")]
    [SerializeField]
    private bool _triggerBeforeExiting = false;

    private bool _isTriggered = false;

    public float Time => _time;
    public float Chance => _chance;
    public float WeightThreshold => _weightThreshold;
    public bool TriggerBeforeExiting => _triggerBeforeExiting;

    public bool IsTriggered
    {
        get { return _isTriggered; }
        set { _isTriggered = value; }
    }

    public void Reset()
    {
        _isTriggered = false;
    }
}