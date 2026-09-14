using System;
using UnityEngine;

/// <summary>One editable balance resource. Consumers never modify this data.</summary>
[Serializable]
public sealed class BalanceV1
{
    public int levelCap = 20;
    public int upgradeBase = 25;
    public int upgradeLinear = 15;
    public int upgradeQuadratic = 5;
    public int directionalLevel = 2;
    public int poisonLevel = 3;
    public int healingLevel = 4;
    public int shieldLevel = 5;
    public int potionPrice = 18;
    public int bombPrice = 24;
    public float potionHealthFraction = 0.35f;
    public float consumableCooldown = 5f;
    public int maximumRunCharges = 3;
    public int consumableBombRadius = 1;
    public int maximumGlobalMines = 4;
    public int maximumGlobalStructures = 10;
    public int maximumGlobalChains = 6;
    public int shieldBombBase = 25;
    public int shieldBombGrowth = 2;
    public int waveGoldBase = 5;
    public int waveGoldDivisor = 3;
    public int milestoneGold = 12;
    public int kingGold = 60;
    public int bestWaveGold = 2;
    public int firstKingGold = 80;
    public int[] cardWaves = { 2, 5, 9, 13, 17, 21, 25, 28 };

    private static BalanceV1 current;
    public static BalanceV1 Current
    {
        get
        {
            if (current == null)
            {
                var data = Resources.Load<TextAsset>("Balance/BalanceV1");
                current = data != null ? JsonUtility.FromJson<BalanceV1>(data.text) : new BalanceV1();
            }
            return current;
        }
    }

    public int UpgradeCost(int level)
    {
        int n = Mathf.Clamp(level, 1, levelCap) - 1;
        return upgradeBase + upgradeLinear * n + upgradeQuadratic * n * n;
    }

    public int UnlockLevel(GemSpecialType type)
    {
        switch (type)
        {
            case GemSpecialType.RowBomb:
            case GemSpecialType.ColumnBomb: return directionalLevel;
            case GemSpecialType.PoisonBomb: return poisonLevel;
            case GemSpecialType.HealingBomb: return healingLevel;
            case GemSpecialType.ShieldBomb: return shieldLevel;
            case GemSpecialType.ColorCrystal: return 1;
            default: return int.MaxValue;
        }
    }

    public bool OffersCard(int completedWave) => Array.IndexOf(cardWaves, completedWave) >= 0;
    public int WaveGold(int wave) => waveGoldBase + Mathf.Max(0, wave - 1) / Mathf.Max(1, waveGoldDivisor);
}
