using UnityEngine;

public interface IEventScope
{
    void OnScopeEnter(Animator animator, int layerIndex);
    void OnScopeUpdate(Animator animator, int layerIndex);
    void OnScopeExit(Animator animator, int layerIndex);
    void OnScopeMove(Animator animator, int layerIndex);
    void OnScopeIK(Animator animator, int layerIndex);
}
