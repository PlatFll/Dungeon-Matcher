using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public sealed partial class ForestFoundationPlayTests
{
    private IEnumerator RoyalCofferFixture()
    {
        yield return LaunchCourt("queen_nacre","shellback_porter");yield return Move();PrepareSafeMove();
        var board=Run.Board;board.Aquatic.StartFlood(18,Run.MoveClock.Tick);
        board.Aquatic.bubbles=Enumerable.Range(0,5).Select(x=>board.GetGem(x,0).BoardIdentity).ToList();
        ReadyForest(Enemy("queen_nacre"));yield return Move();
        Assert.That(board.Aquatic.bubbles,Is.Empty);Assert.That(board.Aquatic.coffer.charges,Is.EqualTo(5));
        Assert.That(board.Aquatic.coffer.royal,Is.True);
        Assert.That(Enemy("queen_nacre").GetComponent<AquaticEnemyAbility>().IsPreparing,Is.False);
        PrepareSafeMove();
    }
    [UnityTest] public IEnumerator CourtRevisionRoyalSlideRotatesSpecialsBeforeOneSettleAndContinues()
    {yield return CourtRoyalSlide();}
    private IEnumerator CourtRoyalSlide()
    {
        yield return RoyalCofferFixture();var board=Run.Board;var coffer=board.Aquatic.coffer;
        var origin=new Vector2Int(coffer.x,coffer.y);
        var grid=(Gem[,])Get(board,"gems");
        var before=grid.Cast<Gem>().Where(g=>g!=null).ToDictionary(g=>new Vector2Int(g.Column,g.Row));
        var victim=new[]{Vector2Int.left,Vector2Int.right,Vector2Int.up,Vector2Int.down}
            .Select(d=>board.GetGem(origin.x+d.x,origin.y+d.y)).First(g=>g!=null);
        // Every surviving route contains actual specials; the slide may move them,
        // but may not activate/delete them until its complete rotation settles.
        foreach(var pair in before)if(pair.Value!=victim)pair.Value.SetSpecialType(GemSpecialType.ColorCrystal);
        int slides=0;bool held=false;int countBefore=before.Count;
        board.CofferMoving+=(start,end,duration)=>
        {
            slides++;held=board.IsBusy&&!Run.Continuation.CanCapture;
            var target=new Vector2Int(board.Aquatic.coffer.x,board.Aquatic.coffer.y);var delta=target-origin;
            Assert.That(delta.x==0||delta.y==0,Is.True);int length=Mathf.Abs(delta.x)+Mathf.Abs(delta.y);
            Assert.That(length,Is.InRange(2,3));var direction=new Vector2Int(Math.Sign(delta.x),Math.Sign(delta.y));
            for(int i=0;i<length;i++)
            {
                var from=origin+direction*(i+1);var to=origin+direction*i;
                Assert.That(board.GetGem(to.x,to.y),Is.SameAs(before[from]));
                Assert.That(board.GetGem(to.x,to.y).SpecialType,Is.EqualTo(GemSpecialType.ColorCrystal));
            }
            Assert.That(board.GetGem(target.x,target.y),Is.Null);
            Assert.That(grid.Cast<Gem>().Count(g=>g!=null),Is.EqualTo(countBefore-1),"only the chosen hit gem cleared; no mid-slide refill");
        };
        Assert.That(board.TryClearPlayerArea(victim,0,()=>true),Is.True);
        yield return Stable();Assert.That(slides,Is.EqualTo(1));Assert.That(held,Is.True);
        Assert.That(board.Aquatic.coffer,Is.Not.Null);var destination=new Vector2Int(coffer.x,coffer.y);
        var saved=board.CaptureContinuation(_=>-1).cells.Single(c=>c.x==destination.x&&c.y==destination.y);
        Assert.That(saved.durability,Is.EqualTo(1));
        yield return ResumeRoster();board=Run.Board;Assert.That(new Vector2Int(board.Aquatic.coffer.x,board.Aquatic.coffer.y),Is.EqualTo(destination));
        var art=board.GetComponentsInChildren<SpriteRenderer>().Where(r=>r.sprite==Run.Zone.Definition.theme.exposedPearl).ToArray();
        Assert.That(art,Is.Not.Empty,"Continue restores exposed royal art");
        board.Aquatic.air=0;var hit=new[]{Vector2Int.left,Vector2Int.right,Vector2Int.up,Vector2Int.down}
            .Select(d=>board.GetGem(destination.x+d.x,destination.y+d.y)).First(g=>g!=null);
        Call(board,"DamageBarricadesForClear",new HashSet<Gem>{hit},new HashSet<Gem>(),false);
        Assert.That(board.Aquatic.coffer,Is.Null);Assert.That(board.Aquatic.air,Is.EqualTo(5));
    }
    [UnityTest] public IEnumerator CourtRevisionRoyalSlideBlockedRoutesStayAndKeepEveryGem()
    {yield return CourtRoyalBlocked();}
    private IEnumerator CourtRoyalBlocked()
    {
        yield return RoyalCofferFixture();var board=Run.Board;var c=board.Aquatic.coffer;var origin=new Vector2Int(c.x,c.y);
        var holes=(Dictionary<Vector2Int,int>)Get(board,"minedCellOwners");
        var adjacent=new[]{Vector2Int.left,Vector2Int.right,Vector2Int.up,Vector2Int.down}
            .Select(d=>board.GetGem(c.x+d.x,c.y+d.y)).First(g=>g!=null);
        var ids=((Gem[,])Get(board,"gems")).Cast<Gem>().Where(g=>g!=null).Select(g=>g.BoardIdentity).OrderBy(n=>n).ToArray();
        // Probe existing structural restrictions without creating another mover.
        foreach(var d in new[]{Vector2Int.left,Vector2Int.right,Vector2Int.up,Vector2Int.down})holes[origin+d]=123;
        Call(board,"DamageBarricadesForClear",new HashSet<Gem>{adjacent},new HashSet<Gem>(),false);
        yield return (IEnumerator)Call(board,"ResolvePendingCofferSlide");holes.Clear();
        Assert.That(new Vector2Int(c.x,c.y),Is.EqualTo(origin));
        Assert.That(((Gem[,])Get(board,"gems")).Cast<Gem>().Where(g=>g!=null).Select(g=>g.BoardIdentity).OrderBy(n=>n),Is.EqualTo(ids));
        Assert.That(board.CaptureContinuation(_=>-1).cells.Single(v=>v.x==c.x&&v.y==c.y).durability,Is.EqualTo(1));
    }
    [UnityTest] public IEnumerator CourtRevisionThiefInstantExactTwoIndependentPayout()
    {yield return CourtInstantTheft("pearl_thief",2,1);}
    [UnityTest] public IEnumerator CourtRevisionWardenInstantThreeAndTwoDistinctHits()
    {yield return CourtInstantTheft("lantern_warden",3,2);}
    private IEnumerator CourtInstantTheft(string id,int charges,int durability)
    {
        yield return LaunchCourt(id,"shellback_porter");yield return Move();PrepareSafeMove();
        var board=Run.Board;var owner=Enemy(id);var kit=owner.GetComponent<AquaticEnemyAbility>();
        board.Aquatic.StartFlood(18,Run.MoveClock.Tick);
        board.Aquatic.bubbles=Enumerable.Range(0,charges).Select(x=>board.GetGem(x,0).BoardIdentity).ToList();
        var special=board.GetGem(5,0);special.SetSpecialType(GemSpecialType.ColumnBomb);
        var oldSpecial=special.BoardIdentity;
        float start=-1,contact=-1;bool everWarning=false;int announced=0;
        owner.SpecialMotionRequested+=_=>start=Time.time;owner.AbilityCastCommitted+=(_,__)=>announced++;
        board.AquaticChanged+=()=>{everWarning|=kit.IsPreparing;if(board.Aquatic.coffer!=null&&contact<0)contact=Time.time;};
        ReadyForest(owner);yield return Move(()=>special.SetSpecialType(GemSpecialType.ColumnBomb));
        Assert.That(board.Aquatic.coffer,Is.Not.Null);Assert.That(board.Aquatic.coffer.charges,Is.EqualTo(charges));
        Assert.That(contact-start,Is.InRange(.2f,2f));Assert.That(everWarning,Is.False);Assert.That(kit.IsPreparing,Is.False);
        Assert.That(owner.HasAnimationActionInProgress,Is.False);Assert.That(announced,Is.EqualTo(1));
        Assert.That(board.FindAquaticGem(oldSpecial)?.SpecialType,Is.EqualTo(GemSpecialType.ColumnBomb));
        Assert.That(board.Aquatic.bubbles,Is.Empty,"capture does not mint a replacement rescue");
        var coffer=board.Aquatic.coffer;var cell=new Vector2Int(coffer.x,coffer.y);
        Assert.That(board.GetGem(cell.x,cell.y),Is.Null);Assert.That(board.RemainingOxygenReserve,Is.EqualTo(charges));
        owner.ResolveDirectDamage(99999);yield return Stable();
        Assert.That(board.Aquatic.coffer,Is.Not.Null,"caster death preserves the board problem");
        yield return ResumeRoster();board=Run.Board;coffer=board.Aquatic.coffer;
        Assert.That(coffer.charges,Is.EqualTo(charges));board.Aquatic.air=0;
        int fractures=0;board.CofferShellBroken+=_=>fractures++;
        for(int hit=0;hit<durability;hit++)
        {
            var adjacent=new[]{Vector2Int.left,Vector2Int.right,Vector2Int.up,Vector2Int.down}
                .Select(d=>board.GetGem(cell.x+d.x,cell.y+d.y)).First(g=>g!=null);
            Call(board,"DamageBarricadesForClear",new HashSet<Gem>{adjacent},new HashSet<Gem>(),false);
            if(hit+1<durability)
            {
                Assert.That(board.Aquatic.coffer,Is.Not.Null);Assert.That(board.Aquatic.air,Is.Zero);
                var saved=board.CaptureContinuation(_=>-1).cells.Single(c=>c.x==cell.x&&c.y==cell.y);
                Assert.That(saved.durability,Is.EqualTo(1));Assert.That(fractures,Is.EqualTo(1));
            }
        }
        Assert.That(board.Aquatic.coffer,Is.Null);Assert.That(board.Aquatic.air,Is.EqualTo(durability==1?4:5));
        Call(board,"AquaticCofferBroken",cell);Assert.That(board.Aquatic.air,Is.EqualTo(durability==1?4:5),"no duplicate payout");
    }

    [UnityTest] public IEnumerator CourtRevisionTheftAndPressureDeferWithoutEnoughBubbles()
    {yield return CourtInsufficientBubbles();}
    private IEnumerator CourtInsufficientBubbles()
    {
        yield return LaunchCourt("lantern_warden","shellback_porter");yield return Move();PrepareSafeMove();
        var board=Run.Board;board.Aquatic.StartFlood(18,Run.MoveClock.Tick);
        board.Aquatic.bubbles.Add(board.GetGem(0,0).BoardIdentity);
        Assert.That(board.TryPlanAirTheft(2,false,out _,out _),Is.False);
        Assert.That(board.TryPlanAirTheft(3,false,out _,out _),Is.False);
        var actor=Enemy("lantern_warden");var kit=actor.GetComponent<AquaticEnemyAbility>();
        kit.RestoreContinuation(new EnemyCombatSnapshot{aquaticEnemy=new AquaticEnemySnapshot{version=2,cycle=1}},_=>null);
        ReadyForest(actor);yield return Move();
        Assert.That(kit.IsPreparing,Is.False);Assert.That(kit.ResponseCells,Is.Empty);
        Assert.That(board.Aquatic.coffer,Is.Null);Assert.That(actor.IsSpecialReady,Is.True);
    }

    [UnityTest] public IEnumerator CourtRevisionPressureEitherBubbleCancelsAndContinueKeepsPhysicalTargets()
    {yield return CourtPressure(true);}
    [UnityTest] public IEnumerator CourtRevisionPressureUnansweredDealsFullConfiguredHit()
    {yield return CourtPressure(false);}
    private IEnumerator CourtPressure(bool answer)
    {
        yield return LaunchCourt("lantern_warden","shellback_porter");yield return Move();PrepareSafeMove();
        var board=Run.Board;board.Aquatic.StartFlood(18,Run.MoveClock.Tick);
        board.Aquatic.bubbles=Enumerable.Range(0,3).Select(x=>board.GetGem(x,0).BoardIdentity).ToList();
        var actor=Enemy("lantern_warden");var kit=actor.GetComponent<AquaticEnemyAbility>();
        kit.RestoreContinuation(new EnemyCombatSnapshot{aquaticEnemy=new AquaticEnemySnapshot{version=2,cycle=1}},_=>null);
        ReadyForest(actor);yield return Move();
        Assert.That(kit.CastName,Is.EqualTo("PRESSURE"));Assert.That(kit.MarkedBubbles.Count,Is.EqualTo(2));
        Assert.That(kit.ResponseCells,Is.Empty);Assert.That(kit.CofferTarget,Is.Null);
        int[] targets=kit.MarkedBubbles.ToArray();int due=board.CompletedValidPlayerMoves+kit.ResponseMoves;
        yield return ResumeRoster();board=Run.Board;actor=Enemy("lantern_warden");kit=actor.GetComponent<AquaticEnemyAbility>();
        Assert.That(kit.MarkedBubbles,Is.EqualTo(targets));
        int damage=0;Run.Player.DamageTaken+=(_,amount)=>damage+=amount;
        if(answer)
        {
            // Use the same physical destruction receipt as specials/abilities.
            var gem=board.FindAquaticGem(targets[1]);var gems=new HashSet<Gem>{gem};
            Call(board,"RegisterAquaticClear",gems,false);Call(board,"ResolveAquaticDestruction",gems,new HashSet<Gem>());
            yield return Until(()=>!kit.IsPreparing,"one answer cancels every remaining mark");
            Assert.That(board.Aquatic.bubbles.Contains(targets[0]),Is.True);Assert.That(kit.MarkedBubbles,Is.Empty);
            Assert.That(actor.GetComponent<EnemyStagger>().IsStaggered,Is.False);
            Assert.That(actor.CurrentSpecialTurnCount,Is.Zero);
        }
        else {while(board.CompletedValidPlayerMoves<due)yield return Move();}
        Assert.That(damage,Is.EqualTo(answer?0:CombatAmounts.Round(actor.Definition.aquaticAbilityDamage*actor.RuntimeStats.DamageMultiplier)));
        Assert.That(kit.IsPreparing,Is.False);Assert.That(kit.BlocksBasic,Is.False);
    }

    [UnityTest] public IEnumerator CourtRevisionLegacyTheftWarningsRetireHarmlessly()
    {yield return CourtLegacyTheft();}
    private IEnumerator CourtLegacyTheft()
    {
        yield return LaunchCourt("pearl_thief","shellback_porter");yield return Move();
        var actor=Enemy("pearl_thief");var kit=actor.GetComponent<AquaticEnemyAbility>();
        kit.RestoreContinuation(new EnemyCombatSnapshot{aquaticEnemy=new AquaticEnemySnapshot{stage=1,action="THEFT",dueMove=99}},_=>null);
        Assert.That(kit.IsPreparing,Is.False);Assert.That(kit.MarkedBubbles,Is.Empty);Assert.That(actor.CurrentSpecialTurnCount,Is.Zero);
        Assert.That(actor.GetComponent<EnemyStagger>().IsStaggered,Is.False);Assert.That(Run.Board.Aquatic.coffer,Is.Null);
    }
    [UnityTest] public IEnumerator CourtRevisionFiniteReserveAndEmergencyCadence()
    {yield return FiniteCourtReserve();}
    private IEnumerator FiniteCourtReserve()
    {
        yield return LaunchCourt("shellback_porter");yield return Move();PrepareSafeMove();
        var board=Run.Board;var state=board.Aquatic;state.StartFlood(18,board.CompletedValidPlayerMoves);
        Call(board,"EnsureAquaticSupply",5);Assert.That(state.bubbles.Count,Is.EqualTo(5));
        int[] ids=state.bubbles.ToArray();state.air=1;
        Call(board,"AdvanceEmergencyAir",100);Assert.That(state.bubbles,Is.EqualTo(ids));
        Assert.That(state.reserveExhausted,Is.False);
        // Captured charges are still reserve oxygen, including when no bubble is free.
        state.bubbles.Clear();state.coffer=new AquaticCofferState{id=1,charges=5};
        Call(board,"AdvanceEmergencyAir",101);Call(board,"EnsureReachableAir");
        Assert.That(board.RemainingOxygenReserve,Is.EqualTo(5));Assert.That(state.bubbles,Is.Empty);
        Assert.That(state.reserveExhausted,Is.False);
        state.coffer=null;Call(board,"AdvanceEmergencyAir",102);
        Assert.That(state.reserveExhausted,Is.True);Assert.That(state.nextSupplyMove,Is.EqualTo(105));
        Call(board,"AdvanceEmergencyAir",104);Assert.That(state.bubbles,Is.Empty);
        state.air=3;Call(board,"AdvanceEmergencyAir",105);Assert.That(state.bubbles.Count,Is.EqualTo(1));
        state.air=1;Call(board,"AdvanceEmergencyAir",108);Assert.That(state.bubbles.Count,Is.EqualTo(2));
        Call(board,"AdvanceEmergencyAir",111);Assert.That(state.bubbles.Count,Is.EqualTo(2));
        var before=Run.Continuation.Capture();yield return ResumeRoster();
        Assert.That(Run.Board.Aquatic.reserveExhausted,Is.True);
        Assert.That(Run.Board.Aquatic.nextSupplyMove,Is.EqualTo(before.board.aquatic.nextSupplyMove));
        Assert.That(Run.Board.Aquatic.bubbles,Is.EqualTo(before.board.aquatic.bubbles));
    }

    [UnityTest] public IEnumerator CourtRevisionLowAirRelocatesWithoutMintingResources()
    {yield return CourtAirRelocation();}
    private IEnumerator CourtAirRelocation()
    {
        yield return LaunchCourt("shellback_porter");yield return Move();PrepareSafeMove();
        var board=Run.Board;var state=board.Aquatic;state.StartFlood(18,Run.MoveClock.Tick);state.air=0;
        Call(board,"EnsureReachableAir");Assert.That(state.bubbles,Is.Empty,"no free rescue during stolen reserve");
        var reachable=(HashSet<Gem>)Call(board,"ImmediatelyClearableOrdinaryGems");
        var blocked=Enumerable.Range(0,board.Width).SelectMany(x=>Enumerable.Range(0,board.Height).Select(y=>board.GetGem(x,y)))
            .First(g=>g!=null&&!reachable.Contains(g));
        state.bubbles.Add(blocked.BoardIdentity);Call(board,"EnsureReachableAir");
        Assert.That(state.bubbles.Count,Is.EqualTo(1));Assert.That(state.air,Is.Zero);
        Assert.That(reachable.Any(g=>state.bubbles.Contains(g.BoardIdentity)),Is.True);
    }

    [UnityTest] public IEnumerator CourtRevisionAirReceiptShowsSpendThenActualTwoBlockGain()
    {yield return CourtAirReceipt();}
    private IEnumerator CourtAirReceipt()
    {
        yield return LaunchCourt("shellback_porter");yield return Move();PrepareSafeMove();
        var board=Run.Board;var state=board.Aquatic;state.StartFlood(18,Run.MoveClock.Tick);state.air=3;
        var collected=board.GetGem(safeMoveFrom.x,safeMoveFrom.y);state.bubbles.Add(collected.BoardIdentity);
        int opening=-1,cost=0,receipts=0;int[] gains=null;
        board.AirReceipt+=(before,spend,adds)=>{opening=before;cost=spend;gains=adds;receipts++;};
        var view=board.GetComponent<AquaticEnvironmentView>();var beats=new List<string>();
        int tick=board.CompletedValidPlayerMoves;
        board.StartCoroutine((IEnumerator)Call(board,"TrySwap",collected,board.GetGem(safeMoveTo.x,safeMoveTo.y)));
        yield return Until(()=>
        {
            var label=Get(view,"airReturn") as TMPro.TMP_Text;
            if(label!=null && label.gameObject.activeInHierarchy && !beats.Contains(label.text))beats.Add(label.text);
            return board.CompletedValidPlayerMoves>tick && Run.Continuation.CanCapture && beats.Contains("+2");
        },"distinct AIR spend and gain beats");
        Assert.That(opening,Is.EqualTo(3));Assert.That(cost,Is.EqualTo(1));Assert.That(gains,Is.EqualTo(new[]{2}));
        Assert.That(state.air,Is.EqualTo(4),"bubble remains +2, move remains -1");
        Assert.That(beats.IndexOf("-1"),Is.GreaterThanOrEqualTo(0));
        Assert.That(beats.IndexOf("-1"),Is.LessThan(beats.IndexOf("+2")));
        yield return (IEnumerator)Call(board,"AdvanceAquaticEnvironment",board.CompletedValidPlayerMoves);
        Assert.That(receipts,Is.EqualTo(1),"settled accounting cannot replay its presentation");
        Assert.That(state.air,Is.EqualTo(4));
    }

    [UnityTest] public IEnumerator CourtRevisionLegacyFloodPreservesRemainingTimeAndReserve()
    {yield return CourtLegacyReserve();}
    private IEnumerator CourtLegacyReserve()
    {
        yield return LaunchCourt("shellback_porter");yield return Move();
        var board=Run.Board;var legacy=new AquaticEnvironmentState{version=1,phase=TidePhase.Flooded,air=2,wetMoves=7,
            bubbles=new List<int>{board.GetGem(0,0).BoardIdentity},nextSupplyMove=1};
        Call(board,"RestoreAquatic",legacy);
        Assert.That(board.Aquatic.version,Is.EqualTo(2));Assert.That(board.Aquatic.air,Is.EqualTo(2));
        Assert.That(board.Aquatic.wetMoves,Is.EqualTo(7));Assert.That(board.Aquatic.bubbles,Is.EqualTo(legacy.bubbles));
        Call(board,"AdvanceEmergencyAir",100);Assert.That(board.Aquatic.bubbles.Count,Is.EqualTo(1));
        Assert.That(board.Aquatic.reserveExhausted,Is.False);
    }
}
