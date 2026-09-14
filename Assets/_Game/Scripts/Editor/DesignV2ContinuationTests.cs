using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed class DesignV2ContinuationTests
{
    [UnityTest] public IEnumerator ContinuePreservesCombatAndDoesNotPayOrRefill()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        yield return new EnterPlayMode();
        string path=Profile();
        using(AccountProgression.UseDisposableProfile(path))
        using(CharacterSelectionSettings.UseTemporarySelection("bardley"))
        {
            SceneManager.LoadScene("Game"); yield return Stable();
            var run=RunSession.Current;
            Assert.That(run.Player.GetComponent<PlayerAbilityEnergy>().CurrentEnergy,Is.EqualTo(16),"new Bardley receives the opening charge after initialization");
            run.Player.TryTakeDamage(17); run.Player.GrantShield(11);
            run.Player.GetComponent<PlayerAbilityEnergy>().AddEnergy(19);
            var enemy=run.Waves.ActiveEnemies[0];
            (enemy.GetComponent<EnemyPoisonStatus>() ?? enemy.gameObject.AddComponent<EnemyPoisonStatus>()).Apply(7,2,1);
            enemy.GetComponent<EnemyStagger>().ApplyStagger(1.5f,1.5f);
            Time.timeScale=0;
            Assert.That(run.Continuation.SaveNow(),Is.True);
            string id=run.RunId;
            var saved=AccountProgression.Current.ActiveRun.checkpoint;
            string board=JsonUtility.ToJson(saved.board);
            int gold=AccountProgression.Current.Gold;
            Assert.That(run.SuspendToMenu(),Is.True); yield return null;
            using(AccountProgression.UseDisposableProfile(path))
            {
                Assert.That(AccountProgression.Current.ActiveRun.id,Is.EqualTo(id));
                SceneManager.LoadScene("Game"); yield return Restored();
                run=RunSession.Current;
                Assert.That(run.RunId,Is.EqualTo(id));
                Assert.That(run.Player.CurrentHealth,Is.EqualTo(saved.player.health));
                Assert.That(run.Player.CurrentShield,Is.EqualTo(saved.player.shield));
                Assert.That(run.Player.GetComponent<PlayerAbilityEnergy>().CurrentEnergy,Is.EqualTo(saved.player.energy));
                Assert.That(JsonUtility.ToJson(run.Board.CaptureContinuation(run.Waves.ContinuationOwnerSlot)),Is.EqualTo(board));
                enemy=run.Waves.ActiveEnemies[0];
                Assert.That(enemy.CurrentHealth,Is.EqualTo(saved.enemies[0].health));
                Assert.That(enemy.GetComponent<EnemyAutoAttack>().RemainingAttackTime,Is.EqualTo(saved.enemies[0].attackRemaining).Within(.04));
                Assert.That(enemy.GetComponent<EnemyPoisonStatus>().RemainingDuration,Is.EqualTo(saved.enemies[0].poisonRemaining).Within(.04));
                Assert.That(enemy.GetComponent<EnemyStagger>().RemainingStaggerTime,Is.EqualTo(saved.enemies[0].staggerRemaining).Within(.04));
                Assert.That(run.Charges(ConsumableKind.Bomb),Is.EqualTo(2));
                Assert.That(AccountProgression.Current.Gold,Is.EqualTo(gold));
                run.GetComponent<RunControlsUI>().Close();
                Assert.That(run.ExitTo("MainMenu"),Is.True); yield return null;
            }
        }
        Time.timeScale=1; yield return new ExitPlayMode();
    }

    [UnityTest] public IEnumerator AcceptedBombAndBardleyCastSurviveRestartExactlyOnce()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        yield return new EnterPlayMode();
        for(int scenario=0;scenario<2;scenario++)
        {
        string path=Profile();
        using(AccountProgression.UseDisposableProfile(path))
        using(CharacterSelectionSettings.UseTemporarySelection("bardley"))
        {
            SceneManager.LoadScene("Game"); yield return Stable();
            var run=RunSession.Current;
            foreach(var enemy in run.Waves.ActiveEnemies) enemy.GetComponent<EnemyAutoAttack>().StopAttacking();
            Assert.That(run.Continuation.SaveNow(),Is.True);
            var weakness=run.Waves.ActiveEnemies[0].AssignedGemType;
            var corner=new[]{new Vector2Int(0,0),new Vector2Int(6,0),new Vector2Int(0,7),new Vector2Int(6,7)}
                .OrderBy(c=>Enumerable.Range(Math.Max(0,c.x-1),2).Sum(x=>Enumerable.Range(Math.Max(0,c.y-1),2).Count(y=>run.Board.GetGem(x,y)?.Type==weakness))).First();
            if(scenario==0) Assert.That(run.TryUseBomb(run.Board.GetGem(corner.x,corner.y)),Is.True);
            else
            {
                run.Player.GetComponent<PlayerAbilityEnergy>().AddEnergy(100);
                Assert.That(run.Player.GetComponent<PlayerAbilityController>().TryActivate(),Is.True);
                yield return new WaitForSeconds(.25f);
            }
            Assert.That(run.Board.IsBusy,Is.True);
            Assert.That(run.Continuation.SaveNow(),Is.True,"durable in-flight action");
            string pending=File.ReadAllText(path);
            int remainingBombs=scenario==0?1:2;
            Assert.That(AccountProgression.Current.Owned(ConsumableKind.Bomb),Is.EqualTo(remainingBombs));
            yield return Stable();
            var expected=run.Continuation.Capture();
            string colors=Colors(run.Board);
            SceneManager.LoadScene("MainMenu"); yield return null;
            File.WriteAllText(path,pending);
            using(AccountProgression.UseDisposableProfile(path))
            {
                SceneManager.LoadScene("Game"); yield return Restored();
                run=RunSession.Current; run.GetComponent<RunControlsUI>().Close();
                yield return Stable();
                Assert.That(Colors(run.Board),Is.EqualTo(colors),"same saved random stream and clear pipeline");
                Assert.That(run.Player.GetComponent<PlayerAbilityEnergy>().CurrentEnergy,Is.EqualTo(expected.player.energy));
                Assert.That(run.Charges(ConsumableKind.Bomb),Is.EqualTo(remainingBombs));
                Assert.That(AccountProgression.Current.Owned(ConsumableKind.Bomb),Is.EqualTo(remainingBombs));
                Assert.That(run.Continuation.SaveNow(),Is.True);
                Assert.That(run.SuspendToMenu(),Is.True); yield return null;
                SceneManager.LoadScene("Game"); yield return Restored();
                run=RunSession.Current;
                Assert.That(Colors(run.Board),Is.EqualTo(colors),"repeat continue cannot repeat the accepted action");
                Assert.That(AccountProgression.Current.Owned(ConsumableKind.Bomb),Is.EqualTo(remainingBombs));
                run.GetComponent<RunControlsUI>().Close(); run.ExitTo("MainMenu"); yield return null;
            }
        }
        }
        Time.timeScale=1; yield return new ExitPlayMode();
    }
    [UnityTest] public IEnumerator DraftRefinementAndSelectedBuildSurviveRepeatedContinue()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        yield return new EnterPlayMode();
        string path=Profile();
        using(AccountProgression.UseDisposableProfile(path))
        using(CharacterSelectionSettings.UseTemporarySelection("bardley"))
        {
            SceneManager.LoadScene("Game"); yield return Stable();
            for(int wave=1;wave<=2;wave++)
            {
                int wanted=wave;
                yield return Until(()=>RunSession.Current.Waves.CurrentWave==wanted && RunSession.Current.Continuation.CanCapture,"wave spawns");
                foreach(var enemy in RunSession.Current.Waves.ActiveEnemies.ToArray()) enemy.ResolveDirectDamage(99999);
            }
            yield return Until(()=>UnityEngine.Object.FindFirstObjectByType<UpgradeChoiceUI>()?.IsOpen==true,"first real card choice");
            var run=RunSession.Current;
            Assert.That(run.Continuation.SaveNow(),Is.True);
            var draft=AccountProgression.Current.ActiveRun.checkpoint.draft.ToArray();
            Assert.That(draft.Length,Is.EqualTo(3));
            Assert.That(run.SuspendToMenu(),Is.True); yield return null;
            using(AccountProgression.UseDisposableProfile(path))
            {
                SceneManager.LoadScene("Game"); yield return Restored();
                run=RunSession.Current; run.GetComponent<RunControlsUI>().Close();
                var coordinator=run.Waves.GetComponent<RunUpgradeCoordinator>();
                var current=run.Continuation.Capture();
                Assert.That(current.draft,Is.EqualTo(draft));
                Assert.That(coordinator.TryRefine(RunUpgradeTheme.Ability),Is.True);
                Assert.That(run.Continuation.SaveNow(),Is.True);
                string chosen=AccountProgression.Current.ActiveRun.checkpoint.draft[0];
                Assert.That(run.SuspendToMenu(),Is.True); yield return null;
                SceneManager.LoadScene("Game"); yield return Restored();
                run=RunSession.Current; run.GetComponent<RunControlsUI>().Close();
                coordinator=run.Waves.GetComponent<RunUpgradeCoordinator>();
                Assert.That(coordinator.CanRefine,Is.False);
                Assert.That(coordinator.TryRefine(RunUpgradeTheme.Board),Is.False);
                Assert.That(coordinator.SelectRecordedCard(chosen),Is.True);
                Assert.That(coordinator.SelectRecordedCard(chosen),Is.False);
                yield return Until(()=>run.Waves.CurrentWave==3 && run.Continuation.CanCapture,"chosen build continues");
                Assert.That(RunUpgradeRuntime.Current.GetStackCount(chosen),Is.EqualTo(1));
                Assert.That(run.Continuation.SaveNow(),Is.True);
                Assert.That(run.SuspendToMenu(),Is.True); yield return null;
                SceneManager.LoadScene("Game"); yield return Restored();
                run=RunSession.Current;
                Assert.That(RunUpgradeRuntime.Current.GetStackCount(chosen),Is.EqualTo(1));
                Assert.That(AccountProgression.Current.ActiveRun.refinementUsed,Is.True);
                Assert.That(AccountProgression.Current.ActiveRun.draft,Is.Empty);
                run.GetComponent<RunControlsUI>().Close(); run.ExitTo("MainMenu"); yield return null;
            }
        }
        Time.timeScale=1; yield return new ExitPlayMode();
    }

    [Test] public void AcceptedItemJournalIsAtomicAndFailedRecoveryPreservesTheOriginal()
    {
        string path=Profile();
        using(AccountProgression.UseDisposableProfile(path))
        {
            var account=AccountProgression.Current;
            string id=account.BeginRun("bardley");
            var snapshot=new RunCombatSnapshot {potionCharges=2,bombCharges=2};
            Assert.That(account.StoreCheckpoint(id,snapshot),Is.True);
            var tape=new RunReplayTape();
            tape.frames.Add(new RunReplayFrame());
            tape.frames[0].actions.Add(new RunRecordedAction {kind=RunActionKind.Bomb});
            string original=File.ReadAllText(path);
            using(var locked=new FileStream(path+".tmp",FileMode.Create,FileAccess.ReadWrite,FileShare.None))
            {
                LogAssert.Expect(LogType.Error,new System.Text.RegularExpressions.Regex("Could not save account"));
                Assert.That(account.AcceptRecordedAction(id,tape,ConsumableKind.Bomb),Is.False);
            }
            Assert.That(File.ReadAllText(path),Is.EqualTo(original));
            Assert.That(account.Owned(ConsumableKind.Bomb),Is.EqualTo(2));
            Assert.That(account.AcceptRecordedAction(id,tape,ConsumableKind.Bomb),Is.True);
            string accepted=File.ReadAllText(path);
            account.BeginContinuationReplay(snapshot);
            Assert.That(account.TrySpendCharge(id,ConsumableKind.Bomb),Is.True);
            Assert.That(account.EndContinuationReplay(false),Is.True);
            Assert.That(File.ReadAllText(path),Is.EqualTo(accepted));
            Assert.That(account.Owned(ConsumableKind.Bomb),Is.EqualTo(1));
        }
    }

    [UnityTest] public IEnumerator OwnedObstaclesWarningsAndDecreeRestoreThroughTheirRealOwners()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        yield return new EnterPlayMode();
        string[][] cases={new[]{"Miner","CrossbowGuard"},new[]{"BarricadeGuard","CourtMage"},new[]{"RoyalStandardBearer","RoyalArchbishop"}};
        foreach(var pair in cases)
        {
            string path=Profile();
            using(AccountProgression.UseDisposableProfile(path))
            using(CharacterSelectionSettings.UseTemporarySelection("skeleton"))
            {
                SceneManager.LoadScene("Game");yield return Stable();var run=RunSession.Current;
                Assert.That(run.Player.GetComponent<PlayerAbilityEnergy>().CurrentEnergy,Is.EqualTo(20));
                foreach(string name in pair)
                {
                    var data=AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Game/Data/Enemies/Enemy_"+name+".asset");
                    Assert.That(data,Is.Not.Null,name);
                    Assert.That(run.Waves.TrySummonEnemy(data,out var actor),Is.True);
                    yield return Stable();
                    Time.timeScale=0;
                    string pausedBoard=JsonUtility.ToJson(run.Board.CaptureContinuation(run.Waves.ContinuationOwnerSlot));
                    int turns=actor.SpecialTurnRequirement;
                    for(int i=0;i<turns;i++)actor.RegisterValidPlayerTurn();
                    yield return new WaitForSecondsRealtime(.12f);
                    Assert.That(actor.IsSpecialReady,Is.True,"ready enemy waits during pause");
                    Assert.That(JsonUtility.ToJson(run.Board.CaptureContinuation(run.Waves.ContinuationOwnerSlot)),Is.EqualTo(pausedBoard));
                    Time.timeScale=1;
                    yield return Until(()=>!actor.IsSpecialReady&&!actor.HasAnimationActionInProgress&&!run.Board.IsBusy,"special commits and settles");
                }
                yield return Stable();
                foreach(var enemy in run.Waves.ActiveEnemies) enemy.GetComponent<EnemyAutoAttack>().StopAttacking();
                run.Player.GetComponent<PlayerAbilityEnergy>().AddEnergy(100);
                Assert.That(run.Player.GetComponent<PlayerAbilityController>().TryActivate(),Is.True);
                Time.timeScale=0;Assert.That(run.Continuation.SaveNow(),Is.True);
                var saved=AccountProgression.Current.ActiveRun.checkpoint;
                Assert.That(saved.decreeRemaining,Is.GreaterThan(0));
                Assert.That(saved.board.cells.Any(c=>c.mined||c.pinned||c.barricade||c.banner),Is.True,pair[0]);
                if(pair[1]=="RoyalArchbishop") Assert.That(saved.board.warnings.Count,Is.GreaterThan(0));
                string board=JsonUtility.ToJson(saved.board);
                Assert.That(run.SuspendToMenu(),Is.True);yield return null;
                using(AccountProgression.UseDisposableProfile(path))
                {
                    SceneManager.LoadScene("Game");yield return Restored();run=RunSession.Current;
                    Assert.That(JsonUtility.ToJson(run.Board.CaptureContinuation(run.Waves.ContinuationOwnerSlot)),Is.EqualTo(board));
                    var decree=run.Player.GetComponent<RoyalDecreeRuntime>();
                    Assert.That(decree.IsActive,Is.True);Assert.That(decree.RemainingDuration,Is.EqualTo(saved.decreeRemaining).Within(.04));
                    Assert.That(decree.CurrentTarget,Is.EqualTo(run.Waves.ContinuationEnemy(saved.decreeTarget)));
                    foreach(var value in saved.enemies)
                    {
                        var enemy=run.Waves.ContinuationEnemy(value.slot);
                        Assert.That(enemy.Definition.name,Is.EqualTo(value.definition));
                        Assert.That(enemy.CurrentHealth,Is.EqualTo(value.health));
                    }
                    if(pair[1]=="RoyalArchbishop")
                    {
                        var bishop=run.Waves.ActiveEnemies.First(e=>e.Definition.name=="Enemy_RoyalArchbishop");
                        Assert.That(run.Board.RestoredSet(bishop),Is.Not.Null,"warning rebinds to restored owner");
                    }
                    run.GetComponent<RunControlsUI>().Close();run.ExitTo("MainMenu");yield return null;
                }
            }
        }
        Time.timeScale=1;yield return new ExitPlayMode();
    }

    private static string Colors(BoardController board)
    {
        var result=new System.Text.StringBuilder();
        for(int y=0;y<board.Height;y++) for(int x=0;x<board.Width;x++)
        { var gem=board.GetGem(x,y); result.Append(gem==null?"-":$"{(int)gem.Type}:{(int)gem.SpecialType}").Append(','); }
        return result.ToString();
    }
    private static string Profile()
    {
        var path=Path.GetFullPath(".utmp/ContinuationTests/"+Guid.NewGuid().ToString("N")+".json");
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path,JsonUtility.ToJson(new AccountSave {gold=40,potions=2,bombs=2,equipPotions=true,equipBombs=true}));
        return path;
    }
    private static IEnumerator Stable()=>Until(()=>RunSession.Current!=null && RunSession.Current.Continuation.CanCapture && RunSession.Current.Waves.IsWaveActive,"combat settles");
    private static IEnumerator Restored()=>Until(()=>RunSession.Current!=null && !RunSession.Current.Continuation.IsRestoring,"restore completes: "+RunSession.Current?.Continuation.Error);
    private static IEnumerator Until(Func<bool> condition,string label)
    { float end=Time.realtimeSinceStartup+45; while(!condition()) { Assert.That(Time.realtimeSinceStartup,Is.LessThan(end),label); yield return null; } }
}
