using System;
using System.Collections.Generic;

public static class UpgradeDraftGenerator
{
    public const int DefaultChoiceCount = 3;

    public static List<RunUpgradeDefinition> Generate(
        RunUpgradeCatalog catalog,
        RunUpgradeRuntime runtime,
        PlayerActor playerActor,
        int currentWave,
        Random random,
        int choiceCount = DefaultChoiceCount)
    {
        List<RunUpgradeDefinition> eligible =
            new List<RunUpgradeDefinition>();

        if (catalog == null || runtime == null || random == null ||
            choiceCount <= 0)
        {
            return eligible;
        }

        IReadOnlyList<RunUpgradeDefinition> definitions = catalog.Upgrades;

        for (int index = 0; index < definitions.Count; index++)
        {
            RunUpgradeDefinition definition = definitions[index];

            if (runtime.IsEligible(definition, playerActor, currentWave))
            {
                eligible.Add(definition);
            }
        }

        List<RunUpgradeDefinition> choices =
            new List<RunUpgradeDefinition>(choiceCount);

        while (eligible.Count > 0 && choices.Count < choiceCount)
        {
            double totalWeight = 0d;

            for (int index = 0; index < eligible.Count; index++)
            {
                totalWeight += GetEffectiveWeight(eligible[index]);
            }

            int selectedIndex;

            if (totalWeight <= 0d)
            {
                selectedIndex = random.Next(0, eligible.Count);
            }
            else
            {
                double roll = random.NextDouble() * totalWeight;
                selectedIndex = eligible.Count - 1;

                for (int index = 0; index < eligible.Count; index++)
                {
                    roll -= GetEffectiveWeight(eligible[index]);

                    if (roll < 0d)
                    {
                        selectedIndex = index;
                        break;
                    }
                }
            }

            choices.Add(eligible[selectedIndex]);
            eligible.RemoveAt(selectedIndex);
        }

        return choices;
    }

    public static double GetRarityWeightMultiplier(RunUpgradeRarity rarity)
    {
        switch (rarity)
        {
            case RunUpgradeRarity.Uncommon:
                return 0.65d;
            case RunUpgradeRarity.Rare:
                return 0.35d;
            case RunUpgradeRarity.Epic:
                return 0.20d;
            default:
                return 1d;
        }
    }

    private static double GetEffectiveWeight(RunUpgradeDefinition definition)
    {
        if (definition == null)
        {
            return 0d;
        }

        return Math.Max(0d, definition.Weight) *
               GetRarityWeightMultiplier(definition.Rarity);
    }
}
