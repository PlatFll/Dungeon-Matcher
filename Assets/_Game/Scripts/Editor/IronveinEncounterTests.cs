using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public sealed class IronveinEncounterTests
{
    private ZoneDefinition Zone=>Resources.Load<ZoneDefinition>("Zones/ironvein-excavation");
    [Test] public void IronveinSeededTeachingHasVariationReliefBudgetsAndNoSummonRecipes()
    {
        var zone=Zone;var signatures=new HashSet<string>();var firstBoss=new List<int>();
        foreach(var recipe in zone.liveEncounters)
        {
            Assert.That(recipe.members.Any(e=>e.EnemyId=="rivet_turret"),Is.False);
            Assert.That(recipe.members.Count(e=>e.IsBoardDisruptor),Is.LessThanOrEqualTo(2));
        }
        for(int seed=1;seed<=120;seed++)
        {
            var progress=new MineEncounterProgress();var random=new SavedRandom(seed);var labels=new List<string>();
            bool lastLeader=false;var leaders=new HashSet<string>();
            for(int local=1;local<=32;local++)
            {
                uint before=random.State;var copy=JsonUtility.FromJson<MineEncounterProgress>(JsonUtility.ToJson(progress));
                var chosen=MineEncounterSelector.Select(zone,progress,local,random);
                Assert.That(MineEncounterSelector.Select(zone,copy,local,new SavedRandom(before)).label,Is.EqualTo(chosen.label),"Continue selection");
                Assert.That(MineEncounterSelector.FitsBudget(zone,chosen,local),Is.True,chosen.label);
                if(lastLeader)Assert.That(chosen.members.All(e=>e.Category==EnemyCategory.Normal),Is.True,"post-milestone relief");
                foreach(var special in chosen.members.Where(e=>e.Category!=EnemyCategory.Normal && !progress.seen.Contains(e.EnemyId)))
                {
                    Assert.That(special,Is.SameAs(chosen.introduction));
                    Assert.That(chosen.members.Length,Is.LessThanOrEqualTo(2),"one new mechanic at a time");
                }
                lastLeader=chosen.members.Any(e=>e.Category==EnemyCategory.Miniboss || e.Category==EnemyCategory.Boss);
                foreach(var leader in chosen.members.Where(e=>e.Category==EnemyCategory.Miniboss || e.Category==EnemyCategory.Boss))
                    Assert.That(leaders.Add(leader.EnemyId),Is.True,"one leader per visit");
                MineEncounterSelector.Record(progress,chosen,chosen.members,local,local+50);
                labels.Add(chosen.label);
                if(chosen.members.Contains(zone.apexEnemy)){firstBoss.Add(local);break;}
                foreach(var lesson in zone.liveEncounters.Where(e=>e.introduceByLocalWave>0 && local>=e.introduceByLocalWave))
                    Assert.That(progress.seen,Does.Contain(lesson.introduction.EnemyId),"lesson deadline: "+lesson.introduction.EnemyId);
            }
            Assert.That(progress.seen.Count,Is.EqualTo(13),"all identities taught, turret summoned only");
            Assert.That(leaders.Count,Is.EqualTo(3));signatures.Add(string.Join("|",labels));
        }
        Assert.That(signatures.Count,Is.GreaterThan(100));Assert.That(firstBoss.Min(),Is.EqualTo(28));Assert.That(firstBoss.Max(),Is.EqualTo(30));
    }

    [Test] public void IronveinRecordsOnlySuccessfulSpawnsAndKeepsBoundedHistory()
    {
        var progress=new MineEncounterProgress();var selected=Zone.liveEncounters.First(e=>e.introduction!=null);
        MineEncounterSelector.Record(progress,selected,Array.Empty<EnemyDefinition>(),3,3);Assert.That(progress.seen,Is.Empty);
        MineEncounterSelector.Record(progress,selected,selected.members,3,3);
        MineEncounterSelector.Record(progress,selected,selected.members,3,3);Assert.That(progress.recent.Count,Is.EqualTo(1));
        for(int i=4;i<30;i++)MineEncounterSelector.Record(progress,selected,selected.members,i,i);
        Assert.That(progress.recent.Count,Is.EqualTo(4));
        Assert.That(ZoneTravelController.DestinationReady(Zone),Is.True);
        var zones=Resources.LoadAll<ZoneDefinition>("Zones").Where(ZoneTravelController.DestinationReady).ToArray();
        Assert.That(zones.Length,Is.EqualTo(4));
        var picks=new HashSet<string>();var random=new SavedRandom(951);
        for(int i=0;i<100;i++)picks.Add(ZoneTravelController.ChooseDestination("dungeon",zones,random));
        CollectionAssert.AreEquivalent(new[]{"magical-forest","drowned-court","ironvein-excavation"},picks);
    }
}
