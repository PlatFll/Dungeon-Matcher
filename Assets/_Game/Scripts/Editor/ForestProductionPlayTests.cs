using System.Collections;
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
    [UnityTest] public IEnumerator ProductionWardenWarnsAndStaggerCancelsBeforePlanting()
    {
        yield return Launch(9,true);QuietKitFixture();
        var warden=Enemy("barkhide_warden");var kit=warden.GetComponent<ForestMilestoneEnemyAbility>();
        yield return Move();Assert.That(kit.IsPreparing,Is.False);
        yield return Move();Assert.That(kit.IsPreparing,Is.True);Assert.That(kit.ResponseMoves,Is.EqualTo(1));
        float basic=warden.GetComponent<EnemyAutoAttack>().RemainingAttackTime;
        float began=Time.time;yield return Until(()=>Time.time>=began+.2f,"held basic observes game time");
        Assert.That(warden.GetComponent<EnemyAutoAttack>().RemainingAttackTime,Is.EqualTo(basic));
        warden.GetComponent<EnemyStagger>().RestoreContinuation(new EnemyCombatSnapshot());
        warden.GetComponent<EnemyStagger>().ApplyStagger(2,2);yield return Stable();
        Assert.That(kit.IsPreparing,Is.False);Assert.That(kit.IsExposed,Is.True);
        Assert.That(kit.Outcome,Is.EqualTo("Interrupted"));Assert.That(Run.Board.OwnedVineCount(warden),Is.Zero);
        Assert.That(Run.Board.CaptureContinuation(Run.Waves.ContinuationOwnerSlot).warnings.Any(w=>w.vine),Is.False);
    }
    [UnityTest] public IEnumerator ProductionAnchorsProtectOnceDoNotSpreadAndPhotoCannotRestoreFinishedCast()
    {
        yield return Launch(9,true);QuietKitFixture();
        var warden=Enemy("barkhide_warden");var kit=warden.GetComponent<ForestMilestoneEnemyAbility>();
        Assert.That(Run.Board.TryQueueVineAnchors(warden,2,null),Is.True);yield return Stable();
        kit.RestoreContinuation(new EnemyCombatSnapshot{forestMilestone=new ForestMilestoneSnapshot{state=2,sequence=1}},_=>null);
        Assert.That(kit.IsProtected,Is.True);
        var nodes=Run.Board.CaptureContinuation(Run.Waves.ContinuationOwnerSlot).vines;
        Assert.That(nodes.Count,Is.EqualTo(2));Assert.That(nodes.All(n=>n.nonSpreading),Is.True);
        yield return Run.Board.AdvanceVineNetworks(100);
        Assert.That(Run.Board.OwnedVineCount(warden),Is.EqualTo(2));
        Assert.That(Run.Board.CaptureContinuation(Run.Waves.ContinuationOwnerSlot).warnings.Any(w=>w.vine),Is.False);
        int hp=warden.CurrentHealth;warden.ResolveWeaknessDamage(40);
        Assert.That(hp-warden.CurrentHealth,Is.EqualTo(30),"one 25% reduction, not once per anchor");
        var photo=Run.Board.CaptureBoardMemory(Run.Waves.ContinuationOwnerSlot);
        Run.Board.RemoveVineSource(warden);yield return Stable();yield return null;
        Assert.That(kit.IsExposed,Is.True);Assert.That(kit.IsProtected,Is.False);
        hp=warden.CurrentHealth;warden.ResolveWeaknessDamage(20);
        Assert.That(hp-warden.CurrentHealth,Is.EqualTo(25),"free weakness hit benefits without advancing expiry");
        hp=warden.CurrentHealth;warden.ResolveDirectDamage(20);
        Assert.That(hp-warden.CurrentHealth,Is.EqualTo(20),"direct damage is not weakness damage");
        Assert.That(Run.Board.TryRestoreBoardMemory(photo,Run.Waves.ContinuationEnemy),Is.True);
        Assert.That(Run.Board.OwnedVineCount(warden),Is.Zero,"finished cast cannot return through a photo");
        Assert.That(Run.Board.RestrictionCount,Is.Zero);
        yield return Move();Assert.That(kit.IsExposed,Is.True);
        yield return Move();Assert.That(kit.IsExposed,Is.False);
    }
    [UnityTest] public IEnumerator ProductionMatriarchFixedTargetAndAnchorsSurviveSaveThenResolveOnce()
    {
        yield return Launch(11,true);QuietKitFixture();
        var boss=Enemy("briar_matriarch");var scout=Enemy("elven_scout");
        scout.ResolveDamageWithoutFeedback(25);
        yield return Move();yield return Move();yield return Move();
        var kit=boss.GetComponent<ForestMilestoneEnemyAbility>();
        Assert.That(kit.IsPreparing,Is.True);Assert.That(kit.Target,Is.SameAs(scout));Assert.That(kit.ResponseMoves,Is.EqualTo(2));
        Assert.That(Run.Board.OwnedVineCount(boss),Is.EqualTo(2));long target=scout.PersistentId;
        Assert.That(Run.SuspendToMenu(),Is.True);yield return null;SceneManager.LoadScene("Game");
        yield return Until(()=>Run?.Continuation!=null&&!Run.Continuation.IsRestoring,"ritual restore");
        Run.GetComponent<RunControlsUI>().Close();yield return Stable();QuietKitFixture();
        boss=Enemy("briar_matriarch");scout=Enemy("elven_scout");kit=boss.GetComponent<ForestMilestoneEnemyAbility>();
        Assert.That(kit.Target.PersistentId,Is.EqualTo(target));Assert.That(kit.ResponseMoves,Is.EqualTo(2));
        int heals=0;scout.Healed+=(_,amount)=>heals+=amount;
        // Preserve the authored anchors through the response fixture, then
        // explicitly offer its deadline through the same move coordinator.
        yield return Move();
        int dueAnchors=-1,missing=0;
        System.Action<int> beforeConsequences=_=>{dueAnchors=Run.Board.OwnedVineCount(boss);missing=scout.MaxHealth-scout.CurrentHealth;};
        Run.Board.ValidPlayerMoveCompleted+=beforeConsequences;
        yield return Move();
        Run.Board.ValidPlayerMoveCompleted-=beforeConsequences;
        Assert.That(kit.IsPreparing,Is.False);
        Assert.That(dueAnchors,Is.GreaterThan(0),"fixture must demonstrate a successful ritual, not only cancellation");
        Assert.That(kit.Outcome,Is.EqualTo("Renewed"));
        Assert.That(heals,Is.EqualTo(Mathf.Min(missing,dueAnchors*10)));
        Assert.That(heals%10,Is.Zero);
        Assert.That(Run.Board.OwnedVineCount(boss),Is.Zero);
        int resolved=heals;float now=Time.time;yield return Until(()=>Time.time>=now+.8f,"release motion completes");
        Assert.That(heals,Is.EqualTo(resolved),"presentation cannot produce a second heal");
        Assert.That(kit.BlocksBasic,Is.True);
    }
    [UnityTest] public IEnumerator ProductionRitualStaggerAndTargetDeathCancelWithoutRetarget()
    {
        yield return Launch(11,true);QuietKitFixture();
        var boss=Enemy("briar_matriarch");var scout=Enemy("elven_scout");var kit=boss.GetComponent<ForestMilestoneEnemyAbility>();
        Assert.That(Run.Board.TryQueueVineAnchors(boss,2,null),Is.True);yield return Stable();
        kit.RestoreContinuation(new EnemyCombatSnapshot{forestMilestone=new ForestMilestoneSnapshot{state=1,sequence=1,targetId=scout.PersistentId,deadline=2}},_=>scout);
        boss.GetComponent<EnemyStagger>().RestoreContinuation(new EnemyCombatSnapshot());
        boss.GetComponent<EnemyStagger>().ApplyStagger(2,2);yield return Stable();
        Assert.That(kit.Outcome,Is.EqualTo("Interrupted"));Assert.That(kit.IsExposed,Is.True);Assert.That(Run.Board.OwnedVineCount(boss),Is.Zero);
        Assert.That(Run.Board.TryQueueVineAnchors(boss,2,null),Is.True);yield return Stable();
        kit.RestoreContinuation(new EnemyCombatSnapshot{forestMilestone=new ForestMilestoneSnapshot{state=1,sequence=2,resolvedSequence=1,targetId=scout.PersistentId,deadline=2}},_=>scout);
        scout.ResolveDirectDamage(9999);yield return Stable();
        Assert.That(kit.Outcome,Is.EqualTo("Target lost"));Assert.That(kit.Target,Is.Null);
        Assert.That(Run.Board.OwnedVineCount(boss),Is.Zero);Assert.That(kit.IsExposed,Is.False);
        var snapshot=new EnemyCombatSnapshot();kit.CaptureContinuation(snapshot,_=>0);
        Assert.That(snapshot.forestMilestone.resolvedSequence,Is.EqualTo(2));
    }
    [UnityTest] public IEnumerator ProductionSoloMenderNeverHealsSelf()
    {
        yield return Launch(5,true);QuietKitFixture();
        var mender=Enemy("elven_mender");mender.ResolveDamageWithoutFeedback(20);
        yield return Move();yield return Move();yield return Move();
        Assert.That(mender.GetComponent<EnemyChannelRuntime>().IsChanneling,Is.False);
    }
    [UnityTest] public IEnumerator ProductionSoloMatriarchCanTargetSelf()
    {
        yield return Launch(8,true);QuietKitFixture();
        var boss=Enemy("briar_matriarch");boss.ResolveDamageWithoutFeedback(30);
        yield return Move();yield return Move();yield return Move();
        Assert.That(boss.GetComponent<ForestMilestoneEnemyAbility>().Target,Is.SameAs(boss));
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
}
