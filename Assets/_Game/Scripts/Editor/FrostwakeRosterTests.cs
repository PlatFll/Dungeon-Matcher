using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed class FrostwakeEncounterTests
{
    [Test] public void FrostwakeWeightedVisitsTeachEveryIdentityWithReliefAndRepeatableSaveState()
    {
        var zone=Resources.Load<ZoneDefinition>("Zones/frostwake-peaks");
        Assert.That(zone.enemies.Length,Is.EqualTo(11));var signatures=new HashSet<string>();
        for(int seed=1;seed<=100;seed++)
        {
            var progress=new MineEncounterProgress();var random=new SavedRandom(seed);var labels=new List<string>();
            bool leader=false;
            for(int local=1;local<=32;local++)
            {
                var copy=JsonUtility.FromJson<MineEncounterProgress>(JsonUtility.ToJson(progress));uint state=random.State;
                var chosen=MineEncounterSelector.Select(zone,progress,local,random);
                Assert.That(MineEncounterSelector.Select(zone,copy,local,new SavedRandom(state)).label,Is.EqualTo(chosen.label));
                Assert.That(MineEncounterSelector.FitsBudget(zone,chosen,local),Is.True);
                if(leader)Assert.That(chosen.members.All(e=>e.Category==EnemyCategory.Normal),Is.True);
                Assert.That(chosen.members.All(e=>!e.IsSupport || chosen.members.Length>1),Is.True,"support has someone to support");
                leader=chosen.members.Any(e=>e.Category==EnemyCategory.Boss || e.Category==EnemyCategory.Miniboss);
                MineEncounterSelector.Record(progress,chosen,chosen.members,local,local);
                labels.Add(chosen.label);
                if(chosen.members.Contains(zone.apexEnemy))break;
            }
            CollectionAssert.AreEquivalent(zone.enemies.Select(e=>e.EnemyId),progress.seen);
            signatures.Add(string.Join("|",labels));
        }
        Assert.That(signatures.Count,Is.GreaterThan(80));
    }
}

public sealed partial class ForestFoundationPlayTests
{
    private IEnumerator LaunchFrostwakeKit(params string[] ids)
    {
        var zone=Resources.Load<ZoneDefinition>("Zones/frostwake-peaks");
        var recipes=zone.liveEncounters;var budget=zone.encounterBudget;
        zone.encounterBudget=null;zone.liveEncounters=new[]{new ZoneTestEncounter{label="Frostwake kit fixture",
            members=ids.Select(id=>zone.enemies.Single(d=>d.EnemyId==id)).ToArray()}};
        try {yield return LaunchUnified("frostwake-peaks");}
        finally {zone.liveEncounters=recipes;zone.encounterBudget=budget;}
        Set(Run.Waves,"advanceWavesAutomatically",false);PreserveRoster();
        yield return Until(()=>Run.Waves.ActiveEnemies.All(e=>e.GetComponent<EnemyLifecycleVFX>()?.IsSpawning!=true),"Frostwake spawn settles");
    }
    private IEnumerator ReadyFrostwake(EnemyActor actor)
    {Set(actor,"currentSpecialTurnCount",actor.SpecialTurnRequirement-1);yield return EnvironmentMove();}

    [UnityTest] public IEnumerator FrostwakeHunterResumesWarningAndReleasesExactlyOnce()
    {
        yield return LaunchFrostwakeKit("harpoon_hunter");var actor=Enemy("harpoon_hunter");
        yield return ReadyFrostwake(actor);var kit=actor.GetComponent<FrostwakeEnemyAbility>();
        Assert.That(kit.IsPreparing,Is.True);Assert.That(kit.ResponseMoves,Is.EqualTo(2));
        var warning=(BoardController.GemSetThreat)Get(kit,"warning");
        // Keep the deadline fixture's marked identity away from the controlled
        // top-row response clears; cancellation has its own real-clear test.
        warning.Targets.Clear();warning.Targets.Add(Run.Board.GetGem(0,0));
        int identity=warning.Targets[0].BoardIdentity;
        yield return ResumeRoster();PreserveRoster();Set(Run.Waves,"advanceWavesAutomatically",false);
        actor=Enemy("harpoon_hunter");kit=actor.GetComponent<FrostwakeEnemyAbility>();
        Assert.That(kit.ResponseMoves,Is.EqualTo(2));Assert.That(actor.GetComponent<EnemyAutoAttack>().IsPausedByAction,Is.True);
        Assert.That(((BoardController.GemSetThreat)Get(kit,"warning")).Targets[0].BoardIdentity,Is.EqualTo(identity));
        int hp=Run.Player.CurrentHealth;
        yield return EnvironmentMove();yield return EnvironmentMove();
        Assert.That(kit.IsPreparing,Is.False);Assert.That(Run.Player.CurrentHealth,Is.EqualTo(hp-15));
        Assert.That(Run.Board.FindGemIdentity(identity),Is.Null);Assert.That(actor.GetComponent<EnemyAutoAttack>().IsPausedByAction,Is.False);
        kit.ResolveAcceptedMove();yield return Stable();Assert.That(Run.Player.CurrentHealth,Is.EqualTo(hp-15));
    }
    [UnityTest] public IEnumerator FrostwakeChampionConversionCancelsAllMarksAndStaggers()
    {
        yield return LaunchFrostwakeKit("iceborn_champion");var actor=Enemy("iceborn_champion");
        yield return ReadyFrostwake(actor);var kit=actor.GetComponent<FrostwakeEnemyAbility>();
        var warning=(BoardController.GemSetThreat)Get(kit,"warning");Assert.That(warning.Targets.Count,Is.EqualTo(2));
        Set(actor.GetComponent<EnemyStagger>(),"remainingImmunityTime",0f);
        warning.Targets[0].SetSpecialType(GemSpecialType.ColorCrystal);int hp=Run.Player.CurrentHealth;
        yield return null;yield return null;
        Assert.That(kit.IsPreparing,Is.False);Assert.That(warning.Ended,Is.True);
        Assert.That(actor.GetComponent<EnemyStagger>().IsStaggered,Is.True);Assert.That(Run.Player.CurrentHealth,Is.EqualTo(hp));
    }
    [UnityTest] public IEnumerator FrostwakeTrollArmorBreakStaggersAndNextCastIsSlam()
    {
        yield return LaunchFrostwakeKit("glacier_troll");var actor=Enemy("glacier_troll");
        yield return ReadyFrostwake(actor);Assert.That(actor.CurrentShield,Is.EqualTo(30));
        yield return ResumeRoster();PreserveRoster();Set(Run.Waves,"advanceWavesAutomatically",false);
        actor=Enemy("glacier_troll");Assert.That(actor.CurrentShield,Is.EqualTo(30));
        Set(actor.GetComponent<EnemyStagger>(),"remainingImmunityTime",0f);int hp=actor.CurrentHealth;
        actor.TryTakeDamage(100);Assert.That(actor.CurrentShield,Is.Zero);Assert.That(actor.CurrentHealth,Is.EqualTo(hp));
        Assert.That(actor.GetComponent<EnemyStagger>().IsStaggered,Is.True);
        yield return EnvironmentMove();yield return EnvironmentMove();QuietKitFixture();
        yield return ReadyFrostwake(actor);Assert.That(actor.GetComponent<FrostwakeEnemyAbility>().IsPreparing,Is.True);
        Assert.That(((BoardController.GemSetThreat)Get(actor.GetComponent<FrostwakeEnemyAbility>(),"warning")).Targets.Count,Is.EqualTo(2));
    }
    [UnityTest] public IEnumerator FrostwakeRimecarverAddsIceAndDeathDoesNotRemoveIt()
    {
        yield return LaunchFrostwakeKit("rimecarver","iceborn_raider");var actor=Enemy("rimecarver");
        yield return ReadyFrostwake(actor);var saved=Run.Continuation.Capture().board;
        Assert.That(saved.frost.Count,Is.GreaterThan(0));var ids=saved.frost.Select(x=>x.identity).ToArray();
        actor.TryTakeDamage(10000);yield return Stable();
        CollectionAssert.IsSubsetOf(ids,Run.Continuation.Capture().board.frost.Select(x=>x.identity));
    }
}
