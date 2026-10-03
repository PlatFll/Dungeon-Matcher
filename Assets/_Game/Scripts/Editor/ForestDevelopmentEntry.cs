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
    { NewFixture(0); }
    [MenuItem("Dungeon Matcher/Forest/Fixtures/Scout solo")] public static void Scout() => NewFixture(3);
    [MenuItem("Dungeon Matcher/Forest/Fixtures/Trailguard solo")] public static void Trailguard() => NewFixture(4);
    [MenuItem("Dungeon Matcher/Forest/Fixtures/Mender solo")] public static void Mender() => NewFixture(5);
    [MenuItem("Dungeon Matcher/Forest/Fixtures/Rootbinder solo")] public static void Rootbinder() => NewFixture(6);
    [MenuItem("Dungeon Matcher/Forest/Fixtures/Warden solo")] public static void Warden() => NewFixture(7);
    [MenuItem("Dungeon Matcher/Forest/Fixtures/Matriarch solo")] public static void Matriarch() => NewFixture(8);
    [MenuItem("Dungeon Matcher/Forest/Fixtures/Rootbinder with attackers")] public static void Roots() => NewFixture(1);
    [MenuItem("Dungeon Matcher/Forest/Fixtures/Warden and Scout")] public static void WardenScout() => NewFixture(9);
    [MenuItem("Dungeon Matcher/Forest/Fixtures/Warden and Trailguard")] public static void WardenTrailguard() => NewFixture(10);
    [MenuItem("Dungeon Matcher/Forest/Fixtures/Matriarch with attackers")] public static void MatriarchAttackers() => NewFixture(11);
    [MenuItem("Dungeon Matcher/Forest/Fixtures/Matriarch with Rootbinder")] public static void MatriarchRoots() => NewFixture(12);
    private static void NewFixture(int offset)
    {
        if(EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play Mode before starting a separate test.");
        string path=Path.GetFullPath(".utmp/ForestPlaytest/"+Guid.NewGuid().ToString("N")+".json");
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path,JsonUtility.ToJson(new AccountSave{potions=3,bombs=3,equipPotions=true,equipBombs=true}));
        SessionState.SetInt(Key+".fixture",offset);EditorPrefs.SetString(Key,path);Begin();
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
        RunLaunchOptions.ForestEncounterOffset=SessionState.GetInt(Key+".fixture",0);
        RunLaunchOptions.ForestClockProfile=CombatClockSnapshot.HybridProfile;
    }
    private static void OnPlayState(PlayModeStateChange state)
    {
        if(state!=PlayModeStateChange.ExitingPlayMode) return;
        RunSession.Current?.Continuation?.SaveNow();profile?.Dispose();profile=null;
        EditorSceneManager.playModeStartScene=null;
    }
}
