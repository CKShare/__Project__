using UnityEngine;

public static class AIAnimatorInfo
{
    public static class Hash
    {
        public static int CombatState = Animator.StringToHash("CombatState");
        public static int IsWalking = Animator.StringToHash("IsWalking");
        public static int IsRunning = Animator.StringToHash("IsRunning");
        public static int Attack = Animator.StringToHash("Attack");
        public static int Hit = Animator.StringToHash("Hit");
        public static int ReactionID = Animator.StringToHash("ReactionID");
        public static int IsDead = Animator.StringToHash("IsDead");
    }
}