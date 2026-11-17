#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

// Selection statistics/fixtures only. Never included in played visit timing.
public sealed class CrystalResonanceSelectionTests
{
    [Test] public void OrdinaryAndCapstoneSelectionsRespectAllFiveDepthPools()
    {
        var lines=new List<string>{"zone,tier,kind,recipe,count"};
        foreach(var zone in Resources.LoadAll<ZoneDefinition>("Zones").Where(z=>z.eligibleForLiveTravel))
        for(int tier=0;tier<5;tier++)
        {
            var state=new CrystalResonanceSnapshot{version=1,zoneId=zone.zoneId,visit=4};
            state.seenMechanics.AddRange(zone.enemies.Select(e=>e.EnemyId));
            var counts=new Dictionary<string,int>();var kinds=new HashSet<ZoneEncounterKind>();
            for(int seed=1;seed<=500;seed++)
            {
                var ordinary=WaveController.SelectResonanceOrdinary(zone,zone.resonanceTierStarts[tier],state,new SavedRandom(unchecked((uint)seed*2654435761u)));
                Assert.AreEqual(ZoneEncounterKind.Ordinary,ordinary.resonanceKind);
                Assert.IsTrue(WaveController.ResonanceFormationLegal(zone,ordinary,tier,state,false),zone.zoneId+"/"+tier+"/"+ordinary.label);
                Assert.IsTrue(ordinary.members.All(e=>e.Category==EnemyCategory.Normal||e.Category==EnemyCategory.Special));
                if(zone.zoneId=="dungeon" && tier>=2)
                    Assert.IsFalse(ordinary.members.Any(e=>new[]{"farmer","basket_villager","pan_villager","miner","barricade_villager"}.Contains(e.EnemyId)));
                var capstone=WaveController.ChooseResonanceCapstone(zone,zone.resonanceTierStarts[tier],state,new SavedRandom(unchecked((uint)seed*2654435761u)));
                Assert.IsTrue(WaveController.ResonanceFormationLegal(zone,capstone,tier,state,true));
                Assert.IsTrue(capstone.members.All(e=>zone.enemies.Contains(e)));kinds.Add(capstone.resonanceKind);
                foreach(var recipe in new[]{ordinary,capstone})
                {string key=(int)recipe.resonanceKind+","+recipe.label;counts[key]=counts.TryGetValue(key,out int n)?n+1:1;}
            }
            var available=zone.resonanceEncounters.Where(r=>r.resonanceKind!=ZoneEncounterKind.Ordinary &&
                WaveController.ResonanceFormationLegal(zone,r,tier,state,true)).Select(r=>r.resonanceKind).Distinct();
            CollectionAssert.AreEquivalent(available,kinds,"dispersed independent seeds exercise every legal rank");
            foreach(var entry in counts.OrderBy(e=>e.Key))lines.Add(zone.zoneId+","+tier+","+entry.Key+","+entry.Value);
        }
        Directory.CreateDirectory(".utmp/CrystalResonance");
        File.WriteAllLines(".utmp/CrystalResonance/selection-distribution.csv",lines);
    }
    [Test] public void EmptyAuthoredOrdinaryLibraryHasBoundedSameZoneFallbackAtEveryTier()
    {
        foreach(var original in Resources.LoadAll<ZoneDefinition>("Zones").Where(z=>z.eligibleForLiveTravel))
        {
            var zone=UnityEngine.Object.Instantiate(original);
            try
            {
                zone.resonanceEncounters=Array.Empty<ZoneTestEncounter>();
                var state=new CrystalResonanceSnapshot();state.seenMechanics.AddRange(zone.enemies.Select(e=>e.EnemyId));
                for(int tier=0;tier<5;tier++)for(int seed=1;seed<=100;seed++)
                {
                    var recipe=WaveController.SelectResonanceOrdinary(zone,zone.resonanceTierStarts[tier],state,new SavedRandom(unchecked((uint)seed*2654435761u)));
                    Assert.IsTrue(WaveController.ResonanceFormationLegal(zone,recipe,tier,state,false),zone.zoneId+"/"+tier);
                    Assert.IsTrue(recipe.members.All(e=>zone.enemies.Contains(e)));
                }
            }
            finally {UnityEngine.Object.DestroyImmediate(zone);}
        }
    }
    [Test] public void SequentialDeepCapstonesPreserveUniqueLeadersAndMajorSpacing()
    {
        foreach(var zone in Resources.LoadAll<ZoneDefinition>("Zones").Where(z=>z.eligibleForLiveTravel))
        for(int seed=1;seed<=100;seed++)
        {
            var state=new CrystalResonanceSnapshot();state.seenMechanics.AddRange(zone.enemies.Select(e=>e.EnemyId));
            var random=new SavedRandom(unchecked((uint)seed*2654435761u));
            for(int visit=0;visit<10;visit++)
            {
                state.visit=visit;var recipe=WaveController.ChooseResonanceCapstone(zone,45,state,random);
                if(recipe.resonanceKind==ZoneEncounterKind.MajorBoss)
                {Assert.That(visit-state.lastMajorVisit,Is.GreaterThanOrEqualTo(2));state.lastMajorVisit=visit;}
                foreach(var member in recipe.members.Where(e=>e.Category==EnemyCategory.Miniboss||e.Category==EnemyCategory.Boss))
                {
                    bool required=recipe.allowRepeatedRequiredEscort && recipe.members.Any(e=>e.RequiredBossEscort==member);
                    if(!required)Assert.IsFalse(state.seenLeaders.Contains(member.EnemyId),zone.zoneId+" repeated "+member.EnemyId);
                    if(!state.seenLeaders.Contains(member.EnemyId))state.seenLeaders.Add(member.EnemyId);
                }
                state.recentCapstones.Add(zone.zoneId+":"+recipe.label);
                while(state.recentCapstones.Count>4)state.recentCapstones.RemoveAt(0);
            }
        }
    }
    [Test] public void AuthoredRecipeWeightsFadeWithTheirActualMembers()
    {
        var zone=Resources.Load<ZoneDefinition>("Zones/dungeon");
        var knights=zone.resonanceEncounters.Single(r=>r.label=="Knight patrol");
        var royals=zone.resonanceEncounters.Single(r=>r.label=="Royal vanguard");
        Assert.Less(WaveController.ResonanceRecipeWeight(zone,4,knights)/WaveController.ResonanceRecipeWeight(zone,4,royals),
            WaveController.ResonanceRecipeWeight(zone,3,knights)/WaveController.ResonanceRecipeWeight(zone,3,royals));
        Assert.AreEqual(0,WaveController.ResonanceRecipeWeight(zone,4,zone.resonanceEncounters.Single(r=>r.label=="Local search")));
    }
}
#endif
