using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public sealed class PlayerStatusSnapshot
{
    public int currentMove, completedMove;
    public List<PlayerStatusEntry> effects = new List<PlayerStatusEntry>();
}

[Serializable]
public sealed class PlayerStatusEntry
{
    public PlayerStatusKind kind;
    public int remainingMoves, appliedMove, damagePerMove;
    public float multiplier;
    public long sourceId;
    public PlayerStatusEntry Copy() => (PlayerStatusEntry)MemberwiseClone();
}

/// <summary>Player-owned move durations; resource changes still go through their existing owners.</summary>
public sealed class PlayerStatusRuntime : IDisposable
{
    private readonly PlayerActor player;
    private readonly List<PlayerStatusEntry> effects = new List<PlayerStatusEntry>();
    private readonly Dictionary<long, EnemyActor> fearSources = new Dictionary<long, EnemyActor>();
    private int currentMove, completedMove;
    public event Action Changed;
    public IReadOnlyList<PlayerStatusEntry> Effects => effects;
    public PlayerStatusRuntime(PlayerActor owner) { player = owner; }

    public bool Has(PlayerStatusKind kind) => effects.Any(e => e.kind == kind);
    public int Remaining(PlayerStatusKind kind) => effects.Where(e => e.kind == kind).Select(e => e.remainingMoves).DefaultIfEmpty(0).Max();
    public float HealingMultiplier => Minimum(PlayerStatusKind.Wounded);
    public float IncomingMultiplier => effects.Where(e => e.kind == PlayerStatusKind.Frostbite).Select(e => e.multiplier).DefaultIfEmpty(1f).Max();
    public int GeneratedEnergy(int amount) => Mathf.FloorToInt(Mathf.Max(0, amount) * Minimum(PlayerStatusKind.Sapped));
    private float Minimum(PlayerStatusKind kind) => effects.Where(e => e.kind == kind).Select(e => e.multiplier).DefaultIfEmpty(1f).Min();

    public float OutgoingMultiplier(EnemyActor target)
    {
        PruneFear();
        float fear = effects.Where(e => e.kind == PlayerStatusKind.Fear && target != null && e.sourceId == target.PersistentId)
            .Select(e => e.multiplier).DefaultIfEmpty(1f).Min();
        return Minimum(PlayerStatusKind.Weakened) * fear;
    }

    public bool Apply(PlayerStatusDefinition definition, EnemyActor source = null)
    {
        if (definition == null || !player.IsInitialized || player.IsDefeated ||
            !Enum.IsDefined(typeof(PlayerStatusKind), definition.kind)) return false;
        bool fear = definition.kind == PlayerStatusKind.Fear;
        if (fear && (source == null || source.IsDefeated || source.PersistentId <= 0 ||
            source.GetComponent<EnemyStagger>()?.IsStaggered == true)) return false;
        long sourceId = fear ? source.PersistentId : 0;
        var entry = effects.FirstOrDefault(e => e.kind == definition.kind && e.sourceId == sourceId);
        float value = float.IsNaN(definition.multiplier) || float.IsInfinity(definition.multiplier) ? 1f : definition.multiplier;
        value = definition.kind == PlayerStatusKind.Frostbite ? Mathf.Max(1f, value) : Mathf.Clamp01(value);
        if (entry == null)
        {
            entry = new PlayerStatusEntry { kind = definition.kind, sourceId = sourceId, multiplier = value };
            effects.Add(entry);
        }
        // Refresh one channel, never multiply repeated applications together.
        entry.multiplier = definition.kind == PlayerStatusKind.Frostbite ? Mathf.Max(entry.multiplier, value) : Mathf.Min(entry.multiplier, value);
        entry.damagePerMove = Mathf.Max(entry.damagePerMove, CombatAmounts.Round(definition.damagePerMove));
        entry.remainingMoves = Mathf.Max(entry.remainingMoves, Mathf.Max(1, definition.durationMoves));
        entry.appliedMove = currentMove;
        if (fear) BindFear(source);
        Changed?.Invoke();
        return true;
    }

    public bool Cleanse(PlayerStatusKind kind)
    {
        bool removed = effects.RemoveAll(e => e.kind == kind) > 0;
        if (removed) { ReleaseUnusedSources(); Changed?.Invoke(); }
        return removed;
    }
    public bool ExtinguishBurn() => Cleanse(PlayerStatusKind.Burn);
    public void BeginMove(int move) { currentMove = Mathf.Max(currentMove, move); }
    public void CompleteMove(int move)
    {
        if (move <= completedMove) return;
        currentMove = Mathf.Max(currentMove, move);
        completedMove = move; // Claim the tick before callbacks can re-enter.
        PruneFear();
        var ticking = effects.Where(e => e.appliedMove < move).ToArray();
        int burn = ticking.Where(e => e.kind == PlayerStatusKind.Burn).Select(e => e.damagePerMove).DefaultIfEmpty(0).Max();
        if (burn > 0) player.TryTakeDamage(burn, BurnDamageSource.Instance);
        foreach (var entry in ticking)
            if (effects.Contains(entry) && entry.appliedMove < move && --entry.remainingMoves <= 0) effects.Remove(entry);
        ReleaseUnusedSources();
        Changed?.Invoke();
    }

    public PlayerStatusSnapshot Capture() => new PlayerStatusSnapshot
    {
        currentMove = currentMove, completedMove = completedMove, effects = effects.Select(e => e.Copy()).ToList()
    };
    public void Restore(PlayerStatusSnapshot saved, Func<long, EnemyActor> findSource)
    {
        Clear();
        if (saved == null) return;
        currentMove = Mathf.Max(0, saved.currentMove); completedMove = Mathf.Clamp(saved.completedMove, 0, currentMove);
        foreach (var original in saved.effects ?? new List<PlayerStatusEntry>())
        {
            if (original == null || original.remainingMoves <= 0 || !Enum.IsDefined(typeof(PlayerStatusKind), original.kind) ||
                float.IsNaN(original.multiplier) || float.IsInfinity(original.multiplier)) continue;
            var entry = original.Copy();
            entry.multiplier = entry.kind == PlayerStatusKind.Frostbite ? Mathf.Max(1f, entry.multiplier) : Mathf.Clamp01(entry.multiplier);
            entry.damagePerMove = CombatAmounts.Round(entry.damagePerMove);
            entry.appliedMove = Mathf.Clamp(entry.appliedMove, 0, currentMove);
            if (entry.kind == PlayerStatusKind.Fear)
            {
                var source = findSource?.Invoke(entry.sourceId);
                if (source == null || source.IsDefeated || source.GetComponent<EnemyStagger>()?.IsStaggered == true) continue;
                BindFear(source);
            }
            else entry.sourceId = 0;
            if (!effects.Any(e => e.kind == entry.kind && e.sourceId == entry.sourceId)) effects.Add(entry);
        }
        Changed?.Invoke();
    }
    private void BindFear(EnemyActor source)
    {
        if (fearSources.ContainsKey(source.PersistentId)) return;
        fearSources.Add(source.PersistentId, source);
        source.Defeated += SourceDefeated;
        var stagger = source.GetComponent<EnemyStagger>();
        if (stagger != null) stagger.StaggerApplied += SourceStaggered;
    }
    private void SourceDefeated(EnemyActor source) => RemoveFear(source.PersistentId);
    private void SourceStaggered(EnemyStagger source, float duration, float remaining) => RemoveFear(source.EnemyActor.PersistentId);
    private void RemoveFear(long id)
    {
        if (effects.RemoveAll(e => e.kind == PlayerStatusKind.Fear && e.sourceId == id) == 0) return;
        ReleaseUnusedSources(); Changed?.Invoke();
    }
    public void PruneFear()
    {
        foreach (var pair in fearSources.ToArray())
            if (pair.Value == null || pair.Value.IsDefeated || pair.Value.GetComponent<EnemyStagger>()?.IsStaggered == true) RemoveFear(pair.Key);
    }
    private void ReleaseUnusedSources()
    {
        foreach (var pair in fearSources.ToArray())
        {
            if (effects.Any(e => e.kind == PlayerStatusKind.Fear && e.sourceId == pair.Key)) continue;
            if (pair.Value != null)
            {
                pair.Value.Defeated -= SourceDefeated;
                var stagger = pair.Value.GetComponent<EnemyStagger>();
                if (stagger != null) stagger.StaggerApplied -= SourceStaggered;
            }
            fearSources.Remove(pair.Key);
        }
    }
    public void Clear()
    {
        effects.Clear(); ReleaseUnusedSources(); currentMove = completedMove = 0; Changed?.Invoke();
    }
    public void Dispose() { Clear(); Changed = null; }
}

public sealed class BurnDamageSource : ScriptableObject
{
    private static BurnDamageSource instance;
    public static BurnDamageSource Instance => instance != null ? instance : instance = CreateInstance<BurnDamageSource>();
}
