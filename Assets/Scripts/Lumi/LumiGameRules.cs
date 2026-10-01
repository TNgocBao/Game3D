using System;

namespace LumiAdventure
{
    public static class LumiProgressionRules
    {
        public static int Rating(bool completed,int collected,int required)
        {
            if(!completed)return 0;
            if(required<=0)return 1;
            collected=Math.Max(0,Math.Min(required,collected));
            if(collected==required)return 3;
            return collected >= (required+1)/2 ? 2 : 1;
        }

        public static int UnlockedAfterResult(int unlocked,int level,bool completed)
        {
            unlocked=Math.Max(1,Math.Min(5,unlocked));
            if(!completed)return unlocked;
            if(level<1 || level>5)throw new ArgumentOutOfRangeException(nameof(level));
            return Math.Max(unlocked,Math.Min(5,level+1));
        }
    }

    public sealed class LumiShotGate
    {
        private double readyAt=double.NegativeInfinity;
        public bool TryConsume(double now,double cooldown)
        {
            if(double.IsNaN(now) || double.IsInfinity(now))return false;
            if(double.IsNaN(cooldown) || double.IsInfinity(cooldown) || cooldown<=0)throw new ArgumentOutOfRangeException(nameof(cooldown));
            if(now<readyAt)return false;
            readyAt=now+cooldown;
            return true;
        }
    }
}
