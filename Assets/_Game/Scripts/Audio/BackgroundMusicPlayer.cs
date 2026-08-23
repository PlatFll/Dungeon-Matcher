using UnityEngine;

[DisallowMultipleComponent]
public sealed class BackgroundMusicPlayer : MonoBehaviour
{
    private const string SettingsResourcePath =
        "Audio/BackgroundMusicSettings";

    private static BackgroundMusicPlayer instance;

    private AudioSource audioSource;
    private BackgroundMusicSettings settings;

    public static BackgroundMusicPlayer Instance =>
        instance;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.AfterSceneLoad
    )]
    private static void Install()
    {
        if (!Application.isPlaying ||
            instance != null)
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

        audioSource.volume =
            Mathf.Clamp01(volume);
    }

    public void PauseMusic()
    {
        if (audioSource == null ||
            !audioSource.isPlaying)
        {
            return;
        }

        audioSource.Pause();
    }

    public void ResumeMusic()
    {
        if (audioSource == null ||
            audioSource.clip == null)
        {
            return;
        }

        audioSource.UnPause();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
