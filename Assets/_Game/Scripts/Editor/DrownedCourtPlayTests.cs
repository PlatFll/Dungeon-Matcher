using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed partial class ForestFoundationPlayTests
{
    private IEnumerator LaunchCourt(params string[] ids)
    {
        var zone=Resources.Load<ZoneDefinition>("Zones/drowned-court");
        Assert.That(zone,Is.Not.Null);
        bool eligible=zone.eligibleForLiveTravel;var recipes=zone.liveEncounters;
        zone.eligibleForLiveTravel=true;
        if(ids.Length>0) zone.liveEncounters=new[]{new ZoneTestEncounter{label="Court regression formation",
            members=ids.Select(id=>zone.enemies.Single(e=>e.EnemyId==id)).ToArray()}};
        RunLaunchOptions.StartingZone="drowned-court";
        SceneManager.LoadScene("Game");yield return Stable();
        zone.eligibleForLiveTravel=eligible;zone.liveEncounters=recipes;
        Assert.That(Run.Zone.Definition.zoneId,Is.EqualTo("drowned-court"));
        Assert.That(Run.MoveClock.Capture().profile,Is.EqualTo(CombatClockSnapshot.LegacyEffectsProfile));
        Assert.That(Run.MoveClock.Capture().zoneId,Is.EqualTo("drowned-court"));
        Assert.That(EditorUtility.audioMasterMute,Is.True);
        yield return Until(()=>Run.Waves.ActiveEnemies.All(e=>e.GetComponent<EnemyLifecycleVFX>()?.IsSpawning!=true),"all court portraits finish their spawn presentation");
        foreach(var enemy in Run.Waves.ActiveEnemies) enemy.GetComponent<EnemyAutoAttack>().SetActionPaused(this,true);
        Set(Run.Waves,"advanceWavesAutomatically",false);
    }

    [UnityTest] public IEnumerator CourtFirstEncounterStaysDryAndFloodUsesAcceptedMoves()
    {
        yield return LaunchCourt("shellback_porter");
        for(int i=0;i<6;i++)yield return Move();
        Assert.That(Run.Board.Aquatic.phase,Is.EqualTo(TidePhase.Pending));
        Assert.That(Run.Board.IsFlooded,Is.False,"first whole encounter remains dry");
        Run.Board.Aquatic.protectedThroughEncounter=0;
        yield return Move();
        var state=Run.Board.Aquatic;
        Assert.That(state.phase,Is.EqualTo(TidePhase.Flooded));Assert.That(state.air,Is.EqualTo(5));
        Assert.That(state.wetMoves,Is.InRange(10,12));Assert.That(state.bubbles.Count,Is.EqualTo(2));
        int remaining=state.wetMoves;
        float observeUntil=Time.time+.15f;yield return Until(()=>Time.time>=observeUntil,"thinking does not advance tide");
        Assert.That(state.wetMoves,Is.EqualTo(remaining));
        yield return Move();Assert.That(state.wetMoves,Is.EqualTo(remaining-1));
    }

    [UnityTest] public IEnumerator CourtAirSurvivesContinuationAndFinalDrainDoesNotSuffocate()
    {
        yield return LaunchCourt("shellback_porter");yield return Move();
        Run.Board.Aquatic.StartFlood(12,Run.MoveClock.Tick);
        Run.Board.Aquatic.air=0;Run.Board.Aquatic.wetMoves=1;
        var saved=Run.Continuation.Capture();
        Assert.That(RunContinuation.SupportsSnapshot(saved),Is.True);
        Assert.That(saved.board.aquatic.wetMoves,Is.EqualTo(1));
        Assert.That(saved.board.aquatic.coffer,Is.Null,"an absent coffer remains absent in a checkpoint");
        int hp=Run.Player.CurrentHealth;
        yield return Move();Assert.That(Run.Player.CurrentHealth,Is.EqualTo(hp));
        Assert.That(Run.Board.IsFlooded,Is.False);Assert.That(Run.Board.Aquatic.bubbles,Is.Empty);
    }

    [UnityTest] public IEnumerator CourtBubbleReceiptsRespectPhysicalDestructionAndManualSnareSource()
    {
        yield return LaunchCourt("shellback_porter");yield return Move();
        var board=Run.Board;var state=board.Aquatic;state.StartFlood(12,Run.MoveClock.Tick);state.air=1;
        var gem=board.GetGem(0,0);state.bubbles.Add(gem.BoardIdentity);
        var clear=new HashSet<Gem>{gem};
        Call(board,"RegisterAquaticClear",clear,false);
        Call(board,"ResolveAquaticDestruction",clear,new HashSet<Gem>{gem});
        Assert.That(state.air,Is.EqualTo(1));Assert.That(state.bubbles,Does.Contain(gem.BoardIdentity),"special conversion preserves physical bubble");
        Call(board,"RegisterAquaticClear",clear,false);
        Call(board,"ResolveAquaticDestruction",clear,new HashSet<Gem>());
        Assert.That(state.air,Is.EqualTo(3));
        Call(board,"ResolveAquaticDestruction",clear,new HashSet<Gem>());
        Assert.That(state.air,Is.EqualTo(3),"duplicate clear gives no extra AIR");
        state.Accept(Run.MoveClock.Tick+1);
        state.snares.Add(new AquaticSnareState{gemId=gem.BoardIdentity,expiresMove=99});
        Call(board,"RegisterAquaticClear",clear,true);
        Call(board,"ResolveAquaticDestruction",clear,new HashSet<Gem>());
        Assert.That(state.snareLoss,Is.EqualTo(1));
    }

    [UnityTest] public IEnumerator CourtMorayRespectsShieldGateAndHealsOnlyHpLost()
    {
        yield return LaunchCourt("moray_siphoner","shellback_porter");
        var moray=Enemy("moray_siphoner");moray.ResolveDamageWithoutFeedback(25);
        int before=moray.CurrentHealth;Run.Player.GrantShield(5);int hp=Run.Player.CurrentHealth;
        for(int i=0;i<4;i++)yield return Move();
        var ability=moray.GetComponent<AquaticEnemyAbility>();
        Assert.That(ability.IsPreparing,Is.True);Assert.That(ability.ResponseMoves,Is.EqualTo(2));
        yield return Move();Assert.That(ability.ResponseMoves,Is.EqualTo(1));
        yield return Move();Assert.That(Run.Player.CurrentHealth,Is.EqualTo(hp));
        Assert.That(moray.CurrentHealth,Is.EqualTo(before));Assert.That(ability.BlocksBasic,Is.True);
    }

    [UnityTest] public IEnumerator CourtMaterialAndRosterImportsAreNativeAndComplete()
    {
        yield return LaunchCourt("queen_nacre","reef_spearman","hammerhead_bruiser");
        Assert.That(Run.Zone.Definition.enemies.Length,Is.EqualTo(14));
        Assert.That(Run.Zone.Definition.liveEncounters.Length,Is.EqualTo(32));
        foreach(var definition in Run.Zone.Definition.enemies)
        {
            Assert.That(definition.canFightFlooded,Is.True);
            var sprite=definition.FallbackVisualSprite;
            Assert.That(sprite,Is.Not.Null);Assert.That(sprite.texture.filterMode,Is.EqualTo(FilterMode.Point));
            Assert.That(sprite.texture.mipmapCount,Is.EqualTo(1));
        }
        var theme=Run.Zone.Definition.theme;
        Assert.That(theme.generalBackground,Is.Not.SameAs(theme.panelBackground));
        Assert.That(theme.boardCells.Length,Is.EqualTo(3));Assert.That(theme.airCoffer,Is.Not.Null);
        var layout=UnityEngine.Object.FindFirstObjectByType<GameplayPixelLayoutController>();
        Assert.That(GameplayPixelLayoutValidator.Validate(layout,out string report),Is.Empty,report);
        yield return new WaitForSeconds(1.5f);
        foreach(var actor in Run.Waves.ActiveEnemies)
        {
            var portrait=actor.transform.Find("VisualRoot").GetComponent<UnityEngine.UI.Image>();
            Assert.That(portrait.sprite,Is.Not.Null,actor.name);
            Assert.That(portrait.color.a,Is.GreaterThan(.99f),actor.name+" visible alpha");
            Assert.That(portrait.enabled,Is.True,actor.name+" visible image");
        }
        string output=System.IO.Path.GetFullPath(".utmp/DrownedCourtValidation/DryCourt.png");
        ScreenCapture.CaptureScreenshot(output);
        yield return null;
    }

    [UnityTest] public IEnumerator CourtCofferCaptureOwnerDeathAndDrainHaveSinglePayout()
    {
        yield return LaunchCourt("pearl_thief","shellback_porter");yield return Move();
        var board=Run.Board;PrepareSafeMove();board.Aquatic.StartFlood(12,Run.MoveClock.Tick);board.Aquatic.air=1;
        var candidates=(HashSet<Gem>)Call(board,"ImmediatelyClearableOrdinaryGems");
        Assert.That(candidates.Count,Is.GreaterThanOrEqualTo(3));
        board.Aquatic.bubbles=candidates.Take(2).Select(g=>g.BoardIdentity).ToList();
        Assert.That(board.TryPlanAirTheft(1,false,out var targets,out var site),Is.True);
        var owner=Enemy("pearl_thief");bool done=false,success=false;
        Assert.That(board.TryQueueAirTheft(owner,targets,site,1,false,result=>{done=true;success=result;}),Is.True);
        yield return Until(()=>done && Run.Continuation.CanCapture,"coffer capture resolves");
        Assert.That(success,Is.True);Assert.That(board.Aquatic.coffer.charges,Is.EqualTo(1));
        Assert.That(board.Aquatic.air,Is.EqualTo(1),"theft never debits the meter");
        Assert.That(board.Aquatic.bubbles.Count,Is.GreaterThanOrEqualTo(1),"ordinary rescue survives");
        board.ReleaseAquaticOwner(owner.PersistentId);
        board.ReleaseAquaticOwner(owner.PersistentId);
        yield return Stable();
        Assert.That(board.Aquatic.air,Is.EqualTo(3),"owner cleanup refunds once");
        Assert.That(board.Aquatic.coffer,Is.Null);
        Assert.That(board.GetGem(site.x,site.y),Is.Not.Null,"coffer footprint refills");
    }

    [UnityTest] public IEnumerator CourtSnareExpiresAfterThreeFutureMovesAndSparesAirRoutes()
    {
        yield return LaunchCourt("reef_netweaver","shellback_porter");yield return Move();PrepareSafeMove();
        var board=Run.Board;board.Aquatic.StartFlood(12,Run.MoveClock.Tick);
        // The placement probe needs a useful combat answer, not the deliberately
        // harmless color used by the general timing fixture.
        var types=(Sprite[])Get(board,"gemSprites");var useful=Enemy("shellback_porter").AssignedGemType;
        foreach(var point in new[]{new Vector2Int(safeMoveTo.x-2,safeMoveTo.y),new Vector2Int(safeMoveTo.x-1,safeMoveTo.y),safeMoveFrom})
            board.GetGem(point.x,point.y).SetType(useful,types[(int)useful]);
        var routes=(HashSet<Gem>)Call(board,"ImmediatelyClearableOrdinaryGems");
        var owner=Enemy("reef_netweaver");
        Assert.That(board.TryApplyAquaticSnares(owner),Is.True);
        var placed=board.Aquatic.snares.Select(s=>s.gemId).ToArray();
        Assert.That(placed.Length,Is.InRange(1,2));
        Assert.That(placed.All(id=>board.IsGemPinned(board.FindAquaticGem(id))),Is.True,"thorn snares block direct swaps through the canonical restriction query");
        Assert.That(board.RestrictionCount,Is.EqualTo(placed.Length));
        Assert.That(placed.Intersect(routes.Select(g=>g.BoardIdentity)),Is.Empty);
        Assert.That(board.Aquatic.snares.All(s=>s.expiresMove==Run.MoveClock.Tick+3),Is.True);
        for(int i=0;i<3;i++)yield return Move();
        Assert.That(board.Aquatic.snares.Any(s=>placed.Contains(s.gemId)),Is.False);
    }

    [UnityTest] public IEnumerator CourtWetResumePreservesAirAndChannelDeadline()
    {
        yield return LaunchCourt("moray_siphoner","shellback_porter");
        for(int i=0;i<4;i++)yield return Move();
        var board=Run.Board;board.Aquatic.StartFlood(12,Run.MoveClock.Tick);board.Aquatic.air=2;
        var before=Run.Continuation.Capture();string runId=Run.RunId;
        Assert.That(Enemy("moray_siphoner").GetComponent<AquaticEnemyAbility>().ResponseMoves,Is.EqualTo(2));
        Assert.That(Run.SuspendToMenu(),Is.True);yield return null;yield return null;
        SceneManager.LoadScene("Game");yield return Stable();
        Assert.That(Run.RunId,Is.EqualTo(runId));Assert.That(Run.Board.IsFlooded,Is.True);
        Assert.That(Run.Board.Aquatic.air,Is.EqualTo(2));Assert.That(Run.Board.Aquatic.wetMoves,Is.EqualTo(12));
        Assert.That(Run.MoveClock.Tick,Is.EqualTo(before.board.moves));
        Assert.That(Enemy("moray_siphoner").GetComponent<AquaticEnemyAbility>().ResponseMoves,Is.EqualTo(2));
        Assert.That(Run.Board.Aquatic.coffer,Is.Null);
        Run.GetComponent<RunControlsUI>().Close();
        yield return Until(()=>Run.Waves.ActiveEnemies.All(e=>e.GetComponent<EnemyLifecycleVFX>()?.IsSpawning!=true),"restored portraits finish spawning");
        yield return new WaitForSeconds(.5f);
        ScreenCapture.CaptureScreenshot(System.IO.Path.GetFullPath(".utmp/DrownedCourtValidation/WetCourt.png"));
        yield return null;
    }

    [UnityTest] public IEnumerator CourtConchDamageLeaseDoesNotStackSpeedAndSurvivesOtherOwnerDeath()
    {
        yield return LaunchCourt("conch_marshal","conch_marshal","shellback_porter");
        var marshals=Run.Waves.ActiveEnemies.Where(e=>e.Definition.EnemyId=="conch_marshal").ToArray();
        var ally=Enemy("shellback_porter").GetComponent<EnemyAutoAttack>();
        foreach(var marshal in marshals)Call(marshal.GetComponent<AquaticEnemyAbility>(),"ApplyRally");
        var damage=(Dictionary<object,float>)Get(ally,"damageModifiers");
        var speed=(Dictionary<object,float>)Get(ally,"speedModifiers");
        Assert.That(damage.Values.Single(),Is.EqualTo(1.3f));Assert.That(speed.Values.Single(),Is.EqualTo(1));
        marshals[0].ResolveDamageWithoutFeedback(999999);
        Assert.That(damage.Values.Single(),Is.EqualTo(1.3f),"another living owner's lease remains");
        float seconds=((AquaticEnemySnapshot)Get(marshals[1].GetComponent<AquaticEnemyAbility>(),"state")).rallySeconds;
        Time.timeScale=0;float pausedUntil=Time.realtimeSinceStartup+.1f;
        yield return Until(()=>Time.realtimeSinceStartup>=pausedUntil,"paused lease observation");
        Assert.That(((AquaticEnemySnapshot)Get(marshals[1].GetComponent<AquaticEnemyAbility>(),"state")).rallySeconds,Is.EqualTo(seconds));
        Time.timeScale=1;float expiry=Time.time+seconds+.1f;
        yield return Until(()=>Time.time>=expiry,"five seconds of scaled combat time pass");
        var remaining=(AquaticEnemySnapshot)Get(marshals[1].GetComponent<AquaticEnemyAbility>(),"state");
        Assert.That(damage,Is.Empty,"lease remaining="+remaining.rallySeconds+"; first defeated="+marshals[0].IsDefeated+
            "; second active="+marshals[1].GetComponent<AquaticEnemyAbility>().isActiveAndEnabled);
        Assert.That(speed,Is.Empty);
    }

    [UnityTest] public IEnumerator CourtPufferHoldsInflationThroughBasicsThenDeflatesWithoutStalling()
    {
        yield return LaunchCourt("puffer_sentinel","shellback_porter");
        var puffer=Enemy("puffer_sentinel");var ability=puffer.GetComponent<AquaticEnemyAbility>();
        for(int i=0;i<4;i++)yield return Move();
        Assert.That(ability.IsPreparing,Is.True);Assert.That(ability.BlocksBasic,Is.False);
        Assert.That(puffer.SpecialIdleState,Is.EqualTo("InflatedIdle"));
        var attack=puffer.GetComponent<EnemyAutoAttack>();attack.SetActionPaused(this,false);
        Assert.That(attack.PerformAttackImmediately(),Is.True);
        yield return Until(()=>!attack.IsAttackSequenceInProgress,"inflated basic reaches impact and recovery");
        attack.SetActionPaused(this,true);
        Assert.That(puffer.SpecialAutoAttackState,Is.EqualTo("InflatedAttack"));
        Assert.That(ability.ResponseMoves,Is.EqualTo(2),"a timed basic spends no response move");
        yield return Move();yield return Move();
        Assert.That(ability.IsPreparing,Is.False);Assert.That(puffer.SpecialIdleState,Is.Null);
        Assert.That(puffer.SpecialAutoAttackState,Is.Null);Assert.That(puffer.HasAnimationActionInProgress,Is.False);
    }

    [UnityTest] public IEnumerator CourtQueenAnswersDeduplicateAndRotationSummonsIntoAnnouncedSlot()
    {
        yield return LaunchCourt("queen_nacre","shellback_porter");
        var queen=Enemy("queen_nacre");var ability=queen.GetComponent<AquaticEnemyAbility>();
        for(int i=0;i<3;i++)yield return Move();
        Assert.That(queen.CurrentShield,Is.EqualTo(20));
        for(int i=0;i<5;i++)yield return Move();
        Assert.That(ability.CastName,Is.EqualTo("DEPTHS"));Assert.That(ability.ResponseMoves,Is.EqualTo(3));
        Assert.That(ability.ResponseCells.Count,Is.EqualTo(2));
        var cell=ability.ResponseCells[0];var gem=Run.Board.GetGem(cell.x,cell.y);
        var targets=new HashSet<Gem>{gem};
        Call(Run.Board,"RegisterAquaticClear",targets,false);Call(Run.Board,"ResolveAquaticDestruction",targets,new HashSet<Gem>());
        Call(Run.Board,"RegisterAquaticClear",targets,false);Call(Run.Board,"ResolveAquaticDestruction",targets,new HashSet<Gem>());
        Assert.That(ability.Answers,Is.EqualTo(1),"one physical identity cannot answer twice");
        int hp=Run.Player.CurrentHealth;
        for(int i=0;i<3;i++)yield return Move();
        Assert.That(Run.Player.CurrentHealth,Is.InRange(hp-25,hp-10));
        for(int i=0;i<5;i++)yield return Move();
        Assert.That(ability.CastName,Is.EqualTo("MUSTER"));
        int slot=((AquaticEnemySnapshot)Get(ability,"state")).summonSlot;Assert.That(slot,Is.GreaterThanOrEqualTo(0));
        yield return Move();yield return Move();
        Assert.That(Run.Waves.ActiveEnemies.Count(e=>e.Definition.EnemyId=="skittercrab"),Is.EqualTo(1));
        var crab=Enemy("skittercrab");queen.ResolveDamageWithoutFeedback(999999);queen.ResolveDamageWithoutFeedback(999999);
        Assert.That(crab.IsDefeated,Is.False);Assert.That(Run.Travel.State.stage,Is.Zero,"living escort prevents apex travel");
    }

    [UnityTest] public IEnumerator CourtMotionControllersResolveEveryBasicWithOneOwnedSequence()
    {
        foreach(string id in DrownedCourtImporter.Ids)
        {
            if(Run!=null){Assert.That(Run.ExitTo("MainMenu"),Is.True);yield return null;yield return null;}
            yield return LaunchCourt(id);
            var actor=Enemy(id);var attack=actor.GetComponent<EnemyAutoAttack>();
            Assert.That(actor.Definition.UseAuthoredAutoAttackMotion,Is.True,id);
            int hits=0;attack.AttackResolved+=(_,damage,finishes)=>hits++;
            attack.SetActionPaused(this,false);
            Assert.That(attack.PerformAttackImmediately(),Is.True,id);
            yield return Until(()=>!attack.IsAttackSequenceInProgress,"owned basic completes: "+id);
            attack.SetActionPaused(this,true);
            Assert.That(hits,Is.EqualTo(id=="needlefin_skirmisher"?2:1),id);
        }
    }

    [UnityTest] public IEnumerator CourtThreeZoneSoakCleansEffectsPreservesRunAndBoundsSaveSize()
    {
        using var destinations=new TravelDestinationFixture("dungeon","magical-forest","drowned-court");
        yield return LaunchCourt("shellback_porter");
        yield return Move();
        string id=Run.RunId;long largest=0;int initialObjects=0;
        string[] route={"dungeon","drowned-court","magical-forest","drowned-court","dungeon","magical-forest"};
        for(int hop=0;hop<18;hop++)
        {
            Run.Travel.enabled=false;Set(Run.Waves,"advanceWavesAutomatically",false);
            foreach(var enemy in Run.Waves.ActiveEnemies)enemy.GetComponent<EnemyAutoAttack>().SetActionPaused(this,true);
            KillEncounter();yield return Until(()=>!Run.Waves.IsWaveActive && Run.Continuation.CanCapture,"source encounter finishes");
            var draft=Run.Waves.GetComponent<RunUpgradeCoordinator>();
            var snapshot=Run.Continuation.Capture();
            if(snapshot.draft.Count>0){Assert.That(draft.SelectRecordedCard(snapshot.draft[0]),Is.True);yield return StableAfterCourtWave();}
            if(Run.Zone.Definition.zoneId=="drowned-court")
            {
                Run.Board.Aquatic.StartFlood(12,Run.Board.CompletedValidPlayerMoves);Run.Board.Aquatic.air=1;
                Run.Board.Aquatic.bubbles.Add(Run.Board.GetGem(0,0).BoardIdentity);
            }
            else if(Run.Zone.Definition.zoneId=="magical-forest")
            {Run.Board.QueueEnvironmentalVine(Run.Board.GetGem(0,0));yield return StableAfterCourtWave();}
            var before=Run.Continuation.Capture();string destination=route[hop%route.Length];
            int expectedEnergy=Mathf.Min(Run.Player.GetComponent<PlayerAbilityEnergy>().MaximumEnergy,
                before.player.energy+(RunUpgradeResolver.HasMechanic(RunUpgradeMechanic.PreparedCasting,RunUpgradeRuntime.Current)?15:0));
            Assert.That(destination,Is.Not.EqualTo(Run.Zone.Definition.zoneId));
            Run.Travel.State.stage=1;Run.Travel.State.destination=destination;
            Assert.That(Run.Continuation.TryCommitZoneTravel(destination),Is.True);
            SceneManager.LoadScene("Game");yield return null;yield return null;
            yield return Until(()=>Run?.InitialStateReady==true && !Run.Continuation.IsRestoring,"committed handoff restores");
            Run.GetComponent<RunControlsUI>().Close();yield return Stable();
            yield return Until(()=>Run.Travel.State.stage==0,"destination reveal finishes");
            Assert.That(Run.RunId,Is.EqualTo(id));Assert.That(Run.Zone.Definition.zoneId,Is.EqualTo(destination));
            Assert.That(Run.Board.IsFlooded,Is.False);Assert.That(Run.Board.VineCount,Is.Zero);
            var after=Run.Continuation.Capture();AssertBoardCarryover(before.board,after.board);
            Assert.That(after.player.health,Is.EqualTo(before.player.health));
            Assert.That(after.player.energy,Is.EqualTo(expectedEnergy),"one new destination wave may grant Prepared Casting once");
            if(destination=="drowned-court")
            {Assert.That(Run.Board.Aquatic.bubbles,Is.Empty);Assert.That(Run.Board.Aquatic.snares,Is.Empty);Assert.That(Run.Board.Aquatic.coffer,Is.Null);}
            Assert.That(Run.MoveClock.Capture().profile,Is.EqualTo(CombatClockSnapshot.LegacyEffectsProfile));
            largest=Math.Max(largest,new System.IO.FileInfo(path).Length);
            int count=UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None).Length;
            if(hop==5)initialObjects=count;
            if(hop>=6)Assert.That(count,Is.LessThan(initialObjects+250),"scene objects do not accumulate across trips");
        }
        Assert.That(largest,Is.LessThan(500000),"eighteen handoffs keep this fixture's durable account below 500 KB");
        System.IO.File.WriteAllText(".utmp/DrownedCourtValidation/TravelSoak.txt","18 handoffs; 6 route repeats; largest account bytes="+largest+"; final visit="+Run.Travel.State.visit);
    }

    private IEnumerator StableAfterCourtWave()=>Until(()=>Run.Continuation.CanCapture,"completed wave board settles");

    [UnityTest] public IEnumerator CourtCaptainCommandsOneCompleteFixedAllySequence()
    {
        yield return LaunchCourt("breakwater_captain","needlefin_skirmisher");
        var captain=Enemy("breakwater_captain");var ally=Enemy("needlefin_skirmisher");
        var attack=ally.GetComponent<EnemyAutoAttack>();attack.SetActionPaused(this,false);Set(attack,"remainingAttackTime",999f);
        int hits=0;attack.AttackResolved+=(_,__,___)=>hits++;
        for(int i=0;i<3;i++)yield return Move();
        Assert.That(ally.CurrentShield,Is.EqualTo(25));
        for(int i=0;i<5;i++)yield return Move();
        var ability=captain.GetComponent<AquaticEnemyAbility>();Assert.That(ability.CastName,Is.EqualTo("BOARDING"));
        Assert.That(ability.Target,Is.SameAs(ally));Assert.That(ability.ResponseMoves,Is.EqualTo(2));
        yield return Move();yield return Move();
        Assert.That(hits,Is.EqualTo(2),"the two-dart basic sequence is commanded exactly once");
        Assert.That(attack.HasCommandReservation,Is.False);Assert.That(ability.BlocksBasic,Is.True);
    }

    [UnityTest] public IEnumerator CourtPhotographCannotRestoreSpentAirBubblesOrSolvedSnares()
    {
        yield return LaunchCourt("reef_netweaver","shellback_porter");yield return Move();
        var board=Run.Board;var tide=board.Aquatic;tide.StartFlood(12,Run.MoveClock.Tick);tide.air=5;
        var gem=board.GetGem(0,0);int identity=gem.BoardIdentity;
        tide.bubbles.Add(identity);tide.snares.Add(new AquaticSnareState{gemId=identity,expiresMove=Run.MoveClock.Tick+3,ownerId=Enemy("reef_netweaver").PersistentId});
        var photo=board.CaptureBoardMemory(Run.Waves.ContinuationOwnerSlot);
        tide.air=1;var clear=new HashSet<Gem>{gem};Call(board,"RegisterAquaticClear",clear,false);
        Call(board,"ResolveAquaticDestruction",clear,new HashSet<Gem>());
        Assert.That(tide.air,Is.EqualTo(3));
        Assert.That(board.TryRestoreBoardMemory(photo,Run.Waves.ContinuationEnemy),Is.True);
        Assert.That(board.Aquatic.air,Is.EqualTo(3));CollectionAssert.DoesNotContain(board.Aquatic.bubbles,identity);
        Assert.That(board.Aquatic.snares,Is.Empty);Assert.That(board.Aquatic.wetMoves,Is.EqualTo(12));
    }

    [UnityTest] public IEnumerator CourtPortraitMaterialsAndFloodPresentationCapture()
    {
        yield return LaunchCourt("queen_nacre","reef_spearman","hammerhead_bruiser");yield return Move();
        var layout=UnityEngine.Object.FindFirstObjectByType<GameplayPixelLayoutController>();
        var backdrop=UnityEngine.Object.FindFirstObjectByType<BattleBackgroundTilemapController>();
        string folder=System.IO.Path.GetFullPath(".utmp/DrownedCourtValidation");
        try
        {
            int variant=0;
            foreach(var size in new[]{new Vector2Int(720,1280),new Vector2Int(1080,1920),new Vector2Int(1080,2400)})
            {
                typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic)
                    .Invoke(null,new object[]{size});
                yield return Until(()=>Screen.width==size.x && Screen.height==size.y,"court portrait size applies");
                backdrop.ApplyGameplayTheme(Run.Zone.Definition.theme,variant++);
                for(int f=0;f<8;f++)yield return null;
                Assert.That(GameplayPixelLayoutValidator.Validate(layout,out string report),Is.Empty,report);
                System.IO.File.WriteAllText(System.IO.Path.Combine(folder,"Layout-"+size.x+"x"+size.y+".txt"),report);
                ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(folder,"Scene-"+variant+"-"+size.x+"x"+size.y+".png"));yield return null;yield return null;
                Run.Board.Aquatic.StartFlood(12,Run.MoveClock.Tick);Call(Run.Board,"EnsureAquaticSupply",2);
                float rise=Time.time+.5f;yield return Until(()=>Time.time>=rise,"water rises");
                var air=GameObject.Find("CourtAir").GetComponent<RectTransform>();
                foreach(var text in air.GetComponentsInChildren<TMPro.TMP_Text>())
                {text.ForceMeshUpdate();Assert.That(text.isTextOverflowing,Is.False,text.name);}
                ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(folder,"Wet-"+size.x+"x"+size.y+".png"));yield return null;yield return null;
                Run.Board.Aquatic.Drain(Run.Travel.LocalWave);
                float drain=Time.time+.5f;yield return Until(()=>Time.time>=drain,"water finishes draining before the next dry capture");
            }
            GameplayPixelLayoutController.ValidationSafeArea=new Rect(32,72,1016,2240);
            for(int f=0;f<8;f++)yield return null;
            Assert.That(GameplayPixelLayoutValidator.Validate(layout,out string safeReport),Is.Empty,safeReport);
            ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(folder,"SafeArea-1080x2400.png"));yield return null;yield return null;
        }
        finally{GameplayPixelLayoutController.ValidationSafeArea=null;}
        string frames=System.IO.Path.Combine(folder,"FloodFrames");System.IO.Directory.CreateDirectory(frames);
        Run.Board.Aquatic.StartFlood(12,Run.MoveClock.Tick);Call(Run.Board,"EnsureAquaticSupply",2);
        float began=Time.time;
        for(int frame=0;frame<24;frame++)
        {
            if(frame==14)Run.Board.Aquatic.Drain(Run.Travel.LocalWave);
            ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(frames,frame.ToString("D3")+".png"));
            float next=began+(frame+1)/12f;yield return Until(()=>Time.time>=next,"flood presentation recording frame");
        }
        yield return null;yield return null;
    }
}
