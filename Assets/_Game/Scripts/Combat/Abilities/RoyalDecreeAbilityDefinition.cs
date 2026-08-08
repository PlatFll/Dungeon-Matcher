using UnityEngine;

[CreateAssetMenu(
    fileName = "Ability_RoyalDecree",
    menuName =
        "Dungeon Matcher/Abilities/Royal Decree"
)]
public sealed class RoyalDecreeAbilityDefinition :
    CharacterAbilityDefinition
{
    [Header("Activation")]
    [SerializeField, Min(1)]
    private int energyCost = 100;

    [SerializeField, Min(0.1f)]
    private float duration = 7f;

    [Header("Per-Gem Damage")]
    [SerializeField, Min(0)]
    [Tooltip(
        "Damage caused by Royal Decree for each " +
        "rewardable colored gem genuinely destroyed."
    )]
    private int damagePerGem = 5;

    [SerializeField, Range(0f, 1f)]
    [Tooltip(
        "Additional Royal Decree damage for each " +
        "cascade depth."
    )]
    private float cascadeDamageBonusPerDepth =
        0.15f;

    public override int EnergyCost =>
        energyCost;

    public float Duration =>
        duration;

    public int DamagePerGem =>
        damagePerGem;

    public int CalculateDamagePerGem(
        BoardClearContext context)
    {
        float cascadeMultiplier =
            1f +
            Mathf.Max(
                0,
                context.CascadeDepth
            ) *
            cascadeDamageBonusPerDepth;

        return Mathf.Max(
            0,
            Mathf.RoundToInt(
                damagePerGem *
                cascadeMultiplier
            )
        );
    }

    /*
     * Temporary compatibility helper for any older code
     * that still asks for match-based total damage.
     */
    public int CalculateDamage(
        BoardMatchContext context)
    {
        BoardClearContext clearContext =
            new BoardClearContext(
                context.GemType,
                context.GemCount,
                context.CascadeDepth,
                BoardClearSource.Match,
                context.MatchType,
                context.GemCount
            );

        return
            CalculateDamagePerGem(
                clearContext
            ) *
            Mathf.Max(
                0,
                context.GemCount
            );
    }

    protected override void OnValidate()
    {
        base.OnValidate();

        energyCost =
            Mathf.Max(
                1,
                energyCost
            );

        duration =
            Mathf.Max(
                0.1f,
                duration
            );

        damagePerGem =
            Mathf.Max(
                0,
                damagePerGem
            );

        cascadeDamageBonusPerDepth =
            Mathf.Max(
                0f,
                cascadeDamageBonusPerDepth
            );
    }
}