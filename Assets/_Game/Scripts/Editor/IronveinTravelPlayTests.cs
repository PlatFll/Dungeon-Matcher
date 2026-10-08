using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed partial class ForestFoundationPlayTests
{
    [UnityTest] public IEnumerator IronveinNaturalLessonAndEncounterHistoryContinueOnce()
    {yield return MineNaturalContinue();}
    private IEnumerator MineNaturalContinue()
    {
        yield return LaunchMine(true);QuietKitFixture();
        Assert.That(Run.Travel.State.mineEncounters.seen.Count,Is.GreaterThan(0),"actual wave start records history");
        Set(Run.Waves,"advanceWavesAutomatically",false);KillEncounter();
        yield return Until(()=>!Run.Waves.IsWaveActive && Run.Continuation.CanCapture,"entrance clears");
        Set(Run.Waves,"currentWave",2);Run.Waves.SpawnCurrentWave();yield return Stable();QuietKitFixture();
        yield return EnvironmentMove();
        Assert.That(Run.Board.Mine.naturalStoneIntroduced,Is.True);
        Assert.That(Run.Board.MineStoneCount,Is.EqualTo(1));
        var stone=Run.Board.MineStoneTargets().Single();Assert.That(stone.State.ownerId,Is.Zero);
        string history=JsonUtility.ToJson(Run.Travel.State.mineEncounters);
        yield return ResumeRoster();
        Assert.That(JsonUtility.ToJson(Run.Travel.State.mineEncounters),Is.EqualTo(history));
        Assert.That(Run.Board.Mine.naturalStoneIntroduced,Is.True);
        Assert.That(Run.Board.MineStoneTargets().Single().State.id,Is.EqualTo(stone.State.id));
        yield return EnvironmentMove();Assert.That(Run.Board.MineStoneCount,Is.EqualTo(1),"no second natural lesson on resume");
    }

    [UnityTest] public IEnumerator IronveinFourZoneTravelCleansSourceAndPreservesResources()
    {yield return MineFourZoneTravel();}
    private IEnumerator MineFourZoneTravel()
    {
        using var destinations=new TravelDestinationFixture("dungeon","magical-forest","drowned-court","ironvein-excavation");
        yield return LaunchMine();string runId=Run.RunId;
        Run.Player.GetComponent<PlayerAbilityEnergy>().AddEnergy(100);
        Assert.That(Run.Player.GetComponent<PlayerAbilityController>().TryActivate(),Is.True);
        Assert.That(Run.Player.GetComponent<RoyalDecreeRuntime>().RemainingMoves,Is.EqualTo(5));
        string[] route={"magical-forest","drowned-court","dungeon","ironvein-excavation","drowned-court","ironvein-excavation"};
        long largest=0;
        foreach(string destination in route)
        {
            Run.Travel.enabled=false;Set(Run.Waves,"advanceWavesAutomatically",false);QuietKitFixture();
            if(Run.Board.Mine!=null)
            {
                Run.Board.TryQueuePlaceMineStones(Run.Waves.ActiveEnemies[0],2,6,MineStoneStage.Obsidian);yield return Stable();
                Run.Board.TryQueueMineDrillPower(1,2);yield return Stable();
            }
            KillEncounter();yield return Until(()=>!Run.Waves.IsWaveActive && Run.Continuation.CanCapture,"source death settles");
            var draft=Run.Continuation.Capture().draft;
            if(draft.Count>0)
            {Assert.That(Run.Waves.GetComponent<RunUpgradeCoordinator>().SelectRecordedCard(draft[0]),Is.True);yield return StableAfterCourtWave();}
            if(Run.Zone.Definition.growsVines)
            {Run.Board.QueueEnvironmentalVine(Run.Board.GetGem(0,0));yield return StableAfterCourtWave();}
            if(Run.Zone.Definition.periodicallyFloods)
            {Run.Board.Aquatic.StartFlood(12,Run.Board.CompletedValidPlayerMoves);Run.Board.Aquatic.air=1;}
            var before=Run.Continuation.Capture();
            Run.Travel.State.stage=1;Run.Travel.State.destination=destination;
            Assert.That(Run.Continuation.TryCommitZoneTravel(destination),Is.True);
            // A successful detached commit still leaves the source view untouched.
            Assert.That(Run.Board.MineStoneCount,Is.EqualTo(before.board.cells.Count(c=>c.mineStone!=null)));
            SceneManager.LoadScene("Game");yield return null;yield return null;
            yield return Until(()=>Run?.InitialStateReady==true && !Run.Continuation.IsRestoring,"destination loads");
            Run.GetComponent<RunControlsUI>().Close();yield return Stable();
            yield return Until(()=>Run.Travel.State.stage==0,"smoke clears");
            Assert.That(Run.RunId,Is.EqualTo(runId));Assert.That(Run.Zone.Definition.zoneId,Is.EqualTo(destination));
            Assert.That(Run.Board.MineStoneCount,Is.Zero);Assert.That(Run.Board.VineCount,Is.Zero);Assert.That(Run.Board.IsFlooded,Is.False);
            Assert.That(Run.Board.GetComponent<BoardCasterSigilView>().ActiveCount,Is.Zero);
            var after=Run.Continuation.Capture();Assert.That(after.player.health,Is.EqualTo(before.player.health));
            Assert.That(after.clock.profile,Is.EqualTo(CombatClockSnapshot.UnifiedProfile));
            Assert.That(after.decreeRemaining,Is.EqualTo(before.decreeRemaining));
            Assert.That(after.decreeRemaining,Is.EqualTo(5),"travel consumes no Decree moves");
            Assert.That(after.player.shield,Is.EqualTo(before.player.shield));
            foreach(var old in before.board.cells.Where(c=>c.hasGem))
            {
                var next=after.board.cells.Single(c=>c.x==old.x && c.y==old.y);
                Assert.That(next.identity,Is.EqualTo(old.identity));Assert.That(next.special,Is.EqualTo(old.special));Assert.That(next.type,Is.EqualTo(old.type));
            }
            Assert.That(after.travel.mineEncounters.lastMilestoneLocalWave,Is.Zero,"new visit history");
            if(destination=="ironvein-excavation")
            {Assert.That(Run.Board.Mine.naturalStoneIntroduced,Is.False);Assert.That(Run.Board.Mine.drills.All(d=>d.charge==0),Is.True);}
            else Assert.That(Run.Board.Mine,Is.Null);
            largest=System.Math.Max(largest,new FileInfo(path).Length);
        }
        Assert.That(largest,Is.LessThan(500000));
        Directory.CreateDirectory(".utmp/Ironvein");File.WriteAllText(".utmp/Ironvein/FourZoneTravel.txt",
            "6 actual handoffs across 4 zones; source structures and charge cleaned; run/resources/gems retained; largest account bytes="+largest);
    }
}
