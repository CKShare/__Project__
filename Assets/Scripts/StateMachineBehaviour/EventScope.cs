using System;
using UnityEngine;

[Serializable]
public abstract class EventScope
{
    public virtual void OnScopeEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }
    public virtual void OnScopeUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }
    public virtual void OnScopeExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }
    public virtual void OnScopeMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }
    public virtual void OnScopeIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }
}