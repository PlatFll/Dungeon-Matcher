using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BoardController
{
    private const float MiningTileFlashDuration = 0.10f;
    private const float AnimationImpactFailsafeSeconds = 3f;

    public event Action<int> ValidPlayerMoveCompleted;
    public event Action<int, int, float> CellMiningStarted;
    public event Action<int, int> CellRestored;

    private enum BoardMutationKind
    {
        MineRandomCell,
        RestoreOwnerCells,
        PinRandomGem,
        ReleaseOwnerPins,
        PlaceBarricades,
        MarkGemPair,
        ResolveGemPair,
        TopUpMovablePins,
        PlaceRoyalBanner,
        MarkGemSet,
        ResolveGemSet,
        MarkLanes,
        ResolveLanes
    }

    private sealed class BoardMutationRequest
    {
        public BoardMutationKind Kind;
        public EnemyActor OwnerActor;
        public int OwnerInstanceId;
        public int MaximumOwnedMines;
        public int MaximumOwnedPins;
        public Gem TargetGem;
        public bool WaitForAnimationImpact;
        public bool AnimationImpactReached;
        public int AnimationActionId;

        public bool PreferStraightLine;
        public bool ProtectSpecialGems;
        public Action<bool> Completed;
        public Func<bool> IsCancelled;
        public bool Succeeded;
        public bool MovablePin;
        public GemPairThreat PairThreat;
        public int WarningMoves;
        public int PlayerDamage;
        public GemSetThreat SetThreat;
        public LaneThreat Lanes;
        public int TargetCount;
        public Action Pulse;
        public bool RestorationPresentation;

        public int BarricadeCount;
        public int MaximumOwnedBarricades;
        public int BarricadeDurability;
        public EnemyBarricadeStyle BarricadeStyle;
    }

    private readonly Dictionary<Vector2Int, int> minedCellOwners =
        new Dictionary<Vector2Int, int>();

    private readonly Queue<BoardMutationRequest> pendingBoardMutations =
        new Queue<BoardMutationRequest>();

    private readonly HashSet<int> pendingRestoreOwners =
        new HashSet<int>();

    private Coroutine boardMutationCoroutine;
    private BoardMutationKind? activeBoardMutationKind;
    private BoardMutationRequest activeBoardMutationRequest;
    private int completedValidPlayerMoves;

    public bool HasPendingBoardMutation =>
        boardMutationCoroutine != null ||
        pendingBoardMutations.Count > 0;

    public bool IsCellPlayable(int column, int row)
    {
        if (column < 0 ||
            column >= width ||
            row < 0 ||
            row >= height)
        {
            return false;
        }

        Vector2Int cell = new Vector2Int(column, row);

        return
            !minedCellOwners.ContainsKey(cell) &&
            !barricadeCells.ContainsKey(cell) &&
            !IsCellRoyalBanner(column, row);
    }

    public bool IsCellMined(int column, int row)
    {
        return
            column >= 0 &&
            column < width &&
            row >= 0 &&
            row < height &&
            minedCellOwners.ContainsKey(
                new Vector2Int(column, row));
    }

    public int GetMinedCellCountForOwner(int ownerInstanceId)
    {
        if (ownerInstanceId == 0)
        {
            return 0;
        }

        int count = 0;

        foreach (KeyValuePair<Vector2Int, int> entry
                 in minedCellOwners)
        {
            if (entry.Value == ownerInstanceId)
            {
                count++;
            }
        }

        return count;
    }

    public bool TryQueueMineRandomCell(
        EnemyActor owner,
        int maximumOwnedMines,
        bool waitForAnimationImpact = false)
    {
        if (owner == null ||
            owner.IsDefeated ||
            !owner.IsInitialized ||
            gems == null)
        {
            return false;
        }

        int ownerInstanceId = owner.GetInstanceID();
        int safeMaximum = Mathf.Max(1, maximumOwnedMines);

        if (GetMinedCellCountForOwner(ownerInstanceId) >= safeMaximum)
        {
            return false;
        }

        if (waitForAnimationImpact &&
            HasPendingAnimationTimedMineRequest(ownerInstanceId))
        {
            return false;
        }

        if (!HasMineableCell())
        {
            return false;
        }

        pendingBoardMutations.Enqueue(
            new BoardMutationRequest
            {
                Kind = BoardMutationKind.MineRandomCell,
                OwnerActor = owner,
                OwnerInstanceId = ownerInstanceId,
                MaximumOwnedMines = safeMaximum,
                WaitForAnimationImpact = waitForAnimationImpact,
                AnimationActionId = owner.ActiveSpecialAbilityAnimationActionId
            }
        );

        TryStartBoardMutationProcessor();
        return true;
    }

    public bool NotifyMineAnimationImpact(EnemyActor owner) =>
        NotifyBoardMutationAnimationImpact(owner, BoardMutationKind.MineRandomCell);

    public bool NotifyBarricadeAnimationImpact(EnemyActor owner) =>
        NotifyBoardMutationAnimationImpact(owner, BoardMutationKind.PlaceBarricades);

    private bool NotifyBoardMutationAnimationImpact(EnemyActor owner, BoardMutationKind kind)
    {
        if (owner == null || owner.IsDefeated || !owner.isActiveAndEnabled || Time.timeScale <= 0f)
            return false;
        if (TryReleaseAnimationRequest(activeBoardMutationRequest, owner, kind)) return true;
        foreach (var request in pendingBoardMutations)
            if (TryReleaseAnimationRequest(request, owner, kind)) return true;
        return false;
    }

    private static bool TryReleaseAnimationRequest(BoardMutationRequest request, EnemyActor owner, BoardMutationKind kind)
    {
        if (request == null || request.Kind != kind || !request.WaitForAnimationImpact ||
            request.OwnerActor != owner ||
            (request.AnimationActionId > 0 && request.AnimationActionId != owner.ActiveSpecialAbilityAnimationActionId))
            return false;
        // Acknowledging the same request twice is idempotent; there is still
        // only one mutation in the authoritative queue.
        request.AnimationImpactReached = true;
        return true;
    }

    public void QueueRestoreMinedCells(int ownerInstanceId)
    {
        if (ownerInstanceId == 0 ||
            !pendingRestoreOwners.Add(ownerInstanceId))
        {
            return;
        }

        pendingBoardMutations.Enqueue(
            new BoardMutationRequest
            {
                Kind = BoardMutationKind.RestoreOwnerCells,
                OwnerInstanceId = ownerInstanceId
            }
        );

        TryStartBoardMutationProcessor();
    }

    private void NotifyValidPlayerMoveCompleted()
    {
        completedValidPlayerMoves++;
        ValidPlayerMoveCompleted?.Invoke(completedValidPlayerMoves);
    }

    private bool HasMineableCell()
    {
        for (int row = 0; row < height; row++)
        {
            for (int column = 0; column < width; column++)
            {
                if (!IsCellPlayable(column, row))
                {
                    continue;
                }

                Gem gem = GetGem(column, row);

                if (gem != null && !IsGemPinned(gem))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool HasPendingAnimationTimedMineRequest(
        int ownerInstanceId)
    {
        if (IsAnimationTimedMineRequestForOwner(
                activeBoardMutationRequest,
                ownerInstanceId))
        {
            return true;
        }

        foreach (BoardMutationRequest request
                 in pendingBoardMutations)
        {
            if (IsAnimationTimedMineRequestForOwner(
                    request,
                    ownerInstanceId))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsAnimationTimedMineRequestForOwner(
        BoardMutationRequest request,
        int ownerInstanceId)
    {
        return
            request != null &&
            request.Kind == BoardMutationKind.MineRandomCell &&
            request.WaitForAnimationImpact &&
            request.OwnerInstanceId == ownerInstanceId;
    }

    private void TryStartBoardMutationProcessor()
    {
        if (boardMutationCoroutine != null ||
            !isActiveAndEnabled)
        {
            return;
        }

        boardMutationCoroutine =
            StartCoroutine(ProcessBoardMutations());
    }

    private IEnumerator ProcessBoardMutations()
    {
        yield return null;
        bool acquiredBoardBusy = false;

        try
        {
            while (isBusy)
            {
                yield return null;
            }

            isBusy = true;
            acquiredBoardBusy = true;

            while (pendingBoardMutations.Count > 0)
            {
                BoardMutationRequest request =
                    pendingBoardMutations.Dequeue();

                activeBoardMutationRequest = request;
                activeBoardMutationKind = request.Kind;

                if (request.IsCancelled != null && request.IsCancelled())
                {
                    request.Completed?.Invoke(false);
                    activeBoardMutationKind = null;
                    activeBoardMutationRequest = null;
                    continue;
                }

                switch (request.Kind)
                {
                    case BoardMutationKind.MarkGemSet:
                        ExecuteMarkGemSet(request);
                        break;
                    case BoardMutationKind.ResolveGemSet:
                        yield return ExecuteResolveGemSet(request);
                        break;
                    case BoardMutationKind.MarkLanes:
                        ExecuteMarkLanes(request);
                        break;
                    case BoardMutationKind.ResolveLanes:
                        yield return ExecuteResolveLanes(request);
                        break;
                    case BoardMutationKind.TopUpMovablePins:
                        yield return ExecuteTopUpMovablePins(request);
                        break;

                    case BoardMutationKind.MineRandomCell:
                        yield return ExecuteMineRequest(request);
                        break;

                    case BoardMutationKind.RestoreOwnerCells:
                        pendingRestoreOwners.Remove(
                            request.OwnerInstanceId);
                        yield return ExecuteRestoreRequest(
                            request.OwnerInstanceId);
                        break;

                    case BoardMutationKind.PinRandomGem:
                        yield return ExecutePinRequest(request);
                        break;

                    case BoardMutationKind.ReleaseOwnerPins:
                        pendingPinReleaseOwners.Remove(
                            request.OwnerInstanceId);
                        yield return ExecuteReleasePinsRequest(
                            request.OwnerInstanceId);
                        break;

                    case BoardMutationKind.MarkGemPair:
                        ExecuteMarkGemPair(request);
                        break;

                    case BoardMutationKind.ResolveGemPair:
                        yield return ExecuteResolveGemPair(request);
                        break;

                    case BoardMutationKind.PlaceBarricades:
                        yield return ExecutePlaceBarricadesRequest(request);
                        break;

                    case BoardMutationKind.PlaceRoyalBanner:
                        yield return ExecutePlaceRoyalBannerRequest(request);
                        break;
                }

                request.Completed?.Invoke(request.Succeeded);
                activeBoardMutationKind = null;
                activeBoardMutationRequest = null;
            }
        }
        finally
        {
            activeBoardMutationKind = null;
            activeBoardMutationRequest = null;

            if (acquiredBoardBusy)
            {
                isBusy = false;
            }

            boardMutationCoroutine = null;
        }
    }

    private static bool IsAnimationRequestCancelled(BoardMutationRequest request)
    {
        return request.OwnerActor == null || request.OwnerActor.IsDefeated ||
            !request.OwnerActor.isActiveAndEnabled ||
            (request.IsCancelled != null && request.IsCancelled()) ||
            (request.WaitForAnimationImpact && !request.AnimationImpactReached &&
             request.AnimationActionId > 0 &&
             request.OwnerActor.ActiveSpecialAbilityAnimationActionId != request.AnimationActionId);
    }

    private IEnumerator WaitForBoardMutationAnimationImpact(BoardMutationRequest request)
    {
        float started = Time.time;
        while (request.WaitForAnimationImpact && !request.AnimationImpactReached)
        {
            if (IsAnimationRequestCancelled(request))
            {
                ReleaseCancelledAnimationAction(request);
                yield break;
            }
            if (Time.time - started >= AnimationImpactFailsafeSeconds)
            {
                Debug.LogWarning(request.OwnerActor.name +
                    " missed AbilityImpact; resolving through the board fallback.", request.OwnerActor);
                request.AnimationImpactReached = true;
                request.OwnerActor.EndSpecialAbilityAnimationAction();
                break;
            }
            yield return null;
        }
        if (IsAnimationRequestCancelled(request)) ReleaseCancelledAnimationAction(request);
    }

    private static void ReleaseCancelledAnimationAction(BoardMutationRequest request)
    {
        if (request.AnimationActionId > 0 && request.OwnerActor != null &&
            request.OwnerActor.ActiveSpecialAbilityAnimationActionId == request.AnimationActionId)
            request.OwnerActor.EndSpecialAbilityAnimationAction();
    }

    private IEnumerator ExecuteMineRequest(
        BoardMutationRequest request)
    {
        if (request == null ||
            request.OwnerActor == null ||
            request.OwnerActor.IsDefeated ||
            request.OwnerInstanceId == 0)
        {
            yield break;
        }

        if (GetMinedCellCountForOwner(request.OwnerInstanceId) >=
            request.MaximumOwnedMines)
        {
            yield break;
        }

        var impactWait = WaitForBoardMutationAnimationImpact(request);
        while (impactWait.MoveNext()) yield return impactWait.Current;
        if (IsAnimationRequestCancelled(request)) yield break;

        if (minedCellOwners.Count >= BalanceV1.Current.maximumGlobalMines ||
            minedCellOwners.Count + barricadeCells.Count >= BalanceV1.Current.maximumGlobalStructures) yield break;

        List<Vector2Int> candidates = BuildMineableCellList();

        if (candidates.Count == 0)
        {
            yield break;
        }

        Vector2Int selectedCell =
            candidates[GameplayRandom.Range(0, candidates.Count)];

        Gem minedGem = GetGem(selectedCell.x, selectedCell.y);

        if (minedGem == null || IsGemPinned(minedGem))
        {
            yield break;
        }

        minedCellOwners[selectedCell] = request.OwnerInstanceId;
        EnsureMiningVFX();

        CellMiningStarted?.Invoke(
            selectedCell.x,
            selectedCell.y,
            MiningTileFlashDuration
        );

        GemMatchVFXRequested?.Invoke(
            new GemMatchVFXContext(
                minedGem.Type,
                1,
                0,
                new[] { minedGem.transform.position },
                matchFlashDuration
            )
        );

        HashSet<Gem> minedGemOnly =
            new HashSet<Gem> { minedGem };

        yield return ClearMatches(minedGemOnly, null);
        yield return ResolveEnvironmentalBoardChange();
    }

    private List<Vector2Int> BuildMineableCellList()
    {
        List<Vector2Int> candidates = new List<Vector2Int>();

        for (int row = 0; row < height; row++)
        {
            for (int column = 0; column < width; column++)
            {
                if (!IsCellPlayable(column, row))
                {
                    continue;
                }

                Gem gem = GetGem(column, row);

                if (gem == null || IsGemPinned(gem) || gem.SpecialType != GemSpecialType.None)
                {
                    continue;
                }
                var cell = new Vector2Int(column, row);
                // Test against every already-reserved structure, including other
                // owners. This is a bounded local check, never a future-board search.
                minedCellOwners[cell] = int.MinValue;
                bool retainsResponse = RetainsUsefulResponse();
                minedCellOwners.Remove(cell);
                if (retainsResponse) candidates.Add(cell);
            }
        }

        return candidates;
    }

    private IEnumerator ExecuteRestoreRequest(int ownerInstanceId)
    {
        List<Vector2Int> cellsToRestore =
            new List<Vector2Int>();

        foreach (KeyValuePair<Vector2Int, int> entry
                 in minedCellOwners)
        {
            if (entry.Value == ownerInstanceId)
            {
                cellsToRestore.Add(entry.Key);
            }
        }

        if (cellsToRestore.Count == 0)
        {
            yield break;
        }

        foreach (Vector2Int cell in cellsToRestore)
        {
            minedCellOwners.Remove(cell);

            /* A restored hole creates one new gravity slot for banners above. */
            NotifyRoyalBannerSpaceOpened(cell.x, cell.y);

            CellRestored?.Invoke(cell.x, cell.y);
        }

        yield return ResolveEnvironmentalBoardChange();
    }

    private IEnumerator ResolveEnvironmentalBoardChange()
    {
        yield return CollapseAndRefillBoard();

        float settlePause = GetPostFallSettlePause();

        if (settlePause > 0f)
        {
            yield return new WaitForSeconds(settlePause);
        }

        HashSet<Gem> resultingMatches = FindAllMatches();

        if (resultingMatches.Count > 0)
        {
            yield return ResolveCascades(
                resultingMatches,
                null,
                null
            );
        }
        else if (!HasAvailableMove())
        {
            yield return ReshuffleBoard();
        }
    }

    private void EnsureMiningVFX()
    {
        if (GetComponent<BoardMiningVFX>() == null)
        {
            gameObject.AddComponent<BoardMiningVFX>();
        }
    }
}
