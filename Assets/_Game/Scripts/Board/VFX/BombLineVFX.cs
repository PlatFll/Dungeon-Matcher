using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public sealed class BombLineVFX :
    MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    private GemSpecialType specialType;

    private float targetLength;
    private float cellSize;
    private float lifetime;
    private float elapsedTime;

    private Action<BombLineVFX>
        releaseAction;

    private bool isPlaying;

    public bool IsPlaying =>
        isPlaying;

    private void Awake()
    {
        CacheReferences();
    }

    private void CacheReferences()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer =
                GetComponent<SpriteRenderer>();
        }
    }

    public void ConfigureRendering(
        Sprite squareSprite,
        string sortingLayerName,
        int sortingOrder)
    {
        CacheReferences();

        spriteRenderer.sprite =
            squareSprite;

        spriteRenderer.sortingLayerName =
            sortingLayerName;

        spriteRenderer.sortingOrder =
            sortingOrder;

        /*
         * The board already owns a rectangular SpriteMask.
         * Keeping the beam inside that mask makes it vanish
         * exactly at the playable board edge, even when the
         * bomb is close to one side of the board.
         */
        spriteRenderer.maskInteraction =
            SpriteMaskInteraction
                .VisibleInsideMask;
    }

    public void Play(
        GemSpecialType bombSpecialType,
        Vector3 localPosition,
        float fullLength,
        float boardCellSize,
        float duration,
        Action<BombLineVFX> onFinished)
    {
        CacheReferences();

        if (spriteRenderer == null ||
            (
                bombSpecialType !=
                    GemSpecialType.RowBomb &&
                bombSpecialType !=
                    GemSpecialType.ColumnBomb
            ))
        {
            onFinished?.Invoke(this);
            return;
        }

        specialType =
            bombSpecialType;

        targetLength =
            Mathf.Max(
                0.01f,
                fullLength
            );

        cellSize =
            Mathf.Max(
                0.01f,
                boardCellSize
            );

        lifetime =
            Mathf.Max(
                0.05f,
                duration
            );

        elapsedTime = 0f;

        releaseAction =
            onFinished;

        transform.localPosition =
            localPosition;

        transform.localRotation =
            Quaternion.identity;

        spriteRenderer.color =
            Color.white;

        spriteRenderer.enabled = true;

        isPlaying = true;

        gameObject.SetActive(true);

        ApplyVisualState(0f);
    }

    private void Update()
    {
        if (!isPlaying)
        {
            return;
        }

        elapsedTime +=
            Time.deltaTime;

        float normalizedTime =
            Mathf.Clamp01(
                elapsedTime /
                lifetime
            );

        ApplyVisualState(
            normalizedTime
        );

        if (elapsedTime >= lifetime)
        {
            Finish();
        }
    }

    private void ApplyVisualState(
        float normalizedTime)
    {
        /*
         * Reach the board edges quickly, but leave a short
         * tail at full extension so the player can read the
         * row/column direction before the beam disappears.
         */
        float lengthProgress =
            Mathf.InverseLerp(
                0f,
                0.72f,
                normalizedTime
            );

        lengthProgress =
            EaseOutCubic(
                lengthProgress
            );

        float startingLength =
            cellSize *
            0.10f;

        float currentLength =
            Mathf.Lerp(
                startingLength,
                targetLength,
                lengthProgress
            );

        float currentThickness =
            CalculateThickness(
                normalizedTime
            ) *
            cellSize;

        if (specialType ==
            GemSpecialType.RowBomb)
        {
            transform.localScale =
                new Vector3(
                    currentLength,
                    currentThickness,
                    1f
                );
        }
        else
        {
            transform.localScale =
                new Vector3(
                    currentThickness,
                    currentLength,
                    1f
                );
        }

        /*
         * Stay fully opaque during the punch and recoil,
         * then disappear sharply near the end.
         */
        float fadeProgress =
            Mathf.InverseLerp(
                0.68f,
                1f,
                normalizedTime
            );

        fadeProgress =
            SmoothStep(
                fadeProgress
            );

        Color color =
            Color.white;

        color.a =
            1f -
            fadeProgress;

        spriteRenderer.color =
            color;
    }

    private static float CalculateThickness(
        float normalizedTime)
    {
        /*
         * The beam begins narrow, punches outward to roughly
         * one full cell of thickness, then recoils into a
         * tighter streak while it finishes leaving the board.
         * This is the small "bounce" at the center of the hit.
         */
        if (normalizedTime < 0.16f)
        {
            float progress =
                Mathf.InverseLerp(
                    0f,
                    0.16f,
                    normalizedTime
                );

            return Mathf.Lerp(
                0.16f,
                1.04f,
                SmoothStep(progress)
            );
        }

        if (normalizedTime < 0.44f)
        {
            float progress =
                Mathf.InverseLerp(
                    0.16f,
                    0.44f,
                    normalizedTime
                );

            return Mathf.Lerp(
                1.04f,
                0.40f,
                EaseOutCubic(progress)
            );
        }

        if (normalizedTime < 0.80f)
        {
            float progress =
                Mathf.InverseLerp(
                    0.44f,
                    0.80f,
                    normalizedTime
                );

            return Mathf.Lerp(
                0.40f,
                0.28f,
                progress
            );
        }

        float endingProgress =
            Mathf.InverseLerp(
                0.80f,
                1f,
                normalizedTime
            );

        return Mathf.Lerp(
            0.28f,
            0.02f,
            SmoothStep(endingProgress)
        );
    }

    private static float SmoothStep(
        float value)
    {
        value =
            Mathf.Clamp01(value);

        return
            value *
            value *
            (3f - 2f * value);
    }

    private static float EaseOutCubic(
        float value)
    {
        value =
            Mathf.Clamp01(value);

        float inverse =
            1f - value;

        return
            1f -
            inverse *
            inverse *
            inverse;
    }

    private void Finish()
    {
        if (!isPlaying)
        {
            return;
        }

        isPlaying = false;

        Action<BombLineVFX> callback =
            releaseAction;

        releaseAction = null;

        callback?.Invoke(this);
    }

    public void StopImmediately()
    {
        isPlaying = false;
        releaseAction = null;

        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        isPlaying = false;
        elapsedTime = 0f;

        transform.localScale =
            Vector3.one;
    }
}
