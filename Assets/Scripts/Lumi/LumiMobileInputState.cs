using UnityEngine;

namespace LumiAdventure
{
    /// <summary>Transient input state written by mobile UI controls and consumed by gameplay.</summary>
    public static class LumiMobileInput
    {
        public static Vector2 Move;
        public static Vector2 Look;
        public static Vector2 AimScreenPoint;
        public static bool HasAim;
        public static bool PointerAimEnabled;
        private static bool jump;
        private static bool interact;

        public static void QueueJump() => jump = true;
        public static void QueueInteract() => interact = true;
        public static bool ConsumeJump() { bool value = jump; jump = false; return value; }
        public static bool ConsumeInteract() { bool value = interact; interact = false; return value; }

        public static void Reset()
        {
            Move = Vector2.zero;
            Look = Vector2.zero;
            AimScreenPoint = Vector2.zero;
            HasAim = false;
            PointerAimEnabled = false;
            jump = interact = false;
        }
    }
}
