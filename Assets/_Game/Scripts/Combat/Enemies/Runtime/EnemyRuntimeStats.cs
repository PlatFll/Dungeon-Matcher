using UnityEngine;

public readonly struct EnemyRuntimeStats
{
    public int Wave { get; }
    public int Level { get; }

    public int MaxHealth { get; }
    public int Damage { get; }

    public float AttackInterval { get; }

    public int SpecialTurnRequirement { get; }

    public EnemyRuntimeStats(
        int wave,
        int level,
        int maxHealth,
        int damage,
        float attackInterval,
        int specialTurnRequirement)
    {
        Wave = Mathf.Max(1, wave);
        Level = Mathf.Max(1, level);

        MaxHealth = Mathf.Max(1, maxHealth);
        Damage = Mathf.Max(0, damage);

        AttackInterval = Mathf.Max(
            0.25f,
            attackInterval
        );

        SpecialTurnRequirement = Mathf.Max(
            1,
            specialTurnRequirement
        );
    }

    public override string ToString()
    {
        return
            $"Wave {Wave}, Level {Level}, " +
            $"HP {MaxHealth}, Damage {Damage}, " +
            $"Attack Interval {AttackInterval:0.00}s, " +
            $"Special Turns {SpecialTurnRequirement}";
    }
}