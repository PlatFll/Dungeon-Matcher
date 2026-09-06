using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "RunUpgradeCatalog",
    menuName = "Dungeon Matcher/Run Upgrades/Upgrade Catalog"
)]
public sealed class RunUpgradeCatalog : ScriptableObject
{
    [SerializeField]
    private RunUpgradeDefinition[] upgrades =
        new RunUpgradeDefinition[0];

    public IReadOnlyList<RunUpgradeDefinition> Upgrades => upgrades;

    public bool ValidateCatalog(Object logContext = null)
    {
        bool valid = true;
        HashSet<string> ids = new HashSet<string>();

        foreach (RunUpgradeDefinition upgrade in upgrades)
        {
            if (upgrade == null)
            {
                Debug.LogError(
                    $"{name} contains a missing upgrade definition.",
                    logContext != null ? logContext : this
                );
                valid = false;
                continue;
            }

            if (string.IsNullOrWhiteSpace(upgrade.UpgradeId) ||
                !ids.Add(upgrade.UpgradeId))
            {
                Debug.LogError(
                    $"{name} contains a missing or duplicate upgrade ID " +
                    $"'{upgrade.UpgradeId}'.",
                    logContext != null ? logContext : this
                );
                valid = false;
            }
        }

        foreach (RunUpgradeDefinition upgrade in upgrades)
        {
            if (upgrade == null)
            {
                continue;
            }

            valid &= ValidateReferencedIds(
                upgrade,
                upgrade.PrerequisiteUpgradeIds,
                "prerequisite",
                ids,
                logContext
            );
            valid &= ValidateReferencedIds(
                upgrade,
                upgrade.ExcludedUpgradeIds,
                "exclusion",
                ids,
                logContext
            );
        }

        return valid;
    }

    private bool ValidateReferencedIds(
        RunUpgradeDefinition owner,
        IReadOnlyList<string> referencedIds,
        string relationship,
        HashSet<string> catalogIds,
        Object logContext)
    {
        bool valid = true;
        HashSet<string> uniqueIds = new HashSet<string>();

        for (int index = 0; index < referencedIds.Count; index++)
        {
            string referencedId = referencedIds[index];

            if (string.IsNullOrWhiteSpace(referencedId) ||
                referencedId == owner.UpgradeId ||
                !catalogIds.Contains(referencedId) ||
                !uniqueIds.Add(referencedId))
            {
                Debug.LogError(
                    $"Upgrade '{owner.UpgradeId}' has an invalid " +
                    $"{relationship} ID '{referencedId}'.",
                    logContext != null ? logContext : this
                );
                valid = false;
            }
        }

        return valid;
    }
}
