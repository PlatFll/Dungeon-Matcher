using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoardController))]
public sealed class PoisonBombVFXController : MonoBehaviour
{
    [Header("Poison Burst Animation")]
    [SerializeField]
    [Tooltip(
        "Ordered poison explosion frames. Slice the 384x64 sheet into six " +
        "64x64 sprites and assign them from ignition through final particles."
    )]
    private Sprite[] poisonExplosionFrames =
        new Sprite[6];

    [SerializeField, Min(0.01f)]
    [Tooltip("Seconds displayed per poison explosion frame.")]
    private float frameDuration = 0.055f;

    [SerializeField, Range(1f, 4f)]
    [Tooltip(
        "Maximum rendered size of the main explosion relative to one board cell."
    )]
    private float burstSizeInCells = 3f;

    [SerializeField]
    private int burstSortingOrder = 12;

    [Header("Lingering Poison Residue")]
    [SerializeField]
    [Tooltip(
        "Presentation-only residue after the main burst. It never blocks the " +
        "board and disappears from each cell as a refill gem visibly lands."
    )]
    private bool enableResidue = true;

    [SerializeField]
    [Tooltip(
        "Optional dedicated residue sprite. When empty, the last usable burst " +
        "frame is reused at a small translucent size."
    )]
    private Sprite poisonResidueSprite;

    [SerializeField, Range(0f, 0.6f)]
    private float residueDuration = 0.25f;

    [SerializeField, Range(0f, 1f)]
    private float residueAlpha = 0.35f;

    [SerializeField, Range(0.2f, 1.25f)]
    private float residueSizeInCells = 0.65f;

    [SerializeField]
    [Tooltip(
        "Residue renders behind normal gems so a landing gem always wins visually."
    )]
    private int residueSortingOrder = -1;

    [Header("Pooling")]
    [SerializeField, Min(0)]
    private int prewarmCount = 6;

    [SerializeField, Min(1)]
    private int maximumEffectCount = 32;

    private const string SortingLayerName = "Gems";

    private BoardController boardController;

    private readonly Queue<PoisonBombBurstVFX>
        availableEffects =
            new Queue<PoisonBombBurstVFX>();

    private readonly HashSet<PoisonBombBurstVFX>
        activeEffects =
            new HashSet<PoisonBombBurstVFX>();

    private int createdEffectCount;

    public bool HasUsablePoisonBurstFrames
    {
        get
        {
            if (poisonExplosionFrames == null ||
                poisonExplosionFrames.Length == 0)
            {
                return false;
            }

            for (int index = 0;
                 index < poisonExplosionFrames.Length;
                 index++)
            {
                if (poisonExplosionFrames[index] != null)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public float MainBurstDuration =>
        poisonExplosionFrames == null
            ? 0f
            : poisonExplosionFrames.Length *
              Mathf.Max(0.01f, frameDuration);

    private void Awake()
    {
        boardController =
            GetComponent<BoardController>();

        PrewarmPool();
    }

    private void OnEnable()
    {
        Subscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
        StopAllCoroutines();
        StopAllEffectsImmediately();
    }

    private void Subscribe()
    {
        if (boardController == null)
        {
            return;
        }

        boardController.BombVFXRequested -=
            HandleBombVFXRequested;

        boardController.BombVFXRequested +=
            HandleBombVFXRequested;
    }

    private void Unsubscribe()
    {
        if (boardController == null)
        {
            return;
        }

        boardController.BombVFXRequested -=
            HandleBombVFXRequested;
    }

    private void HandleBombVFXRequested(
        BombVFXContext context)
    {
        if (context.SpecialType !=
                GemSpecialType.PoisonBomb ||
            !HasUsablePoisonBurstFrames)
        {
            return;
        }

        if (context.StartDelay <= 0f)
        {
            PlayPoisonBombVFX(context);
            return;
        }

        StartCoroutine(
            PlayAfterDelay(context)
        );
    }

    private IEnumerator PlayAfterDelay(
        BombVFXContext context)
    {
        yield return new WaitForSeconds(
            context.StartDelay
        );

        if (!isActiveAndEnabled)
        {
            yield break;
        }

        PlayPoisonBombVFX(context);
    }

    private void PlayPoisonBombVFX(
        BombVFXContext context)
    {
        if (boardController == null ||
            !HasUsablePoisonBurstFrames)
        {
            return;
        }

        PoisonBombBurstVFX effect =
            GetAvailableEffect();

        if (effect == null)
        {
            return;
        }

        activeEffects.Add(effect);

        effect.ConfigureRendering(
            SortingLayerName,
            burstSortingOrder,
            residueSortingOrder
        );

        effect.Play(
            boardController,
            context,
            poisonExplosionFrames,
            frameDuration,
            burstSizeInCells,
            enableResidue,
            poisonResidueSprite,
            residueDuration,
            residueAlpha,
            residueSizeInCells,
            ReleaseEffect
        );
    }

    private PoisonBombBurstVFX GetAvailableEffect()
    {
        while (availableEffects.Count > 0)
        {
            PoisonBombBurstVFX effect =
                availableEffects.Dequeue();

            if (effect != null)
            {
                return effect;
            }
        }

        if (createdEffectCount >=
            maximumEffectCount)
        {
            return null;
        }

        return CreateEffect();
    }

    private PoisonBombBurstVFX CreateEffect()
    {
        GameObject effectObject =
            new GameObject("PoisonBombBurstVFX");

        effectObject.transform.SetParent(
            transform,
            false
        );

        PoisonBombBurstVFX effect =
            effectObject.AddComponent<
                PoisonBombBurstVFX
            >();

        effect.ConfigureRendering(
            SortingLayerName,
            burstSortingOrder,
            residueSortingOrder
        );

        effectObject.SetActive(false);
        createdEffectCount++;

        return effect;
    }

    private void ReleaseEffect(
        PoisonBombBurstVFX effect)
    {
        if (effect == null ||
            !activeEffects.Remove(effect))
        {
            return;
        }

        effect.StopImmediately();
        effect.gameObject.SetActive(false);

        effect.transform.SetParent(
            transform,
            false
        );

        availableEffects.Enqueue(effect);
    }

    private void PrewarmPool()
    {
        int effectsToCreate =
            Mathf.Min(
                prewarmCount,
                maximumEffectCount
            );

        for (int index = 0;
             index < effectsToCreate;
             index++)
        {
            availableEffects.Enqueue(
                CreateEffect()
            );
        }
    }

    private void StopAllEffectsImmediately()
    {
        List<PoisonBombBurstVFX> snapshot =
            new List<PoisonBombBurstVFX>(
                activeEffects
            );

        foreach (PoisonBombBurstVFX effect
                 in snapshot)
        {
            if (effect == null)
            {
                continue;
            }

            effect.StopImmediately();
            effect.gameObject.SetActive(false);
            activeEffects.Remove(effect);
            availableEffects.Enqueue(effect);
        }
    }

    private void OnValidate()
    {
        frameDuration =
            Mathf.Max(0.01f, frameDuration);

        burstSizeInCells =
            Mathf.Clamp(
                burstSizeInCells,
                1f,
                4f
            );

        residueDuration =
            Mathf.Clamp(
                residueDuration,
                0f,
                0.6f
            );

        residueAlpha =
            Mathf.Clamp01(residueAlpha);

        residueSizeInCells =
            Mathf.Clamp(
                residueSizeInCells,
                0.2f,
                1.25f
            );

        maximumEffectCount =
            Mathf.Max(1, maximumEffectCount);

        prewarmCount =
            Mathf.Clamp(
                prewarmCount,
                0,
                maximumEffectCount
            );
    }
}
