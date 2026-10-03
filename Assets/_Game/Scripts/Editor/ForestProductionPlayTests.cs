using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed partial class ForestFoundationPlayTests
{
    private void QuietKitFixture()
    {
        // Isolate move-owned kit outcomes from unrelated timed basic hits and
        // random cascade stagger. Explicit interruption tests remove immunity.
        foreach(var enemy in Run.Waves.ActiveEnemies)
        {
            Set(enemy.GetComponent<EnemyAutoAttack>(),"remainingAttackTime",999f);
            Set(enemy.GetComponent<EnemyStagger>(),"remainingImmunityTime",99f);
        }
    }
    [UnityTest] public IEnumerator ProductionMusicUsesOnePlayerCrossfadesPausesAndHonorsSettings()
    {
        bool muted=AudioPreferences.MusicMuted;
        try
        {
            yield return Launch(secondsBasics:true);QuietKitFixture();
            var music=BackgroundMusicPlayer.Instance;Assert.That(music,Is.Not.Null);
            Assert.That(UnityEngine.Object.FindObjectsByType<BackgroundMusicPlayer>(FindObjectsSortMode.None).Length,Is.EqualTo(1));
            var source=(AudioSource)Get(music,"audioSource");var clip=Run.Zone.Definition.music;
            Assert.That(source.clip,Is.SameAs(clip));Assert.That(clip.length,Is.InRange(137f,138f));
            Assert.That(clip.channels,Is.EqualTo(2));Assert.That(source.loop,Is.True);
            float fade=Time.unscaledTime;yield return Until(()=>Time.unscaledTime>=fade+1,"zone crossfade completes");
            Assert.That((float)Get(music,"fadeProgress"),Is.EqualTo(1));
            Assert.That(music.GetComponents<AudioSource>().Count(s=>s.clip!=null),Is.EqualTo(1));
            AudioPreferences.SetMusicMuted(true);Assert.That(music.GetComponents<AudioSource>().All(s=>s.mute),Is.True);
            AudioPreferences.SetMusicMuted(false);Assert.That(music.GetComponents<AudioSource>().All(s=>!s.mute),Is.True);
            Time.timeScale=0;yield return null;yield return null;
            Assert.That((bool)Get(music,"appliedPause"),Is.True);Time.timeScale=1;
            yield return null;yield return null;Assert.That((bool)Get(music,"appliedPause"),Is.False);
            Call(music,"OnApplicationPause",true);Assert.That((bool)Get(music,"appliedPause"),Is.True);
            Call(music,"OnApplicationPause",false);Assert.That((bool)Get(music,"appliedPause"),Is.False);
            music.SetZoneMusic(clip);Assert.That(music.GetComponents<AudioSource>().Length,Is.LessThanOrEqualTo(2));
            Assert.That((float)Get(music,"fadeProgress"),Is.EqualTo(1),"same zone/boss cannot restart the loop");
            music.SetZoneMusic(null);float began=Time.unscaledTime;
            yield return Until(()=>Time.unscaledTime>=began+1,"menu crossfade completes");
            Assert.That(source.clip,Is.Not.SameAs(clip));
            Assert.That(music.GetComponents<AudioSource>().Count(s=>s.clip!=null),Is.EqualTo(1));
        }
        finally {Time.timeScale=1;AudioPreferences.SetMusicMuted(muted);}
    }
    private BoardCellSnapshot RootCell(EnemyActor owner) => Run.Board.CaptureContinuation(Run.Waves.ContinuationOwnerSlot).cells.First(c=>c.barricade&&c.rootOwnerId==owner.PersistentId);
    private IEnumerator ClearRootSide(BoardCellSnapshot root)
    {
        PrepareSafeMove(); // Remove incidental matches/special chains from the single-hit fixture.
        var board=Run.Board;var gem=board.GetGem(root.x-1,root.y);
        Assert.That(gem,Is.Not.Null);
        var cleared=new System.Collections.Generic.HashSet<Gem>{gem};
        // Exercise the same hit-before-destruction ordering as rewardable clears.
        Call(board,"DamageBarricadesAdjacentToClears",cleared,null);
        yield return (IEnumerator)Call(board,"ClearMatches",cleared,null,false);
        yield return (IEnumerator)Call(board,"CollapseAndRefillBoard");
        yield return Stable();
    }
    [UnityTest] public IEnumerator ProductionRootRequiresVineClearThenLaterHitAndWeakVinesNeverSpread()
    {
        yield return Launch(1,true);QuietKitFixture();var owner=Enemy("orc_rootbinder");
        Assert.That(Run.Board.TryQueuePlantRoots(owner,1,1,false,false,null),Is.True);yield return Stable();
        var root=RootCell(owner);Assert.That(root.hasGem,Is.False);Assert.That(root.durability,Is.EqualTo(1));
        Assert.That(Run.Board.OwnedVineCount(owner),Is.EqualTo(4));Assert.That(Run.Board.RestrictionCount,Is.Zero);
        Run.Board.QueueVineSurge(owner,null);yield return Stable();Assert.That(Run.Board.OwnedVineCount(owner),Is.EqualTo(4));
        yield return ClearRootSide(root);Assert.That(RootCell(owner).durability,Is.EqualTo(1),"first clear only opens a side");
        yield return ClearRootSide(root);Assert.That(Run.Board.OwnedRootCount(owner),Is.Zero);
        Assert.That(Run.Board.OwnedVineCount(owner),Is.Zero);Assert.That(Run.Board.GetGem(root.x,root.y),Is.Not.Null);
    }
    [UnityTest] public IEnumerator ProductionWardenWarnsAndStaggerCancelsBeforePlanting()
    {
        yield return Launch(9,true);QuietKitFixture();var warden=Enemy("barkhide_warden");var kit=warden.GetComponent<ForestMilestoneEnemyAbility>();
        Set(warden,"currentHealth",9995); // This test isolates interruption from cascade lethals.
        yield return Move();yield return Move();Assert.That(kit.IsPreparing,Is.True);Assert.That(kit.ResponseMoves,Is.EqualTo(1));
        float basic=warden.GetComponent<EnemyAutoAttack>().RemainingAttackTime;
        yield return new WaitForSeconds(.2f);Assert.That(warden.GetComponent<EnemyAutoAttack>().RemainingAttackTime,Is.EqualTo(basic));
        var stagger=warden.GetComponent<EnemyStagger>();stagger.RestoreContinuation(new EnemyCombatSnapshot());stagger.ApplyStagger(2,2);yield return Stable();
        Assert.That(stagger.IsStaggered,Is.True);Assert.That(kit.IsPreparing,Is.False);Assert.That(kit.Outcome,Is.EqualTo("Interrupted"));
        Assert.That(Run.Board.OwnedRootCount(warden),Is.Zero);
    }
    [UnityTest] public IEnumerator ProductionWardenRootProtectsEveryAllyAndPhotoCannotRepairOrResurrectIt()
    {
        yield return Launch(9,true);QuietKitFixture();var warden=Enemy("barkhide_warden");var scout=Enemy("elven_scout");
        var kit=warden.GetComponent<ForestMilestoneEnemyAbility>();
        Run.Board.TryQueuePlantRoots(warden,1,2,false,true,null);yield return Stable();
        kit.RestoreContinuation(new EnemyCombatSnapshot{forestMilestone=new ForestMilestoneSnapshot{version=2,state=2,sequence=1}},_=>null);
        Assert.That(kit.IsProtected,Is.True);Assert.That(Run.Board.OwnedVineCount(warden),Is.EqualTo(4));
        foreach(var member in new[]{warden,scout}) {int hp=member.CurrentHealth;member.ResolveDirectDamage(20);Assert.That(hp-member.CurrentHealth,Is.EqualTo(15));}
        Assert.That(Run.Waves.TrySummonEnemy(Run.Zone.FindEnemy("Orc_Trailguard"),out var summoned),Is.True);
        int spawnedHp=summoned.CurrentHealth;summoned.ResolveDirectDamage(20);
        Assert.That(spawnedHp-summoned.CurrentHealth,Is.EqualTo(15),"newly summoned allies are protected before their first LateUpdate");
        Run.Board.QueueVineSurge(warden,null);yield return Stable();Assert.That(Run.Board.OwnedVineCount(warden),Is.GreaterThan(4));
        var photo=Run.Board.CaptureBoardMemory(Run.Waves.ContinuationOwnerSlot);var root=RootCell(warden);
        yield return ClearRootSide(root);yield return ClearRootSide(root);Assert.That(RootCell(warden).durability,Is.EqualTo(1));
        Assert.That(Run.Board.TryRestoreBoardMemory(photo,Run.Waves.ContinuationEnemy),Is.True);yield return Stable();
        Assert.That(RootCell(warden).durability,Is.EqualTo(1),"photo cannot repair durability");
        yield return ClearRootSide(root);
        Assert.That(kit.IsProtected,Is.False);int hp2=warden.CurrentHealth;warden.ResolveDirectDamage(20);Assert.That(hp2-warden.CurrentHealth,Is.EqualTo(20),"no exposure multiplier");
        Assert.That(Run.Board.TryRestoreBoardMemory(photo,Run.Waves.ContinuationEnemy),Is.True);yield return Stable();
        Assert.That(Run.Board.OwnedRootCount(warden),Is.Zero);Assert.That(Run.Board.GetGem(root.x,root.y),Is.Not.Null);
    }
    [UnityTest] public IEnumerator ProductionMatriarchAoEHealRootsAndDeadlineSurviveSaveExactlyOnce()
    {
        yield return Launch(11,true);QuietKitFixture();
        var boss=Enemy("briar_matriarch");boss.ResolveDamageWithoutFeedback(80);
        var scout=Enemy("elven_scout");scout.ResolveDamageWithoutFeedback(20);
        yield return Move();yield return Move();yield return Move();
        var kit=boss.GetComponent<ForestMilestoneEnemyAbility>();Assert.That(kit.IsPreparing,Is.True);Assert.That(kit.ResponseMoves,Is.EqualTo(2));
        Assert.That(Run.Board.OwnedRootCount(boss),Is.EqualTo(2));
        Assert.That(Run.SuspendToMenu(),Is.True);yield return null;SceneManager.LoadScene("Game");
        yield return Until(()=>Run?.Continuation!=null&&!Run.Continuation.IsRestoring,"ritual restore");
        Run.GetComponent<RunControlsUI>().Close();yield return Stable();QuietKitFixture();
        boss=Enemy("briar_matriarch");kit=boss.GetComponent<ForestMilestoneEnemyAbility>();Assert.That(kit.ResponseMoves,Is.EqualTo(2));
        int heals=0;boss.Healed+=(_,amount)=>heals+=amount;int expected=0;
        System.Action<int> due=_=>expected=Mathf.Min(boss.MaxHealth-boss.CurrentHealth,20+20*Run.Board.OwnedRootCount(boss));
        yield return Move();Run.Board.ValidPlayerMoveCompleted+=due;yield return Move();Run.Board.ValidPlayerMoveCompleted-=due;
        Assert.That(kit.Outcome,Is.EqualTo("Renewed"));Assert.That(heals,Is.EqualTo(expected));Assert.That(heals,Is.GreaterThan(0));
        Assert.That(Run.Board.OwnedRootCount(boss),Is.EqualTo(2));int resolved=heals;
        kit.ResolveAcceptedMove();yield return new WaitForSeconds(.8f);Assert.That(heals,Is.EqualTo(resolved));
    }
    [UnityTest] public IEnumerator ProductionMatriarchBothHeartrootsStaggerAndHarvestConsumesOnlyVines()
    {
        yield return Launch(8,true);QuietKitFixture();var boss=Enemy("briar_matriarch");var kit=boss.GetComponent<ForestMilestoneEnemyAbility>();
        Run.Board.TryQueuePlantRoots(boss,2,2,true,true,null);yield return Stable();
        Assert.That(Run.Board.OwnedRootCount(boss),Is.EqualTo(2),"harvest fixture starts with its complete linked pair");
        kit.RestoreContinuation(new EnemyCombatSnapshot{forestMilestone=new ForestMilestoneSnapshot{version=2,state=1,activeAbility=2,sequence=1,deadline=3,heartrootsArmed=true}},_=>null);
        Assert.That(kit.ChannelMoves,Is.EqualTo(3));int damage=0;int count=-1;
        int before=0;
        System.Action<int> due=_=>{count=Run.Board.VineCount;before=Run.Player.CurrentHealth;};
        yield return Move();Assert.That(kit.IsPreparing,Is.True,kit.Outcome);Assert.That(kit.ResponseMoves,Is.EqualTo(2));
        yield return Move();Assert.That(kit.IsPreparing,Is.True,kit.Outcome);Assert.That(kit.ResponseMoves,Is.EqualTo(1));
        Run.Board.ValidPlayerMoveCompleted+=due;yield return Move();Run.Board.ValidPlayerMoveCompleted-=due;
        damage=before-Run.Player.CurrentHealth;Assert.That(kit.Outcome,Is.EqualTo("Harvested"));
        Assert.That(damage,Is.EqualTo(Mathf.Min(before,20+5*count)));Assert.That(Run.Board.VineCount,Is.Zero);
        Assert.That(Run.Board.OwnedRootCount(boss),Is.EqualTo(2));
        boss.GetComponent<EnemyStagger>().RestoreContinuation(new EnemyCombatSnapshot());
        var first=RootCell(boss);yield return ClearRootSide(first);yield return ClearRootSide(first);
        Assert.That(Run.Board.OwnedRootCount(boss),Is.EqualTo(1));Assert.That(boss.GetComponent<EnemyStagger>().IsStaggered,Is.False);
        var second=RootCell(boss);yield return ClearRootSide(second);yield return ClearRootSide(second);
        Assert.That(Run.Board.OwnedRootCount(boss),Is.Zero);Assert.That(boss.GetComponent<EnemyStagger>().IsStaggered,Is.True);
    }
    [UnityTest] public IEnumerator ProductionSoloMenderNeverHealsSelf()
    {
        yield return Launch(5,true);QuietKitFixture();var mender=Enemy("elven_mender");mender.ResolveDamageWithoutFeedback(20);
        var stats=mender.RuntimeStats;
        typeof(EnemyActor).GetProperty("RuntimeStats").SetValue(mender,new EnemyRuntimeStats(stats.Wave,stats.Level,10000,stats.Damage,stats.FollowUpDamage,stats.AttackInterval,stats.SpecialTurnRequirement));
        Set(mender,"currentHealth",7000); // Wounded throughout the fixture, with no eligible ally.
        yield return Move();yield return Move();yield return Move();Assert.That(mender.GetComponent<EnemyChannelRuntime>().IsChanneling,Is.False);
    }
    [UnityTest] public IEnumerator ProductionInvalidTargetFizzlesWithoutStaggerOrRetarget()
    {
        yield return Launch();QuietKitFixture();var mender=Enemy("elven_mender");var target=Enemy("orc_trailguard");
        var channel=mender.GetComponent<EnemyChannelRuntime>();mender.GetComponent<EnemyStagger>().RestoreContinuation(new EnemyCombatSnapshot());
        channel.RestoreContinuation(new EnemyCombatSnapshot{channel=new EnemyChannelSnapshot{state=1,sequence=1,targetId=target.PersistentId,deadlineMove=2}},_=>target);
        target.ResolveDirectDamage(9999);yield return Stable();Assert.That(channel.Outcome,Is.EqualTo("Target lost"));
        Assert.That(mender.GetComponent<EnemyStagger>().IsStaggered,Is.False);Assert.That(channel.Target,Is.Null);
    }
    [UnityTest] public IEnumerator HybridBasicsCountSecondsWhileAbilitiesAndDurationsWaitForMoves()
    {
        yield return Launch(secondsBasics:true);
        Assert.That(CombatMoveClock.MoveBasics,Is.False);
        var enemies=Run.Waves.ActiveEnemies.ToArray();
        object hold=new object();
        foreach(var enemy in enemies) enemy.GetComponent<EnemyAutoAttack>().SetActionPaused(hold,true);
        var attacker=enemies[0].GetComponent<EnemyAutoAttack>();
        int health=Run.Player.CurrentHealth;
        Set(attacker,"remainingAttackTime",.2f);attacker.SetActionPaused(hold,false);
        yield return Until(()=>Run.Player.CurrentHealth<health,"seconds attack without a board move");
        attacker.SetActionPaused(hold,true);yield return Stable();
        Assert.That(Run.MoveClock.Tick,Is.Zero);
        foreach(var enemy in enemies) Assert.That(enemy.CurrentSpecialTurnCount,Is.Zero);
        float remaining=attacker.RemainingAttackTime;
        yield return new WaitForSeconds(.25f);Assert.That(attacker.RemainingAttackTime,Is.EqualTo(remaining));
        Time.timeScale=0;attacker.SetActionPaused(hold,false);
        yield return new WaitForSecondsRealtime(.25f);Assert.That(attacker.RemainingAttackTime,Is.EqualTo(remaining));
        Time.timeScale=1;attacker.SetActionPaused(hold,true);
        yield return Move();Assert.That(Run.MoveClock.Tick,Is.EqualTo(1));
        Assert.That(attacker.RemainingAttackTime,Is.EqualTo(remaining),"a move must not subtract seconds or reset held basics");
        var saved=Run.Continuation.Capture();
        Assert.That(saved.clock.profile,Is.EqualTo(CombatClockSnapshot.HybridProfile));
        Assert.That(RunContinuation.SupportsSnapshot(saved),Is.True);
        Assert.That(Run.SuspendToMenu(),Is.True);yield return null;
        SceneManager.LoadScene("Game");yield return Stable();
        Assert.That(CombatMoveClock.MoveBasics,Is.False);
        Assert.That(Run.Continuation.Capture().clock.profile,Is.EqualTo(CombatClockSnapshot.HybridProfile));
    }

    [UnityTest] public IEnumerator HybridAcceptedBoardActionPausesBasicCountdownWithoutConvertingIt()
    {
        yield return Launch(secondsBasics:true);
        var attacker=Run.Waves.ActiveEnemies[0].GetComponent<EnemyAutoAttack>();
        Set(attacker,"remainingAttackTime",20f);
        float accepted=0;
        yield return Move(()=>accepted=attacker.RemainingAttackTime);
        Assert.That(attacker.RemainingAttackTime,Is.EqualTo(accepted).Within(.07f));
        // Refill cascades may legitimately stagger the attacker. Remove that
        // separate effect before isolating the resumed seconds countdown.
        attacker.GetComponent<EnemyStagger>().RestoreContinuation(new EnemyCombatSnapshot());
        Assert.That(attacker.IsPausedByStagger,Is.False);
        Assert.That(attacker.EnemyActor.IsDefeated,Is.False,"fixture attacker survives the cascade");
        Assert.That(attacker.IsRunning,Is.True,"seconds coroutine still running");
        Assert.That(attacker.IsPausedByAction,Is.False,"no specialist owns this basic attacker");
        Assert.That(CombatMoveClock.PausesTimedBasics,Is.False,"accepted move released the clock gate");
        Assert.That(Time.timeScale,Is.EqualTo(1));
        float resumedAt=Time.time;
        yield return Until(()=>Time.time>=resumedAt+.25f,"a quarter-second of the resumed game clock");
        Assert.That(attacker.RemainingAttackTime,Is.LessThan(accepted-.15f));
        Assert.That(Run.MoveClock.Tick,Is.EqualTo(1));
    }
    [UnityTest] public IEnumerator RootWarningPlayerClearStaggersButTargetFizzleDoesNot()
    {
        yield return Launch(9,true);QuietKitFixture();var owner=Enemy("barkhide_warden");
        var kit=owner.GetComponent<ForestMilestoneEnemyAbility>();var stagger=owner.GetComponent<EnemyStagger>();
        BoardController.GemSetThreat warning=null;
        Run.Board.TryQueueRootWarning(owner,1,2,false,true,w=>warning=w);yield return Stable();Assert.That(warning,Is.Not.Null);
        kit.RestoreContinuation(new EnemyCombatSnapshot{forestMilestone=new ForestMilestoneSnapshot{version=2,state=1,sequence=1,deadline=warning.DueMove}},_=>null);
        stagger.RestoreContinuation(new EnemyCombatSnapshot());
        var targets=new System.Collections.Generic.HashSet<Gem>(warning.Targets);
        Call(Run.Board,"DamageBarricadesAdjacentToClears",targets,null);
        yield return (IEnumerator)Call(Run.Board,"ClearMatches",targets,null,false);
        yield return (IEnumerator)Call(Run.Board,"ResolveEnvironmentalBoardChange");yield return Stable();
        Assert.That(kit.Outcome,Is.EqualTo("Interrupted"));Assert.That(stagger.IsStaggered,Is.True);
        stagger.RestoreContinuation(new EnemyCombatSnapshot());PrepareSafeMove();
        Run.Board.TryQueueRootWarning(owner,1,2,false,true,w=>warning=w);yield return Stable();Assert.That(warning.Ended,Is.False);
        kit.RestoreContinuation(new EnemyCombatSnapshot{forestMilestone=new ForestMilestoneSnapshot{version=2,state=1,sequence=2,resolvedSequence=1,deadline=warning.DueMove}},_=>null);
        Run.Board.CancelGemSetThreat(warning);yield return Stable();
        Assert.That(kit.Outcome,Is.EqualTo("Target lost"));Assert.That(stagger.IsStaggered,Is.False);
    }
    [UnityTest] public IEnumerator LegacyVinePinsUpgradeWithoutRemovingRealChains()
    {
        yield return Launch(1);var board=Run.Board;
        board.QueueEnvironmentalVine(board.GetGem(0,0));yield return Stable();
        Assert.That(Run.SuspendToMenu(),Is.True);yield return null;
        var account=JsonUtility.FromJson<AccountSave>(File.ReadAllText(path));var saved=account.run.checkpoint.board;
        saved.forestRulesVersion=0;saved.vines[0].cellOverlay=false;
        var vine=saved.cells.First(c=>c.x==0&&c.y==0);vine.pinned=true;vine.movable=true;vine.pinOwner=-1;
        var chain=saved.cells.First(c=>c.x==1&&c.y==0);chain.pinned=true;chain.movable=true;chain.pinOwner=0;
        File.WriteAllText(path,JsonUtility.ToJson(account,true));profile.Dispose();profile=AccountProgression.UseDisposableProfile(path);
        SceneManager.LoadScene("Game");yield return Until(()=>Run?.Continuation!=null&&!Run.Continuation.IsRestoring,"legacy vine migration");
        Run.GetComponent<RunControlsUI>().Close();yield return Stable();board=Run.Board;
        Assert.That(board.IsCellVined(0,0),Is.True);Assert.That(board.IsGemPinned(board.GetGem(0,0)),Is.False);
        Assert.That(board.IsGemPinned(board.GetGem(1,0)),Is.True,"unrelated chain survives migration");
        Assert.That(board.NextVineGrowthMove,Is.EqualTo(Run.MoveClock.Tick+2));
    }
}
