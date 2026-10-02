using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>Editor-only entry and separate durable account; no player-facing mode.</summary>
[InitializeOnLoad]
public static class ForestDevelopmentEntry
{
    private static IDisposable profile;
    private static string Key => "DungeonMatcher.Forest."+Application.dataPath;
    static ForestDevelopmentEntry() { EditorApplication.playModeStateChanged+=OnPlayState; }
    [MenuItem("Dungeon Matcher/Forest/Play new isolated test")]
    public static void NewTest()
    {
        if(EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play Mode before starting a separate test.");
        string path=Path.GetFullPath(".utmp/ForestPlaytest/"+Guid.NewGuid().ToString("N")+".json");
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path,JsonUtility.ToJson(new AccountSave{potions=3,bombs=3,equipPotions=true,equipBombs=true}));
        EditorPrefs.SetString(Key,path);Begin();
    }
    [MenuItem("Dungeon Matcher/Forest/Resume isolated test")]
    public static void Resume()
    {
        if(!File.Exists(EditorPrefs.GetString(Key))) throw new InvalidOperationException("Start a new forest test first.");
        Begin();
    }
    private static void Begin()
    {
        if(EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play Mode first.");
        SessionState.SetBool(Key+".launch",true);
        EditorSceneManager.playModeStartScene=AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/_Game/Scenes/Game.unity");
        EditorApplication.isPlaying=true;
    }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Install()
    {
        if(!SessionState.GetBool(Key+".launch",false)) return;
        SessionState.SetBool(Key+".launch",false);
        profile=AccountProgression.UseDisposableProfile(EditorPrefs.GetString(Key));
        RunLaunchOptions.ForestPrototype=true;
    }
    private static void OnPlayState(PlayModeStateChange state)
    {
        if(state!=PlayModeStateChange.ExitingPlayMode) return;
        RunSession.Current?.Continuation?.SaveNow();profile?.Dispose();profile=null;
        EditorSceneManager.playModeStartScene=null;
    }
}
