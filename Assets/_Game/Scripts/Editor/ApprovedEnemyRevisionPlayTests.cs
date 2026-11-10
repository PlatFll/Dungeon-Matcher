using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public sealed partial class ForestFoundationPlayTests
{
    private IEnumerator RevisionDungeon(params EnemySpecialAbilityKind[] kinds)
    {
        var zone=Resources.Load<ZoneDefinition>("Zones/dungeon");
        var recipes=zone.liveEncounters;var development=zone.developmentEncounters;var budget=zone.encounterBudget;
        var definitions=UnityEditor.AssetDatabase.FindAssets("t:EnemyDefinition",new[]{"Assets/_Game/Data/Enemies"})
            .Select(g=>UnityEditor.AssetDatabase.LoadAssetAtPath<EnemyDefinition>(UnityEditor.AssetDatabase.GUIDToAssetPath(g))).ToArray();
        zone.encounterBudget=null;zone.liveEncounters=new[]{new ZoneTestEncounter{label="Approved revision fixture",
            members=kinds.Select(k=>definitions.First(d=>d.SpecialAbilityKind==k)).ToArray()}};
        zone.developmentEncounters=zone.liveEncounters;
        RunLaunchOptions.ForestPrototype=false;
        try
        {
            yield return LaunchUnified();
            Run.Travel.State.visit=1;Run.Waves.SpawnCurrentWave();yield return Stable();
        }
        finally{zone.liveEncounters=recipes;zone.developmentEncounters=development;zone.encounterBudget=budget;}
        Set(Run.Waves,"advanceWavesAutomatically",false);PreserveRoster();
        yield return Until(()=>Run.Waves.ActiveEnemies.All(e=>e.GetComponent<EnemyLifecycleVFX>()?.IsSpawning!=true),"revision spawn");
    }
    private IEnumerator RevisionImpact(Vector2Int cell,Action impact=null)
    {
        Assert.That(Run.Board.TryPlayerImpactSequence(new PlayerBoardImpactSequence{Count=1,ImmediateImpact=true,
            Select=(_,targets)=>cell,Impacted=(_,__,___)=>impact?.Invoke()}),Is.True);
        yield return Stable();
    }
    private void StopAmbientFrost()=>Set(Run.Board,"nextFrostSeedMove",int.MaxValue);
    private IEnumerator ReadyRevisionHearth(EnemyActor owner,EnemyActor recipient)
    {
        // Install a reachable ordinary response after the activation move's
        // random refill, at the real actor opportunity; placement still uses
        // the production queue and every safety check.
        var actors=Run.Waves.ActiveEnemies.ToArray();var protection=new object();
        foreach(var enemy in actors){Set(enemy,"assignedGemType",GemType.Ruby);enemy.SetSharedDamageReduction(protection,()=>0f);}
        Action<EnemyActor> prepare=e=>{if(e==owner){PrepareSafeMove();typeof(RunContinuation).GetProperty("Random").SetValue(Run.Continuation,new SavedRandom(1));Set(recipient,"currentHealth",recipient.MaxHealth-25);}};
        Run.MoveClock.Opportunity+=prepare;
        try{yield return ReadyFrostwake(owner);}finally{Run.MoveClock.Opportunity-=prepare;foreach(var enemy in actors)if(enemy!=null)enemy.RemoveSharedDamageReduction(protection);}
    }
    private Dictionary<int,FrostGemSnapshot> Ice=>(Dictionary<int,FrostGemSnapshot>)Get(Run.Board,"frostGems");
    private void FixtureIce(Gem gem,FrostStage stage,long owner=0)
    {
        Ice[gem.BoardIdentity]=new FrostGemSnapshot{identity=gem.BoardIdentity,ownerId=owner,stage=stage,
            lastAdvancedMove=Run.Board.CompletedValidPlayerMoves};Call(Run.Board,"PublishFrost");
    }
    [UnityTest] public IEnumerator RevisionCaptainAdvancesOthersWithoutImmediateActionsOrSelfAdvance()
    {
        yield return RevisionDungeon(EnemySpecialAbilityKind.KnightCaptain,EnemySpecialAbilityKind.CourtMage,EnemySpecialAbilityKind.King);
        var captain=Run.Waves.ActiveEnemies[0];var mage=Run.Waves.ActiveEnemies[1];var king=Run.Waves.ActiveEnemies[2];
        var kit=captain.GetComponent<KnightCaptainEnemyAbility>();Set(kit,"preferChains",false);
        int hits=0;foreach(var e in Run.Waves.ActiveEnemies)e.GetComponent<EnemyAutoAttack>().AttackStarted+=_=>hits++;
        Set(mage.GetComponent<EnemyAutoAttack>(),"unifiedRemaining",4);
        Set(king.GetComponent<EnemyAutoAttack>(),"unifiedRemaining",2);
        Set(mage,"currentSpecialTurnCount",mage.SpecialTurnRequirement-4);
        Set(king,"currentSpecialTurnCount",king.SpecialTurnRequirement-2);
        yield return ReadyFrostwake(captain);
        Assert.That(hits,Is.Zero);Assert.That(mage.GetComponent<EnemyAutoAttack>().RemainingAttackTime,Is.EqualTo(2));
        Assert.That(king.GetComponent<EnemyAutoAttack>().RemainingAttackTime,Is.EqualTo(1));
        Assert.That(mage.SpecialTurnRequirement-mage.CurrentSpecialTurnCount,Is.EqualTo(2));
        Assert.That(king.SpecialTurnRequirement-king.CurrentSpecialTurnCount,Is.EqualTo(1));
        Assert.That(mage.IsSpecialReady || king.IsSpecialReady,Is.False);
        Assert.That(captain.CurrentSpecialTurnCount,Is.Zero);Assert.That((bool)Get(kit,"preferChains"),Is.True);
        yield return CaptureEnvironment("revision-advance-the-ranks");
        QuietKitFixture();yield return ReadyFrostwake(captain);
        Assert.That((bool)Get(kit,"preferChains"),Is.False);Assert.That(Run.Board.RestrictionCount,Is.GreaterThan(0));
    }
    [UnityTest] public IEnumerator RevisionIceClearRulesAndZoneCleanupUseSharedOwners()
    {
        yield return LaunchFrostwakeKit("iceborn_raider");StopAmbientFrost();var board=Run.Board;
        var gem=board.GetGem(2,2);FixtureIce(gem,FrostStage.Bifrost);
        int hp=Run.Player.CurrentHealth;
        Call(board,"DamageFrostFromClears",new HashSet<Gem>{board.GetGem(2,3)},null,true);
        Assert.That(board.IceStage(gem),Is.EqualTo(FrostStage.HeavyIce));Assert.That(Run.Player.CurrentHealth,Is.EqualTo(hp));
        FixtureIce(gem,FrostStage.Bifrost);Call(board,"DamageFrostFromClears",new HashSet<Gem>{gem},null,false);
        Assert.That(board.IceStage(gem),Is.EqualTo(FrostStage.None));Assert.That(Run.Player.CurrentHealth,Is.EqualTo(hp));
        FixtureIce(gem,FrostStage.Bifrost);Call(board,"DamageFrostFromClears",new HashSet<Gem>{gem},null,true);
        Assert.That(Run.Player.CurrentHealth,Is.EqualTo(hp-10));Assert.That(board.IceStage(gem),Is.EqualTo(FrostStage.None));
        FixtureIce(board.GetGem(0,0),FrostStage.Frost);
        var saved=Run.Continuation.Capture().board;
        board.PrepareZoneArrival(saved,Resources.Load<ZoneDefinition>("Zones/dungeon"));
        Assert.That(saved.frost,Is.Empty);Assert.That(saved.aquatic,Is.Null);
    }
    [UnityTest] public IEnumerator RevisionMageSharesIceOwnsExactlyThreeAndReleasesWithoutPenalty()
    {
        yield return RevisionDungeon(EnemySpecialAbilityKind.CourtMage,EnemySpecialAbilityKind.CourtMage);
        var mage=Run.Waves.ActiveEnemies[0];var other=Run.Waves.ActiveEnemies[1];var board=Run.Board;
        yield return ReadyFrostwake(mage);
        Assert.That(board.CountOwnedIce(mage.PersistentId),Is.EqualTo(1),"the real Mage cast uses shared ice");
        for(int i=0;i<4;i++)
        {
            bool done=false;Assert.That(board.TryQueueEncasedGem(mage,FrostStage.Frost,_=>done=true,ownedLimit:3),Is.True);
            yield return Until(()=>done && Run.Continuation.CanCapture,"mage placement");
        }
        Assert.That(board.CountOwnedIce(mage.PersistentId),Is.EqualTo(3));
        Assert.That(board.RestrictionCount,Is.EqualTo(3),"shared ice counts once");
        bool placed=false;board.TryQueueEncasedGem(other,FrostStage.Frost,_=>placed=true,ownedLimit:3);
        yield return Until(()=>placed && Run.Continuation.CanCapture,"second mage");
        foreach(var ice in Ice.Values)ice.lastAdvancedMove=0;
        for(int i=1;i<=6;i++)Call(board,"ExecuteFrostEnvironment",i);
        Assert.That(Ice.Count,Is.EqualTo(4),"Dungeon never seeds ambient ice");
        Assert.That(Ice.Values.All(i=>i.stage==FrostStage.Bifrost),Is.True);
        var owned=Ice.Values.Where(i=>i.ownerId==mage.PersistentId).Select(i=>i.identity).ToArray();
        yield return CaptureEnvironment("revision-mage-true-ice");
        yield return ResumeRoster();board=Run.Board;mage=Run.Waves.ActiveEnemies[0];other=Run.Waves.ActiveEnemies[1];
        Assert.That(board.CountOwnedIce(mage.PersistentId),Is.EqualTo(3));
        int hp=Run.Player.CurrentHealth;mage.TryTakeDamage(50000);yield return Stable();
        Assert.That(board.CountOwnedIce(mage.PersistentId),Is.Zero);
        Assert.That(board.CountOwnedIce(other.PersistentId),Is.EqualTo(1));
        Assert.That(Run.Player.CurrentHealth,Is.EqualTo(hp));
        Assert.That(owned.All(id=>board.IceStage(board.FindGemIdentity(id))==FrostStage.None),Is.True);
    }
    [UnityTest] public IEnumerator RevisionChampionTwoPacketsRecoveryContinueAndDeath()
    {
        yield return LaunchFrostwakeKit("iceborn_champion");StopAmbientFrost();
        var actor=Enemy("iceborn_champion");yield return ReadyFrostwake(actor);
        var kit=actor.GetComponent<ShieldbreakerChargeRuntime>();Assert.That(kit.ResponseMoves,Is.EqualTo(2));
        int readiness=actor.CurrentSpecialTurnCount;float basic=actor.GetComponent<EnemyAutoAttack>().RemainingAttackTime;
        actor.AdvanceSpecialReadiness();actor.GetComponent<EnemyAutoAttack>().AdvanceReadinessWithoutAttack();
        Assert.That(kit.ResponseMoves,Is.EqualTo(2));Assert.That(actor.CurrentSpecialTurnCount,Is.EqualTo(readiness));
        Assert.That(actor.GetComponent<EnemyAutoAttack>().RemainingAttackTime,Is.EqualTo(basic),"Captain helper cannot accelerate an active channel");
        yield return CaptureEnvironment("revision-shieldbreaker-warning");
        yield return ResumeRoster();StopAmbientFrost();actor=Enemy("iceborn_champion");kit=actor.GetComponent<ShieldbreakerChargeRuntime>();
        Assert.That(kit.ResponseMoves,Is.EqualTo(2));
        Set(Run.Player,"currentShield",5);int hp=Run.Player.CurrentHealth;
        yield return EnvironmentMove();Assert.That(Run.Player.CurrentHealth,Is.EqualTo(hp));
        yield return EnvironmentMove();
        Assert.That(Run.Player.CurrentShield,Is.Zero);
        Assert.That(Run.Player.CurrentHealth,Is.EqualTo(hp-CombatAmounts.Round(25*actor.RuntimeStats.DamageMultiplier)),"first packet shield-gated, second reaches HP");
        Assert.That(kit.RecoveryMoves,Is.EqualTo(3));
        yield return CaptureEnvironment("revision-shieldbreaker-recovery");
        yield return ResumeRoster();StopAmbientFrost();actor=Enemy("iceborn_champion");kit=actor.GetComponent<ShieldbreakerChargeRuntime>();
        for(int i=2;i>=0;i--){yield return EnvironmentMove();Assert.That(kit.RecoveryMoves,Is.EqualTo(i));}
        Assert.That(kit.BlocksBasic,Is.False);
        yield return ReadyFrostwake(actor);Assert.That(kit.IsChanneling,Is.True);
        hp=Run.Player.CurrentHealth;actor.TryTakeDamage(50000);yield return null;
        Assert.That(kit.IsChanneling,Is.False);Assert.That(Run.Player.CurrentHealth,Is.EqualTo(hp));
    }
    [UnityTest] public IEnumerator RevisionTrollColumnAnswersAbilityAndEnvironmentalClearDoesNot()
    {
        yield return LaunchFrostwakeKit("glacier_troll","iceborn_raider");StopAmbientFrost();
        var actor=Enemy("glacier_troll");var kit=actor.GetComponent<FrostwakeEnemyAbility>();
        ((FrostwakeEnemySnapshot)Get(kit,"state")).armorNext=false;
        yield return ReadyFrostwake(actor);
        var column=(BoardController.CellResponseThreat)Get(kit,"column");
        Assert.That(column.Cells.Count,Is.EqualTo(8));Assert.That(kit.ResponseMoves,Is.EqualTo(2));
        yield return CaptureEnvironment("revision-avalanche-column");
        yield return ResumeRoster();StopAmbientFrost();actor=Enemy("glacier_troll");kit=actor.GetComponent<FrostwakeEnemyAbility>();
        column=(BoardController.CellResponseThreat)Get(kit,"column");Assert.That(column.Cells.Count,Is.EqualTo(8));
        var gem=Run.Board.GetGem(column.FrostColumn,0);
        // Environmental physical removal must not supply the player receipt.
        Call(Run.Board,"ClearVinesForDestruction",new HashSet<Gem>{gem},new HashSet<Gem>());
        Assert.That(column.Answered,Is.False);
        yield return RevisionImpact(new Vector2Int(column.FrostColumn,0));yield return null;
        Assert.That(kit.IsPreparing,Is.False);Assert.That(column.Answered,Is.True);
        Assert.That(Ice.Count,Is.Zero,"countered avalanche adds no ice");
        actor.GetComponent<EnemyStagger>().ClearStagger();QuietKitFixture();
        ((FrostwakeEnemySnapshot)Get(kit,"state")).armorNext=false;
        yield return ReadyFrostwake(actor);column=(BoardController.CellResponseThreat)Get(kit,"column");
        // This branch tests a successful unanswered placement, not a legal
        // safety fizzle after a random refill. Keep a useful special response
        // outside the threatened column so either new pin retains counterplay.
        PrepareSafeMove();
        Run.Board.GetGem(column.FrostColumn==7?0:7,0).SetSpecialType(GemSpecialType.ColorCrystal);
        var safePins=(List<Gem>)Call(Run.Board,"BuildSafePinnableGemList");
        Assert.That(safePins.Any(g=>g.Column==column.FrostColumn && g.SpecialType==GemSpecialType.None),Is.True);
        Assert.That(column.Answered,Is.False);
        int hp=Run.Player.CurrentHealth;
        typeof(BoardController.CellResponseThreat).GetProperty("DueMove").SetValue(column,Run.Board.CompletedValidPlayerMoves);
        bool done=false;Assert.That(Run.Board.TryQueueResolveCellResponse(column,(_,__)=>{},_=>done=true),Is.True);
        yield return Until(()=>done && Run.Continuation.CanCapture,"unanswered frost resolves");
        Assert.That(Ice.Count,Is.InRange(1,2));Assert.That(Ice.Values.All(i=>i.stage==FrostStage.Frost && Run.Board.FindGemIdentity(i.identity).Column==column.FrostColumn),Is.True);
        Assert.That(Run.Player.CurrentHealth,Is.EqualTo(hp),"no retired Avalanche Blow damage");
    }
    [UnityTest] public IEnumerator RevisionHearthThreeFutureMovesContinueAndOwnerDeath()
    {
        yield return LaunchFrostwakeKit("hearthkeeper","iceborn_raider");StopAmbientFrost();
        var owner=Enemy("hearthkeeper");var target=Enemy("iceborn_raider");Set(target,"currentHealth",target.MaxHealth-25);
        int hp=target.CurrentHealth;yield return ReadyRevisionHearth(owner,target);
        target.SetSharedDamageReduction(this,()=>0f); // Isolate healing from incidental refill damage.
        var kit=owner.GetComponent<HearthEmberRuntime>();Assert.That(kit.HasEmber,Is.True);
        Assert.That(target.CurrentHealth,Is.EqualTo(hp));Assert.That(kit.RemainingMoves,Is.EqualTo(3));
        yield return CaptureEnvironment("revision-hearth-ember");
        yield return EnvironmentMove();Assert.That(target.CurrentHealth,Is.EqualTo(hp+5));
        Call(kit,"Advance",Run.Board.CompletedValidPlayerMoves);Assert.That(target.CurrentHealth,Is.EqualTo(hp+5));
        yield return ResumeRoster();StopAmbientFrost();owner=Enemy("hearthkeeper");target=Enemy("iceborn_raider");kit=owner.GetComponent<HearthEmberRuntime>();
        target.SetSharedDamageReduction(this,()=>0f);
        Assert.That(kit.RemainingMoves,Is.EqualTo(2));Assert.That(kit.Target,Is.SameAs(target));
        yield return EnvironmentMove();yield return EnvironmentMove();
        Assert.That(target.CurrentHealth,Is.EqualTo(hp+15));Assert.That(kit.HasEmber,Is.False);Assert.That(Run.Board.HasHearthEmber(),Is.False);
        Set(target,"currentHealth",target.MaxHealth-25);yield return ReadyRevisionHearth(owner,target);Assert.That(kit.HasEmber,Is.True);
        owner.TryTakeDamage(50000);yield return Stable();Assert.That(Run.Board.HasHearthEmber(),Is.False);
    }
    [UnityTest] public IEnumerator RevisionHearthPlayerBreakThawsAtMostTwoAndDoesNotRetarget()
    {
        yield return LaunchFrostwakeKit("hearthkeeper","iceborn_raider","harpoon_hunter");StopAmbientFrost();
        var owner=Enemy("hearthkeeper");var target=Enemy("iceborn_raider");Set(target,"currentHealth",target.MaxHealth-25);
        yield return ReadyRevisionHearth(owner,target);var kit=owner.GetComponent<HearthEmberRuntime>();Assert.That(kit.HasEmber,Is.True);
        var cell=Run.Board.CollectPlayerImpactTargets().Single(t=>t.Barrier).Cell;
        var adjacent=new[]{Vector2Int.left,Vector2Int.right,Vector2Int.up,Vector2Int.down}
            .Select(d=>Run.Board.GetGem(cell.x+d.x,cell.y+d.y)).Where(g=>g!=null).ToArray();
        foreach(var g in adjacent)FixtureIce(g,FrostStage.Bifrost);
        int layersAtImpact=-1;
        yield return RevisionImpact(cell,()=>layersAtImpact=adjacent.Count(g=>Run.Board.IceStage(g)==FrostStage.HeavyIce));
        Assert.That(kit.HasEmber,Is.False);Assert.That(Run.Board.HasHearthEmber(),Is.False);
        Assert.That(layersAtImpact,Is.EqualTo(Mathf.Min(2,adjacent.Length)),"measure the bonus before subsequent refill cascades thaw more ice");
        Ice.Clear();Call(Run.Board,"PublishFrost");yield return ReadyRevisionHearth(owner,target);Assert.That(kit.HasEmber,Is.True);
        target.TryTakeDamage(50000);yield return Stable();Assert.That(kit.HasEmber,Is.False);Assert.That(Run.Board.HasHearthEmber(),Is.False);
    }
    [UnityTest] public IEnumerator RevisionCantorOnlyCollectedChannelBubblesReduceHealAndResume()
    {
        yield return LaunchCourt("pearl_cantor","shellback_porter","reef_netweaver");yield return Move();QuietKitFixture();
        var owner=Enemy("pearl_cantor");var target=Enemy("shellback_porter");Set(target,"currentHealth",target.MaxHealth/2);
        var board=Run.Board;board.Aquatic.StartFlood(30,Run.MoveClock.Tick);
        ReadyForest(owner);yield return Move();var kit=owner.GetComponent<EnemyChannelRuntime>();
        Assert.That(kit.IsChanneling,Is.True);Assert.That(kit.PendingHeal,Is.EqualTo(20));
        PrepareSafeMove();var gem=board.GetGem(0,0);board.Aquatic.bubbles.Add(gem.BoardIdentity);
        Call(board,"ResolveAquaticDestruction",new HashSet<Gem>{gem},new HashSet<Gem>());
        Assert.That(kit.PendingHeal,Is.EqualTo(20),"uncollected environmental removal is not a counter");
        board.Aquatic.bubbles.Clear();board.Aquatic.bubbles.Add(gem.BoardIdentity);yield return RevisionImpact(new Vector2Int(0,0));
        Assert.That(target.IsDefeated,Is.False);Assert.That(kit.IsChanneling,Is.True);
        Assert.That(kit.PendingHeal,Is.EqualTo(10));
        yield return CaptureEnvironment("revision-cantor-heal-ten");
        yield return ResumeRoster();owner=Enemy("pearl_cantor");target=Enemy("shellback_porter");board=Run.Board;kit=owner.GetComponent<EnemyChannelRuntime>();
        Assert.That(kit.PendingHeal,Is.EqualTo(10));
        gem=board.GetGem(0,0);board.Aquatic.bubbles.Clear();board.Aquatic.bubbles.Add(gem.BoardIdentity);yield return RevisionImpact(new Vector2Int(0,0));
        Assert.That(kit.PendingHeal,Is.Zero);int hp=target.CurrentHealth;
        yield return EnvironmentMove();yield return EnvironmentMove();Assert.That(target.CurrentHealth,Is.EqualTo(hp));
        Assert.That(kit.IsChanneling,Is.False);
        PrepareSafeMove();
        var types=(Sprite[])Get(board,"gemSprites");var useful=target.AssignedGemType;
        foreach(var point in new[]{new Vector2Int(safeMoveTo.x-2,safeMoveTo.y),new Vector2Int(safeMoveTo.x-1,safeMoveTo.y),safeMoveFrom})
            board.GetGem(point.x,point.y).SetType(useful,types[(int)useful]);
        board.Aquatic.bubbles.Clear();board.Aquatic.bubbles.Add(board.GetGem(safeMoveFrom.x,safeMoveFrom.y).BoardIdentity);
        if(board.Aquatic.snares.Count==0)Assert.That(board.TryApplyAquaticSnares(Enemy("reef_netweaver")),Is.True);
        Assert.That(board.Aquatic.snares.Count,Is.GreaterThan(0));
        yield return null;yield return null;
        var theme=Run.Zone.Definition.theme;
        Assert.That(UnityEditor.AssetDatabase.GetAssetPath(theme.thornySnare),Is.EqualTo("Assets/_Game/Resources/Combat/ApprovedRevisions/MarineSnare.png"));
        var views=board.GetComponentsInChildren<SpriteRenderer>().Where(r=>r.name.StartsWith("snare")).ToArray();
        Assert.That(views.Length,Is.EqualTo(board.Aquatic.snares.Count));
        Assert.That(views.All(r=>r.sprite==theme.thornySnare && r.enabled && r.isVisible),Is.True);
        foreach(var view in views)
            Debug.Log($"Marine net {view.name}: screen={Camera.main.WorldToScreenPoint(view.bounds.center)} bounds={view.bounds.size} sprite={view.sprite.rect} color={view.color} texture={UnityEditor.AssetDatabase.GetAssetPath(view.sprite.texture)} material={view.sharedMaterial.name}");
        yield return CaptureEnvironment("revision-marine-snare");
    }
    [UnityTest] public IEnumerator RevisionCantorDryHealStaggerAndRecipientDeathRetainContract()
    {
        yield return LaunchCourt("pearl_cantor","shellback_porter");QuietKitFixture();
        var owner=Enemy("pearl_cantor");var target=Enemy("shellback_porter");Set(target,"currentHealth",40);
        ReadyForest(owner);yield return Move();var kit=owner.GetComponent<EnemyChannelRuntime>();
        Assert.That(kit.IsChanneling,Is.True);Assert.That(kit.BubblesReduceHeal,Is.False);
        Call(kit,"BubbleCollected",1,true,false);Assert.That(kit.PendingHeal,Is.EqualTo(20));
        int hp=target.CurrentHealth;yield return Move();yield return Move();Assert.That(target.CurrentHealth,Is.EqualTo(hp+20));
        Set(target,"currentHealth",40);ReadyForest(owner);yield return Move();Assert.That(kit.IsChanneling,Is.True);
        Set(owner.GetComponent<EnemyStagger>(),"remainingImmunityTime",0f);owner.GetComponent<EnemyStagger>().ApplyStagger(1,1);
        Assert.That(kit.IsChanneling,Is.False);Assert.That(kit.Target,Is.Null);
        owner.GetComponent<EnemyStagger>().ClearStagger();QuietKitFixture();ReadyForest(owner);yield return Move();
        Assert.That(kit.IsChanneling,Is.True);target.TryTakeDamage(50000);yield return null;
        Assert.That(kit.IsChanneling,Is.False);Assert.That(kit.Outcome,Is.EqualTo("Target lost"));
    }
}

public sealed class ApprovedEnemyRevisionSnapshotTests
{
    [Test] public void NewChannelsRejectCorruptionAndLegacyDefaultsStayReadable()
    {
        Assert.That(EnemyChannelSnapshot.Supports(new EnemyChannelSnapshot()),Is.True);
        Assert.That(EnemyChannelSnapshot.Supports(new EnemyChannelSnapshot{version=1,pendingHeal=15}),Is.False);
        Assert.That(EnemyChannelSnapshot.Supports(new EnemyChannelSnapshot{version=2}),Is.False);
        Assert.That(ShieldbreakerSnapshot.Supports(new ShieldbreakerSnapshot{channeling=true,deadline=4,recoveryUntil=2}),Is.False);
        Assert.That(ShieldbreakerSnapshot.Supports(new ShieldbreakerSnapshot{channeling=true,deadline=4}),Is.True);
        Assert.That(HearthEmberSnapshot.Supports(new HearthEmberSnapshot{active=true,targetId=2,createdMove=1,expiresMove=4,lastHealMove=2}),Is.True);
        Assert.That(HearthEmberSnapshot.Supports(new HearthEmberSnapshot{active=true,targetId=2,createdMove=1,expiresMove=5,lastHealMove=2}),Is.False);
        var old=JsonUtility.FromJson<BoardWarningSnapshot>("{\"kind\":3,\"dueMove\":4}");
        Assert.That(old.frostColumn,Is.EqualTo(-1),"legacy fixed-cell warnings must not become Frost column zero");
    }
}
