using UnityEngine;

[CreateAssetMenu(
    fileName = "BackgroundMusicSettings",
    menuName = "Dungeon Matcher/Audio/Background Music Settings"
)]
public sealed class BackgroundMusicSettings : ScriptableObject
{
    [Header("Music")]
    [SerializeField]
    [Tooltip(
        "Background music clip played by the global music player. " +
        "Assign the loop-ready track here."
    )]
    private AudioClip musicClip;

    [SerializeField, Range(0f, 1f)]
    private float volume = 0.65f;

    public AudioClip MusicClip =>
        musicClip;

    public float Volume =>
        Mathf.Clamp01(volume);

    private void OnValidate()
    {
        volume =
            Mathf.Clamp01(volume);
    }
}
