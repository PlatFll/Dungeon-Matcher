using System.Collections.Generic;
using UnityEngine;

public sealed partial class WaveController
{
    // Resolve the whole composition before spawning: timing and deaths during
    // entrance animations cannot alter escort constraints or random selection.
    private List<EnemyDefinition> BuildEncounter(int count)
    {
        var result = new List<EnemyDefinition>(new EnemyDefinition[count]);
        var selected = new HashSet<EnemyDefinition>();
        EnemyDefinition leader = null;
        for (int i = 0; i < count; i++)
        {
            if (CurrentPlan.Categories[i] != EnemyCategory.Miniboss) continue;
            leader = waveSpawnProfile.GetFixedEnemy(currentWave, i);
            if (leader == null)
                enemyDatabase.TryGetRandomWeightedEnemy(EnemyCategory.Miniboss,
                    currentWave, out leader, deterministicRandom: EncounterRandom);
            if (leader != null && leader.EnemyPrefab != null &&
                leader.Category == EnemyCategory.Miniboss &&
                leader.GetSpawnWeight(currentWave) > 0 && enemyDatabase.ContainsEnemy(leader))
            {
                result[i] = leader;
                selected.Add(leader);
            }
            else leader = null;
            break;
        }
        int specials = 0;
        for (int i = 0; i < count; i++)
        {
            if (result[i] != null) continue;
            EnemyDefinition definition;
            if (leader != null && leader.EncounterEscorts.Length > 0)
            {
                var candidates = new List<EnemyDefinition>();
                float total = 0;
                foreach (var candidate in leader.EncounterEscorts)
                {
                    if (candidate == null || candidate.EnemyPrefab == null ||
                        !enemyDatabase.ContainsEnemy(candidate) || candidate.GetSpawnWeight(currentWave) <= 0 ||
                        (candidate.Category != EnemyCategory.Normal && candidate.Category != EnemyCategory.Special) ||
                        (candidate.Category == EnemyCategory.Special && specials >= leader.MaximumSpecialEscorts)) continue;
                    candidates.Add(candidate);
                    total += candidate.GetSpawnWeight(currentWave);
                }
                definition = null;
                float roll = (float)EncounterRandom.NextDouble() * total;
                foreach (var candidate in candidates)
                {
                    definition = candidate;
                    roll -= candidate.GetSpawnWeight(currentWave);
                    if (roll < 0) break;
                }
            }
            else
            {
                // Fallback only to ordinary troops, never an extra Mini-boss.
                if (!TrySelectFromCategory(CurrentPlan.Categories[i], selected, out definition))
                    TrySelectFromCategory(EnemyCategory.Normal, selected, out definition);
            }
            result[i] = definition;
            if (definition == null) continue;
            selected.Add(definition);
            if (definition.Category == EnemyCategory.Special) specials++;
        }
        return result;
    }
}
