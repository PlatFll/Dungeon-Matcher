using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Batch entry point wrapping existing production regression suites without
// changing their fixtures or the gameplay implementation.
[InitializeOnLoad]
public static class GameplayPresentationRegressionRunner
{
    private const string Pending = "DungeonMatcher.PresentationRegression";
    static GameplayPresentationRegressionRunner()
    {
        EditorApplication.playModeStateChanged += state =>
        {
            if (state != PlayModeStateChange.EnteredEditMode || !SessionState.GetBool(Pending, false)) return;
            SessionState.SetBool(Pending, false);
            string log = File.ReadAllText("Logs/GameplayEdgeCaseValidation.log");
            bool passed = log.Contains("PASSED") && !log.Contains("FAILED");
            if (Application.isBatchMode) EditorApplication.Exit(passed ? 0 : 1);
        };
    }

    public static void Run()
    {
        RoyalMilestoneValidation.Run();
        SiegeSergeantValidation.Run();
        GameplayEdgeCaseValidation.RunEncounterHistory();
        EditorSceneManager.OpenScene("Assets/_Game/Scenes/Game.unity");
        SessionState.SetBool(Pending, true);
        GameplayEdgeCaseValidation.Run();
    }

    public static void RunSupplementary()
    {
        EditorSceneManager.OpenScene("Assets/_Game/Scenes/Game.unity");
        SessionState.SetBool(Pending, true);
        GameplayEdgeCaseValidation.RunSupplementary();
    }
}
