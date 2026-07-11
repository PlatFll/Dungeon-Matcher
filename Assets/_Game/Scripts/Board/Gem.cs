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
public class Gem : MonoBehaviour, IPointerDownHandler
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

        Column = column;
        Row = row;
        Type = gemType;

        spriteRenderer = GetComponent<SpriteRenderer>();
        normalColor = color;
        normalScale = scale;

        spriteRenderer.color = normalColor;
        transform.localScale = Vector3.one * normalScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        board.SelectGem(this);
    }

    public void SetSelected(bool selected)
    {
        if (selected)
        {
            transform.localScale = Vector3.one * (normalScale * 1.12f);
            spriteRenderer.color = Color.Lerp(normalColor, Color.white, 0.35f);
        }
        else
        {
            transform.localScale = Vector3.one * normalScale;
            spriteRenderer.color = normalColor;
        }
    }
}
