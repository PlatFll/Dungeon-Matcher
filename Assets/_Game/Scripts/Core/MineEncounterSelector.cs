using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public sealed class MineEncounterProgress
{
    public List<string> seen = new List<string>();
    public List<string> recent = new List<string>();
    public int lastMilestoneLocalWave, lastRecordedGlobalWave;
}

/// <summary>Selects data-authored mine formations; spawning and global scaling stay in WaveController.</summary>
public static class MineEncounterSelector
{
    public static bool FitsBudget(ZoneDefinition zone, ZoneTestEncounter entry, int localWave)
    {
        var members=entry.members;
        return zone.encounterBudget!=null && members!=null && members.Length>0 && members.Length<=3 &&
            members.All(e=>e!=null && e.EnemyPrefab!=null && e.CombatRole!="Summon") &&
            members.Sum(e=>e.ThreatCost)<=zone.encounterBudget.ThreatBudget(localWave) &&
            members.Count(e=>e.IsBoardDisruptor)<=2 && members.Count(e=>e.IsSupport)<=1 &&
            members.Count(e=>e.Category==EnemyCategory.Miniboss || e.Category==EnemyCategory.Boss)<=1;
    }

    public static ZoneTestEncounter Select(ZoneDefinition zone, MineEncounterProgress progress, int local, Random random)
    {
        progress.seen ??= new List<string>();progress.recent ??= new List<string>();
        bool Seen(EnemyDefinition e)=>e==null || progress.seen.Contains(e.EnemyId);
        bool Leader(EnemyDefinition e)=>e.Category==EnemyCategory.Miniboss || e.Category==EnemyCategory.Boss;
        var choices=zone.liveEncounters.Where(e=>local>=e.firstLocalWave && local<=e.lastLocalWave && FitsBudget(zone,e,local) &&
            (e.requiredSeen==null || e.requiredSeen.All(Seen)) &&
            (!e.oncePerVisit || !e.members.Any(m=>Leader(m)&&Seen(m))) &&
            e.members.All(m=>m.Category==EnemyCategory.Normal || Seen(m) || m==e.introduction)).ToList();
        if(choices.Count==0)throw new InvalidOperationException("Ironvein has no legal encounter for local wave "+local);

        var lesson=choices.Where(e=>e.introduction!=null && !Seen(e.introduction) && e.introduceByLocalWave>0)
            .OrderBy(e=>e.introduceByLocalWave).FirstOrDefault();
        // Real milestones get a normal-only next encounter. Deadlines remain safety
        // bounds for an unseen lesson, never an ordinary exact-wave script.
        bool relief=progress.lastMilestoneLocalWave==local-1;
        var normals=choices.Where(e=>e.members.All(m=>m.Category==EnemyCategory.Normal)).ToList();
        if(relief && normals.Count>0)choices=normals;
        else if(lesson!=null && (local>=lesson.introduceByLocalWave || random.NextDouble()<.55))
            choices=choices.Where(e=>e.introduction==lesson.introduction).ToList();
        else choices.RemoveAll(e=>e.introduction!=null && !Seen(e.introduction));
        if(choices.Count==0)choices=normals;
        var fresh=choices.Where(e=>!progress.recent.Contains(e.label)).ToList();
        if(fresh.Count>0)choices=fresh;
        int roll=random.Next(choices.Sum(e=>Math.Max(1,e.weight)));
        foreach(var entry in choices){roll-=Math.Max(1,entry.weight);if(roll<0)return entry;}
        throw new InvalidOperationException("Ironvein encounter weights are empty.");
    }

    public static void Record(MineEncounterProgress progress, ZoneTestEncounter selected,
        IEnumerable<EnemyDefinition> spawned, int local, int global)
    {
        if(global<=progress.lastRecordedGlobalWave)return;
        progress.seen ??= new List<string>();progress.recent ??= new List<string>();
        var actual=spawned.Where(e=>e!=null).ToArray();
        if(actual.Length==0)return;
        foreach(var enemy in actual)if(!progress.seen.Contains(enemy.EnemyId))progress.seen.Add(enemy.EnemyId);
        if(actual.Any(e=>e.Category==EnemyCategory.Boss || e.Category==EnemyCategory.Miniboss))progress.lastMilestoneLocalWave=local;
        progress.recent.Add(selected.label);while(progress.recent.Count>4)progress.recent.RemoveAt(0);
        progress.lastRecordedGlobalWave=global;
    }
}
