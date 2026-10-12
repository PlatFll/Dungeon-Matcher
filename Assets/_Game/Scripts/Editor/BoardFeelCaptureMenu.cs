using System.IO;
using UnityEditor;
using UnityEngine;

public static class BoardFeelCaptureMenu
{
    [MenuItem("Dungeon Matcher/Validation/Start Board Feel Capture")]
    private static void Start()
    {
        var board = Object.FindFirstObjectByType<BoardController>();
        if (!Application.isPlaying || board == null) { Debug.LogWarning("Start a game in Play Mode first."); return; }
        var capture = board.GetComponent<BoardFeelMetrics>();
        if (capture == null) board.gameObject.AddComponent<BoardFeelMetrics>();
        else capture.enabled = true;
    }
    [MenuItem("Dungeon Matcher/Validation/Save Board Feel Capture")]
    private static void Save()
    {
        var capture = Object.FindFirstObjectByType<BoardFeelMetrics>();
        if (capture == null) { Debug.LogWarning("Start a board feel capture first."); return; }
        string path = Path.GetFullPath(".utmp/BoardFeel2/manual.csv");
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path, capture.ExportCsv());
        capture.enabled = false;
        Debug.Log("Board feel capture: " + path);
    }
}
