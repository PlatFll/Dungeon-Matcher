using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BoardController
{
    // Scoped to the canonical resolver, never a competing gravity/cascade loop.
    private bool resolvingUnrewardedEnvironment;
    public event Action<int, bool, int> MineDrillFired;
    public event Action MineDrillBatchSettled;
    // Presentation observes this board-owned timeline; no animation callback clears gems.
    public event Action<int, Vector2Int, float> MineDrillTravelling;
    public event Action<int, Vector2Int, float> MineDrillBoring;
    public event Action<int, Vector2Int> MineDrillExploded;
    public event Action<int> MineDrillReloading;
    public const float MineDrillTravelSeconds = .075f, MineDrillBurstSeconds = .18f, MineDrillReloadSeconds = .16f;
    public const float MineDrillApproachSeconds = .035f, MineDrillBoreSeconds = .14f;
    private readonly HashSet<int> airborneMineDrills = new HashSet<int>();
    public bool IsMineDrillInFlight(int id) => airborneMineDrills.Contains(id);
    public int MineDrillForCycle(int cycle) => mine?.drills?.Count > 0 ? mine.drills[Math.Abs(cycle) % mine.drills.Count].id : 1;
    public int MineDrillCapacity => Mathf.Max(1, MineZone?.mineDrillCapacity ?? 4);
    public bool HasReadyMineDrill => UsesMine && mine?.drills != null && mine.drills.Exists(d => d.charge >= MineDrillCapacity);

    /// <summary>Zero means all existing drills. A feed can never manufacture a new drill.</summary>
    public bool TryQueueMineDrillPower(int drillId, int units)
    {
        if (!UsesMine || units <= 0 || gems == null) return false;
        InitializeMine();
        if (drillId != 0 && !mine.drills.Exists(d => d.id == drillId)) return false;
        EnqueueBoardMutation(new BoardMutationRequest { Kind = BoardMutationKind.ChargeMineDrills,
            MineDrillId = drillId, MinePower = units });
        TryStartBoardMutationProcessor(); return true;
    }

    private void RegisterMinePlayerMatch(HashSet<Gem> matches, bool deliberate)
    {
        if (!UsesMine || !deliberate || resolvingUnrewardedEnvironment || matches == null) return;
        InitializeMine(); int action = completedValidPlayerMoves + 1;
        foreach (var drill in mine.drills)
        {
            if (drill.lastPlayerAction >= action) continue;
            // Two cells immediately inside the intake are the visible feed zone.
            bool feeds = false;
            foreach (var gem in matches) if (gem != null)
                feeds |= drill.horizontal ? gem.Row == drill.lane && (drill.fromRight ? gem.Column >= width - 2 : gem.Column < 2)
                    : gem.Column == drill.lane && gem.Row < 2;
            if (!feeds) continue;
            drill.lastPlayerAction = action;
            TryQueueMineDrillPower(drill.id, 1);
        }
    }

    private void ExecuteMineDrillPower(BoardMutationRequest request)
    {
        if (!UsesMine || mine?.drills == null) return;
        foreach (var drill in mine.drills)
            if (request.MineDrillId == 0 || request.MineDrillId == drill.id)
                drill.charge = Math.Min(MineDrillCapacity, drill.charge + Math.Min(MineDrillCapacity, request.MinePower));
        request.Succeeded = true; MineChanged?.Invoke();
    }

    /// <summary>The move coordinator drains once after all actor opportunities.</summary>
    public IEnumerator DrainMineDrills()
    {
        if (!UsesMine) yield break;
        EnqueueBoardMutation(new BoardMutationRequest { Kind = BoardMutationKind.FireMineDrills });
        TryStartBoardMutationProcessor();
        while (IsBusy) yield return null;
    }

    private IEnumerator ExecuteReadyMineDrills()
    {
        if (!HasReadyMineDrill) yield break;
        var firing = mine.drills.FindAll(d => d.charge >= MineDrillCapacity);
        firing.Sort((a, b) => a.id.CompareTo(b.id));
        // Reserve every launch before callbacks. Extra fuel saturates at one shot.
        foreach (var drill in firing) drill.charge -= MineDrillCapacity;
        MineChanged?.Invoke();
        bool previous = resolvingUnrewardedEnvironment;
        resolvingUnrewardedEnvironment = true;
        deferRoyalBannerGravity = true;
        var visited = new HashSet<Vector2Int>();
        var crystals = new List<BombTriggeredCrystalRequest>();
        try
        {
            foreach (var drill in firing)
            {
                airborneMineDrills.Add(drill.id);
                MineDrillFired?.Invoke(drill.id, drill.horizontal, drill.lane);
                int length = drill.horizontal ? width : height;
                for (int step = 0; step < length; step++)
                {
                    int column = drill.fromRight ? width - 1 - step : step;
                    var cell = drill.horizontal ? new Vector2Int(column, drill.lane) : new Vector2Int(drill.lane, step);
                    bool first = step == 0;
                    float launchTime = Time.time;
                    // Launch punches through the first cell immediately. Subsequent
                    // occupied cells stay intact while the tip drills at their edge.
                    if (first) MineDrillTravelling?.Invoke(drill.id, cell, MineDrillTravelSeconds);
                    bool occupied = GetGem(cell.x, cell.y) != null || barricadeCells.ContainsKey(cell) || royalBannerCells.ContainsKey(cell);
                    if (!first && !visited.Contains(cell) && occupied)
                    {
                        MineDrillBoring?.Invoke(drill.id, cell, MineDrillApproachSeconds);
                        yield return new WaitForSeconds(MineDrillApproachSeconds + MineDrillBoreSeconds);
                    }
                    // A shifted/opposed launcher can share this row. Empty cells
                    // have no resistance, and an object can never be cleared twice.
                    if (visited.Add(cell))
                    {
                        if (barricadeCells.TryGetValue(cell, out var barrier))
                        {
                            barricadeCells.Remove(cell); RootDestroyed(barrier); AquaticCofferBroken(cell);
                            QueueRoyalBannerGravityOpening(cell.x, cell.y); StartBarricadeHitVFX(barrier, true, immediateImpact: true);
                        }
                        if (royalBannerCells.TryGetValue(cell, out var banner)) CompleteRoyalBannerRemoval(banner);
                        var gem = GetGem(cell.x, cell.y);
                        // Ordinary drilling remains unrewarded. A struck bomb
                        // detonates through the existing bomb resolver at contact.
                        if (gem != null) yield return ClearDrillImpacts(
                            new HashSet<Gem> { gem }, crystals, first);
                    }
                    if (!first)
                    {
                        MineDrillTravelling?.Invoke(drill.id, cell, MineDrillTravelSeconds);
                        yield return new WaitForSeconds(MineDrillTravelSeconds);
                    }
                    else
                    {
                        float remaining = MineDrillTravelSeconds - (Time.time - launchTime);
                        if (remaining > 0f) yield return new WaitForSeconds(remaining);
                    }
                }
                var exit = new Vector2Int(drill.fromRight ? -1 : width, drill.lane);
                MineDrillTravelling?.Invoke(drill.id, exit, MineDrillTravelSeconds);
                yield return new WaitForSeconds(MineDrillTravelSeconds);
                MineDrillExploded?.Invoke(drill.id, exit);
                yield return new WaitForSeconds(MineDrillBurstSeconds);
                airborneMineDrills.Remove(drill.id);
                MineDrillReloading?.Invoke(drill.id);
                yield return new WaitForSeconds(MineDrillReloadSeconds);
            }
            deferRoyalBannerGravity = false;
            ResolvePendingRoyalBannerGravity();
            // Stars wait until every reserved projectile has left the board;
            // their established charge/refill sequence cannot move drill targets.
            yield return ResolveBombTriggeredCrystalRequests(crystals);
            yield return ResolveEnvironmentalBoardChange();
        }
        finally
        {
            airborneMineDrills.Clear();
            deferRoyalBannerGravity = false;
            resolvingUnrewardedEnvironment = previous;
        }
        MineChanged?.Invoke(); MineDrillBatchSettled?.Invoke();
    }

    private IEnumerator ClearDrillImpacts(HashSet<Gem> impacts,
        List<BombTriggeredCrystalRequest> crystals, bool immediateImpact = false)
    {
        var bombs = new HashSet<Gem>();
        foreach (var gem in impacts)
            if (gem != null && IsChainReactiveBomb(gem.SpecialType)) bombs.Add(gem);
        var expanded = BuildBombExpandedClearSet(bombs, true, out var triggered);
        foreach (var gem in impacts)
            if (gem != null && gem.SpecialType == GemSpecialType.ColorCrystal)
                triggered.Add(new BombTriggeredCrystalRequest(gem, gem.Type));
        foreach (var request in triggered)
            if (!crystals.Exists(r => r.CrystalGem == request.CrystalGem)) crystals.Add(request);
        // Only actual bomb effects earn their existing combat effects/rewards.
        // The drill's ordinary cells, refill cascades and intake stay unrewarded.
        if (expanded.Count > 0) ReportBombClearsToCombat(null, expanded, 0);
        foreach (var gem in impacts)
            if (gem != null && gem.SpecialType != GemSpecialType.ColorCrystal) expanded.Add(gem);
        if (expanded.Count > 0) yield return ClearMatchesWithOrderedBursts(
            expanded, null, true, null, null, immediateImpact: immediateImpact);
    }

}
