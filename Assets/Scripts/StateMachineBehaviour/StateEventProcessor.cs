using UnityEngine;
using Sirenix.OdinInspector;

public class StateEventProcessor : SerializedStateMachineBehaviour
{
    private static StateEventInfo DummyEventInfo = new StateEventInfo();

    private StateEventInfo _eventInfo;
    private bool _isTransitioningIn;
    private bool _isTransitioningOut;
    private float _prevNormalizedTime;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_eventInfo == null)
        {
            if (!animator.GetComponent<StateEventMediator>().TryGetEventInfo(stateInfo.fullPathHash, out _eventInfo))
            {
                _eventInfo = DummyEventInfo;
            }
        }
        
        _isTransitioningIn = animator.IsInTransition(layerIndex);
        _isTransitioningOut = false;
        _prevNormalizedTime = stateInfo.normalizedTime;
        
        // Enter
        foreach (var e in _eventInfo.EnterEvents)
            animator.SendMessage(e.Function, e.Parameter, SendMessageOptions.DontRequireReceiver);

        // EventTrigger
        foreach (var triggerInfo in _eventInfo.TimeEvents)
        {
            triggerInfo.Reset();

            if (triggerInfo.Time <= stateInfo.normalizedTime)
            {
                bool condition = triggerInfo.Chance >= UnityEngine.Random.value && (layerIndex == 0 ? true : animator.GetLayerWeight(layerIndex) >= triggerInfo.WeightThreshold);
                if (condition)
                {
                    animator.SendMessage(triggerInfo.Function, triggerInfo.Parameter, SendMessageOptions.DontRequireReceiver);
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
                foreach (var triggerInfo in _eventInfo.TimeEvents)
                {
                    if (!triggerInfo.IsTriggered && triggerInfo.Time <= curTime)
                    {
                        bool condition = triggerInfo.Chance >= UnityEngine.Random.value && (layerIndex == 0 ? true : animator.GetLayerWeight(layerIndex) >= triggerInfo.WeightThreshold);
                        if (condition)
                        {
                            animator.SendMessage(triggerInfo.Function, triggerInfo.Parameter, SendMessageOptions.DontRequireReceiver);
                        }

                        triggerInfo.IsTriggered = true;
                    }
                }

                // Loop
                int diff = Mathf.FloorToInt(stateInfo.normalizedTime) - Mathf.FloorToInt(_prevNormalizedTime);
                if (diff > 0)
                {
                    foreach (var triggerInfo in _eventInfo.TimeEvents)
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
        foreach (var triggerInfo in _eventInfo.TimeEvents)
        {
            if (triggerInfo.TriggerBeforeExiting && !triggerInfo.IsTriggered)
            {
                bool condition = layerIndex == 0 ? true : animator.GetLayerWeight(layerIndex) >= triggerInfo.WeightThreshold;
                if (condition)
                {
                    animator.SendMessage(triggerInfo.Function, triggerInfo.Parameter, SendMessageOptions.DontRequireReceiver);
                }

                triggerInfo.IsTriggered = true;
            }
        }

        // Exit
        foreach (var e in _eventInfo.ExitEvents)
            animator.SendMessage(e.Function, e.Parameter, SendMessageOptions.DontRequireReceiver);
    }
}
