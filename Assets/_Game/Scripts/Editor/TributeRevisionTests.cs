using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public sealed partial class ForestFoundationPlayTests
{
    private IEnumerator TributeFixture(params string[] roster)
    {
        yield return LaunchCourt(roster.Length>0?roster:new[]{"shellback_porter","queen_nacre","reef_spearman"});
        yield return Move();PrepareSafeMove();
        var board=Run.Board;board.Aquatic.StartFlood(18,Run.MoveClock.Tick);
        board.Aquatic.bubbles=Enumerable.Range(0,3).Select(x=>board.GetGem(x,0).BoardIdentity).ToList();
        var queen=Enemy("queen_nacre");var kit=queen.GetComponent<AquaticEnemyAbility>();
        kit.RestoreContinuation(new EnemyCombatSnapshot{aquaticEnemy=new AquaticEnemySnapshot{version=2,cycle=1}},_=>null);
        ReadyForest(queen);yield return Move();
        Assert.That(kit.CastName,Is.EqualTo("TRIBUTE"));Assert.That(kit.MarkedBubbles.Count,Is.EqualTo(3));
        Assert.That(kit.ResponseMoves,Is.EqualTo(3));Assert.That(kit.ResponseCells,Is.Empty);
        Assert.That(EnemyAbilityNames.All(queen.Definition.SpecialAbilityKind),Does.Contain("Nacre Tribute").And.Not.Contain("Crushing"));
    }
    [UnityTest] public IEnumerator CourtRevisionTributeZeroSurvivors(){yield return TributeSurvivors(0);}
    [UnityTest] public IEnumerator CourtRevisionTributeOneSurvivor(){yield return TributeSurvivors(1);}
    [UnityTest] public IEnumerator CourtRevisionTributeTwoSurvivors(){yield return TributeSurvivors(2);}
    [UnityTest] public IEnumerator CourtRevisionTributeThreeSurvivors(){yield return TributeSurvivors(3);}
    private IEnumerator TributeSurvivors(int surviving)
    {
        yield return TributeFixture();
        var board=Run.Board;var queen=Enemy("queen_nacre");var kit=queen.GetComponent<AquaticEnemyAbility>();
        var marked=kit.MarkedBubbles.ToArray();board.Aquatic.air=1;
        foreach(int id in marked.Take(3-surviving))
        {
            var set=new HashSet<Gem>{board.FindAquaticGem(id)};
            Call(board,"RegisterAquaticClear",set,false);Call(board,"ResolveAquaticDestruction",set,new HashSet<Gem>());
        }
        Assert.That(board.Aquatic.air,Is.EqualTo(Math.Min(5,1+2*(3-surviving))),"each collected bubble is +2");
        Assert.That(queen.GetComponent<EnemyStagger>().IsStaggered,Is.False);
        if(surviving==0)
        {
            yield return Until(()=>!kit.IsPreparing,"all answers fizzle without damage or stagger");
            Assert.That(Run.Waves.ActiveEnemies.Sum(e=>e.FortifiedStacks),Is.Zero);yield break;
        }
        yield return ResumeRoster();board=Run.Board;queen=Enemy("queen_nacre");kit=queen.GetComponent<AquaticEnemyAbility>();
        Assert.That(kit.MarkedBubbles,Is.EqualTo(marked.Skip(3-surviving)));
        int deadline=board.CompletedValidPlayerMoves+kit.ResponseMoves;var gains=new List<int>();
        board.AirReceipt+=(_,__,adds)=>gains.AddRange(adds);
        while(board.CompletedValidPlayerMoves<deadline)
        {
            board.Aquatic.air=5;
            yield return Move(()=>{foreach(int id in kit.MarkedBubbles)board.FindAquaticGem(id)?.SetSpecialType(GemSpecialType.ColorCrystal);});
        }
        Assert.That(kit.IsPreparing,Is.False);Assert.That(kit.BlocksBasic,Is.False);
        Assert.That(queen.CurrentSpecialTurnCount,Is.Zero);
        Assert.That(board.Aquatic.bubbles,Is.Empty);Assert.That(gains,Is.Empty,"Tribute never grants AIR");
        Assert.That(marked.Skip(3-surviving).All(id=>board.FindAquaticGem(id)!=null),Is.True,"conversion consumes overlays, not gems");
        Assert.That(queen.FortifiedStacks,Is.EqualTo(1));
        Assert.That(Enemy("shellback_porter").FortifiedStacks,Is.EqualTo(surviving>=2?1:0),"Queen first, then actual left slot");
        Assert.That(Enemy("reef_spearman").FortifiedStacks,Is.EqualTo(surviving>=3?1:0));
        yield return ResumeRoster();Assert.That(Run.Waves.ActiveEnemies.Sum(e=>e.FortifiedStacks),Is.EqualTo(surviving));
        Assert.That(CombatGuide.Enemy(Enemy("queen_nacre")),Does.Contain("FORTIFIED"));
    }
    [UnityTest] public IEnumerator CourtRevisionTributeRealClearAndStaggerLeaveRemainingOxygen()
    {yield return TributeInterrupted();}
    private IEnumerator TributeInterrupted()
    {
        yield return TributeFixture();var board=Run.Board;var queen=Enemy("queen_nacre");var kit=queen.GetComponent<AquaticEnemyAbility>();
        int[] marks=kit.MarkedBubbles.ToArray();board.Aquatic.air=1;
        Assert.That(board.TryClearPlayerArea(board.FindAquaticGem(marks[0]),0,()=>true),Is.True);yield return Stable();
        Assert.That(board.Aquatic.air,Is.EqualTo(3));Assert.That(kit.MarkedBubbles,Is.EqualTo(marks.Skip(1)));
        queen.GetComponent<EnemyStagger>().ApplyStagger(1,1);
        Assert.That(kit.IsPreparing,Is.False);Assert.That(kit.MarkedBubbles,Is.Empty);
        Assert.That(board.Aquatic.bubbles.Contains(marks[1])&&board.Aquatic.bubbles.Contains(marks[2]),Is.True);
        Assert.That(Run.Waves.ActiveEnemies.Sum(e=>e.FortifiedStacks),Is.Zero);
        float end=Time.time+1.2f;yield return Until(()=>Time.time>=end,"ordinary stagger expires");
        Assert.That(queen.GetComponent<EnemyStagger>().IsStaggered,Is.False);Assert.That(kit.BlocksBasic,Is.False);
        yield return Move();Assert.That(queen.CurrentSpecialTurnCount,Is.EqualTo(1));
    }
    [UnityTest] public IEnumerator CourtRevisionTributeRoundRobinCapsQueenAndPreservesMuster()
    {yield return TributeRoundRobin();}
    private IEnumerator TributeRoundRobin()
    {
        yield return TributeFixture("queen_nacre","shellback_porter");var queen=Enemy("queen_nacre");var kit=queen.GetComponent<AquaticEnemyAbility>();
        int due=Run.MoveClock.Tick+kit.ResponseMoves;
        while(Run.MoveClock.Tick<due)
        {
            Run.Board.Aquatic.air=5;
            // Keep all three physical bubble targets out of incidental refill
            // matches so this case isolates round-robin distribution and caps.
            yield return Move(()=>{foreach(int id in kit.MarkedBubbles)Run.Board.FindAquaticGem(id)?.SetSpecialType(GemSpecialType.ColorCrystal);});
        }
        Assert.That(queen.FortifiedStacks,Is.EqualTo(2));Assert.That(Enemy("shellback_porter").FortifiedStacks,Is.EqualTo(1));
        Call(kit,"DistributeTribute",5);Assert.That(queen.FortifiedStacks,Is.EqualTo(2));Assert.That(Enemy("shellback_porter").FortifiedStacks,Is.EqualTo(2));
        for(int i=0;i<3;i++){Run.Board.Aquatic.air=5;yield return Move();}
        Assert.That(kit.CastName,Is.EqualTo("MUSTER"));Assert.That(kit.IsPreparing,Is.True);
        int slot=((AquaticEnemySnapshot)Get(kit,"state")).summonSlot;
        for(int i=0;i<2;i++){Run.Board.Aquatic.air=5;yield return Move();}
        var crab=Enemy("skittercrab");Assert.That(Run.Waves.ContinuationSlot(crab),Is.EqualTo(slot));
        queen.ResolveDamageWithoutFeedback(99999);queen.ResolveDamageWithoutFeedback(99999);
        Assert.That(crab.IsDefeated,Is.False);
    }
    [UnityTest] public IEnumerator CourtRevisionLegacyDepthsWarningRetiresWithoutDamage()
    {yield return RetireDepths();}
    private IEnumerator RetireDepths()
    {
        yield return LaunchCourt("queen_nacre","shellback_porter");var queen=Enemy("queen_nacre");var kit=queen.GetComponent<AquaticEnemyAbility>();
        int hp=Run.Player.CurrentHealth;
        kit.RestoreContinuation(new EnemyCombatSnapshot{aquaticEnemy=new AquaticEnemySnapshot{stage=1,cycle=1,action="DEPTHS",
            dueMove=0,marks=new List<Vector2Int>{new Vector2Int(1,1),new Vector2Int(2,2)}}},_=>null);
        Assert.That(kit.IsPreparing,Is.False);Assert.That(kit.ResponseCells,Is.Empty);Assert.That(kit.BlocksBasic,Is.False);
        Assert.That(Run.Player.CurrentHealth,Is.EqualTo(hp));Assert.That(queen.GetComponent<EnemyStagger>().IsStaggered,Is.False);
    }
    [UnityTest] public IEnumerator CourtRevisionTributeUsesActualSlotsAfterLeftReplacement()
    {yield return TributeReplacementSlot();}
    private IEnumerator TributeReplacementSlot()
    {
        yield return LaunchCourt("shellback_porter","queen_nacre","reef_spearman");
        var queen=Enemy("queen_nacre");var kit=queen.GetComponent<AquaticEnemyAbility>();
        var definition=Enemy("shellback_porter").Definition;Enemy("shellback_porter").ResolveDamageWithoutFeedback(99999);
        yield return Stable();Assert.That(Run.Waves.TrySummonEnemy(definition,out var replacement),Is.True);yield return Stable();
        Assert.That(Run.Waves.ContinuationSlot(replacement),Is.Zero);
        Call(kit,"DistributeTribute",2);
        Assert.That(queen.FortifiedStacks,Is.EqualTo(1));Assert.That(replacement.FortifiedStacks,Is.EqualTo(1));
        Assert.That(Enemy("reef_spearman").FortifiedStacks,Is.Zero,"replacement slot order wins over spawn age");
    }
    [UnityTest] public IEnumerator CourtRevisionFortifiedOrbitFrontBackPauseAndDeathCleanup()
    {yield return FortifiedOrbit();}
    private IEnumerator FortifiedOrbit()
    {
        yield return LaunchCourt("queen_nacre","shellback_porter");var queen=Enemy("queen_nacre");queen.GrantFortified(2);
        yield return null;yield return null;
        var view=queen.GetComponent<EnemyFortifiedView>();var pearls=(Image[])Get(view,"pearls");var visual=(RectTransform)Get(view,"visual");
        Assert.That(pearls.All(p=>p!=null&&p.gameObject.activeInHierarchy),Is.True);
        Assert.That(pearls.Any(p=>p.transform.GetSiblingIndex()<visual.GetSiblingIndex()),Is.True);
        Assert.That(pearls.Any(p=>p.transform.GetSiblingIndex()>visual.GetSiblingIndex()),Is.True);
        bool reduced=PresentationPreferences.ReducedMotion;
        try
        {
            PresentationPreferences.SetReducedMotion(true);yield return null;yield return null;
            var still=pearls.Select(p=>p.transform.position).ToArray();float until=Time.time+.15f;
            yield return Until(()=>Time.time>=until,"reduced-motion pearls remain separated and still");
            Assert.That(pearls.Select(p=>p.transform.position),Is.EqualTo(still));
            Assert.That(Vector3.Distance(still[0],still[1]),Is.GreaterThan(1));
        }
        finally{PresentationPreferences.SetReducedMotion(reduced);}
        yield return null;yield return null;
        var before=pearls.Select(p=>p.transform.position).ToArray();Time.timeScale=0;yield return null;yield return null;
        Assert.That(pearls.Select(p=>p.transform.position),Is.EqualTo(before));Time.timeScale=1;
        queen.ResolveDirectDamage(20);yield return null;yield return null;
        Assert.That(pearls.Count(p=>p.gameObject.activeSelf),Is.EqualTo(1));Assert.That(((Image)Get(view,"pop")).gameObject.activeSelf,Is.True);
        queen.ResolveDamageWithoutFeedback(99999);yield return null;yield return null;
        Assert.That(pearls.All(p=>p==null||!p.gameObject.activeInHierarchy),Is.True);
    }
}
