using System;
using LumiAdventure;

internal static class GameRulesChecks
{
    private static int checks;
    private static void Check(bool condition,string label)
    {
        checks++;
        if(!condition)throw new Exception("FAIL: "+label);
    }

    private static void Main()
    {
        int[] expected={1,1,1,2,2,3};
        for(int stars=0;stars<=5;stars++)
        {
            Check(LumiProgressionRules.Rating(true,stars,5)==expected[stars],"rating for "+stars+"/5");
            Check(LumiProgressionRules.Rating(false,stars,5)==0,"defeat awards no stars");
        }
        Check(LumiProgressionRules.Rating(true,2,4)==2,"exact 50 percent");
        Check(LumiProgressionRules.Rating(true,2,5)==1,"below 50 percent");
        Check(LumiProgressionRules.Rating(true,0,0)==1,"empty objective cannot award full stars");
        Check(LumiProgressionRules.Rating(true,-1,5)==1,"negative collection");
        Check(LumiProgressionRules.Rating(true,8,5)==3,"collection cap");
        for(int level=1;level<=5;level++)
        {
            Check(LumiProgressionRules.UnlockedAfterResult(level,level,false)==level,"defeat keeps level locked");
            Check(LumiProgressionRules.UnlockedAfterResult(level,level,true)==Math.Min(5,level+1),"win unlocks next level");
            Check(LumiProgressionRules.UnlockedAfterResult(5,level,true)==5,"replay preserves later unlocks");
        }
        Check(LumiProgressionRules.UnlockedAfterResult(0,1,false)==1,"damaged save lower bound");
        Check(LumiProgressionRules.UnlockedAfterResult(99,5,false)==5,"damaged save upper bound");
        var gate=new LumiShotGate();
        Check(gate.TryConsume(0,1),"first click fires immediately");
        for(int i=0;i<1000;i++)Check(!gate.TryConsume(i/1000.0,1),"holding/repeated calls cannot accelerate firing");
        Check(gate.TryConsume(1,1),"fires at exactly one second");
        Check(!gate.TryConsume(1,1),"same-frame repeated call denied");
        Check(!gate.TryConsume(1.999999,1),"just before cooldown denied");
        Check(gate.TryConsume(2,1),"second cooldown boundary");
        Check(!gate.TryConsume(double.NaN,1),"invalid clock rejected");
        bool invalid=false;
        try{gate.TryConsume(3,0);}catch(ArgumentOutOfRangeException){invalid=true;}
        Check(invalid,"invalid cooldown rejected");
        Console.WriteLine("PASS: "+checks+" checks for completion ratings, win/lose progression, replay and exact cooldown gating.");
    }
}
