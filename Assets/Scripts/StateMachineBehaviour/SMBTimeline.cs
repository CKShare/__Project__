using UnityEngine;
using Sirenix.OdinInspector;

public class SMBTimeline : SerializedStateMachineBehaviour
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
        [SerializeField, MinValue(0F), MaxValue(1F), HideIf("_triggerBeforeExiting", optionalValue:true)]
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
    
    [SerializeField, HideReferenceObjectPicker, BoxGroup]
    private EventInfo _stateEnterEvent = new EventInfo(), _stateExitEvent = new EventInfo();
    [SerializeField, HideReferenceObjectPicker]
    private EventScopeInfo[] _eventScopes = new EventScopeInfo[0];
    [SerializeField, HideReferenceObjectPicker]
    private EventTriggerInfo[] _eventTriggers = new EventTriggerInfo[0];

    private bool _isTransitioningIn;
    private float _prevNormalizedTime;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _isTransitioningIn = animator.IsInTransition(layerIndex);
        _prevNormalizedTime = stateInfo.normalizedTime;

        // State Enter Event
        if (!string.IsNullOrEmpty(_stateEnterEvent.Function))
            animator.SendMessage(_stateEnterEvent.Function, _stateEnterEvent.Parameter, SendMessageOptions.DontRequireReceiver);

        // EventScope
        foreach (var scopeInfo in _eventScopes)
            scopeInfo.Reset();

        // EventTrigger
        foreach (var triggerInfo in _eventTriggers)
            triggerInfo.Reset();
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        float prevTime = _prevNormalizedTime % 1F;
        float curTime = prevTime + (stateInfo.normalizedTime - _prevNormalizedTime);

        // Check whether current state is transitioning out to others.
        bool isTransitioningOut = false;
        if (animator.IsInTransition(layerIndex))
        {
            if (!_isTransitioningIn)
            {
                isTransitioningOut = true;
            }
        }
        else if (_isTransitioningIn)
        {
            _isTransitioningIn = false;
        }

        // EventScope
        foreach (var scopeInfo in _eventScopes)
        {
            IEventScope scopeReference = scopeInfo.ScopeReference;

            if (!scopeInfo.IsEntered)
            {
                if (!isTransitioningOut && curTime >= scopeInfo.Scope.x)
                {
                    scopeReference.OnScopeEnter(animator, layerIndex);
                    scopeInfo.IsEntered = true;
                }
            }
            else if (!scopeInfo.IsExited)
            {
                if (isTransitioningOut || curTime >= scopeInfo.Scope.y)
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
        if (!isTransitioningOut)
        {
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

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // State Exit Event
        if (!string.IsNullOrEmpty(_stateExitEvent.Function))
            animator.SendMessage(_stateExitEvent.Function, _stateExitEvent.Parameter, SendMessageOptions.DontRequireReceiver);

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
}

