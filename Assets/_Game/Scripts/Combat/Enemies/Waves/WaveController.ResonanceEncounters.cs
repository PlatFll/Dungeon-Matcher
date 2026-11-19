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
        if (state.capstonePhase == ResonanceCapstonePhase.Scheduled)
        {
            if (depth != state.capstoneWave) throw new InvalidOperationException("Scheduled capstone cannot be skipped.");
            var locked = new List<EnemyDefinition>();
            foreach (string id in state.capstoneMembers)
            {
                var member = Array.Find(zone.enemies, enemy => enemy != null && enemy.EnemyId == id);
                if (member == null) throw new InvalidOperationException("Saved capstone member is unavailable: " + id);
                locked.Add(member);
            }
            state.capstonePhase = ResonanceCapstonePhase.InProgress;
            return new ZoneTestEncounter { label = state.capstoneId, members = locked.ToArray(),
                resonanceKind = (ZoneEncounterKind)state.capstoneKind };
        }
        return SelectResonanceOrdinary(zone, depth, state, random);
    }
    // Shared production selector exposed for deterministic selection-only validation.
    public static ZoneTestEncounter SelectResonanceOrdinary(ZoneDefinition zone, int depth,
        CrystalResonanceSnapshot state, System.Random random)
    {
        int tier = zone.ResonanceTier(depth);
        var choices = new List<ZoneTestEncounter>();
        foreach (var recipe in zone.resonanceEncounters)
            if (recipe.resonanceKind == ZoneEncounterKind.Ordinary &&
                ResonanceFormationLegal(zone, recipe, tier, state, false) && IntroductionSafe(recipe, state)) choices.Add(recipe);
        if (choices.Count > 0)
            return OrdinarySelection.Choose(zone, tier, state, choices, random);
        return BuildDepthWeightedEncounter(zone, tier, state, random);
    }
    public static ZoneTestEncounter ChooseResonanceCapstone(ZoneDefinition zone, int depth,
        CrystalResonanceSnapshot state, System.Random random)
    {
        int tier = zone.ResonanceTier(depth);
        var candidates = new List<ZoneTestEncounter>();
        foreach (var recipe in zone.resonanceEncounters)
        {
            if (recipe.resonanceKind == ZoneEncounterKind.Ordinary ||
                !ResonanceFormationLegal(zone, recipe, tier, state, true)) continue;
            if (recipe.resonanceKind == ZoneEncounterKind.MajorBoss && state.visit - state.lastMajorVisit < 2) continue;
            int unseen = recipe.members.Count(e => e.Category != EnemyCategory.Normal && !state.seenMechanics.Contains(e.EnemyId));
            if (unseen > 1 && !recipe.allowRepeatedRequiredEscort) continue;
            candidates.Add(recipe);
        }
        var fresh = candidates.FindAll(r => !state.recentCapstones.Contains(zone.zoneId + ":" + r.label));
        if (fresh.Count > 0) candidates = fresh;
        if (candidates.Count == 0)
            throw new InvalidOperationException("Missing legal same-zone reserve capstone: " + zone.zoneId + "/" + tier);
        // Pick kind first, so adding authored escort variants doesn't inflate major-boss frequency.
        int[] weights = { 0, 6, 4, tier >= 4 ? 3 : 2 };
        int total = 0;
        for (int kind = 1; kind <= 3; kind++) if (candidates.Exists(r => (int)r.resonanceKind == kind)) total += weights[kind];
        int roll = random.Next(total), selected = 1;
        for (int kind = 1; kind <= 3; kind++)
        {
            if (!candidates.Exists(r => (int)r.resonanceKind == kind)) continue;
            roll -= weights[kind]; if (roll < 0) { selected = kind; break; }
        }
        return PickDepthRecipe(zone, tier, candidates.FindAll(r => (int)r.resonanceKind == selected), random);
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
        if (!capstone && zone.periodicallyFloods && board?.IsFlooded == true && disruptors > 1) return false;
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
    public static float ResonanceRecipeWeight(ZoneDefinition zone, int tier, ZoneTestEncounter recipe)
    {
        // Pool fade must affect authored formations too, not only the rare fallback.
        // Average members rather than summing so actor count cannot inflate frequency.
        float total = 0;
        foreach (var member in recipe.members)
            total += Array.Find(zone.resonancePool, entry => entry.enemy == member)?.Weight(tier) ?? 0;
        return recipe.members.Length == 0 ? 0 : recipe.weight * total / recipe.members.Length;
    }
    private static ZoneTestEncounter PickDepthRecipe(ZoneDefinition zone, int tier,
        List<ZoneTestEncounter> candidates, System.Random random)
    {
        float total = 0; foreach (var recipe in candidates) total += ResonanceRecipeWeight(zone, tier, recipe);
        float pick = (float)random.NextDouble() * total;
        foreach (var recipe in candidates) { pick -= ResonanceRecipeWeight(zone, tier, recipe); if (pick < 0) return recipe; }
        return candidates[candidates.Count - 1];
    }
    private static ZoneTestEncounter BuildDepthWeightedEncounter(ZoneDefinition zone, int tier,
        CrystalResonanceSnapshot state, System.Random random)
    {
        var candidates = new List<ZoneDepthMember>();
        foreach (var entry in zone.resonancePool)
            if (entry.enemy != null && (int)entry.enemy.Category <= (int)EnemyCategory.Special && entry.Weight(tier) > 0) candidates.Add(entry);
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
        resonance.Record("formation", recipe.label + ":" + string.Join("+", originalEncounterDefinitions.Select(e => e.EnemyId + "/" + e.Category + "/" + e.ThreatCost)));
        var state = resonance.State;
        OrdinarySelection.Remember(state, recipe, originalEncounterDefinitions);
        var selection = state.formationHistory.Last();
        resonance.Record("selection", $"{selection.phase}/{selection.role}/{selection.family}: {selection.members}");
        state.recentRecipes.Add(recipe.label); while (state.recentRecipes.Count > 3) state.recentRecipes.RemoveAt(0);
        foreach (var member in originalEncounterDefinitions)
        {
            if (!state.seenMechanics.Contains(member.EnemyId)) state.seenMechanics.Add(member.EnemyId);
            if ((member.Category == EnemyCategory.Miniboss || member.Category == EnemyCategory.Boss) &&
                !state.seenLeaders.Contains(member.EnemyId)) state.seenLeaders.Add(member.EnemyId);
        }
    }
}
