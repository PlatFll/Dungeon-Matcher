using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed partial class WaveController
{
    // This is selection inside the existing spawn owner; all actors still use
    // SpawnCurrentWaveRoutine, normal slots, stat curves and summon ownership.
    public ZoneTestEncounter SelectResonanceEncounter(ZoneDefinition zone, int depth, System.Random random)
    {
        var state = RunSession.Current.Resonance.State;
        int tier = zone.ResonanceTier(depth);
        var choices = new List<ZoneTestEncounter>();
        foreach (var recipe in zone.resonanceEncounters)
            if (recipe.resonanceKind == ZoneEncounterKind.Ordinary &&
                ResonanceFormationLegal(zone, recipe, tier, state, false) && IntroductionSafe(recipe, state)) choices.Add(recipe);
        var fresh = choices.FindAll(r => !state.recentRecipes.Contains(r.label));
        if (fresh.Count > 0) choices = fresh;
        if (choices.Count > 0) return PickDepthRecipe(choices, random);
        return BuildDepthWeightedEncounter(zone, tier, state, random);
    }
    public static bool ResonanceFormationLegal(ZoneDefinition zone, ZoneTestEncounter recipe, int tier,
        CrystalResonanceSnapshot state, bool capstone)
    {
        if (recipe == null || tier < recipe.minimumDepthTier || tier > recipe.maximumDepthTier ||
            recipe.members == null || recipe.members.Length == 0 || recipe.members.Length > 3 || recipe.weight <= 0) return false;
        float threat = 0; int disruptors = 0, supports = 0;
        foreach (var member in recipe.members)
        {
            if (member == null || member.EnemyPrefab == null || Array.IndexOf(zone.enemies, member) < 0) return false;
            var entry = Array.Find(zone.resonancePool, p => p.enemy == member);
            if (entry == null || entry.Weight(tier) <= 0) return false;
            bool named = member.Category == EnemyCategory.Miniboss || member.Category == EnemyCategory.Boss;
            bool requiredException = recipe.allowRepeatedRequiredEscort && Array.Exists(recipe.members, p => p.RequiredBossEscort == member);
            if (named && state.seenLeaders.Contains(member.EnemyId) && !requiredException) return false;
            if (member.RequiredBossEscort != null && Array.IndexOf(recipe.members, member.RequiredBossEscort) < 0) return false;
            threat += member.ThreatCost; if (member.IsBoardDisruptor) disruptors++; if (member.IsSupport) supports++;
        }
        if (recipe.prerequisites != null && Array.Exists(recipe.prerequisites, id => !state.seenMechanics.Contains(id))) return false;
        float budget = zone.resonanceThreatBudgets[Mathf.Min(tier, zone.resonanceThreatBudgets.Length - 1)];
        // An authored capstone budget includes every escort; it is not a stat multiplier.
        if (capstone) budget = Mathf.Max(budget, recipe.resonanceThreatBudget);
        if (threat > Mathf.Min(budget, recipe.resonanceThreatBudget) || disruptors > 2 || supports > 1) return false;
        var board = RunSession.Current?.Board;
        if (zone.periodicallyFloods && board != null)
        {
            bool wet = board.IsFlooded, pending = board.Aquatic?.phase == TidePhase.Pending;
            if ((recipe.requiredTide == 1 && wet) || (recipe.requiredTide == 2 && !wet) ||
                ((wet || pending) && !board.FormationCanFlood(recipe.members))) return false;
            if (!capstone && pending && (board.Aquatic?.floodCount ?? 0) == 0 && !BoardController.FirstFloodLessonSafe(recipe.members)) return false;
        }
        return true;
    }
    private static bool IntroductionSafe(ZoneTestEncounter recipe, CrystalResonanceSnapshot state)
    {
        int unseen = 0;
        foreach (var member in recipe.members)
            if (member.Category != EnemyCategory.Normal && !state.seenMechanics.Contains(member.EnemyId)) unseen++;
        return unseen == 0 || (unseen == 1 && recipe.members.Length <= 2);
    }
    private static ZoneTestEncounter PickDepthRecipe(List<ZoneTestEncounter> candidates, System.Random random)
    {
        int total = 0; foreach (var recipe in candidates) total += Math.Max(1, recipe.weight);
        int pick = random.Next(total);
        foreach (var recipe in candidates) { pick -= Math.Max(1, recipe.weight); if (pick < 0) return recipe; }
        return candidates[candidates.Count - 1];
    }
    private ZoneTestEncounter BuildDepthWeightedEncounter(ZoneDefinition zone, int tier,
        CrystalResonanceSnapshot state, System.Random random)
    {
        var candidates = new List<ZoneDepthMember>();
        foreach (var entry in zone.resonancePool)
            if (entry.enemy != null && entry.enemy.Category <= EnemyCategory.Special && entry.Weight(tier) > 0) candidates.Add(entry);
        var members = new List<EnemyDefinition>(); int family = -1;
        for (int attempt = 0; attempt < 24 && members.Count < (tier == 0 ? 2 : 3); attempt++)
        {
            float total = 0; foreach (var entry in candidates) total += entry.Weight(tier);
            if (total <= 0) break;
            float roll = (float)random.NextDouble() * total; var chosen = candidates[candidates.Count - 1];
            foreach (var entry in candidates) { roll -= entry.Weight(tier); if (roll < 0) { chosen = entry; break; } }
            candidates.Remove(chosen);
            if (family >= 0 && Math.Abs(chosen.family - family) > 1) continue;
            var trial = new ZoneTestEncounter { label = "Depth patrol", members = members.Concat(new[] { chosen.enemy }).ToArray() };
            if (!ResonanceFormationLegal(zone, trial, tier, state, false) || !IntroductionSafe(trial, state)) continue;
            members.Add(chosen.enemy); if (family < 0) family = chosen.family;
        }
        if (members.Count == 0)
        {
            // Same-zone legal relief only. Never fall through to Dungeon enemies.
            foreach (var entry in zone.resonancePool.OrderBy(e => e.enemy == null ? float.MaxValue : e.enemy.ThreatCost))
            {
                if (entry.enemy == null || entry.enemy.Category != EnemyCategory.Normal || entry.Weight(tier) <= 0) continue;
                var relief = new ZoneTestEncounter { label = "Depth relief", members = new[] { entry.enemy } };
                if (ResonanceFormationLegal(zone, relief, tier, state, false)) return relief;
            }
            throw new InvalidOperationException("No legal Resonance formation for " + zone.zoneId + " tier " + tier);
        }
        return new ZoneTestEncounter { label = "Depth patrol", members = members.ToArray() };
    }
    private void RecordResonanceFormation(ZoneTestEncounter recipe)
    {
        var resonance = RunSession.Current?.Resonance;
        if (resonance?.Enabled != true || recipe == null) return;
        var state = resonance.State;
        state.recentRecipes.Add(recipe.label); while (state.recentRecipes.Count > 3) state.recentRecipes.RemoveAt(0);
        foreach (var member in originalEncounterDefinitions)
        {
            if (!state.seenMechanics.Contains(member.EnemyId)) state.seenMechanics.Add(member.EnemyId);
            if ((member.Category == EnemyCategory.Miniboss || member.Category == EnemyCategory.Boss) &&
                !state.seenLeaders.Contains(member.EnemyId)) state.seenLeaders.Add(member.EnemyId);
        }
    }
}
