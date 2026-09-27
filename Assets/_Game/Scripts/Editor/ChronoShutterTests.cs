using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed class ChronoShutterTests
{
    private const BindingFlags Fields = BindingFlags.Instance | BindingFlags.NonPublic;

    [Test] public void PlayableDefinitionUsesApprovedFramesAndDedicatedHold()
    {
        var player = AssetDatabase.LoadAssetAtPath<PlayerDefinition>(GideonGlassImporter.PlayerPath);
        Assert.That(player.PlayerId, Is.EqualTo(CharacterSelectionSettings.GideonPlayerId));
        Assert.That(player.ActiveAbility, Is.TypeOf<ChronoShutterAbilityDefinition>());
        Assert.That(player.ActiveAbility.EnergyCost, Is.EqualTo(100));
        Assert.That(player.ActiveAbility.Icon, Is.Not.Null);
        Assert.That(player.ActiveAbility.Icon.rect.size, Is.EqualTo(new Vector2(176,64)), "native button frame is retained");
        Assert.That(((ChronoShutterAbilityDefinition)player.ActiveAbility).ManualMoves, Is.EqualTo(5));
        Assert.That(player.BattleCharacterSprite.rect.size, Is.EqualTo(new Vector2(64,64)));
        Assert.That(player.BattleCharacterSprite.pixelsPerUnit, Is.EqualTo(64));
        var controller = (AnimatorController)player.BattleAnimatorController;
        var states = controller.layers[0].stateMachine.states.Select(s => s.state).ToArray();
        CollectionAssert.AreEquivalent(new[] {"Idle","Ability","Hold","Recovery"}, states.Select(s => s.name));
        Assert.That(states.Single(s => s.name == "Hold").transitions, Is.Empty, "hold cannot fall back into idle");
        Assert.That(states.Single(s => s.name == "Ability").motion.averageDuration, Is.EqualTo(.85f).Within(.011f));
        foreach (var name in new[] {"Idle","Cast","Hold","Recovery"})
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(GideonGlassImporter.ArtRoot + "/Gideon_" + name + ".png");
            Assert.That(importer.filterMode, Is.EqualTo(FilterMode.Point));
            Assert.That(importer.textureCompression, Is.EqualTo(TextureImporterCompression.Uncompressed));
            Assert.That(importer.mipmapEnabled, Is.False);
        }
    }

    [UnityTest] public IEnumerator FiveRealMovesRestoreWithoutRewardsAndReplayTheSameRefills()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        yield return new EnterPlayMode();
        using (AccountProgression.UseDisposableProfile(Profile()))
        using (CharacterSelectionSettings.UseTemporarySelection(CharacterSelectionSettings.GideonPlayerId))
        {
            SceneManager.LoadScene("Game"); yield return Stable();
            var run = RunSession.Current; MakeDurableEnemies(run);
            var board = run.Board;
            board.GetGem(0,0).SetSpecialType(GemSpecialType.RowBomb);
            board.GetGem(1,0).SetSpecialType(GemSpecialType.ColumnBomb);
            board.GetGem(2,0).SetSpecialType(GemSpecialType.ColorCrystal);
            var initial = board.CaptureBoardMemory(id => id);
            var energy = run.Player.GetComponent<PlayerAbilityEnergy>();
            var controller = run.Player.GetComponent<PlayerAbilityController>();
            energy.AddEnergy(100);
            int paidFrom = energy.CurrentEnergy;
            Assert.That(controller.TryActivate(), Is.True);
            var ability = run.Player.GetComponent<ChronoShutterRuntime>();
            Assert.That(energy.CurrentEnergy, Is.EqualTo(paidFrom - controller.RequiredEnergy));
            Assert.That(controller.TryActivate(), Is.False, "recast rejected");
            Assert.That(ability.RemainingMoves, Is.EqualTo(5));
            yield return Until(() => ability.Phase == BoardMemoryPhase.Holding, "cast completes");
            Assert.That(board.IsExternalInputBlocked, Is.False);

            int accepted = 0, completed = 0, clears = 0, rewinds = 0;
            int clearsAtCompletion = 0, energyAtCompletion = 0, goldAtCompletion = 0;
            int[] healthAtCompletion = null;
            uint unrelatedAtCompletion = 0;
            bool fifthWasResolving = false;
            var turns = new List<Vector4Int>();
            var futures = new List<string>();
            var futureRandom = new List<uint>();
            board.BoardClearOutcomeResolved += _ => clears++;
            board.ValidPlayerMoveAccepted += _ =>
            {
                accepted++;
                if (ability.RemainingMoves == 0 && ability.IsActive)
                { fifthWasResolving = board.IsBusy; Assert.That(rewinds, Is.Zero); }
            };
            board.ValidPlayerMoveCompleted += _ =>
            {
                completed++; clearsAtCompletion = clears;
                energyAtCompletion = energy.CurrentEnergy; goldAtCompletion = AccountProgression.Current.Gold;
                healthAtCompletion = run.Waves.ActiveEnemies.Select(e => e.CurrentHealth).ToArray();
                unrelatedAtCompletion = run.Continuation.Random.State;
                if (completed <= 5) { futures.Add(Colors(board)); futureRandom.Add(board.RefillRandomState); }
            };
            board.BoardStateRestored += () =>
            {
                rewinds++;
                Assert.That(completed, Is.EqualTo(5));
                Assert.That(clears, Is.EqualTo(clearsAtCompletion), "state replacement emits no clear/reward event");
                Assert.That(energy.CurrentEnergy, Is.EqualTo(energyAtCompletion));
                Assert.That(AccountProgression.Current.Gold, Is.EqualTo(goldAtCompletion));
                CollectionAssert.AreEqual(healthAtCompletion, run.Waves.ActiveEnemies.Select(e => e.CurrentHealth));
                Assert.That(run.Continuation.Random.State, Is.EqualTo(unrelatedAtCompletion), "enemy randomness is untouched");
            };

            // A rejected manual swap does not spend a photographed move.
            bool invalid = false;
            for (int y=0; y<board.Height && !invalid; y++) for (int x=0; x<board.Width-1 && !invalid; x++)
                if (!board.IsHintMoveStillValid(board.GetGem(x,y),board.GetGem(x+1,y)))
                { board.ReplayPlayerSwap(x,y,x+1,y); invalid = true; }
            Assert.That(invalid, Is.True); yield return Stable();
            Assert.That(accepted, Is.Zero); Assert.That(ability.RemainingMoves, Is.EqualTo(5));
            for (int move=0; move<5; move++)
            {
                Assert.That(board.TryGetRandomHintMove(out var a,out var b), Is.True);
                turns.Add(new Vector4Int(a.Column,a.Row,b.Column,b.Row));
                board.ReplayPlayerSwap(a.Column,a.Row,b.Column,b.Row);
                int expected = move + 1;
                yield return Until(() => completed >= expected, "manual move fully resolves");
                if (move < 4) Assert.That(ability.RemainingMoves, Is.EqualTo(4-move), "cascades count once");
            }
            yield return Until(() => !ability.IsActive, "rewind and recovery finish");
            Assert.That(fifthWasResolving, Is.True, "zero appears while the fifth move is still resolving");
            Assert.That(accepted, Is.EqualTo(5)); Assert.That(completed, Is.EqualTo(5)); Assert.That(rewinds, Is.EqualTo(1));
            Assert.That(energyAtCompletion, Is.GreaterThan(0), "ordinary energy remains enabled during hold");
            Assert.That(Colors(board), Is.EqualTo(Colors(initial)));
            Assert.That(board.RefillRandomState, Is.EqualTo(initial.refillRandom));
            CollectionAssert.AreEqual(initial.cells.Select(c=>c.identity),
                board.CaptureBoardMemory(id=>id).cells.Select(c=>c.identity), "photographed logical gem identities return");
            Assert.That(board.CompletedValidPlayerMoves, Is.EqualTo(5), "enemy time is not rewound");
            for (int move=0; move<5; move++)
            {
                var input = turns[move];
                board.ReplayPlayerSwap(input.x,input.y,input.z,input.w);
                int expected = move + 6;
                yield return Until(() => completed >= expected && !board.IsBusy, "repeated manual move resolves");
                Assert.That(Colors(board), Is.EqualTo(futures[move]), "same board after repeated turn " + (move+1));
                Assert.That(board.RefillRandomState, Is.EqualTo(futureRandom[move]));
            }
            Assert.That(rewinds, Is.EqualTo(1));
            Assert.That(run.ExitTo("MainMenu"), Is.True); yield return null;
        }
        yield return new ExitPlayMode();
    }

    [UnityTest] public IEnumerator ActivePhotographAndInFlightFifthMoveSurviveRestart()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        yield return new EnterPlayMode();
        SceneManager.sceneLoaded += InstallDurableFixture;
        string path=Profile();
        using (AccountProgression.UseDisposableProfile(path))
        using (CharacterSelectionSettings.UseTemporarySelection(CharacterSelectionSettings.GideonPlayerId))
        {
            SceneManager.LoadScene("Game");yield return Stable();
            var run=RunSession.Current;
            Assert.That(run.Board.TryQueueTopUpMovablePins(run.Waves.ActiveEnemies[0],1,null,null),Is.True);
            yield return Stable();
            run.Player.GetComponent<PlayerAbilityEnergy>().AddEnergy(100);
            Assert.That(run.Player.GetComponent<PlayerAbilityController>().TryActivate(),Is.True);
            var ability=run.Player.GetComponent<ChronoShutterRuntime>();
            yield return Until(()=>ability.Phase==BoardMemoryPhase.Holding,"hold begins");
            for(int i=0;i<2;i++)yield return HintTurn();
            Assert.That(ability.RemainingMoves,Is.EqualTo(3));
            Time.timeScale=0;Assert.That(run.Continuation.SaveNow(),Is.True);
            var saved=AccountProgression.Current.ActiveRun.checkpoint;
            string photo=JsonUtility.ToJson(saved.boardMemory.photograph);
            string board=JsonUtility.ToJson(saved.board);
            Assert.That(saved.boardMemory.photograph.cells.Any(c=>c.pinned&&c.pinOwner>=0),Is.True,"owner keys are stable encounter slots");
            Assert.That(run.SuspendToMenu(),Is.True);yield return null;
            using(AccountProgression.UseDisposableProfile(path))
            {
                SceneManager.LoadScene("Game");yield return Restored();run=RunSession.Current;
                ability=run.Player.GetComponent<ChronoShutterRuntime>();
                Assert.That(ability.Phase,Is.EqualTo(BoardMemoryPhase.Holding));
                Assert.That(ability.RemainingMoves,Is.EqualTo(3));
                Assert.That(JsonUtility.ToJson(run.Board.CaptureContinuation(run.Waves.ContinuationOwnerSlot)),Is.EqualTo(board));
                Assert.That(JsonUtility.ToJson(ability.CaptureContinuation().photograph),Is.EqualTo(photo));
                run.GetComponent<RunControlsUI>().Close();
                for(int i=0;i<2;i++)yield return HintTurn();
                Assert.That(ability.RemainingMoves,Is.EqualTo(1));
                // Force a long fifth action through the real double-crystal pipeline.
                var a=run.Board.GetGem(0,0);var b=run.Board.GetGem(1,0);
                run.Board.QueueReleasePinnedGems(run.Waves.ActiveEnemies[0].GetInstanceID());yield return Stable();
                a=run.Board.GetGem(0,0);b=run.Board.GetGem(1,0);
                a.SetSpecialType(GemSpecialType.ColorCrystal);b.SetSpecialType(GemSpecialType.ColorCrystal);
                run.Board.ReplayPlayerSwap(0,0,1,0);
                yield return Until(()=>ability.Phase==BoardMemoryPhase.AwaitingSettlement,"fifth move accepted");
                Assert.That(run.Board.IsBusy,Is.True,"rewind waits through double-crystal clear, refill and cascades");
                Assert.That(run.Continuation.SaveNow(),Is.True);
                string pending=File.ReadAllText(path);
                yield return Until(()=>!ability.IsActive,"original fifth timeline completes");
                string expected=Colors(run.Board);uint random=run.Board.RefillRandomState;
                int energy=run.Player.GetComponent<PlayerAbilityEnergy>().CurrentEnergy;
                int gold=AccountProgression.Current.Gold;
                SceneManager.LoadScene("MainMenu");yield return null;
                File.WriteAllText(path,pending);
                using(AccountProgression.UseDisposableProfile(path))
                {
                    SceneManager.LoadScene("Game");yield return Restored();run=RunSession.Current;
                    run.GetComponent<RunControlsUI>().Close();
                    ability=run.Player.GetComponent<ChronoShutterRuntime>();
                    yield return Until(()=>!ability.IsActive,"recorded fifth move finishes once");
                    Assert.That(Colors(run.Board),Is.EqualTo(expected));
                    Assert.That(run.Board.RefillRandomState,Is.EqualTo(random));
                    Assert.That(run.Board.CompletedValidPlayerMoves,Is.EqualTo(5));
                    Assert.That(run.Player.GetComponent<PlayerAbilityEnergy>().CurrentEnergy,Is.EqualTo(energy));
                    Assert.That(AccountProgression.Current.Gold,Is.EqualTo(gold));
                    Assert.That(run.ExitTo("MainMenu"),Is.True);yield return null;
                }
            }
        }
        SceneManager.sceneLoaded-=InstallDurableFixture;
        yield return new ExitPlayMode();
    }

    [UnityTest] public IEnumerator RestorePreservesSpecialsAndObstaclesWithoutPhysicalClearSideEffects()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        yield return new EnterPlayMode();
        using(AccountProgression.UseDisposableProfile(Profile()))
        using(CharacterSelectionSettings.UseTemporarySelection(CharacterSelectionSettings.GideonPlayerId))
        {
            SceneManager.LoadScene("Game");yield return Stable();var run=RunSession.Current;MakeDurableEnemies(run);
            var board=run.Board;var owner=run.Waves.ActiveEnemies[0];
            Assert.That(board.TryQueueMineRandomCell(owner,1),Is.True);
            Assert.That(board.TryQueuePlaceBarricades(owner,1,1,2,EnemyBarricadeStyle.Stone),Is.True);
            Assert.That(board.TryQueueTopUpMovablePins(owner,1,null,null),Is.True);
            yield return Stable();
            Assert.That(board.TryQueueFreezeRandomGem(owner,2),Is.True);
            yield return Stable();
            Assert.That(board.TryQueuePinRandomGem(owner,3),Is.True);
            Assert.That(board.TryQueuePlaceRoyalBanner(owner),Is.True);
            yield return Stable();
            int index=0;
            foreach(GemSpecialType special in Enum.GetValues(typeof(GemSpecialType)))
            {
                while(index<board.Width*board.Height && (board.GetGem(index%board.Width,index/board.Width)==null || board.IsGemPinned(board.GetGem(index%board.Width,index/board.Width))))index++;
                var gem=board.GetGem(index%board.Width,index/board.Width);Assert.That(gem,Is.Not.Null);
                gem.SetSpecialType(special);index++;
            }
            var saved=board.CaptureBoardMemory(id=>id);
            Assert.That(board.IsValidBoardMemory(saved),Is.True);
            int clears=0;board.BoardClearOutcomeResolved+=_=>clears++;
            uint unrelated=run.Continuation.Random.State;
            // Replacing identical state exercises delayed Gem/overlay destruction too.
            Assert.That(board.TryRestoreBoardMemory(saved,id=>id==owner.GetInstanceID()?owner:null),Is.True);
            yield return null;yield return null;
            var restored=board.CaptureBoardMemory(id=>id);
            for(int i=0;i<saved.cells.Count;i++)Assert.That(JsonUtility.ToJson(restored.cells[i]),Is.EqualTo(JsonUtility.ToJson(saved.cells[i])),"cell "+i);
            Assert.That(clears,Is.Zero);Assert.That(unrelated,Is.EqualTo(run.Continuation.Random.State));
            Assert.That(board.GetPinnedGemCountForOwner(owner.GetInstanceID()),Is.EqualTo(3));
            Assert.That(restored.cells.Count(c=>c.frozen),Is.EqualTo(1));
            Assert.That(restored.cells.Count(c=>c.movable),Is.EqualTo(1));
            Assert.That(board.GetMinedCellCountForOwner(owner.GetInstanceID()),Is.EqualTo(1));
            Assert.That(board.ActiveRoyalBannerIds().Count,Is.EqualTo(1));
            // Dead owners cannot return their chains/mines. Walls and standards orphan.
            owner.TryTakeDamage(1000000);yield return Until(()=>owner==null||owner.IsDefeated,"owner dies");yield return Stable();
            Assert.That(board.TryRestoreBoardMemory(saved,id=>null),Is.True);yield return null;yield return null;
            var orphaned=board.CaptureBoardMemory(id=>id);
            Assert.That(orphaned.cells.Any(c=>c.pinned||c.mined),Is.False);
            Assert.That(orphaned.cells.Any(c=>c.barricade&&c.barricadeOwner==0),Is.True);
            Assert.That(orphaned.cells.Any(c=>c.banner&&c.bannerOwner==0),Is.True);
            foreach(var cell in orphaned.cells)Assert.That(cell.hasGem||cell.barricade||cell.banner,Is.True,"no impossible empty playable cell");
            Assert.That(run.ExitTo("MainMenu"),Is.True);yield return null;
        }
        yield return new ExitPlayMode();
    }

    [UnityTest] public IEnumerator ReshufflesReplayAndInvalidMemoriesLeaveTheBoardUntouched()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);yield return new EnterPlayMode();
        using(AccountProgression.UseDisposableProfile(Profile()))
        using(CharacterSelectionSettings.UseTemporarySelection(CharacterSelectionSettings.GideonPlayerId))
        {
            SceneManager.LoadScene("Game");yield return Stable();var run=RunSession.Current;MakeDurableEnemies(run);
            var board=run.Board;var saved=board.CaptureBoardMemory(id=>id);
            typeof(BoardController).GetMethod("ForceReshuffle",Fields).Invoke(board,null);yield return Stable();
            string future=Colors(board);uint futureRandom=board.RefillRandomState;
            Assert.That(board.TryRestoreBoardMemory(saved,id=>null),Is.True);yield return null;
            typeof(BoardController).GetMethod("ForceReshuffle",Fields).Invoke(board,null);yield return Stable();
            Assert.That(Colors(board),Is.EqualTo(future));Assert.That(board.RefillRandomState,Is.EqualTo(futureRandom));
            string before=JsonUtility.ToJson(board.CaptureBoardMemory(id=>id));
            saved.cells[1].x=saved.cells[0].x;saved.cells[1].y=saved.cells[0].y;
            Assert.That(board.TryRestoreBoardMemory(saved,id=>null),Is.False);
            Assert.That(JsonUtility.ToJson(board.CaptureBoardMemory(id=>id)),Is.EqualTo(before));
            Assert.That(run.ExitTo("MainMenu"),Is.True);yield return null;
        }
        yield return new ExitPlayMode();
    }

    [UnityTest] public IEnumerator EnemyWarningsKeepTheirPresentDeadlinesAndResolveOnlyOnce()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);yield return new EnterPlayMode();
        using(AccountProgression.UseDisposableProfile(Profile()))
        using(CharacterSelectionSettings.UseTemporarySelection(CharacterSelectionSettings.GideonPlayerId))
        {
            SceneManager.LoadScene("Game");yield return Stable();var run=RunSession.Current;MakeDurableEnemies(run);
            var board=run.Board;var owner=run.Waves.ActiveEnemies[0];
            var photo=board.CaptureBoardMemory(id=>id);
            BoardController.GemPairThreat pair=null;
            BoardController.GemSetThreat set=null;
            BoardController.LaneThreat lane=null;
            board.TryQueueMarkGemPair(owner,2,value=>pair=value);
            board.TryQueueMarkGemSet(owner,2,3,false,value=>set=value,null);
            board.TryQueueMarkLanes(owner,4,value=>lane=value,null);
            yield return Stable();
            Assert.That(pair,Is.Not.Null);Assert.That(set,Is.Not.Null);Assert.That(lane,Is.Not.Null);
            int first=pair.First.BoardIdentity,second=pair.Second.BoardIdentity;
            var targets=set.Targets.Select(g=>g.BoardIdentity).ToArray();
            int pairDue=pair.DueMove,setDue=set.DueMove,laneDue=lane.DueMove;
            Assert.That(board.TryRestoreBoardMemory(photo,id=>owner),Is.True);yield return null;yield return null;
            Assert.That(pair.Ended,Is.False);Assert.That(pair.DueMove,Is.EqualTo(pairDue));
            Assert.That(pair.First.BoardIdentity,Is.EqualTo(first));Assert.That(pair.Second.BoardIdentity,Is.EqualTo(second));
            CollectionAssert.AreEqual(targets,set.Targets.Select(g=>g.BoardIdentity));
            Assert.That(set.DueMove,Is.EqualTo(setDue));Assert.That(lane.DueMove,Is.EqualTo(laneDue));
            Assert.That(board.CompletedValidPlayerMoves,Is.Zero);
            int impacts=0;board.GemPairImpact+=(_,a,b,duration)=>impacts++;
            // The enemy owns this deadline. Advance its fixture to the real resolver boundary.
            typeof(BoardController).GetField("completedValidPlayerMoves",Fields).SetValue(board,pairDue);
            Assert.That(board.TryQueueResolveGemPair(owner,pair,0,null),Is.True);
            yield return Stable();
            Assert.That(impacts,Is.EqualTo(1));Assert.That(pair.Ended,Is.True);
            Assert.That(board.TryRestoreBoardMemory(photo,id=>owner),Is.True);yield return null;yield return null;
            Assert.That(board.TryQueueResolveGemPair(owner,pair,0,null),Is.False);
            Assert.That(impacts,Is.EqualTo(1),"a consumed warning is not resurrected by the photo");
            Assert.That(run.ExitTo("MainMenu"),Is.True);yield return null;
        }
        yield return new ExitPlayMode();
    }

    [UnityTest] public IEnumerator PauseDeathAndEncounterCompletionCannotLeakOrReviveAPhotograph()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);yield return new EnterPlayMode();
        for(int scenario=0;scenario<3;scenario++)
        using(AccountProgression.UseDisposableProfile(Profile()))
        using(CharacterSelectionSettings.UseTemporarySelection(CharacterSelectionSettings.GideonPlayerId))
        {
            SceneManager.LoadScene("Game");yield return Stable();var run=RunSession.Current;MakeDurableEnemies(run);
            run.Player.GetComponent<PlayerAbilityEnergy>().AddEnergy(100);
            Assert.That(run.Player.GetComponent<PlayerAbilityController>().TryActivate(),Is.True);
            var ability=run.Player.GetComponent<ChronoShutterRuntime>();
            Time.timeScale=0;float remaining=ability.PhaseRemaining;
            yield return new WaitForSecondsRealtime(.12f);
            Assert.That(ability.PhaseRemaining,Is.EqualTo(remaining));Assert.That(ability.RemainingMoves,Is.EqualTo(5));
            Time.timeScale=1;yield return Until(()=>ability.Phase==BoardMemoryPhase.Holding,"hold resumes");
            var unrelatedInputBlock=run.Board.AcquireExternalInputBlock();
            int rewinds=0;ability.Rewound+=()=>rewinds++;
            if(scenario==0)
            {
                run.Player.TryTakeDamage(100000);
                Assert.That(run.Player.IsDefeated,Is.True);
            }
            else if(scenario==1)
            {
                foreach(var enemy in run.Waves.ActiveEnemies.ToArray())enemy.TryTakeDamage(1000000);
                yield return Until(()=>!ability.IsActive,"completed encounter cancels photograph");
            }
            else run.Player.GetComponent<PlayerAbilityController>().CancelActiveAbility();
            Assert.That(ability.IsActive,Is.False);Assert.That(ability.CaptureContinuation(),Is.Null);Assert.That(rewinds,Is.Zero);
            Assert.That(run.Board.IsExternalInputBlocked,Is.True,"ability cleanup preserves another owner's input gate");
            unrelatedInputBlock.Dispose();
            Assert.That(run.Board.IsExternalInputBlocked,Is.False,"ability releases its own input gate");
            if(!run.IsFinished)run.ExitTo("MainMenu");else SceneManager.LoadScene("MainMenu");yield return null;
        }
        yield return new ExitPlayMode();
    }

    [UnityTest] public IEnumerator CharacterCardsExtendThePhotoAndGrantShieldExactlyOnceAcrossResume()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);yield return new EnterPlayMode();
        SceneManager.sceneLoaded+=InstallDurableFixture;
        string path=Profile();
        using(AccountProgression.UseDisposableProfile(path))
        using(CharacterSelectionSettings.UseTemporarySelection(CharacterSelectionSettings.GideonPlayerId))
        {
            SceneManager.LoadScene("Game");yield return Stable();var run=RunSession.Current;
            var runtime=RunUpgradeRuntime.Current;
            var exposure=runtime.Catalog.Upgrades.Single(c=>c.UpgradeId=="long_exposure");
            var fluid=runtime.Catalog.Upgrades.Single(c=>c.UpgradeId=="developing_fluid");
            foreach(var card in new[]{exposure,fluid})
            {
                Assert.That(card.Rarity,Is.EqualTo(RunUpgradeRarity.Epic));
                Assert.That(card.Theme,Is.EqualTo(RunUpgradeTheme.Ability));
                Assert.That(card.RequiredPlayerId,Is.EqualTo("gideon_glass"));
                Assert.That(card.RequiredAbilityId,Is.EqualTo("chronoshutter"));
                Assert.That(runtime.TryApply(card,1),Is.True);
                Assert.That(runtime.TryApply(card,1),Is.False,"one stack per run");
            }
            Assert.That(RunUpgradeResolver.ResolveBoardMemoryMoves(5),Is.EqualTo(6));
            Assert.That(RunUpgradeResolver.ResolveBoardMemoryRewindShield(),Is.EqualTo(12));
            run.Player.GetComponent<PlayerAbilityEnergy>().AddEnergy(100);
            Assert.That(run.Player.GetComponent<PlayerAbilityController>().TryActivate(),Is.True);
            var ability=run.Player.GetComponent<ChronoShutterRuntime>();
            int rewinds=0,shieldEvents=0;
            ability.Rewound+=()=>rewinds++;
            run.Player.ShieldGranted+=(_,amount)=>{shieldEvents++;Assert.That(amount,Is.EqualTo(12));};
            yield return Until(()=>ability.Phase==BoardMemoryPhase.Holding,"extended photo begins");
            for(int i=0;i<5;i++)yield return HintTurn();
            Assert.That(ability.RemainingMoves,Is.EqualTo(1));Assert.That(rewinds,Is.Zero);Assert.That(shieldEvents,Is.Zero);
            yield return HintTurn();yield return Until(()=>rewinds==1,"sixth turn rewinds");
            Assert.That(shieldEvents,Is.EqualTo(1));Assert.That(run.Player.CurrentShield,Is.EqualTo(12));
            Time.timeScale=0;Assert.That(run.Continuation.SaveNow(),Is.True);
            Assert.That(AccountProgression.Current.ActiveRun.checkpoint.boardMemory.phase,Is.EqualTo(BoardMemoryPhase.Rewinding));
            Assert.That(run.SuspendToMenu(),Is.True);yield return null;
            using(AccountProgression.UseDisposableProfile(path))
            {
                SceneManager.LoadScene("Game");yield return Restored();run=RunSession.Current;
                ability=run.Player.GetComponent<ChronoShutterRuntime>();
                int resumedShieldEvents=0;run.Player.ShieldGranted+=(_,amount)=>resumedShieldEvents++;
                run.GetComponent<RunControlsUI>().Close();yield return Until(()=>!ability.IsActive,"saved recovery finishes");
                Assert.That(run.Player.CurrentShield,Is.EqualTo(12));Assert.That(resumedShieldEvents,Is.Zero);
                Assert.That(RunUpgradeRuntime.Current.GetStackCount("long_exposure"),Is.EqualTo(1));
                Assert.That(RunUpgradeRuntime.Current.GetStackCount("developing_fluid"),Is.EqualTo(1));
                run.Player.GetComponent<PlayerAbilityEnergy>().AddEnergy(100);
                Assert.That(run.Player.GetComponent<PlayerAbilityController>().TryActivate(),Is.True);
                run.Player.GetComponent<PlayerAbilityController>().CancelActiveAbility();
                Assert.That(run.Player.CurrentShield,Is.EqualTo(12),"cancelled photo grants nothing");
                Assert.That(run.ExitTo("MainMenu"),Is.True);yield return null;
                using(CharacterSelectionSettings.UseTemporarySelection(CharacterSelectionSettings.RattlebonesPlayerId))
                {
                    SceneManager.LoadScene("Game");yield return Stable();run=RunSession.Current;
                    Assert.That(RunUpgradeRuntime.Current.IsEligible(exposure,run.Player,1),Is.False);
                    Assert.That(RunUpgradeRuntime.Current.IsEligible(fluid,run.Player,1),Is.False);
                    Assert.That(run.Player.GetComponent<ChronoShutterRuntime>(),Is.Null);
                    Assert.That(run.Continuation.Capture().boardMemory,Is.Null,"photograph cannot leak into another character/run");
                    Assert.That(RunUpgradeResolver.ResolveBoardMemoryMoves(5),Is.EqualTo(5));
                    Assert.That(RunUpgradeResolver.ResolveBoardMemoryRewindShield(),Is.Zero);
                    Assert.That(run.ExitTo("MainMenu"),Is.True);yield return null;
                }
            }
        }
        SceneManager.sceneLoaded-=InstallDurableFixture;yield return new ExitPlayMode();
    }

    // A compact input value for testing real board swaps; no replay model here.
    private readonly struct Vector4Int
    {
        public readonly int x,y,z,w;
        public Vector4Int(int x,int y,int z,int w) {this.x=x;this.y=y;this.z=z;this.w=w;}
    }
    private static void MakeDurableEnemies(RunSession run)
    {
        foreach (var enemy in run.Waves.ActiveEnemies)
        {
            enemy.GetComponent<EnemyAutoAttack>().StopAttacking();
            typeof(EnemyActor).GetField("currentHealth",Fields).SetValue(enemy,100000);
        }
    }
    private static void InstallDurableFixture(Scene scene,LoadSceneMode mode)
    {
        if(scene.name!="Game")return;
        UnityEngine.Object.FindFirstObjectByType<WaveController>().EnemySpawned+=enemy=>
        {
            var stats=enemy.RuntimeStats;
            typeof(EnemyActor).GetProperty("RuntimeStats").SetValue(enemy,new EnemyRuntimeStats(stats.Wave,stats.Level,100000,0,0,1000,1000));
            typeof(EnemyActor).GetField("currentHealth",Fields).SetValue(enemy,100000);
            enemy.GetComponent<EnemyAutoAttack>().StopAttacking();
        };
    }
    private static IEnumerator HintTurn()
    {
        var board=RunSession.Current.Board;int before=board.CompletedValidPlayerMoves;
        Assert.That(board.TryGetRandomHintMove(out var a,out var b),Is.True);
        board.ReplayPlayerSwap(a.Column,a.Row,b.Column,b.Row);
        yield return Until(()=>board.CompletedValidPlayerMoves==before+1&&!board.IsBusy,"hint move completes");
    }
    private static IEnumerator Restored()=>Until(()=>RunSession.Current!=null&&!RunSession.Current.Continuation.IsRestoring,"run restore completes");
    private static string Colors(BoardController board) => Colors(board.CaptureBoardMemory(id=>id));
    private static string Colors(BoardCombatSnapshot snapshot) => string.Join(",", snapshot.cells.Select(c => c.hasGem ? $"{(int)c.type}:{(int)c.special}" : "-"));
    private static string Profile()
    {
        string path = Path.GetFullPath(".utmp/GideonProfiles/" + Guid.NewGuid().ToString("N") + ".json");
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path,JsonUtility.ToJson(new AccountSave {gold=40,potions=2,bombs=2,equipBombs=true}));
        return path;
    }
    private static IEnumerator Stable() => Until(() => RunSession.Current != null &&
        RunSession.Current.Continuation.CanCapture && RunSession.Current.Waves.IsWaveActive,"combat settles");
    private static IEnumerator Until(Func<bool> condition,string label)
    {
        float end=Time.realtimeSinceStartup+60;
        while (!condition()) { Assert.That(Time.realtimeSinceStartup,Is.LessThan(end),label);yield return null; }
    }
    [UnityTearDown] public IEnumerator Cleanup()
    {
        SceneManager.sceneLoaded-=InstallDurableFixture;
        Time.timeScale=1;
        if (EditorApplication.isPlaying)
        { SceneManager.LoadScene("MainMenu");yield return null;yield return new ExitPlayMode(); }
    }
}
