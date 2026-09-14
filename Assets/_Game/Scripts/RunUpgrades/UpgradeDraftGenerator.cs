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

        // The first draft establishes distinct, usable directions. Subsequent
        // offers retain weighted variety and all normal eligibility rules.
        if (BalanceV1.Current.cardWaves.Length > 0 && currentWave == BalanceV1.Current.cardWaves[0] && choiceCount >= 3)
        {
            foreach (RunUpgradeTheme theme in Enum.GetValues(typeof(RunUpgradeTheme)))
            {
                var themed = eligible.FindAll(card => card.Theme == theme);
                if (themed.Count == 0) continue;
                var card = PickWeighted(themed, random);
                choices.Add(card);
                eligible.Remove(card);
            }
        }

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

    public static List<RunUpgradeDefinition> Refine(RunUpgradeCatalog catalog, RunUpgradeRuntime runtime,
        PlayerActor player, int wave, Random random, RunUpgradeTheme theme,
        IReadOnlyCollection<RunUpgradeDefinition> prior)
    {
        var candidates = new List<RunUpgradeDefinition>();
        if (catalog == null || runtime == null || random == null) return candidates;
        foreach (var card in catalog.Upgrades)
            if (card != null && card.Theme == theme && runtime.IsEligible(card, player, wave) &&
                (prior == null || !System.Linq.Enumerable.Contains(prior, card))) candidates.Add(card);
        var result = new List<RunUpgradeDefinition>();
        while (candidates.Count > 0 && result.Count < DefaultChoiceCount)
        {
            var card = PickWeighted(candidates, random);
            result.Add(card);
            candidates.Remove(card);
        }
        return result;
    }

    private static RunUpgradeDefinition PickWeighted(List<RunUpgradeDefinition> cards, Random random)
    {
        double total = 0;
        foreach (var card in cards) total += GetEffectiveWeight(card);
        if (total <= 0) return cards[random.Next(cards.Count)];
        double roll = random.NextDouble() * total;
        foreach (var card in cards) { roll -= GetEffectiveWeight(card); if (roll < 0) return card; }
        return cards[cards.Count - 1];
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
