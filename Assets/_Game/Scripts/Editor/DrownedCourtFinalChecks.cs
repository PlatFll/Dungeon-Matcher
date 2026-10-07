using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed partial class ForestFoundationPlayTests
{
    [UnityTest] public IEnumerator CourtLayoutMathAcrossSafeAreas()
    {GameplayPixelLayoutTests.MathTests();yield return null;}

    [UnityTest] public IEnumerator CourtQueenTelegraphAndNormalCadenceResumeWithoutNewTargets()
    {
        yield return TributeFixture("queen_nacre","shellback_porter");
        var queen=Enemy("queen_nacre");var ability=queen.GetComponent<AquaticEnemyAbility>();
        Assert.That(ability.CastName,Is.EqualTo("TRIBUTE"));
        var marks=ability.MarkedBubbles.ToArray();int tick=Run.MoveClock.Tick;
        var identity=queen.PersistentId;var target=marks[0];
        // Preserve collected oxygen and remaining physical targets through a real scene reload.
        var gem=Run.Board.FindAquaticGem(target);var clear=new HashSet<Gem>{gem};
        Call(Run.Board,"RegisterAquaticClear",clear,false);Call(Run.Board,"ResolveAquaticDestruction",clear,new HashSet<Gem>());
        Assert.That(ability.MarkedBubbles.Count,Is.EqualTo(2));
        yield return ResumeCourtCheckpoint();
        queen=Enemy("queen_nacre");ability=queen.GetComponent<AquaticEnemyAbility>();
        Assert.That(queen.PersistentId,Is.EqualTo(identity));Assert.That(Run.MoveClock.Tick,Is.EqualTo(tick));
        Assert.That(ability.CastName,Is.EqualTo("TRIBUTE"));Assert.That(ability.ResponseMoves,Is.EqualTo(3));
        CollectionAssert.AreEqual(marks.Skip(1),ability.MarkedBubbles);
        Assert.That(queen.SpecialIdleState,Is.EqualTo("ChannelHold"));Assert.That(ability.BlocksBasic,Is.True);
        for(int i=0;i<3;i++)yield return Move();
        Assert.That(((AquaticEnemySnapshot)Get(ability,"state")).stage,Is.Zero);
        Assert.That(ability.BlocksBasic,Is.False);Assert.That(queen.CurrentSpecialTurnCount,Is.Zero);
        yield return ResumeCourtCheckpoint();
        ability=Enemy("queen_nacre").GetComponent<AquaticEnemyAbility>();
        Assert.That(((AquaticEnemySnapshot)Get(ability,"state")).stage,Is.Zero);
        Assert.That(ability.BlocksBasic,Is.False);
        yield return Move();Assert.That(Enemy("queen_nacre").CurrentSpecialTurnCount,Is.EqualTo(1));
    }

    private IEnumerator ResumeCourtCheckpoint()
    {
        string id=Run.RunId;Assert.That(Run.SuspendToMenu(),Is.True);yield return null;yield return null;
        SceneManager.LoadScene("Game");yield return null;yield return null;
        yield return Until(()=>Run?.InitialStateReady==true && !Run.Continuation.IsRestoring,"Court checkpoint restores");
        Assert.That(Run.RunId,Is.EqualTo(id));Run.GetComponent<RunControlsUI>().Close();yield return Stable();
        foreach(var actor in Run.Waves.ActiveEnemies)actor.GetComponent<EnemyAutoAttack>().SetActionPaused(this,true);
        Set(Run.Waves,"advanceWavesAutomatically",false);
    }

    [UnityTest] public IEnumerator CourtWaterPresentationHasBoundedObjectsAndMeasuredCost()
    {
        yield return LaunchCourt("shellback_porter");yield return Move();
        Run.Board.Aquatic.StartFlood(12,Run.MoveClock.Tick);Call(Run.Board,"EnsureAquaticSupply",3);
        float rise=Time.time+.5f;yield return Until(()=>Time.time>=rise,"profile stable flooded view");
        var view=Run.Board.GetComponent<AquaticEnvironmentView>();
        Assert.That(view.enabled,Is.True);
        Assert.That(((System.Collections.IDictionary)Get(view,"overlays")).Count,Is.GreaterThan(0),"profile includes active bubble overlays");
        var update=(Action)Delegate.CreateDelegate(typeof(Action),view,typeof(AquaticEnvironmentView).GetMethod("LateUpdate",Flags));
        for(int i=0;i<20;i++)update();
        long counterBefore=GC.GetAllocatedBytesForCurrentThread();var probe=new byte[8192];
        bool allocationCounter=GC.GetAllocatedBytesForCurrentThread()>counterBefore;GC.KeepAlive(probe);
        int objects=view.transform.childCount;long allocated=GC.GetAllocatedBytesForCurrentThread();
        var clock=Stopwatch.StartNew();for(int i=0;i<120;i++)update();clock.Stop();
        allocated=GC.GetAllocatedBytesForCurrentThread()-allocated;
        Assert.That(view.transform.childCount,Is.EqualTo(objects),"stable water does not create a new object per frame or gem");
        File.WriteAllText(".utmp/DrownedCourtValidation/PresentationProfile.json",
            "{\"scope\":\"120 warmed AquaticEnvironmentView.LateUpdate calls in Windows Editor; CPU and managed allocations only, not device/GPU performance\","+
            "\"calls\":120,\"total_ms\":"+clock.Elapsed.TotalMilliseconds.ToString("F4",CultureInfo.InvariantCulture)+
            ",\"allocated_bytes\":"+(allocationCounter?allocated.ToString():"null")+",\"allocation_counter_available\":"+allocationCounter.ToString().ToLowerInvariant()+
            ",\"board_children_before\":"+objects+",\"board_children_after\":"+view.transform.childCount+"}");
    }

    [UnityTest] public IEnumerator DungeonPlayerControlRunsRecordActualOutcomes(){yield return PlayerControlRuns("dungeon");}
    [UnityTest] public IEnumerator ForestPlayerControlRunsRecordActualOutcomes(){yield return PlayerControlRuns("magical-forest");}
    [UnityTest] public IEnumerator CourtPlayerControlRunsRecordActualOutcomes(){yield return PlayerControlRuns("drowned-court");}

    private IEnumerator PlayerControlRuns(string zone)
    {
        string output=".utmp/DrownedCourtValidation/ControlRuns-"+zone+".csv";
        File.WriteAllText(output,"zone,player,level,seed,outcome,game_seconds,waves,moves,abilities,floods,low_air_observations,suspends\n");
        var pacing=typeof(BalancePacingValidation);
        var move=pacing.GetMethod("Move",BindingFlags.NonPublic|BindingFlags.Static);
        var owner=pacing.GetField("run",BindingFlags.NonPublic|BindingFlags.Static);
        foreach(string player in new[]{"skeleton","bardley","gideon_glass"})
        foreach(int level in new[]{1,5})
        {
            profile?.Dispose();character?.Dispose();profile=character=null;
            path=Path.GetFullPath(".utmp/ForestTestProfiles/"+Guid.NewGuid().ToString("N")+".json");
            var account=new AccountSave();account.characters.Add(new CharacterProgress{id=player,level=level});
            File.WriteAllText(path,JsonUtility.ToJson(account));profile=AccountProgression.UseDisposableProfile(path);
            character=CharacterSelectionSettings.UseTemporarySelection(player);
            UnityEngine.Random.InitState(3101);RunLaunchOptions.StartingZone=zone;Time.timeScale=6;
            SceneManager.LoadScene("Game");yield return null;yield return null;yield return Stable();
            Assert.That(Run.Player.Definition.PlayerId,Is.EqualTo(player));
            Assert.That(Run.Zone.Definition.zoneId,Is.EqualTo(zone));owner.SetValue(null,Run);
            var controller=Run.Player.GetComponent<PlayerAbilityController>();
            float began=Time.time,next=began+1.8f,lastInput=Time.realtimeSinceStartup;int abilities=0,low=0,seenMove=-1,suspends=0;
            while(!Run.IsFinished && Time.time-began<60)
            {
                Assert.That(Time.realtimeSinceStartup-lastInput,Is.LessThan(40),"synthetic input remains responsive: "+zone+"/"+player+"/"+level);
                var draft=UnityEngine.Object.FindFirstObjectByType<UpgradeChoiceUI>();
                if(draft?.IsOpen==true && Run.Continuation.CanCapture)
                {
                    var state=Run.Continuation.Capture();Assert.That(state.draft.Count,Is.GreaterThan(0));
                    Assert.That(Run.Waves.GetComponent<RunUpgradeCoordinator>().SelectRecordedCard(state.draft[0]),Is.True);
                    Time.timeScale=6;lastInput=Time.realtimeSinceStartup;yield return null;continue;
                }
                if(Run.Continuation.CanCapture && Run.Waves.IsWaveActive && suspends==0 && Time.time-began>=25)
                {
                    int tick=Run.Board.CompletedValidPlayerMoves;string id=Run.RunId;
                    var before=Run.Continuation.Capture();
                    Assert.That(Run.SuspendToMenu(),Is.True);yield return null;yield return null;
                    SceneManager.LoadScene("Game");yield return null;yield return null;
                    yield return Until(()=>Run?.InitialStateReady==true && !Run.Continuation.IsRestoring,"control run resumes");
                    Assert.That(Run.RunId,Is.EqualTo(id));Assert.That(Run.Board.CompletedValidPlayerMoves,Is.EqualTo(tick));
                    Assert.That(Run.Player.CurrentHealth,Is.EqualTo(before.player.health));
                    Run.GetComponent<RunControlsUI>().Close();Time.timeScale=6;owner.SetValue(null,Run);
                    controller=Run.Player.GetComponent<PlayerAbilityController>();suspends++;lastInput=Time.realtimeSinceStartup;
                }
                if(Time.time>=next && !Run.Board.IsBusy && !Run.Board.IsExternalInputBlocked && Run.Waves.IsWaveActive)
                {
                    bool acted=controller.CanActivate && controller.TryActivate();if(acted)abilities++;
                    if(!acted)acted=(bool)move.Invoke(null,new object[]{Run.Board,true});
                    if(acted){next=Time.time+1.8f;lastInput=Time.realtimeSinceStartup;}
                }
                if(seenMove!=Run.Board.CompletedValidPlayerMoves)
                {seenMove=Run.Board.CompletedValidPlayerMoves;if(Run.Board.IsFlooded && Run.Board.Aquatic.air<=1)low++;}
                yield return null;
            }
            string outcome=Run.IsFinished?"death":"censored_60s";
            File.AppendAllText(output,string.Join(",",zone,player,level,3101,outcome,(Time.time-began).ToString("F2",CultureInfo.InvariantCulture),
                AccountProgression.Current.ActiveRun?.completedWaves??AccountProgression.Current.LastReward.waves,
                Run.Board.CompletedValidPlayerMoves,abilities,Run.Board.Aquatic?.floodCount??0,low,suspends)+"\n");
            // A censored measurement ends through the supported End Run path.
            // Waiting for another capture without supplying further input can
            // leave a card/response window intentionally paused indefinitely.
            Assert.That(Run.ExitTo("MainMenu"),Is.True);Time.timeScale=1;yield return null;yield return null;
        }
    }
}
