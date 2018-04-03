using UnityEngine;

public static class PlayerAnimatorInfo
{
    public static class Hash
    {
        public static int InputVertical = Animator.StringToHash("InputVertical");
        public static int IsMoving = Animator.StringToHash("IsMoving");
        public static int IsCombating = Animator.StringToHash("IsCombating");
        public static int Attack = Animator.StringToHash("Attack");
        public static int ComboNumber = Animator.StringToHash("ComboNumber");
        public static int Random = Animator.StringToHash("Random");
    }

    public static class Value
    {
        public static int MaxCombo = 3;
    }
}