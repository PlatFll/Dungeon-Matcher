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

    [Header("Healing Bomb Visuals")]
    [SerializeField]
    private Sprite healingBombSprite;

    [Header("Shield Bomb Visuals")]
    [SerializeField]
    private Sprite shieldBombSprite;

    public Sprite PoisonBombSprite =>
        poisonBombSprite;

    public Sprite HealingBombSprite =>
        healingBombSprite;

    public Sprite ShieldBombSprite =>
        shieldBombSprite;

    public Sprite PoisonedStatusIcon =>
        poisonedStatusIcon;

    // Compatibility alias used by the poison-status presenter.
    public Sprite PoisonedStatusEffectSprite =>
        poisonedStatusIcon;

    public Sprite GetSpecialBombSprite(
        GemSpecialType specialType)
    {
        switch (specialType)
        {
            case GemSpecialType.PoisonBomb:
                return poisonBombSprite;

            case GemSpecialType.HealingBomb:
                return healingBombSprite;

            case GemSpecialType.ShieldBomb:
                return shieldBombSprite;

            default:
                return null;
        }
    }

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
