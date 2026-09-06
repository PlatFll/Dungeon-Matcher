using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class RoyalMilestoneValidation
{
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
    [MenuItem("Dungeon Matcher/Validation/Royal Milestones")]
    public static void Run()
    {
        var root = new GameObject("Royal milestone validation"); root.SetActive(false);
        var random = UnityEngine.Random.state;
        try
        {
            var kingData = Data("King"); var bishopData = Data("RoyalArchbishop");
            var database = AssetDatabase.LoadAssetAtPath<EnemyDatabase>("Assets/_Game/Data/Enemies/EnemyDatabase_Main.asset");
            var waves = AssetDatabase.LoadAssetAtPath<WaveSpawnProfile>("Assets/_Game/Data/Balance/WaveSpawnProfile_Standard.asset");
            var difficulty = AssetDatabase.LoadAssetAtPath<DifficultyProfile>("Assets/_Game/Data/Balance/DifficultyProfile_Standard.asset");
            Check(kingData.Category == EnemyCategory.Boss && bishopData.Category == EnemyCategory.Miniboss, "Ranks");
            Check(database.ContainsEnemy(kingData) && database.ContainsEnemy(bishopData), "Database references");
            Check(kingData.RequiredBossEscort == bishopData, "Boss escort reference (loaded: " +
                AssetDatabase.GetAssetPath(kingData.RequiredBossEscort) + ")");
            Check(kingData.EnemyPrefab != null, "King prefab reference");
            Check(bishopData.EnemyPrefab != null, "Archbishop prefab reference");
            Check(kingData.RoyalMarkCount == 3 && kingData.RoyalMarkMoves == 3 && bishopData.RoyalMarkMoves == 3 &&
                kingData.BombardmentWarningMoves == 2, "Serialized telegraph counts");
            foreach (string name in new[] { "King", "RoyalSwordsman", "RoyalLancer", "RoyalArbalist" })
                Check(Data(name).RoyalAssaultParticipant, "Included participant " + name);
            foreach (string name in new[] { "CourtMage", "RoyalArchbishop", "RoyalStandardBearer", "Knight", "SpearGuard", "KnightCaptain" })
                Check(!Data(name).RoyalAssaultParticipant, "Excluded participant " + name);
            Check(kingData.RoyalReinforcements.Length == 5, "Five reinforcement definitions");
            foreach (var data in kingData.RoyalReinforcements)
                Check(data != null && database.ContainsEnemy(data) && data != bishopData && data.Category <= EnemyCategory.Special, "Legal reinforcement pool");
            var kingStats = difficulty.CalculateStats(kingData,25); var bishopStats = difficulty.CalculateStats(bishopData,21);
            Check(kingStats.MaxHealth >= 700 && kingStats.MaxHealth <= 850 && bishopStats.MaxHealth >= 300 && bishopStats.MaxHealth <= 400, "Normalized HP pacing");
            Debug.Log("Royal first-pass stats: King " + kingStats + "; Archbishop " + bishopStats);

            var kingWaves = new HashSet<int>(); var bishopWaves = new HashSet<int>();
            for (int seed=1;seed<=100;seed++)
            {
                var seen = new HashSet<EnemyDefinition>(); var rng = new System.Random(seed);
                for (int wave=1;wave<=30;wave++)
                {
                    var selected = waves.SelectMilestone(wave,rng,seen,out int count);
                    if (selected == null) continue;
                    Check(count == 2 && !seen.Contains(selected), "One-time two-unit milestone");
                    seen.Add(selected);
                    if (selected == kingData) { kingWaves.Add(wave); Check(seen.Contains(bishopData), "Archbishop precedes King"); }
                    if (selected == bishopData) bishopWaves.Add(wave);
                }
                Check(seen.Contains(kingData) && seen.Contains(bishopData), "Milestones reached");
            }
            Check(kingWaves.Count > 1 && bishopWaves.Count > 1, "Variable milestone timing across seeds");
            foreach(int wave in kingWaves) Check(wave >= 24 && wave <= 26,"King tuning window");

            var king = Actor(root, kingData,1000,990); var bishop = Actor(root,bishopData,1000,500);
            var special = Actor(root,Data("CourtMage"),1000,200);
            var roster = new List<EnemyActor> { king,bishop,special };
            Check(RoyalArchbishopEnemyAbility.SelectTriageTarget(bishop,roster)==special, "Badly wounded Special outranks almost-full Boss");
            Set(king,"currentHealth",400);
            Check(RoyalArchbishopEnemyAbility.SelectTriageTarget(bishop,roster)==king, "Rank weights compose with need");
            king.RestoreHealth(500);
            Check(RoyalArchbishopEnemyAbility.SelectTriageTarget(bishop,roster)==special, "Triage recalculates after heal");
            Check(RoyalArchbishopEnemyAbility.SelectTriageTarget(bishop,new[] { bishop })==bishop, "Solo self heal");
            bishop.RestoreHealth(1000);
            Check(RoyalArchbishopEnemyAbility.SelectTriageTarget(bishop,new[] { bishop })==null, "Full health excluded");

            var attack=king.gameObject.AddComponent<EnemyAutoAttack>(); Set(attack,"enemyActor",king);
            object enrage=new object(), blessing=new object();
            attack.SetNormalAttackModifiers(enrage,1.2f,1.25f); attack.SetRuntimeAttackSpeedMultiplier(1.2f);
            attack.SetNextSequenceModifier(blessing,1.4f); Set(attack,"commandDamageMultiplier",1.1f);
            Check(Mathf.Abs((float)Call(attack,"ConsumeSequenceDamageMultiplier")-1.848f)<0.0001f,"Enrage + blessing + command composition");
            Check(!attack.HasNextSequenceModifier(blessing),"Exactly one sequence consumes blessing");
            Set(attack,"commandDamageMultiplier",1f);
            Check(Mathf.Approximately((float)Call(attack,"ConsumeSequenceDamageMultiplier"),1.2f),"Command/blessing do not persist");
            attack.RemoveNormalAttackModifiers(enrage);
            Check(Mathf.Approximately(attack.RuntimeAttackSpeedMultiplier,1.2f),"Removing Enrage preserves banner speed");

            var ability=king.gameObject.AddComponent<KingEnemyAbility>();
            Set(ability,"actor",king); Set(ability,"ownAttack",attack); Set(ability,"released",false);
            var handler=(Action<EnemyActor,int,int>)Delegate.CreateDelegate(typeof(Action<EnemyActor,int,int>),ability,
                typeof(KingEnemyAbility).GetMethod("OnHealthDamage",Flags));
            king.SurvivedHealthDamage += handler;
            Set(king,"currentHealth",600); king.TryTakeDamage(400);
            var thresholds=(Queue<int>)Get(ability,"thresholds");
            Check(thresholds.Count==2 && thresholds.Dequeue()==50 && thresholds.Dequeue()==25,"Surviving 60 to 20 queues both in order");
            Check(ability.IsEnraged && king.SpecialTurnRequirement==3,"Enrage and cadence");
            king.RestoreHealth(800); king.TryTakeDamageWithoutFeedback(800);
            Check(thresholds.Count==0,"Healing never rearms thresholds; DoT uses same hook");
            Set(ability,"crossedHalf",false); Set(ability,"crossedQuarter",false);
            Set(king,"currentHealth",600); king.TryTakeDamage(1000);
            Check(thresholds.Count==0 && !ability.IsEnraged,"Lethal crossing never triggers");
            king.SurvivedHealthDamage -= handler;

            Set(king,"isDefeated",false); Set(king,"currentHealth",200);
            Set(ability,"roster",roster);
            for(int free=0;free<=2;free++)
            {
                for(int seed=0;seed<20;seed++)
                {
                    UnityEngine.Random.InitState(seed);
                    var service=new SummonProbe(free); ability.ConfigureSummonService(service);
                    Call(ability,"Reinforce");
                    Check(service.Calls.Count==free,"Fill exactly available slots");
                    int specials=0;
                    foreach(var definition in service.Calls)
                    {
                        Check(definition!=bishopData && Array.IndexOf(kingData.RoyalReinforcements,definition)>=0,"Summon only configured Royals");
                        if(definition.Category==EnemyCategory.Special) specials++;
                        Check(definition!=special.Definition,"Avoid duplicate living Special");
                    }
                    Check(specials<=1,"At most one Special in reinforcement batch");
                }
            }
            bishop.ResetSpecialCounter();
            for(int move=0;move<3;move++) bishop.RegisterValidPlayerTurn();
            Check(!bishop.IsSpecialReady,"Cadence is not prematurely ready");
            bishop.RegisterValidPlayerTurn(); Check(bishop.IsSpecialReady,"Fourth accepted turn readies special");

            var board=root.AddComponent<BoardController>(); Set(board,"width",4); Set(board,"height",4);
            var grid=new Gem[4,4]; Set(board,"gems",grid); Set(board,"completedValidPlayerMoves",7);
            for(int y=0;y<4;y++) for(int x=0;x<4;x++)
            {
                var go=new GameObject("Gem"); go.transform.SetParent(root.transform);
                var gem=go.AddComponent<Gem>(); gem.SetGridPosition(x,y); grid[x,y]=gem;
            }
            object request=NewRequest(bishop,3,3);
            Call(board,"ExecuteMarkGemSet",request);
            var first=(BoardController.GemSetThreat)Get(request,"SetThreat");
            Check(first.Targets.Count==3 && first.DueMove==10,"Three independently tracked marks and exact deadline");
            request=NewRequest(special,3,3); Call(board,"ExecuteMarkGemSet",request);
            var second=(BoardController.GemSetThreat)Get(request,"SetThreat");
            foreach(var gem in second.Targets) Check(!first.Targets.Contains(gem),"Simultaneous casts do not overlap marks");
            Gem moved=first.Targets[0]; int oldX=moved.Column,oldY=moved.Row;
            Gem other=grid[(oldX+1)%4,oldY]; grid[oldX,oldY]=other; other.SetGridPosition(oldX,oldY);
            moved.SetGridPosition((oldX+1)%4,oldY); grid[moved.Column,moved.Row]=moved;
            Check(board.IsEnvironmentalOrdinaryGem(moved) && first.Targets.Contains(moved),"Marks follow moving gem identity");
            grid[moved.Column,moved.Row]=null;
            Check(!board.IsEnvironmentalOrdinaryGem(moved),"Cleared gem cannot resolve pulse");
            Gem frozen=second.Targets[0]; ((IDictionary)Get(board,"pinnedGemOwners")).Add(frozen,123);
            Check(board.IsEnvironmentalOrdinaryGem(frozen) && !board.IsOrdinaryGemOnBoard(frozen),"Frozen ordinary gem remains environmentally clearable");
            typeof(Gem).GetProperty("SpecialType").SetValue(frozen,GemSpecialType.RowBomb);
            Check(!board.IsEnvironmentalOrdinaryGem(frozen),"Player special survives lane clear");
            board.CancelGemSetThreat(first); Check(first.Ended,"Owner cancellation");
            Debug.Log("Royal milestone validation PASSED. Play Mode timing, VFX, and encounter combinations require separate verification.");
        }
        finally { UnityEngine.Object.DestroyImmediate(root); UnityEngine.Random.state=random; }
    }
    private static EnemyDefinition Data(string name) => AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Game/Data/Enemies/Enemy_"+name+".asset");
    private sealed class SummonProbe : IEnemySummonService
    {
        private int remaining;
        public readonly List<EnemyDefinition> Calls=new List<EnemyDefinition>();
        public SummonProbe(int slots) { remaining=slots; }
        public bool HasFreeEnemySlot=>remaining>0;
        public bool TrySummonEnemy(EnemyDefinition definition,out EnemyActor enemy)
        {
            enemy=null; if(remaining<=0) return false;
            remaining--; Calls.Add(definition); return true;
        }
    }
    private static EnemyActor Actor(GameObject root,EnemyDefinition data,int max,int hp)
    {
        var go=new GameObject(data.DisplayName); go.transform.SetParent(root.transform);
        var actor=go.AddComponent<EnemyActor>(); Set(actor,"definition",data); Set(actor,"isInitialized",true); Set(actor,"currentHealth",hp);
        typeof(EnemyActor).GetProperty("RuntimeStats").SetValue(actor,new EnemyRuntimeStats(25,1,max,10,0,10,4)); return actor;
    }
    private static object NewRequest(EnemyActor owner,int count,int moves)
    {
        object request=Activator.CreateInstance(typeof(BoardController).GetNestedType("BoardMutationRequest",Flags),true);
        Set(request,"OwnerActor",owner); Set(request,"TargetCount",count); Set(request,"WarningMoves",moves); return request;
    }
    private static void Check(bool value,string label) { if(!value) throw new Exception("Royal milestone validation failed: "+label); }
    private static object Get(object target,string name)=>target.GetType().GetField(name,Flags).GetValue(target);
    private static void Set(object target,string name,object value)=>target.GetType().GetField(name,Flags).SetValue(target,value);
    private static object Call(object target,string name,params object[] args)=>target.GetType().GetMethod(name,Flags).Invoke(target,args);
}
