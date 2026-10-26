using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

// Focused art-integration regressions. Existing legacy tests and their failures remain unchanged.
public sealed partial class ForestFoundationPlayTests
{
    const string CourtIntegrationEvidence="ArtSource/DrownedCourtEnvironmentV2/Integration";
    [UnityTest,Explicit] public IEnumerator CourtProductionWetResumePreservesCurrentDeadlineAndArt()
    {
        yield return LaunchCourt("moray_siphoner","shellback_porter");
        var ability=Enemy("moray_siphoner").GetComponent<AquaticEnemyAbility>();
        for(int i=0;i<8&&!ability.IsPreparing;i++)yield return Move();
        Assert.That(ability.IsPreparing,Is.True);int deadline=ability.ResponseMoves;Assert.That(deadline,Is.GreaterThan(0));
        var board=Run.Board;board.Aquatic.StartFlood(12,Run.MoveClock.Tick);board.Aquatic.air=2;
        var before=Run.Continuation.Capture();string id=Run.RunId;
        Assert.That(Run.SuspendToMenu(),Is.True);yield return null;yield return null;
        SceneManager.LoadScene("Game");yield return Stable();
        Assert.That(Run.RunId,Is.EqualTo(id));Assert.That(Run.Board.IsFlooded,Is.True);
        Assert.That(Run.Board.Aquatic.air,Is.EqualTo(2));Assert.That(Run.Board.Aquatic.wetMoves,Is.EqualTo(12));
        Assert.That(Run.MoveClock.Tick,Is.EqualTo(before.board.moves));AssertBoardCarryover(before.board,Run.Continuation.Capture().board);
        Assert.That(Enemy("moray_siphoner").GetComponent<AquaticEnemyAbility>().ResponseMoves,Is.EqualTo(deadline));
        Assert.That(Run.Board.Aquatic.coffer,Is.Null);
        Run.GetComponent<RunControlsUI>().Close();
        yield return Until(()=>Run.Waves.ActiveEnemies.All(e=>e.GetComponent<EnemyLifecycleVFX>()?.IsSpawning!=true),"restored portraits finish spawning");
        yield return new WaitForSeconds(.5f);
        Assert.That(GameObject.Find("ApprovedDrownedCourtEnvironment"),Is.Not.Null);
        Assert.That(UnityEngine.Object.FindFirstObjectByType<DrownedCourtAmbientMotion>().tracks.Count,Is.EqualTo(15));
        Directory.CreateDirectory(CourtIntegrationEvidence);
        string screenshot=Path.GetFullPath(CourtIntegrationEvidence+"/WetResume.png");ScreenCapture.CaptureScreenshot(screenshot);
        yield return Until(()=>File.Exists(screenshot),"wet gameplay screenshot saved");
        File.WriteAllText(CourtIntegrationEvidence+"/WetResume.txt",$"AIR=2; wetMoves=12; moveClock={Run.MoveClock.Tick}; preservedChannelDeadline={deadline}; exact board carryover; production environment active");
    }
    [UnityTest,Explicit] public IEnumerator CourtProductionTravelPreservesStateAndReleasesEnvironment()
    {
        using var destinations=new TravelDestinationFixture("dungeon","magical-forest","drowned-court");
        yield return LaunchCourt("shellback_porter");
        yield return Move();
        string id=Run.RunId;long largest=0;var artCounts=new Dictionary<string,int>();var receipt=new List<string>();
        string[] route={"dungeon","drowned-court","magical-forest","drowned-court","dungeon","magical-forest"};
        for(int hop=0;hop<18;hop++)
        {
            Run.Travel.enabled=false;Set(Run.Waves,"advanceWavesAutomatically",false);
            foreach(var enemy in Run.Waves.ActiveEnemies)enemy.GetComponent<EnemyAutoAttack>().SetActionPaused(this,true);
            KillEncounter();yield return Until(()=>!Run.Waves.IsWaveActive && Run.Continuation.CanCapture,"source encounter finishes");
            var draft=Run.Waves.GetComponent<RunUpgradeCoordinator>();
            var snapshot=Run.Continuation.Capture();
            if(snapshot.draft.Count>0){Assert.That(draft.SelectRecordedCard(snapshot.draft[0]),Is.True);yield return StableAfterCourtWave();}
            if(Run.Zone.Definition.zoneId=="drowned-court")
            {
                Run.Board.Aquatic.StartFlood(12,Run.Board.CompletedValidPlayerMoves);Run.Board.Aquatic.air=1;
                Run.Board.Aquatic.bubbles.Add(Run.Board.GetGem(0,0).BoardIdentity);
            }
            else if(Run.Zone.Definition.zoneId=="magical-forest")
            {Run.Board.QueueEnvironmentalVine(Run.Board.GetGem(0,0));yield return StableAfterCourtWave();}
            var before=Run.Continuation.Capture();string destination=route[hop%route.Length];
            int expectedEnergy=Mathf.Min(Run.Player.GetComponent<PlayerAbilityEnergy>().MaximumEnergy,
                before.player.energy+(RunUpgradeResolver.HasMechanic(RunUpgradeMechanic.PreparedCasting,RunUpgradeRuntime.Current)?15:0));
            Assert.That(destination,Is.Not.EqualTo(Run.Zone.Definition.zoneId));
            Run.Travel.State.stage=1;Run.Travel.State.destination=destination;
            Assert.That(Run.Continuation.TryCommitZoneTravel(destination),Is.True);
            SceneManager.LoadScene("Game");yield return null;yield return null;
            yield return Until(()=>Run?.InitialStateReady==true && !Run.Continuation.IsRestoring && Run.GetComponent<RunControlsUI>()!=null,"committed handoff restores");
            Run.GetComponent<RunControlsUI>().Close();yield return Stable();
            yield return Until(()=>Run.Travel.State.stage==0,"destination reveal finishes");
            Assert.That(Run.RunId,Is.EqualTo(id));Assert.That(Run.Zone.Definition.zoneId,Is.EqualTo(destination));
            Assert.That(Run.Board.IsFlooded,Is.False);Assert.That(Run.Board.VineCount,Is.Zero);
            Assert.That(Run.Board.GetComponent<BoardCasterSigilView>().ActiveCount,Is.Zero,"travel leaves no source-zone caster marks");
            var after=Run.Continuation.Capture();AssertBoardCarryover(before.board,after.board);
            Assert.That(after.player.health,Is.EqualTo(before.player.health));
            Assert.That(after.player.energy,Is.EqualTo(expectedEnergy),"one new destination wave may grant Prepared Casting once");
            if(destination=="drowned-court")
            {Assert.That(Run.Board.Aquatic.bubbles,Is.Empty);Assert.That(Run.Board.Aquatic.snares,Is.Empty);Assert.That(Run.Board.Aquatic.coffer,Is.Null);}
            Assert.That(Run.MoveClock.Capture().profile,Is.EqualTo(CombatClockSnapshot.UnifiedProfile));
            largest=Math.Max(largest,new System.IO.FileInfo(path).Length);
            // Different approved zones have different module counts. Compare each environment
            // to its own first visit, and require that all previous environment roots are gone.
            yield return null;yield return null;
            string selected=destination=="drowned-court"?"ApprovedDrownedCourtEnvironment":destination=="dungeon"?"DungeonPixelLabEnvironment":"ApprovedForestEnvironment";
            var roots=UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include,FindObjectsSortMode.None)
                .Where(c=>c.name=="ApprovedDrownedCourtEnvironment"||c.name=="DungeonPixelLabEnvironment"||c.name=="ApprovedForestEnvironment").ToArray();
            Assert.That(roots.Length,Is.EqualTo(1),"exactly one selected environment survives handoff");Assert.That(roots[0].name,Is.EqualTo(selected));
            int count=roots[0].GetComponentsInChildren<Transform>(true).Length;
            if(artCounts.TryGetValue(destination,out int previous))Assert.That(count,Is.EqualTo(previous),"no environment modules accumulate on return");else artCounts[destination]=count;
            Assert.That(UnityEngine.Object.FindObjectsByType<DrownedCourtAmbientMotion>(FindObjectsInactive.Include,FindObjectsSortMode.None).Length,Is.EqualTo(destination=="drowned-court"?1:0));
            Assert.That(UnityEngine.Object.FindObjectsByType<DungeonPresentationArt>(FindObjectsInactive.Include,FindObjectsSortMode.None).Length,Is.EqualTo(1));
            receipt.Add($"hop={hop+1}; zone={destination}; environmentObjects={count}; accountBytes={new FileInfo(path).Length}");
        }
        Assert.That(largest,Is.LessThan(500000),"eighteen handoffs keep this fixture's durable account below 500 KB");
        File.WriteAllText(CourtIntegrationEvidence+"/TravelSoak.txt",string.Join("\n",receipt)+"\n18 handoffs; largest account bytes="+largest);
    }

}
