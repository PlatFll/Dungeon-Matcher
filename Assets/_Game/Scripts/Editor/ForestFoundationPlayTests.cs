using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed partial class ForestFoundationPlayTests
{
    private const BindingFlags Flags=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
    private IDisposable profile, character, mastery;
    private string path;
    private bool preserveCounterplayCrystal;
    private Vector2Int safeMoveFrom,safeMoveTo;
    private RunSession Run=>RunSession.Current;
    [UnitySetUp] public IEnumerator SetUp()
    {
        typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.Static|BindingFlags.NonPublic)
            .Invoke(null,new object[]{new Vector2Int(1080,1920)});
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        yield return new EnterPlayMode();
        path=Path.GetFullPath(".utmp/ForestTestProfiles/"+Guid.NewGuid().ToString("N")+".json");
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path,JsonUtility.ToJson(new AccountSave {potions=3,bombs=3,equipPotions=true,equipBombs=true}));
        profile=AccountProgression.UseDisposableProfile(path);
        character=CharacterSelectionSettings.UseTemporarySelection("skeleton");
        mastery=GemMasterySettings.UseTemporaryLoadout(GemMasteryLoadout.Default);
    }
    [UnityTearDown] public IEnumerator TearDown()
    {
        Time.timeScale=1;SceneManager.LoadScene("MainMenu");yield return null;
        profile?.Dispose();character?.Dispose();mastery?.Dispose();
        profile=character=mastery=null;RunLaunchOptions.ForestPrototype=false;RunLaunchOptions.ForestEncounterOffset=0;
        yield return new ExitPlayMode();
    }
    private IEnumerator Launch(int offset=0, bool secondsBasics=false)
    {
        RunLaunchOptions.ForestClockProfile=secondsBasics?CombatClockSnapshot.HybridProfile:CombatClockSnapshot.MoveProfile;
        RunLaunchOptions.ForestPrototype=true;RunLaunchOptions.ForestEncounterOffset=offset;
        SceneManager.LoadScene("Game");yield return Stable();
        Assert.That(Run.MoveClock,Is.Not.Null);Assert.That(Run.Zone.Definition.eligibleForLiveTravel,Is.False);
        Assert.That(Run.Waves.ActiveEnemies.Count,Is.EqualTo(Run.Zone.TestEncounter(1).members.Length));
    }
    private IEnumerator Stable()
    {
        yield return null;
        yield return Until(()=>Run!=null && Run.Continuation!=null && Run.Continuation.CanCapture && Run.Waves.IsWaveActive,"forest settles");
    }
    private static IEnumerator Until(Func<bool> ready,string why)
    {float end=Time.realtimeSinceStartup+40;while(!ready()){Assert.That(Time.realtimeSinceStartup,Is.LessThan(end),why);yield return null;}}
    private EnemyActor Enemy(string name)=>Run.Waves.ActiveEnemies.First(e=>e.Definition.EnemyId==name);
    private static object Get(object o,string field)=>o.GetType().GetField(field,Flags).GetValue(o);
    private static void Set(object o,string field,object value)=>o.GetType().GetField(field,Flags).SetValue(o,value);
    private static object Call(object o,string method,params object[] args)=>o.GetType().GetMethod(method,Flags).Invoke(o,args);
    private void PrepareSafeMove()
    {
        var board=Run.Board;var sprites=(Sprite[])Get(board,"gemSprites");
        // A controlled no-special opening isolates timing from random damage.
        // Refill and all resolution still use the production deterministic path.
        for(int x=0;x<board.Width;x++)for(int y=0;y<board.Height;y++)
        {var g=board.GetGem(x,y);if(g==null) continue;if(!(preserveCounterplayCrystal && x==board.Width-1 && y==0)) g.SetSpecialType(GemSpecialType.None);g.SetType((GemType)((x+2*y)%6),sprites[(x+2*y)%6]);}
        var color=(GemType)Enumerable.Range(0,6).First(n=>Run.Waves.ActiveEnemies.All(e=>(int)e.AssignedGemType!=n));
        int top=-1,left=0;
        // A ritual may pin the old fixed swap. Find an actual unrestrained
        // response region instead of waiting forever on an invalid fixture.
        for(int y=board.Height-1;y>=1&&top<0;y--)for(int x=0;x<=board.Width-4&&top<0;x++)
        {
            bool clear=true;
            for(int px=x;px<x+4;px++)for(int py=Mathf.Max(0,y-2);py<=Mathf.Min(board.Height-1,y+1);py++)
                clear&=board.GetGem(px,py)!=null && !board.IsGemPinned(board.GetGem(px,py));
            if(clear) {top=y;left=x;}
        }
        Assert.That(top,Is.GreaterThanOrEqualTo(1),"fixture has an unrestrained response");
        foreach(int x in new[]{left,left+1}) board.GetGem(x,top).SetType(color,sprites[(int)color]);
        board.GetGem(left+3,top).SetType((GemType)(((int)color+2)%6),sprites[((int)color+2)%6]);
        board.GetGem(left+2,top).SetType((GemType)(((int)color+1)%6),sprites[((int)color+1)%6]);
        board.GetGem(left+2,top-1).SetType(color,sprites[(int)color]);
        safeMoveFrom=new Vector2Int(left+2,top-1);safeMoveTo=new Vector2Int(left+2,top);
        Set(board,"refillRandom",new SavedRandom(13579));
    }
    private IEnumerator Move(Action duringAccepted=null)
    {
        PrepareSafeMove();var board=Run.Board;int before=Run.MoveClock.Tick;
        Action<int> callback=_=>duringAccepted?.Invoke();board.ValidPlayerMoveAccepted+=callback;
        board.StartCoroutine((IEnumerator)Call(board,"TrySwap",board.GetGem(safeMoveFrom.x,safeMoveFrom.y),board.GetGem(safeMoveTo.x,safeMoveTo.y)));
        yield return Until(()=>Run.MoveClock.Tick==before+1 && Run.Continuation.CanCapture,"one accepted action settles");
        board.ValidPlayerMoveAccepted-=callback;
        Assert.That(board.CompletedValidPlayerMoves,Is.EqualTo(before+1));
    }
    [UnityTest] public IEnumerator ThinkingInvalidSwapsAndFreeActionsDoNotAdvanceResources()
    {
        yield return Launch();
        Run.Player.TryTakeDamage(25);Assert.That(Run.TryUsePotion(),Is.True);
        var energy=Run.Player.GetComponent<PlayerAbilityEnergy>();energy.AddEnergy(100);
        Assert.That(Run.Player.GetComponent<PlayerAbilityController>().TryActivate(),Is.True);
        var decree=Run.Player.GetComponent<RoyalDecreeRuntime>();Assert.That(decree.RemainingMoves,Is.EqualTo(3));
        var target=Enemy("orc_trailguard");var poison=target.gameObject.AddComponent<EnemyPoisonStatus>();poison.Apply(10,1,5);
        target.GetComponent<EnemyStagger>().ApplyStagger(2,2);
        yield return Stable();var before=Run.Continuation.Capture();
        yield return new WaitForSeconds(3.2f);
        var after=Run.Continuation.Capture();
        Assert.That(after.enemies.Select(JsonUtility.ToJson).ToArray(),Is.EqualTo(before.enemies.Select(JsonUtility.ToJson).ToArray()));
        Assert.That(after.player.health,Is.EqualTo(before.player.health));Assert.That(after.player.energy,Is.EqualTo(before.player.energy));
        Assert.That(Run.Cooldown(ConsumableKind.HealthPotion),Is.EqualTo(2));Assert.That(decree.RemainingMoves,Is.EqualTo(3));
        Assert.That(poison.RemainingDuration,Is.EqualTo(3));Assert.That(Run.MoveClock.Tick,Is.Zero);
        PrepareSafeMove();
        Run.Board.StartCoroutine((IEnumerator)Call(Run.Board,"TrySwap",Run.Board.GetGem(0,0),Run.Board.GetGem(1,0)));
        yield return new WaitForSeconds(.5f);yield return Stable();Assert.That(Run.MoveClock.Tick,Is.Zero,"invalid swap");
        yield return Move();Assert.That(Run.MoveClock.Tick,Is.EqualTo(1));
        Assert.That(Run.Cooldown(ConsumableKind.HealthPotion),Is.EqualTo(1));Assert.That(decree.RemainingMoves,Is.EqualTo(2));
        Assert.That(poison.RemainingDuration,Is.EqualTo(2));
    }
    [UnityTest] public IEnumerator ChannelGetsTwoFutureResponsesAndRecoveryAndStableResume()
    {
        yield return Launch();
        var mender=Enemy("elven_mender");var target=Enemy("orc_trailguard");
        target.ResolveDamageWithoutFeedback(30);
        yield return Move();yield return Move();
        var channel=mender.GetComponent<EnemyChannelRuntime>();
        Assert.That(channel.IsChanneling,Is.True);Assert.That(channel.Target,Is.SameAs(target));Assert.That(channel.ResponseMoves,Is.EqualTo(2));
        float basic=mender.GetComponent<EnemyAutoAttack>().RemainingAttackTime;
        Assert.That(Run.Continuation.SaveNow(),Is.True);
        var saved=Run.Continuation.Capture();string id=Run.RunId;long next=saved.clock.actions.nextActorId;
        Assert.That(Run.SuspendToMenu(),Is.True);yield return null;
        SceneManager.LoadScene("Game");yield return Until(()=>Run!=null && Run.Continuation!=null && !Run.Continuation.IsRestoring,"channel resumes");
        Run.GetComponent<RunControlsUI>().Close();yield return Stable();
        mender=Enemy("elven_mender");target=Enemy("orc_trailguard");channel=mender.GetComponent<EnemyChannelRuntime>();
        Assert.That(Run.RunId,Is.EqualTo(id));Assert.That(Run.MoveClock.Capture().actions.nextActorId,Is.EqualTo(next));
        Assert.That(channel.Target,Is.SameAs(target));Assert.That(channel.ResponseMoves,Is.EqualTo(2));
        yield return Move();Assert.That(channel.IsChanneling,Is.True);Assert.That(channel.ResponseMoves,Is.EqualTo(1));
        int hp=target.CurrentHealth;yield return Move();
        Assert.That(channel.Outcome,Is.EqualTo("Healed"));Assert.That(target.CurrentHealth,Is.EqualTo(Math.Min(target.MaxHealth,hp+20)));
        Assert.That(mender.GetComponent<EnemyAutoAttack>().RemainingAttackTime,Is.EqualTo(basic));
        yield return Move();Assert.That(channel.BlocksBasic,Is.True);
        yield return Move();Assert.That(channel.BlocksBasic,Is.False);
        Assert.That(mender.GetComponent<EnemyAutoAttack>().RemainingAttackTime,Is.EqualTo(basic),"recovery expiry grants no same-action basic");
    }
    [UnityTest] public IEnumerator DeadlineStaggerAndTargetDeathCancelExactlyOnce()
    {
        yield return Launch();var target=Enemy("orc_trailguard");var mender=Enemy("elven_mender");
        target.ResolveDamageWithoutFeedback(30);
        yield return Move();yield return Move();yield return Move();
        var channel=mender.GetComponent<EnemyChannelRuntime>();Assert.That(channel.ResponseMoves,Is.EqualTo(1));
        int hp=target.CurrentHealth;
        yield return Move(()=>mender.GetComponent<EnemyStagger>().ApplyStagger(2,2));
        Assert.That(channel.Outcome,Is.EqualTo("Interrupted"));Assert.That(target.CurrentHealth,Is.LessThanOrEqualTo(hp));
        var state=new EnemyCombatSnapshot();channel.CaptureContinuation(state,_=>0);Assert.That(state.channel.lastOutcomeSequence,Is.EqualTo(1));
        // A fresh channel fixture restores through its owner, then removes its
        // recipient and puts a different persistent actor in that same slot.
        var restored=new EnemyCombatSnapshot {channel=new EnemyChannelSnapshot{state=1,sequence=2,targetId=target.PersistentId,deadlineMove=Run.MoveClock.Tick+2}};
        channel.RestoreContinuation(restored,_=>target);
        int slot=Run.Waves.ContinuationSlot(target);long targetId=target.PersistentId;
        target.ResolveDirectDamage(9999);yield return Stable();
        Assert.That(channel.Outcome,Is.EqualTo("Target lost"));
        Assert.That(channel.IsChanneling,Is.False);
        Assert.That(Run.Waves.ContinuationEnemy(slot)==null || Run.Waves.ContinuationEnemy(slot).PersistentId!=targetId,Is.True);
        Assert.That(Run.Waves.TrySummonEnemy(Run.Zone.FindEnemy("Orc_Trailguard"),out var replacement),Is.True);
        Assert.That(Run.Waves.ContinuationSlot(replacement),Is.EqualTo(slot));Assert.That(replacement.PersistentId,Is.Not.EqualTo(targetId));
        replacement.ResolveDamageWithoutFeedback(25);int replacementHp=replacement.CurrentHealth;
        yield return Move();yield return Move();Assert.That(replacement.CurrentHealth,Is.LessThanOrEqualTo(replacementHp),"finished channel cannot heal the replacement");
        mender.ResolveDirectDamage(9999);yield return Stable();
        Assert.That(Run.MoveClock.Tick,Is.EqualTo(6),"free lethal actions create no tick");
    }
    [UnityTest] public IEnumerator FullRecipientAndSmallHitsKeepFixedTargetButDeadlineLethalCancels()
    {
        yield return Launch();
        var mender=Enemy("elven_mender");var target=Enemy("orc_trailguard");
        var channel=mender.GetComponent<EnemyChannelRuntime>();
        target.ResolveDamageWithoutFeedback(30);
        channel.RestoreContinuation(new EnemyCombatSnapshot{channel=new EnemyChannelSnapshot
            {state=1,sequence=1,targetId=target.PersistentId,deadlineMove=2}},_=>target);
        mender.ResolveDirectDamage(5);
        Assert.That(channel.IsChanneling,Is.True,"a hit below stagger threshold is not an interrupt");
        target.RestoreHealth(999);
        yield return Move();
        Assert.That(channel.Target,Is.SameAs(target),"a full recipient does not cause retargeting");
        yield return Move();
        Assert.That(channel.Outcome,Is.EqualTo("Healed"));
        var outcome=new EnemyCombatSnapshot();channel.CaptureContinuation(outcome,_=>0);
        Assert.That(outcome.channel.lastOutcomeSequence,Is.EqualTo(1));

        target.ResolveDamageWithoutFeedback(25);int health=target.CurrentHealth;
        channel.RestoreContinuation(new EnemyCombatSnapshot{channel=new EnemyChannelSnapshot
            {state=1,sequence=2,targetId=target.PersistentId,deadlineMove=Run.MoveClock.Tick+1}},_=>target);
        string terminal=null;int terminals=0;
        channel.Changed+=()=>{if(!channel.IsChanneling){terminal=channel.Outcome;terminals++;}};
        yield return Move(()=>mender.ResolveDamageWithoutFeedback(9999));
        Assert.That(terminal,Is.EqualTo("Caster defeated"),"player-side lethal on the deadline precedes enemy completion");
        Assert.That(terminals,Is.EqualTo(1));
        Assert.That(target.CurrentHealth,Is.LessThanOrEqualTo(health),"the removed caster cannot heal later in that same action");
    }
    [UnityTest] public IEnumerator VinesAreOverlaysWithOrdinarySwapGravityClearAndSave()
    {
        yield return Launch(1,true);QuietKitFixture();
        Enemy("orc_rootbinder").SetSpecialTurnRequirement(100);
        var board=Run.Board;PrepareSafeMove();
        var gem=board.GetGem(safeMoveFrom.x,safeMoveFrom.y);
        board.QueueEnvironmentalVine(gem);yield return Stable();
        Assert.That(board.IsGemPinned(gem),Is.False);Assert.That(board.RestrictionCount,Is.Zero);
        Assert.That(board.IsHintMoveStillValid(gem,board.GetGem(safeMoveTo.x,safeMoveTo.y)),Is.True);
        yield return Move();Assert.That(Run.MoveClock.Tick,Is.EqualTo(1));
        Assert.That(board.GetGem(safeMoveFrom.x,safeMoveFrom.y),Is.Not.Null,"normal gravity/refill reaches a vine cell");
        board.QueueEnvironmentalVine(board.GetGem(0,0));yield return Stable();
        int deadline=board.NextVineGrowthMove;
        Assert.That(Run.SuspendToMenu(),Is.True);yield return null;SceneManager.LoadScene("Game");
        yield return Until(()=>Run?.Continuation!=null&&!Run.Continuation.IsRestoring,"overlay save resumes");
        Run.GetComponent<RunControlsUI>().Close();yield return Stable();QuietKitFixture();board=Run.Board;
        Assert.That(board.IsCellVined(0,0),Is.True);Assert.That(board.NextVineGrowthMove,Is.EqualTo(deadline));
        int tick=Run.MoveClock.Tick;
        Assert.That(Run.TryUseBomb(board.GetGem(0,0)),Is.True);yield return Stable();
        Assert.That(board.IsCellVined(0,0),Is.False);Assert.That(Run.MoveClock.Tick,Is.EqualTo(tick));
        Assert.That(board.RestrictionCount,Is.Zero);
    }
    [UnityTest] public IEnumerator ZoneGrowthAndSurgeHaveIndependentCadenceAndBoundedFrontiers()
    {
        yield return Launch(1,true);QuietKitFixture();var board=Run.Board;
        var owner=Enemy("orc_rootbinder");owner.SetSpecialTurnRequirement(100);
        yield return board.AdvanceVineNetworks(1);Assert.That(board.VineCount,Is.Zero);
        yield return board.AdvanceVineNetworks(2);yield return Stable();Assert.That(board.VineCount,Is.EqualTo(1));
        var edge=board.CaptureContinuation(Run.Waves.ContinuationOwnerSlot).vines.Single();
        Assert.That(edge.x==0||edge.y==0||edge.x==board.Width-1||edge.y==board.Height-1,Is.True);
        int deadline=board.NextVineGrowthMove;
        Assert.That(board.QueueVineSurge(owner,null),Is.True);yield return Stable();
        Assert.That(board.VineCount,Is.InRange(2,5));Assert.That(board.NextVineGrowthMove,Is.EqualTo(deadline));
        for(int i=0;i<12;i++) {board.QueueVineSurge(owner,null);yield return Stable();}
        Assert.That(board.VineCount,Is.LessThanOrEqualTo(Run.Zone.Definition.maximumVineOverlays));
        Assert.That(board.RestrictionCount,Is.Zero);Assert.That(board.TryGetRandomHintMove(out _,out _),Is.True);
        owner.ResolveDirectDamage(9999);yield return Stable();
        Assert.That(board.CaptureContinuation(Run.Waves.ContinuationOwnerSlot).vines.All(n=>n.environmental),Is.True);
        board.RemoveVineSource(null);yield return Stable();Assert.That(board.VineCount,Is.Zero);
    }
    [UnityTest] public IEnumerator AcceptedSwapReplayProducesSameLogicalStateExactlyOnce()
    {
        yield return Launch();PrepareSafeMove();Assert.That(Run.Continuation.SaveNow(),Is.True);
        var board=Run.Board;
        board.StartCoroutine((IEnumerator)Call(board,"TrySwap",board.GetGem(2,board.Height-2),board.GetGem(2,board.Height-1)));
        Assert.That(Run.Continuation.SaveNow(),Is.True);string pending=File.ReadAllText(path);
        yield return Until(()=>Run.MoveClock.Tick==1 && Run.Continuation.CanCapture,"uninterrupted action");
        var expected=Run.Continuation.Capture();
        SceneManager.LoadScene("MainMenu");yield return null;
        File.WriteAllText(path,pending);profile.Dispose();profile=AccountProgression.UseDisposableProfile(path);
        SceneManager.LoadScene("Game");yield return null;
        yield return Until(()=>Run!=null && Run.Continuation!=null&&!Run.Continuation.IsRestoring,"pending action replay");
        Assert.That(Run.Continuation.Error,Is.Null);Run.GetComponent<RunControlsUI>().Close();yield return Stable();
        var resumed=Run.Continuation.Capture();
        File.WriteAllText(".utmp/ForestValidation/replay-expected.json",JsonUtility.ToJson(expected,true));
        File.WriteAllText(".utmp/ForestValidation/replay-resumed.json",JsonUtility.ToJson(resumed,true));
        Assert.That(resumed.clock.actions.completed,Is.EqualTo(1));
        Assert.That(JsonUtility.ToJson(resumed.board),Is.EqualTo(JsonUtility.ToJson(expected.board)));
        Assert.That(resumed.enemies.Select(JsonUtility.ToJson).ToArray(),Is.EqualTo(expected.enemies.Select(JsonUtility.ToJson).ToArray()));
        Assert.That(JsonUtility.ToJson(resumed.player),Is.EqualTo(JsonUtility.ToJson(expected.player)));
        Assert.That(Run.Continuation.SaveNow(),Is.True);
        yield return new WaitForSeconds(1);Assert.That(Run.MoveClock.Tick,Is.EqualTo(1));
    }
    [UnityTest] public IEnumerator MixedColorDamageKeepsAttributionAndFinalFiveStepRounding()
    {
        yield return Launch();var emerald=Enemy("orc_trailguard");var ruby=Enemy("elven_mender");
        emerald.AssignGemType(GemType.Emerald);ruby.AssignGemType(GemType.Ruby);
        Enemy("elven_scout").AssignGemType(GemType.Sapphire);
        var combat=UnityEngine.Object.FindFirstObjectByType<CombatController>();
        int first=emerald.CurrentHealth,second=ruby.CurrentHealth;
        Assert.That(combat.ResolveFixedGemDamage(new BoardClearContext(GemType.Emerald,1,0,BoardClearSource.Bomb),30),Is.True);
        Assert.That(combat.ResolveFixedGemDamage(new BoardClearContext(GemType.Ruby,1,0,BoardClearSource.Bomb),30),Is.True);
        Assert.That(first-emerald.CurrentHealth,Is.EqualTo(35));Assert.That(second-ruby.CurrentHealth,Is.EqualTo(30));
        Assert.That(Run.MoveClock.Tick,Is.Zero,"free attributed clear packets are not manual actions");
        int hp=Run.Player.CurrentHealth;combat.ResolveFixedGemDamage(new BoardClearContext(GemType.Amethyst,1,0,BoardClearSource.Bomb),30);
        Assert.That(Run.Player.CurrentHealth,Is.EqualTo(hp),"resonance does not change off-target or affinity routing");
    }
    [UnityTest] public IEnumerator FreeBardleyCastsAndTheirChainsLeaveDeadlinesAndReadinessUnchanged()
    {
        character.Dispose();character=CharacterSelectionSettings.UseTemporarySelection("bardley");
        yield return Launch();
        foreach(var enemy in Run.Waves.ActiveEnemies) Set(enemy,"currentHealth",9995);
        var energy=Run.Player.GetComponent<PlayerAbilityEnergy>();
        var ability=Run.Player.GetComponent<PlayerAbilityController>();
        var clocks=Run.Waves.ActiveEnemies.Select(e=>e.GetComponent<EnemyAutoAttack>().RemainingAttackTime).ToArray();
        int clears=0;Run.Board.BoardClearResolved+=_=>clears++;
        for(int cast=0;cast<2;cast++)
        {
            energy.AddEnergy(100);Assert.That(ability.TryActivate(),Is.True);
            yield return Until(()=>!ability.IsAbilityActive && Run.Continuation.CanCapture,"free cast and chain settle");
            Assert.That(Run.MoveClock.Tick,Is.Zero);
            Assert.That(Run.Waves.ActiveEnemies.Select(e=>e.GetComponent<EnemyAutoAttack>().RemainingAttackTime).ToArray(),Is.EqualTo(clocks));
        }
        Assert.That(clears,Is.GreaterThan(1),"actual ability and chained clears ran");
    }
    [UnityTest] public IEnumerator LargeCascadeAndSpecialChainConsumeOneManualAction()
    {
        yield return Launch();var board=Run.Board;var sprites=(Sprite[])Get(board,"gemSprites");
        foreach(var enemy in Run.Waves.ActiveEnemies) Set(enemy,"currentHealth",9995);
        for(int x=0;x<board.Width;x++)for(int y=0;y<board.Height;y++)
        {var gem=board.GetGem(x,y);gem.SetSpecialType(GemSpecialType.None);gem.SetType(GemType.Ruby,sprites[(int)GemType.Ruby]);}
        board.GetGem(2,0).SetType(GemType.Sapphire,sprites[(int)GemType.Sapphire]);
        board.GetGem(5,3).SetSpecialType(GemSpecialType.RowBomb);
        board.GetGem(6,3).SetSpecialType(GemSpecialType.ColumnBomb);
        Set(board,"refillRandom",new SavedRandom(13579));
        int accepted=0,completed=0,deepest=0,clears=0;
        board.ValidPlayerMoveAccepted+=_=>accepted++;board.ValidPlayerMoveCompleted+=_=>completed++;
        board.BoardClearResolved+=c=>{deepest=Math.Max(deepest,c.CascadeDepth);clears+=c.GemCount;};
        board.StartCoroutine((IEnumerator)Call(board,"TrySwap",board.GetGem(2,0),board.GetGem(2,1)));
        yield return Until(()=>Run.MoveClock.Tick==1 && Run.Continuation.CanCapture,"large cascade settles");
        Assert.That(accepted,Is.EqualTo(1));Assert.That(completed,Is.EqualTo(1));
        Assert.That(clears,Is.GreaterThan(40));Assert.That(deepest,Is.GreaterThanOrEqualTo(2),"at least three cascade stages");
    }
    [UnityTest] public IEnumerator RoyalCommandConsumesParticipantsOnceIncludingFollowUpHits()
    {
        yield return Launch();
        Enemy("orc_trailguard").ResolveDirectDamage(9999);Enemy("elven_mender").ResolveDirectDamage(9999);
        yield return Stable();
        Assert.That(Run.Waves.TrySummonEnemy(AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Game/Data/Enemies/Enemy_King.asset"),out var king),Is.True);
        Assert.That(Run.Waves.TrySummonEnemy(AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Game/Data/Enemies/Enemy_RoyalLancer.asset"),out var lancer),Is.True);
        yield return Until(()=>Run.Waves.ActiveEnemies.All(e=>e.GetComponent<EnemyLifecycleVFX>()?.IsSpawning!=true),"command formation spawned");
        Set(king.GetComponent<KingEnemyAbility>(),"cycle",1);king.SetSpecialTurnRequirement(1);
        Set(lancer.GetComponent<EnemyAutoAttack>(),"remainingAttackTime",1f);
        Set(Enemy("elven_scout").GetComponent<EnemyAutoAttack>(),"remainingAttackTime",100f);
        int kingHits=0,lancerHits=0;
        king.GetComponent<EnemyAutoAttack>().AttackResolved+=(_,__,___)=>kingHits++;
        lancer.GetComponent<EnemyAutoAttack>().AttackResolved+=(_,__,___)=>lancerHits++;
        Run.Player.GrantShield(10);int playerHp=Run.Player.CurrentHealth;
        yield return Move();
        Assert.That(kingHits,Is.EqualTo(1));
        Assert.That(lancerHits,Is.EqualTo(lancer.RuntimeStats.FollowUpDamage>0?2:1),"commanded participant receives no extra ordinary sequence");
        Assert.That(king.GetComponent<EnemyAutoAttack>().HasCommandReservation,Is.False);
        Assert.That(lancer.GetComponent<EnemyAutoAttack>().HasCommandReservation,Is.False);
        Assert.That(Run.Player.CurrentHealth,Is.LessThan(playerHp));Assert.That(Run.Player.CurrentShield,Is.Zero);
        Assert.That(Run.MoveClock.Tick,Is.EqualTo(1));

        // A legitimate stagger during command windup must release the turn,
        // because its move duration cannot expire inside that same turn.
        king.GetComponent<KingEnemyAbility>().CommandIssued+=_=>lancer.GetComponent<EnemyStagger>().ApplyStagger(2,2);
        Set(king.GetComponent<KingEnemyAbility>(),"cycle",1);king.SetSpecialTurnRequirement(1);
        int previousLancerHits=lancerHits;
        yield return Move();
        Assert.That(lancerHits,Is.EqualTo(previousLancerHits));
        Assert.That(lancer.GetComponent<EnemyAutoAttack>().HasCommandReservation,Is.False);
        Assert.That(Run.MoveClock.IsBlockingWaveProgression,Is.False);
    }
    [UnityTest] public IEnumerator UnsupportedProfilePreservesDurableRunAndDoesNotStartLegacyCombat()
    {
        yield return Launch();Assert.That(Run.Continuation.SaveNow(),Is.True);
        Assert.That(Run.SuspendToMenu(),Is.True);yield return null;
        var saved=JsonUtility.FromJson<AccountSave>(File.ReadAllText(path));
        saved.run.checkpoint.clock.profile="future-unavailable-profile";
        string original=JsonUtility.ToJson(saved,true);File.WriteAllText(path,original);
        profile.Dispose();profile=AccountProgression.UseDisposableProfile(path);
        SceneManager.LoadScene("Game");yield return null;
        yield return Until(()=>Run?.Continuation?.Error!=null,"explicit incompatibility");
        Assert.That(Run.MoveClock,Is.Null);Assert.That(Time.timeScale,Is.Zero);
        Assert.That(Run.Continuation.SaveNow(),Is.False);Assert.That(Run.Player.GetComponent<PlayerAbilityController>().CanActivate,Is.False);
        yield return new WaitForSecondsRealtime(.4f);
        Assert.That(File.ReadAllText(path),Is.EqualTo(original),"unsupported save is retained byte-for-byte");
    }
    [UnityTest] public IEnumerator PhotoKeepsPresentGrowthDeadlineAndWarningsWhileCasterRemovalCancelsHeal()
    {
        yield return Launch();var board=Run.Board;PrepareSafeMove();
        board.QueueEnvironmentalVine(board.GetGem(0,1));yield return Stable();
        var photo=board.CaptureBoardMemory(Run.Waves.ContinuationOwnerSlot);
        yield return Move();yield return board.AdvanceVineNetworks(2);yield return Stable();
        int deadline=board.NextVineGrowthMove;
        Assert.That(board.TryRestoreBoardMemory(photo,Run.Waves.ContinuationEnemy),Is.True);yield return Stable();
        Assert.That(board.NextVineGrowthMove,Is.EqualTo(deadline));Assert.That(Run.MoveClock.Tick,Is.EqualTo(1));
        var mender=Enemy("elven_mender");var trail=Enemy("orc_trailguard");
        BoardController.GemSetThreat one=null,two=null;
        board.TryQueueRootWarning(mender,1,1,false,false,w=>one=w);yield return Stable();
        board.TryQueueRootWarning(trail,1,1,false,false,w=>two=w);yield return Stable();
        Assert.That(one,Is.Not.Null);Assert.That(two,Is.Not.Null);Assert.That(two.DueMove,Is.GreaterThan(one.DueMove));
        trail.ResolveDamageWithoutFeedback(30);var channel=mender.GetComponent<EnemyChannelRuntime>();
        channel.RestoreContinuation(new EnemyCombatSnapshot{channel=new EnemyChannelSnapshot{state=1,sequence=1,targetId=trail.PersistentId,deadlineMove=3}},_=>trail);
        int health=trail.CurrentHealth;string outcome=null;channel.Changed+=()=>outcome=channel.Outcome;
        mender.ResolveDamageWithoutFeedback(9999);yield return Stable();
        Assert.That(outcome,Is.EqualTo("Caster defeated"));Assert.That(trail.CurrentHealth,Is.EqualTo(health));
    }
}
