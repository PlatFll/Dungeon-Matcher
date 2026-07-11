using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "DifficultyProfile_",
    menuName = "Dungeon Matcher/Balance/Difficulty Profile"
)]
public sealed class DifficultyProfile : ScriptableObject
{
    [Serializable]
    private sealed class CategoryModifier
    {
        [SerializeField]
        private EnemyCategory category = EnemyCategory.Normal;

        [SerializeField, Min(0.1f)]
        private float healthMultiplier = 1f;

        [SerializeField, Min(0.1f)]
        private float damageMultiplier = 1f;

        [SerializeField, Min(0.1f)]
        [Tooltip("Values above 1 make the enemy attack faster.")]
        private float attackSpeedMultiplier = 1f;

        [SerializeField]
        private int levelOffset;

        [SerializeField]
        [Tooltip(
            "Added to the enemy's special-turn requirement. " +
            "Negative values make the special happen sooner."
        )]
        private int specialTurnAdjustment;

        public EnemyCategory Category => category;
        public float HealthMultiplier => healthMultiplier;
        public float DamageMultiplier => damageMultiplier;

        public float AttackSpeedMultiplier =>
            attackSpeedMultiplier;

        public int LevelOffset => levelOffset;

        public int SpecialTurnAdjustment =>
            specialTurnAdjustment;

        public CategoryModifier(
            EnemyCategory category,
            float healthMultiplier,
            float damageMultiplier,
            float attackSpeedMultiplier,
            int levelOffset,
            int specialTurnAdjustment)
        {
            this.category = category;
            this.healthMultiplier = healthMultiplier;
            this.damageMultiplier = damageMultiplier;
            this.attackSpeedMultiplier = attackSpeedMultiplier;
            this.levelOffset = levelOffset;
            this.specialTurnAdjustment = specialTurnAdjustment;
        }
    }

    [Header("Wave Scaling Curves")]
    [Tooltip(
        "Horizontal axis: wave. Vertical axis: HP multiplier."
    )]
    [SerializeField]
    private AnimationCurve healthMultiplierByWave =
        new AnimationCurve(
            new Keyframe(1f, 1f),
            new Keyframe(5f, 1.25f),
            new Keyframe(10f, 1.65f),
            new Keyframe(20f, 2.6f),
            new Keyframe(40f, 5f),
            new Keyframe(100f, 12f)
        );

    [Tooltip(
        "Horizontal axis: wave. Vertical axis: damage multiplier."
    )]
    [SerializeField]
    private AnimationCurve damageMultiplierByWave =
        new AnimationCurve(
            new Keyframe(1f, 1f),
            new Keyframe(5f, 1.12f),
            new Keyframe(10f, 1.35f),
            new Keyframe(20f, 1.9f),
            new Keyframe(40f, 3.2f),
            new Keyframe(100f, 7f)
        );

    [Tooltip(
        "Values above 1 make attacks faster. " +
        "The enemy's interval is divided by this value."
    )]
    [SerializeField]
    private AnimationCurve attackSpeedMultiplierByWave =
        new AnimationCurve(
            new Keyframe(1f, 1f),
            new Keyframe(10f, 1.05f),
            new Keyframe(20f, 1.12f),
            new Keyframe(40f, 1.22f),
            new Keyframe(100f, 1.35f)
        );

    [Tooltip(
        "Number of turns removed from special ability counters."
    )]
    [SerializeField]
    private AnimationCurve specialTurnReductionByWave =
        new AnimationCurve(
            new Keyframe(1f, 0f),
            new Keyframe(9f, 0f),
            new Keyframe(10f, 1f),
            new Keyframe(24f, 1f),
            new Keyframe(25f, 2f),
            new Keyframe(49f, 2f),
            new Keyframe(50f, 3f),
            new Keyframe(100f, 3f)
        );

    [Header("Endless Scaling")]
    [SerializeField, Min(1)]
    private int configuredCurveWaveLimit = 100;

    [SerializeField, Min(0f)]
    [Tooltip(
        "Additional linear HP growth per wave after " +
        "the configured curve limit."
    )]
    private float healthGrowthPerWaveAfterLimit = 0.03f;

    [SerializeField, Min(0f)]
    private float damageGrowthPerWaveAfterLimit = 0.015f;

    [Header("Pressure Limits")]
    [SerializeField, Min(0.1f)]
    [Tooltip(
        "No enemy attack interval can fall below this value."
    )]
    private float minimumAttackInterval = 1.25f;

    [SerializeField, Min(1)]
    private int minimumSpecialTurnRequirement = 2;

    [SerializeField, Min(0)]
    private int maximumSpecialTurnReduction = 3;

    [Header("Player Power Correction")]
    [SerializeField]
    private bool usePlayerPowerCorrection = true;

    [SerializeField, Range(0f, 0.3f)]
    [Tooltip(
        "Maximum extra enemy HP caused by an " +
        "above-expected player build."
    )]
    private float maximumPlayerPowerHealthBoost = 0.15f;

    [SerializeField, Range(0f, 1f)]
    [Tooltip(
        "How strongly excess player power affects enemy HP."
    )]
    private float playerPowerResponse = 0.25f;

    [Header("Category Modifiers")]
    [SerializeField]
    private List<CategoryModifier> categoryModifiers =
        new List<CategoryModifier>
        {
            new CategoryModifier(
                EnemyCategory.Normal,
                1f,
                1f,
                1f,
                0,
                0
            ),

            new CategoryModifier(
                EnemyCategory.Special,
                1.2f,
                1.1f,
                1.05f,
                0,
                0
            ),

            new CategoryModifier(
                EnemyCategory.Miniboss,
                2.2f,
                1.25f,
                0.9f,
                2,
                -1
            ),

            new CategoryModifier(
                EnemyCategory.Boss,
                4f,
                1.5f,
                0.85f,
                5,
                -1
            )
        };

    /// <summary>
    /// Calculates the runtime statistics for one enemy.
    ///
    /// playerPowerRatio:
    /// 1.0 = expected strength for this wave.
    /// 1.5 = approximately 50% above expected strength.
    /// Values below 1 do not reduce enemy strength.
    /// </summary>
    public EnemyRuntimeStats CalculateStats(
        EnemyDefinition definition,
        int wave,
        float playerPowerRatio = 1f)
    {
        if (definition == null)
        {
            Debug.LogError(
                "Cannot calculate enemy stats without " +
                "an EnemyDefinition."
            );

            return new EnemyRuntimeStats(
                wave: 1,
                level: 1,
                maxHealth: 1,
                damage: 0,
                attackInterval: minimumAttackInterval,
                specialTurnRequirement:
                    minimumSpecialTurnRequirement
            );
        }

        wave = Mathf.Max(1, wave);

        CategoryModifier categoryModifier =
            GetCategoryModifier(definition.Category);

        float waveHealthMultiplier =
            EvaluateEndlessCurve(
                healthMultiplierByWave,
                wave,
                healthGrowthPerWaveAfterLimit
            );

        float waveDamageMultiplier =
            EvaluateEndlessCurve(
                damageMultiplierByWave,
                wave,
                damageGrowthPerWaveAfterLimit
            );

        float waveAttackSpeedMultiplier =
            Mathf.Max(
                0.1f,
                attackSpeedMultiplierByWave.Evaluate(
                    Mathf.Min(
                        wave,
                        configuredCurveWaveLimit
                    )
                )
            );

        float playerPowerHealthMultiplier =
            CalculatePlayerPowerHealthMultiplier(
                playerPowerRatio
            );

        float calculatedHealth =
            definition.BaseMaxHealth *
            waveHealthMultiplier *
            categoryModifier.HealthMultiplier *
            definition.HealthMultiplier *
            playerPowerHealthMultiplier;

        float calculatedDamage =
            definition.BaseDamage *
            waveDamageMultiplier *
            categoryModifier.DamageMultiplier *
            definition.DamageMultiplier;

        float totalAttackSpeedMultiplier =
            waveAttackSpeedMultiplier *
            categoryModifier.AttackSpeedMultiplier *
            definition.AttackSpeedMultiplier;

        float calculatedAttackInterval =
            definition.BaseAttackInterval /
            Mathf.Max(
                0.1f,
                totalAttackSpeedMultiplier
            );

        calculatedAttackInterval =
            Mathf.Max(
                minimumAttackInterval,
                calculatedAttackInterval
            );

        int specialTurnReduction =
            Mathf.Clamp(
                Mathf.RoundToInt(
                    specialTurnReductionByWave.Evaluate(
                        Mathf.Min(
                            wave,
                            configuredCurveWaveLimit
                        )
                    )
                ),
                0,
                maximumSpecialTurnReduction
            );

        int calculatedSpecialTurns =
            definition.BaseSpecialTurnRequirement +
            categoryModifier.SpecialTurnAdjustment -
            specialTurnReduction;

        calculatedSpecialTurns =
            Mathf.Max(
                minimumSpecialTurnRequirement,
                calculatedSpecialTurns
            );

        int calculatedLevel =
            Mathf.Max(
                1,
                wave + categoryModifier.LevelOffset
            );

        return new EnemyRuntimeStats(
            wave: wave,
            level: calculatedLevel,
            maxHealth: Mathf.Max(
                1,
                Mathf.RoundToInt(calculatedHealth)
            ),
            damage: Mathf.Max(
                0,
                Mathf.RoundToInt(calculatedDamage)
            ),
            attackInterval: calculatedAttackInterval,
            specialTurnRequirement:
                calculatedSpecialTurns
        );
    }

    private float EvaluateEndlessCurve(
        AnimationCurve curve,
        int wave,
        float growthPerWaveAfterLimit)
    {
        if (curve == null ||
            curve.length == 0)
        {
            return 1f;
        }

        int evaluatedWave =
            Mathf.Min(
                wave,
                configuredCurveWaveLimit
            );

        float baseValue =
            Mathf.Max(
                0.01f,
                curve.Evaluate(evaluatedWave)
            );

        int extraWaves =
            Mathf.Max(
                0,
                wave - configuredCurveWaveLimit
            );

        float extraGrowthMultiplier =
            1f +
            extraWaves *
            Mathf.Max(
                0f,
                growthPerWaveAfterLimit
            );

        return baseValue *
               extraGrowthMultiplier;
    }

    private float CalculatePlayerPowerHealthMultiplier(
        float playerPowerRatio)
    {
        if (!usePlayerPowerCorrection)
        {
            return 1f;
        }

        playerPowerRatio =
            Mathf.Max(
                0.01f,
                playerPowerRatio
            );

        float excessPower =
            Mathf.Max(
                0f,
                playerPowerRatio - 1f
            );

        float healthBoost =
            excessPower *
            playerPowerResponse;

        healthBoost =
            Mathf.Min(
                maximumPlayerPowerHealthBoost,
                healthBoost
            );

        return 1f + healthBoost;
    }

    private CategoryModifier GetCategoryModifier(
        EnemyCategory category)
    {
        if (categoryModifiers != null)
        {
            foreach (
                CategoryModifier modifier
                in categoryModifiers)
            {
                if (modifier != null &&
                    modifier.Category == category)
                {
                    return modifier;
                }
            }
        }

        Debug.LogWarning(
            $"Difficulty profile has no modifier for " +
            $"{category}. Neutral values will be used.",
            this
        );

        return new CategoryModifier(
            category,
            1f,
            1f,
            1f,
            0,
            0
        );
    }

    private void OnValidate()
    {
        configuredCurveWaveLimit =
            Mathf.Max(
                1,
                configuredCurveWaveLimit
            );

        minimumAttackInterval =
            Mathf.Max(
                0.1f,
                minimumAttackInterval
            );

        minimumSpecialTurnRequirement =
            Mathf.Max(
                1,
                minimumSpecialTurnRequirement
            );

        maximumSpecialTurnReduction =
            Mathf.Max(
                0,
                maximumSpecialTurnReduction
            );
    }
}