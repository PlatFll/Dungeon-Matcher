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
        File.WriteAllLines(".utmp/CrystalResonance/selection-distribution-"+Guid.NewGuid().ToString("N")+".csv",lines);
    }
    [Test] public void CourtOrdinaryRosterAndEscortPairsRemainReachableAtEveryTier()
    {
        var zone=Resources.Load<ZoneDefinition>("Zones/drowned-court");
        var state=new CrystalResonanceSnapshot();
        state.seenMechanics.AddRange(zone.enemies.Select(e=>e.EnemyId));
        for(int tier=0;tier<5;tier++)
        {
            var seen=new HashSet<string>();var escortPairs=new HashSet<string>();
            for(int seed=1;seed<=500;seed++)
            {
                var recipe=WaveController.SelectResonanceOrdinary(zone,zone.resonanceTierStarts[tier],state,
                    new SavedRandom(unchecked((uint)seed*2654435761u)));
                Assert.IsTrue(WaveController.ResonanceFormationLegal(zone,recipe,tier,state,false));
                var normals=recipe.members.Where(e=>e.Category==EnemyCategory.Normal).Select(e=>e.EnemyId).OrderBy(id=>id).ToArray();
                foreach(var id in normals)seen.Add(id);
                if(recipe.members.Length==3 && normals.Length==2)escortPairs.Add(string.Join("+",normals));
            }
            var eligible=zone.resonancePool.Where(e=>e.Weight(tier)>0 && e.enemy.Category==EnemyCategory.Normal)
                .Select(e=>e.enemy.EnemyId).ToArray();
            CollectionAssert.AreEquivalent(eligible,seen,"Eligible ordinary Court roster disappeared at tier "+tier);
            if(tier>=1)Assert.GreaterOrEqual(escortPairs.Count,3,"Routine patrols should not all reuse one heavy pair");
        }
    }

    [Test] public void FirstFloodKnownMechanicPatrolsHaveDifferentEscortPairs()
    {
        var zone=Resources.Load<ZoneDefinition>("Zones/drowned-court");
        var state=new CrystalResonanceSnapshot();
        state.seenMechanics.AddRange(new[]{"moray_siphoner","conch_marshal"});
        // Actual first-visit histories can know only these two flood-safe kits.
        // All-roster/dry selection statistics hid their identical heavy escorts.
        var patrols=zone.resonanceEncounters.Where(r=>r.resonanceKind==ZoneEncounterKind.Ordinary &&
            r.members.Length==3 && r.members.All(e=>e.canFightFlooded &&
                (e.Category==EnemyCategory.Normal || state.seenMechanics.Contains(e.EnemyId))) &&
            BoardController.FirstFloodLessonSafe(r.members) &&
            WaveController.ResonanceFormationLegal(zone,r,2,state,false)).ToArray();
        var pairs=patrols.Select(r=>string.Join("+",r.members.Where(e=>e.Category==EnemyCategory.Normal)
            .Select(e=>e.EnemyId).OrderBy(id=>id))).Distinct().ToArray();
        Assert.GreaterOrEqual(patrols.Length,2,"Keep distinct taught pressure mechanics available.");
        Assert.GreaterOrEqual(pairs.Length,2,"Changing the caster must not always retain the same heavy escort pair.");
        Assert.IsTrue(patrols.Any(r=>r.members.Any(e=>e.EnemyId=="hammerhead_bruiser")),
            "Retain a heavy patrol; this is variety, not removal of Court pressure.");
    }

    [Test] public void QueenCapstoneKeepsApprovedCourtCultureAndCompleteEscortBudget()
    {
        var zone=Resources.Load<ZoneDefinition>("Zones/drowned-court");
        var state=new CrystalResonanceSnapshot();state.seenMechanics.AddRange(zone.enemies.Select(e=>e.EnemyId));
        var queens=zone.resonanceEncounters.Where(r=>r.members.Any(e=>e.EnemyId=="queen_nacre")).ToArray();
        Assert.IsNotEmpty(queens);
        foreach(var recipe in queens)
        {
            Assert.IsTrue(recipe.members.All(e=>e.Faction=="Nacre Court"),recipe.label);
            Assert.LessOrEqual(recipe.members.Sum(e=>e.ThreatCost),recipe.resonanceThreatBudget);
            Assert.IsTrue(WaveController.ResonanceFormationLegal(zone,recipe,4,state,true));
        }
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
    [Test] public void ConsecutiveRecipeVariantsDoNotRepeatTheSameSpecialWhenAlternativesExist()
    {
        var zone=Resources.Load<ZoneDefinition>("Zones/drowned-court");
        var state=new CrystalResonanceSnapshot();state.seenMechanics.AddRange(zone.enemies.Select(e=>e.EnemyId));
        state.recentRecipes.Add("Learn moray_siphoner");
        for(int seed=1;seed<=200;seed++)
        {
            var recipe=WaveController.SelectResonanceOrdinary(zone,25,state,new SavedRandom(unchecked((uint)seed*2654435761u)));
            Assert.IsFalse(recipe.members.Any(e=>e.EnemyId=="moray_siphoner"),"Different labels are still the same pressure mechanic: "+recipe.label);
            Assert.IsTrue(WaveController.ResonanceFormationLegal(zone,recipe,3,state,false));
            var resumed=JsonUtility.FromJson<CrystalResonanceSnapshot>(JsonUtility.ToJson(state));
            Assert.AreEqual(recipe.label,WaveController.SelectResonanceOrdinary(zone,25,resumed,new SavedRandom(unchecked((uint)seed*2654435761u))).label);
        }
    }

    [Test] public void PostCapstoneReliefUsesCurrentDepthAndEndsAfterOneEncounter()
    {
        foreach(var zone in Resources.LoadAll<ZoneDefinition>("Zones").Where(z=>z.eligibleForLiveTravel))
        {
            var state=new CrystalResonanceSnapshot {version=1,zoneId=zone.zoneId,charge=90,capstoneWave=45,
                lastBankedWave=45,capstonePhase=ResonanceCapstonePhase.Defeated};
            state.seenMechanics.AddRange(zone.enemies.Select(e=>e.EnemyId));
            for(int seed=1;seed<=100;seed++)
            {
                var recipe=WaveController.SelectResonanceOrdinary(zone,46,state,new SavedRandom(unchecked((uint)seed*2654435761u)));
                Assert.IsTrue(recipe.members.Length<=2 && recipe.members.All(e=>e.Category==EnemyCategory.Normal),zone.zoneId+"/"+recipe.label);
                Assert.IsTrue(WaveController.ResonanceFormationLegal(zone,recipe,4,state,false));
            }
            state.lastBankedWave=46;
            Assert.IsTrue(Enumerable.Range(1,100).Any(seed=>WaveController.SelectResonanceOrdinary(zone,47,state,new SavedRandom(unchecked((uint)seed*2654435761u)))
                .members.Any(e=>e.Category==EnemyCategory.Special)),"Relief must not suppress all later pressure: "+zone.zoneId);
        }
    }

    [Test] public void ARecentReliefRecipeCanPreventAnAdjacentSpecialVariant()
    {
        var zone=UnityEngine.Object.Instantiate(Resources.Load<ZoneDefinition>("Zones/drowned-court"));
        try
        {
            zone.resonanceEncounters=zone.resonanceEncounters.Where(r=>new[]{"Learn moray_siphoner","Patrol moray_siphoner","Quiet patrol"}.Contains(r.label)).ToArray();
            var state=new CrystalResonanceSnapshot();state.seenMechanics.AddRange(zone.enemies.Select(e=>e.EnemyId));
            state.recentRecipes.AddRange(new[]{"Quiet patrol","Learn moray_siphoner"});
            Assert.AreEqual("Quiet patrol",WaveController.SelectResonanceOrdinary(zone,25,state,new SavedRandom(12)).label,
                "A fresh label must not defeat mechanic variety when the legal alternative is recent");
        }
        finally {UnityEngine.Object.DestroyImmediate(zone);}
    }

    [Test] public void RepetitionPreferenceNeverExhaustsTheOnlyLegalFormation()
    {
        var zone=UnityEngine.Object.Instantiate(Resources.Load<ZoneDefinition>("Zones/drowned-court"));
        try
        {
            var only=zone.resonanceEncounters.Single(r=>r.label=="Learn moray_siphoner");
            zone.resonanceEncounters=new[]{only};
            var state=new CrystalResonanceSnapshot();state.seenMechanics.AddRange(zone.enemies.Select(e=>e.EnemyId));state.recentRecipes.Add(only.label);
            Assert.AreEqual(only.label,WaveController.SelectResonanceOrdinary(zone,25,state,new SavedRandom(12)).label);
        }
        finally { UnityEngine.Object.DestroyImmediate(zone); }
    }

}
#endif
