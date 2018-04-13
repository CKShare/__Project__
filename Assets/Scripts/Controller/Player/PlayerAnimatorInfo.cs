using UnityEngine;

public static class PlayerAnimatorInfo
{
    public static class Hash
    {
        public static int IsMoving = Animator.StringToHash("IsMoving");
        public static int Accel = Animator.StringToHash("Accel");
        public static int TurnAngle = Animator.StringToHash("TurnAngle");
        public static int IsAiming = Animator.StringToHash("IsAiming");
        public static int Yaw = Animator.StringToHash("Yaw");
        public static int Pitch = Animator.StringToHash("Pitch");
        public static int Attack = Animator.StringToHash("Attack");
    }
}