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
                feeds |= drill.horizontal ? gem.Row == drill.lane && gem.Column < 2
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
                MineDrillFired?.Invoke(drill.id, drill.horizontal, drill.lane);
                var cleared = new HashSet<Gem>();
                int length = drill.horizontal ? width : height;
                for (int step = 0; step < length; step++)
                {
                    var cell = drill.horizontal ? new Vector2Int(step, drill.lane) : new Vector2Int(drill.lane, step);
                    if (!visited.Add(cell)) continue;
                    if (barricadeCells.TryGetValue(cell, out var barrier))
                    {
                        // Environmental infrastructure destroys the whole object,
                        // irrespective of material tier or remaining durability.
                        barricadeCells.Remove(cell); RootDestroyed(barrier); AquaticCofferBroken(cell);
                        QueueRoyalBannerGravityOpening(cell.x, cell.y); StartBarricadeHitVFX(barrier, true);
                    }
                    if (royalBannerCells.TryGetValue(cell, out var banner)) CompleteRoyalBannerRemoval(banner);
                    var gem = GetGem(cell.x, cell.y);
                    if (gem != null) cleared.Add(gem);
                }
                // Includes special gems, with no detonation, report or energy.
                if (cleared.Count > 0) yield return ClearMatches(cleared, null);
            }
            deferRoyalBannerGravity = false;
            ResolvePendingRoyalBannerGravity();
            yield return ResolveEnvironmentalBoardChange();
        }
        finally
        {
            deferRoyalBannerGravity = false;
            resolvingUnrewardedEnvironment = previous;
        }
        MineChanged?.Invoke(); MineDrillBatchSettled?.Invoke();
    }
}
