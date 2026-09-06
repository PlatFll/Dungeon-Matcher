using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "RunUpgrade_",
    menuName = "Dungeon Matcher/Run Upgrades/Upgrade Definition"
)]
public sealed class RunUpgradeDefinition : ScriptableObject
{
    [Header("Identity")]
    [SerializeField]
    [Tooltip("Stable internal identifier. Avoid changing it after release.")]
    private string upgradeId = "upgrade_id";

    [SerializeField]
    private string displayTitle = "Upgrade";

    [SerializeField, TextArea(3, 6)]
    private string description;

    [SerializeField]
    [Tooltip("Optional 64x64 transparent pixel-art sprite.")]
    private Sprite artwork;

    [SerializeField]
    private RunUpgradeRarity rarity = RunUpgradeRarity.Common;

    [Header("Drafting / Eligibility")]
    [SerializeField, Min(0f)]
    private float weight = 1f;

    [SerializeField, Min(1)]
    private int maxStacks = 1;

    [SerializeField, Min(1)]
    private int minimumWave = 1;

    [SerializeField, Min(0)]
    [Tooltip("Zero means no maximum wave.")]
    private int maximumWave;

    [SerializeField]
    [Tooltip("Stable PlayerDefinition.PlayerId. Empty means any player.")]
    private string requiredPlayerId;

    [SerializeField]
    [Tooltip("Stable CharacterAbilityDefinition.AbilityId. Empty means any ability.")]
    private string requiredAbilityId;

    [SerializeField]
    private string[] prerequisiteUpgradeIds = new string[0];

    [SerializeField]
    private string[] excludedUpgradeIds = new string[0];

    [Header("Effects")]
    [SerializeField]
    private RunUpgradeModifier[] modifiers = new RunUpgradeModifier[0];

    [SerializeField]
    [Tooltip(
        "Typed capabilities are for behavior changes. Numeric tuning belongs " +
        "in Modifiers."
    )]
    private RunUpgradeMechanicGrant[] mechanicGrants =
        new RunUpgradeMechanicGrant[0];

    public string UpgradeId => upgradeId;
    public string DisplayTitle => displayTitle;
    public string Description => description;
    public Sprite Artwork => artwork;
    public RunUpgradeRarity Rarity => rarity;
    public float Weight => weight;
    public int MaxStacks => maxStacks;
    public int MinimumWave => minimumWave;
    public int MaximumWave => maximumWave;
    public string RequiredPlayerId => requiredPlayerId;
    public string RequiredAbilityId => requiredAbilityId;
    public IReadOnlyList<string> PrerequisiteUpgradeIds =>
        prerequisiteUpgradeIds;
    public IReadOnlyList<string> ExcludedUpgradeIds =>
        excludedUpgradeIds;
    public IReadOnlyList<RunUpgradeModifier> Modifiers => modifiers;
    public IReadOnlyList<RunUpgradeMechanicGrant> MechanicGrants =>
        mechanicGrants;

    private void OnValidate()
    {
        upgradeId = NormalizeId(upgradeId);
        displayTitle = displayTitle == null
            ? string.Empty
            : displayTitle.Trim();
        weight = Mathf.Max(0f, weight);
        maxStacks = Mathf.Max(1, maxStacks);
        minimumWave = Mathf.Max(1, minimumWave);
        maximumWave = Mathf.Max(0, maximumWave);
        requiredPlayerId = NormalizeId(requiredPlayerId);
        requiredAbilityId = NormalizeId(requiredAbilityId);
        NormalizeIds(prerequisiteUpgradeIds);
        NormalizeIds(excludedUpgradeIds);
    }

    private static void NormalizeIds(string[] ids)
    {
        if (ids == null)
        {
            return;
        }

        for (int index = 0; index < ids.Length; index++)
        {
            ids[index] = NormalizeId(ids[index]);
        }
    }

    private static string NormalizeId(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim().ToLowerInvariant().Replace(" ", "_");
    }
}
