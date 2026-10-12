using UnityEngine;

/// <summary>Serialized legacy clip fallback. The board supplies actual shatter
/// timing and grouping; no delayed sound survives pause or changes clear timing.</summary>
[DisallowMultipleComponent]
public sealed class GemBreakAudioController : MonoBehaviour
{
    [SerializeField] private BoardController boardController;
    [SerializeField] private AudioClip gemBreakClip;
    [SerializeField, Range(0f, 1f)] private float baseVolume = .72f;
    [SerializeField, Range(0f, .2f)] private float extraGemVolumeBonus = .035f;
    [SerializeField, Range(0f, 1f)] private float maximumVolume = .95f;
    [SerializeField, Min(0f)] private float semitonesPerCascade = 1f;
    [SerializeField, Min(0)] private int maximumCascadePitchSteps = 4;
    [SerializeField, Range(.5f, 2f)] private float maximumPitch = 1.26f;
    [SerializeField, Range(0f, .05f)] private float randomPitchVariation = .01f;
    [SerializeField, Range(1, 8)] private int audioSourceCount = 4;
    private AudioSource[] sources;
    private int nextSource;
    private long lastReceipt;
    private readonly System.Random random = new System.Random();

    private void OnEnable()
    {
        if (boardController == null) boardController = GetComponent<BoardController>();
        if (boardController != null) boardController.FeedbackPresented += OnFeedback;
        AudioPreferences.Changed += ApplyPreferences;
    }
    private void OnDisable()
    {
        if (boardController != null) boardController.FeedbackPresented -= OnFeedback;
        AudioPreferences.Changed -= ApplyPreferences;
        Silence();
    }
    private void OnFeedback(BoardFeedback feedback)
    {
        if (feedback.Kind != BoardFeedbackKind.MatchImpact || feedback.Id <= lastReceipt) return;
        lastReceipt = feedback.Id;
        PlayBreakSound(feedback.Count, feedback.Cascade);
    }
    private void PlayBreakSound(int count, int cascade)
    {
        if (!CombatAudioController.FeedbackAllowed) return;
        if (CombatAudioController.TryPlayMatch(count, cascade)) return;
        MobileHaptics.Request(HapticStrength.Tick, 1);
        MobileHaptics.Flush();
        if (AudioPreferences.SfxMuted || gemBreakClip == null) return;
        if (sources == null)
        {
            sources = new AudioSource[Mathf.Clamp(audioSourceCount, 1, 8)];
            for (int i = 0; i < sources.Length; i++)
            {
                var child = new GameObject("Fallback gem sound");
                child.transform.SetParent(transform, false);
                var source = child.AddComponent<AudioSource>();
                source.playOnAwake = false; source.loop = false;
                source.spatialBlend = 0; source.dopplerLevel = 0;
                sources[i] = source;
            }
        }
        var voice = sources[nextSource];
        nextSource = (nextSource + 1) % sources.Length;
        float pitch = Mathf.Pow(2f, Mathf.Clamp(cascade, 0, maximumCascadePitchSteps) * semitonesPerCascade / 12f);
        pitch += ((float)random.NextDouble() * 2f - 1f) * randomPitchVariation;
        voice.pitch = Mathf.Clamp(pitch, .5f, maximumPitch);
        voice.PlayOneShot(gemBreakClip, Mathf.Clamp(baseVolume + Mathf.Max(0, count - 3) * extraGemVolumeBonus, 0, maximumVolume));
    }
    private void Update() { if (!CombatAudioController.FeedbackAllowed) Silence(); }
    private void ApplyPreferences() { if (AudioPreferences.SfxMuted) Silence(); }
    private void OnApplicationFocus(bool focused) { if (!focused) Silence(); }
    private void OnApplicationPause(bool paused) { if (paused) Silence(); }
    private void Silence()
    {
        if (sources != null) foreach (var source in sources) if (source != null) source.Stop();
    }
}
