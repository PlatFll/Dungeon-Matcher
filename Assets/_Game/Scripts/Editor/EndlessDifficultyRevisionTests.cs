using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed class EndlessDifficultyRevisionTests
{
    internal static readonly int[] Waves={1,15,30,50,70,100,150};
    internal static DifficultyProfile Profile=>AssetDatabase.LoadAssetAtPath<DifficultyProfile>(
        "Assets/_Game/Data/Balance/DifficultyProfile_Standard.asset");
    private static IEnumerable<EnemyDefinition> Definitions()=>AssetDatabase.FindAssets("t:EnemyDefinition",new[]{"Assets/_Game/Data/Enemies"})
        .Select(g=>AssetDatabase.LoadAssetAtPath<EnemyDefinition>(AssetDatabase.GUIDToAssetPath(g))).OrderBy(d=>d.EnemyId);

    [TestCase(1,1f)][TestCase(15,1.4f)][TestCase(30,2f)][TestCase(50,3f)]
    [TestCase(70,4f)][TestCase(100,6f)][TestCase(150,9f)]
    public void ProductionAnchorsApplyToEveryEnemyWithoutIncreasingOtherPressure(int wave,float hpMultiplier)
    {
        Assert.That(Profile,Is.Not.Null);
        var rows=new List<string>{"Enemy\tWave\tHP\tDamage\tFollowUp\tIntervalSeconds\tSpecialMoves"};
        foreach(var def in Definitions())
        {
            var stats=Profile.CalculateStats(def,wave);
            Assert.That(stats.MaxHealth,Is.EqualTo(CombatAmounts.Health(def.BaseMaxHealth*hpMultiplier*def.HealthMultiplier)),def.EnemyId);
            float damage=(1f+.01f*(Math.Min(wave,100)-1))*(1f+Math.Max(0,wave-100)*.015f)*def.DamageMultiplier;
            Assert.That(stats.DamageMultiplier,Is.EqualTo(damage).Within(.00001f),def.EnemyId);
            Assert.That(stats.Damage,Is.EqualTo(CombatAmounts.Round(def.BaseDamage*stats.DamageMultiplier)),def.EnemyId);
            Assert.That(stats.FollowUpDamage,Is.EqualTo(CombatAmounts.Round(def.BaseFollowUpDamage*stats.DamageMultiplier)),def.EnemyId);
            Assert.That(stats.AttackInterval,Is.EqualTo(Mathf.Max(1.25f,def.BaseAttackInterval/def.AttackSpeedMultiplier)).Within(.0001f),def.EnemyId);
            Assert.That(stats.SpecialTurnRequirement,Is.EqualTo(Math.Max(def.LockSpecialTurnRequirement?1:2,def.BaseSpecialTurnRequirement)),def.EnemyId);
            Assert.That(Profile.CalculateStats(def,wave,1000f).MaxHealth,Is.EqualTo(stats.MaxHealth),"no hidden player-power correction");
            Assert.That(stats.MaxHealth%5,Is.Zero);Assert.That(stats.Damage%5,Is.Zero);
            rows.Add(FormattableString.Invariant($"{def.DisplayName}\t{wave}\t{stats.MaxHealth}\t{stats.Damage}\t{stats.FollowUpDamage}\t{stats.AttackInterval:0.###}\t{stats.SpecialTurnRequirement}"));
        }
        Directory.CreateDirectory(".utmp/RosterEndless");
        File.WriteAllLines($".utmp/RosterEndless/Stats-{wave}.tsv",rows);
        Assert.That(rows.Count,Is.GreaterThan(40),"all three production rosters are covered");
    }

    [Test] public void ProductionHpIsMonotoneAndRemainsFiniteBeyondTheLastAnchor()
    {
        var curve=new SerializedObject(Profile).FindProperty("healthMultiplierByWave").animationCurveValue;
        float prior=0;
        for(int wave=1;wave<=100;wave++) {float value=curve.Evaluate(wave);Assert.That(value,Is.GreaterThanOrEqualTo(prior));prior=value;}
        foreach(var def in Definitions())
        {
            int hp=0;
            foreach(int wave in Waves.Concat(new[]{1000,1000000}))
            {var stats=Profile.CalculateStats(def,wave);Assert.That(stats.MaxHealth,Is.GreaterThanOrEqualTo(hp));hp=stats.MaxHealth;
                Assert.That(stats.Level,Is.GreaterThanOrEqualTo(wave));Assert.That(float.IsInfinity(stats.AttackInterval),Is.False);}
        }
    }
}

public sealed partial class ForestFoundationPlayTests
{
    [UnityTest] public IEnumerator EndlessDungeonRuntimeStatsResumeAndTravel(){yield return EndlessRuntimeStats("dungeon","magical-forest");}
    [UnityTest] public IEnumerator EndlessForestRuntimeStatsResumeAndTravel(){yield return EndlessRuntimeStats("magical-forest","drowned-court");}
    [UnityTest] public IEnumerator EndlessCourtRuntimeStatsResumeAndTravel(){yield return EndlessRuntimeStats("drowned-court","dungeon");}
    private void PauseScalingActor(EnemyActor actor)=>actor.GetComponent<EnemyAutoAttack>()?.SetActionPaused(this,true);

    private IEnumerator EndlessRuntimeStats(string zone,string destination)
    {
        RunLaunchOptions.ForestPrototype=false;RunLaunchOptions.StartingZone=zone;
        SceneManager.LoadScene("Game");yield return Stable();
        Assert.That(EditorUtility.audioMasterMute,Is.True);
        Assert.That(Run.Resonance.Enabled, Is.True, "exercise the live travel and score profile");
        Run.Travel.enabled = false;
        Set(Run.Waves, "advanceWavesAutomatically", false);
        var swap = Run.Board.GetImmediateResponses().First();
        long initialScore = Run.Resonance.RunScore;
        Run.Board.ReplayPlayerSwap(swap.Source.Column, swap.Source.Row, swap.Target.Column, swap.Target.Row);
        yield return Until(() => Run.Continuation.CanCapture && Run.Resonance.RunScore > initialScore,
            "a legal board swap earns persistent score");
        string folder=Path.GetFullPath(".utmp/RosterEndless");Directory.CreateDirectory(folder);
        var rows=new List<string>{"Zone\tGlobalWave\tEnemy\tHP\tDamage\tIntervalSeconds\tSpecialMoves"};
        foreach(int wave in EndlessDifficultyRevisionTests.Waves)
        {
            Run.Travel.enabled=false;Set(Run.Waves,"advanceWavesAutomatically",false);
            Run.Waves.EnemySpawned+=PauseScalingActor;
            // Advance only the disposable journal; actual spawning and stats use production owners.
            bool finalSample = wave == EndlessDifficultyRevisionTests.Waves.Last();
            int firstFixtureWave = finalSample ? wave - 1 : wave;
            while(AccountProgression.Current.ActiveRun.completedWaves<firstFixtureWave-1)
                Assert.That(AccountProgression.Current.RecordWave(Run.RunId,AccountProgression.Current.ActiveRun.completedWaves+1,false,false),Is.True);
            if (finalSample)
            {
                // Model a late-visit checkpoint; the current encounter must still bank its own
                // victory, schedule a real capstone and defeat it before the travel gate opens.
                var state = Run.Resonance.State;
                state.charge = Run.Resonance.Tuning.capacity - 1;
                state.triggeredMask = state.claimedMask = 7;
                Set(Run.Waves, "currentWave", firstFixtureWave);
                Run.Waves.SpawnCurrentWave();
                yield return Stable();
                KillEncounter();
                yield return Until(() => Run.Continuation.CanCapture && !Run.Resonance.IsSavePending &&
                    state.capstonePhase == ResonanceCapstonePhase.Scheduled, "bank predecessor and schedule capstone");
                Assert.That(state.capstoneWave, Is.EqualTo(wave));
            }
            Set(Run.Waves,"currentWave",wave);Run.Waves.SpawnCurrentWave();yield return Stable();
            Assert.That(Run.Waves.ActiveEnemies,Is.Not.Empty);
            foreach(var actor in Run.Waves.ActiveEnemies)
            {
                PauseScalingActor(actor);AssertScalingActor(actor,wave);
                var stats=actor.RuntimeStats;
                rows.Add(FormattableString.Invariant($"{zone}\t{wave}\t{actor.Definition.DisplayName}\t{actor.MaxHealth}\t{stats.Damage}\t{stats.AttackInterval:0.###}\t{stats.SpecialTurnRequirement}"));
                actor.ResolveDamageWithoutFeedback(5);
            }
            var before=Run.Continuation.Capture();
            yield return ResumeCourtCheckpoint();
            Assert.That(before.wave, Is.EqualTo(wave));
            Assert.That(Run.Waves.CurrentWave,Is.EqualTo(before.wave));Assert.That(Run.Zone.Definition.zoneId,Is.EqualTo(zone));
            Assert.That(Run.Resonance.RunScore, Is.EqualTo(before.resonance.score));
            Assert.That(Run.Resonance.Committed, Is.EqualTo(before.resonance.charge));
            var tracker=UnityEngine.Object.FindFirstObjectByType<WaveTrackerUI>();
            var label=(TMPro.TMP_Text)Get(tracker,"waveText");
            yield return null;label.ForceMeshUpdate();
            Assert.That(tracker.GetComponent<CrystalResonanceHud>(), Is.Not.Null);
            Assert.That(label.GetParsedText(), Is.EqualTo("SCORE " + before.resonance.score.ToString("N0")),
                "Continue displays the cumulative score from the saved run");
            Assert.That(label.isTextOverflowing,Is.False);
            foreach(var saved in before.enemies)
            {
                var actor=Run.Waves.ContinuationEnemy(saved.slot);AssertScalingActor(actor,wave);
                Assert.That(actor.CurrentHealth,Is.EqualTo(saved.health),"Continue cannot heal or re-scale current HP");
                Assert.That(actor.PersistentId,Is.EqualTo(saved.persistentId));
            }
        }
        File.WriteAllLines(Path.Combine(folder,zone+"-runtime.tsv"),rows);
        yield return Until(()=>Run.Waves.ActiveEnemies.All(e=>e.GetComponent<EnemyLifecycleVFX>()?.IsSpawning!=true),"scaling portraits finish spawning");
        float feedbackEnd=Time.time+1f;yield return Until(()=>Time.time>=feedbackEnd,"scaling damage feedback settles");
        yield return CaptureScalingPortraits(folder,zone);
        Run.Travel.enabled = false;
        Set(Run.Waves, "advanceWavesAutomatically", false);
        Assert.That(Run.Resonance.State.capstonePhase, Is.EqualTo(ResonanceCapstonePhase.InProgress));
        CollectionAssert.AreEquivalent(Run.Resonance.State.capstoneMembers,
            Run.Waves.OriginalEncounterDefinitions.Select(e => e.EnemyId));
        Assert.That(Run.Resonance.PortalReady, Is.False);
        string runId = Run.RunId;
        long savedScore = Run.Resonance.RunScore;
        KillEncounter();
        yield return Until(() => !Run.Waves.IsWaveActive && Run.Continuation.CanCapture &&
            Run.Resonance.PortalReady, "complete the entire assigned capstone and settle its rewards");
        Assert.That(Run.Resonance.State.capstonePhase, Is.EqualTo(ResonanceCapstonePhase.Defeated));
        Assert.That(Run.Travel.State.stage, Is.Zero);
        var candidates = Resources.LoadAll<ZoneDefinition>("Zones");
        uint travelSeed = Enumerable.Range(1, 128).Select(i => unchecked((uint)i * 2654435761u))
            .First(s => ZoneTravelController.ChooseDestination(zone, candidates, new SavedRandom(s)) == destination);
        Run.Travel.State.random = travelSeed;
        Run.Travel.enabled = true;
        yield return Until(() => Run?.InitialStateReady == true && Run.Zone.Definition.zoneId == destination &&
            !Run.Continuation.IsRestoring && Run.Travel.State.stage == 0 && Run.Continuation.CanCapture &&
            Run.Waves.IsWaveActive, "production travel commits, reloads and reveals the selected zone");
        Assert.That(Run.RunId, Is.EqualTo(runId));
        Assert.That(Run.Resonance.Committed, Is.Zero);
        Assert.That(Run.Resonance.RunScore, Is.GreaterThanOrEqualTo(savedScore));
        Assert.That(Run.Waves.CurrentWave,Is.EqualTo(151));Assert.That(Run.Travel.LocalWave,Is.EqualTo(1));
        Assert.That(Run.Zone.Definition.zoneId,Is.EqualTo(destination));
        foreach(var actor in Run.Waves.ActiveEnemies)AssertScalingActor(actor,151);
    }

    private void AssertScalingActor(EnemyActor actor,int wave)
    {
        Assert.That(Get(Run.Waves,"difficultyProfile"),Is.SameAs(EndlessDifficultyRevisionTests.Profile));
        var expected=EndlessDifficultyRevisionTests.Profile.CalculateStats(actor.Definition,wave);
        Assert.That(actor.RuntimeStats.Wave,Is.EqualTo(wave));Assert.That(actor.MaxHealth,Is.EqualTo(expected.MaxHealth));
        Assert.That(actor.RuntimeStats.Damage,Is.EqualTo(expected.Damage));
        Assert.That(actor.RuntimeStats.AttackInterval,Is.EqualTo(expected.AttackInterval));
        Assert.That(actor.RuntimeStats.SpecialTurnRequirement,Is.EqualTo(expected.SpecialTurnRequirement));
    }

    private IEnumerator CaptureScalingPortraits(string folder,string zone)
    {
        var layout=UnityEngine.Object.FindFirstObjectByType<GameplayPixelLayoutController>();
        try
        {
            foreach(var size in new[]{new Vector2Int(720,1280),new Vector2Int(1080,1920),new Vector2Int(1080,2400)})
            {
                typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{size});
                // Editor renders can run between GameView resize and LateUpdate.
                // Refresh the existing layout owner before inspecting the resized view.
                layout.Refresh(true);
                yield return Until(()=>Screen.width==size.x && Screen.height==size.y,"scaling portrait size");
                layout.Refresh(true);
                for(int i=0;i<8;i++)yield return null;
                Assert.That(GameplayPixelLayoutValidator.Validate(layout,out string report),Is.Empty,report);
                ScreenCapture.CaptureScreenshot(Path.Combine(folder,$"{zone}-{size.x}x{size.y}.png"));yield return null;yield return null;
            }
            GameplayPixelLayoutController.ValidationSafeArea=new Rect(32,72,1016,2240);
            for(int i=0;i<8;i++)yield return null;
            Assert.That(GameplayPixelLayoutValidator.Validate(layout,out string safeReport),Is.Empty,safeReport);
            ScreenCapture.CaptureScreenshot(Path.Combine(folder,zone+"-safe-inset.png"));yield return null;yield return null;
        }
        finally {GameplayPixelLayoutController.ValidationSafeArea=null;}
    }
}
