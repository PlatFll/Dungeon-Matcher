using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class GemBreakAudioController :
    MonoBehaviour
{
    [Header("Board")]
    [SerializeField]
    private BoardController boardController;

    [Header("Sound")]
    [SerializeField]
    private AudioClip gemBreakClip;

    [SerializeField, Min(0f)]
    [Tooltip(
        "Delay used to synchronize the sound with " +
        "the gem's full-white flash peak."
    )]
    private float breakSoundDelay = 0.05f;

    [Header("Volume")]
    [SerializeField, Range(0f, 1f)]
    private float baseVolume = 0.72f;

    [SerializeField, Range(0f, 0.2f)]
    private float extraGemVolumeBonus = 0.035f;

    [SerializeField, Range(0f, 1f)]
    private float maximumVolume = 0.95f;

    [Header("Cascade Pitch")]
    [SerializeField, Min(0f)]
    private float semitonesPerCascade = 1f;

    [SerializeField, Min(0)]
    private int maximumCascadePitchSteps = 4;

    [SerializeField, Range(0.5f, 2f)]
    private float maximumPitch = 1.26f;

    [SerializeField, Range(0f, 0.05f)]
    private float randomPitchVariation = 0.01f;

    [Header("Audio Source Pool")]
    [SerializeField, Range(1, 8)]
    private int audioSourceCount = 4;

    private AudioSource[] audioSources;
    private int nextAudioSourceIndex;

    /*
     * MatchResolved can be invoked multiple times in one frame
     * when separate match groups clear simultaneously.
     *
     * These values combine them into one break sound.
     */
    private bool hasPendingBreak;
    private int pendingGemCount;
    private int pendingCascadeDepth;

    private void Awake()
    {
        ResolveReferences();
        CreateAudioSourcePool();
    }

    private void OnEnable()
    {
        ResolveReferences();
        Subscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();

        hasPendingBreak = false;
        pendingGemCount = 0;
        pendingCascadeDepth = 0;
    }

    private void ResolveReferences()
    {
        if (boardController == null)
        {
            boardController =
                GetComponent<BoardController>();
        }
    }

    private void Subscribe()
    {
        if (boardController == null)
        {
            return;
        }

        boardController.BoardMatchResolved -=
            HandleMatchResolved;

        boardController.BoardMatchResolved +=
            HandleMatchResolved;
    }

    private void Unsubscribe()
    {
        if (boardController == null)
        {
            return;
        }

        boardController.BoardMatchResolved -=
            HandleMatchResolved;
    }

    private void HandleMatchResolved(
        BoardMatchContext context)
    {
        /*
         * Multiple match groups reported during the same
         * clear step are combined into one sound.
         */
        hasPendingBreak = true;

        pendingGemCount +=
            context.GemCount;

        pendingCascadeDepth =
            Mathf.Max(
                pendingCascadeDepth,
                context.CascadeDepth
            );
    }

    private void LateUpdate()
    {
        if (!hasPendingBreak)
        {
            return;
        }

        int gemCount =
            pendingGemCount;

        int cascadeDepth =
            pendingCascadeDepth;

        hasPendingBreak = false;
        pendingGemCount = 0;
        pendingCascadeDepth = 0;

        StartCoroutine(
            PlayBreakSoundAfterDelay(
                gemCount,
                cascadeDepth
            )
        );
    }

    private IEnumerator PlayBreakSoundAfterDelay(
        int gemCount,
        int cascadeDepth)
    {
        if (breakSoundDelay > 0f)
        {
            yield return new WaitForSeconds(
                breakSoundDelay
            );
        }

        PlayBreakSound(
            gemCount,
            cascadeDepth
        );
    }

    private void PlayBreakSound(
        int gemCount,
        int cascadeDepth)
    {
        if (gemBreakClip == null ||
            audioSources == null ||
            audioSources.Length == 0)
        {
            return;
        }

        AudioSource source =
            audioSources[
                nextAudioSourceIndex
            ];

        nextAudioSourceIndex =
            (
                nextAudioSourceIndex + 1
            ) %
            audioSources.Length;

        int pitchStep =
            Mathf.Clamp(
                cascadeDepth,
                0,
                maximumCascadePitchSteps
            );

        float semitoneOffset =
            pitchStep *
            semitonesPerCascade;

        /*
         * Converts musical semitones into Unity pitch.
         */
        float pitch =
            Mathf.Pow(
                2f,
                semitoneOffset / 12f
            );

        pitch +=
            Random.Range(
                -randomPitchVariation,
                randomPitchVariation
            );

        source.pitch =
            Mathf.Clamp(
                pitch,
                0.5f,
                maximumPitch
            );

        int extraGemCount =
            Mathf.Max(
                0,
                gemCount - 3
            );

        float volume =
            baseVolume +
            extraGemCount *
            extraGemVolumeBonus;

        volume =
            Mathf.Clamp(
                volume,
                0f,
                maximumVolume
            );

        source.PlayOneShot(
            gemBreakClip,
            volume
        );
    }

    private void CreateAudioSourcePool()
    {
        audioSourceCount =
            Mathf.Clamp(
                audioSourceCount,
                1,
                8
            );

        audioSources =
            new AudioSource[
                audioSourceCount
            ];

        for (int index = 0;
             index < audioSourceCount;
             index++)
        {
            GameObject sourceObject =
                new GameObject(
                    $"Gem Break Audio {index + 1}"
                );

            sourceObject.transform.SetParent(
                transform,
                false
            );

            AudioSource source =
                sourceObject.AddComponent<
                    AudioSource
                >();

            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f;
            source.dopplerLevel = 0f;

            audioSources[index] =
                source;
        }
    }

    private void OnValidate()
    {
        breakSoundDelay =
            Mathf.Max(
                0f,
                breakSoundDelay
            );

        baseVolume =
            Mathf.Clamp01(
                baseVolume
            );

        maximumVolume =
            Mathf.Clamp01(
                maximumVolume
            );

        maximumVolume =
            Mathf.Max(
                baseVolume,
                maximumVolume
            );

        semitonesPerCascade =
            Mathf.Max(
                0f,
                semitonesPerCascade
            );

        maximumCascadePitchSteps =
            Mathf.Max(
                0,
                maximumCascadePitchSteps
            );

        maximumPitch =
            Mathf.Clamp(
                maximumPitch,
                0.5f,
                2f
            );

        audioSourceCount =
            Mathf.Clamp(
                audioSourceCount,
                1,
                8
            );
    }
}