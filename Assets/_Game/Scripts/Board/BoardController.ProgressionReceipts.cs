using System;
using System.Collections.Generic;

/// <summary>Observation of board-owned work, never a request to mutate or award combat rewards.</summary>
public readonly struct BoardProgressionReceipt
{
    public readonly long Sequence;
    public readonly int Move, GemCount;
    public readonly bool Special;
    public readonly ResonanceMastery Mastery;
    public readonly UnityEngine.Vector3 Origin;
    public BoardProgressionReceipt(long sequence, int move, int gems, bool special, ResonanceMastery mastery, UnityEngine.Vector3 origin = default)
    { Origin = origin; Sequence = sequence; Move = move; GemCount = gems; Special = special; Mastery = mastery; }
}
public partial class BoardController
{
    public event Action<BoardProgressionReceipt> ProgressionReceipt;
    private long progressionReceiptSequence;
    private bool progressionManualAction;
    private UnityEngine.Vector3 progressionOrigin;
    private readonly HashSet<int> deliberateCounterGems = new HashSet<int>();
    private readonly HashSet<UnityEngine.Vector2Int> deliberateCounterCells = new HashSet<UnityEngine.Vector2Int>();
    private readonly HashSet<int> deliberateSpecials = new HashSet<int>();
    public void RestoreProgressionReceiptSequence(long sequence)
    { progressionReceiptSequence = Math.Max(progressionReceiptSequence, sequence); }
    private void BeginProgressionMove(Gem first, Gem second)
    {
        progressionManualAction = true; deliberateSpecials.Clear();
        deliberateCounterGems.Clear(); deliberateCounterCells.Clear();
        progressionOrigin = first != null ? first.transform.position : transform.position;
        if (first != null && first.SpecialType != GemSpecialType.None) deliberateSpecials.Add(first.BoardIdentity);
        if (second != null && second.SpecialType != GemSpecialType.None) deliberateSpecials.Add(second.BoardIdentity);
        if (deliberateSpecials.Count >= 2) EmitProgression(0, true, ResonanceMastery.SpecialSetup);
    }
    private void EmitProgression(int count, bool special, ResonanceMastery mastery = ResonanceMastery.None)
    {
        // Enemy/environment cascades and passive drill chains are not intentional matching.
        if (!progressionManualAction || ResolvingEnvironmentalMutation || resolvingUnrewardedEnvironment) return;
        ProgressionReceipt?.Invoke(new BoardProgressionReceipt(++progressionReceiptSequence,
            completedValidPlayerMoves + 1, count, special, mastery, progressionOrigin));
    }
    private void ReportProgressionMatch(BoardClearContext context, bool deliberate)
    {
        EmitProgression(context.GemCount, false, deliberate && context.TriggerGemCount >= 4
            ? ResonanceMastery.DeliberateMatch : ResonanceMastery.None);
    }
    private void MarkDeliberateClear(HashSet<Gem> matches)
    {
        if (!progressionManualAction || matches == null || ResolvingEnvironmentalMutation || resolvingUnrewardedEnvironment) return;
        foreach (var gem in matches)
        {
            if (gem == null) continue;
            deliberateCounterGems.Add(gem.BoardIdentity);
            deliberateCounterCells.Add(new UnityEngine.Vector2Int(gem.Column, gem.Row));
            if (gem.SpecialType != GemSpecialType.None) deliberateSpecials.Add(gem.BoardIdentity);
        }
    }
    private void ReportProgressionSpecial(HashSet<Gem> expanded, bool initial)
    {
        if (expanded == null) return;
        int specials = 0; bool deliberate = false;
        foreach (var gem in expanded)
        {
            if (gem == null || gem.SpecialType == GemSpecialType.None) continue;
            specials++; deliberate |= deliberateSpecials.Contains(gem.BoardIdentity);
        }
        if (initial && deliberate) MarkDeliberateClear(expanded);
        if (specials >= 2 && deliberate) EmitProgression(0, true, ResonanceMastery.SpecialSetup);
    }
    private void ReportProgressionCounter(Gem gem)
    { if (gem != null && deliberateCounterGems.Contains(gem.BoardIdentity)) EmitProgression(0, false, ResonanceMastery.TacticalCounter); }
    private void ReportProgressionCounter(UnityEngine.Vector2Int cell)
    { if (deliberateCounterCells.Contains(cell)) EmitProgression(0, false, ResonanceMastery.TacticalCounter); }
}
