using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>Opt-in real scene checks in isolated editor processes; never a production auto-player.</summary>
[InitializeOnLoad]
public static class DesignV2PlayValidation
{
    private const string Key="DungeonMatcher.DesignV2.Play";
    private static readonly Stack<IEnumerator> steps=new Stack<IEnumerator>();
    private static IDisposable profile,selection;
    private static string error;
    private static readonly string Output=Path.GetFullPath(".utmp/DesignV2");
    private static int result;
    private static bool priorMotion;
    static DesignV2PlayValidation()
    {
        EditorApplication.playModeStateChanged+=state=>
        {
            if(SessionState.GetString(Key,"")=="") return;
            if(state==PlayModeStateChange.EnteredPlayMode)
            {
                error=null; result=0; steps.Clear();
                Application.logMessageReceived+=Log;
                string mode=SessionState.GetString(Key,"");
                steps.Push(mode=="write"?WriteRestart():mode=="read"?ReadRestart():Visuals());
                EditorApplication.update+=Tick;
            }
            if(state==PlayModeStateChange.EnteredEditMode)
            {
                SessionState.SetString(Key,""); profile?.Dispose();selection?.Dispose();
                EditorApplication.Exit(result);
            }
        };
    }
    public static void Write()=>Begin("write");
    public static void Read()=>Begin("read");
    public static void UI()=>Begin("ui");
    private static void Begin(string mode)
    {
        if(!Application.isBatchMode) throw new InvalidOperationException("Use an isolated batch editor.");
        Directory.CreateDirectory(Output);
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        SessionState.SetString(Key,mode);
        SetSize(1080,1920);
        EditorApplication.EnterPlaymode();
    }
    private static void Tick()
    {
        try
        {
            Check(error==null,error);
            if(steps.Count==0) { Finish(); return; }
            var step=steps.Peek();
            if(!step.MoveNext()) { steps.Pop(); return; }
            if(step.Current is IEnumerator nested) steps.Push(nested);
            EditorApplication.QueuePlayerLoopUpdate();
        }
        catch(Exception exception) { result=1; Debug.LogException(exception); Finish(); }
    }
    private static void Finish()
    {
        EditorApplication.update-=Tick;Application.logMessageReceived-=Log;
        Time.captureDeltaTime=0;Time.timeScale=1;
        if(SessionState.GetString(Key,"")=="ui") PresentationPreferences.SetReducedMotion(priorMotion);
        File.AppendAllText(Path.Combine(Output,"play-validation.txt"),SessionState.GetString(Key,"")+": "+(result==0?"PASS":"FAIL")+"\n");
        EditorApplication.ExitPlaymode();
    }
    private static void Log(string text,string trace,LogType type)
    {
        if(type==LogType.Exception && trace.Contains("UnityEditor.Search.SearchInit.IndexationOnStartup"))
        { File.AppendAllText(Path.Combine(Output,"editor-search-errors.txt"),text+"\n"); return; }
        if(type==LogType.Error || type==LogType.Exception || type==LogType.Assert) error=text;
    }
    private static void Profile(string filename,bool fresh)
    {
        string path=Path.Combine(Output,filename);
        if(fresh) File.WriteAllText(path,JsonUtility.ToJson(new AccountSave { gold=200,potions=3,bombs=3,equipPotions=true,equipBombs=true,firstKingClaimed=true }));
        profile=AccountProgression.UseDisposableProfile(path);
        selection=CharacterSelectionSettings.UseTemporarySelection("bardley");
    }
    private static IEnumerator WriteRestart()
    {
        Profile("writer.json",true);Time.captureDeltaTime=1f/60;
        SceneManager.LoadScene("Game");yield return Stable();
        var run=RunSession.Current;
        // Start at a real late formation and add a Spear Knight escort, whose
        // two-hit attack has an observable in-flight interval.
        run.Waves.ClearCurrentWave();yield return Wait(.3f);
        yield return Until(()=>!run.Board.IsBusy,"old encounter cleanup");
        var fields=BindingFlags.Instance|BindingFlags.NonPublic;
        typeof(WaveController).GetField("currentWave",fields).SetValue(run.Waves,30);
        var seen=(HashSet<EnemyDefinition>)typeof(WaveController).GetField("seenMilestoneLeaders",fields).GetValue(run.Waves);
        foreach(string prior in new[]{"TownMarshal","SiegeSergeant","KnightCaptain","RoyalArchbishop"})
            seen.Add(AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Game/Data/Enemies/Enemy_"+prior+".asset"));
        run.Waves.SpawnCurrentWave();yield return Stable();
        Check(run.Waves.TrySummonEnemy(AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Game/Data/Enemies/Enemy_SpearKnight.asset"),out var spear),"two-hit escort spawned");
        yield return Stable();
        run.Player.TryTakeDamage(13);run.Player.GrantShield(5);
        run.Player.GetComponent<PlayerAbilityEnergy>().AddEnergy(100);
        var attack=spear.GetComponent<EnemyAutoAttack>();
        typeof(EnemyAutoAttack).GetField("remainingAttackTime",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(attack,.06f);
        Check(run.Continuation.SaveNow(),"save pre-attack boundary");
        yield return Until(()=>attack.IsAttackSequenceInProgress,"normal enemy attack starts");
        Check(run.Player.GetComponent<PlayerAbilityController>().TryActivate(),"ability accepted during enemy action");
        for(int i=0;i<5;i++) yield return null;
        Check(run.Board.IsBusy,"cast is still in flight");
        Check(run.Continuation.SaveNow(),"durable partial cast/attack");
        File.Copy(Path.Combine(Output,"writer.json"),Path.Combine(Output,"pending.json"),true);
        yield return Stable();Time.timeScale=0;
        File.WriteAllText(Path.Combine(Output,"expected.json"),JsonUtility.ToJson(run.Continuation.Capture(),true));
        Check(run.Continuation.SaveNow(),"oracle settled snapshot");
        Debug.Log("Design v2 writer: accepted cast plus overlapping enemy attack saved before resolution; settled oracle recorded.");
    }
    private static IEnumerator ReadRestart()
    {
        Profile("pending.json",false);
        var expected=JsonUtility.FromJson<RunCombatSnapshot>(File.ReadAllText(Path.Combine(Output,"expected.json")));
        string id=AccountProgression.Current.ActiveRun.id;
        SceneManager.LoadScene("Game");yield return Restored();
        var run=RunSession.Current;run.GetComponent<RunControlsUI>().Close();Time.captureDeltaTime=1f/60;
        yield return Stable();Time.timeScale=0;
        var actual=run.Continuation.Capture();
        Check(run.RunId==id,"same durable run identity");
        Check(JsonUtility.ToJson(actual.board)==JsonUtility.ToJson(expected.board),"fresh-process board and specials match uninterrupted result");
        Check(actual.player.health==expected.player.health && actual.player.shield==expected.player.shield,"overlapping attack resolves once");
        Check(actual.player.energy==expected.player.energy,"accepted cast energy/refund resolves once");
        Check(actual.gameplayRandom==expected.gameplayRandom,"same gameplay random position");
        Check(run.Continuation.SaveNow(),"commit recovered state");
        yield return Shot("continue-combat");
        Debug.Log("Design v2 reader: separate process restored board, player HP/shield, energy and random state exactly after overlapping enemy attack and Bardley cast.");
    }
    private static IEnumerator Visuals()
    {
        priorMotion=PresentationPreferences.ReducedMotion;PresentationPreferences.SetReducedMotion(false);
        Profile("ui.json",true);
        string original=File.ReadAllText(Path.Combine(Output,"ui.json"));
        SceneManager.LoadScene("MainMenu");yield return Wait(.5f);yield return Shot("menu");
        Press("ShopButton");yield return Wait(.2f);yield return Shot("shop");
        Press("Back");Press("PracticeButton");yield return Stable();
        var run=RunSession.Current;
        Check(run.IsPractice,"practice launched from menu");
        run.Player.TryTakeDamage(20);Check(run.TryUsePotion(),"practice Potion accepted");
        run.GetComponent<RunControlsUI>().OpenSettings();yield return Shot("settings");
        Check(!run.Waves.ActiveEnemies[0].GetComponent<EnemyAutoAttack>().PerformAttackImmediately(),"pause rejects a ready enemy attack");
        var clocks=run.Waves.ActiveEnemies.Select(e=>e.GetComponent<EnemyAutoAttack>().RemainingAttackTime).ToArray();
        Press("ReducedMotion");Check(PresentationPreferences.ReducedMotion,"motion enabled");
        yield return Wait(.5f);
        Check(run.Waves.ActiveEnemies.Select(e=>e.GetComponent<EnemyAutoAttack>().RemainingAttackTime).SequenceEqual(clocks),"settings pause gameplay clocks");
        Press("Resume");
        run.GetComponent<RunControlsUI>().OpenGuide(CombatGuide.Basics);yield return Shot("guide");Press("ResumeGuide");
        run.ToggleBombTargeting();Check((bool)typeof(RunSession).GetMethod("PreviewBomb",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(run,new object[]{run.Board.GetGem(3,3)}),"Bomb preview");yield return Shot("bomb-preview");run.CancelTargeting();
        Check(run.ExitTo("MainMenu"),"practice exits");yield return Wait(.2f);
        Check(File.ReadAllText(Path.Combine(Output,"ui.json"))==original,"practice leaves real account file byte-identical");
        RunLaunchOptions.Challenge=RunChallenge.BoardOnly;SceneManager.LoadScene("Game");yield return Stable();run=RunSession.Current;
        Check(run.Challenge==RunChallenge.BoardOnly && run.Charges(ConsumableKind.Bomb)==0,"challenge carries no supply charges");
        run.Player.GetComponent<PlayerAbilityEnergy>().AddEnergy(1000);
        Check(!run.Player.GetComponent<PlayerAbilityController>().CanActivate,"Board Only disables abilities");
        run.GetComponent<RunControlsUI>().OpenGuide(CombatGuide.Ability(run.Player));yield return Shot("challenge");Press("ResumeGuide");
        run.Player.TryTakeDamage(99999);yield return Wait(1.5f);yield return Shot("failure");
        Check(run.RewardFinalized,"defeat reward committed");
        SceneManager.LoadScene("MainMenu");yield return Wait(.2f);
        RunLaunchOptions.Challenge=RunChallenge.Standard;SceneManager.LoadScene("Game");yield return Stable();run=RunSession.Current;
        foreach(int wave in new[]{1,2})
        {
            yield return Until(()=>run.Waves.CurrentWave==wave&&run.Continuation.CanCapture,"draft wave");
            foreach(var enemy in run.Waves.ActiveEnemies.ToArray()) enemy.ResolveDirectDamage(99999);
        }
        yield return Until(()=>Object.FindFirstObjectByType<UpgradeChoiceUI>()?.IsOpen==true,"real draft visible");
        yield return Shot("draft");
        SetSize(720,1280);yield return Wait(.3f);yield return Shot("draft-720");
        run.GetComponent<RunControlsUI>().OpenSettings();yield return Shot("settings-720");
        Debug.Log("Design v2 UI: live menus/practice/supplies/settings pause/guide/Bomb preview/challenge/defeat/draft checked and rendered at two portrait sizes.");
    }
    private static void Press(string name)
    {
        var button=Object.FindObjectsByType<Button>(FindObjectsSortMode.None).FirstOrDefault(b=>b.name==name&&b.gameObject.activeInHierarchy);
        Check(button!=null&&button.interactable,"button available: "+name);button.onClick.Invoke();
    }
    private static IEnumerator Shot(string name)
    { yield return Wait(.15f);ScreenCapture.CaptureScreenshot(Path.Combine(Output,name+".png"));yield return Wait(.2f); }
    private static IEnumerator Stable()=>Until(()=>RunSession.Current!=null&&RunSession.Current.Continuation.CanCapture&&RunSession.Current.Waves.IsWaveActive,"combat settles");
    private static IEnumerator Restored()=>Until(()=>RunSession.Current!=null&&!RunSession.Current.Continuation.IsRestoring,"continuation completes");
    private static IEnumerator Until(Func<bool> predicate,string label)
    { float deadline=Time.realtimeSinceStartup+45;while(!predicate()){Check(Time.realtimeSinceStartup<deadline,label);yield return null;} }
    private static IEnumerator Wait(float seconds)
    { float deadline=Time.realtimeSinceStartup+seconds;while(Time.realtimeSinceStartup<deadline)yield return null; }
    private static void Check(bool condition,string label)
    { if(!condition)throw new InvalidOperationException("Design v2 play validation: "+label); }
    private static void SetSize(int width,int height)
    {
        var assembly=typeof(Editor).Assembly;var type=assembly.GetType("UnityEditor.GameViewSizes");
        var sizes=typeof(ScriptableSingleton<>).MakeGenericType(type).GetProperty("instance",BindingFlags.Public|BindingFlags.Static).GetValue(null);
        var groupType=type.GetProperty("currentGroupType",BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance).GetValue(sizes);
        var group=type.GetMethod("GetGroup").Invoke(sizes,new[]{groupType});
        var size=Activator.CreateInstance(assembly.GetType("UnityEditor.GameViewSize"),new object[]{Enum.ToObject(assembly.GetType("UnityEditor.GameViewSizeType"),1),width,height,"Design v2 validation"});
        group.GetType().GetMethod("AddCustomSize").Invoke(group,new[]{size});
        int count=(int)group.GetType().GetMethod("GetTotalCount").Invoke(group,null);
        var viewType=assembly.GetType("UnityEditor.GameView");var view=EditorWindow.GetWindow(viewType);
        viewType.GetProperty("selectedSizeIndex",BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance).SetValue(view,count-1);
        view.Show();view.Focus();view.Repaint();
    }
    public static void BuildWindows()
    {
        Directory.CreateDirectory(Path.Combine(Output,"Windows"));
        // Isolate the test player's account/PlayerPrefs from the user's game.
        string company=PlayerSettings.companyName;
        try
        {
            PlayerSettings.companyName=company+" Validation";
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),
                locationPathName=Path.Combine(Output,"Windows/DungeonMatcher.exe"),target=BuildTarget.StandaloneWindows64,
                options=BuildOptions.Development });
            Check(report.summary.result==BuildResult.Succeeded,"Windows build succeeds");
        }
        finally {PlayerSettings.companyName=company;AssetDatabase.SaveAssets();}
        EditorApplication.Exit(0);
    }
}
