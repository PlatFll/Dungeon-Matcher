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
    private static StreamWriter events,nativeJournal;
    static CrystalResonancePlaytest()
    {
        EditorApplication.playModeStateChanged+=state=>
        {
            if(state==PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Pending,false))
            {
                output=Path.GetFullPath(".utmp/CrystalResonance/Playtest/"+System.DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"));Directory.CreateDirectory(output);
                Application.logMessageReceived+=Log;steps.Clear();steps.Push(Runs());EditorApplication.update+=Tick;
            }
            if(state==PlayModeStateChange.ExitingPlayMode && SessionState.GetBool(Pending,false)) Finish();
        };
    }
    [MenuItem("Dungeon Matcher/Validation/Crystal Resonance Human-like Runs")]
    public static void Run()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Exit Play Mode first.");
        failed=false;
        typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic)
            .Invoke(null,new object[]{new Vector2Int(1080,1920)});
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        SessionState.SetBool(Pending,true);
        // Let startup/import callbacks finish before entering Play Mode (Editor Search initializes here).
        double ready=EditorApplication.timeSinceStartup+3;
        void EnterWhenReady()
        {
            if(EditorApplication.timeSinceStartup<ready || EditorApplication.isUpdating || EditorApplication.isCompiling)return;
            EditorApplication.update-=EnterWhenReady;EditorApplication.EnterPlaymode();
        }
        EditorApplication.update+=EnterWhenReady;
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
        File.Copy(path,Path.Combine(output,"config.json"),true);
        string manifest=Path.GetFullPath(".utmp/CrystalResonance/prepared-source-manifest.json");
        if(File.Exists(manifest))File.Copy(manifest,Path.Combine(output,"source-manifest.json"),true);
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
        UnityEngine.Random.InitState(sample.seed);Time.timeScale=1;Application.runInBackground=true;
        // Seed each initial gameplay stream before Start creates any board or encounter.
        // This is initial fixture configuration, never a mid-run rewrite or peek at future RNG.
        void SeedInitial(Scene scene,LoadSceneMode mode)
        {
            if(scene.name!="Game")return;SceneManager.sceneLoaded-=SeedInitial;
            var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
            var initial=RunSession.Current;
            typeof(WaveController).GetField("encounterSeed",flags).SetValue(initial.Waves,sample.seed);
            typeof(WaveController).GetField("encounterRandom",flags).SetValue(initial.Waves,new SavedRandom(sample.seed));
            typeof(BoardController).GetField("refillRandom",flags).SetValue(initial.Board,new SavedRandom(sample.seed^0x4263));
            typeof(RunContinuation).GetProperty("Random").SetValue(initial.Continuation,new SavedRandom(sample.seed^0x71ab));
        }
        SceneManager.sceneLoaded+=SeedInitial;
        SceneManager.LoadScene("Game");float loadDeadline=Time.realtimeSinceStartup+60;
        while(RunSession.Current==null || !RunSession.Current.InitialStateReady || RunSession.Current.Continuation.IsRestoring)
        { if(Time.realtimeSinceStartup>loadDeadline)throw new Exception("Initial Game load did not settle");yield return null; }
        events=new StreamWriter(Path.Combine(folder,"events.jsonl")){AutoFlush=true};
        var actions=new StreamWriter(Path.Combine(folder,"actions.jsonl")){AutoFlush=true};
        nativeJournal=new StreamWriter(new System.IO.Compression.GZipStream(
            File.Create(Path.Combine(folder,"native-journal.jsonl.gz")),System.IO.Compression.CompressionLevel.Fastest));
        var driver=new HumanLikePlaytestDriver(sample.policy,sample.seed);
        float start=Time.realtimeSinceStartup,visitStart=start,lastAction=start,nextProbe=start+10;
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
                    run.Continuation.ValidationActionRecorded+=RecordNative;
                    run.Resonance.Record("observer_attached",sample.policy+"/"+sample.character+"/L"+sample.level);
                    lastCapturedWave=-1;
                }
                if(Time.realtimeSinceStartup>=nextProbe)
                {
                    nextProbe=Time.realtimeSinceStartup+10;
                    string gate=$"wall={Time.realtimeSinceStartup-start} timeScale={Time.timeScale} editorPaused={EditorApplication.isPaused} focused={Application.isFocused} busy={run.Board.IsBusy} inputBlocked={run.Board.IsExternalInputBlocked} canCapture={run.Continuation.CanCapture} waveCapture={run.Waves.CanCaptureContinuation} boardCapture={run.Board.CanCaptureContinuation} clockGate={run.MoveClock.IsBlockingWaveProgression} waveActive={run.Waves.IsWaveActive} clockTick={run.MoveClock.Tick} actors={run.Waves.ActiveEnemies.Count} cardGate={run.Waves.GetComponent<RunUpgradeCoordinator>().IsBlockingWaveProgression} savePending={run.Resonance.IsSavePending} modal={run.GetComponent<RunControlsUI>().IsModalOpen} restore={run.Continuation.IsRestoring} error={run.Continuation.Error}";
                    foreach(var field in typeof(BoardController).GetFields(System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic))
                        if(field.FieldType==typeof(bool)) gate+="\n"+field.Name+"="+field.GetValue(run.Board);
                    File.WriteAllText(Path.Combine(folder,"gates.txt"),gate);
                    if(run.Board.CanCaptureContinuation) File.WriteAllText(Path.Combine(folder,"last-board.json"),JsonUtility.ToJson(run.Board.CaptureContinuation(run.Waves.ContinuationOwnerSlot),true));
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
        finally { actions.Dispose(); events?.Dispose();events=null;nativeJournal?.Dispose();nativeJournal=null; }
        SceneManager.LoadScene("MainMenu");yield return null;Cleanup();
    }
    [Serializable] private sealed class InputRow { public float wall; public string action;public int visit; }
    [Serializable] private sealed class Result { public string outcome; public float wallSeconds;public int completedVisits,swaps,abilities,cards; }
    [Serializable] private sealed class NativeBoundary {public RunCombatSnapshot checkpoint;public RunReplayTape tape;}
    private static void RecordNative(RunCombatSnapshot checkpoint,RunReplayTape tape)
        => nativeJournal?.WriteLine(JsonUtility.ToJson(new NativeBoundary{checkpoint=checkpoint,tape=tape}));
    private static void Record(ResonanceTelemetryEvent e) => events?.WriteLine(JsonUtility.ToJson(e));
    private static void Log(string message,string trace,LogType kind)
    { if(kind==LogType.Error||kind==LogType.Exception) {failed=true;File.AppendAllText(Path.Combine(output,"errors.txt"),message+"\n"+trace+"\n");} }
    private static void Cleanup()
    { if(observed!=null){if(observed.Resonance!=null)observed.Resonance.Telemetry-=Record;
        if(observed.Continuation!=null)observed.Continuation.ValidationActionRecorded-=RecordNative;}observed=null;mastery?.Dispose();selection?.Dispose();profile?.Dispose();mastery=selection=profile=null; }
    private static void Finish()
    {
        EditorApplication.update-=Tick;Application.logMessageReceived-=Log;steps.Clear();events?.Dispose();events=null;
        nativeJournal?.Dispose();nativeJournal=null;Cleanup();SessionState.SetBool(Pending,false);Time.timeScale=1;
        File.WriteAllText(Path.Combine(output,"completion.txt"),failed?"FAILED — inspect errors":"Runner completed; inspect outcomes and sample counts. Proxies do not certify human enjoyment.");
        if(Application.isBatchMode)EditorApplication.Exit(failed?1:0);else EditorApplication.ExitPlaymode();
    }
}
#endif
