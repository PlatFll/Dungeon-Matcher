using System.Collections.Generic;
using UnityEngine;

public sealed partial class WaveController
{
    private readonly HashSet<EnemyDefinition> previousEncounterLeaders = new HashSet<EnemyDefinition>();
    private string previousRecipeId;
    private List<EnemyDefinition> selectedRecipeMembers;

    private HashSet<EnemyDefinition> GetRepeatExclusions()
    {
        var excluded = new HashSet<EnemyDefinition>(previousEncounterLeaders);
        foreach (var definition in seenMilestoneLeaders)
            if (definition != null && (definition.Category == EnemyCategory.Boss || definition.Category == EnemyCategory.Miniboss))
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
        if (selectedMilestoneLeader == null)
        {
            var introduction = waveSpawnProfile.SelectIntroduction(currentWave, EncounterRandom, seenMilestoneLeaders,
                onlyDue: previousEncounterLeaders.Count > 0);
            if (introduction != null || previousEncounterLeaders.Count > 0)
                return BuildReadableEncounter(introduction, count);
        }
        if (selectedRecipeMembers != null) return IntroduceOneMechanic(new List<EnemyDefinition>(selectedRecipeMembers), count);
        List<EnemyDefinition> formation = null;
        for (int attempt = 0; attempt < 24; attempt++)
        {
            formation = BuildWeightedEncounter(count);
            if (IsWithinThreatBudget(formation)) return IntroduceOneMechanic(formation, count);
        }
        // Deterministic bounded fallback: retain narrative members and fill with
        // the cheapest eligible escorts that fit. Never return an over-budget roll.
        var fallback = new List<EnemyDefinition>();
        EnemyDefinition leader = formation?.Find(e => e != null &&
            (e.Category == EnemyCategory.Miniboss || e.Category == EnemyCategory.Boss));
        if (leader != null)
        {
            fallback.Add(leader);
            if (leader.RequiredBossEscort != null) fallback.Add(leader.RequiredBossEscort);
        }
        if (!IsWithinThreatBudget(fallback))
        {
            Debug.LogError("Required encounter exceeds its whole-formation budget; correct the spawn profile.", this);
            return new List<EnemyDefinition>();
        }
        var candidates = new List<EnemyDefinition>();
        foreach (var enemy in enemyDatabase.Enemies)
            if (enemy != null && enemy.EnemyPrefab != null && enemy.Category == EnemyCategory.Normal &&
                enemy.GetSpawnWeight(currentWave) > 0 &&
                (leader == null || leader.EncounterEscorts.Length == 0 || System.Array.IndexOf(leader.EncounterEscorts, enemy) >= 0))
                candidates.Add(enemy);
        candidates.Sort((a,b) => a.ThreatCost.CompareTo(b.ThreatCost));
        if (leader == null || leader.RequiredBossEscort == null)
            foreach (var enemy in candidates)
            {
                if (fallback.Count >= count) break;
                fallback.Add(enemy);
                if (!IsWithinThreatBudget(fallback)) fallback.RemoveAt(fallback.Count - 1);
            }
        return fallback;
    }

    private List<EnemyDefinition> IntroduceOneMechanic(List<EnemyDefinition> formation, int count)
    {
        if (selectedMilestoneLeader != null) return formation;
        foreach (var enemy in formation)
            if (waveSpawnProfile.NeedsIntroduction(enemy, seenMilestoneLeaders)) return BuildReadableEncounter(enemy, count);
        return formation;
    }

    private List<EnemyDefinition> BuildReadableEncounter(EnemyDefinition lesson, int slots)
    {
        var result = new List<EnemyDefinition>();
        if (lesson != null) result.Add(lesson);
        var excluded = new HashSet<EnemyDefinition>();
        int target = Mathf.Min(2, slots);
        for (int attempt = 0; attempt < 30 && result.Count < target; attempt++)
        {
            if (!enemyDatabase.TryGetRandomWeightedEnemy(EnemyCategory.Normal, currentWave, out var candidate, excluded, EncounterRandom)) break;
            excluded.Add(candidate);
            result.Add(candidate);
            if (!IsWithinThreatBudget(result)) result.RemoveAt(result.Count - 1);
        }
        CurrentPlan = new WaveSpawnPlan(currentWave, lesson != null ? "Introduction: " + lesson.DisplayName : "Relief patrol",
            result.ConvertAll(enemy => enemy.Category));
        return result;
    }

    private bool IsWithinThreatBudget(List<EnemyDefinition> formation)
    {
        float threat = 0; int disruptors = 0, supports = 0;
        foreach (var enemy in formation)
        {
            if (enemy == null) continue;
            threat += enemy.ThreatCost;
            if (enemy.IsBoardDisruptor) disruptors++;
            if (enemy.IsSupport) supports++;
        }
        return threat <= waveSpawnProfile.ThreatBudget(currentWave) && disruptors <= 2 && supports <= 1;
    }

    private List<EnemyDefinition> BuildWeightedEncounter(int count)
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
