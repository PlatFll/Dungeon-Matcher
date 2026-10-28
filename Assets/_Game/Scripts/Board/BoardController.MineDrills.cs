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
    public event Action<int, Vector2Int> MineDrillExploded;
    public event Action<int> MineDrillReloading;
    public const float MineDrillTravelSeconds = .075f, MineDrillBurstSeconds = .18f, MineDrillReloadSeconds = .16f;
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
                    MineDrillTravelling?.Invoke(drill.id, cell, MineDrillTravelSeconds);
                    yield return new WaitForSeconds(MineDrillTravelSeconds);
                    // A shifted launcher can share a lane. Empty cells still receive the
                    // same readable cadence, but cannot clear or reward twice.
                    if (!visited.Add(cell)) { yield return new WaitForSeconds(.05f); continue; }
                    if (barricadeCells.TryGetValue(cell, out var barrier))
                    {
                        // Environmental infrastructure destroys the whole object,
                        // irrespective of material tier or remaining durability.
                        barricadeCells.Remove(cell); RootDestroyed(barrier); AquaticCofferBroken(cell);
                        QueueRoyalBannerGravityOpening(cell.x, cell.y); StartBarricadeHitVFX(barrier, true);
                    }
                    if (royalBannerCells.TryGetValue(cell, out var banner)) CompleteRoyalBannerRemoval(banner);
                    var gem = GetGem(cell.x, cell.y);
                    // Canonical shatter supplies the short impact hold; no refill yet.
                    // Includes specials without activation, reporting, energy or fuel.
                    if (gem != null) yield return ClearMatches(new HashSet<Gem> { gem }, null);
                    else yield return new WaitForSeconds(.05f);
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
}
