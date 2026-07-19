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

    [Header("Match Damage")]
    [SerializeField, Min(0)]
    [Tooltip(
        "The single bonus hit caused by a connected " +
        "group of exactly three gems."
    )]
    private int threeGemDamage = 15;

    [SerializeField, Min(0)]
    [Tooltip(
        "Damage added to the same single hit for " +
        "every connected gem beyond three."
    )]
    private int additionalDamagePerGem = 8;

    [SerializeField, Range(0f, 1f)]
    [Tooltip(
        "Additional damage per cascade depth. " +
        "0.15 means fifteen percent per depth."
    )]
    private float cascadeDamageBonusPerDepth =
        0.15f;

    public override int EnergyCost => energyCost;

    public float Duration => duration;

    public int CalculateDamage(
        BoardMatchContext context)
    {
        int safeGemCount =
            Mathf.Max(
                3,
                context.GemCount
            );

        int baseDamage =
            threeGemDamage +
            Mathf.Max(
                0,
                safeGemCount - 3
            ) *
            additionalDamagePerGem;

        float cascadeMultiplier =
            1f +
            context.CascadeDepth *
            cascadeDamageBonusPerDepth;

        return Mathf.Max(
            0,
            Mathf.RoundToInt(
                baseDamage *
                cascadeMultiplier
            )
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

        threeGemDamage =
            Mathf.Max(
                0,
                threeGemDamage
            );

        additionalDamagePerGem =
            Mathf.Max(
                0,
                additionalDamagePerGem
            );
    }
}