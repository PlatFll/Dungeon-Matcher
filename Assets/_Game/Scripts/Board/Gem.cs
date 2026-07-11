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

    private Color normalColor;
    private float normalScale;

    public void Initialize(
        BoardController boardController,
        int column,
        int row,
        GemType gemType,
        Color color,
        float scale)
    {
        board = boardController;
        spriteRenderer = GetComponent<SpriteRenderer>();
        normalScale = scale;

        SetGridPosition(column, row);
        SetType(gemType, color);

        transform.localScale = Vector3.one * normalScale;
    }

    public void SetGridPosition(int column, int row)
    {
        Column = column;
        Row = row;

        gameObject.name = $"Gem_{column}_{row}";
    }

    public void SetType(GemType gemType, Color color)
    {
        Type = gemType;
        normalColor = color;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

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
        if (selected)
        {
            transform.localScale =
                Vector3.one * (normalScale * 1.12f);

            spriteRenderer.color =
                Color.Lerp(normalColor, Color.white, 0.35f);
        }
        else
        {
            transform.localScale =
                Vector3.one * normalScale;

            spriteRenderer.color = normalColor;
        }
    }
}