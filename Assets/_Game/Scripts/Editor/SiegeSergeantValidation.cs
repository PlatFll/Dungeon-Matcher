using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

// Run from the menu or batch mode with -executeMethod SiegeSergeantValidation.Run.
// This exercises production selection, identity tracking, actor damage and assets.
public static class SiegeSergeantValidation
{
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Static |
        BindingFlags.Public | BindingFlags.NonPublic;

    [MenuItem("Dungeon Matcher/Validation/Siege Sergeant")]
    public static void Run()
    {
        UnityEngine.Random.State randomState = UnityEngine.Random.state;
        GameObject root = new GameObject("SergeantValidation");
        root.SetActive(false);
        try
        {
            EnemyDefinition definition = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(
                "Assets/_Game/Data/Enemies/Enemy_SiegeSergeant.asset");
            DifficultyProfile profile = AssetDatabase.LoadAssetAtPath<DifficultyProfile>(
                "Assets/_Game/Data/Balance/DifficultyProfile_Standard.asset");
            EnemyDatabase database = AssetDatabase.LoadAssetAtPath<EnemyDatabase>(
                "Assets/_Game/Data/Enemies/EnemyDatabase_Main.asset");
            WaveSpawnProfile waves = AssetDatabase.LoadAssetAtPath<WaveSpawnProfile>(
                "Assets/_Game/Data/Balance/WaveSpawnProfile_Standard.asset");
            Check(definition != null && profile != null && database != null && waves != null,
                "Required assets resolve");
            Check(definition.FallbackVisualSprite != null && definition.EnemyPrefab != null,
                "Sprite and prefab references import");
            EnemyRuntimeStats stats = profile.CalculateStats(definition,16);
            Check(stats.MaxHealth == 600 && stats.Damage == 5 &&
                Mathf.Abs(stats.AttackInterval-10f) < 0.001f &&
                Mathf.RoundToInt(definition.HammerBaseDamage*stats.DamageMultiplier) == 12 &&
                stats.SpecialTurnRequirement == 3, "Introduction stats");
            Check(database.ContainsEnemy(definition) &&
                !database.GetEligibleEnemies(EnemyCategory.Miniboss,15).Contains(definition) &&
                database.GetEligibleEnemies(EnemyCategory.Miniboss,16).Contains(definition),
                "Eligibility boundary");
            Check(waves.CreatePlan(16).EnemyCount == 1 &&
                waves.GetFixedEnemy(16,0) == definition && waves.GetFixedEnemy(8,0) == null,
                "Solo checkpoint and unchanged Marshal rule");

            BoardController board = root.AddComponent<BoardController>();
            Set(board,"width",4); Set(board,"height",4);
            Gem[,] grid = new Gem[4,4];
            Set(board,"gems",grid);
            List<Vector2Int> cells = new List<Vector2Int>();
            for (int y=0;y<4;y++) for (int x=0;x<4;x++)
            {
                GameObject go = new GameObject("Gem");
                go.transform.SetParent(root.transform);
                Gem gem = go.AddComponent<Gem>();
                gem.SetGridPosition(x,y);
                grid[x,y] = gem;
                cells.Add(new Vector2Int(x,y));
            }
            for (int seed=0;seed<30;seed++)
            {
                UnityEngine.Random.InitState(seed);
                List<Vector2Int> line = (List<Vector2Int>)Call(board,"ChooseStraightCellRun",cells,4);
                Check(line != null && line.Count == 4,"Full line selected");
                Vector2Int step = line[1]-line[0];
                Check(step == Vector2Int.right || step == Vector2Int.up,"Orthogonal formation");
                for(int i=1;i<4;i++) Check(line[i] == line[0]+step*i,"Contiguous line");
            }
            List<Vector2Int> scattered = new List<Vector2Int>
            { new Vector2Int(0,0),new Vector2Int(2,0),new Vector2Int(0,2),new Vector2Int(2,2) };
            Check(Call(board,"ChooseStraightCellRun",scattered,4) == null,
                "No full formation permits random fallback");

            IDictionary holes = (IDictionary)Get(board,"minedCellOwners");
            holes.Add(new Vector2Int(0,0),123);
            typeof(Gem).GetProperty("SpecialType").SetValue(grid[1,0],GemSpecialType.RowBomb);
            List<Vector2Int> legal = (List<Vector2Int>)Call(board,"BuildBarricadableCellList",true);
            Check(!legal.Contains(new Vector2Int(0,0)) && !legal.Contains(new Vector2Int(1,0)),
                "Holes and specials excluded");

            EnemyActor owner = root.AddComponent<EnemyActor>();
            Set(owner,"isInitialized",true); Set(owner,"definition",definition);
            Set(owner,"currentHealth",600);
            typeof(EnemyActor).GetProperty("RuntimeStats").SetValue(owner,stats);
            Type requestType = typeof(BoardController).GetNestedType("BoardMutationRequest",Flags);
            object request = Activator.CreateInstance(requestType,true);
            Set(request,"OwnerActor",owner); Set(request,"WarningMoves",2);
            Set(board,"completedValidPlayerMoves",7);
            Call(board,"ExecuteMarkGemPair",request);
            BoardController.GemPairThreat threat =
                (BoardController.GemPairThreat)Get(request,"PairThreat");
            Check(threat != null && threat.DueMove == 9 && board.IsGemPairThreatValid(threat),
                "Warning grants two full moves");
            Gem first = threat.First;
            Gem second = threat.Second;
            // Move the original gem by swapping identity/coordinates: warning survives.
            Gem replacement = grid[3,3] == first || grid[3,3] == second ? grid[2,3] : grid[3,3];
            int oldX=first.Column,oldY=first.Row,newX=replacement.Column,newY=replacement.Row;
            grid[oldX,oldY]=replacement; replacement.SetGridPosition(oldX,oldY);
            grid[newX,newY]=first; first.SetGridPosition(newX,newY);
            Check(board.IsGemPairThreatValid(threat),"Warning follows moved gem");
            grid[newX,newY]=null;
            Check(!board.IsGemPairThreatValid(threat),"Either cleared gem cancels both targets");
            grid[newX,newY]=replacement;
            Check(!board.IsGemPairThreatValid(threat),"Refill cannot inherit old warning");
            board.CancelGemPairThreat(threat);
            Check(threat.Ended,"Cancellation is explicit and permanent");

            // Test passive against real owner counts and the actor damage path.
            SiegeSergeantEnemyAbility ability = root.AddComponent<SiegeSergeantEnemyAbility>();
            Set(ability,"actor",owner); Set(ability,"board",board);
            Set(ability,"ownerId",owner.GetInstanceID()); Set(ability,"released",false);
            owner.IncomingDamageMultiplier = () => (float)Call(ability,"GetIncomingDamageMultiplier");
            IDictionary barricades = (IDictionary)Get(board,"barricadeCells");
            Type stateType = typeof(BoardController).GetNestedType("BarricadeCellState",Flags);
            object ownBlock = Activator.CreateInstance(stateType,true);
            Set(ownBlock,"OwnerInstanceId",owner.GetInstanceID());
            barricades.Add(new Vector2Int(0,1),ownBlock);
            owner.TryTakeDamage(100);
            Check(owner.CurrentHealth==520,"Own block reduces 100 to 80");
            owner.TryTakeDamageWithoutFeedback(100);
            Check(owner.CurrentHealth==440,"DoT uses same conditional defence");
            board.OrphanBarricadesForOwner(owner.GetInstanceID());
            owner.TryTakeDamage(100);
            Check(owner.CurrentHealth==340,"Orphan/other-owner blocks give no defence");
            owner.IncomingDamageMultiplier=null;
            Debug.Log("Siege Sergeant validation PASSED. Play Mode visual and pacing checks remain separate.");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(root);
            UnityEngine.Random.state = randomState;
        }
    }

    private static void Check(bool value,string name)
    {
        if (!value) throw new Exception("Siege Sergeant validation failed: "+name);
    }
    private static object Get(object target,string name) =>
        target.GetType().GetField(name,Flags).GetValue(target);
    private static void Set(object target,string name,object value) =>
        target.GetType().GetField(name,Flags).SetValue(target,value);
    private static object Call(object target,string name,params object[] args) =>
        target.GetType().GetMethod(name,Flags).Invoke(target,args);
}
