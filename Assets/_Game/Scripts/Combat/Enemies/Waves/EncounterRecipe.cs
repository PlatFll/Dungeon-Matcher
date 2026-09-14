using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class EncounterMember
{
    public EnemyDefinition enemy;
    [Range(1, 3)] public int count = 1;
}

[Serializable]
public sealed class EncounterRecipe
{
    public string id;
    public string purpose;
    [Min(1)] public int minimumWave = 1;
    [Min(1)] public int maximumWave = 30;
    [Min(0)] public float weight = 1;
    [Min(1)] public float threatBudget = 6;
    [Range(0, 3)] public int maximumDisruptors = 2;
    public List<EncounterMember> members = new List<EncounterMember>();

    public bool TryBuild(int wave, int slots, EnemyDatabase database, ISet<EnemyDefinition> excluded, out List<EnemyDefinition> result)
    {
        result = new List<EnemyDefinition>();
        if (wave < minimumWave || wave > maximumWave || weight <= 0) return false;
        float threat = 0;
        int disruptors = 0, supports = 0;
        foreach (var member in members)
        {
            var enemy = member.enemy;
            if (enemy == null || !database.ContainsEnemy(enemy) || enemy.EnemyPrefab == null ||
                enemy.GetSpawnWeight(wave) <= 0 || excluded.Contains(enemy) || member.count < 1) return false;
            for (int i = 0; i < member.count; i++)
            {
                result.Add(enemy); threat += enemy.ThreatCost;
                if (enemy.IsBoardDisruptor) disruptors++;
                if (enemy.IsSupport) supports++;
            }
        }
        return result.Count > 0 && result.Count <= slots && threat <= threatBudget &&
            disruptors <= maximumDisruptors && supports <= 1;
    }
}
