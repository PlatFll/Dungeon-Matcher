using System;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PoisonBombBurstVFX : MonoBehaviour
{
    private sealed class ResidueSlot
    {
        public SpriteRenderer Renderer;
        public int Column;
        public int Row;
        public bool Active;
    }

    private const int MaximumResidueCells = 9;

    private SpriteRenderer burstRenderer;
    private readonly ResidueSlot[] residueSlots =
        new ResidueSlot[MaximumResidueCells];

    private BoardController boardController;
    private Coroutine playRoutine;
    private Action<PoisonBombBurstVFX> releaseCallback;

    public void ConfigureRendering(
        string sortingLayerName,
        int burstSortingOrder,
        int residueSortingOrder)
    {
        EnsureRenderers();

        if (burstRenderer != null)
        {
            ConfigureRenderer(
                burstRenderer,
                sortingLayerName,
                burstSortingOrder
            );
        }

        for (int index = 0;
             index < residueSlots.Length;
             index++)
        {
            ResidueSlot slot = residueSlots[index];

            if (slot == null ||
                slot.Renderer == null)
            {
                continue;
            }

            ConfigureRenderer(
                slot.Renderer,
                sortingLayerName,
                residueSortingOrder
            );
        }
    }

    public void Play(
        BoardController board,
        BombVFXContext context,
        Sprite[] burstFrames,
        float frameDuration,
        float burstSizeInCells,
        bool enableResidue,
        Sprite residueSprite,
        float residueDuration,
        float residueAlpha,
        float residueSizeInCells,
        Action<PoisonBombBurstVFX> onFinished)
    {
        StopImmediately();

        if (board == null ||
            burstFrames == null ||
            burstFrames.Length == 0)
        {
            onFinished?.Invoke(this);
            return;
        }

        boardController = board;
        releaseCallback = onFinished;

        transform.SetParent(
            boardController.transform,
            false
        );

        transform.localPosition =
            boardController.transform
                .InverseTransformPoint(
                    context.WorldPosition
                );

        transform.localRotation =
            Quaternion.identity;

        transform.localScale =
            Vector3.one;

        gameObject.SetActive(true);

        playRoutine = StartCoroutine(
            PlayRoutine(
                context,
                burstFrames,
                Mathf.Max(0.01f, frameDuration),
                Mathf.Max(0.1f, burstSizeInCells),
                enableResidue,
                residueSprite,
                Mathf.Max(0f, residueDuration),
                Mathf.Clamp01(residueAlpha),
                Mathf.Max(0.1f, residueSizeInCells)
            )
        );
    }

    public void StopImmediately()
    {
        if (playRoutine != null)
        {
            StopCoroutine(playRoutine);
            playRoutine = null;
        }

        if (burstRenderer != null)
        {
            burstRenderer.enabled = false;
            burstRenderer.sprite = null;
            burstRenderer.color = Color.white;
        }

        HideAllResidue();

        releaseCallback = null;
        boardController = null;
    }

    private IEnumerator PlayRoutine(
        BombVFXContext context,
        Sprite[] burstFrames,
        float frameDuration,
        float burstSizeInCells,
        bool enableResidue,
        Sprite configuredResidueSprite,
        float residueDuration,
        float residueAlpha,
        float residueSizeInCells)
    {
        EnsureRenderers();

        if (burstRenderer == null ||
            boardController == null)
        {
            Finish();
            yield break;
        }

        burstRenderer.color = Color.white;
        burstRenderer.enabled = true;

        for (int index = 0;
             index < burstFrames.Length;
             index++)
        {
            Sprite frame = burstFrames[index];

            burstRenderer.sprite = frame;
            burstRenderer.enabled = frame != null;

            if (frame != null)
            {
                SetRendererSizeInCells(
                    burstRenderer,
                    frame,
                    burstSizeInCells
                );
            }

            yield return new WaitForSeconds(
                frameDuration
            );
        }

        burstRenderer.enabled = false;
        burstRenderer.sprite = null;

        if (!enableResidue ||
            !context.HasGridPosition ||
            residueDuration <= 0f ||
            residueAlpha <= 0f ||
            boardController == null)
        {
            Finish();
            yield break;
        }

        Sprite effectiveResidueSprite =
            configuredResidueSprite != null
                ? configuredResidueSprite
                : FindLastUsableFrame(
                    burstFrames
                );

        if (effectiveResidueSprite == null)
        {
            Finish();
            yield break;
        }

        int activeResidueCount =
            ShowResidueFootprint(
                context.Column,
                context.Row,
                effectiveResidueSprite,
                residueAlpha,
                residueSizeInCells
            );

        if (activeResidueCount <= 0)
        {
            Finish();
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < residueDuration &&
               boardController != null)
        {
            float fade =
                1f -
                Mathf.Clamp01(
                    elapsed /
                    residueDuration
                );

            activeResidueCount = 0;

            for (int index = 0;
                 index < residueSlots.Length;
                 index++)
            {
                ResidueSlot slot =
                    residueSlots[index];

                if (slot == null ||
                    !slot.Active ||
                    slot.Renderer == null)
                {
                    continue;
                }

                bool shouldDisappear =
                    !boardController
                        .IsCellEligibleForPoisonResidue(
                            slot.Column,
                            slot.Row
                        ) ||
                    boardController
                        .IsGemVisuallySettledInCell(
                            slot.Column,
                            slot.Row
                        );

                if (shouldDisappear)
                {
                    HideResidueSlot(slot);
                    continue;
                }

                Color color = Color.white;
                color.a = residueAlpha * fade;
                slot.Renderer.color = color;
                activeResidueCount++;
            }

            if (activeResidueCount <= 0)
            {
                break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        HideAllResidue();
        Finish();
    }

    private int ShowResidueFootprint(
        int centerColumn,
        int centerRow,
        Sprite residueSprite,
        float alpha,
        float sizeInCells)
    {
        HideAllResidue();

        if (boardController == null ||
            residueSprite == null)
        {
            return 0;
        }

        int slotIndex = 0;

        for (int rowOffset = -1;
             rowOffset <= 1;
             rowOffset++)
        {
            for (int columnOffset = -1;
                 columnOffset <= 1;
                 columnOffset++)
            {
                int column =
                    centerColumn +
                    columnOffset;

                int row =
                    centerRow +
                    rowOffset;

                if (!boardController
                        .IsCellEligibleForPoisonResidue(
                            column,
                            row
                        ) ||
                    boardController
                        .IsGemVisuallySettledInCell(
                            column,
                            row
                        ))
                {
                    continue;
                }

                if (slotIndex >=
                    residueSlots.Length)
                {
                    return slotIndex;
                }

                ResidueSlot slot =
                    residueSlots[slotIndex];

                if (slot == null ||
                    slot.Renderer == null)
                {
                    slotIndex++;
                    continue;
                }

                slot.Column = column;
                slot.Row = row;
                slot.Active = true;

                slot.Renderer.sprite =
                    residueSprite;

                Color color = Color.white;
                color.a = alpha;
                slot.Renderer.color = color;

                slot.Renderer.transform.localPosition =
                    boardController
                        .GetCellLocalPosition(
                            column,
                            row
                        ) -
                    transform.localPosition;

                slot.Renderer.transform.localRotation =
                    Quaternion.identity;

                /*
                 * Mirroring avoids an obvious tiled residue pattern while
                 * preserving pixel-perfect orientation and scale.
                 */
                slot.Renderer.flipX =
                    ((column + row) & 1) != 0;

                slot.Renderer.flipY =
                    ((column - row) & 1) != 0;

                SetRendererSizeInCells(
                    slot.Renderer,
                    residueSprite,
                    sizeInCells
                );

                slot.Renderer.enabled = true;
                slotIndex++;
            }
        }

        return slotIndex;
    }

    private void EnsureRenderers()
    {
        if (burstRenderer == null)
        {
            Transform burstTransform =
                transform.Find("Burst");

            if (burstTransform == null)
            {
                GameObject burstObject =
                    new GameObject("Burst");

                burstObject.transform.SetParent(
                    transform,
                    false
                );

                burstTransform =
                    burstObject.transform;
            }

            burstRenderer =
                burstTransform
                    .GetComponent<SpriteRenderer>();

            if (burstRenderer == null)
            {
                burstRenderer =
                    burstTransform.gameObject
                        .AddComponent<SpriteRenderer>();
            }
        }

        for (int index = 0;
             index < residueSlots.Length;
             index++)
        {
            if (residueSlots[index] != null &&
                residueSlots[index].Renderer != null)
            {
                continue;
            }

            Transform residueTransform =
                transform.Find(
                    $"Residue_{index}"
                );

            if (residueTransform == null)
            {
                GameObject residueObject =
                    new GameObject(
                        $"Residue_{index}"
                    );

                residueObject.transform.SetParent(
                    transform,
                    false
                );

                residueTransform =
                    residueObject.transform;
            }

            SpriteRenderer renderer =
                residueTransform
                    .GetComponent<SpriteRenderer>();

            if (renderer == null)
            {
                renderer =
                    residueTransform.gameObject
                        .AddComponent<SpriteRenderer>();
            }

            renderer.enabled = false;

            residueSlots[index] =
                new ResidueSlot
                {
                    Renderer = renderer
                };
        }
    }

    private void SetRendererSizeInCells(
        SpriteRenderer renderer,
        Sprite sprite,
        float sizeInCells)
    {
        if (renderer == null ||
            sprite == null ||
            boardController == null)
        {
            return;
        }

        float spriteExtent =
            Mathf.Max(
                sprite.bounds.size.x,
                sprite.bounds.size.y
            );

        float desiredSize =
            boardController.CellSize *
            sizeInCells;

        renderer.transform.localScale =
            spriteExtent > 0.0001f
                ? Vector3.one *
                    (desiredSize / spriteExtent)
                : Vector3.one;
    }

    private static void ConfigureRenderer(
        SpriteRenderer renderer,
        string sortingLayerName,
        int sortingOrder)
    {
        if (renderer == null)
        {
            return;
        }

        renderer.sortingLayerName =
            sortingLayerName;

        renderer.sortingOrder =
            sortingOrder;

        renderer.maskInteraction =
            SpriteMaskInteraction.VisibleInsideMask;
    }

    private static Sprite FindLastUsableFrame(
        Sprite[] frames)
    {
        if (frames == null)
        {
            return null;
        }

        for (int index = frames.Length - 1;
             index >= 0;
             index--)
        {
            if (frames[index] != null)
            {
                return frames[index];
            }
        }

        return null;
    }

    private void HideAllResidue()
    {
        for (int index = 0;
             index < residueSlots.Length;
             index++)
        {
            HideResidueSlot(
                residueSlots[index]
            );
        }
    }

    private static void HideResidueSlot(
        ResidueSlot slot)
    {
        if (slot == null)
        {
            return;
        }

        slot.Active = false;

        if (slot.Renderer == null)
        {
            return;
        }

        slot.Renderer.enabled = false;
        slot.Renderer.sprite = null;
        slot.Renderer.color = Color.white;
        slot.Renderer.flipX = false;
        slot.Renderer.flipY = false;
    }

    private void Finish()
    {
        playRoutine = null;

        Action<PoisonBombBurstVFX> callback =
            releaseCallback;

        releaseCallback = null;
        boardController = null;

        callback?.Invoke(this);
    }
}
