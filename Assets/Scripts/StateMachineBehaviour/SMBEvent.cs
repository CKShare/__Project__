using UnityEngine;
using Sirenix.OdinInspector;

public class SMBEvent : SerializedStateMachineBehaviour
{
    private class EventInfo
    {
        [Tooltip("Function name to trigger.")]
        [SerializeField]
        private string _function;
        [Tooltip("Parameter to be sent when function is triggered.")]
        [SerializeField]
        private IEventParameter _parameter;

        public string Function => _function;
        public IEventParameter Parameter => _parameter;
    }

    private class EventTriggerInfo
    {
        [SerializeField, HideLabel, HideReferenceObjectPicker]
        private EventInfo _triggerEvent = new EventInfo();
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

        public EventInfo TriggerEvent => _triggerEvent;
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

    [SerializeField, HideReferenceObjectPicker]
    private EventInfo[] _enterEvents = new EventInfo[0];
    [SerializeField, HideReferenceObjectPicker]
    private EventInfo[] _exitEvents = new EventInfo[0];
    [SerializeField, HideReferenceObjectPicker]
    private EventTriggerInfo[] _eventTriggers = new EventTriggerInfo[0];

    private bool _isTransitioningIn;
    private bool _isTransitioningOut;
    private float _prevNormalizedTime;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _isTransitioningIn = animator.IsInTransition(layerIndex);
        _isTransitioningOut = false;
        _prevNormalizedTime = stateInfo.normalizedTime;
        
        // Enter
        foreach (var e in _enterEvents)
            animator.SendMessage(e.Function, e.Parameter, SendMessageOptions.DontRequireReceiver);

        // EventTrigger
        foreach (var triggerInfo in _eventTriggers)
        {
            triggerInfo.Reset();

            if (triggerInfo.Time <= stateInfo.normalizedTime)
            {
                bool condition = triggerInfo.Chance >= UnityEngine.Random.value && (layerIndex == 0 ? true : animator.GetLayerWeight(layerIndex) >= triggerInfo.WeightThreshold);
                if (condition)
                {
                    var triggerEvent = triggerInfo.TriggerEvent;
                    animator.SendMessage(triggerEvent.Function, triggerEvent.Parameter, SendMessageOptions.DontRequireReceiver);
                }

                triggerInfo.IsTriggered = true;
            }
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        float prevTime = _prevNormalizedTime % 1F;
        float curTime = prevTime + (stateInfo.normalizedTime - _prevNormalizedTime);
        
        // Check whether current state is transitioning out to others.
        if (!_isTransitioningOut)
        {
            if (animator.IsInTransition(layerIndex))
            {
                if (!_isTransitioningIn)
                {
                    OnStatePreExit(animator, stateInfo, layerIndex);
                    _isTransitioningOut = true;
                }
            }
            else if (_isTransitioningIn)
            {
                _isTransitioningIn = false;
            }

            if (!_isTransitioningOut)
            {
                // EventTrigger
                foreach (var triggerInfo in _eventTriggers)
                {
                    if (!triggerInfo.IsTriggered && triggerInfo.Time <= curTime)
                    {
                        bool condition = triggerInfo.Chance >= UnityEngine.Random.value && (layerIndex == 0 ? true : animator.GetLayerWeight(layerIndex) >= triggerInfo.WeightThreshold);
                        if (condition)
                        {
                            var triggerEvent = triggerInfo.TriggerEvent;
                            animator.SendMessage(triggerEvent.Function, triggerEvent.Parameter, SendMessageOptions.DontRequireReceiver);
                        }

                        triggerInfo.IsTriggered = true;
                    }
                }

                // Loop
                int diff = Mathf.FloorToInt(stateInfo.normalizedTime) - Mathf.FloorToInt(_prevNormalizedTime);
                if (diff > 0)
                {
                    foreach (var triggerInfo in _eventTriggers)
                        triggerInfo.Reset();
                }

                _prevNormalizedTime = stateInfo.normalizedTime;
            }
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!_isTransitioningOut)
            OnStatePreExit(animator, stateInfo, layerIndex);
    }

    private void OnStatePreExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // EventTrigger
        foreach (var triggerInfo in _eventTriggers)
        {
            if (triggerInfo.TriggerBeforeExiting && !triggerInfo.IsTriggered)
            {
                bool condition = layerIndex == 0 ? true : animator.GetLayerWeight(layerIndex) >= triggerInfo.WeightThreshold;
                if (condition)
                {
                    var triggerEvent = triggerInfo.TriggerEvent;
                    animator.SendMessage(triggerEvent.Function, triggerEvent.Parameter, SendMessageOptions.DontRequireReceiver);
                }

                triggerInfo.IsTriggered = true;
            }
        }

        // Exit
        foreach (var e in _exitEvents)
            animator.SendMessage(e.Function, e.Parameter, SendMessageOptions.DontRequireReceiver);
    }
}
