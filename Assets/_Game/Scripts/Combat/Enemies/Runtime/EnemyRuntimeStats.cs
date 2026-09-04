using UnityEngine;

public readonly struct EnemyRuntimeStats
{
    public int Wave { get; }
    public int Level { get; }

    public int MaxHealth { get; }
    public int Damage { get; }
    public int FollowUpDamage { get; }
    public float DamageMultiplier { get; }

    public float AttackInterval { get; }

    public int SpecialTurnRequirement { get; }

    public EnemyRuntimeStats(
        int wave,
        int level,
        int maxHealth,
        int damage,
        int followUpDamage,
        float attackInterval,
        int specialTurnRequirement,
        float damageMultiplier = 1f)
    {
        DamageMultiplier = Mathf.Max(0f, damageMultiplier);
        Wave = Mathf.Max(1, wave);
        Level = Mathf.Max(1, level);

        MaxHealth = Mathf.Max(1, maxHealth);
        Damage = Mathf.Max(0, damage);
        FollowUpDamage = Mathf.Max(
            0,
            followUpDamage
        );

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
            $"Follow-up Damage {FollowUpDamage}, " +
            $"Attack Interval {AttackInterval:0.00}s, " +
            $"Special Turns {SpecialTurnRequirement}";
    }
}
