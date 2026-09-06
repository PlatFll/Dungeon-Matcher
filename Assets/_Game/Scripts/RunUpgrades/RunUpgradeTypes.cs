using System;
using UnityEngine;

public enum RunUpgradeRarity
{
    Common = 0,
    Uncommon = 1,
    Rare = 2
}

public enum RunUpgradeStat
{
    GemDamage = 0,
    MaximumHealth = 1,
    Healing = 2,
    ShieldGranted = 3,
    AbilityEnergyGain = 4,
    AbilityEnergyCost = 5,
    BarricadeDurabilityDamage = 6,
    CrackedGemsTargetCount = 7
}

public enum RunUpgradeModifierOperation
{
    Flat = 0,
    AdditivePercent = 1,
    Multiplicative = 2
}

public enum RunUpgradeMechanic
{
    None = 0
}

[Serializable]
public sealed class RunUpgradeModifier
{
    [SerializeField]
    private RunUpgradeStat stat;

    [SerializeField]
    private RunUpgradeModifierOperation operation;

    [SerializeField]
    [Tooltip(
        "Flat uses raw units, Additive Percent uses 0.15 for +15%, " +
        "and Multiplicative uses 1.15 for x1.15."
    )]
    private float value;

    public RunUpgradeStat Stat => stat;
    public RunUpgradeModifierOperation Operation => operation;
    public float Value => value;
}

[Serializable]
public sealed class RunUpgradeMechanicGrant
{
    [SerializeField]
    private RunUpgradeMechanic mechanic;

    public RunUpgradeMechanic Mechanic => mechanic;
}
