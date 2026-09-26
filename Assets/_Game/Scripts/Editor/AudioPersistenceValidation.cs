using System;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>Two separate editor processes verify actual PlayerPrefs persistence and restore original values.</summary>
public static class AudioPersistenceValidation
{
    private const string Music="DungeonMatcher.Audio.MusicMuted",Sfx="DungeonMatcher.Audio.SfxMuted";
    private static string FilePath=>Path.GetFullPath(".utmp/audio-persistence.json");
    [Serializable] private sealed class Snapshot { public int version; public bool hadMusic,hadSfx,hadVibration,music,sfx,vibration,restored; }
    public static void Write()
    {
        if(File.Exists(FilePath)&&!JsonUtility.FromJson<Snapshot>(File.ReadAllText(FilePath)).restored)
            throw new InvalidOperationException("Restore the previous audio test with ReadAndRestore before writing another.");
        var prior=new Snapshot{version=2,hadMusic=PlayerPrefs.HasKey(Music),hadSfx=PlayerPrefs.HasKey(Sfx),hadVibration=PlayerPrefs.HasKey(AudioPreferences.VibrationMutedKey),music=AudioPreferences.MusicMuted,sfx=AudioPreferences.SfxMuted,vibration=AudioPreferences.VibrationMuted};
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath));File.WriteAllText(FilePath,JsonUtility.ToJson(prior));
        AudioPreferences.SetMusicMuted(!prior.music);AudioPreferences.SetSfxMuted(!prior.sfx);
        AudioPreferences.SetVibrationMuted(!prior.vibration);
        if(AudioPreferences.MusicMuted==prior.music||AudioPreferences.SfxMuted==prior.sfx||AudioPreferences.VibrationMuted==prior.vibration)throw new Exception("Audio write failed.");
        Debug.Log("Audio persistence stage 1 saved independent values. Start a fresh Unity process with ReadAndRestore.");
    }
    public static void ReadAndRestore()
    {
        var prior=JsonUtility.FromJson<Snapshot>(File.ReadAllText(FilePath));bool ok=false;
        try
        {
            if(AudioPreferences.MusicMuted==prior.music||AudioPreferences.SfxMuted==prior.sfx||(prior.version>=2&&AudioPreferences.VibrationMuted==prior.vibration))throw new Exception("Audio settings did not survive process restart.");
            AudioPreferences.SetMusicMuted(prior.music);
            if(AudioPreferences.SfxMuted==prior.sfx)throw new Exception("Changing music also changed SFX.");
            if(prior.version>=2&&AudioPreferences.VibrationMuted==prior.vibration)throw new Exception("Changing music also changed vibration.");
            ok=true;
        }
        finally
        {
            if(prior.hadMusic)PlayerPrefs.SetInt(Music,prior.music?1:0);else PlayerPrefs.DeleteKey(Music);
            if(prior.hadSfx)PlayerPrefs.SetInt(Sfx,prior.sfx?1:0);else PlayerPrefs.DeleteKey(Sfx);
            if(prior.version>=2) { if(prior.hadVibration)PlayerPrefs.SetInt(AudioPreferences.VibrationMutedKey,prior.vibration?1:0);else PlayerPrefs.DeleteKey(AudioPreferences.VibrationMutedKey); }
            PlayerPrefs.Save();prior.restored=true;File.WriteAllText(FilePath,JsonUtility.ToJson(prior));
        }
        if(ok)File.WriteAllText(".utmp/audio-persistence-report.txt",prior.version>=2 ? "PASS: independent music, SFX and vibration settings survived a complete Unity process restart; changing music left SFX/vibration unchanged. Original keys/values restored.\n" : "PASS: legacy music/SFX persistence check restored; vibration was not tested or modified.\n");
    }
}
