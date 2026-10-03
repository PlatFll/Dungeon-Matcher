using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed partial class ForestFoundationPlayTests
{
    private void PreserveRoster()
    {
        QuietKitFixture();
        foreach(var e in Run.Waves.ActiveEnemies) Set(e,"currentHealth",9995);
    }
    private IEnumerator ResumeRoster()
    {
        Assert.That(Run.SuspendToMenu(),Is.True);yield return null;
        SceneManager.LoadScene("Game");
        yield return Until(()=>Run?.Continuation!=null&&!Run.Continuation.IsRestoring,"roster continuation");
        Run.GetComponent<RunControlsUI>().Close();yield return Stable();QuietKitFixture();
    }
    private float RosterSpeed(EnemyActor e) =>
        ((Dictionary<object,float>)Get(e.GetComponent<EnemyAutoAttack>(),"speedModifiers")).Values.Aggregate(1f,(a,b)=>a*b);

    [UnityTest] public IEnumerator RosterRageTriggersStrictlyBelowHalfOnceAndResumes()
    {
        yield return Launch(14,true);QuietKitFixture();
        var e=Enemy("orc_berserker");var kit=e.GetComponent<ForestCombatAbility>();
        e.ResolveDirectDamage(e.MaxHealth/2);
        Assert.That(kit.IsEnraged,Is.False,"exactly half is not below half");
        e.ResolveDirectDamage(5);Assert.That(kit.IsEnraged,Is.True);
        Assert.That(RosterSpeed(e),Is.EqualTo(1.4f).Within(.001));
        e.RestoreHealth(999);e.ResolveDirectDamage(5);
        Assert.That(RosterSpeed(e),Is.EqualTo(1.4f).Within(.001),"rage cannot stack");
        yield return Stable();yield return ResumeRoster();e=Enemy("orc_berserker");
        Assert.That(e.GetComponent<ForestCombatAbility>().IsEnraged,Is.True);
        Assert.That(RosterSpeed(e),Is.EqualTo(1.4f).Within(.001));
        e.RestoreHealth(999);
        Assert.That(e.GetComponent<ForestCombatAbility>().IsEnraged,Is.True,"healing does not reset once-only rage");
    }

    [UnityTest] public IEnumerator RosterSummonWaitsForRoomHasOneOwnerAndSurvivesCaller()
    {
        yield return Launch(21,true);PreserveRoster();
        var caller=Enemy("orc_bloomcaller");var kit=caller.GetComponent<ForestCombatAbility>();
        for(int i=0;i<4;i++) yield return Move();
        Assert.That(kit.HasLivingSummon,Is.False);Assert.That(Run.Waves.ActiveEnemies.Count,Is.EqualTo(3));
        Enemy("elven_scout").ResolveDirectDamage(20000);yield return Stable();
        yield return Move();QuietKitFixture();
        Assert.That(kit.HasLivingSummon,Is.True);
        var summon=Enemy("snapvine");long summonId=summon.PersistentId;
        Assert.That(summon.MaxHealth,Is.EqualTo(20));Assert.That(summon.Definition.HasSpecialAbility,Is.False);
        yield return ResumeRoster();caller=Enemy("orc_bloomcaller");kit=caller.GetComponent<ForestCombatAbility>();
        Assert.That(kit.HasLivingSummon,Is.True);Assert.That(Enemy("snapvine").PersistentId,Is.EqualTo(summonId));
        PreserveRoster();
        for(int i=0;i<4;i++) yield return Move();
        Assert.That(Run.Waves.ActiveEnemies.Count(e=>e.Definition.EnemyId=="snapvine"),Is.EqualTo(1));
        caller.ResolveDirectDamage(20000);yield return Stable();
        Assert.That(Enemy("snapvine").PersistentId,Is.EqualTo(summonId));
    }

    [UnityTest] public IEnumerator RosterRhythmBuffsOnlyAlliesRefreshesAndExpires()
    {
        yield return Launch(17,true);PreserveRoster();var drummer=Enemy("orc_drummer");var scout=Enemy("elven_scout");
        for(int i=0;i<4;i++) yield return Move();
        Assert.That(RosterSpeed(drummer),Is.EqualTo(1));Assert.That(RosterSpeed(scout),Is.EqualTo(1.4f).Within(.001));
        var kit=drummer.GetComponent<ForestCombatAbility>();
        // Two refreshes must keep a single multiplier, including after Continue.
        Call(kit,"ApplyRhythm");Call(kit,"ApplyRhythm");
        Assert.That(RosterSpeed(scout),Is.EqualTo(1.4f).Within(.001));
        yield return ResumeRoster();drummer=Enemy("orc_drummer");scout=Enemy("elven_scout");kit=drummer.GetComponent<ForestCombatAbility>();
        Assert.That(RosterSpeed(scout),Is.EqualTo(1.4f).Within(.001));
        // This isolated hybrid fixture deliberately uses move-based durations.
        PreserveRoster();for(int i=0;i<3;i++) yield return Move();
        Assert.That(RosterSpeed(scout),Is.EqualTo(1));
        scout.ResolveDirectDamage(20000);yield return Stable();
        for(int i=0;i<4;i++) yield return Move();
        var saved=new EnemyCombatSnapshot();kit.CaptureContinuation(saved,_=>0);
        Assert.That(saved.forestRoster.buffSeconds,Is.Zero,"solo rhythm waits");
    }

    [UnityTest] public IEnumerator RosterThornsHaveOneSafeSideAndOnlyDeliberateOrdinaryClearsRetaliate()
    {
        yield return Launch(13,true);PreserveRoster();var owner=Enemy("elven_thornkeeper");var board=Run.Board;
        foreach(int mode in new[]{0,1,2,3})
        {
            PrepareSafeMove();Assert.That(board.TryQueuePlaceBarricades(owner,1,2,1,EnemyBarricadeStyle.Thorn,false,true),Is.True);
            yield return Stable();
            var cell=board.CaptureContinuation(Run.Waves.ContinuationOwnerSlot).cells.Single(c=>c.barricade&&c.barricadeStyle==EnemyBarricadeStyle.Thorn);
            int safe=cell.thornSafeSide;Assert.That(safe,Is.GreaterThan(0));Assert.That(safe&(safe-1),Is.Zero);
            var directions=(Vector2Int[])typeof(BoardController).GetField("BarricadeHitDirections",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static).GetValue(null);
            int side=Enumerable.Range(0,4).First(i=>((safe&(1<<i))!=0)==(mode==0)&&board.GetGem(cell.x+directions[i].x,cell.y+directions[i].y)!=null);
            var target=board.GetGem(cell.x+directions[side].x,cell.y+directions[side].y);
            target.SetSpecialType(mode==3?GemSpecialType.RowBomb:GemSpecialType.None);
            int hp=Run.Player.CurrentHealth;
            Call(board,"DamageBarricadesForClear",new HashSet<Gem>{target},null,mode!=2);
            Assert.That(hp-Run.Player.CurrentHealth,Is.EqualTo(mode==1?10:0),"safe/cascade/special clear is harmless");
            Assert.That(board.CaptureContinuation(Run.Waves.ContinuationOwnerSlot).cells.Any(c=>c.barricade&&c.x==cell.x&&c.y==cell.y),Is.False);
            yield return (IEnumerator)Call(board,"CollapseAndRefillBoard");yield return Stable();
        }
    }

    [UnityTest] public IEnumerator RosterRhythmUsesSecondsInLiveProfileAndTwoDrummersNeverStack()
    {
        yield return Launch(17,true);QuietKitFixture();
        ((CombatClockSnapshot)Get(Run.MoveClock,"state")).profile=CombatClockSnapshot.LegacyEffectsProfile;
        var first=Enemy("orc_drummer");var scout=Enemy("elven_scout");
        Assert.That(Run.Waves.TrySummonEnemy(first.Definition,out var second),Is.True);QuietKitFixture();
        Call(first.GetComponent<ForestCombatAbility>(),"ApplyRhythm");
        Call(second.GetComponent<ForestCombatAbility>(),"ApplyRhythm");
        Assert.That(RosterSpeed(scout),Is.EqualTo(1.4f).Within(.001));
        Assert.That(RosterSpeed(first),Is.EqualTo(1.4f).Within(.001),"only the other drummer buffs this caster");
        Time.timeScale=0;yield return new WaitForSecondsRealtime(.2f);
        Assert.That(RosterSpeed(scout),Is.EqualTo(1.4f).Within(.001));Time.timeScale=1;
        first.ResolveDirectDamage(20000);yield return Stable();
        Assert.That(RosterSpeed(scout),Is.EqualTo(1.4f).Within(.001),"other caster still holds a lease");
        float expiryStart=Time.time;
        yield return Until(()=>Time.time>=expiryStart+5.1f,"five seconds of live simulation");
        var liveKit=second.GetComponent<ForestCombatAbility>();var liveSaved=new EnemyCombatSnapshot();
        liveKit.CaptureContinuation(liveSaved,_=>0);
        Assert.That(RosterSpeed(scout),Is.EqualTo(1),
            $"seconds={liveSaved.forestRoster.buffSeconds}, moves={CombatMoveClock.MoveEffects}, active={liveKit.isActiveAndEnabled}, released={Get(liveKit,"released")}");
        Assert.That(Run.MoveClock.Tick,Is.Zero);
    }

    [UnityTest] public IEnumerator RosterThornSafeSideCanBeBrokenByItsAdvertisedRealSwap()
    {
        yield return Launch(13,true);PreserveRoster();PrepareSafeMove();
        var board=Run.Board;var owner=Enemy("elven_thornkeeper");
        board.TryQueuePlaceBarricades(owner,1,2,1,EnemyBarricadeStyle.Thorn,false,true);yield return Stable();
        var cell=board.CaptureContinuation(Run.Waves.ContinuationOwnerSlot).cells.Single(c=>c.barricade);
        var dirs=(Vector2Int[])typeof(BoardController).GetField("BarricadeHitDirections",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static).GetValue(null);
        var safe=new Vector2Int(cell.x,cell.y)+dirs[Enumerable.Range(0,4).Single(i=>(cell.thornSafeSide&(1<<i))!=0)];
        var response=board.GetImmediateResponses().First(r=>!r.UsesSpecial && r.Clears.Any(g=>{
            var at=g==r.Source?r.Target:g==r.Target?r.Source:g;
            return at.Column==safe.x && at.Row==safe.y;
        }));
        int damage=0;Run.Player.DamageTaken+=(_,n)=>damage+=n;int tick=Run.MoveClock.Tick;
        board.StartCoroutine((IEnumerator)Call(board,"TrySwap",response.Source,response.Target));
        yield return Until(()=>Run.MoveClock.Tick==tick+1 && Run.Continuation.CanCapture,"advertised thorn answer settles");
        Assert.That(damage,Is.Zero,"safe-side match and all its cascades remain harmless");
        Assert.That(board.CaptureContinuation(Run.Waves.ContinuationOwnerSlot).cells.Any(c=>c.barricade&&c.x==cell.x&&c.y==cell.y),Is.False);
    }

    [UnityTest] public IEnumerator RosterPressureStaggerCancelsWithoutDamageAndWarningDeadlinesSeparate()
    {
        yield return Launch(19,true);PreserveRoster();PrepareSafeMove();
        var board=Run.Board;var treant=Enemy("ancient_treant");var scout=Enemy("elven_scout");
        BoardController.CellResponseThreat first=null,second=null;
        board.TryQueueCellResponse(treant,3,2,false,t=>first=t);yield return Stable();
        board.TryQueueCellResponse(scout,3,2,false,t=>second=t);yield return Stable();
        Assert.That(first,Is.Not.Null);Assert.That(second,Is.Not.Null);
        Assert.That(second.DueMove,Is.GreaterThanOrEqualTo(first.DueMove+2));
        var available=(List<Vector2Int>)Call(board,"BuildBarricadableCellList",true);
        foreach(var cell in first.Cells.Concat(second.Cells))
        {
            Assert.That(available,Has.No.Member(cell),"new structures cannot bury a response mark");
            Assert.That((bool)Call(board,"IsProtectedWarningTarget",board.GetGem(cell.x,cell.y)),Is.True);
        }
        var kit=treant.GetComponent<ForestPressureAbility>();
        kit.RestoreContinuation(new EnemyCombatSnapshot{forestRoster=new ForestRosterSnapshot{cycle=1}},_=>null);
        int hp=Run.Player.CurrentHealth;treant.GetComponent<EnemyStagger>().RestoreContinuation(new EnemyCombatSnapshot());
        treant.GetComponent<EnemyStagger>().ApplyStagger(2,2);
        Assert.That(kit.IsPreparing,Is.False);Assert.That(first.Ended,Is.True);
        Assert.That(Run.Player.CurrentHealth,Is.EqualTo(hp));
        Assert.That(treant.GetComponent<EnemyAutoAttack>().IsPausedByAction,Is.False);
    }

    [UnityTest] public IEnumerator RosterVolleyKeepsCancelledShotsCancelledThroughRegrowthAndResume()
    {
        yield return Launch(18,true);PreserveRoster();var board=Run.Board;var archer=Enemy("briar_archer");
        PrepareSafeMove();
        for(int y=0;y<board.Height;y++)for(int x=0;x<board.Width;x++)board.QueueEnvironmentalVine(board.GetGem(x,y));
        yield return Stable();
        BoardController.CellResponseThreat warning=null;
        Assert.That(board.TryQueueCellResponse(archer,3,2,true,t=>warning=t),Is.True);yield return Stable();
        Assert.That(warning,Is.Not.Null);Assert.That(warning.Cells.Count,Is.EqualTo(3));Assert.That(warning.DueMove,Is.GreaterThanOrEqualTo(2));
        var answered=warning.Cells[0];var gem=board.GetGem(answered.x,answered.y);
        Call(board,"ClearVinesForDestruction",new HashSet<Gem>{gem},null);
        board.QueueEnvironmentalVine(gem);yield return Stable();
        Assert.That(warning.Cells.Count,Is.EqualTo(2));Assert.That(board.IsCellVined(answered.x,answered.y),Is.True);
        yield return ResumeRoster();board=Run.Board;archer=Enemy("briar_archer");warning=board.RestoredCellResponse(archer);
        Assert.That(warning.Cells.Count,Is.EqualTo(2));Assert.That(warning.Answered,Is.True);
        int shots=-1,calls=0;Set(board,"completedValidPlayerMoves",warning.DueMove);
        Assert.That(board.TryQueueResolveCellResponse(warning,(n,a)=>{shots=n;calls++;},_=>{}),Is.True);yield return Stable();
        Assert.That(shots,Is.EqualTo(2));Assert.That(calls,Is.EqualTo(1));
        Assert.That(board.TryQueueResolveCellResponse(warning,(n,a)=>calls++,_=>{}),Is.False);
    }

    [UnityTest] public IEnumerator RosterTreantOrdinaryShieldGatesDamageAndBreakStaggers()
    {
        yield return Launch(19,true);PreserveRoster();var treant=Enemy("ancient_treant");
        for(int i=0;i<3;i++) yield return Move();
        Assert.That(treant.CurrentShield,Is.EqualTo(30));int hp=treant.CurrentHealth;
        treant.GetComponent<EnemyStagger>().RestoreContinuation(new EnemyCombatSnapshot());
        treant.ResolveDirectDamage(100);
        Assert.That(treant.CurrentShield,Is.Zero);Assert.That(treant.CurrentHealth,Is.EqualTo(hp));
        Assert.That(treant.GetComponent<EnemyStagger>().IsStaggered,Is.True);
    }

    [UnityTest] public IEnumerator RosterBoughAnswerWeakensHitAndNeverClearsBoard()
    {
        yield return Launch(19,true);PreserveRoster();var board=Run.Board;var treant=Enemy("ancient_treant");PrepareSafeMove();
        BoardController.CellResponseThreat warning=null;
        board.TryQueueCellResponse(treant,3,2,false,t=>warning=t);yield return Stable();Assert.That(warning,Is.Not.Null);
        var a=warning.Cells[0];Assert.That(warning.Cells.All(c=>Mathf.Abs(c.x-a.x)+Mathf.Abs(c.y-a.y)<=1),Is.True);
        Call(board,"ClearVinesForDestruction",new HashSet<Gem>{board.GetGem(a.x,a.y)},null);
        Assert.That(warning.Answered,Is.True);
        yield return ResumeRoster();board=Run.Board;treant=Enemy("ancient_treant");
        var kit=treant.GetComponent<ForestPressureAbility>();Assert.That(kit.IsPreparing,Is.True);
        Assert.That(treant.GetComponent<EnemyAutoAttack>().IsPausedByAction,Is.True);
        warning=board.RestoredCellResponse(treant);Set(board,"completedValidPlayerMoves",warning.DueMove);
        var before=board.CaptureContinuation(Run.Waves.ContinuationOwnerSlot).cells.Select(c=>c.type).ToArray();
        int hp=Run.Player.CurrentHealth;
        board.TryQueueResolveCellResponse(warning,(n,answered)=>Call(kit,"Impact",n,answered),_=>{});yield return Stable();
        Assert.That(hp-Run.Player.CurrentHealth,Is.EqualTo(15));
        Assert.That(board.CaptureContinuation(Run.Waves.ContinuationOwnerSlot).cells.Select(c=>c.type).ToArray(),Is.EqualTo(before));
    }
}
