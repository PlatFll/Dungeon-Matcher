using System.Collections.Generic;
using UnityEngine;

public sealed partial class WaveController
{
    private readonly HashSet<EnemyDefinition> previousEncounterLeaders = new HashSet<EnemyDefinition>();

    private HashSet<EnemyDefinition> GetRepeatExclusions()
    {
        var excluded = new HashSet<EnemyDefinition>(previousEncounterLeaders);
        foreach (var definition in seenMilestoneLeaders)
            if (definition != null && definition.Category == EnemyCategory.Boss)
                excluded.Add(definition);
        // A required escort is part of the encounter too. Defer its leader
        // rather than bypass repeat protection or break the required pairing.
        if (enemyDatabase != null)
            foreach (var definition in enemyDatabase.Enemies)
                if (definition != null && definition.RequiredBossEscort != null &&
                    previousEncounterLeaders.Contains(definition.RequiredBossEscort))
                    excluded.Add(definition);
        return excluded;
    }

    // Resolve the whole composition before spawning: timing and deaths during
    // entrance animations cannot alter escort constraints or random selection.
    private List<EnemyDefinition> BuildEncounter(int count)
    {
        var result = new List<EnemyDefinition>(new EnemyDefinition[count]);
        var selected = new HashSet<EnemyDefinition>();
        var repeatExclusions = GetRepeatExclusions();
        EnemyDefinition leader = null;
        for (int i = 0; i < count; i++)
        {
            EnemyCategory leaderCategory = CurrentPlan.Categories[i];
            if (leaderCategory != EnemyCategory.Miniboss && leaderCategory != EnemyCategory.Boss) continue;
            leader = selectedMilestoneLeader != null ? selectedMilestoneLeader : waveSpawnProfile.GetFixedEnemy(currentWave, i);
            if (leader != null && repeatExclusions.Contains(leader)) leader = null;
            if (leader == null)
                enemyDatabase.TryGetRandomWeightedEnemy(leaderCategory,
                    currentWave, out leader, repeatExclusions, EncounterRandom);
            if (leader != null && leader.EnemyPrefab != null &&
                leader.Category == leaderCategory &&
                leader.GetSpawnWeight(currentWave) > 0 && enemyDatabase.ContainsEnemy(leader))
            {
                result[i] = leader;
                selected.Add(leader);
            }
            else leader = null;
            break;
        }
        int specials = 0;
        if (leader != null && leader.RequiredBossEscort != null)
        {
            var escort = leader.RequiredBossEscort;
            if (count < 2 || !enemyDatabase.ContainsEnemy(escort) || escort.EnemyPrefab == null || escort.GetSpawnWeight(currentWave) <= 0)
            {
                Debug.LogError("Boss encounter requires an eligible escort and two configured slots.", this);
                return new List<EnemyDefinition>(new EnemyDefinition[count]);
            }
            // Narrative boss composition is exactly leader + required escort.
            result.Clear(); result.Add(leader); result.Add(escort);
            while (result.Count < count) result.Add(null);
            return result;
        }
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
