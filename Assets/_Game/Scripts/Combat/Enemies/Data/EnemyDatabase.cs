using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "EnemyDatabase",
    menuName = "Dungeon Matcher/Enemies/Enemy Database"
)]
public sealed class EnemyDatabase : ScriptableObject
{
    [SerializeField]
    private List<EnemyDefinition> enemies =
        new List<EnemyDefinition>();

    public IReadOnlyList<EnemyDefinition> Enemies =>
        enemies;

    public List<EnemyDefinition> GetEligibleEnemies(
        EnemyCategory category,
        int wave,
        ISet<EnemyDefinition> excludedEnemies = null)
    {
        wave = Mathf.Max(1, wave);

        List<EnemyDefinition> eligibleEnemies =
            new List<EnemyDefinition>();

        if (enemies == null)
        {
            return eligibleEnemies;
        }

        foreach (EnemyDefinition definition in enemies)
        {
            if (definition == null)
            {
                continue;
            }

            if (definition.Category != category)
            {
                continue;
            }

            if (wave < definition.MinimumWave)
            {
                continue;
            }

            if (definition.EnemyPrefab == null)
            {
                continue;
            }

            if (excludedEnemies != null &&
                excludedEnemies.Contains(definition))
            {
                continue;
            }

            eligibleEnemies.Add(definition);
        }

        return eligibleEnemies;
    }

    public bool TryGetRandomWeightedEnemy(
        EnemyCategory category,
        int wave,
        out EnemyDefinition selectedEnemy,
        ISet<EnemyDefinition> excludedEnemies = null)
    {
        List<EnemyDefinition> eligibleEnemies =
            GetEligibleEnemies(
                category,
                wave,
                excludedEnemies
            );

        if (eligibleEnemies.Count == 0)
        {
            selectedEnemy = null;

            Debug.LogWarning(
                $"No eligible {category} enemies were found " +
                $"for wave {wave}.",
                this
            );

            return false;
        }

        float totalWeight = 0f;

        foreach (EnemyDefinition definition in eligibleEnemies)
        {
            totalWeight += Mathf.Max(
                0.01f,
                definition.SpawnWeight
            );
        }

        float randomValue =
            Random.Range(0f, totalWeight);

        float accumulatedWeight = 0f;

        foreach (EnemyDefinition definition in eligibleEnemies)
        {
            accumulatedWeight += Mathf.Max(
                0.01f,
                definition.SpawnWeight
            );

            if (randomValue <= accumulatedWeight)
            {
                selectedEnemy = definition;
                return true;
            }
        }

        // Floating-point fallback.
        selectedEnemy =
            eligibleEnemies[eligibleEnemies.Count - 1];

        return true;
    }

    public bool ContainsEnemy(
        EnemyDefinition definition)
    {
        return definition != null &&
               enemies != null &&
               enemies.Contains(definition);
    }

    private void OnValidate()
    {
        if (enemies == null)
        {
            enemies =
                new List<EnemyDefinition>();

            return;
        }

        HashSet<EnemyDefinition> duplicateAssets =
            new HashSet<EnemyDefinition>();

        HashSet<string> usedIds =
            new HashSet<string>();

        foreach (EnemyDefinition definition in enemies)
        {
            if (definition == null)
            {
                continue;
            }

            if (!duplicateAssets.Add(definition))
            {
                Debug.LogWarning(
                    $"Enemy database contains the asset " +
                    $"{definition.name} more than once.",
                    this
                );
            }

            if (!string.IsNullOrWhiteSpace(
                    definition.EnemyId) &&
                !usedIds.Add(definition.EnemyId))
            {
                Debug.LogWarning(
                    $"More than one enemy uses the ID " +
                    $"'{definition.EnemyId}'. Enemy IDs " +
                    $"should be unique.",
                    this
                );
            }

            if (definition.EnemyPrefab == null)
            {
                Debug.LogWarning(
                    $"{definition.name} has no enemy prefab assigned.",
                    definition
                );
            }
        }
    }
}