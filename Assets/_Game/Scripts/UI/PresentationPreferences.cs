using UnityEngine;

public static class PresentationPreferences
{
    public const string ReducedMotionKey = "DungeonMatcher.Presentation.ReducedMotion";
    public static bool ReducedMotion => PlayerPrefs.GetInt(ReducedMotionKey, 0) != 0;
    public static void SetReducedMotion(bool value)
    {
        PlayerPrefs.SetInt(ReducedMotionKey, value ? 1 : 0);
        PlayerPrefs.Save();
    }
}
