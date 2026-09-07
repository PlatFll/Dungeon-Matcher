using UnityEngine;

[CreateAssetMenu(
    fileName = "PlayerAreaFrameProfile",
    menuName = "Dungeon Matcher/UI/Player Area Frame Profile"
)]
public sealed class PlayerAreaFrameProfile : ScriptableObject
{
    [SerializeField, Min(1f)]
    [Tooltip(
        "Authored player-frame width in reference UI pixels. Keep the three " +
        "frame sprites at this exact source width to avoid horizontal scaling."
    )]
    private float playerSectionWidth = 146f;

    [SerializeField]
    [Tooltip(
        "Top cap of the isolated player frame. Author at the full frame width."
    )]
    private Sprite topPiece;

    [SerializeField]
    [Tooltip(
        "Repeatable middle slice. Author at the full frame width; it is tiled " +
        "vertically to absorb different battle-area heights."
    )]
    private Sprite middlePiece;

    [SerializeField]
    [Tooltip(
        "Bottom cap of the isolated player frame. Author at the full frame width."
    )]
    private Sprite bottomPiece;

    public float PlayerSectionWidth =>
        Mathf.Max(1f, playerSectionWidth);

    public Sprite TopPiece => topPiece;
    public Sprite MiddlePiece => middlePiece;
    public Sprite BottomPiece => bottomPiece;

    private void OnValidate()
    {
        playerSectionWidth =
            Mathf.Max(1f, playerSectionWidth);
    }
}
