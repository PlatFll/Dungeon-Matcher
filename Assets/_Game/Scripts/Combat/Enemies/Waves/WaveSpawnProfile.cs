using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class EnemyCategoryWeight
{
    [SerializeField]
    private EnemyCategory category = EnemyCategory.Normal;

    [SerializeField, Min(0f)]
    private float weight = 1f;

    [SerializeField, Range(0, 3)]
    [Tooltip(
        "Maximum enemies of this category in one wave. " +
        "Set to 0 for no category limit."
    )]
    private int maximumPerWave = 3;

    public EnemyCategory Category => category;
    public float Weight => Mathf.Max(0f, weight);
    public int MaximumPerWave => maximumPerWave;

    public EnemyCategoryWeight()
    {
    }

    public EnemyCategoryWeight(
        EnemyCategory category,
        float weight,
        int maximumPerWave)
    {
        this.category = category;
        this.weight = weight;
        this.maximumPerWave = maximumPerWave;
    }

    public bool CanSelect(int currentCount)
    {
        return maximumPerWave == 0 ||
               currentCount < maximumPerWave;
    }
}

[Serializable]
public sealed class WaveBandRule
{
    [SerializeField]
    private string ruleName = "Wave Band";

    [SerializeField, Min(1)]
    private int minimumWave = 1;

    [SerializeField, Min(0)]
    [Tooltip(
        "Final wave included in this band. " +
        "Set to 0 for an endless upper range."
    )]
    private int maximumWave;

    [SerializeField, Range(1, 3)]
    private int minimumEnemyCount = 1;

    [SerializeField, Range(1, 3)]
    private int maximumEnemyCount = 1;

    [SerializeField]
    [Tooltip(
        "These categories are added first. " +
        "Remaining slots use the weighted category list."
    )]
    private List<EnemyCategory> guaranteedCategories =
        new List<EnemyCategory>();

    [SerializeField]
    private List<EnemyCategoryWeight> categoryWeights =
        new List<EnemyCategoryWeight>();

    public string RuleName => ruleName;
    public int MinimumWave => minimumWave;
    public int MaximumWave => maximumWave;

    public int MinimumEnemyCount =>
        Mathf.Clamp(minimumEnemyCount, 1, 3);

    public int MaximumEnemyCount =>
        Mathf.Clamp(maximumEnemyCount, MinimumEnemyCount, 3);

    public IReadOnlyList<EnemyCategory> GuaranteedCategories =>
        guaranteedCategories;

    public IReadOnlyList<EnemyCategoryWeight> CategoryWeights =>
        categoryWeights;

    public WaveBandRule()
    {
    }

    public WaveBandRule(
        string ruleName,
        int minimumWave,
        int maximumWave,
        int minimumEnemyCount,
        int maximumEnemyCount,
        IEnumerable<EnemyCategory> guaranteedCategories,
        IEnumerable<EnemyCategoryWeight> categoryWeights)
    {
        this.ruleName = ruleName;
        this.minimumWave = minimumWave;
        this.maximumWave = maximumWave;
        this.minimumEnemyCount = minimumEnemyCount;
        this.maximumEnemyCount = maximumEnemyCount;

        this.guaranteedCategories =
            new List<EnemyCategory>(guaranteedCategories);

        this.categoryWeights =
            new List<EnemyCategoryWeight>(categoryWeights);
    }

    public bool ContainsWave(int wave)
    {
        if (wave < minimumWave)
        {
            return false;
        }

        return maximumWave == 0 ||
               wave <= maximumWave;
    }

    public void Validate()
    {
        minimumWave = Mathf.Max(1, minimumWave);

        if (maximumWave != 0)
        {
            maximumWave =
                Mathf.Max(minimumWave, maximumWave);
        }

        minimumEnemyCount =
            Mathf.Clamp(minimumEnemyCount, 1, 3);

        maximumEnemyCount =
            Mathf.Clamp(
                maximumEnemyCount,
                minimumEnemyCount,
                3
            );

        guaranteedCategories ??=
            new List<EnemyCategory>();

        categoryWeights ??=
            new List<EnemyCategoryWeight>();
    }
}

[Serializable]
public sealed class ExactWaveRule
{
    [SerializeField, Min(1)]
    private int wave = 5;

    [SerializeField]
    private string ruleName = "Special Wave";

    [SerializeField]
    [Tooltip(
        "The exact ordered category composition for this wave."
    )]
    private List<EnemyCategory> enemyCategories =
        new List<EnemyCategory>
        {
            EnemyCategory.Normal,
            EnemyCategory.Normal,
            EnemyCategory.Special
        };

    public int Wave => wave;
    public string RuleName => ruleName;

    public IReadOnlyList<EnemyCategory> EnemyCategories =>
        enemyCategories;

    public ExactWaveRule()
    {
    }

    public ExactWaveRule(
        int wave,
        string ruleName,
        IEnumerable<EnemyCategory> enemyCategories)
    {
        this.wave = wave;
        this.ruleName = ruleName;

        this.enemyCategories =
            new List<EnemyCategory>(enemyCategories);
    }

    public void Validate()
    {
        wave = Mathf.Max(1, wave);

        enemyCategories ??=
            new List<EnemyCategory>();

        while (enemyCategories.Count > 3)
        {
            enemyCategories.RemoveAt(
                enemyCategories.Count - 1
            );
        }
    }
}

public sealed class WaveSpawnPlan
{
    private readonly List<EnemyCategory> categories;

    public int Wave { get; }
    public string SourceRuleName { get; }

    public IReadOnlyList<EnemyCategory> Categories =>
        categories;

    public int EnemyCount => categories.Count;

    public WaveSpawnPlan(
        int wave,
        string sourceRuleName,
        IEnumerable<EnemyCategory> categories)
    {
        Wave = Mathf.Max(1, wave);
        SourceRuleName = sourceRuleName;

        this.categories =
            new List<EnemyCategory>(categories);
    }
}

[CreateAssetMenu(
    fileName = "WaveSpawnProfile_",
    menuName = "Dungeon Matcher/Waves/Wave Spawn Profile"
)]
public sealed class WaveSpawnProfile : ScriptableObject
{
    [Header("Exact Wave Overrides")]
    [SerializeField]
    private List<ExactWaveRule> exactWaveRules =
        new List<ExactWaveRule>
        {
            new ExactWaveRule(
                5,
                "First Special Encounter",
                new[]
                {
                    EnemyCategory.Normal,
                    EnemyCategory.Normal,
                    EnemyCategory.Special
                }
            )
        };

    [Header("Wave Bands")]
    [SerializeField]
    private List<WaveBandRule> waveBands =
        new List<WaveBandRule>
        {
            new WaveBandRule(
                "Introduction",
                1,
                2,
                1,
                1,
                new[]
                {
                    EnemyCategory.Normal
                },
                new[]
                {
                    new EnemyCategoryWeight(
                        EnemyCategory.Normal,
                        100f,
                        0
                    )
                }
            ),

            new WaveBandRule(
                "Early Combat",
                3,
                4,
                2,
                2,
                new[]
                {
                    EnemyCategory.Normal,
                    EnemyCategory.Normal
                },
                new[]
                {
                    new EnemyCategoryWeight(
                        EnemyCategory.Normal,
                        100f,
                        0
                    )
                }
            ),

            new WaveBandRule(
                "Developing",
                6,
                9,
                2,
                3,
                new[]
                {
                    EnemyCategory.Normal
                },
                new[]
                {
                    new EnemyCategoryWeight(
                        EnemyCategory.Normal,
                        75f,
                        0
                    ),

                    new EnemyCategoryWeight(
                        EnemyCategory.Special,
                        25f,
                        1
                    )
                }
            ),

            new WaveBandRule(
                "Escalation",
                10,
                14,
                3,
                3,
                new[]
                {
                    EnemyCategory.Normal
                },
                new[]
                {
                    new EnemyCategoryWeight(
                        EnemyCategory.Normal,
                        55f,
                        0
                    ),

                    new EnemyCategoryWeight(
                        EnemyCategory.Special,
                        45f,
                        2
                    )
                }
            ),

            new WaveBandRule(
                "Miniboss Introduction",
                15,
                19,
                3,
                3,
                Array.Empty<EnemyCategory>(),
                new[]
                {
                    new EnemyCategoryWeight(
                        EnemyCategory.Normal,
                        45f,
                        0
                    ),

                    new EnemyCategoryWeight(
                        EnemyCategory.Special,
                        45f,
                        2
                    ),

                    new EnemyCategoryWeight(
                        EnemyCategory.Miniboss,
                        10f,
                        1
                    )
                }
            ),

            new WaveBandRule(
                "Endless Pressure",
                20,
                0,
                3,
                3,
                Array.Empty<EnemyCategory>(),
                new[]
                {
                    new EnemyCategoryWeight(
                        EnemyCategory.Normal,
                        35f,
                        0
                    ),

                    new EnemyCategoryWeight(
                        EnemyCategory.Special,
                        50f,
                        2
                    ),

                    new EnemyCategoryWeight(
                        EnemyCategory.Miniboss,
                        15f,
                        1
                    )
                }
            )
        };

    public WaveSpawnPlan CreatePlan(
        int wave,
        System.Random deterministicRandom = null)
    {
        wave = Mathf.Max(1, wave);

        ExactWaveRule exactRule =
            FindExactWaveRule(wave);

        if (exactRule != null &&
            exactRule.EnemyCategories.Count > 0)
        {
            return new WaveSpawnPlan(
                wave,
                exactRule.RuleName,
                exactRule.EnemyCategories
            );
        }

        WaveBandRule band =
            FindWaveBand(wave);

        if (band == null)
        {
            Debug.LogError(
                $"No wave spawn rule covers wave {wave}. " +
                "One Normal enemy will be used.",
                this
            );

            return new WaveSpawnPlan(
                wave,
                "Emergency Fallback",
                new[]
                {
                    EnemyCategory.Normal
                }
            );
        }

        int enemyCount = NextIntInclusive(
            band.MinimumEnemyCount,
            band.MaximumEnemyCount,
            deterministicRandom
        );

        List<EnemyCategory> categories =
            new List<EnemyCategory>(enemyCount);

        Dictionary<EnemyCategory, int> categoryCounts =
            new Dictionary<EnemyCategory, int>();

        foreach (
            EnemyCategory guaranteedCategory
            in band.GuaranteedCategories)
        {
            if (categories.Count >= enemyCount)
            {
                break;
            }

            AddCategory(
                guaranteedCategory,
                categories,
                categoryCounts
            );
        }

        while (categories.Count < enemyCount)
        {
            EnemyCategory? selectedCategory =
                SelectWeightedCategory(
                    band.CategoryWeights,
                    categoryCounts,
                    deterministicRandom
                );

            if (!selectedCategory.HasValue)
            {
                Debug.LogWarning(
                    $"{band.RuleName} could not fill all " +
                    $"enemy slots from its weighted categories. " +
                    "Normal will be used as a fallback.",
                    this
                );

                selectedCategory =
                    EnemyCategory.Normal;
            }

            AddCategory(
                selectedCategory.Value,
                categories,
                categoryCounts
            );
        }

        return new WaveSpawnPlan(
            wave,
            band.RuleName,
            categories
        );
    }

    private ExactWaveRule FindExactWaveRule(int wave)
    {
        if (exactWaveRules == null)
        {
            return null;
        }

        foreach (ExactWaveRule rule in exactWaveRules)
        {
            if (rule != null &&
                rule.Wave == wave)
            {
                return rule;
            }
        }

        return null;
    }

    private WaveBandRule FindWaveBand(int wave)
    {
        if (waveBands == null)
        {
            return null;
        }

        foreach (WaveBandRule rule in waveBands)
        {
            if (rule != null &&
                rule.ContainsWave(wave))
            {
                return rule;
            }
        }

        return null;
    }

    private EnemyCategory? SelectWeightedCategory(
        IReadOnlyList<EnemyCategoryWeight> weights,
        Dictionary<EnemyCategory, int> categoryCounts,
        System.Random deterministicRandom)
    {
        if (weights == null ||
            weights.Count == 0)
        {
            return null;
        }

        float totalWeight = 0f;

        foreach (EnemyCategoryWeight option in weights)
        {
            if (option == null ||
                option.Weight <= 0f)
            {
                continue;
            }

            int currentCount =
                GetCategoryCount(
                    option.Category,
                    categoryCounts
                );

            if (!option.CanSelect(currentCount))
            {
                continue;
            }

            totalWeight += option.Weight;
        }

        if (totalWeight <= 0f)
        {
            return null;
        }

        float roll =
            NextFloat(deterministicRandom) *
            totalWeight;

        float accumulatedWeight = 0f;

        foreach (EnemyCategoryWeight option in weights)
        {
            if (option == null ||
                option.Weight <= 0f)
            {
                continue;
            }

            int currentCount =
                GetCategoryCount(
                    option.Category,
                    categoryCounts
                );

            if (!option.CanSelect(currentCount))
            {
                continue;
            }

            accumulatedWeight += option.Weight;

            if (roll <= accumulatedWeight)
            {
                return option.Category;
            }
        }

        return null;
    }

    private static void AddCategory(
        EnemyCategory category,
        ICollection<EnemyCategory> categories,
        IDictionary<EnemyCategory, int> categoryCounts)
    {
        categories.Add(category);

        if (!categoryCounts.ContainsKey(category))
        {
            categoryCounts[category] = 0;
        }

        categoryCounts[category]++;
    }

    private static int GetCategoryCount(
        EnemyCategory category,
        IReadOnlyDictionary<EnemyCategory, int> categoryCounts)
    {
        return categoryCounts.TryGetValue(
            category,
            out int count
        )
            ? count
            : 0;
    }

    private static int NextIntInclusive(
        int minimum,
        int maximum,
        System.Random deterministicRandom)
    {
        if (minimum >= maximum)
        {
            return minimum;
        }

        return deterministicRandom != null
            ? deterministicRandom.Next(
                minimum,
                maximum + 1
            )
            : UnityEngine.Random.Range(
                minimum,
                maximum + 1
            );
    }

    private static float NextFloat(
        System.Random deterministicRandom)
    {
        return deterministicRandom != null
            ? (float)deterministicRandom.NextDouble()
            : UnityEngine.Random.value;
    }

    private void OnValidate()
    {
        exactWaveRules ??=
            new List<ExactWaveRule>();

        waveBands ??=
            new List<WaveBandRule>();

        HashSet<int> exactWaves =
            new HashSet<int>();

        foreach (ExactWaveRule rule in exactWaveRules)
        {
            if (rule == null)
            {
                continue;
            }

            rule.Validate();

            if (!exactWaves.Add(rule.Wave))
            {
                Debug.LogWarning(
                    $"More than one exact rule exists for " +
                    $"wave {rule.Wave}. The first rule will be used.",
                    this
                );
            }
        }

        foreach (WaveBandRule rule in waveBands)
        {
            rule?.Validate();
        }
    }
}