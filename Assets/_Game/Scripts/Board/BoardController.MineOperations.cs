using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum MineStoneOperation { Harden, ArmCharge, DetonateCharge, Extract }
public readonly struct MineStoneTarget
{
    public readonly Vector2Int Cell;
    public readonly MineStoneState State;
    public readonly int Durability;
    public MineStoneTarget(Vector2Int cell, MineStoneState state, int durability)
    { Cell = cell; State = state; Durability = durability; }
}

public partial class BoardController
{
    public IEnumerable<MineStoneTarget> MineStoneTargets() => barricadeCells
        .Where(p => IsMineStone(p.Value) && p.Value.MineStone != null)
        .OrderBy(p => p.Value.MineStone.id)
        .Select(p => new MineStoneTarget(p.Key, p.Value.MineStone.Copy(), p.Value.RemainingDurability));
    public MineStoneTarget? FindMineStone(long id)
    {
        foreach (var p in barricadeCells)
            if (IsMineStone(p.Value) && p.Value.MineStone?.id == id)
                return new MineStoneTarget(p.Key, p.Value.MineStone.Copy(), p.Value.RemainingDurability);
        return null;
    }

    public bool TryQueueMineStoneOperation(EnemyActor owner, long id, MineStoneOperation operation, Action<bool> completed)
    {
        if (!UsesMine || owner == null || owner.IsDefeated || FindMineStone(id) == null) return false;
        EnqueueBoardMutation(new BoardMutationRequest { Kind = BoardMutationKind.MineStoneOperation,
            OwnerActor = owner, MineStoneId = id, MineOperation = operation, Completed = completed,
            IsCancelled = () => MineCasterCancelled(owner) });
        TryStartBoardMutationProcessor(); return true;
    }

    public void ReleaseMineCharges(long ownerId)
    {
        foreach (var value in barricadeCells.Values)
            if (IsMineStone(value) && value.MineStone?.bombOwnerId == ownerId) value.MineStone.bombOwnerId = 0;
        MineChanged?.Invoke();
    }
    public bool TryQueueExtractMineStone(EnemyActor owner, long id, Action<bool, MineStoneStage> completed)
    {
        if (!UsesMine || MineCasterCancelled(owner) || FindMineStone(id) == null) return false;
        var request = new BoardMutationRequest { Kind = BoardMutationKind.MineStoneOperation,
            OwnerActor = owner, MineStoneId = id, MineOperation = MineStoneOperation.Extract,
            IsCancelled = () => MineCasterCancelled(owner) };
        request.Completed = ok => completed?.Invoke(ok,request.MineExtractedStage);
        EnqueueBoardMutation(request); TryStartBoardMutationProcessor(); return true;
    }
    private bool DefuseMineCharge(BarricadeCellState stone)
    {
        if (!IsMineStone(stone) || stone.MineStone == null || stone.MineStone.bombOwnerId <= 0) return false;
        stone.MineStone.bombOwnerId = 0; MineChanged?.Invoke(); return true;
    }
    private IEnumerator ExecuteMineStoneOperation(BoardMutationRequest request)
    {
        var target = FindMineStone(request.MineStoneId);
        if (target == null) yield break;
        var cell = target.Value.Cell; var barrier = barricadeCells[cell]; var stone = barrier.MineStone;
        // Cores belong to their active ritual; other stone specialists do not steal it.
        if (stone.isCore && (request.MineOperation != MineStoneOperation.Extract ||
            request.OwnerActor.PersistentId != stone.ownerId)) yield break;
        switch (request.MineOperation)
        {
            case MineStoneOperation.Harden:
                if (stone.stage == MineStoneStage.Obsidian) yield break;
                stone.stage++; stone.ignoredMoves = 0; stone.lastAdvanceMove = MineMove;
                barrier.MaximumDurability = barrier.RemainingDurability = (int)stone.stage + 1;
                CreateOrRefreshBarricadeView(cell, barrier); StartBarricadeMaterialization(barrier); break;
            case MineStoneOperation.ArmCharge:
                if (stone.bombOwnerId != 0) yield break;
                stone.bombOwnerId = request.OwnerActor.PersistentId; break;
            case MineStoneOperation.DetonateCharge:
                if (stone.bombOwnerId != request.OwnerActor.PersistentId) yield break;
                stone.bombOwnerId = 0; DamageMineStoneExact(cell, 2);
                // The actor's one damage packet is emitted by the successful callback.
                bool prior = resolvingUnrewardedEnvironment; resolvingUnrewardedEnvironment = true;
                try { yield return ResolveEnvironmentalBoardChange(); }
                finally { resolvingUnrewardedEnvironment = prior; }
                break;
            case MineStoneOperation.Extract:
                request.MineExtractedStage = stone.stage;
                barricadeCells.Remove(cell); QueueRoyalBannerGravityOpening(cell.x,cell.y);
                StartBarricadeHitVFX(barrier,true);
                bool previous = resolvingUnrewardedEnvironment; resolvingUnrewardedEnvironment = true;
                try { yield return ResolveEnvironmentalBoardChange(); }
                finally { resolvingUnrewardedEnvironment = previous; }
                break;
        }
        request.Succeeded = true; MineChanged?.Invoke();
    }
    private void DamageMineStoneExact(Vector2Int cell, int hits)
    {
        if (!barricadeCells.TryGetValue(cell, out var barrier) || !IsMineStone(barrier)) return;
        barrier.RemainingDurability -= hits; barrier.MineStone.RecordHit(MineMove);
        bool removed = barrier.RemainingDurability <= 0;
        if (removed) { barricadeCells.Remove(cell); QueueRoyalBannerGravityOpening(cell.x, cell.y); }
        StartBarricadeHitVFX(barrier, removed); MineChanged?.Invoke();
    }

    public void LeastStoneMineLane(out bool horizontal, out int lane)
    {
        horizontal = true; lane = 0; int best = int.MaxValue;
        // Stable ties: rows bottom-to-top, then columns left-to-right.
        for (int axis = 0; axis < 2; axis++) for (int index = 0; index < (axis == 0 ? height : width); index++)
        {
            int count = 0;
            foreach (var p in barricadeCells)
                if (IsMineStone(p.Value) && (axis == 0 ? p.Key.y : p.Key.x) == index) count++;
            if (count < best) { best = count; horizontal = axis == 0; lane = index; }
        }
    }
    public bool TryQueueSmallMineDrill(EnemyActor owner, bool horizontal, int lane, Action<bool> reachedEdge)
    {
        if (!UsesMine || owner == null || owner.IsDefeated || lane < 0 || lane >= (horizontal ? height : width)) return false;
        var request = new BoardMutationRequest { Kind = BoardMutationKind.SmallMineDrill, OwnerActor = owner,
            MineHorizontal = horizontal, MineLane = lane, IsCancelled = () => MineCasterCancelled(owner) };
        request.Completed = ok => reachedEdge?.Invoke(ok && request.MineReachedEdge);
        EnqueueBoardMutation(request); TryStartBoardMutationProcessor(); return true;
    }
    private IEnumerator ExecuteSmallMineDrill(BoardMutationRequest request)
    {
        bool prior = resolvingUnrewardedEnvironment; resolvingUnrewardedEnvironment = true;
        var cleared = new HashSet<Gem>(); bool stopped = false;
        try
        {
            int length = request.MineHorizontal ? width : height;
            for (int step = 0; step < length; step++)
            {
                var cell = request.MineHorizontal ? new Vector2Int(step, request.MineLane) : new Vector2Int(request.MineLane, step);
                if (barricadeCells.TryGetValue(cell, out var barrier))
                {
                    if (IsMineStone(barrier)) DamageMineStoneExact(cell, 1);
                    stopped = true; break; // Even a just-broken first stone ends the projectile.
                }
                var gem = GetGem(cell.x, cell.y);
                if (gem != null && gem.SpecialType == GemSpecialType.None) cleared.Add(gem);
            }
            ExecuteMineDrillPower(new BoardMutationRequest { MineDrillId = request.MineHorizontal ? 1 : 2, MinePower = 1 });
            if (cleared.Count > 0) yield return ClearMatches(cleared, null);
            yield return ResolveEnvironmentalBoardChange();
        }
        finally { resolvingUnrewardedEnvironment = prior; }
        request.MineReachedEdge = !stopped; request.Succeeded = true;
    }

    public bool TryQueueMineDrillShift(EnemyActor owner, int id, int lane, Action<bool> completed)
    {
        var drill = mine?.drills?.Find(d => d.id == id);
        if (!UsesMine || owner == null || owner.IsDefeated || drill == null || lane < 0 ||
            lane >= (drill.horizontal ? height : width) || Math.Abs(lane - drill.lane) != 1) return false;
        EnqueueBoardMutation(new BoardMutationRequest { Kind = BoardMutationKind.ShiftMineDrill,
            OwnerActor = owner, MineDrillId = id, MineLane = lane, Completed = completed,
            IsCancelled = () => MineCasterCancelled(owner) });
        TryStartBoardMutationProcessor(); return true;
    }
    private void ExecuteMineDrillShift(BoardMutationRequest request)
    {
        var drill = mine?.drills?.Find(d => d.id == request.MineDrillId);
        if (drill == null || Math.Abs(drill.lane - request.MineLane) != 1) return;
        drill.lane = request.MineLane; request.Succeeded = true; MineChanged?.Invoke();
    }
    private static bool MineCasterCancelled(EnemyActor actor) => actor == null || actor.IsDefeated ||
        !actor.isActiveAndEnabled || actor.GetComponent<EnemyStagger>()?.IsStaggered == true;
}
