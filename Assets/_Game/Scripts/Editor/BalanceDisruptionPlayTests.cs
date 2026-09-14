using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed class BalanceDisruptionPlayTests
{
    private const BindingFlags Flags=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
    private readonly List<EnemyActor> owners=new List<EnemyActor>();
    private BoardController board;
    [UnityTest] public IEnumerator ConcurrentOwnersRespectCapsResponsesAndDeathCleanup()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        yield return new EnterPlayMode();
        string path=Path.GetFullPath(".utmp/DisruptionProfiles/"+Guid.NewGuid().ToString("N")+".json");Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path,JsonUtility.ToJson(new AccountSave{bombs=10,equipBombs=true}));
        using(AccountProgression.UseDisposableProfile(path))
        using(CharacterSelectionSettings.UseTemporarySelection("skeleton"))
        using(GemMasterySettings.UseTemporaryLoadout(GemMasteryLoadout.Default))
        {
            SceneManager.LoadScene("Game");yield return Until(()=>RunSession.Current!=null&&RunSession.Current.Waves.IsWaveActive,"scene ready");
            board=RunSession.Current.Board;yield return Settle();
            var first=Owner<MinerEnemyAbility>("Miner");var second=Owner<MinerEnemyAbility>("Miner");
            // Queue several owners in the same frame while one mutation owns the board.
            for(int attempt=0;attempt<3;attempt++)
            {board.TryQueueMineRandomCell(first,3);board.TryQueueMineRandomCell(second,3);}
            yield return Settle();
            Assert.That(Count("minedCellOwners"),Is.EqualTo(4));
            Assert.That(board.GetMinedCellCountForOwner(first.GetInstanceID()),Is.LessThanOrEqualTo(3));
            Assert.That(board.GetMinedCellCountForOwner(second.GetInstanceID()),Is.LessThanOrEqualTo(3));
            AssertResponse();
            first.TryTakeDamage(10000);second.TryTakeDamage(10000);yield return Settle();
            Assert.That(Count("minedCellOwners"),Is.Zero,"miner death restores its cells through the queue");
            DestroyOwners();

            var miner=Owner<MinerEnemyAbility>("Miner");var wallA=Owner<BarricadeEnemyAbility>("BarricadeGuard");var wallB=Owner<BarricadeEnemyAbility>("BarricadeGuard");
            for(int attempt=0;attempt<3;attempt++)board.TryQueueMineRandomCell(miner,3);
            Assert.That(board.TryQueuePlaceBarricades(wallA,4,4,2,EnemyBarricadeStyle.Stone),Is.True);
            Assert.That(board.TryQueuePlaceBarricades(wallB,4,4,2,EnemyBarricadeStyle.Stone),Is.True);
            yield return Settle();
            Assert.That(Count("minedCellOwners")+Count("barricadeCells"),Is.EqualTo(10),"shared cap rechecked when each queued owner executes");
            AssertResponse();int walls=Count("barricadeCells");
            miner.TryTakeDamage(10000);wallA.TryTakeDamage(10000);wallB.TryTakeDamage(10000);yield return Settle();
            Assert.That(Count("minedCellOwners"),Is.Zero);
            Assert.That(Count("barricadeCells"),Is.EqualTo(walls),"orphaned barricades deliberately persist");
            Assert.That(board.GetBarricadeCountForOwner(wallA.GetInstanceID())+board.GetBarricadeCountForOwner(wallB.GetInstanceID()),Is.Zero);
            AssertResponse();DestroyOwners();

            // Start another actual scene so persistent barricades do not contaminate chain cases.
            string previousRun=RunSession.Current.RunId;
            Assert.That(RunSession.Current.ExitTo("Game"),Is.True);
            yield return Until(()=>RunSession.Current!=null&&RunSession.Current.RunId!=previousRun&&RunSession.Current.Waves.IsWaveActive,"second scene ready");
            board=RunSession.Current.Board;yield return Settle();
            var crossbowA=Owner<CrossbowGuardEnemyAbility>("CrossbowGuard");var crossbowB=Owner<CrossbowGuardEnemyAbility>("CrossbowGuard");var captain=Owner<KnightCaptainEnemyAbility>("KnightCaptain");
            Assert.That(crossbowA.Definition.ChainCap,Is.EqualTo(2));Assert.That(captain.Definition.ChainCap,Is.EqualTo(3));
            board.TryQueueTopUpMovablePins(crossbowA,crossbowA.Definition.ChainCap,null,null);board.TryQueueTopUpMovablePins(crossbowB,crossbowB.Definition.ChainCap,null,null);board.TryQueueTopUpMovablePins(captain,captain.Definition.ChainCap,null,null);
            yield return Settle();Assert.That(Count("pinnedGemOwners"),Is.EqualTo(6));AssertResponse();
            var pins=(IDictionary)Get(board,"pinnedGemOwners");
            var movable=(HashSet<Gem>)Get(board,"movablePinnedGems");
            Gem pinned=null;
            foreach(DictionaryEntry entry in pins)
            {
                var gem=(Gem)entry.Key;pinned=gem;
                Assert.That(movable.Contains(gem),Is.True,"Crossbow and Captain share falling chains");
                Assert.That(board.IsGemFrozen(gem),Is.False,"chains do not become Mage freezes");
                foreach(var delta in new[]{Vector2Int.up,Vector2Int.down,Vector2Int.left,Vector2Int.right})
                {
                    var neighbour=(Gem)Call(board,"GetGem",gem.Column+delta.x,gem.Row+delta.y);
                    if(neighbour!=null)Assert.That(board.IsHintMoveStillValid(gem,neighbour),Is.False,"manual chain swap rejected");
                }
            }
            Assert.That(RunSession.Current.TryUseBomb(pinned),Is.True);yield return Settle();
            Assert.That(pinned==null||!board.IsGemPinned(pinned),Is.True,"consumable uses normal chain breaking");
            foreach(var owner in owners)owner.TryTakeDamage(10000);yield return Settle();
            Assert.That(Count("pinnedGemOwners"),Is.Zero,"owner death releases remaining chains");AssertResponse();DestroyOwners();
            Assert.That(RunSession.Current.ExitTo("MainMenu"),Is.True);yield return null;
        }
        File.WriteAllText(".utmp/balance-disruption-play.txt","PASS: two Miners; queued Miner + two Barricade Guards; global/per-owner caps; legal responses after each formation; normal mine cleanup and persistent orphaned walls; two Crossbows + Captain share movable chains; manual chain swaps rejected; Bomb breaking and death cleanup. Real Game scenes and authoritative mutation coroutines.\n");
        yield return new ExitPlayMode();
    }
    [UnityTest] public IEnumerator RoyalMechanicsResolveWithEscortsInActualPlayMode()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);yield return new EnterPlayMode();
        string path=Path.GetFullPath(".utmp/DisruptionProfiles/"+Guid.NewGuid().ToString("N")+".json");
        using(AccountProgression.UseDisposableProfile(path))
        using(CharacterSelectionSettings.UseTemporarySelection("skeleton"))
        using(GemMasterySettings.UseTemporaryLoadout(GemMasteryLoadout.Default))
        {
            SceneManager.LoadScene("Game");yield return Until(()=>RunSession.Current!=null&&RunSession.Current.Waves.IsWaveActive,"royal scene ready");
            yield return (IEnumerator)typeof(RoyalMilestonePlayValidation).GetMethod("Scenarios",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,null);
            Assert.That(RunSession.Current.ExitTo("MainMenu"),Is.True);yield return null;
        }
        File.WriteAllText(".utmp/balance-royal-play.txt","PASS: existing Royal production-coroutine scenarios: simultaneous King/Archbishop marks, three real swaps, Standard/Mage bombardment, blessed full Lancer command, owner death cancellation and limited reinforcements. Deliberate high-HP scenario fixtures, not pacing measurements.\n");
        yield return new ExitPlayMode();
    }
    private EnemyActor Owner<T>(string name) where T:MonoBehaviour,IEnemySpecialAbilityRuntime
    {
        var definition=AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Game/Data/Enemies/Enemy_"+name+".asset");
        var go=UnityEngine.Object.Instantiate(definition.EnemyPrefab);var actor=go.GetComponent<EnemyActor>();
        actor.Initialize(definition,new EnemyRuntimeStats(10,1,1000,0,0,10,1000),GemType.Ruby);owners.Add(actor);
        (go.GetComponent<T>()??go.AddComponent<T>()).InitializeSpecialAbility(actor,board,owners);return actor;
    }
    private void DestroyOwners(){foreach(var owner in owners)if(owner!=null)UnityEngine.Object.Destroy(owner.gameObject);owners.Clear();}
    private void AssertResponse(){Assert.That(board.TryGetRandomHintMove(out _,out _),Is.True,"at least one legal response remains");}
    private int Count(string name)=>((IDictionary)Get(board,name)).Count;
    private IEnumerator Settle()=>Until(()=>!board.IsBusy&&!board.HasPendingBoardMutation,"board settles");
    private static IEnumerator Until(Func<bool> ready,string label)
    {float end=Time.realtimeSinceStartup+35;while(!ready()){Assert.That(Time.realtimeSinceStartup,Is.LessThan(end),label);yield return null;}}
    private static object Get(object target,string name)=>target.GetType().GetField(name,Flags).GetValue(target);
    private static object Call(object target,string name,params object[] args)=>target.GetType().GetMethod(name,Flags).Invoke(target,args);
}
