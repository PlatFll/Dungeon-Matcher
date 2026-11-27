using UnityEngine;

public enum MenuCue { None, Press, Page, Socket, Coin, Level, Release, Denied }

/// <summary>Menu-only playback. Tails can finish during navigation. Combat and music keep their own routing.</summary>
public sealed class MenuSoundPlayer : MonoBehaviour
{
    private static MenuSoundPlayer instance;
    private AudioSource[] voices;
    private AudioClip[] clips;
    private AudioClip alternatePress;
    private bool alternate, suspended;
    private int nextVoice;

    public static void Play(MenuCue cue)
    {
        if (cue == MenuCue.None || !Application.isPlaying || AudioPreferences.SfxMuted) return;
        if (!instance)
        {
            var root = new GameObject("Menu sound");
            DontDestroyOnLoad(root);
            instance = root.AddComponent<MenuSoundPlayer>();
        }
        instance.PlayCue(cue);
    }

    private void Awake()
    {
        var values = (MenuCue[])System.Enum.GetValues(typeof(MenuCue));
        clips = new AudioClip[values.Length];
        foreach (var cue in values)
            if (cue != MenuCue.None) clips[(int)cue] = Resources.Load<AudioClip>("Audio/Menu/" + cue);
        alternatePress = Resources.Load<AudioClip>("Audio/Menu/PressAlt");
        voices = new AudioSource[3];
        for (int i = 0; i < voices.Length; i++)
        {
            var voice = voices[i] = gameObject.AddComponent<AudioSource>();
            voice.playOnAwake = false;
            voice.spatialBlend = 0;
            voice.volume = .38f;
        }
        AudioPreferences.Changed += ApplyPreferences;
    }

    private void PlayCue(MenuCue cue)
    {
        if (suspended) return;
        var clip = clips[(int)cue];
        if (cue == MenuCue.Press)
        {
            if (alternate && alternatePress) clip = alternatePress;
            alternate = !alternate;
        }
        if (!clip) return;
        var voice = voices[nextVoice];
        nextVoice = (nextVoice + 1) % voices.Length;
        voice.clip = clip;
        voice.Play();
    }

    private void ApplyPreferences()
    {
        if (suspended || AudioPreferences.SfxMuted)
            foreach (var voice in voices) voice.Stop();
    }

    private void OnApplicationPause(bool paused)
    {
        suspended = paused;
        ApplyPreferences();
    }

    private void OnDestroy()
    {
        AudioPreferences.Changed -= ApplyPreferences;
        if (instance == this) instance = null;
    }
}
