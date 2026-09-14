using System;
using UnityEngine;

public static class AudioPreferences
{
    public static event Action Changed;
    public static bool MusicMuted => PlayerPrefs.GetInt("DungeonMatcher.Audio.MusicMuted", 0) != 0;
    public static bool SfxMuted => PlayerPrefs.GetInt("DungeonMatcher.Audio.SfxMuted", 0) != 0;
    public static void SetMusicMuted(bool muted) { PlayerPrefs.SetInt("DungeonMatcher.Audio.MusicMuted", muted ? 1 : 0); PlayerPrefs.Save(); Changed?.Invoke(); }
    public static void SetSfxMuted(bool muted) { PlayerPrefs.SetInt("DungeonMatcher.Audio.SfxMuted", muted ? 1 : 0); PlayerPrefs.Save(); Changed?.Invoke(); }
}
