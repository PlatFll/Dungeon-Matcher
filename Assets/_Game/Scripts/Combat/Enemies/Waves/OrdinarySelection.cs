using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public sealed class EncounterHistoryEntry
{
    public string members, escorts, family, pressureFamily, phase, role;
}

[Serializable]
internal sealed class OrdinaryWeights
{
    public int[] phases = { 100,0,0, 75,25,0, 20,80,0, 0,65,35, 0,25,75 };
    public int[] roles = { 35,55,10, 25,55,20, 20,55,25 };
    public float sameEscorts = 0.35f, repeatedPressure = 0.4f;
    private static OrdinaryWeights loaded;
    public static OrdinaryWeights Current => loaded ??= JsonUtility.FromJson<OrdinaryWeights>(
        Resources.Load<TextAsset>("Balance/OrdinaryEncounters").text);
}

public static class OrdinarySelection
{
    private static string MemberKey(IEnumerable<EnemyDefinition> members) =>
        string.Join("+", members.Select(e => e.EnemyId).OrderBy(id => id, StringComparer.Ordinal));
    private static string Escorts(EnemyDefinition[] members) => members.Length == 3 ? MemberKey(members.Skip(1)) : "";
    private static string Family(ZoneTestEncounter r) => r.formationFamily ?? "Local patrol";
    private static string PressureKey(ZoneTestEncounter r) =>
        Family(r) + "/" + MemberKey(r.members.Where(e => e.Category != EnemyCategory.Normal));
    private static OrdinaryRole Role(ZoneTestEncounter r) => r.ordinaryRole == OrdinaryRole.Introduction ?
        OrdinaryRole.Standard : r.ordinaryRole;
    private static List<T> Prefer<T>(List<T> candidates, Predicate<T> condition)
    {
        var preferred = candidates.FindAll(condition);
        return preferred.Count > 0 ? preferred : candidates;
    }
    private static T Draw<T>(IList<T> choices, Func<T, float> weight, System.Random rng)
    {
        double roll = rng.NextDouble() * choices.Sum(weight);
        foreach (var c in choices) { roll -= weight(c); if (roll < 0) return c; }
        return choices[choices.Count - 1];
    }

    public static ZoneTestEncounter Choose(ZoneDefinition zone, int tier, CrystalResonanceSnapshot state,
        List<ZoneTestEncounter> choices, System.Random rng)
    {
        var history = state.formationHistory ?? new List<EncounterHistoryEntry>();
        var newest = history.AsEnumerable().Reverse().ToList();
        var weights = OrdinaryWeights.Current;
        bool firstVisit = state.taughtZones?.Contains(zone.zoneId) != true;
        bool teaching = firstVisit && state.encountersThisVisit < 2 &&
            zone.resonancePool.Any(e => e.Weight(tier) > 0 && e.enemy.Category == EnemyCategory.Special &&
                !state.seenMechanics.Contains(e.enemy.EnemyId));
        if (teaching)
            choices = Prefer(choices, r => r.members.Length <= 2 && Role(r) != OrdinaryRole.Pressure);
        bool relief = state.capstonePhase == ResonanceCapstonePhase.Defeated &&
            state.capstoneWave > 0 && state.lastBankedWave == state.capstoneWave;
        if (relief) choices = Prefer(choices, r => r.members.Length <= 2 && r.members.All(e => e.Category == EnemyCategory.Normal));
        // Member history also covers fallback patrols. Older saves only recorded recipe labels.
        var previousMembers = newest.FirstOrDefault()?.members;
        var previousIds = previousMembers != null
            ? previousMembers.Split('+')
            : Array.Find(zone.resonanceEncounters, r => r.label == state.recentRecipes?.LastOrDefault())
                ?.members.Select(e => e.EnemyId).ToArray();
        if (previousIds != null)
            choices = Prefer(choices, r => !r.members.Any(e =>
                e.Category != EnemyCategory.Normal && previousIds.Contains(e.EnemyId)));
        if (newest.Count >= 2 && newest.Take(2).All(h => h.role == "Pressure"))
            choices = Prefer(choices, r => Role(r) != OrdinaryRole.Pressure);
        choices = Prefer(choices, r => !newest.Take(2).Any(h => h.members == MemberKey(r.members)));

        var lessons = choices.Where(r => r.ordinaryRole == OrdinaryRole.Introduction &&
            r.members.Any(e => e.Category != EnemyCategory.Normal && !state.seenMechanics.Contains(e.EnemyId))).ToList();
        bool lesson = lessons.Count > 0 && !relief && ((teaching && state.encountersThisVisit == 1) || rng.Next(5) == 0);
        if (lesson) choices = lessons;
        var phases = Enumerable.Range(0, 3).Where(i => weights.phases[tier * 3 + i] > 0 &&
            choices.Any(r => (r.phaseMask & (1 << i)) != 0)).ToList();
        int phase = phases.Count > 0 ? Draw(phases, i => weights.phases[tier * 3 + i], rng) : -1;
        if (phase >= 0) choices = choices.FindAll(r => (r.phaseMask & (1 << phase)) != 0);
        var roles = choices.Select(Role).Distinct().ToList();
        var role = Draw(roles, r => weights.roles[Math.Max(phase, 0) * 3 + (int)r], rng);
        choices = choices.FindAll(r => Role(r) == role);
        var families = choices.GroupBy(Family).ToList();
        float PressureWeight(ZoneTestEncounter r) => role == OrdinaryRole.Pressure &&
            newest.Take(4).Count(h => h.pressureFamily == PressureKey(r)) >= 2 ? weights.repeatedPressure : 1f;
        // Adjust the family's baseline by its mean recurrence factor, not by variant count.
        var family = Draw(families, f => f.First().familyWeight *
            f.Sum(r => WaveController.ResonanceRecipeWeight(zone, tier, r) * PressureWeight(r)) /
            f.Sum(r => WaveController.ResonanceRecipeWeight(zone, tier, r)), rng);
        float VariantWeight(ZoneTestEncounter r)
        {
            string escorts = Escorts(r.members);
            float penalty = escorts != "" && newest.Take(3).Any(h => h.escorts == escorts) ? weights.sameEscorts : 1f;
            return penalty * PressureWeight(r) * WaveController.ResonanceRecipeWeight(zone, tier, r);
        }
        var selected = Draw(family.ToList(), VariantWeight, rng);
        // Selection returns a per-draw envelope. Shared ScriptableObject recipes stay immutable.
        return selected.WithSelection(phase >= 0 ? ((EncounterPhase)phase).ToString() : "Fallback",
            lesson ? "Introduction" : role.ToString());
    }

    public static void Remember(CrystalResonanceSnapshot state, ZoneTestEncounter recipe, IEnumerable<EnemyDefinition> actors)
    {
        var history = state.formationHistory ??= new List<EncounterHistoryEntry>();
        var members = actors.ToArray();
        history.Add(new EncounterHistoryEntry { members = MemberKey(members), escorts = Escorts(members),
            family = Family(recipe), pressureFamily = PressureKey(recipe), phase = recipe.selectedPhase ?? "Unbanded",
            role = recipe.resonanceKind == ZoneEncounterKind.Ordinary ? recipe.selectedRole ?? Role(recipe).ToString() : "Capstone" });
        if (history.Count > 4) history.RemoveAt(0);
        state.taughtZones ??= new List<string>();
        if (state.encountersThisVisit >= 1 && !state.taughtZones.Contains(state.zoneId)) state.taughtZones.Add(state.zoneId);
    }
}
