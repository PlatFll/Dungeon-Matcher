using UnityEngine;

[CreateAssetMenu(
    fileName = "ModularHealthBarStyle",
    menuName = "Dungeon Matcher/UI/Modular Health Bar Style"
)]
public sealed class ModularHealthBarStyle : ScriptableObject
{
    [Header("Three-Piece Frame")]
    [SerializeField]
    [Tooltip("Left/start cap sprite. This piece is never stretched.")]
    private Sprite startPiece;

    [SerializeField]
    [Tooltip(
        "Repeatable center sprite. It is tiled horizontally between the two caps."
    )]
    private Sprite middlePiece;

    [SerializeField]
    [Tooltip("Right/end cap sprite. This piece is never stretched.")]
    private Sprite endPiece;

    [Header("Fill")]
    [SerializeField]
    [Tooltip("Color shown in the missing-health portion inside the frame.")]
    private Color emptyFillColor =
        new Color32(42, 20, 25, 255);

    [SerializeField]
    [Tooltip("Color shown for current health.")]
    private Color fillColor =
        new Color32(224, 45, 45, 255);

    [SerializeField, Min(0f)]
    [Tooltip("Empty space reserved inside the frame before the health fill begins.")]
    private float fillInsetLeft = 4f;

    [SerializeField, Min(0f)]
    private float fillInsetRight = 4f;

    [SerializeField, Min(0f)]
    private float fillInsetVertical = 4f;

    [Header("Motion")]
    [SerializeField, Min(0f)]
    [Tooltip(
        "Normalized health per second used by the visible fill. Set to zero for " +
        "instant updates. The moving edge is snapped to reference pixels."
    )]
    private float fillAnimationSpeed = 4f;

    public Sprite StartPiece => startPiece;
    public Sprite MiddlePiece => middlePiece;
    public Sprite EndPiece => endPiece;
    public Color EmptyFillColor => emptyFillColor;
    public Color FillColor => fillColor;
    public float FillInsetLeft => fillInsetLeft;
    public float FillInsetRight => fillInsetRight;
    public float FillInsetVertical => fillInsetVertical;
    public float FillAnimationSpeed => fillAnimationSpeed;

    public bool HasCompleteFrame =>
        startPiece != null &&
        middlePiece != null &&
        endPiece != null;

    private void OnValidate()
    {
        fillInsetLeft = Mathf.Max(0f, fillInsetLeft);
        fillInsetRight = Mathf.Max(0f, fillInsetRight);
        fillInsetVertical = Mathf.Max(0f, fillInsetVertical);
        fillAnimationSpeed = Mathf.Max(0f, fillAnimationSpeed);
    }
}
