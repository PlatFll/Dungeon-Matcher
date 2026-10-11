using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;

public sealed partial class ForestFoundationPlayTests
{
    [UnityTest] public IEnumerator FrostwakeUnifiedCleaveResumesIdentityStageDeadlineAndAttackHold()
    {
        yield return LaunchUnified();QuietKitFixture();var actor=Run.Waves.ActiveEnemies[0];
        var definition=actor.Definition;var priorKind=definition.SpecialAbilityKind;var priorRules=definition.frostwakeRules;
        bool priorEnabled=definition.HasSpecialAbility;
        var rules=ScriptableObject.CreateInstance<FrostwakeRules>();
        rules.frostbite=Resources.Load<PlayerStatusDefinition>("PlayerStatuses/Frostbite");
        try
        {
            // Keep a registered enemy identity for full Continue coverage. This
            // temporary in-memory kit change is restored in finally, never saved.
            definition.frostwakeRules=rules;Set(definition,"specialAbilityKind",EnemySpecialAbilityKind.GlacierCleave);
            Set(definition,"hasSpecialAbility",true);
            var kit=actor.gameObject.AddComponent<GlacierCleaveRuntime>();
            kit.InitializeSpecialAbility(actor,Run.Board,Run.Waves.ActiveEnemies);
            Set(actor,"isSpecialReady",true);yield return Move();
            Assert.That(kit.IsChanneling,Is.True);Assert.That(kit.ResponseMoves,Is.EqualTo(3));
            int id=kit.Target.BoardIdentity;float attack=actor.GetComponent<EnemyAutoAttack>().RemainingAttackTime;
            Call(Run.Board,"DamageFrostFromClears",new HashSet<Gem>{
                new[]{Vector2Int.up,Vector2Int.down,Vector2Int.left,Vector2Int.right}
                .Select(d=>Run.Board.GetGem(kit.Target.Column+d.x,kit.Target.Row+d.y)).First(g=>g!=null)},null,false);
            Assert.That(Run.Board.IceStage(kit.Target),Is.EqualTo(FrostStage.HeavyIce));
            Assert.That(Run.Continuation.SaveNow(),Is.True);string enemyId=definition.EnemyId;
            Assert.That(Run.SuspendToMenu(),Is.True);yield return null;SceneManager.LoadScene("Game");
            yield return Until(()=>Run!=null && Run.Continuation!=null && !Run.Continuation.IsRestoring,"cleave resumes");
            Run.GetComponent<RunControlsUI>().Close();yield return Stable();
            actor=Enemy(enemyId);kit=actor.GetComponent<GlacierCleaveRuntime>();
            Assert.That(kit,Is.Not.Null);Assert.That(kit.Target.BoardIdentity,Is.EqualTo(id));
            Assert.That(kit.ResponseMoves,Is.EqualTo(3));Assert.That(Run.Board.IceStage(kit.Target),Is.EqualTo(FrostStage.HeavyIce));
            Assert.That(actor.GetComponent<EnemyAutoAttack>().IsPausedByAction,Is.True);
            var intent=actor.GetComponent<EnemyUnifiedIntentView>();
            Assert.That((int)Call(intent,"ResponseMoves"),Is.EqualTo(3));
            yield return Move();Assert.That(kit.ResponseMoves,Is.EqualTo(2));
            Assert.That(actor.GetComponent<EnemyAutoAttack>().RemainingAttackTime,Is.EqualTo(attack));
        }
        finally {Set(definition,"specialAbilityKind",priorKind);Set(definition,"hasSpecialAbility",priorEnabled);definition.frostwakeRules=priorRules;Object.Destroy(rules);}
    }
    [UnityTest] public IEnumerator FrostwakeCleaveDamageBandsAndFrostbiteUseCentralPlayerDamage()
    {
        yield return Launch();QuietKitFixture();var actor=Enemy("orc_trailguard");
        var rules=ScriptableObject.CreateInstance<FrostwakeRules>();
        rules.frostbite=Resources.Load<PlayerStatusDefinition>("PlayerStatuses/Frostbite");
        Assert.That(rules.frostbite,Is.Not.Null);var kit=AttachCleave(actor,rules);
        Gem target=null;Run.Board.TryQueueEncasedGem(actor,FrostStage.Bifrost,g=>target=g);yield return Stable();
        int sequence=0;
        foreach(var stage in new[]{FrostStage.Frost,FrostStage.HeavyIce,FrostStage.Bifrost})
        {
            Run.Player.Heal(10000);int before=Run.Player.CurrentHealth;
            kit.RestoreContinuation(new EnemyCombatSnapshot {glacierCleave=new GlacierCleaveSnapshot
                {channeling=true,targetIdentity=target.BoardIdentity,deadlineMove=3,sequence=++sequence}},_=>actor);
            Call(kit,"Impact",stage);
            Assert.That(before-Run.Player.CurrentHealth,Is.EqualTo(CombatAmounts.Round(rules.CleaveDamage(stage)*actor.RuntimeStats.DamageMultiplier)));
            Assert.That(Run.Player.Statuses.Has(PlayerStatusKind.Frostbite),Is.EqualTo(stage==FrostStage.Bifrost));
            int after=Run.Player.CurrentHealth;Call(kit,"Impact",stage);Assert.That(Run.Player.CurrentHealth,Is.EqualTo(after));
        }
        Object.Destroy(rules);
    }
    private GlacierCleaveRuntime AttachCleave(EnemyActor actor,FrostwakeRules rules)
    {
        var definition=Object.Instantiate(actor.Definition);
        definition.frostwakeRules=rules;Set(definition,"specialAbilityKind",EnemySpecialAbilityKind.GlacierCleave);
        Set(definition,"hasSpecialAbility",true);
        Set(actor,"definition",definition);
        var kit=actor.gameObject.AddComponent<GlacierCleaveRuntime>();
        kit.InitializeSpecialAbility(actor,Run.Board,Run.Waves.ActiveEnemies);
        return kit;
    }
    [UnityTest] public IEnumerator FrostwakeCleaveHoldsBasicForThreeResponsesAndConsumesIdentityOnce()
    {
        yield return Launch();QuietKitFixture();var actor=Enemy("orc_trailguard");
        var rules=ScriptableObject.CreateInstance<FrostwakeRules>();
        var kit=AttachCleave(actor,rules);
        Set(actor,"isSpecialReady",true);yield return Move();
        Assert.That(kit.IsChanneling,Is.True);Assert.That(kit.ResponseMoves,Is.EqualTo(3));
        int id=kit.Target.BoardIdentity;float attack=actor.GetComponent<EnemyAutoAttack>().RemainingAttackTime;
        Assert.That(Run.Board.IceStage(kit.Target),Is.EqualTo(FrostStage.Bifrost));
        int hp=Run.Player.CurrentHealth;
        yield return Move();Assert.That(kit.ResponseMoves,Is.EqualTo(2));
        yield return Move();Assert.That(kit.ResponseMoves,Is.EqualTo(1));
        Assert.That(actor.GetComponent<EnemyAutoAttack>().RemainingAttackTime,Is.EqualTo(attack));
        // Avoid unrelated cascade thaw in this timing fixture, while retaining
        // the real queued consume and centralized damage path at the deadline.
        var remaining=Run.Board.IceStage(kit.Target);Assert.That(remaining,Is.Not.EqualTo(FrostStage.None));
        yield return Move();
        Assert.That(kit.IsChanneling,Is.False);Assert.That(kit.Outcome,Is.EqualTo("Struck"));
        Assert.That(Run.Board.FindGemIdentity(id),Is.Null);
        Assert.That(Run.Player.CurrentHealth,Is.LessThan(hp));
        int after=Run.Player.CurrentHealth;kit.ResolveAcceptedMove();yield return Stable();
        Assert.That(Run.Player.CurrentHealth,Is.EqualTo(after),"resolution cannot repeat");
        Object.Destroy(rules);
    }
    [UnityTest] public IEnumerator FrostwakeFullThawCancelsCleaveAndPhotoDoesNotRewindIce()
    {
        yield return Launch();QuietKitFixture();var board=Run.Board;var actor=Enemy("orc_trailguard");
        var rules=ScriptableObject.CreateInstance<FrostwakeRules>();var kit=AttachCleave(actor,rules);
        Set(actor,"isSpecialReady",true);yield return Move();Assert.That(kit.IsChanneling,Is.True);
        var target=kit.Target;int identity=target.BoardIdentity;
        var photo=board.CaptureBoardMemory(Run.Waves.ContinuationOwnerSlot);
        var saved=new EnemyCombatSnapshot();kit.CaptureContinuation(saved,_=>0);
        kit.RestoreContinuation(saved,_=>actor);
        Assert.That(kit.Target.BoardIdentity,Is.EqualTo(identity));Assert.That(kit.ResponseMoves,Is.EqualTo(3));
        var neighbor=new[]{Vector2Int.up,Vector2Int.down,Vector2Int.left,Vector2Int.right}
            .Select(d=>board.GetGem(target.Column+d.x,target.Row+d.y)).First(g=>g!=null);
        Set(actor.GetComponent<EnemyStagger>(),"remainingImmunityTime",0f);
        Call(board,"DamageFrostFromClears",new HashSet<Gem>{neighbor},null,false);
        Assert.That(board.IceStage(target),Is.EqualTo(FrostStage.HeavyIce));
        Assert.That(board.TryRestoreBoardMemory(photo,_=>actor),Is.True);yield return Stable();
        target=board.FindGemIdentity(identity);Assert.That(board.IceStage(target),Is.EqualTo(FrostStage.HeavyIce));
        neighbor=new[]{Vector2Int.up,Vector2Int.down,Vector2Int.left,Vector2Int.right}
            .Select(d=>board.GetGem(target.Column+d.x,target.Row+d.y)).First(g=>g!=null);
        Call(board,"DamageFrostFromClears",new HashSet<Gem>{neighbor},null,false);
        Call(board,"DamageFrostFromClears",new HashSet<Gem>{neighbor},null,false);
        Assert.That(kit.IsChanneling,Is.False);Assert.That(kit.Outcome,Is.EqualTo("Thawed"));
        Assert.That(actor.GetComponent<EnemyStagger>().IsStaggered,Is.True);
        Assert.That(board.FindGemIdentity(identity),Is.SameAs(target),"thaw does not replace the target gem");
        Object.Destroy(rules);
    }
    [UnityTest] public IEnumerator FrostwakeEnvironmentPersistsAtStableSaveAndCleansOnTravel()
    {
        yield return Launch();QuietKitFixture();var board=Run.Board;var actor=Enemy("orc_trailguard");
        Gem target=null;Assert.That(board.TryQueueEncasedGem(actor,FrostStage.HeavyIce,g=>target=g),Is.True);
        yield return Stable();Assert.That(target,Is.Not.Null);int identity=target.BoardIdentity;
        var saved=board.CaptureContinuation(Run.Waves.ContinuationOwnerSlot);
        Assert.That(saved.frost.Single().identity,Is.EqualTo(identity));
        Call(board,"RestoreFrost",saved);Assert.That(board.IceStage(target),Is.EqualTo(FrostStage.HeavyIce));
        board.PrepareZoneArrival(saved,Resources.Load<ZoneDefinition>("Zones/dungeon"));
        Assert.That(saved.frost,Is.Empty);Assert.That(saved.nextFrostSeedMove,Is.Zero);
        Call(board,"ReleaseAllPinsForEmergencyReshuffle");
        Assert.That(board.IsGemPinned(target),Is.False);
    }
}
