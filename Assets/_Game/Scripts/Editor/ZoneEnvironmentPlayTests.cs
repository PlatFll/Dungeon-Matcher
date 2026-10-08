using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed partial class ForestFoundationPlayTests
{
    private IEnumerator EnvironmentMove()
    {
        PrepareSafeMove();var board=Run.Board;int before=board.CompletedValidPlayerMoves;
        board.StartCoroutine((IEnumerator)Call(board,"TrySwap",board.GetGem(safeMoveFrom.x,safeMoveFrom.y),board.GetGem(safeMoveTo.x,safeMoveTo.y)));
        yield return Until(()=>board.CompletedValidPlayerMoves==before+1 && Run.Continuation.CanCapture,"environment move settles");
    }
    private IEnumerator LaunchDungeonHazards(bool legacy = false)
    {
        RunLaunchOptions.ForestPrototype=false;RunLaunchOptions.StartingZone="dungeon";
        SceneManager.LoadScene("Game");yield return Stable();
        if (legacy) yield return LegacyCombatTestProfile.Load();
        PreserveRoster();
        // Keep a real useful response available after the fixture's controlled
        // non-damaging swaps; the production hazard must still pass its checks.
        preserveCounterplayCrystal=true;
        Run.Board.GetGem(Run.Board.Width-1,0).SetSpecialType(GemSpecialType.ColorCrystal);
    }
    private static IEnumerator CaptureEnvironment(string name)
    {
        string dir=Path.GetFullPath(".utmp/ForestValidation/ZoneEnvironment");Directory.CreateDirectory(dir);
        yield return null;ScreenCapture.CaptureScreenshot(Path.Combine(dir,name+".png"));yield return null;yield return null;
    }

    [UnityTest] public IEnumerator EnvironmentVinesSurviveWavesDraftPauseAndContinue()
    {
        yield return Launch(1,true);QuietKitFixture();
        Run.Board.QueueEnvironmentalVine(Run.Board.GetGem(0,0));yield return Stable();
        KillEncounter();yield return Until(()=>Run.Waves.CurrentWave==2 && Run.Continuation.CanCapture,"next wave");
        Assert.That(Run.Board.VineCount,Is.EqualTo(1));QuietKitFixture();KillEncounter();
        var coordinator=Run.Waves.GetComponent<RunUpgradeCoordinator>();
        yield return Until(()=>coordinator.IsBlockingWaveProgression && Run.Continuation.CanCapture,"perk selection");
        Assert.That(Run.Board.VineCount,Is.EqualTo(1));Assert.That(Run.Board.IsCellVined(0,0),Is.True);
        yield return CaptureEnvironment("forest-vines-during-perks");
        Assert.That(Run.SuspendToMenu(),Is.True);yield return null;
        SceneManager.LoadScene("Game");yield return null;
        yield return Until(()=>Run?.Continuation!=null && !Run.Continuation.IsRestoring && Run.Continuation.CanCapture,"draft restores");
        Assert.That(Run.Board.IsCellVined(0,0),Is.True,"draft resume retains overlays");
        Run.GetComponent<RunControlsUI>().Close();
        coordinator=Run.Waves.GetComponent<RunUpgradeCoordinator>();
        Assert.That(coordinator.SelectRecordedCard(Run.Continuation.Capture().draft[0]),Is.True);
        yield return Stable();Assert.That(Run.Board.VineCount,Is.EqualTo(1));
    }

    [UnityTest] public IEnumerator EnvironmentTravelCleansVinesBeforeDestinationReveal()
    {
        using var destinations=new TravelDestinationFixture("dungeon","magical-forest");
        yield return Launch(8,true);QuietKitFixture();Run.Travel.State.enabled=true;
        Run.Board.QueueEnvironmentalVine(Run.Board.GetGem(0,0));yield return Stable();
        var before=Run.Continuation.Capture().board;Assert.That(before.vines.Count,Is.EqualTo(1));
        KillEncounter();
        yield return Until(()=>Run?.Zone?.Definition?.zoneId=="dungeon" && Run.Continuation.CanCapture && Run.Travel.State.stage==2,"covered destination");
        Assert.That(Run.Board.VineCount,Is.Zero);Assert.That(Run.Board.transform.Cast<Transform>().Any(t=>t.name.StartsWith("VineOverlay_")&&t.gameObject.activeSelf),Is.False);
        AssertBoardCarryover(before,Run.Continuation.Capture().board);
        yield return Until(()=>Run.Travel.State.stage==0 && Run.Waves.IsWaveActive,"reveal finishes");
        Assert.That(Run.Board.VineCount,Is.Zero);yield return CaptureEnvironment("clean-dungeon-arrival");
    }

    [UnityTest] public IEnumerator EnvironmentCrumblePausesAndReturnsAfterTwoMovesAcrossResume()
    {
        yield return LaunchDungeonHazards();
        for(int i=0;i<3;i++) yield return EnvironmentMove();
        IReadOnlyList<Vector2Int> cells=null;
        Run.Board.CellsShaking+=(value,_)=>cells=value;
        PrepareSafeMove();var board=Run.Board;
        board.StartCoroutine((IEnumerator)Call(board,"TrySwap",board.GetGem(safeMoveFrom.x,safeMoveFrom.y),board.GetGem(safeMoveTo.x,safeMoveTo.y)));
        yield return Until(()=>cells!=null,"shake starts");
        Assert.That(cells.Count,Is.InRange(1,2));
        var first=cells[0];var tile=board.transform.Find($"CellTiles/CellTile_{first.x}_{first.y}");
        yield return new WaitForSeconds(.08f);Run.GetComponent<RunControlsUI>().OpenSettings();
        var position=tile.localPosition;var gemPosition=board.GetGem(first.x,first.y).transform.localPosition;
        float start=Time.realtimeSinceStartup;yield return Until(()=>Time.realtimeSinceStartup>start+.2f,"paused shake");
        Assert.That(tile.localPosition,Is.EqualTo(position));Assert.That(board.GetGem(first.x,first.y).transform.localPosition,Is.EqualTo(gemPosition));
        Run.GetComponent<RunControlsUI>().Close();yield return Stable();
        var broken=Run.Continuation.Capture().board;
        Assert.That(broken.cells.Count(c=>c.crumbleRestoreMove==6),Is.EqualTo(cells.Count));
        foreach(var cell in cells) {Assert.That(board.GetGem(cell.x,cell.y),Is.Null);Assert.That(board.IsCellPlayable(cell.x,cell.y),Is.False);}
        Assert.That(tile.localPosition,Is.EqualTo(board.GetCellLocalPosition(first.x,first.y)));
        yield return CaptureEnvironment("dungeon-broken-tiles");
        yield return EnvironmentMove();Assert.That(Run.Continuation.Capture().board.cells.Count(c=>c.crumbleRestoreMove>0),Is.EqualTo(cells.Count));
        yield return ResumeRoster();board=Run.Board;
        Assert.That(board.CompletedValidPlayerMoves,Is.EqualTo(5));
        foreach(var cell in cells) Assert.That(board.IsCellMined(cell.x,cell.y),Is.True);
        yield return EnvironmentMove();
        foreach(var cell in cells)
        {
            Assert.That(board.IsCellPlayable(cell.x,cell.y),Is.True);Assert.That(board.GetGem(cell.x,cell.y),Is.Not.Null);
            Assert.That(board.transform.Find($"CellTiles/CellTile_{cell.x}_{cell.y}").GetComponent<SpriteRenderer>().enabled,Is.True);
        }
        Assert.That(Run.Continuation.Capture().board.cells.Any(c=>c.crumbleRestoreMove>0),Is.False);
        yield return CaptureEnvironment("dungeon-tiles-returned");
    }

    [UnityTest] public IEnumerator EnvironmentCrumbleRespectsSpecialsPinsStructuresAndUsefulMoves()
    {
        yield return LaunchDungeonHazards();PrepareSafeMove();var board=Run.Board;
        int specialId=board.GetGem(board.Width-1,0).BoardIdentity;
        var owner=Run.Waves.ActiveEnemies[0];
        Assert.That(board.TryQueueTopUpMovablePins(owner,3,null,null),Is.True);yield return Stable();
        var pins=Run.Continuation.Capture().board.cells.Where(c=>c.pinned).Select(c=>c.identity).ToArray();
        board.QueueZoneEnvironment(4);yield return Stable();
        var after=Run.Continuation.Capture().board;
        Assert.That(after.cells.Any(c=>c.identity==specialId&&c.special==GemSpecialType.ColorCrystal),Is.True);
        Assert.That(after.cells.Where(c=>c.pinned).Select(c=>c.identity),Is.EquivalentTo(pins));
        Assert.That(after.cells.Count(c=>c.crumbleRestoreMove>0),Is.InRange(1,2));
        Assert.That(board.GetImmediateResponses().Count,Is.GreaterThan(0));
        // Environmental holes themselves never report rewardable clears;
        // independently formed refill cascades retain the established semantics.
        Assert.That(board.IsBusy,Is.False);
    }

    [UnityTest] public IEnumerator EnvironmentPhotoKeepsCurrentHoleDeadlinesAndCannotResurrectReturnedHoles()
    {
        yield return LaunchDungeonHazards();var board=Run.Board;
        var memory=board.CaptureBoardMemory(Run.Waves.ContinuationOwnerSlot);
        for(int i=0;i<4;i++) yield return EnvironmentMove();
        var holes=board.CaptureBoardMemory(Run.Waves.ContinuationOwnerSlot);
        Assert.That(holes.cells.Any(c=>c.crumbleRestoreMove==6),Is.True);
        Assert.That(board.TryRestoreBoardMemory(memory,Run.Waves.ContinuationEnemy),Is.True);
        Assert.That(Run.Continuation.Capture().board.cells.Count(c=>c.crumbleRestoreMove==6),Is.EqualTo(holes.cells.Count(c=>c.crumbleRestoreMove==6)));
        yield return EnvironmentMove();yield return EnvironmentMove();
        Assert.That(board.TryRestoreBoardMemory(holes,Run.Waves.ContinuationEnemy),Is.True);
        Assert.That(Run.Continuation.Capture().board.cells.Any(c=>c.crumbleRestoreMove>0),Is.False);
        Assert.That(board.CompletedValidPlayerMoves,Is.EqualTo(6));
    }

    [UnityTest] public IEnumerator EnvironmentTravelSnapshotRestoresZoneHolesWithoutChangingSourceOnWriteFailure()
    {
        yield return LaunchDungeonHazards();var board=Run.Board;
        for(int i=0;i<4;i++) yield return EnvironmentMove();
        Run.Travel.enabled=false;Run.Travel.State.stage=1;Run.Travel.State.destination="magical-forest";
        KillEncounter();yield return Until(()=>Run.Continuation.CanCapture,"source settles");
        var before=Run.Continuation.Capture().board;
        Assert.That(before.cells.Count(c=>c.crumbleRestoreMove>0),Is.InRange(1,2));
        var cleaned=JsonUtility.FromJson<BoardCombatSnapshot>(JsonUtility.ToJson(before));
        board.PrepareZoneArrival(cleaned,Resources.Load<ZoneDefinition>("Zones/magical-forest"));
        Assert.That(cleaned.cells.Any(c=>c.crumbleRestoreMove>0||c.mined),Is.False);
        Assert.That(cleaned.cells.All(c=>c.hasGem),Is.True);
        Assert.That(JsonUtility.ToJson(Run.Continuation.Capture().board),Is.EqualTo(JsonUtility.ToJson(before)));
        Assert.That(Run.Continuation.SaveNow(),Is.True);byte[] durable=File.ReadAllBytes(path);
        using(var locked=new FileStream(path+".tmp",FileMode.Create,FileAccess.ReadWrite,FileShare.None))
        {
            LogAssert.Expect(LogType.Error,new System.Text.RegularExpressions.Regex("Could not save account"));
            Assert.That(Run.Continuation.TryCommitZoneTravel("magical-forest"),Is.False);
            Assert.That(File.ReadAllBytes(path),Is.EqualTo(durable));
            Assert.That(JsonUtility.ToJson(Run.Continuation.Capture().board),Is.EqualTo(JsonUtility.ToJson(before)));
        }
        Assert.That(Run.Continuation.TryCommitZoneTravel("magical-forest"),Is.True);
        SceneManager.LoadScene("Game");yield return null;
        yield return Until(()=>Run?.Zone?.Definition?.zoneId=="magical-forest" && Run.Continuation.CanCapture,"clean forest restore");
        Assert.That(Run.Continuation.Capture().board.cells.Any(c=>c.crumbleRestoreMove>0||c.mined),Is.False);
    }

    private IEnumerator SummonArbalist()
    {
        yield return LaunchDungeonHazards();
        var data=AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Game/Data/Enemies/Enemy_RoyalArbalist.asset");
        Assert.That(Run.Waves.TrySummonEnemy(data,out var actor),Is.True);yield return Stable();PreserveRoster();
        Set(Run.Board,"nextCrumbleMove",999);
    }

    [UnityTest] public IEnumerator EnvironmentRoyalChainShotHasTwoAuthoredContactsAndReleasesOnDeath()
    {
        yield return SummonArbalist();var actor=Enemy("royal_arbalist");
        Assert.That(actor.Definition.Category,Is.EqualTo(EnemyCategory.Special));
        Assert.That(actor.GetComponent<CrossbowGuardEnemyAbility>(),Is.Not.Null);
        var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>(CombatActionImporter.AnimationRoot+"/RoyalArbalist_Ability.anim");
        Assert.That(clip.events.Where(e=>e.functionName=="AbilityBeat").Select(e=>e.intParameter),Is.EqualTo(new[]{1,2}));
        Assert.That(clip.events.Any(e=>e.functionName=="AutoAttackImpact"),Is.False);
        for(int i=0;i<4;i++) yield return EnvironmentMove();
        Assert.That(Run.Board.GetPinnedGemCountForOwner(actor.GetInstanceID()),Is.EqualTo(2));
        Assert.That(actor.HasAnimationActionInProgress,Is.False);
        yield return CaptureEnvironment("royal-arbalist-two-chains");
        actor.ResolveDamageWithoutFeedback(999999);yield return Stable();
        Assert.That(Run.Board.RestrictionCount,Is.Zero);
    }

    [UnityTest] public IEnumerator EnvironmentRoyalChainShotDeathBetweenContactsCancelsSecondBolt()
    {
        yield return SummonArbalist();var actor=Enemy("royal_arbalist");var board=Run.Board;
        for(int i=0;i<3;i++) yield return EnvironmentMove();
        PrepareSafeMove();board.StartCoroutine((IEnumerator)Call(board,"TrySwap",board.GetGem(safeMoveFrom.x,safeMoveFrom.y),board.GetGem(safeMoveTo.x,safeMoveTo.y)));
        yield return Until(()=>board.GetPinnedGemCountForOwner(actor.GetInstanceID())==1,"first chain contact");
        Run.GetComponent<RunControlsUI>().OpenSettings();
        float start=Time.realtimeSinceStartup;yield return Until(()=>Time.realtimeSinceStartup>start+.2f,"paused chain shot");
        Assert.That(board.GetPinnedGemCountForOwner(actor.GetInstanceID()),Is.EqualTo(1));
        Run.GetComponent<RunControlsUI>().Close();actor.ResolveDamageWithoutFeedback(999999);
        yield return Stable();Assert.That(board.RestrictionCount,Is.Zero);
        Assert.That(board.HasPendingBoardMutation,Is.False);
    }

    [UnityTest] public IEnumerator EnvironmentRoyalChainShotDisableReleasesExistingChains()
    {
        yield return SummonArbalist();var actor=Enemy("royal_arbalist");var board=Run.Board;
        for(int i=0;i<4;i++) yield return EnvironmentMove();
        Assert.That(board.GetPinnedGemCountForOwner(actor.GetInstanceID()),Is.EqualTo(2));
        actor.GetComponent<CrossbowGuardEnemyAbility>().enabled=false;
        yield return Stable();Assert.That(board.RestrictionCount,Is.Zero);
        Assert.That(actor.IsDefeated,Is.False);Assert.That(actor.HasAnimationActionInProgress,Is.False);
    }

    [UnityTest] public IEnumerator EnvironmentCrumbleSharesMinerCapacityAndRestoresOnlyItsOwnCells()
    {
        yield return LaunchDungeonHazards();PrepareSafeMove();var board=Run.Board;var owner=Run.Waves.ActiveEnemies[0];
        for(int i=0;i<3;i++)
        {Assert.That(board.TryQueueMineRandomCell(owner,3),Is.True);yield return Stable();}
        Assert.That(board.GetMinedCellCountForOwner(owner.GetInstanceID()),Is.EqualTo(3));
        bool prior=PresentationPreferences.ReducedMotion;
        try
        {
            PresentationPreferences.SetReducedMotion(true);
            board.CellMiningStarted+=(x,y,duration)=>{if(duration>0) board.StartCoroutine(CaptureEnvironment("dungeon-white-break"));};
            board.CellMaterializing+=(x,y,duration)=>board.StartCoroutine(CaptureEnvironment("dungeon-white-return"));
            for(int i=0;i<4;i++) yield return EnvironmentMove();
            Assert.That(Run.Continuation.Capture().board.cells.Count(c=>c.mined),Is.EqualTo(4));
            Assert.That(Run.Continuation.Capture().board.cells.Count(c=>c.crumbleRestoreMove>0),Is.EqualTo(1));
            yield return EnvironmentMove();yield return EnvironmentMove();
            Assert.That(board.GetMinedCellCountForOwner(owner.GetInstanceID()),Is.EqualTo(3));
            Assert.That(Run.Continuation.Capture().board.cells.Any(c=>c.crumbleRestoreMove>0),Is.False);
            board.QueueRestoreMinedCells(owner.GetInstanceID());yield return Stable();
            Assert.That(Run.Continuation.Capture().board.cells.Any(c=>c.mined),Is.False);
        }
        finally {PresentationPreferences.SetReducedMotion(prior);}
    }
}
