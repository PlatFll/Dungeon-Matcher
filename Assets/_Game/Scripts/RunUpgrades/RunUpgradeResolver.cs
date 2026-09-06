using System;
using System.Collections.Generic;
using UnityEngine;

public static class RunUpgradeResolver
{
    private const float CascadeCatalystBonusPerDepth = 0.10f;
    private const float BombsmithDamageBonus = 0.25f;
    private const float ChainReactionBonusPerExtraSpecial = 0.10f;
    private const int ChainReactionMaximumBonusSteps = 3;
    private const float ToxicMomentumEnergyBonus = 0.20f;
    private const float BossHunterDamageBonus = 0.20f;
    private const float ExecutionerDamageBonus = 0.25f;
    private const float OpeningVolleyDamageBonus = 0.25f;

    public static int ResolveGemDamage(
        int baseValue,
        BoardClearContext clearContext,
        RunUpgradeRuntime runtime = null)
    {
        RunUpgradeRuntime resolvedRuntime = ResolveRuntime(runtime);
        int resolved = ResolveInt(
            RunUpgradeStat.GemDamage,
            baseValue,
            0,
            resolvedRuntime
        );

        if (resolvedRuntime == null || resolved <= 0)
        {
            return resolved;
        }

        float bonusPercent = 0f;

        if (resolvedRuntime.HasMechanic(RunUpgradeMechanic.CascadeCatalyst) &&
            clearContext.CascadeDepth > 0)
        {
            bonusPercent +=
                clearContext.CascadeDepth * CascadeCatalystBonusPerDepth;
        }

        bool isSpecialDamage =
            clearContext.Source == BoardClearSource.Bomb ||
            clearContext.Source == BoardClearSource.ColorCrystal ||
            clearContext.Source == BoardClearSource.DoubleColorCrystal ||
            (clearContext.Source == BoardClearSource.Ability &&
             clearContext.GrantsSpecialEnergy);

        if (isSpecialDamage &&
            resolvedRuntime.HasMechanic(RunUpgradeMechanic.Bombsmith) &&
            RunUpgradeGameplayHooks.CurrentDirectionalBombCount > 0)
        {
            bonusPercent += BombsmithDamageBonus;
        }

        if (isSpecialDamage &&
            resolvedRuntime.HasMechanic(RunUpgradeMechanic.ChainReaction) &&
            RunUpgradeGameplayHooks.CurrentSpecialClearCount > 1)
        {
            int bonusSteps = Mathf.Min(
                ChainReactionMaximumBonusSteps,
                RunUpgradeGameplayHooks.CurrentSpecialClearCount - 1
            );

            bonusPercent +=
                bonusSteps * ChainReactionBonusPerExtraSpecial;
        }

        return ApplyAdditivePercent(resolved, bonusPercent, 0);
    }

    public static int ResolveEnemyDamage(
        int baseValue,
        EnemyActor enemy,
        RunUpgradeRuntime runtime = null)
    {
        RunUpgradeRuntime resolvedRuntime = ResolveRuntime(runtime);
        int safeBase = Mathf.Max(0, baseValue);

        if (resolvedRuntime == null || enemy == null || safeBase <= 0)
        {
            return safeBase;
        }

        float bonusPercent = 0f;

        if (resolvedRuntime.HasMechanic(RunUpgradeMechanic.BossHunter) &&
            enemy.Definition != null &&
            (enemy.Definition.Category == EnemyCategory.Miniboss ||
             enemy.Definition.Category == EnemyCategory.Boss))
        {
            bonusPercent += BossHunterDamageBonus;
        }

        if (resolvedRuntime.HasMechanic(RunUpgradeMechanic.Executioner) &&
            enemy.HealthNormalized < 0.30f)
        {
            bonusPercent += ExecutionerDamageBonus;
        }

        if (resolvedRuntime.HasMechanic(RunUpgradeMechanic.OpeningVolley) &&
            enemy.HealthNormalized > 0.80f)
        {
            bonusPercent += OpeningVolleyDamageBonus;
        }

        return ApplyAdditivePercent(safeBase, bonusPercent, 0);
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

    public static int ResolveHealingBombHealing(
        int baseValue,
        RunUpgradeRuntime runtime = null)
    {
        RunUpgradeRuntime resolvedRuntime = ResolveRuntime(runtime);
        int globallyResolved = ResolveHealing(baseValue, resolvedRuntime);

        return ResolveInt(
            RunUpgradeStat.HealingBombHealing,
            globallyResolved,
            0,
            resolvedRuntime
        );
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

    public static int ResolveShieldBombShield(
        int baseValue,
        RunUpgradeRuntime runtime = null)
    {
        RunUpgradeRuntime resolvedRuntime = ResolveRuntime(runtime);
        int globallyResolved = ResolveShieldGranted(baseValue, resolvedRuntime);

        return ResolveInt(
            RunUpgradeStat.ShieldBombShield,
            globallyResolved,
            0,
            resolvedRuntime
        );
    }

    public static int ResolveAbilityEnergyGain(
        int baseValue,
        RunUpgradeRuntime runtime = null)
    {
        return ResolveAbilityEnergyGainInternal(
            baseValue,
            false,
            0,
            runtime
        );
    }

    public static int ResolveAbilityEnergyGain(
        int baseValue,
        BoardClearContext clearContext,
        RunUpgradeRuntime runtime = null)
    {
        return ResolveAbilityEnergyGainInternal(
            baseValue,
            clearContext.Source == BoardClearSource.ColorCrystal,
            clearContext.GemCount,
            runtime
        );
    }

    private static int ResolveAbilityEnergyGainInternal(
        int baseValue,
        bool isColorCrystalClear,
        int clearedGemCount,
        RunUpgradeRuntime runtime)
    {
        RunUpgradeRuntime resolvedRuntime = ResolveRuntime(runtime);
        int contextualBase = Mathf.Max(0, baseValue);

        if (resolvedRuntime != null &&
            isColorCrystalClear &&
            resolvedRuntime.HasMechanic(RunUpgradeMechanic.ChromaticConductor))
        {
            contextualBase += Mathf.Max(0, clearedGemCount);
        }

        int resolved = ResolveInt(
            RunUpgradeStat.AbilityEnergyGain,
            contextualBase,
            0,
            resolvedRuntime
        );

        if (resolvedRuntime != null &&
            resolvedRuntime.HasMechanic(RunUpgradeMechanic.ToxicMomentum) &&
            RunUpgradeGameplayHooks.HasAnyPoisonedEnemy)
        {
            resolved = ApplyAdditivePercent(
                resolved,
                ToxicMomentumEnergyBonus,
                0
            );
        }

        return resolved;
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

    public static int ResolveAbilityDamage(
        int baseValue,
        CharacterAbilityDefinition ability,
        RunUpgradeRuntime runtime = null)
    {
        return ResolveInt(
            RunUpgradeStat.AbilityDamage,
            baseValue,
            0,
            runtime
        );
    }

    public static int ResolveCrackedGemDamage(
        int baseValue,
        CrackedGemsAbilityDefinition ability,
        RunUpgradeRuntime runtime = null)
    {
        RunUpgradeRuntime resolvedRuntime = ResolveRuntime(runtime);
        int abilityResolved = ResolveAbilityDamage(
            baseValue,
            ability,
            resolvedRuntime
        );

        return ResolveInt(
            RunUpgradeStat.CrackedGemDamage,
            abilityResolved,
            1,
            resolvedRuntime
        );
    }

    public static float ResolveRoyalDecreeDuration(
        float baseValue,
        RoyalDecreeAbilityDefinition ability,
        RunUpgradeRuntime runtime = null)
    {
        return ResolveFloat(
            RunUpgradeStat.RoyalDecreeDuration,
            baseValue,
            0.1f,
            runtime
        );
    }

    public static int ResolveRoyalDecreeDamage(
        int baseValue,
        RoyalDecreeAbilityDefinition ability,
        RunUpgradeRuntime runtime = null)
    {
        RunUpgradeRuntime resolvedRuntime = ResolveRuntime(runtime);
        int abilityResolved = ResolveAbilityDamage(
            baseValue,
            ability,
            resolvedRuntime
        );

        return ResolveInt(
            RunUpgradeStat.RoyalDecreeDamage,
            abilityResolved,
            0,
            resolvedRuntime
        );
    }

    public static int ResolvePoisonTickDamage(
        int baseValue,
        RunUpgradeRuntime runtime = null)
    {
        return ResolveInt(
            RunUpgradeStat.PoisonTickDamage,
            baseValue,
            1,
            runtime
        );
    }

    public static float ResolvePoisonDuration(
        float baseValue,
        RunUpgradeRuntime runtime = null)
    {
        return ResolveFloat(
            RunUpgradeStat.PoisonDuration,
            baseValue,
            0.05f,
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
        RunUpgradeRuntime resolvedRuntime = ResolveRuntime(runtime);

        return resolvedRuntime != null &&
               resolvedRuntime.HasMechanic(mechanic);
    }

    private static RunUpgradeRuntime ResolveRuntime(
        RunUpgradeRuntime runtime)
    {
        return runtime != null
            ? runtime
            : RunUpgradeRuntime.Current;
    }

    private static int ResolveInt(
        RunUpgradeStat stat,
        int baseValue,
        int minimumValue,
        RunUpgradeRuntime runtime)
    {
        RunUpgradeRuntime resolvedRuntime = ResolveRuntime(runtime);

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
            IReadOnlyList<RunUpgradeModifier> modifiers = definition.Modifiers;

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
                        multiplicative *= Math.Pow(modifier.Value, stacks);
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

    private static float ResolveFloat(
        RunUpgradeStat stat,
        float baseValue,
        float minimumValue,
        RunUpgradeRuntime runtime)
    {
        RunUpgradeRuntime resolvedRuntime = ResolveRuntime(runtime);

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
            IReadOnlyList<RunUpgradeModifier> modifiers = definition.Modifiers;

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
                        multiplicative *= Math.Pow(modifier.Value, stacks);
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

        return Mathf.Max(minimumValue, (float)result);
    }

    private static int ApplyAdditivePercent(
        int baseValue,
        float additivePercent,
        int minimumValue)
    {
        if (additivePercent == 0f)
        {
            return Mathf.Max(minimumValue, baseValue);
        }

        double result = baseValue * (1d + additivePercent);

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
