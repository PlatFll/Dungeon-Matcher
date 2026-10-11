using System;
using UnityEngine;

public enum FrostStage { None=0,Frost=1,HeavyIce=2,Bifrost=3 }

[CreateAssetMenu(menuName="Dungeon Matcher/Zones/Frostwake Rules")]
public sealed class FrostwakeRules : ScriptableObject
{
    [Min(1)] public int movesPerStage=3;
    [Min(1)] public int seedCadence=3;
    [Range(1,6)] public int maximumEncasedGems=4;
    [Min(0)] public int bifrostRetaliation=10;
    [Min(0)] public int frostCleaveDamage=20,heavyCleaveDamage=35,bifrostCleaveDamage=50;
    public PlayerStatusDefinition frostbite;
    public Sprite frostOverlay,heavyIceOverlay,bifrostOverlay;
    public int CleaveDamage(FrostStage stage) => stage==FrostStage.Bifrost?bifrostCleaveDamage:
        stage==FrostStage.HeavyIce?heavyCleaveDamage:stage==FrostStage.Frost?frostCleaveDamage:0;
}

[Serializable]
public sealed class FrostGemSnapshot
{
    public int identity;
    public FrostStage stage;
    public int ignoredMoves,lastAdvancedMove,lastHitMove=-1;
    public bool Advance(int move,int cadence)
    {
        if(move<=lastAdvancedMove)return false;
        lastAdvancedMove=move;
        if(lastHitMove>=move || stage>=FrostStage.Bifrost)return false;
        if(++ignoredMoves<Mathf.Max(1,cadence))return false;
        stage++;ignoredMoves=0;return true;
    }
    public void Hit(int move) {stage=(FrostStage)Mathf.Max(0,(int)stage-1);ignoredMoves=0;lastHitMove=move;}
    public FrostGemSnapshot Copy() => (FrostGemSnapshot)MemberwiseClone();
    public static bool Supports(BoardCombatSnapshot saved)
    {
        if(saved==null || saved.frost==null || saved.frost.Count==0)return true;
        if(saved.cells==null || saved.nextFrostSeedMove<0)return false;
        var seen=new System.Collections.Generic.HashSet<int>();
        foreach(var ice in saved.frost)
            if(ice==null || ice.identity<=0 || !seen.Add(ice.identity) ||
                ice.stage<FrostStage.Frost || ice.stage>FrostStage.Bifrost ||
                ice.ignoredMoves<0 || ice.lastAdvancedMove<0 || ice.lastHitMove < -1 ||
                !saved.cells.Exists(c=>c!=null && c.hasGem && c.identity==ice.identity))return false;
        return true;
    }
}

[Serializable]
public sealed class GlacierCleaveSnapshot
{
    public int targetIdentity,deadlineMove,sequence,lastOutcomeSequence;
    public bool channeling;
    public string outcome;
    public static bool Supports(GlacierCleaveSnapshot saved) => saved==null ||
        (saved.sequence>=0 && saved.lastOutcomeSequence>=0 && saved.lastOutcomeSequence<=saved.sequence &&
         saved.deadlineMove>=0 && (!saved.channeling ||
         (saved.targetIdentity>0 && saved.deadlineMove>0 && saved.sequence>saved.lastOutcomeSequence)));
}
