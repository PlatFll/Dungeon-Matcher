using UnityEngine;

[CreateAssetMenu(
    fileName = "Ability_CrackedGems",
    menuName =
        "Dungeon Matcher/Abilities/Cracked Gems"
)]
public sealed class CrackedGemsAbilityDefinition :
    CharacterAbilityDefinition
{
    [Header("Activation")]
    [SerializeField, Min(0)]
    [Tooltip(
        "Bardley's energy cost is intentionally left at 0 until the final " +
        "design value is chosen. The runtime refuses to activate while this " +
        "value is 0 so an unfinished balance value cannot ship silently."
    )]
    private int energyCost;

    [SerializeField, Min(1)]
    private int targetGemCount = 5;

    [Header("Cracked Gem Damage")]
    [SerializeField, Min(1)]
    [Tooltip(
        "Fixed base color damage dealt by each cracked gem when it detonates."
    )]
    private int crackedGemDamage = 50;

    [Header("Timing")]
    [SerializeField, Min(0f)]
    private float bubbleTravelDuration = 0.35f;

    [SerializeField, Min(0f)]
    private float bubbleHoverDuration = 0.20f;

    [SerializeField, Min(0f)]
    private float crackedShakeDuration = 1f;

    [SerializeField, Range(1f, 1.2f)]
    private float crackedBurstScale = 1.08f;

    [SerializeField, Min(0f)]
    private float crackedWhiteHoldDuration = 0.05f;

    public override int EnergyCost =>
        energyCost;

    public int TargetGemCount =>
        targetGemCount;

    public int CrackedGemDamage =>
        crackedGemDamage;

    public float BubbleTravelDuration =>
        bubbleTravelDuration;

    public float BubbleHoverDuration =>
        bubbleHoverDuration;

    public float CrackedShakeDuration =>
        crackedShakeDuration;

    public float CrackedBurstScale =>
        crackedBurstScale;

    public float CrackedWhiteHoldDuration =>
        crackedWhiteHoldDuration;

    protected override void OnValidate()
    {
        base.OnValidate();

        energyCost =
            Mathf.Max(0, energyCost);

        targetGemCount =
            Mathf.Max(1, targetGemCount);

        crackedGemDamage =
            Mathf.Max(1, crackedGemDamage);

        bubbleTravelDuration =
            Mathf.Max(0f, bubbleTravelDuration);

        bubbleHoverDuration =
            Mathf.Max(0f, bubbleHoverDuration);

        crackedShakeDuration =
            Mathf.Max(0f, crackedShakeDuration);

        crackedBurstScale =
            Mathf.Clamp(
                crackedBurstScale,
                1f,
                1.2f
            );

        crackedWhiteHoldDuration =
            Mathf.Max(
                0f,
                crackedWhiteHoldDuration
            );
    }
}
