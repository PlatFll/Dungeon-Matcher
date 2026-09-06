using System;
using System.Collections.Generic;
using UnityEngine;

public static class RunUpgradeResolver
{
    public static int ResolveGemDamage(
        int baseValue,
        BoardClearContext clearContext,
        RunUpgradeRuntime runtime = null)
    {
        return ResolveInt(
            RunUpgradeStat.GemDamage,
            baseValue,
            0,
            runtime
        );
    }

    public static int ResolveMaximumHealth(
        int baseValue,
        RunUpgradeRuntime runtime = null)
    {
        return ResolveInt(
            RunUpgradeStat.MaximumHealth,
            baseValue,
            1,
            runtime
        );
    }

    public static int ResolveHealing(
        int baseValue,
        RunUpgradeRuntime runtime = null)
    {
        return ResolveInt(RunUpgradeStat.Healing, baseValue, 0, runtime);
    }

    public static int ResolveShieldGranted(
        int baseValue,
        RunUpgradeRuntime runtime = null)
    {
        return ResolveInt(
            RunUpgradeStat.ShieldGranted,
            baseValue,
            0,
            runtime
        );
    }

    public static int ResolveAbilityEnergyGain(
        int baseValue,
        RunUpgradeRuntime runtime = null)
    {
        return ResolveInt(
            RunUpgradeStat.AbilityEnergyGain,
            baseValue,
            0,
            runtime
        );
    }

    public static int ResolveAbilityEnergyCost(
        int baseValue,
        CharacterAbilityDefinition ability,
        RunUpgradeRuntime runtime = null)
    {
        return ResolveInt(
            RunUpgradeStat.AbilityEnergyCost,
            baseValue,
            1,
            runtime
        );
    }

    public static int ResolveBarricadeDurabilityDamage(
        int baseValue,
        RunUpgradeRuntime runtime = null)
    {
        return ResolveInt(
            RunUpgradeStat.BarricadeDurabilityDamage,
            baseValue,
            0,
            runtime
        );
    }

    public static int ResolveCrackedGemsTargetCount(
        int baseValue,
        CrackedGemsAbilityDefinition ability,
        RunUpgradeRuntime runtime = null)
    {
        return ResolveInt(
            RunUpgradeStat.CrackedGemsTargetCount,
            baseValue,
            1,
            runtime
        );
    }

    public static bool HasMechanic(
        RunUpgradeMechanic mechanic,
        RunUpgradeRuntime runtime = null)
    {
        RunUpgradeRuntime resolvedRuntime = runtime != null
            ? runtime
            : RunUpgradeRuntime.Current;

        return resolvedRuntime != null &&
               resolvedRuntime.HasMechanic(mechanic);
    }

    private static int ResolveInt(
        RunUpgradeStat stat,
        int baseValue,
        int minimumValue,
        RunUpgradeRuntime runtime)
    {
        RunUpgradeRuntime resolvedRuntime = runtime != null
            ? runtime
            : RunUpgradeRuntime.Current;

        if (resolvedRuntime == null)
        {
            return Mathf.Max(minimumValue, baseValue);
        }

        List<RunUpgradeDefinition> definitions =
            resolvedRuntime.GetOwnedDefinitions();

        double flat = 0d;
        double additivePercent = 0d;
        double multiplicative = 1d;

        for (int definitionIndex = 0;
             definitionIndex < definitions.Count;
             definitionIndex++)
        {
            RunUpgradeDefinition definition = definitions[definitionIndex];
            int stacks = resolvedRuntime.GetStackCount(definition);
            IReadOnlyList<RunUpgradeModifier> modifiers =
                definition.Modifiers;

            for (int modifierIndex = 0;
                 modifierIndex < modifiers.Count;
                 modifierIndex++)
            {
                RunUpgradeModifier modifier = modifiers[modifierIndex];

                if (modifier == null || modifier.Stat != stat)
                {
                    continue;
                }

                switch (modifier.Operation)
                {
                    case RunUpgradeModifierOperation.Flat:
                        flat += modifier.Value * stacks;
                        break;

                    case RunUpgradeModifierOperation.AdditivePercent:
                        additivePercent += modifier.Value * stacks;
                        break;

                    case RunUpgradeModifierOperation.Multiplicative:
                        multiplicative *= Math.Pow(
                            modifier.Value,
                            stacks
                        );
                        break;
                }
            }
        }

        double result = baseValue;
        result += flat;
        result *= 1d + additivePercent;
        result *= multiplicative;

        if (double.IsNaN(result) || double.IsInfinity(result))
        {
            return Mathf.Max(minimumValue, baseValue);
        }

        result = Math.Max(minimumValue, result);
        result = Math.Min(int.MaxValue, result);

        return Mathf.Max(
            minimumValue,
            Mathf.RoundToInt((float)result)
        );
    }
}
