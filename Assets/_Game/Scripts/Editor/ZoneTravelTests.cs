using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed class ZoneTravelTests
{
    [Test] public void DestinationPolicyExcludesCurrentAndUnreadyAndKeepsDeterministicState()
    {
        var zones=new[]{"dungeon","magical-forest","fixture-aquatic","unfinished"}.Select(id=>{
            var z=ScriptableObject.CreateInstance<ZoneDefinition>();z.zoneId=id;z.eligibleForLiveTravel=id!="unfinished";return z;}).ToArray();
        try
        {
            var random=new SavedRandom(98231);var seen=new HashSet<string>();
            for(int i=0;i<60;i++) {uint state=random.State;string pick=ZoneTravelController.ChooseDestination("dungeon",zones,random);
                Assert.That(pick,Is.EqualTo(ZoneTravelController.ChooseDestination("dungeon",zones,new SavedRandom(state))));seen.Add(pick);}
            CollectionAssert.AreEquivalent(new[]{"magical-forest","fixture-aquatic"},seen);
            Assert.That(ZoneTravelController.ChooseDestination("dungeon",zones.Take(1),random),Is.Null);
            foreach(var zone in Resources.LoadAll<ZoneDefinition>("Zones").Where(z=>z.eligibleForLiveTravel))
                Assert.That(ZoneTravelController.DestinationReady(zone),Is.True,zone.zoneId);
        }
        finally {foreach(var zone in zones) UnityEngine.Object.DestroyImmediate(zone);}
    }
    [Test] public void RootAndVineBindingsAreDistinctNativeAndComplete()
    {
        var theme=Resources.Load<GameplayThemeDefinition>("Zones/ForestTheme");
        Assert.That(theme.rootLevelOne,Is.Not.Null);Assert.That(theme.rootLevelTwo,Is.Not.SameAs(theme.rootLevelOne));
        Assert.That(theme.vineSpreadFrames.Length,Is.EqualTo(9));Assert.That(theme.vineHitFrames.Length,Is.EqualTo(9));
        foreach(var sprite in theme.vineSpreadFrames.Concat(theme.vineHitFrames).Concat(new[]{theme.vineOverlay,theme.rootLevelOne,theme.rootLevelTwo}))
        {Assert.That(sprite.rect.size,Is.EqualTo(new Vector2(64,64)));Assert.That(sprite.texture.filterMode,Is.EqualTo(FilterMode.Point));Assert.That(sprite.pixelsPerUnit,Is.EqualTo(64));}
        Assert.That(Resources.Load<Sprite>("UI/Transition/Smoke"),Is.Not.Null);
        Assert.That(Resources.Load<Sprite>("UI/Finalized/SplitStoryGem"),Is.Not.Null);
    }
}

public sealed partial class ForestFoundationPlayTests
{
    private sealed class TravelDestinationFixture : IDisposable
    {
        private readonly ZoneDefinition[] zones=Resources.LoadAll<ZoneDefinition>("Zones");
        private readonly bool[] original;
        public TravelDestinationFixture(params string[] eligible)
        {
            original=zones.Select(z=>z.eligibleForLiveTravel).ToArray();
            for(int i=0;i<zones.Length;i++)zones[i].eligibleForLiveTravel=eligible.Contains(zones[i].zoneId);
        }
        public void Dispose(){for(int i=0;i<zones.Length;i++)zones[i].eligibleForLiveTravel=original[i];}
    }
    private static IEnumerator CaptureTravel(string name)
    {
        string dir=Path.GetFullPath(".utmp/ForestValidation/Travel");Directory.CreateDirectory(dir);
        if(ZoneTransitionView.Active==null)
            yield return Until(()=>RunSession.Current.Waves.ActiveEnemies.All(e=>e.GetComponent<EnemyLifecycleVFX>()?.IsSpawning!=true),"arrival sprites settle");
        yield return null;ScreenCapture.CaptureScreenshot(Path.Combine(dir,name+".png"));yield return null;yield return null;
    }
    private void KillEncounter()
    { foreach(var enemy in Run.Waves.ActiveEnemies.ToArray()) {enemy.ResolveDamageWithoutFeedback(999999);if(!enemy.IsDefeated)enemy.ResolveDamageWithoutFeedback(999999);} }
    private static void AssertBoardCarryover(BoardCombatSnapshot before,BoardCombatSnapshot after)
    {
        Assert.That(after.moves,Is.EqualTo(before.moves));Assert.That(after.refillRandom,Is.EqualTo(before.refillRandom));
        Assert.That(after.cells.Count,Is.EqualTo(before.cells.Count));
        for(int i=0;i<before.cells.Count;i++)
        {Assert.That(after.cells[i].identity,Is.EqualTo(before.cells[i].identity));Assert.That(after.cells[i].type,Is.EqualTo(before.cells[i].type));Assert.That(after.cells[i].special,Is.EqualTo(before.cells[i].special));}
    }
    [UnityTest] public IEnumerator CrystalSmokeKeepsSettingsInteractiveAndFreezesCombat()
    {
        yield return Launch(1,true);QuietKitFixture();
        var before=Run.Continuation.Capture();var controls=Run.GetComponent<RunControlsUI>();
        var view=ZoneTransitionView.Create(Run);int prepared=0;
        Assert.That(view.TryPlay("Briarheart Wilds",()=>{prepared++;return true;}),Is.True);
        yield return Until(()=>prepared==1,"full smoke");
        var pointer=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,controls.SettingsButton.transform.position)};
        var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);
        Assert.That(hits.Count,Is.GreaterThan(0));
        var button=hits[0].gameObject.GetComponentInParent<UnityEngine.UI.Button>();
        Assert.That(button,Is.SameAs(controls.SettingsButton),"gear receives input through smoke");
        ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerClickHandler);
        Assert.That(controls.IsModalOpen,Is.True);
        Assert.That(view.Cover,Is.EqualTo(1));Assert.That(Time.timeScale,Is.Zero);Assert.That(Run.Board.IsExternalInputBlocked,Is.True);
        Assert.That(controls.SettingsButton.GetComponent<Canvas>().sortingOrder,Is.GreaterThan(view.GetComponent<Canvas>().sortingOrder));
        yield return CaptureTravel("01-settings-over-smoke");
        float cover=view.Cover;float t=Time.realtimeSinceStartup;yield return Until(()=>Time.realtimeSinceStartup>t+.25f,"paused presentation");
        Assert.That(view.Cover,Is.EqualTo(cover));Assert.That(prepared,Is.EqualTo(1));
        AssertBoardCarryover(before.board,Run.Continuation.Capture().board);
        controls.Close();yield return Until(()=>ZoneTransitionView.Active==null,"reveal ends");
        Assert.That(Time.timeScale,Is.EqualTo(1));Assert.That(Run.Board.IsExternalInputBlocked,Is.False);
        var after=Run.Continuation.Capture();Assert.That(after.player.health,Is.EqualTo(before.player.health));
        Assert.That(after.enemies[0].attackRemaining,Is.EqualTo(before.enemies[0].attackRemaining).Within(.1));
    }
    [UnityTest] public IEnumerator KingEscortThenForestApexTravelRoundTripPreservesRunAndUnifiedMoves()
    {
        using var destinations=new TravelDestinationFixture("dungeon","magical-forest");
        RunLaunchOptions.ForestPrototype=false;yield return LaunchUnified();
        Assert.That(CombatMoveClock.Unified,Is.True);Assert.That(Run.Travel.State.enabled,Is.True);
        // Jump past already-completed introductory milestones in this fixture.
        var seen=(HashSet<EnemyDefinition>)Get(Run.Waves,"seenMilestoneLeaders");
        foreach(var enemy in Run.Zone.Definition.enemies) if(enemy.Category==EnemyCategory.Miniboss) seen.Add(enemy);
        for(int wave=1;wave<30;wave++) Assert.That(AccountProgression.Current.RecordWave(Run.RunId,wave,false,false),Is.True);
        Set(Run.Waves,"currentWave",30);Run.Waves.SpawnCurrentWave();yield return Stable();QuietKitFixture();
        var king=Enemy("king");Assert.That(Run.Waves.ActiveEnemies.Count,Is.GreaterThan(1));
        Run.Player.TryTakeDamage(25);Run.Player.GrantShield(15);Run.Player.GetComponent<PlayerAbilityEnergy>().AddEnergy(25);
        Run.Board.GetGem(0,0).SetSpecialType(GemSpecialType.RowBomb);
        var before=Run.Continuation.Capture();string runId=Run.RunId;
        king.ResolveDamageWithoutFeedback(999999);yield return null;
        Assert.That(Run.Travel.State.stage,Is.Zero,"the independent escort must also die");
        KillEncounter();
        yield return Until(()=>RunSession.Current?.Zone?.Definition?.zoneId=="magical-forest" && Run.Travel.State.stage==0 && Run.Waves.IsWaveActive && Run.Continuation.CanCapture,"dungeon to forest");
        QuietKitFixture();var forest=Run.Continuation.Capture();
        Assert.That(Run.RunId,Is.EqualTo(runId));Assert.That(forest.clock.profile,Is.EqualTo(CombatClockSnapshot.UnifiedProfile));
        Assert.That(CombatMoveClock.MoveEffects,Is.True);Assert.That(forest.wave,Is.EqualTo(31));Assert.That(Run.Travel.LocalWave,Is.EqualTo(1));
        Assert.That(forest.player.health,Is.EqualTo(before.player.health));Assert.That(forest.player.shield,Is.EqualTo(before.player.shield));
        Assert.That(forest.player.energy,Is.EqualTo(before.player.energy));AssertBoardCarryover(before.board,forest.board);
        yield return CaptureTravel("02-dungeon-to-forest");
        for(int wave=31;wave<48;wave++) Assert.That(AccountProgression.Current.RecordWave(Run.RunId,wave,false,false),Is.True);
        Set(Run.Waves,"currentWave",48);Run.Waves.SpawnCurrentWave();yield return Stable();QuietKitFixture();
        Assert.That(Run.Waves.ActiveEnemies.Any(e=>e.Definition.EnemyId=="briar_matriarch"),Is.True);
        KillEncounter();
        yield return Until(()=>Run.Continuation.CanCapture && Run.Waves.GetComponent<RunUpgradeCoordinator>().IsBlockingWaveProgression &&
            Run.Continuation.Capture().draft.Count>0,"forest apex card reward");
        var coordinator=Run.Waves.GetComponent<RunUpgradeCoordinator>();
        var choice=(UpgradeChoiceUI)Get(coordinator,"choiceUI");
        Assert.That(choice,Is.Not.Null);Assert.That(choice.gameObject.scene,Is.EqualTo(Run.gameObject.scene));
        Assert.That(ZoneTransitionView.Active,Is.Null,"travel waits for the reward choice");
        string card=Run.Continuation.Capture().draft[0];
        Assert.That(coordinator.SelectRecordedCard(card),Is.True);
        var rewarded=Run.Continuation.Capture();
        yield return Until(()=>RunSession.Current?.Zone?.Definition?.zoneId=="dungeon" && Run.Travel.State.stage==0 && Run.Waves.IsWaveActive && Run.Continuation.CanCapture,"forest to dungeon");
        QuietKitFixture();var after=Run.Continuation.Capture();
        Assert.That(after.cards.Any(c=>c.id==card),Is.True,"the chosen reward crosses the handoff once");
        Assert.That(Run.RunId,Is.EqualTo(runId));Assert.That(after.wave,Is.EqualTo(49));Assert.That(Run.Travel.State.visit,Is.EqualTo(2));
        Assert.That(after.player.health,Is.EqualTo(rewarded.player.health));AssertBoardCarryover(rewarded.board,after.board);
        Assert.That(AccountProgression.Current.ActiveRun.completedWaves,Is.EqualTo(48));
        Assert.That(AccountProgression.Current.ActiveRun.milestoneCount,Is.EqualTo(1),"Matriarch is a milestone, not another King reward");
        yield return CaptureTravel("03-forest-to-dungeon");
    }
    [UnityTest] public IEnumerator PendingTravelSuspendRestoresChosenDestinationAndBoard()
    {
        yield return Launch(8,true);QuietKitFixture();Run.Travel.State.enabled=true;
        var board=Run.Continuation.Capture().board;KillEncounter();
        yield return Until(()=>ZoneTransitionView.Active?.Phase=="Covering","pending travel cover");
        var controls=Run.GetComponent<RunControlsUI>();controls.OpenSettings();
        string destination=Run.Travel.State.destination;uint random=Run.Travel.State.random;string id=Run.RunId;
        Assert.That(Run.SuspendToMenu(),Is.True);yield return null;yield return null;
        Assert.That(ZoneTransitionView.Active,Is.Null);
        SceneManager.LoadScene("Game");yield return null;
        yield return Until(()=>Run!=null && !Run.Continuation.IsRestoring && Run.InitialStateReady,"pending source restores");
        Assert.That(Run.Travel.State.destination,Is.EqualTo(destination));Assert.That(Run.Travel.State.random,Is.EqualTo(random));
        Run.GetComponent<RunControlsUI>().Close();
        yield return Until(()=>Run?.Zone?.Definition?.zoneId==destination && Run.Travel.State.stage==0 && Run.Continuation.CanCapture && Run.Waves.IsWaveActive,"chosen travel resumes");
        Assert.That(Run.RunId,Is.EqualTo(id));Assert.That(Run.Travel.State.visit,Is.EqualTo(1));
        AssertBoardCarryover(board,Run.Continuation.Capture().board);
        Assert.That(AccountProgression.Current.ActiveRun.completedWaves,Is.EqualTo(1));
    }
    [UnityTest] public IEnumerator CommittedTravelSuspendResumesRevealWithoutRepeatingReward()
    {
        using var destinations=new TravelDestinationFixture("dungeon","magical-forest");
        yield return Launch(8,true);QuietKitFixture();Run.Travel.State.enabled=true;
        var before=Run.Continuation.Capture().board;string id=Run.RunId;KillEncounter();
        yield return Until(()=>Run?.Zone?.Definition?.zoneId=="dungeon" && Run.Continuation.CanCapture &&
            ZoneTransitionView.Active?.Phase=="Revealing","destination committed beneath smoke");
        Run.GetComponent<RunControlsUI>().OpenSettings();
        Assert.That(Run.Travel.State.stage,Is.EqualTo(2));
        Assert.That(Run.SuspendToMenu(),Is.True);yield return null;yield return null;
        Assert.That(ZoneTransitionView.Active,Is.Null);
        SceneManager.LoadScene("Game");yield return null;
        yield return Until(()=>Run!=null && Run.InitialStateReady && !Run.Continuation.IsRestoring,"destination restores");
        Assert.That(Run.Travel.State.stage,Is.EqualTo(2));Run.GetComponent<RunControlsUI>().Close();
        yield return Until(()=>Run.Travel.State.stage==0 && Run.Waves.IsWaveActive && Run.Continuation.CanCapture,"committed reveal resumes");
        Assert.That(Run.RunId,Is.EqualTo(id));Assert.That(Run.Travel.State.visit,Is.EqualTo(1));
        Assert.That(AccountProgression.Current.ActiveRun.completedWaves,Is.EqualTo(1));
        AssertBoardCarryover(before,Run.Continuation.Capture().board);
    }
    [UnityTest] public IEnumerator FailedTravelCheckpointKeepsSourceAndRetriesSameDestination()
    {
        yield return Launch(8,true);QuietKitFixture();Run.Travel.State.enabled=true;
        // Hold only presentation startup while source death/board cleanup runs.
        Run.Travel.enabled=false;KillEncounter();
        yield return Until(()=>Run.Continuation.CanCapture,"source cleanup");
        Run.GetComponent<RunControlsUI>().OpenSettings();Run.Travel.enabled=true;
        Assert.That(Run.Continuation.SaveNow(),Is.True);
        string destination=Run.Travel.State.destination;byte[] durable=File.ReadAllBytes(path);
        using(var locked=new FileStream(path+".tmp",FileMode.Create,FileAccess.ReadWrite,FileShare.None))
        {
            LogAssert.Expect(LogType.Error,new System.Text.RegularExpressions.Regex("Could not save account"));
            Assert.That(Run.Continuation.TryCommitZoneTravel(destination),Is.False);
            Assert.That(File.ReadAllBytes(path),Is.EqualTo(durable));
            Assert.That(Run.Travel.State.stage,Is.EqualTo(1));Assert.That(Run.Travel.State.destination,Is.EqualTo(destination));
            Assert.That(Run.NeedsSaveRetry,Is.True);
        }
        Assert.That(Run.Continuation.SaveNow(),Is.True);Run.GetComponent<RunControlsUI>().Close();
        yield return Until(()=>Run?.Zone?.Definition?.zoneId==destination && Run.Travel.State.stage==0 && Run.Waves.IsWaveActive,"save retry travels once");
        Assert.That(Run.Travel.State.visit,Is.EqualTo(1));Assert.That(AccountProgression.Current.ActiveRun.completedWaves,Is.EqualTo(1));
    }
    [UnityTest] public IEnumerator ReducedMotionFailedPreparationReleasesInputAndCombatTime()
    {
        yield return Launch(1,true);QuietKitFixture();bool prior=PresentationPreferences.ReducedMotion;
        try
        {
            PresentationPreferences.SetReducedMotion(true);
            var view=ZoneTransitionView.Create(Run);Assert.That(view.TryPlay("Briarheart Wilds",()=>false),Is.True);
            yield return Until(()=>view.Phase=="Covered","crystal and smoke coverage");
            Assert.That(view.transform.Find("SplitStoryGem").GetComponent<RectTransform>().anchoredPosition,Is.EqualTo(Vector2.zero));
            yield return CaptureTravel("04-crystal-smoke");
            yield return Until(()=>ZoneTransitionView.Active==null,"failed preparation reveal");
            Assert.That(Time.timeScale,Is.EqualTo(1));Assert.That(Run.Board.IsExternalInputBlocked,Is.False);
            Assert.That(Run.Zone.Definition.zoneId,Is.EqualTo("magical-forest"));
        }
        finally {PresentationPreferences.SetReducedMotion(prior);}
    }
    [UnityTest] public IEnumerator OlderForestCheckpointKeepsItsIsolatedLoop()
    {
        yield return Launch(1,true);QuietKitFixture();
        var saved=Run.Continuation.Capture();saved.travel=null;
        var legacy=JsonUtility.FromJson<RunCombatSnapshot>(JsonUtility.ToJson(saved));
        Assert.That(legacy.travel==null || legacy.travel.version==0,Is.True,"missing travel does not become a live profile");
        Time.timeScale=0;Assert.That(AccountProgression.Current.StoreCheckpoint(Run.RunId,legacy),Is.True);
        SceneManager.LoadScene("Game");yield return null;
        yield return Until(()=>Run!=null && Run.InitialStateReady && !Run.Continuation.IsRestoring,"older forest restore");
        Assert.That(Run.Zone.Definition.zoneId,Is.EqualTo("magical-forest"));
        Assert.That(Run.Travel.State.enabled,Is.False);Assert.That(CombatMoveClock.MoveEffects,Is.True);
        Run.GetComponent<RunControlsUI>().Close();yield return Stable();
    }
    [UnityTest] public IEnumerator TravelSecondsProfileKeepsBuffPoisonStaggerAndSupplyUnits()
    {
        yield return Launch(1,true);QuietKitFixture();
        var saved=Run.Continuation.Capture();saved.clock.profile=CombatClockSnapshot.LegacyEffectsProfile;
        Time.timeScale=0;Assert.That(AccountProgression.Current.StoreCheckpoint(Run.RunId,saved),Is.True);
        SceneManager.LoadScene("Game");yield return null;
        yield return Until(()=>Run!=null && Run.InitialStateReady && !Run.Continuation.IsRestoring,"seconds effect profile restores");
        Run.GetComponent<RunControlsUI>().Close();yield return Stable();QuietKitFixture();
        Run.Player.TryTakeDamage(25);Assert.That(Run.TryUsePotion(),Is.True);
        Run.Player.GetComponent<PlayerAbilityEnergy>().AddEnergy(1000);
        Assert.That(Run.Player.GetComponent<PlayerAbilityController>().TryActivate(),Is.True);
        var enemy=Enemy("elven_scout");var stagger=enemy.GetComponent<EnemyStagger>();
        Set(stagger,"remainingImmunityTime",0f);stagger.ApplyStagger(2,2);
        (enemy.GetComponent<EnemyPoisonStatus>()??enemy.gameObject.AddComponent<EnemyPoisonStatus>()).Apply(5,1,5);
        var before=Run.Continuation.Capture();var status=before.enemies.First(e=>e.definition==enemy.Definition.name);
        float start=Time.time;yield return Until(()=>Time.time>=start+.4f,"seconds effects advance without accepted moves");
        var after=Run.Continuation.Capture();var later=after.enemies.First(e=>e.definition==enemy.Definition.name);
        Assert.That(Run.MoveClock.Tick,Is.EqualTo(before.clock.actions.completed));
        Assert.That(after.potionCooldown,Is.LessThan(before.potionCooldown-.2f));
        Assert.That(after.decreeRemaining,Is.LessThan(before.decreeRemaining-.2f));
        Assert.That(later.poisonRemaining,Is.LessThan(status.poisonRemaining-.2f));
        Assert.That(later.staggerRemaining,Is.LessThan(status.staggerRemaining-.2f));
    }
}
