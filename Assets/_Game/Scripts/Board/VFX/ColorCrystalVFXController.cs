using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoardController))]
public sealed class ColorCrystalVFXController :
    MonoBehaviour
{
    [Header("Target Star Glints")]
    [SerializeField, Min(0f)]
    private float glintSpawnStagger =
        0.02f;

    [SerializeField, Min(0.06f)]
    private float glintDuration =
        0.15f;

    [SerializeField, Min(0.041f)]
    private float glintPeakTime =
        0.09f;

    [SerializeField, Min(0f)]
    private float targetFlashDuration =
        0.04f;

    [SerializeField, Min(0.01f)]
    private float glintSizeInCells =
        0.34f;

    [SerializeField]
    private float glintRotationDegrees =
        32f;

    [SerializeField]
    private Color glintColor =
        new Color(
            0.86f,
            0.72f,
            1f,
            1f
        );

    [Header("Rendering")]
    [SerializeField]
    private int sortingOrder = 6;

    [Header("Pooling")]
    [SerializeField, Min(0)]
    private int prewarmCount = 32;

    [SerializeField, Min(1)]
    private int maximumGlintCount = 64;

    private BoardController boardController;

    private Texture2D runtimeStarTexture;
    private Sprite runtimeStarSprite;

    private readonly Queue<ColorCrystalGlintVFX>
        availableGlints =
            new Queue<ColorCrystalGlintVFX>();

    private readonly HashSet<ColorCrystalGlintVFX>
        activeGlints =
            new HashSet<ColorCrystalGlintVFX>();

    private int createdGlintCount;

    /*
     * Counts target-launch sequences rather than active glints.
     * BoardController uses this to know when every selected gem
     * has received its visual cue and the temporary fast-clear
     * timings can safely be restored.
     */
    private int activeTargetLaunchSequences;

    private const string SortingLayerName =
        "Gems";

    public float TargetSpawnStagger =>
        glintSpawnStagger;

    public bool IsTargetLaunchSequenceActive =>
        activeTargetLaunchSequences > 0;

    private void Awake()
    {
        boardController =
            GetComponent<BoardController>();

        CreateRuntimeStarSprite();
        PrewarmPool();
    }

    public IEnumerator PlayActivation(
        ColorCrystalVFXContext context)
    {
        if (!context.IsValid ||
            boardController == null)
        {
            yield break;
        }

        yield return PlayCrystalPulse(
            context.CrystalGem
        );

        int spawnedGlintCount = 0;

        for (int index = 0;
             index < context.TargetGems.Length;
             index++)
        {
            Gem targetGem =
                context.TargetGems[index];

            if (!TryPlayTargetGlint(
                    targetGem))
            {
                continue;
            }

            spawnedGlintCount++;

            if (glintSpawnStagger > 0f &&
                index <
                    context.TargetGems.Length - 1)
            {
                yield return new WaitForSeconds(
                    glintSpawnStagger
                );
            }
        }

        /*
         * Resume gameplay when the final glint reaches its
         * brightest/large state. Its short shrink tail can finish
         * while the crystal's normal clear animation begins.
         */
        if (spawnedGlintCount > 0 &&
            glintPeakTime > 0f)
        {
            yield return new WaitForSeconds(
                glintPeakTime
            );
        }
    }

    /*
     * Synchronized mode is used by the board when the selected
     * gems should begin clearing at the same travelling cadence
     * as the star glints instead of waiting for every glint first.
     *
     * The crystal anticipation is still awaited. Target glints are
     * then launched in a background sequence after a tiny lead-in,
     * allowing the existing board clear coroutine to advance in
     * parallel without giving VFX ownership of gameplay state.
     */
    public IEnumerator PlaySynchronizedActivation(
        ColorCrystalVFXContext context,
        float targetStartDelay)
    {
        if (!context.IsValid ||
            boardController == null)
        {
            yield break;
        }

        yield return PlayCrystalPulse(
            context.CrystalGem
        );

        StartCoroutine(
            PlayTargetGlintSequence(
                context.TargetGems,
                Mathf.Max(
                    0f,
                    targetStartDelay
                )
            )
        );
    }

    private IEnumerator PlayCrystalPulse(
        Gem crystalGem)
    {
        if (crystalGem == null)
        {
            yield break;
        }

        /*
         * Anticipation belongs to the already-selected crystal.
         * It does not decide targets or alter board state.
         */
        yield return
            crystalGem
                .PlayColorCrystalActivationPulse();
    }

    private IEnumerator PlayTargetGlintSequence(
        Gem[] targetGems,
        float startDelay)
    {
        if (targetGems == null ||
            targetGems.Length == 0)
        {
            yield break;
        }

        activeTargetLaunchSequences++;

        if (startDelay > 0f)
        {
            yield return new WaitForSeconds(
                startDelay
            );
        }

        for (int index = 0;
             index < targetGems.Length;
             index++)
        {
            TryPlayTargetGlint(
                targetGems[index]
            );

            if (glintSpawnStagger > 0f &&
                index <
                    targetGems.Length - 1)
            {
                yield return new WaitForSeconds(
                    glintSpawnStagger
                );
            }
        }

        activeTargetLaunchSequences =
            Mathf.Max(
                0,
                activeTargetLaunchSequences - 1
            );
    }

    private bool TryPlayTargetGlint(
        Gem targetGem)
    {
        if (targetGem == null ||
            boardController == null)
        {
            return false;
        }

        ColorCrystalGlintVFX glint =
            GetAvailableGlint();

        if (glint == null)
        {
            return false;
        }

        Vector3 localPosition =
            boardController.transform
                .InverseTransformPoint(
                    targetGem.transform.position
                );

        activeGlints.Add(
            glint
        );

        glint.Play(
            targetGem,
            localPosition,
            boardController.CellSize *
                glintSizeInCells,
            glintDuration,
            glintPeakTime,
            targetFlashDuration,
            glintRotationDegrees,
            glintColor,
            ReleaseGlint
        );

        return true;
    }

    private ColorCrystalGlintVFX
        GetAvailableGlint()
    {
        while (availableGlints.Count > 0)
        {
            ColorCrystalGlintVFX glint =
                availableGlints.Dequeue();

            if (glint != null)
            {
                return glint;
            }
        }

        if (createdGlintCount >=
            maximumGlintCount)
        {
            return null;
        }

        return CreateGlint();
    }

    private ColorCrystalGlintVFX CreateGlint()
    {
        GameObject glintObject =
            new GameObject(
                "ColorCrystalStarGlint"
            );

        glintObject.transform.SetParent(
            boardController.transform,
            false
        );

        glintObject.AddComponent<
            SpriteRenderer
        >();

        ColorCrystalGlintVFX glint =
            glintObject.AddComponent<
                ColorCrystalGlintVFX
            >();

        glint.ConfigureRendering(
            runtimeStarSprite,
            SortingLayerName,
            sortingOrder
        );

        glintObject.SetActive(false);

        createdGlintCount++;

        return glint;
    }

    private void ReleaseGlint(
        ColorCrystalGlintVFX glint)
    {
        if (glint == null ||
            !activeGlints.Remove(glint))
        {
            return;
        }

        glint.gameObject.SetActive(false);

        glint.transform.SetParent(
            boardController.transform,
            false
        );

        availableGlints.Enqueue(
            glint
        );
    }

    private void PrewarmPool()
    {
        int amountToCreate =
            Mathf.Min(
                prewarmCount,
                maximumGlintCount
            );

        for (int index = 0;
             index < amountToCreate;
             index++)
        {
            availableGlints.Enqueue(
                CreateGlint()
            );
        }
    }

    private void CreateRuntimeStarSprite()
    {
        const int textureSize = 9;

        runtimeStarTexture =
            new Texture2D(
                textureSize,
                textureSize,
                TextureFormat.RGBA32,
                false
            );

        runtimeStarTexture.name =
            "Runtime Color Crystal Star Glint";

        runtimeStarTexture.filterMode =
            FilterMode.Point;

        runtimeStarTexture.wrapMode =
            TextureWrapMode.Clamp;

        Color[] pixels =
            new Color[
                textureSize *
                textureSize
            ];

        for (int index = 0;
             index < pixels.Length;
             index++)
        {
            pixels[index] =
                Color.clear;
        }

        PaintHorizontalSpan(
            pixels,
            textureSize,
            0,
            4,
            4
        );

        PaintHorizontalSpan(
            pixels,
            textureSize,
            1,
            4,
            4
        );

        PaintHorizontalSpan(
            pixels,
            textureSize,
            2,
            3,
            5
        );

        PaintHorizontalSpan(
            pixels,
            textureSize,
            3,
            2,
            6
        );

        PaintHorizontalSpan(
            pixels,
            textureSize,
            4,
            0,
            8
        );

        PaintHorizontalSpan(
            pixels,
            textureSize,
            5,
            2,
            6
        );

        PaintHorizontalSpan(
            pixels,
            textureSize,
            6,
            3,
            5
        );

        PaintHorizontalSpan(
            pixels,
            textureSize,
            7,
            4,
            4
        );

        PaintHorizontalSpan(
            pixels,
            textureSize,
            8,
            4,
            4
        );

        runtimeStarTexture.SetPixels(
            pixels
        );

        runtimeStarTexture.Apply();

        runtimeStarSprite =
            Sprite.Create(
                runtimeStarTexture,
                new Rect(
                    0f,
                    0f,
                    textureSize,
                    textureSize
                ),
                new Vector2(
                    0.5f,
                    0.5f
                ),
                textureSize
            );

        runtimeStarSprite.name =
            "Runtime Color Crystal Star Glint Sprite";
    }

    private static void PaintHorizontalSpan(
        Color[] pixels,
        int textureSize,
        int y,
        int startX,
        int endX)
    {
        for (int x = startX;
             x <= endX;
             x++)
        {
            pixels[
                y *
                textureSize +
                x
            ] = Color.white;
        }
    }

    private void StopAllGlintsImmediately()
    {
        List<ColorCrystalGlintVFX> snapshot =
            new List<ColorCrystalGlintVFX>(
                activeGlints
            );

        foreach (ColorCrystalGlintVFX glint
                 in snapshot)
        {
            if (glint == null)
            {
                continue;
            }

            glint.StopImmediately();

            activeGlints.Remove(
                glint
            );

            availableGlints.Enqueue(
                glint
            );
        }

        activeTargetLaunchSequences = 0;
    }

    private void OnDisable()
    {
        StopAllGlintsImmediately();
    }

    private void OnDestroy()
    {
        if (runtimeStarSprite != null)
        {
            Destroy(
                runtimeStarSprite
            );
        }

        if (runtimeStarTexture != null)
        {
            Destroy(
                runtimeStarTexture
            );
        }
    }

    private void OnValidate()
    {
        glintSpawnStagger =
            Mathf.Max(
                0f,
                glintSpawnStagger
            );

        glintDuration =
            Mathf.Max(
                0.06f,
                glintDuration
            );

        glintPeakTime =
            Mathf.Clamp(
                glintPeakTime,
                0.041f,
                glintDuration -
                    0.01f
            );

        targetFlashDuration =
            Mathf.Max(
                0f,
                targetFlashDuration
            );

        glintSizeInCells =
            Mathf.Max(
                0.01f,
                glintSizeInCells
            );

        maximumGlintCount =
            Mathf.Max(
                1,
                maximumGlintCount
            );

        prewarmCount =
            Mathf.Clamp(
                prewarmCount,
                0,
                maximumGlintCount
            );
    }
}
