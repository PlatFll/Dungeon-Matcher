using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public enum GemType
{
    Ruby,
    Amber,
    Topaz,
    Emerald,
    Sapphire,
    Amethyst
}

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class Gem :
    MonoBehaviour,
    IPointerDownHandler,
    IDragHandler,
    IPointerUpHandler
{
    private static readonly int
        FlashAmountId =
            Shader.PropertyToID(
                "_FlashAmount"
            );

    public int Column { get; private set; }

    public int Row { get; private set; }

    public GemType Type { get; private set; }

    public GemSpecialType SpecialType
    {
        get;
        private set;
    }

    private BoardController board;

    private SpriteRenderer spriteRenderer;

    private GemSpecialOverlayView
    specialOverlayView;

    private PoisonBombGemView
    poisonBombView;

    private MaterialPropertyBlock
        materialPropertyBlock;

    private float normalScale;

    private readonly Color normalColor =
        Color.white;

    private readonly Color selectedColor =
        new Color(
            1f,
            1f,
            0.72f,
            1f
        );

    public void Initialize(
        BoardController boardController,
        int column,
        int row,
        GemType gemType,
        Sprite sprite,
        float scale)
    {
        board =
            boardController;

        spriteRenderer =
            GetComponent<SpriteRenderer>();

        specialOverlayView =
            GetComponentInChildren<
                GemSpecialOverlayView
            >(true);

        poisonBombView =
            GetComponentInChildren<
                PoisonBombGemView
            >(true);

        normalScale =
            scale;

        SetGridPosition(
            column,
            row
        );

        SetType(
            gemType,
            sprite
        );

        SetSpecialType(
            GemSpecialType.None
        );

        transform.localScale =
            Vector3.one *
            normalScale;

        SetFlashAmount(0f);

        spriteRenderer.enabled =
            true;
    }

    public void SetGridPosition(
        int column,
        int row)
    {
        Column =
            column;

        Row =
            row;

        gameObject.name =
            $"Gem_{column}_{row}";
    }

    public void SetType(
        GemType gemType,
        Sprite sprite)
    {
        Type =
            gemType;

        if (spriteRenderer == null)
        {
            spriteRenderer =
                GetComponent<SpriteRenderer>();
        }

        spriteRenderer.sprite =
            sprite;

        spriteRenderer.color =
            normalColor;

        spriteRenderer.enabled =
            true;

        SetFlashAmount(0f);

        if (SpecialType ==
            GemSpecialType.PoisonBomb)
        {
            RefreshPoisonBombVisual();
        }
    }

    public void SetSpecialType(
        GemSpecialType specialType)
    {
        SpecialType = specialType;

        if (specialOverlayView == null)
        {
            specialOverlayView =
                GetComponentInChildren<
                    GemSpecialOverlayView
                >(true);
        }

        if (poisonBombView == null)
        {
            poisonBombView =
                GetComponentInChildren<
                    PoisonBombGemView
                >(true);
        }

        if (spriteRenderer == null)
        {
            spriteRenderer =
                GetComponent<SpriteRenderer>();
        }

        if (SpecialType ==
            GemSpecialType.None)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
            }

            if (specialOverlayView != null)
            {
                specialOverlayView.Hide();
            }

            if (poisonBombView != null)
            {
                poisonBombView.Hide();
            }

            return;
        }

        if (SpecialType ==
            GemSpecialType.PoisonBomb)
        {
            if (specialOverlayView != null)
            {
                specialOverlayView.Hide();
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = false;
            }

            RefreshPoisonBombVisual();
            return;
        }

        if (poisonBombView != null)
        {
            poisonBombView.Hide();
        }

        if (specialOverlayView == null)
        {
            return;
        }

        /*
         * Row/column bombs are overlays, so keep the gem visible.
         * Color crystals replace the normal gem sprite.
         */
        bool isCrystal =
            SpecialType ==
            GemSpecialType.ColorCrystal;

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled =
                !isCrystal;
        }

        specialOverlayView.Show(
            Type,
            SpecialType
        );
    }

    private void RefreshPoisonBombVisual()
    {
        if (SpecialType !=
            GemSpecialType.PoisonBomb ||
            board == null)
        {
            return;
        }

        if (spriteRenderer == null)
        {
            spriteRenderer =
                GetComponent<SpriteRenderer>();
        }

        if (poisonBombView == null)
        {
            poisonBombView =
                PoisonBombGemView.GetOrCreate(
                    transform,
                    spriteRenderer
                );
        }

        if (poisonBombView == null)
        {
            return;
        }

        poisonBombView.Show(
            board.PoisonBombSprite,
            board.GetSpecialBombSourceIcon(
                Type
            ),
            spriteRenderer
        );
    }

    public IEnumerator
        PlayColorCrystalMaterialization()
    {
        if (SpecialType !=
            GemSpecialType.ColorCrystal)
        {
            yield break;
        }

        if (specialOverlayView == null)
        {
            specialOverlayView =
                GetComponentInChildren<
                    GemSpecialOverlayView
                >(true);
        }

        if (specialOverlayView == null)
        {
            yield break;
        }

        yield return
            specialOverlayView
                .PlayColorCrystalMaterialization();
    }

    public IEnumerator
        PlayColorCrystalActivationPulse()
    {
        if (SpecialType !=
            GemSpecialType.ColorCrystal)
        {
            yield break;
        }

        if (specialOverlayView == null)
        {
            specialOverlayView =
                GetComponentInChildren<
                    GemSpecialOverlayView
                >(true);
        }

        if (specialOverlayView == null)
        {
            yield break;
        }

        yield return
            specialOverlayView
                .PlayColorCrystalActivationPulse();
    }

    public void SetFlashAmount(
        float amount)
    {
        if (spriteRenderer == null)
        {
            spriteRenderer =
                GetComponent<SpriteRenderer>();
        }

        if (materialPropertyBlock == null)
        {
            materialPropertyBlock =
                new MaterialPropertyBlock();
        }

        spriteRenderer.GetPropertyBlock(
            materialPropertyBlock
        );

        materialPropertyBlock.SetFloat(
            FlashAmountId,
            Mathf.Clamp01(amount)
        );

        spriteRenderer.SetPropertyBlock(
            materialPropertyBlock
        );

        if (poisonBombView != null &&
            SpecialType ==
                GemSpecialType.PoisonBomb)
        {
            poisonBombView.SetFlashAmount(
                amount
            );
        }
    }

    public void SetVFXFlashAmount(
        float amount)
    {
        SetFlashAmount(
            amount
        );

        if (specialOverlayView == null)
        {
            specialOverlayView =
                GetComponentInChildren<
                    GemSpecialOverlayView
                >(true);
        }

        if (specialOverlayView != null &&
            SpecialType !=
                GemSpecialType.None &&
            SpecialType !=
                GemSpecialType.PoisonBomb)
        {
            specialOverlayView
                .SetVFXFlashAmount(
                    amount
                );
        }
    }

    public void OnPointerDown(
        PointerEventData eventData)
    {
        if (board == null ||
            board.IsGemPinned(this))
        {
            board?.CancelPointerInteraction(
                this
            );

            return;
        }

        board.BeginPointer(
            this,
            eventData.position
        );
    }

    public void OnDrag(
        PointerEventData eventData)
    {
        if (board == null ||
            board.IsGemPinned(this))
        {
            board?.CancelPointerInteraction(
                this
            );

            return;
        }

        board.UpdatePointerDrag(
            this,
            eventData.position
        );
    }

    public void OnPointerUp(
        PointerEventData eventData)
    {
        if (board == null ||
            board.IsGemPinned(this))
        {
            board?.CancelPointerInteraction(
                this
            );

            return;
        }

        board.EndPointer(
            this,
            eventData.position
        );
    }

    public void SetSelected(
        bool selected)
    {
        transform.localScale =
            Vector3.one *
            normalScale *
            (
                selected
                    ? 1.12f
                    : 1f
            );

        spriteRenderer.color =
            selected
                ? selectedColor
                : normalColor;

        spriteRenderer.sortingOrder =
            selected
                ? 1
                : 0;
    }

    private void OnDestroy()
    {
        /*
         * Every actual board clear ultimately destroys its Gem object after
         * the clear flash. Reporting here gives pin/chain obstacles one common
         * physical-clear signal regardless of whether the gem was removed by
         * a normal match, cascade, bomb, crystal, ability, or environmental
         * board mutation.
         */
        if (!Application.isPlaying ||
            board == null)
        {
            return;
        }

        board.NotifyGemDestroyedForPins(
            this
        );
    }

    [ContextMenu("Debug/Show Row Bomb")]
    private void DebugShowRowBomb()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        SetSpecialType(
            GemSpecialType.RowBomb
        );
    }

    [ContextMenu("Debug/Show Column Bomb")]
    private void DebugShowColumnBomb()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        SetSpecialType(
            GemSpecialType.ColumnBomb
        );
    }

    [ContextMenu("Debug/Clear Special Type")]
    private void DebugClearSpecialType()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        SetSpecialType(
            GemSpecialType.None
        );
    }

    [ContextMenu("Debug/Show Color Crystal")]
    private void DebugShowColorCrystal()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        SetSpecialType(
            GemSpecialType.ColorCrystal
        );
    }

    [ContextMenu("Debug/Show Poison Bomb")]
    private void DebugShowPoisonBomb()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        SetSpecialType(
            GemSpecialType.PoisonBomb
        );
    }
}
