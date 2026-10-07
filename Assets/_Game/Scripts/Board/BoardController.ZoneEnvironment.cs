using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BoardController
{
    private const int CrumblingTileOwner = -2147482999;
    private const float CrumbleShakeSeconds = .48f;
    private const float TileReturnSeconds = .18f;
    private int nextCrumbleMove = 4;
    private readonly Dictionary<Vector2Int, int> crumbleRestoreMoves = new Dictionary<Vector2Int, int>();
    public event Action<IReadOnlyList<Vector2Int>, float> CellsShaking;
    public event Action<int, int, float> CellMaterializing;

    public void QueueZoneEnvironment(int move)
    {
        var zone = RunSession.Current?.Zone?.Definition;
        if (zone == null || gems == null) return;
        if (zone.crumblesTiles)
        {
            bool due = move >= nextCrumbleMove;
            foreach (int deadline in crumbleRestoreMoves.Values) due |= move >= deadline;
            if (!due) return;
            EnqueueBoardMutation(new BoardMutationRequest {
                Kind = BoardMutationKind.AdvanceCrumblingTiles, EnvironmentMove = move });
            TryStartBoardMutationProcessor();
        }
    }

    public IEnumerator AdvanceZoneEnvironment(int move)
    {
        if (RunSession.Current?.Zone?.Definition?.periodicallyFloods == true)
            yield return AdvanceAquaticEnvironment(move);
        if (RunSession.Current?.Zone?.Definition?.growsVines == true)
            yield return AdvanceVineNetworks(move);
        QueueZoneEnvironment(move);
        while (IsBusy) yield return null;
    }

    private IEnumerator ExecuteCrumblingTiles(int move)
    {
        var zone = RunSession.Current?.Zone?.Definition;
        if (zone?.crumblesTiles != true || RunSession.Current.Player.IsDefeated) yield break;
        EnsureMiningVFX();
        var returning = new List<Vector2Int>();
        foreach (var pair in crumbleRestoreMoves) if (move >= pair.Value) returning.Add(pair.Key);
        if (returning.Count > 0)
        {
            foreach (var cell in returning) CellMaterializing?.Invoke(cell.x, cell.y, TileReturnSeconds);
            yield return new WaitForSeconds(TileReturnSeconds);
            foreach (var cell in returning)
            {
                crumbleRestoreMoves.Remove(cell);
                minedCellOwners.Remove(cell);
                NotifyRoyalBannerSpaceOpened(cell.x, cell.y);
                CellRestored?.Invoke(cell.x, cell.y);
            }
            yield return ResolveEnvironmentalBoardChange();
        }
        if (move < nextCrumbleMove || RunSession.Current.Player.IsDefeated) yield break;
        nextCrumbleMove = move + Mathf.Max(3, zone.crumbleCadenceMoves);
        int count = GameplayRandom.Range(1, 3);
        var breaking = new List<Vector2Int>();
        // Reserve each choice before checking the next. Existing mines, roots,
        // warnings, specials, chains and the useful-response budget all count.
        for (int i = 0; i < count; i++)
        {
            if (minedCellOwners.Count >= BalanceV1.Current.maximumGlobalMines ||
                minedCellOwners.Count + barricadeCells.Count >= BalanceV1.Current.maximumGlobalStructures) break;
            var candidates = BuildMineableCellList();
            candidates.RemoveAll(cell => IsProtectedWarningTarget(GetGem(cell.x, cell.y)));
            if (candidates.Count == 0) break;
            var selected = candidates[GameplayRandom.Range(0, candidates.Count)];
            minedCellOwners[selected] = CrumblingTileOwner;
            crumbleRestoreMoves[selected] = move + 2;
            breaking.Add(selected);
        }
        if (breaking.Count == 0) yield break;
        CellsShaking?.Invoke(breaking, CrumbleShakeSeconds);
        yield return new WaitForSeconds(CrumbleShakeSeconds);
        var removed = new HashSet<Gem>();
        foreach (var cell in breaking)
        {
            CellMiningStarted?.Invoke(cell.x, cell.y, MiningTileFlashDuration);
            var gem = GetGem(cell.x, cell.y);
            if (gem == null) continue;
            removed.Add(gem);
            GemMatchVFXRequested?.Invoke(new GemMatchVFXContext(gem.Type, 1, 0,
                new[] {gem.transform.position}, matchFlashDuration));
        }
        // Environmental removal: no reward report and no special creation.
        yield return ClearMatches(removed, null);
        yield return ResolveEnvironmentalBoardChange();
    }

    /// <summary>Clean only zone-owned effects in a detached travel checkpoint.</summary>
    public void PrepareZoneArrival(BoardCombatSnapshot saved, ZoneDefinition destination)
    {
        if (destination == null) throw new ArgumentNullException(nameof(destination));
        saved.vines.Clear();
        saved.aquatic = destination.periodicallyFloods ? new AquaticEnvironmentState { lastSettledMove = saved.moves } : null;
        saved.warnings.RemoveAll(w => w.environmental);
        saved.nextVineGrowthMove = saved.moves + Mathf.Max(1, destination.vineCadenceMoves);
        saved.dungeonRulesVersion = 1;
        saved.nextCrumbleMove = saved.moves + Mathf.Max(3, destination.crumbleCadenceMoves);
        var openings = new List<BoardCellSnapshot>();
        foreach (var cell in saved.cells)
        {
            bool root = cell.barricade && IsRootStyle(cell.barricadeStyle);
            if (cell.crumbleRestoreMove <= 0 && !root && cell.barricadeStyle != EnemyBarricadeStyle.AirCoffer) continue;
            cell.mined = cell.barricade = false;
            cell.crumbleRestoreMove = 0;
            openings.Add(cell);
        }
        // Arrival is an atomic restore beneath smoke, not a rewardable clear.
        // Reopened zone holes receive ordinary gems without creating free matches.
        var random = new SavedRandom(saved.refillRandom == 0 ? 7919u : saved.refillRandom);
        foreach (var cell in openings)
        {
            var allowed = new List<GemType>();
            for (int type = 0; type < gemSprites.Length; type++)
            {
                bool match = false;
                for (int axis = 0; axis < 2; axis++) for (int start = -2; start <= 0; start++)
                {
                    bool same = true;
                    for (int offset = start; offset <= start + 2; offset++)
                    {
                        if (offset == 0) continue;
                        int x = cell.x + (axis == 0 ? offset : 0), y = cell.y + (axis == 1 ? offset : 0);
                        var neighbor = saved.cells.Find(c => c.x == x && c.y == y);
                        same &= neighbor != null && neighbor.hasGem && (int)neighbor.type == type;
                    }
                    match |= same;
                }
                if (!match) allowed.Add((GemType)type);
            }
            cell.type = allowed.Count > 0 ? allowed[random.Next(allowed.Count)] : (GemType)random.Next(gemSprites.Length);
            cell.hasGem = true; cell.identity = ++saved.nextGem;
            cell.special = GemSpecialType.None; cell.pinned = cell.frozen = cell.movable = false;
        }
        if (openings.Count > 0) saved.refillRandom = random.State;
    }
}
