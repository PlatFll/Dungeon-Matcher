using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

// Runs against the real Game scene and production coroutines. Fixture changes
// are Play Mode only; exiting restores the scene and serialized combat tuning.
[InitializeOnLoad]
public static class RoyalMilestonePlayValidation
{
    private const string PendingKey="DungeonMatcher.RoyalPlayValidation";
    private const BindingFlags Flags=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
    private static readonly Stack<IEnumerator> steps=new Stack<IEnumerator>();
    private static BoardController board;
    private static WaveController waves;
    private static EnemyActor king,bishop;
    static RoyalMilestonePlayValidation() { EditorApplication.playModeStateChanged+=StateChanged; }
    [MenuItem("Dungeon Matcher/Validation/Royal Play Mode")]
    public static void Run()
    {
        if(EditorApplication.isPlaying) throw new InvalidOperationException("Start this suite outside Play Mode.");
        SessionState.SetBool(PendingKey,true); EditorApplication.EnterPlaymode();
    }
    private static void StateChanged(PlayModeStateChange state)
    {
        if(state==PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(PendingKey,false))
        {
            SessionState.SetBool(PendingKey,false); steps.Clear(); steps.Push(Scenarios());
            EditorApplication.update-=Tick; EditorApplication.update+=Tick;
        }
        if(state==PlayModeStateChange.ExitingPlayMode) { EditorApplication.update-=Tick; steps.Clear(); }
    }
    private static void Tick()
    {
        try
        {
            while(steps.Count>0)
            {
                var top=steps.Peek();
                if(!top.MoveNext()) { steps.Pop(); continue; }
                if(top.Current is IEnumerator nested) { steps.Push(nested); continue; }
                return;
            }
            Debug.Log("Royal Play Mode validation PASSED: real swaps, simultaneous warnings, banner/freeze bombardment, blessed Lancer command, death cancellation and reinforcement slots.");
            EditorApplication.update-=Tick; EditorApplication.ExitPlaymode();
        }
        catch(Exception exception)
        {
            Debug.LogException(exception); steps.Clear(); EditorApplication.update-=Tick;
            EditorApplication.ExitPlaymode();
        }
    }
    private static IEnumerator Scenarios()
    {
        yield return Delay(2);
        board=UnityEngine.Object.FindFirstObjectByType<BoardController>();
        waves=UnityEngine.Object.FindFirstObjectByType<WaveController>();
        Check(board!=null && waves!=null,"Game scene is loaded");
        var player=UnityEngine.Object.FindFirstObjectByType<PlayerActor>();
        Set(player,"maximumHealth",100000); Set(player,"currentHealth",100000);
        Set(waves,"advanceWavesAutomatically",false);

        yield return SpawnBoss("RoyalStandardBearer");
        EnemyActor bearer=Find("RoyalStandardBearer"); Prime(bearer,5);
        yield return Until(()=>board.GetRoyalBannerCountForOwner(bearer.GetInstanceID())==1,"standard placed");
        Prime(king,4); Prime(bishop,4);
        var kingAbility=king.GetComponent<KingEnemyAbility>();
        var bishopAbility=bishop.GetComponent<RoyalArchbishopEnemyAbility>();
        yield return Until(()=>Get(kingAbility,"judgment")!=null && Get(bishopAbility,"runes")!=null && !board.IsBusy,"simultaneous warnings");
        var judgment=(BoardController.GemSetThreat)Get(kingAbility,"judgment");
        var runes=(BoardController.GemSetThreat)Get(bishopAbility,"runes");
        Check(judgment.Targets.Count==3 && runes.Targets.Count==3,"three marks per owner");
        foreach(var gem in runes.Targets) Check(!judgment.Targets.Contains(gem),"readable non-overlap");
        int moveBefore=board.CompletedValidPlayerMoves;
        for(int i=0;i<3;i++) yield return PlayerMove();
        yield return Until(()=>judgment.Ended && runes.Ended && !board.IsBusy,"warnings resolve after moves");
        Check(board.CompletedValidPlayerMoves==moveBefore+3,"cascades and enemy clears add no player moves");
        Debug.Log("Royal Play Mode: King + Archbishop + Standard Bearer, simultaneous marks and three real completed moves passed.");

        yield return SpawnBoss("CourtMage");
        EnemyActor mage=Find("CourtMage"); Prime(mage,5);
        yield return Until(()=>board.GetFrozenGemCountForOwner(mage.GetInstanceID())==1 && !board.IsBusy,"freeze placed");
        Gem frozen=null;
        for(int y=0;y<board.Height;y++) for(int x=0;x<board.Width;x++)
            if(board.IsGemFrozen(GemAt(x,y))) frozen=GemAt(x,y);
        Check(frozen!=null,"frozen identity");
        Check(board.TryQueuePlaceRoyalBanner(king),"standard footprint fixture accepted");
        yield return Until(()=>board.GetRoyalBannerCountForOwner(king.GetInstanceID())==1 && !board.IsBusy,"standard footprint fixture placed");
        int standardColumn=0;
        foreach(DictionaryEntry entry in (IDictionary)Get(board,"royalBannerCells"))
            if((int)Get(entry.Value,"OwnerInstanceId")==king.GetInstanceID()) standardColumn=((Vector2Int)entry.Key).x;
        BoardController.LaneThreat lanes=null;
        Check(board.TryQueueMarkLanes(king,2,t=>lanes=t,()=>false),"lane warning accepted");
        yield return Until(()=>lanes!=null && !board.IsBusy,"lane warning installed");
        SetProperty(lanes,"Row",frozen.Row); SetProperty(lanes,"Column",standardColumn);
        // Preserve a real special during the direct environmental footprint.
        Gem special=GemAt((frozen.Column+1)%board.Width,frozen.Row);
        if(special!=null) special.SetSpecialType(GemSpecialType.RowBomb);
        int pulses=0;
        SetProperty(lanes,"DueMove",board.CompletedValidPlayerMoves);
        Check(board.TryQueueResolveLanes(lanes,()=>
        {
            pulses++;
            Check(board.GetRoyalBannerCountForOwner(king.GetInstanceID())==1,"bombardment preserves Standard before normal gravity");
        },success=>Check(success,"lane completion"),()=>false),"bombardment accepted");
        yield return Until(()=>lanes.Ended && !board.IsBusy,"bombardment settles");
        Check(frozen==null && board.GetFrozenGemCountForOwner(mage.GetInstanceID())==0,"freeze destruction cleanup");
        Check(pulses==1,"one bombardment direct impact");
        // Resulting genuine cascades may activate the preserved bomb; direct-clear
        // exclusion is independently covered by the editor suite.
        Debug.Log("Royal Play Mode: King + Archbishop + Court Mage and environmental frozen-gem bombardment passed.");

        yield return SpawnBoss("RoyalLancer");
        EnemyActor lancer=Find("RoyalLancer"); bishopAbility=bishop.GetComponent<RoyalArchbishopEnemyAbility>();
        Set(bishopAbility,"preferRunes",false); Prime(bishop,4);
        var lancerAttack=lancer.GetComponent<EnemyAutoAttack>(); var kingAttack=king.GetComponent<EnemyAutoAttack>();
        yield return Until(()=>lancerAttack.HasNextSequenceModifier(bishopAbility),"Lancer blessing");
        int lancerHits=0,kingHits=0;
        lancerAttack.AttackResolved+=(a,damage,applied)=>
        {
            int expected=Mathf.RoundToInt((lancerHits==0 ? lancer.Damage : lancer.FollowUpDamage)*1.4f*1.1f);
            Check(damage==expected,"both blessed commanded Lancer hits"); lancerHits++;
        };
        kingAttack.AttackResolved+=(a,damage,applied)=>kingHits++;
        kingAbility=king.GetComponent<KingEnemyAbility>(); Set(kingAbility,"cycle",1); Prime(king,4);
        yield return Until(()=>lancerHits==2 && kingHits==1 && !(bool)Get(kingAbility,"pending"),"coordinated full sequences");
        Check(!lancerAttack.HasNextSequenceModifier(bishopAbility) && lancerAttack.RemainingAttackTime>0,"blessing consumed and normal cooldown restarted");
        Set(kingAbility,"cycle",1); Prime(king,4);
        yield return Until(()=>(bool)Get(kingAbility,"pending"),"second command windup");
        king.TryTakeDamage(king.CurrentHealth+100);
        yield return Delay(1.5f); Check(lancerHits==2,"King death cancels remaining command");
        Debug.Log("Royal Play Mode: blessed Lancer two-hit Assault, cooldown consumption, and King-death cancellation passed.");

        for(int free=0;free<=2;free++)
        {
            yield return SpawnBoss(free==0 ? "RoyalLancer" : null);
            if(free==2) { bishop.TryTakeDamage(bishop.CurrentHealth+100); yield return Delay(1.5f); }
            int before=waves.ActiveEnemies.Count;
            king.TryTakeDamage(Mathf.RoundToInt(king.MaxHealth*0.6f));
            yield return Until(()=>king.GetComponent<KingEnemyAbility>().IsEnraged && waves.ActiveEnemies.Count==3,"50 percent free-slot batch");
            Check(waves.ActiveEnemies.Count-before==free,"50 percent fills exactly free slots");
            foreach(var enemy in new List<EnemyActor>(waves.ActiveEnemies)) if(enemy!=king) enemy.TryTakeDamage(enemy.CurrentHealth+100);
            yield return Delay(1.5f);
            king.TryTakeDamage(Mathf.RoundToInt(king.MaxHealth*0.2f));
            yield return Until(()=>waves.ActiveEnemies.Count==3,"25 percent refill after earlier allies die");
            foreach(var enemy in waves.ActiveEnemies) Check(enemy==king || enemy.Definition!=Data("RoyalArchbishop"),"never Archbishop reinforcement");
        }
        Debug.Log("Royal Play Mode: 50-percent batches with 0/1/2 slots and second reinforcement after deaths passed.");
    }
    private static IEnumerator SpawnBoss(string extra)
    {
        yield return Until(()=>!board.IsBusy,"board idle before fixture"); waves.ClearCurrentWave();
        yield return Delay(0.3f); yield return Until(()=>!board.IsBusy,"old owner cleanup");
        Set(waves,"currentWave",26);
        var seen=(HashSet<EnemyDefinition>)Get(waves,"seenMilestoneLeaders"); seen.Clear(); seen.Add(Data("RoyalArchbishop"));
        // Each scenario is an independent run fixture, not a repeated encounter.
        ((HashSet<EnemyDefinition>)Get(waves,"previousEncounterLeaders")).Clear();
        waves.SpawnCurrentWave();
        yield return Until(()=>waves.ActiveEnemies.Count==2 && !(bool)Get(waves,"isSpawningWave"),"King opening composition");
        king=Find("King"); bishop=Find("RoyalArchbishop"); Check(king!=null && bishop!=null,"King plus Archbishop");
        if(extra!=null) Check(waves.TrySummonEnemy(Data(extra),out _),"extra Royal participant");
        foreach(var actor in waves.ActiveEnemies)
        {
            var attack=actor.GetComponent<EnemyAutoAttack>(); Set(attack,"attackAutomatically",false); attack.StopAttacking();
            actor.SetSpecialTurnRequirement(10000); actor.ResetSpecialCounter();
            var stats=actor.RuntimeStats;
            SetProperty(actor,"RuntimeStats",new EnemyRuntimeStats(stats.Wave,stats.Level,50000,stats.Damage,stats.FollowUpDamage,
                stats.AttackInterval,stats.SpecialTurnRequirement,stats.DamageMultiplier));
            Set(actor,"currentHealth",50000);
        }
        yield return Until(()=>!board.IsBusy,"spawn settled");
    }
    private static IEnumerator PlayerMove()
    {
        yield return Until(()=>!board.IsBusy,"before real swap");
        Check(board.TryGetRandomHintMove(out Gem a,out Gem b),"legal player move available");
        int before=board.CompletedValidPlayerMoves;
        board.StartCoroutine((IEnumerator)Call(board,"TrySwap",a,b));
        yield return Until(()=>board.CompletedValidPlayerMoves==before+1 && !board.IsBusy,"real player swap completion");
    }
    private static void Prime(EnemyActor actor,int turns)
    { actor.SetSpecialTurnRequirement(turns); actor.ResetSpecialCounter(); for(int i=0;i<turns;i++) actor.RegisterValidPlayerTurn(); }
    private static IEnumerator Until(Func<bool> condition,string label)
    {
        double deadline=EditorApplication.timeSinceStartup+35;
        while(!condition()) { Check(EditorApplication.timeSinceStartup<deadline,"timeout: "+label); yield return null; }
    }
    private static IEnumerator Delay(float seconds)
    { double end=EditorApplication.timeSinceStartup+seconds; while(EditorApplication.timeSinceStartup<end) yield return null; }
    private static Gem GemAt(int x,int y)=>(Gem)Call(board,"GetGem",x,y);
    private static EnemyActor Find(string name)
    { foreach(var actor in waves.ActiveEnemies) if(actor.Definition==Data(name)) return actor; return null; }
    private static EnemyDefinition Data(string name)=>AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Game/Data/Enemies/Enemy_"+name+".asset");
    private static void Check(bool value,string label) { if(!value) throw new Exception("Royal Play Mode validation failed: "+label); }
    private static object Get(object target,string name)=>target.GetType().GetField(name,Flags).GetValue(target);
    private static void Set(object target,string name,object value)=>target.GetType().GetField(name,Flags).SetValue(target,value);
    private static void SetProperty(object target,string name,object value)=>target.GetType().GetProperty(name,Flags).SetValue(target,value);
    private static object Call(object target,string name,params object[] args)=>target.GetType().GetMethod(name,Flags).Invoke(target,args);
}
