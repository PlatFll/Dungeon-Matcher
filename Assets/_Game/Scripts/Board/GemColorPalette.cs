using UnityEngine;

[CreateAssetMenu(
    fileName = "GemColorPalette_",
    menuName = "Dungeon Matcher/Gems/Gem Color Palette"
)]
public sealed class GemColorPalette : ScriptableObject
{
    [Header("Empty Slot")]
    [SerializeField]
    private Color emptyColor =
        new Color32(105, 105, 105, 255);

    [Header("Gem Colors")]
    [SerializeField]
    private Color rubyColor =
        new Color32(220, 45, 55, 255);

    [SerializeField]
    private Color amberColor =
        new Color32(245, 125, 35, 255);

    [SerializeField]
    private Color topazColor =
        new Color32(245, 210, 45, 255);

    [SerializeField]
    private Color emeraldColor =
        new Color32(45, 190, 90, 255);

    [SerializeField]
    private Color sapphireColor =
        new Color32(55, 120, 230, 255);

    [SerializeField]
    private Color amethystColor =
        new Color32(155, 75, 210, 255);

    public Color EmptyColor => emptyColor;

    public Color GetColor(GemType gemType)
    {
        return gemType switch
        {
            GemType.Ruby => rubyColor,
            GemType.Amber => amberColor,
            GemType.Topaz => topazColor,
            GemType.Emerald => emeraldColor,
            GemType.Sapphire => sapphireColor,
            GemType.Amethyst => amethystColor,
            _ => emptyColor
        };
    }
}