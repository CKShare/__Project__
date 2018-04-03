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
        private EventParameter _parameter;

        public string Function => _function;
        public EventParameter Parameter => _parameter;
    }


    private class EventScopeInfo
    {
        [Tooltip("Scope of event.")]
        [SerializeField, MinMaxSlider(0F, 1F, true)]
        private Vector2 _scope = new Vector2(0F, 1F);
        [Tooltip("Scope class reference that implements callbacks.")]
        [SerializeField, Required]
        private IEventScope _scopeReference;

        private bool _isEntered = false, _isExited = false;

        public Vector2 Scope => _scope;
        public IEventScope ScopeReference => _scopeReference;

        public bool IsEntered
        {
            get { return _isEntered; }
            set { _isEntered = value; }
        }

        public bool IsExited
        {
            get { return _isExited; }
            set { _isExited = value; }
        }

        public void Reset()
        {
            _isEntered = false;
            _isExited = false;
        }
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
        [Tooltip("Layer weight threshold to trigger event.\n Base Layer is never affected by this.")]
        [SerializeField, MinValue(0F), MaxValue(1F)]
        private float _weightThreshold = 0F;
        [Tooltip("If true, this event will be triggered before exiting a state.")]
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
    private EventScopeInfo[] _eventScopes = new EventScopeInfo[0];
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

        // EventScope
        foreach (var scopeInfo in _eventScopes)
            scopeInfo.Reset();

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
                // EventScope
                foreach (var scopeInfo in _eventScopes)
                {
                    IEventScope scopeReference = scopeInfo.ScopeReference;

                    if (!scopeInfo.IsEntered)
                    {
                        if (curTime >= scopeInfo.Scope.x)
                        {
                            scopeReference.OnScopeEnter(animator, layerIndex);
                            scopeInfo.IsEntered = true;
                        }
                    }
                    else if (!scopeInfo.IsExited)
                    {
                        if (curTime >= scopeInfo.Scope.y)
                        {
                            scopeReference.OnScopeExit(animator, layerIndex);
                            scopeInfo.IsExited = true;
                        }
                        else
                        {
                            scopeReference.OnScopeUpdate(animator, layerIndex);
                        }
                    }
                }

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
                    // Reset
                    foreach (var scopeInfo in _eventScopes)
                        scopeInfo.Reset();
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

    public override void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // EventScope
        foreach (var scopeInfo in _eventScopes)
        {
            if (scopeInfo.IsEntered && !scopeInfo.IsExited)
            {
                scopeInfo.ScopeReference.OnScopeMove(animator, layerIndex);
            }
        }
    }

    public override void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // EventScope
        foreach (var scopeInfo in _eventScopes)
        {
            if (scopeInfo.IsEntered && !scopeInfo.IsExited)
            {
                scopeInfo.ScopeReference.OnScopeIK(animator, layerIndex);
            }
        }
    }

    private void OnStatePreExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // EventScope
        foreach (var scopeInfo in _eventScopes)
        {
            if (scopeInfo.IsEntered && !scopeInfo.IsExited)
            {
                scopeInfo.ScopeReference.OnScopeExit(animator, layerIndex);
                scopeInfo.IsExited = true;
            }
        }

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
