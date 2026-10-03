using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public sealed class BackgroundMusicPlayer : MonoBehaviour
{
    private const string SettingsResourcePath =
        "Audio/BackgroundMusicSettings";

    private static BackgroundMusicPlayer instance;

    private AudioSource audioSource;
    private BackgroundMusicSettings settings;
    private AudioSource fadingSource;
    private float fadeProgress = 1f, desiredVolume = .65f;
    private bool zoneMusic, manualPause, applicationPause, appliedPause;

    public static BackgroundMusicPlayer Instance =>
        instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSceneSubscription()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterForSceneLoads()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Install();
        if(instance!=null) instance.SetZoneMusic(RunSession.Current?.Zone?.Definition?.music);
    }

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.AfterSceneLoad
    )]
    private static void Install()
    {
        if (!Application.isPlaying ||
            instance != null ||
            SceneManager.GetActiveScene().path == StudioIdentPlayer.ScenePath)
        {
            return;
        }

        GameObject playerObject =
            new GameObject(
                "BackgroundMusicPlayer"
            );

        instance =
            playerObject.AddComponent<
                BackgroundMusicPlayer
            >();

        DontDestroyOnLoad(
            playerObject
        );
    }

    private void Awake()
    {
        if (instance != null &&
            instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        settings =
            Resources.Load<
                BackgroundMusicSettings
            >(
                SettingsResourcePath
            );

        audioSource =
            gameObject.AddComponent<
                AudioSource
            >();

        ConfigureAudioSource();
        desiredVolume=settings!=null?settings.Volume:.65f;
        AudioPreferences.Changed += RefreshMute;
        RefreshMute();
        TryStartMusic();
    }

    private void ConfigureAudioSource()
    {
        if (audioSource == null)
        {
            return;
        }

        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.spatialBlend = 0f;
        audioSource.dopplerLevel = 0f;
        audioSource.pitch = 1f;

        if (settings != null)
        {
            audioSource.volume =
                settings.Volume;
        }
    }

    private void TryStartMusic()
    {
        if (audioSource == null)
        {
            return;
        }

        if (settings == null)
        {
            Debug.LogWarning(
                "Background music settings could not be loaded from " +
                $"Resources/{SettingsResourcePath}."
            );

            return;
        }

        if (settings.MusicClip == null)
        {
            Debug.Log(
                "BackgroundMusicPlayer is ready. Assign a music clip to " +
                "Assets/_Game/Resources/Audio/BackgroundMusicSettings.asset " +
                "to enable looping background music."
            );

            return;
        }

        audioSource.clip =
            settings.MusicClip;

        audioSource.Play();
    }

    public void SetVolume(float volume)
    {
        if (audioSource == null)
        {
            return;
        }

        desiredVolume=Mathf.Clamp01(volume);
        ApplyVolumes();
    }

    public void PauseMusic()
    {
        manualPause=true;
        RefreshPause();
        if (audioSource == null ||
            !audioSource.isPlaying)
        {
            return;
        }

        audioSource.Pause();
    }

    public void ResumeMusic()
    {
        manualPause=false;
        RefreshPause();
        if (audioSource == null ||
            audioSource.clip == null)
        {
            return;
        }

        if(!appliedPause) audioSource.UnPause();
    }

    private void OnDestroy()
    {
        AudioPreferences.Changed -= RefreshMute;
        if (instance == this)
        {
            instance = null;
        }
    }

    public void SetZoneMusic(AudioClip clip)
    {
        zoneMusic=clip!=null;
        clip=clip!=null?clip:settings?.MusicClip;
        if(audioSource==null || clip==null || audioSource.clip==clip) { RefreshPause();return; }
        // One persistent player, at most one outgoing source during a fade.
        if(fadingSource==null)
        {
            fadingSource=gameObject.AddComponent<AudioSource>();
            fadingSource.playOnAwake=false;fadingSource.loop=true;fadingSource.spatialBlend=0;fadingSource.dopplerLevel=0;
        }
        fadingSource.Stop();fadingSource.clip=audioSource.clip;
        if(fadingSource.clip!=null)
        {
            fadingSource.timeSamples=audioSource.timeSamples;fadingSource.volume=audioSource.volume;
            fadingSource.Play();
        }
        audioSource.Stop();audioSource.clip=clip;fadeProgress=0;audioSource.volume=0;audioSource.Play();
        RefreshMute();appliedPause=false;RefreshPause();
    }
    private void Update()
    {
        RefreshPause();
        if(appliedPause || fadeProgress>=1) return;
        fadeProgress=Mathf.Min(1,fadeProgress+Time.unscaledDeltaTime/.75f);ApplyVolumes();
        if(fadeProgress>=1 && fadingSource!=null) { fadingSource.Stop();fadingSource.clip=null; }
    }
    private void ApplyVolumes()
    {
        if(audioSource!=null) audioSource.volume=desiredVolume*fadeProgress;
        if(fadingSource!=null) fadingSource.volume=desiredVolume*(1-fadeProgress);
    }
    private void RefreshPause()
    {
        bool pause=manualPause || applicationPause || (zoneMusic && Time.timeScale<=0);
        if(pause==appliedPause) return;
        appliedPause=pause;
        if(pause) { audioSource?.Pause();fadingSource?.Pause(); }
        else { audioSource?.UnPause();fadingSource?.UnPause(); }
    }
    private void OnApplicationPause(bool paused) { applicationPause=paused;RefreshPause(); }
    private void RefreshMute()
    {
        if(audioSource!=null) audioSource.mute=AudioPreferences.MusicMuted;
        if(fadingSource!=null) fadingSource.mute=AudioPreferences.MusicMuted;
    }
}
