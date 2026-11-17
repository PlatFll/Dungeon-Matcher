#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable] public sealed class ResonancePlaytestCase
{
    public string label="typical-dungeon",zone="dungeon",character="skeleton";
    public ResonancePlaystyle policy=ResonancePlaystyle.Typical;
    public int seed=8201,level=1,maxVisits=2;
    public float maxSeconds=1500;
}
[Serializable] public sealed class ResonancePlaytestConfig { public ResonancePlaytestCase[] cases; public bool capture=true; }

/// <summary>Explicit editor-only x1 Game-scene runner. Never called by production bootstrap.</summary>
[InitializeOnLoad]
public static class CrystalResonancePlaytest
{
    private const string Pending="DungeonMatcher.CrystalResonance.Playtest";
    private static readonly Stack<IEnumerator> steps=new Stack<IEnumerator>();
    private static IDisposable profile,selection,mastery;
    private static string output;
    private static bool failed;
    private static RunSession observed;
    private static StreamWriter events;
    static CrystalResonancePlaytest()
    {
        EditorApplication.playModeStateChanged+=state=>
        {
            if(state==PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Pending,false))
            {
                output=Path.GetFullPath(".utmp/CrystalResonance/Playtest");Directory.CreateDirectory(output);
                Application.logMessageReceived+=Log;steps.Clear();steps.Push(Runs());EditorApplication.update+=Tick;
            }
            if(state==PlayModeStateChange.ExitingPlayMode && SessionState.GetBool(Pending,false)) Finish();
        };
    }
    [MenuItem("Dungeon Matcher/Validation/Crystal Resonance Human-like Runs")]
    public static void Run()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Exit Play Mode first.");
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
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
        catch(Exception e) { failed=true;File.AppendAllText(Path.Combine(output,"errors.txt"),e+"\n");Finish(); }
    }
    private static IEnumerator Runs()
    {
        var path=Path.GetFullPath(".utmp/CrystalResonance/playtest-config.json");
        var config=JsonUtility.FromJson<ResonancePlaytestConfig>(File.ReadAllText(path));
        foreach(var sample in config.cases) yield return Play(sample,config.capture);
    }
    private static IEnumerator Play(ResonancePlaytestCase sample,bool capture)
    {
        Cleanup();string folder=Path.Combine(output,sample.label);Directory.CreateDirectory(folder);
        string profilePath=Path.Combine(folder,"account.json");
        var save=new AccountSave{potions=3,bombs=3,equipPotions=true,equipBombs=true};
        save.characters.Add(new CharacterProgress{id=sample.character,level=sample.level});
        // Explicit disposable starting profile/zone selection, no depth/HP/charge grants in a played run.
        save.run=new AccountRunJournal { id="cr-"+sample.seed+"-"+sample.character,playerId=sample.character,level=sample.level,
            supportsContinuation=true,potionCharges=3,bombCharges=3,initialPotionCharges=3,initialBombCharges=3,
            checkpoint=new RunCombatSnapshot { version=2, clock=new CombatClockSnapshot {profile=CombatClockSnapshot.UnifiedProfile,zoneId=sample.zone}, travel=new ZoneTravelSnapshot {version=1,enabled=true,zoneId=sample.zone} } };
        File.WriteAllText(profilePath,JsonUtility.ToJson(save));profile=AccountProgression.UseDisposableProfile(profilePath);
        selection=CharacterSelectionSettings.UseTemporarySelection(sample.character);mastery=GemMasterySettings.UseTemporaryLoadout(GemMasteryLoadout.Default);
        UnityEngine.Random.InitState(sample.seed);Time.timeScale=1;
        SceneManager.LoadScene("Game");float loadDeadline=Time.realtimeSinceStartup+60;
        while(RunSession.Current==null || !RunSession.Current.InitialStateReady || RunSession.Current.Continuation.IsRestoring)
        { if(Time.realtimeSinceStartup>loadDeadline)throw new Exception("Initial Game load did not settle");yield return null; }
        events=new StreamWriter(Path.Combine(folder,"events.jsonl")){AutoFlush=true};
        var actions=new StreamWriter(Path.Combine(folder,"actions.jsonl")){AutoFlush=true};
        var driver=new HumanLikePlaytestDriver(sample.policy,sample.seed);
        float start=Time.realtimeSinceStartup,visitStart=start,lastAction=start;
        int visit=RunSession.Current.Travel.State.visit,completed=0,lastCapturedWave=-1;
        string zone=RunSession.Current.Zone.Definition.zoneId;
        driver.ActionLog=action=> { lastAction=Time.realtimeSinceStartup;actions.WriteLine(JsonUtility.ToJson(new InputRow {wall=lastAction-start,action=action,visit=visit})); };
        File.WriteAllText(Path.Combine(folder,"case.json"),JsonUtility.ToJson(sample,true));
        File.WriteAllText(Path.Combine(folder,"visits.csv"),"zone,visit,completed,wall_seconds,end_depth,score,hp\n");
        try
        {
            while(Time.realtimeSinceStartup-start<sample.maxSeconds && completed<sample.maxVisits)
            {
                var run=RunSession.Current;
                if(run==null){yield return null;continue;}
                if(observed!=run)
                {
                    observed=run;run.Resonance.Telemetry+=Record;
                    run.Resonance.Record("observer_attached",sample.policy+"/"+sample.character+"/L"+sample.level);
                    lastCapturedWave=-1;
                }
                if(run.IsFinished)break;
                if(run.Travel.State.visit!=visit && run.Travel.State.stage==0 && !run.Continuation.IsRestoring)
                {
                    File.AppendAllText(Path.Combine(folder,"visits.csv"),FormattableString.Invariant($"{zone},{visit},true,{Time.realtimeSinceStartup-visitStart:0.###},{run.Waves.CurrentWave},{run.Resonance.RunScore},{run.Player.CurrentHealth}\n"));
                    completed++;visit=run.Travel.State.visit;zone=run.Zone.Definition.zoneId;visitStart=Time.realtimeSinceStartup;lastAction=visitStart;
                }
                if(run.Continuation.CanCapture && !run.Continuation.IsRestoring)
                {
                    if(lastCapturedWave!=run.Waves.CurrentWave)
                    {
                        lastCapturedWave=run.Waves.CurrentWave;
                        File.WriteAllText(Path.Combine(folder,"checkpoint-"+lastCapturedWave+".json"),JsonUtility.ToJson(run.Continuation.Capture(),true));
                        if(capture)ScreenCapture.CaptureScreenshot(Path.Combine(folder,"game-"+lastCapturedWave+".png"));
                    }
                    driver.Step(run);
                }
                if(Time.realtimeSinceStartup-lastAction>120 && run.Travel.State.stage==0)
                    throw new Exception("No accepted input/progress for 120 seconds; inspect checkpoint/gates, not a completed pacing sample.");
                yield return null;
            }
            var end=RunSession.Current;string result=end!=null && end.IsFinished?"defeat":completed>=sample.maxVisits?"visit limit":"censored duration limit";
            File.WriteAllText(Path.Combine(folder,"result.json"),JsonUtility.ToJson(new Result {outcome=result,wallSeconds=Time.realtimeSinceStartup-start,completedVisits=completed,swaps=driver.Swaps,abilities=driver.Abilities,cards=driver.CardPicks},true));
            if(end!=null)File.AppendAllText(Path.Combine(folder,"visits.csv"),FormattableString.Invariant($"{zone},{visit},false,{Time.realtimeSinceStartup-visitStart:0.###},{end.Waves.CurrentWave},{end.Resonance.RunScore},{end.Player.CurrentHealth}\n"));
        }
        finally { actions.Dispose(); events?.Dispose();events=null; }
        SceneManager.LoadScene("MainMenu");yield return null;Cleanup();
    }
    [Serializable] private sealed class InputRow { public float wall; public string action;public int visit; }
    [Serializable] private sealed class Result { public string outcome; public float wallSeconds;public int completedVisits,swaps,abilities,cards; }
    private static void Record(ResonanceTelemetryEvent e) => events?.WriteLine(JsonUtility.ToJson(e));
    private static void Log(string message,string trace,LogType kind)
    { if(kind==LogType.Error||kind==LogType.Exception) {failed=true;File.AppendAllText(Path.Combine(output,"errors.txt"),message+"\n"+trace+"\n");} }
    private static void Cleanup()
    { if(observed!=null && observed.Resonance!=null)observed.Resonance.Telemetry-=Record;observed=null;mastery?.Dispose();selection?.Dispose();profile?.Dispose();mastery=selection=profile=null; }
    private static void Finish()
    {
        EditorApplication.update-=Tick;Application.logMessageReceived-=Log;steps.Clear();events?.Dispose();events=null;
        Cleanup();SessionState.SetBool(Pending,false);Time.timeScale=1;
        File.WriteAllText(Path.Combine(output,"completion.txt"),failed?"FAILED — inspect errors":"Runner completed; inspect outcomes and sample counts. Proxies do not certify human enjoyment.");
        if(Application.isBatchMode)EditorApplication.Exit(failed?1:0);else EditorApplication.ExitPlaymode();
    }
}
#endif
