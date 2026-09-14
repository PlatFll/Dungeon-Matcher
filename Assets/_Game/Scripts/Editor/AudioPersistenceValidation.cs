using System;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>Two separate editor processes verify actual PlayerPrefs persistence and restore original values.</summary>
public static class AudioPersistenceValidation
{
    private const string Music="DungeonMatcher.Audio.MusicMuted",Sfx="DungeonMatcher.Audio.SfxMuted";
    private static string FilePath=>Path.GetFullPath(".utmp/audio-persistence.json");
    [Serializable] private sealed class Snapshot { public bool hadMusic,hadSfx,music,sfx,restored; }
    public static void Write()
    {
        if(File.Exists(FilePath)&&!JsonUtility.FromJson<Snapshot>(File.ReadAllText(FilePath)).restored)
            throw new InvalidOperationException("Restore the previous audio test with ReadAndRestore before writing another.");
        var prior=new Snapshot{hadMusic=PlayerPrefs.HasKey(Music),hadSfx=PlayerPrefs.HasKey(Sfx),music=AudioPreferences.MusicMuted,sfx=AudioPreferences.SfxMuted};
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath));File.WriteAllText(FilePath,JsonUtility.ToJson(prior));
        AudioPreferences.SetMusicMuted(!prior.music);AudioPreferences.SetSfxMuted(!prior.sfx);
        if(AudioPreferences.MusicMuted==prior.music||AudioPreferences.SfxMuted==prior.sfx)throw new Exception("Audio write failed.");
        Debug.Log("Audio persistence stage 1 saved independent values. Start a fresh Unity process with ReadAndRestore.");
    }
    public static void ReadAndRestore()
    {
        var prior=JsonUtility.FromJson<Snapshot>(File.ReadAllText(FilePath));bool ok=false;
        try
        {
            if(AudioPreferences.MusicMuted==prior.music||AudioPreferences.SfxMuted==prior.sfx)throw new Exception("Audio settings did not survive process restart.");
            AudioPreferences.SetMusicMuted(prior.music);
            if(AudioPreferences.SfxMuted==prior.sfx)throw new Exception("Changing music also changed SFX.");
            ok=true;
        }
        finally
        {
            if(prior.hadMusic)PlayerPrefs.SetInt(Music,prior.music?1:0);else PlayerPrefs.DeleteKey(Music);
            if(prior.hadSfx)PlayerPrefs.SetInt(Sfx,prior.sfx?1:0);else PlayerPrefs.DeleteKey(Sfx);
            PlayerPrefs.Save();prior.restored=true;File.WriteAllText(FilePath,JsonUtility.ToJson(prior));
        }
        if(ok)File.WriteAllText(".utmp/audio-persistence-report.txt","PASS: two independent mute settings survived a complete Unity process restart; changing music left SFX unchanged. Original keys/values restored.\n");
    }
}
