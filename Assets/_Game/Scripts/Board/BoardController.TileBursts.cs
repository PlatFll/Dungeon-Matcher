using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BoardController
{
    public event Action<TileBurstVFXContext> TileBurstVFXRequested;
    public event Action<int> GemsLanded;
    public event Action<int> PrimaryExplosionPresented;

    // Split only presentation. The caller has already planned and reported one
    // deduplicated clear; special effects retain their original sorted commit.
    private IEnumerator PresentOrderedShatter(List<ClearVisual> visuals,
        IReadOnlyList<Gem> primaryOrder, TileBurstVFXContext[] bursts)
    {
        var primary = new List<Gem>();
        foreach (Gem gem in primaryOrder)
            if (gem != null && !primary.Contains(gem)) primary.Add(gem);
        if (primary.Count == 0)
        {
            foreach (ClearVisual visual in visuals) HideShatteredGem(visual.Gem);
            ReportTileBursts(bursts);
            yield break;
        }
        var groups = new List<List<Gem>>();
        for (int i = 0; i < primary.Count; i++) groups.Add(new List<Gem>());
        foreach (ClearVisual visual in visuals)
        {
            Gem gem = visual.Gem;
            if (gem == null) continue;
            int index = primary.IndexOf(gem);
            if (index < 0)
            {
                index = primary.Count - 1;
                for (int i = 0; i < primary.Count; i++)
                    if (Mathf.Abs(gem.Column - primary[i].Column) <= 1 &&
                        Mathf.Abs(gem.Row - primary[i].Row) <= 1)
                    { index = i; break; }
            }
            // A later primary keeps its own beat even when footprints overlap.
            groups[index].Add(gem);
        }
        for (int i = 0; i < groups.Count; i++)
        {
            var positions = new HashSet<Vector3>();
            foreach (Gem gem in groups[i])
            {
                positions.Add(transform.TransformPoint(GetLocalPosition(gem.Column, gem.Row)));
                HideShatteredGem(gem);
            }
            if (bursts != null)
                foreach (TileBurstVFXContext burst in bursts)
                {
                    var selected = new List<Vector3>();
                    foreach (Vector3 position in burst.WorldPositions)
                        if (positions.Contains(position)) selected.Add(position);
                    if (selected.Count > 0)
                        ReportTileBursts(new[] { new TileBurstVFXContext(burst.Kind, selected.ToArray()) });
                }
            PrimaryExplosionPresented?.Invoke(i);
            if (i + 1 < groups.Count) yield return new WaitForSeconds(0.17f);
        }
    }

    private static void HideShatteredGem(Gem gem)
    {
        if (gem == null) return;
        foreach (SpriteRenderer renderer in gem.GetComponentsInChildren<SpriteRenderer>())
            renderer.enabled = false;
    }

    // Build from the authoritative clear set before gems leave the grid. The
    // preserved reward cell is deliberately excluded, including a bomb whose
    // old utility effect still activates while its replacement survives.
    private TileBurstVFXContext[] BuildTileBurstContexts(HashSet<Gem> matches,
        Dictionary<Gem, GemSpecialType> preserved, HashSet<Gem> genericBurstTargets)
    {
        var cells = new Dictionary<Vector2Int, TileBurstKind>();
        var bombs = new List<Gem>();
        foreach (Gem gem in matches)
            if (gem != null && IsChainReactiveBomb(gem.SpecialType)) bombs.Add(gem);
        bombs.Sort(CompareGemsByGridPosition);

        foreach (Gem target in matches)
        {
            if (target == null || preserved.ContainsKey(target) || GetGem(target.Column, target.Row) != target) continue;
            bool hasKind = genericBurstTargets != null && genericBurstTargets.Contains(target);
            TileBurstKind kind = TileBurstKind.Explosion;
            int priority = hasKind ? 0 : -1;
            foreach (Gem bomb in bombs)
            {
                if (!BombCoversCell(bomb, target.Column, target.Row)) continue;
                TileBurstKind candidate = BurstKindFor(bomb.SpecialType);
                // A special's own cell keeps its material. Overlapping collateral
                // uses one deterministic readable burst, never stacked explosions.
                int candidatePriority = bomb == target ? 100 : BurstPriority(candidate);
                if (candidatePriority <= priority) continue;
                priority = candidatePriority;
                kind = candidate;
                hasKind = true;
            }
            if (hasKind) cells[new Vector2Int(target.Column, target.Row)] = kind;
        }

        var result = new List<TileBurstVFXContext>();
        foreach (TileBurstKind kind in Enum.GetValues(typeof(TileBurstKind)))
        {
            var positions = new List<Vector3>();
            // Board order gives stable event and effect order across HashSets.
            for (int row = 0; row < height; row++)
                for (int column = 0; column < width; column++)
                    if (cells.TryGetValue(new Vector2Int(column, row), out TileBurstKind cellKind) && cellKind == kind)
                        positions.Add(transform.TransformPoint(GetLocalPosition(column, row)));
            if (positions.Count > 0) result.Add(new TileBurstVFXContext(kind, positions.ToArray()));
        }
        return result.ToArray();
    }

    private static bool BombCoversCell(Gem bomb, int column, int row)
    {
        if (bomb.SpecialType == GemSpecialType.RowBomb) return row == bomb.Row;
        if (bomb.SpecialType == GemSpecialType.ColumnBomb) return column == bomb.Column;
        return Mathf.Abs(column - bomb.Column) <= 1 && Mathf.Abs(row - bomb.Row) <= 1;
    }

    private static int BurstPriority(TileBurstKind kind) => kind == TileBurstKind.Shield ? 3
        : kind == TileBurstKind.Healing ? 2 : kind == TileBurstKind.Poison ? 1 : 0;

    private static TileBurstKind BurstKindFor(GemSpecialType special) => special == GemSpecialType.PoisonBomb
        ? TileBurstKind.Poison : special == GemSpecialType.ShieldBomb ? TileBurstKind.Shield
        : special == GemSpecialType.HealingBomb ? TileBurstKind.Healing : TileBurstKind.Explosion;

    private TileBurstVFXController EnsureTileBurstVFXController()
    {
        var controller = GetComponent<TileBurstVFXController>();
        return controller != null ? controller : gameObject.AddComponent<TileBurstVFXController>();
    }

    private void ReportTileBursts(TileBurstVFXContext[] contexts)
    {
        if (contexts == null || contexts.Length == 0) return;
        EnsureTileBurstVFXController();
        foreach (var context in contexts) TileBurstVFXRequested?.Invoke(context);
    }
}
