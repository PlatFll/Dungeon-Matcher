using UnityEngine;

public partial class BoardController
{
    [Header("Special Bomb Source Gem Icons (16x16)")]
    [SerializeField]
    private Sprite rubySpecialBombIcon;

    [SerializeField]
    private Sprite amberSpecialBombIcon;

    [SerializeField]
    private Sprite topazSpecialBombIcon;

    [SerializeField]
    private Sprite emeraldSpecialBombIcon;

    [SerializeField]
    private Sprite sapphireSpecialBombIcon;

    [SerializeField]
    private Sprite amethystSpecialBombIcon;

    [Header("Poison Bomb Visuals")]
    [SerializeField]
    private Sprite poisonBombSprite;

    [SerializeField]
    private Sprite poisonedStatusIcon;

    public Sprite PoisonBombSprite =>
        poisonBombSprite;

    public Sprite PoisonedStatusIcon =>
        poisonedStatusIcon;

    public Sprite GetSpecialBombSourceIcon(
        GemType gemType)
    {
        switch (gemType)
        {
            case GemType.Ruby:
                return rubySpecialBombIcon;

            case GemType.Amber:
                return amberSpecialBombIcon;

            case GemType.Topaz:
                return topazSpecialBombIcon;

            case GemType.Emerald:
                return emeraldSpecialBombIcon;

            case GemType.Sapphire:
                return sapphireSpecialBombIcon;

            case GemType.Amethyst:
                return amethystSpecialBombIcon;

            default:
                return null;
        }
    }
}
