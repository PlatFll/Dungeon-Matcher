using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoardController))]
public sealed class BombVFXController :
    MonoBehaviour
{
    [Header("Beam")]
    [SerializeField, Min(0.05f)]
    private float beamDuration = 0.36f;

    [SerializeField, Min(0f)]
    [Tooltip(
        "Small extra travel beyond the farthest board edge. " +
        "The board SpriteMask clips this overshoot exactly at " +
        "the playable frame."
    )]
    private float beamEdgeMarginInCells = 0.08f;

    [SerializeField]
    private int beamSortingOrder = 4;

    [SerializeField, Min(0)]
    private int beamPrewarmCount = 8;

    [SerializeField, Min(1)]
    private int maximumBeamCount = 24;

    [Header("White Pixel Sprinkle")]
    [SerializeField, Min(1)]
    private int pixelCount = 8;

    [SerializeField]
    private Vector2 pixelSizeRange =
        new Vector2(
            0.05f,
            0.085f
        );

    [SerializeField]
    private Vector2 pixelSpeedRange =
        new Vector2(
            1.0f,
            1.6667f
        );

    [SerializeField]
    private Vector2 pixelLifetimeRange =
        new Vector2(
            0.255f,
            0.375f
        );

    [SerializeField, Min(0f)]
    private float pixelSpawnRadius = 0.07f;

    [SerializeField, Min(0f)]
    private float pixelMovementDrag = 0.8f;

    [SerializeField]
    private int pixelSortingOrder = 5;

    [SerializeField, Min(0)]
    private int pixelPrewarmCount = 32;

    [SerializeField, Min(1)]
    private int maximumPixelCount = 96;

    private BoardController boardController;

    private Texture2D runtimeTexture;
    private Sprite runtimeSquareSprite;

    private readonly Queue<BombLineVFX>
        availableBeams =
            new Queue<BombLineVFX>();

    private readonly HashSet<BombLineVFX>
        activeBeams =
            new HashSet<BombLineVFX>();

    private readonly Queue<GemPixelParticle>
        availablePixels =
            new Queue<GemPixelParticle>();

    private readonly HashSet<GemPixelParticle>
        activePixels =
            new HashSet<GemPixelParticle>();

    private int createdBeamCount;
    private int createdPixelCount;

    private const string SortingLayerName =
        "Gems";

    private void Awake()
    {
        boardController =
            GetComponent<BoardController>();

        CreateRuntimeSquareSprite();
        PrewarmPools();
    }

    private void OnEnable()
    {
        Subscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
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
        if (!context.IsDirectionalBomb)
        {
            return;
        }

        if (context.StartDelay <= 0f)
        {
            PlayBombVFX(
                context
            );

            return;
        }

        StartCoroutine(
            PlayBombVFXAfterDelay(
                context
            )
        );
    }

    private IEnumerator
        PlayBombVFXAfterDelay(
            BombVFXContext context)
    {
        yield return new WaitForSeconds(
            context.StartDelay
        );

        if (!isActiveAndEnabled)
        {
            yield break;
        }

        PlayBombVFX(
            context
        );
    }

    private void PlayBombVFX(
        BombVFXContext context)
    {
        if (boardController == null ||
            runtimeSquareSprite == null)
        {
            return;
        }

        Vector3 boardLocalPosition =
            boardController.transform
                .InverseTransformPoint(
                    context.WorldPosition
                );

        float fullBeamLength =
            CalculateBeamLength(
                context.SpecialType,
                boardLocalPosition
            );

        BombLineVFX beam =
            GetAvailableBeam();

        if (beam != null)
        {
            activeBeams.Add(
                beam
            );

            beam.Play(
                context.SpecialType,
                boardLocalPosition,
                fullBeamLength,
                boardController.CellSize,
                beamDuration,
                ReleaseBeam
            );
        }

        SpawnWhitePixelSprinkle(
            boardLocalPosition
        );
    }

    private float CalculateBeamLength(
        GemSpecialType specialType,
        Vector3 boardLocalPosition)
    {
        float halfExtent;
        float axisPosition;

        if (specialType ==
            GemSpecialType.RowBomb)
        {
            halfExtent =
                boardController.LocalBoardWidth *
                0.5f;

            axisPosition =
                boardLocalPosition.x;
        }
        else
        {
            halfExtent =
                boardController.LocalBoardHeight *
                0.5f;

            axisPosition =
                boardLocalPosition.y;
        }

        float negativeDirectionDistance =
            axisPosition +
            halfExtent;

        float positiveDirectionDistance =
            halfExtent -
            axisPosition;

        float farthestEdgeDistance =
            Mathf.Max(
                negativeDirectionDistance,
                positiveDirectionDistance
            );

        float edgeMargin =
            boardController.CellSize *
            beamEdgeMarginInCells;

        /*
         * BombLineVFX expands symmetrically from its center.
         * Size it for the farther edge; the nearer side can
         * overshoot geometrically, but the existing board mask
         * clips that side precisely at the frame.
         */
        return
            (farthestEdgeDistance +
             edgeMargin) *
            2f;
    }

    private void SpawnWhitePixelSprinkle(
        Vector3 localPosition)
    {
        float angleOffset =
            Random.Range(
                0f,
                Mathf.PI * 2f
            );

        for (int index = 0;
             index < pixelCount;
             index++)
        {
            GemPixelParticle pixel =
                GetAvailablePixel();

            if (pixel == null)
            {
                return;
            }

            float evenAngle =
                (
                    Mathf.PI *
                    2f *
                    index
                ) /
                pixelCount;

            float angle =
                angleOffset +
                evenAngle +
                Random.Range(
                    -0.28f,
                    0.28f
                );

            Vector2 direction =
                new Vector2(
                    Mathf.Cos(angle),
                    Mathf.Sin(angle)
                );

            float speed =
                Random.Range(
                    pixelSpeedRange.x,
                    pixelSpeedRange.y
                );

            Vector2 velocity =
                direction *
                speed;

            Vector2 randomOffset =
                Random.insideUnitCircle *
                pixelSpawnRadius;

            float lifetime =
                Random.Range(
                    pixelLifetimeRange.x,
                    pixelLifetimeRange.y
                );

            float size =
                Random.Range(
                    pixelSizeRange.x,
                    pixelSizeRange.y
                );

            activePixels.Add(
                pixel
            );

            /*
             * Aspect ratio 1, no spin, no gravity, and pure
             * white keep these fragments looking like crisp
             * little pixel squares instead of the normal
             * colored match shards.
             */
            pixel.Play(
                localPosition +
                (Vector3)randomOffset,
                velocity,
                lifetime,
                size,
                1f,
                0f,
                0f,
                0f,
                pixelMovementDrag,
                Color.white,
                ReleasePixel
            );
        }
    }

    private BombLineVFX GetAvailableBeam()
    {
        while (availableBeams.Count > 0)
        {
            BombLineVFX beam =
                availableBeams.Dequeue();

            if (beam != null)
            {
                return beam;
            }
        }

        if (createdBeamCount >=
            maximumBeamCount)
        {
            return null;
        }

        return CreateBeam();
    }

    private BombLineVFX CreateBeam()
    {
        GameObject beamObject =
            new GameObject(
                "BombLineVFX"
            );

        beamObject.transform.SetParent(
            boardController.transform,
            false
        );

        beamObject.AddComponent<
            SpriteRenderer
        >();

        BombLineVFX beam =
            beamObject.AddComponent<
                BombLineVFX
            >();

        beam.ConfigureRendering(
            runtimeSquareSprite,
            SortingLayerName,
            beamSortingOrder
        );

        beamObject.SetActive(false);

        createdBeamCount++;

        return beam;
    }

    private GemPixelParticle
        GetAvailablePixel()
    {
        while (availablePixels.Count > 0)
        {
            GemPixelParticle pixel =
                availablePixels.Dequeue();

            if (pixel != null)
            {
                return pixel;
            }
        }

        if (createdPixelCount >=
            maximumPixelCount)
        {
            return null;
        }

        return CreatePixel();
    }

    private GemPixelParticle CreatePixel()
    {
        GameObject pixelObject =
            new GameObject(
                "BombPixelVFX"
            );

        pixelObject.transform.SetParent(
            boardController.transform,
            false
        );

        SpriteRenderer renderer =
            pixelObject.AddComponent<
                SpriteRenderer
            >();

        renderer.sprite =
            runtimeSquareSprite;

        GemPixelParticle pixel =
            pixelObject.AddComponent<
                GemPixelParticle
            >();

        pixel.ConfigureRendering(
            SortingLayerName,
            pixelSortingOrder
        );

        pixelObject.SetActive(false);

        createdPixelCount++;

        return pixel;
    }

    private void ReleaseBeam(
        BombLineVFX beam)
    {
        if (beam == null ||
            !activeBeams.Remove(beam))
        {
            return;
        }

        beam.gameObject.SetActive(false);

        beam.transform.SetParent(
            boardController.transform,
            false
        );

        availableBeams.Enqueue(
            beam
        );
    }

    private void ReleasePixel(
        GemPixelParticle pixel)
    {
        if (pixel == null ||
            !activePixels.Remove(pixel))
        {
            return;
        }

        pixel.gameObject.SetActive(false);

        pixel.transform.SetParent(
            boardController.transform,
            false
        );

        availablePixels.Enqueue(
            pixel
        );
    }

    private void PrewarmPools()
    {
        int beamsToCreate =
            Mathf.Min(
                beamPrewarmCount,
                maximumBeamCount
            );

        for (int index = 0;
             index < beamsToCreate;
             index++)
        {
            availableBeams.Enqueue(
                CreateBeam()
            );
        }

        int pixelsToCreate =
            Mathf.Min(
                pixelPrewarmCount,
                maximumPixelCount
            );

        for (int index = 0;
             index < pixelsToCreate;
             index++)
        {
            availablePixels.Enqueue(
                CreatePixel()
            );
        }
    }

    private void StopAllEffectsImmediately()
    {
        List<BombLineVFX> beamSnapshot =
            new List<BombLineVFX>(
                activeBeams
            );

        foreach (BombLineVFX beam
                 in beamSnapshot)
        {
            if (beam == null)
            {
                continue;
            }

            beam.StopImmediately();
            activeBeams.Remove(beam);
            availableBeams.Enqueue(beam);
        }

        List<GemPixelParticle> pixelSnapshot =
            new List<GemPixelParticle>(
                activePixels
            );

        foreach (GemPixelParticle pixel
                 in pixelSnapshot)
        {
            if (pixel == null)
            {
                continue;
            }

            pixel.StopImmediately();
            activePixels.Remove(pixel);
            availablePixels.Enqueue(pixel);
        }
    }

    private void CreateRuntimeSquareSprite()
    {
        runtimeTexture =
            new Texture2D(
                1,
                1,
                TextureFormat.RGBA32,
                false
            );

        runtimeTexture.name =
            "Runtime Bomb VFX Square Texture";

        runtimeTexture.filterMode =
            FilterMode.Point;

        runtimeTexture.wrapMode =
            TextureWrapMode.Clamp;

        runtimeTexture.SetPixel(
            0,
            0,
            Color.white
        );

        runtimeTexture.Apply();

        runtimeSquareSprite =
            Sprite.Create(
                runtimeTexture,
                new Rect(
                    0f,
                    0f,
                    1f,
                    1f
                ),
                new Vector2(
                    0.5f,
                    0.5f
                ),
                1f
            );

        runtimeSquareSprite.name =
            "Runtime Bomb VFX Square Sprite";
    }

    private void OnValidate()
    {
        beamDuration =
            Mathf.Max(
                0.05f,
                beamDuration
            );

        beamEdgeMarginInCells =
            Mathf.Max(
                0f,
                beamEdgeMarginInCells
            );

        maximumBeamCount =
            Mathf.Max(
                1,
                maximumBeamCount
            );

        beamPrewarmCount =
            Mathf.Clamp(
                beamPrewarmCount,
                0,
                maximumBeamCount
            );

        pixelCount =
            Mathf.Max(
                1,
                pixelCount
            );

        pixelSizeRange.x =
            Mathf.Max(
                0.005f,
                pixelSizeRange.x
            );

        pixelSizeRange.y =
            Mathf.Max(
                pixelSizeRange.x,
                pixelSizeRange.y
            );

        pixelSpeedRange.x =
            Mathf.Max(
                0f,
                pixelSpeedRange.x
            );

        pixelSpeedRange.y =
            Mathf.Max(
                pixelSpeedRange.x,
                pixelSpeedRange.y
            );

        pixelLifetimeRange.x =
            Mathf.Max(
                0.05f,
                pixelLifetimeRange.x
            );

        pixelLifetimeRange.y =
            Mathf.Max(
                pixelLifetimeRange.x,
                pixelLifetimeRange.y
            );

        pixelSpawnRadius =
            Mathf.Max(
                0f,
                pixelSpawnRadius
            );

        pixelMovementDrag =
            Mathf.Max(
                0f,
                pixelMovementDrag
            );

        maximumPixelCount =
            Mathf.Max(
                1,
                maximumPixelCount
            );

        pixelPrewarmCount =
            Mathf.Clamp(
                pixelPrewarmCount,
                0,
                maximumPixelCount
            );
    }

    private void OnDestroy()
    {
        if (runtimeSquareSprite != null)
        {
            Destroy(
                runtimeSquareSprite
            );
        }

        if (runtimeTexture != null)
        {
            Destroy(
                runtimeTexture
            );
        }
    }
}
