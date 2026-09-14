using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Opt-in engine playthroughs, not a combat simulator or production auto-player.</summary>
[InitializeOnLoad]
public static class BalancePacingValidation
{
    private const string Pending="DungeonMatcher.BalancePacing";
    private const float Speed=6;
    private const BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;
    private static readonly Stack<IEnumerator> steps=new Stack<IEnumerator>();
    private static IDisposable profile,selection,mastery;
    private static string output,label;
    private static float started,waveStarted,hpLost,waveLoss,menuSeconds;
    private static int movesAtStart,choices,abilities,specialMoves;
    private static RunSession run;
    private static bool failed;
    private static readonly List<UnityEngine.Object> clones=new List<UnityEngine.Object>();
    static BalancePacingValidation()
    {
        EditorApplication.playModeStateChanged+=state=>
        {
            if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Pending,false))
            {
                output=Path.GetFullPath(".utmp/BalancePacing");Directory.CreateDirectory(output);
                File.WriteAllText(Path.Combine(output,"runs.csv"),"policy,character,level,items,seed,result,completed_waves,game_seconds_with_choices,hp_lost,gold,gold_per_minute,moves,abilities,special_swaps,potions_used,bombs_used\n");
                File.WriteAllText(Path.Combine(output,"waves.csv"),"policy,wave,formation,game_seconds,moves,hp_lost,hp_remaining\n");
                Application.logMessageReceived+=Log;steps.Clear();steps.Push(Checks());EditorApplication.update+=Tick;
            }
            if(state==PlayModeStateChange.ExitingPlayMode&&SessionState.GetBool(Pending,false))
            {EditorApplication.update-=Tick;Application.logMessageReceived-=Log;Cleanup();SessionState.SetBool(Pending,false);Time.timeScale=1;}
        };
    }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void BeforeScene()
    {
        if(!SessionState.GetBool(Pending,false))return;
        InstallProfile("skeleton",1,false);
    }
    [MenuItem("Dungeon Matcher/Validation/Balance v1 Pacing")]
    public static void Run()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Exit Play Mode first.");
        EditorSceneManager.OpenScene("Assets/_Game/Scenes/MainMenu.unity");
        SessionState.SetBool(Pending,true);EditorApplication.EnterPlaymode();
    }
    private static void Tick()
    {
        try
        {
            if(steps.Count==0){Finish();return;}
            var step=steps.Peek();if(!step.MoveNext()){steps.Pop();return;}
            if(step.Current is IEnumerator nested)steps.Push(nested);
        }
        catch(Exception error){failed=true;File.AppendAllText(Path.Combine(output,"report.txt"),error+"\n");Finish();}
    }
    private static IEnumerator Checks()
    {
        File.WriteAllText(Path.Combine(output,"report.txt"),"Real Unity Game scenes at 6x time scale. Reported seconds are game time plus 3 seconds per card choice. Policies are synthetic input, not human retention evidence. Bardley uses a disposable 80-energy ability clone; committed cost remains 1. No HP/damage/encounter changes. 20-minute observations are censored, never forced deaths.\n");
        for(int mode=0;mode<4;mode++)
        foreach(string id in new[]{"skeleton","bardley"})
        {
            int level=mode<2?1:5;bool skilled=mode>0,items=mode==3;int seed=id=="skeleton"?3101:3102;
            yield return Play(id,level,skilled,items,seed);
        }
    }
    private static void InstallProfile(string id,int level,bool items)
    {
        mastery?.Dispose();selection?.Dispose();profile?.Dispose();
        string path=Path.GetFullPath(".utmp/BalancePacing/profiles/"+Guid.NewGuid().ToString("N")+".json");Directory.CreateDirectory(Path.GetDirectoryName(path));
        var save=new AccountSave{gold=0,potions=items?10:0,bombs=items?10:0,equipPotions=items,equipBombs=items};
        save.characters.Add(new CharacterProgress{id=id,level=level});File.WriteAllText(path,JsonUtility.ToJson(save));
        profile=AccountProgression.UseDisposableProfile(path);selection=CharacterSelectionSettings.UseTemporarySelection(id);
        mastery=GemMasterySettings.UseTemporaryLoadout(level>=5?new GemMasteryLoadout(GemMasteryReward.ColorCrystal,GemMasteryReward.PoisonBomb,GemMasteryReward.HealBomb,GemMasteryReward.ShieldBomb):GemMasteryLoadout.Default);
    }
    private static IEnumerator Play(string id,int level,bool skilled,bool items,int seed)
    {
        InstallProfile(id,level,items);UnityEngine.Random.InitState(seed);Time.timeScale=Speed;
        label=id+"-L"+level+"-"+(skilled?"greedy-special":"casual")+(items?"-equipped":"-bare");
        SceneManager.LoadScene("Game");float waitEnd=Time.realtimeSinceStartup+45;
        while(RunSession.Current==null||!RunSession.Current.Player.IsInitialized)
        {Require(Time.realtimeSinceStartup<waitEnd,"scene initialized");yield return null;}
        run=RunSession.Current;
        Set(run.Waves,"encounterRandom",new System.Random(seed));
        if(id=="bardley")
        {
            var definition=UnityEngine.Object.Instantiate(run.Player.Definition);
            var ability=UnityEngine.Object.Instantiate(definition.ActiveAbility);
            clones.Add(definition);clones.Add(ability);Set(ability,"energyCost",80);Set(definition,"activeAbility",ability);run.Player.Initialize(definition);
            Require(run.Player.ActiveAbility.EnergyCost==80,"disposable production-cost Bardley");
        }
        started=Time.time;waveStarted=Time.time;hpLost=waveLoss=menuSeconds=0;choices=abilities=specialMoves=0;movesAtStart=0;
        run.Waves.WaveStarted+=WaveStarted;run.Waves.WaveCompleted+=WaveCompleted;run.Player.DamageTaken+=Damage;
        var controller=UnityEngine.Object.FindFirstObjectByType<PlayerAbilityController>();
        float next=Time.time+(skilled?1.8f:4f),cardOpened=-1,lastAction=Time.realtimeSinceStartup;
        while(!run.IsFinished&&Time.time-started+menuSeconds<1200)
        {
            Require(Time.realtimeSinceStartup-lastAction<90,"board/transition remains responsive");
            var choice=UnityEngine.Object.FindFirstObjectByType<UpgradeChoiceUI>();
            if(choice!=null&&choice.IsOpen)
            {
                if(cardOpened<0)cardOpened=Time.realtimeSinceStartup;
                if(Time.realtimeSinceStartup-cardOpened>=3f/Speed)
                {ChooseCard(choice,skilled);choices++;menuSeconds+=3;cardOpened=-1;lastAction=Time.realtimeSinceStartup;}
                yield return null;continue;
            }
            if(Time.time>=next&&!run.Board.IsBusy&&!run.Board.IsExternalInputBlocked&&run.Waves.IsWaveActive)
            {
                if(items&&run.Player.CurrentHealth<run.Player.MaximumHealth*.55f&&run.TryUsePotion()){lastAction=Time.realtimeSinceStartup;}
                bool acted=false;
                if(items&&run.Waves.CurrentWave>=6&&run.Waves.ActiveEnemies.Count>=2&&run.CanUse(ConsumableKind.Bomb))
                {var grid=Grid(run.Board);acted=run.TryUseBomb(grid[3,3]);}
                if(!acted&&controller.CanActivate&&controller.TryActivate()){abilities++;acted=true;}
                if(!acted&&Move(run.Board,skilled)){acted=true;}
                if(acted){next=Time.time+(skilled?1.8f:4f);lastAction=Time.realtimeSinceStartup;}
            }
            yield return null;
        }
        float seconds=Time.time-started+menuSeconds;
        int wave=AccountProgression.Current.ActiveRun?.completedWaves??AccountProgression.Current.LastReward.waves;
        int gold=AccountProgression.Current.ActiveRun!=null?AccountProgression.Current.PreviewReward("Measurement").Total:AccountProgression.Current.LastReward.Total;
        string result=run.IsVictory?"Victory":run.IsFinished?"Defeat":"Censored at 20 minutes";
        int potionUses=items?10-AccountProgression.Current.Owned(ConsumableKind.HealthPotion):0,bombUses=items?10-AccountProgression.Current.Owned(ConsumableKind.Bomb):0;
        string row=string.Join(",",label,id,level,items,seed,result,wave,F(seconds),F(hpLost),gold,F(gold*60/seconds),run.Board.CompletedValidPlayerMoves,abilities,specialMoves,potionUses,bombUses);
        File.AppendAllText(Path.Combine(output,"runs.csv"),row+"\n");File.AppendAllText(Path.Combine(output,"report.txt"),row+"\n");
        run.Waves.WaveStarted-=WaveStarted;run.Waves.WaveCompleted-=WaveCompleted;run.Player.DamageTaken-=Damage;
        Require(run.ExitTo("MainMenu"),"measurement run finalized");Time.timeScale=Speed;
        float pause=Time.realtimeSinceStartup+.5f;while(Time.realtimeSinceStartup<pause)yield return null;
        foreach(var clone in clones)if(clone!=null)UnityEngine.Object.Destroy(clone);clones.Clear();
    }
    private static void WaveStarted(int wave){waveStarted=Time.time;waveLoss=0;movesAtStart=run.Board.CompletedValidPlayerMoves;}
    private static void Damage(PlayerActor actor,int amount){hpLost+=amount;waveLoss+=amount;}
    private static void WaveCompleted(int wave)
    {
        string formation=string.Join("+",run.Waves.OriginalEncounterDefinitions.Select(d=>d.name));
        File.AppendAllText(Path.Combine(output,"waves.csv"),string.Join(",",label,wave,formation,F(Time.time-waveStarted),run.Board.CompletedValidPlayerMoves-movesAtStart,F(waveLoss),run.Player.CurrentHealth)+"\n");
    }
    private static void ChooseCard(UpgradeChoiceUI choice,bool skilled)
    {
        var views=(UpgradeCardView[])Get(choice,"cardViews");
        var buttons=views.Select(v=>(Button)Get(v,"button")).Where(b=>b!=null&&b.interactable).ToArray();
        Require(buttons.Length>0,"offered card can be selected");
        // Alternate offer positions makes runs differ without inventing unavailable cards.
        buttons[skilled?choices%buttons.Length:0].onClick.Invoke();
    }
    private static bool Move(BoardController board,bool skilled)
    {
        Gem a=null,b=null;var grid=Grid(board);float best=-1;
        if(!skilled){if(!board.TryGetRandomHintMove(out a,out b))return false;}
        else for(int x=0;x<grid.GetLength(0);x++)for(int y=0;y<grid.GetLength(1);y++)
        foreach(var delta in new[]{Vector2Int.right,Vector2Int.up})
        {
            int xx=x+delta.x,yy=y+delta.y;if(xx>=grid.GetLength(0)||yy>=grid.GetLength(1))continue;
            var source=grid[x,y];var target=grid[xx,yy];
            if(source==null||target==null||!board.IsHintMoveStillValid(source,target))continue;
            float score=Score(grid,source,target);
            if(score>best){best=score;a=source;b=target;}
        }
        if(a==null||b==null)return false;
        if(a.SpecialType!=GemSpecialType.None||b.SpecialType!=GemSpecialType.None)specialMoves++;
        board.StartCoroutine((IEnumerator)typeof(BoardController).GetMethod("TrySwap",Flags).Invoke(board,new object[]{a,b}));return true;
    }
    private static float Score(Gem[,] grid,Gem a,Gem b)
    {
        if(a.SpecialType==GemSpecialType.ColorCrystal||b.SpecialType==GemSpecialType.ColorCrystal)return 150;
        Func<int,int,Gem> at=(x,y)=>x<0||y<0||x>=grid.GetLength(0)||y>=grid.GetLength(1)?null:grid[x,y]==a?b:grid[x,y]==b?a:grid[x,y];
        float score=0;
        foreach(var position in new[]{new Vector2Int(a.Column,a.Row),new Vector2Int(b.Column,b.Row)})
        foreach(var axis in new[]{Vector2Int.right,Vector2Int.up})
        {
            var center=at(position.x,position.y);int count=1;
            foreach(int sign in new[]{-1,1})for(int n=1;n<8;n++)
            {var gem=at(position.x+axis.x*n*sign,position.y+axis.y*n*sign);if(gem==null||gem.SpecialType==GemSpecialType.ColorCrystal||gem.Type!=center.Type)break;count++;}
            if(count>=3)
            {
                score+=count+(count>=4?30:0)+(count>=5?50:0);
                foreach(var enemy in run.Waves.ActiveEnemies)if(enemy!=null&&!enemy.IsDefeated&&enemy.AssignedGemType==center.Type)score+=count*(enemy.Definition.IsSupport?4:2);
            }
        }
        return score;
    }
    private static Gem[,] Grid(BoardController board)=>(Gem[,])Get(board,"gems");
    private static object Get(object target,string field)=>target.GetType().GetField(field,Flags).GetValue(target);
    private static void Set(object target,string field,object value)=>target.GetType().GetField(field,Flags).SetValue(target,value);
    private static string F(float value)=>value.ToString("F2",CultureInfo.InvariantCulture);
    private static void Require(bool value,string message){if(!value)throw new Exception(label+": "+message);}
    private static void Log(string message,string trace,LogType type)
    {if(type==LogType.Exception||type==LogType.Error){failed=true;File.AppendAllText(Path.Combine(output,"errors.txt"),message+"\n"+trace+"\n");}}
    private static void Cleanup(){mastery?.Dispose();selection?.Dispose();profile?.Dispose();foreach(var clone in clones)if(clone!=null)UnityEngine.Object.Destroy(clone);clones.Clear();}
    private static void Finish()
    {EditorApplication.update-=Tick;steps.Clear();File.AppendAllText(Path.Combine(output,"report.txt"),failed?"CHECK FAILED; inspect errors.\n":"ENGINE PLAYTHROUGHS COMPLETED.\n");Debug.Log("Pacing evidence: "+output);}
}
