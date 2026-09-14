using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed class DesignV2Tests
{
    [Test] public void TeachingDeadlinesPrecedeTheirMilestoneExamsAcrossSeeds()
    {
        var profile=AssetDatabase.LoadAssetAtPath<WaveSpawnProfile>("Assets/_Game/Data/Balance/WaveSpawnProfile_Standard.asset");
        var deadlines=new Dictionary<string,int> {{"Miner",6},{"CrossbowGuard",10},{"BarricadeGuard",11},{"ShieldKnight",16},{"RoyalStandardBearer",22},{"CourtMage",23}};
        for(int seed=0;seed<100;seed++)
        {
            var seen=new HashSet<EnemyDefinition>();var rng=new SavedRandom(seed);bool afterLeader=false;
            for(int wave=1;wave<=30;wave++)
            {
                var leader=profile.SelectMilestone(wave,rng,seen,out _);
                var lesson=leader==null?profile.SelectIntroduction(wave,rng,seen,afterLeader):null;
                if(leader!=null) seen.Add(leader);
                if(lesson!=null) seen.Add(lesson);
                afterLeader=leader!=null;
                foreach(var entry in deadlines)
                    if(wave==entry.Value) Assert.That(seen.Any(d=>d.name=="Enemy_"+entry.Key),Is.True,$"seed {seed}: {entry.Key} taught by {wave}");
            }
        }
    }

    [Test]
    public void PracticeUsesOwnedProgressionButNeverWritesInventoryOrRewards()
    {
        string path=Path.GetFullPath(".utmp/DesignV2Tests/"+Guid.NewGuid().ToString("N")+".json");
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        var initial=new AccountSave { gold=75, bombs=1, potions=2 };
        initial.characters.Add(new CharacterProgress { id="bardley",level=4 });
        File.WriteAllText(path,JsonUtility.ToJson(initial));
        string before=File.ReadAllText(path);
        using(AccountProgression.UseDisposableProfile(path))
        {
            var real=AccountProgression.Current;
            using(AccountProgression.UsePracticeProfile())
            {
                var practice=AccountProgression.Current;
                Assert.That(practice.Level("bardley"),Is.EqualTo(4));
                string run=practice.BeginRun("bardley");
                Assert.That(practice.TrySpendCharge(run,ConsumableKind.Bomb),Is.True);
                Assert.That(practice.RecordWave(run,1,false,false),Is.True);
                Assert.That(practice.FinalizeRun(run,"Practice"),Is.True);
                Assert.That(practice.LastReward.Total,Is.Zero);
            }
            Assert.That(AccountProgression.Current,Is.SameAs(real));
            Assert.That(real.Gold,Is.EqualTo(75));Assert.That(real.Owned(ConsumableKind.Bomb),Is.EqualTo(1));
        }
        Assert.That(File.ReadAllText(path),Is.EqualTo(before));
    }

    [Test]
    public void InitialDraftDirectionsAndRefinementRemainEligibleAndDistinct()
    {
        string path=Path.GetFullPath(".utmp/DesignV2Tests/"+Guid.NewGuid().ToString("N")+".json");
        using(AccountProgression.UseDisposableProfile(path))
        using(GemMasterySettings.UseTemporaryLoadout(GemMasteryLoadout.Default))
        {
            var go=new GameObject("Draft direction test");
            try
            {
                var player=go.AddComponent<PlayerActor>();
                player.Initialize(Resources.Load<PlayerDefinition>("Players/Player_Bardley"));
                var waves=go.AddComponent<WaveController>();var runtime=go.AddComponent<RunUpgradeRuntime>();
                var catalog=Resources.Load<RunUpgradeCatalog>("RunUpgrades/PrototypeRunUpgradeCatalog");
                runtime.Configure(catalog,player,waves);
                for(int seed=0;seed<100;seed++)
                {
                    var rng=new System.Random(seed);
                    var draft=UpgradeDraftGenerator.Generate(catalog,runtime,player,2,rng);
                    Assert.That(draft.Select(card=>card.Theme).Distinct().Count(),Is.EqualTo(3));
                    foreach(var card in draft) Assert.That(runtime.IsEligible(card,player,2),Is.True);
                    var refined=UpgradeDraftGenerator.Refine(catalog,runtime,player,2,rng,RunUpgradeTheme.Ability,draft);
                    Assert.That(refined,Is.Not.Empty);
                    foreach(var card in refined) {Assert.That(card.Theme,Is.EqualTo(RunUpgradeTheme.Ability));Assert.That(draft.Contains(card),Is.False);}
                }
            }
            finally { UnityEngine.Object.DestroyImmediate(go); }
        }
    }

    [Test]
    public void ChallengesRequireKingAndTrackSeparateDurableRecords()
    {
        string path=Path.GetFullPath(".utmp/DesignV2Tests/"+Guid.NewGuid().ToString("N")+".json");
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        using(AccountProgression.UseDisposableProfile(path))
            Assert.That(AccountProgression.Current.BeginRun("bardley",RunChallenge.NoSupplies),Is.Null);
        File.WriteAllText(path,JsonUtility.ToJson(new AccountSave {firstKingClaimed=true,bombs=3,potions=3,equipBombs=true,equipPotions=true}));
        using(AccountProgression.UseDisposableProfile(path))
        {
            var account=AccountProgression.Current;string run=account.BeginRun("bardley",RunChallenge.BoardOnly);
            Assert.That(run,Is.Not.Null);Assert.That(account.Charges(run,ConsumableKind.Bomb),Is.Zero);
            account.RecordWave(run,1,false,false);account.FinalizeRun(run,"Defeat");
        }
        var restored=new AccountProgression(path);
        Assert.That(restored.Record(RunChallenge.BoardOnly).bestWave,Is.EqualTo(1));
        Assert.That(restored.Record(RunChallenge.NoSupplies).bestWave,Is.Zero);
    }

    [UnityTest]
    public IEnumerator AcceptedBardleyCastKeepsRefundCapAfterCancellationAndBombPreviewDoesNotSpend()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        yield return new EnterPlayMode();
        string path=Path.GetFullPath(".utmp/DesignV2Tests/"+Guid.NewGuid().ToString("N")+".json");
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path,JsonUtility.ToJson(new AccountSave { bombs=2,equipBombs=true }));
        using(AccountProgression.UseDisposableProfile(path))
        using(CharacterSelectionSettings.UseTemporarySelection("bardley"))
        using(GemMasterySettings.UseTemporaryLoadout(GemMasteryLoadout.Default))
        {
            SceneManager.LoadScene("Game");
            yield return Until(()=>RunSession.Current!=null && RunSession.Current.Waves.IsWaveActive && !RunSession.Current.Board.IsBusy,"start");
            var run=RunSession.Current;var player=run.Player;
            var energy=player.GetComponent<PlayerAbilityEnergy>();var ability=player.GetComponent<PlayerAbilityController>();
            var runtime=RunUpgradeRuntime.Current;
            var efficiency=Resources.Load<RunUpgradeDefinition>("RunUpgrades/Prototype_EfficientCasting");
            Assert.That(runtime.TryApply(efficiency,2),Is.True);Assert.That(runtime.TryApply(efficiency,2),Is.True);
            Assert.That(runtime.TryApply(Resources.Load<RunUpgradeDefinition>("RunUpgrades/RunUpgrade_ResonantCracks"),2),Is.True);
            energy.AddEnergy(100);energy.TrySpendEnergy(energy.CurrentEnergy-ability.RequiredEnergy);
            int cost=ability.RequiredEnergy;
            Assert.That(cost,Is.EqualTo(64));Assert.That(ability.TryActivate(),Is.True);
            ability.CancelActiveAbility();Assert.That(run.Board.IsBusy,Is.True,"cancellation preserves accepted board ownership");
            yield return Until(()=>!run.Board.IsBusy,"Bardley settles");
            Assert.That(energy.CurrentEnergy,Is.InRange(1,32),"all explosion and card refunds share the effective-cost cap");
            yield return Until(()=>run.CanUse(ConsumableKind.Bomb),"Bomb ready");
            Assert.That(run.ToggleBombTargeting(),Is.True);
            Gem target=run.Board.GetGem(3,3);run.Board.BeginPointerGesture(target,Vector2.zero,0);
            Assert.That(run.HasBombPreview,Is.True);Assert.That(run.Charges(ConsumableKind.Bomb),Is.EqualTo(2));
            run.CancelTargeting();Assert.That(run.Charges(ConsumableKind.Bomb),Is.EqualTo(2));
            run.ToggleBombTargeting();run.Board.BeginPointerGesture(target,Vector2.zero,0);
            Assert.That(run.ConfirmBomb(),Is.True);Assert.That(run.Charges(ConsumableKind.Bomb),Is.EqualTo(1));
            yield return Until(()=>!run.Board.IsBusy,"Bomb settles");
            var ui=UnityEngine.Object.FindFirstObjectByType<RunControlsUI>();ui.OpenGuide(CombatGuide.Ability(player));
            Assert.That(Time.timeScale,Is.Zero);Assert.That(ability.CanActivate,Is.False);ui.Close();
            Assert.That(Time.timeScale,Is.EqualTo(1));Assert.That(run.ExitTo("MainMenu"),Is.True);yield return null;
        }
        yield return new ExitPlayMode();
    }

    private static IEnumerator Until(Func<bool> condition,string label)
    {
        float end=Time.realtimeSinceStartup+45;
        while(!condition()){Assert.That(Time.realtimeSinceStartup,Is.LessThan(end),label);yield return null;}
    }

    [Test]
    public void ChainedClearsAndCardRefundsShareEffectiveCostBudget()
    {
        var budget = new AbilityRefundBudget();
        budget.Begin(64, .5f);
        Assert.That(budget.Take(21), Is.EqualTo(21));
        Assert.That(budget.Take(5), Is.EqualTo(5));
        Assert.That(budget.Take(40), Is.EqualTo(6));
        Assert.That(budget.Take(5), Is.Zero);
        budget.End();
        Assert.That(budget.Take(10), Is.EqualTo(10), "ordinary matches after the cast are unrestricted");
    }

    [Test]
    public void RefundNeverPaysMoreThanRoundedBudgetAndRejectedCastReleasesIt()
    {
        var budget = new AbilityRefundBudget();
        budget.Begin(1, .5f);
        Assert.That(budget.Take(100), Is.Zero);
        budget.End();
        Assert.That(budget.Take(100), Is.EqualTo(100));
        budget.Begin(80, -.1f);
        Assert.That(budget.Take(100), Is.EqualTo(100));
    }
}
