using UnityEngine;
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
public class Gem : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public int Column { get; private set; }
    public int Row { get; private set; }
    public GemType Type { get; private set; }

    private BoardController board;
    private SpriteRenderer spriteRenderer;

    private float normalScale;
    private readonly Color normalColor = Color.white;
    private readonly Color selectedColor =
        new Color(1f, 1f, 0.72f, 1f);

    public void Initialize(
        BoardController boardController,
        int column,
        int row,
        GemType gemType,
        Sprite sprite,
        float scale)
    {
        board = boardController;
        spriteRenderer = GetComponent<SpriteRenderer>();
        normalScale = scale;

        SetGridPosition(column, row);
        SetType(gemType, sprite);

        transform.localScale =
            Vector3.one * normalScale;
    }

    public void SetGridPosition(int column, int row)
    {
        Column = column;
        Row = row;

        gameObject.name = $"Gem_{column}_{row}";
    }

    public void SetType(GemType gemType, Sprite sprite)
    {
        Type = gemType;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        spriteRenderer.sprite = sprite;
        spriteRenderer.color = normalColor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        board.BeginPointer(this, eventData.position);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        board.EndPointer(this, eventData.position);
    }

    public void SetSelected(bool selected)
    {
        transform.localScale =
            Vector3.one *
            normalScale *
            (selected ? 1.12f : 1f);

        spriteRenderer.color =
            selected ? selectedColor : normalColor;

        // Keeps an enlarged selected gem in front of its neighbors.
        spriteRenderer.sortingOrder =
            selected ? 1 : 0;
    }
}