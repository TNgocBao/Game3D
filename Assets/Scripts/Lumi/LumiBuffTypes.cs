namespace LumiAdventure
{
    public enum LumiBuffType { Attack, Speed, Defense, Invisibility }

    public readonly struct LumiActiveBuff
    {
        public readonly LumiBuffType Type;
        public readonly float RemainingSeconds;
        public readonly float Multiplier;

        public LumiActiveBuff(LumiBuffType type, float remainingSeconds, float multiplier)
        {
            Type = type;
            RemainingSeconds = remainingSeconds;
            Multiplier = multiplier;
        }
    }
}
