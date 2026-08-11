using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BoardController
{
    private const float MiningTileFlashDuration =
        0.10f;

    private const float AnimationImpactFailsafeSeconds =
        3f;

    public event Action<int>
        ValidPlayerMoveCompleted;

    public event Action<int, int, float>
        CellMiningStarted;

    public event Action<int, int>
        CellRestored;

    private enum BoardMutationKind
    {
        MineRandomCell,
        RestoreOwnerCells,
        PinRandomGem,
        ReleaseOwnerPins
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
    }

    /*
     * One source of truth for permanent enemy-created holes.
     * The value is the EnemyActor instance ID that owns the cell.
     * This lets multiple Miners coexist while restoring only the
     * cells belonging to the Miner that died.
     */
    private readonly Dictionary<Vector2Int, int>
        minedCellOwners =
            new Dictionary<Vector2Int, int>();

    private readonly Queue<BoardMutationRequest>
        pendingBoardMutations =
            new Queue<BoardMutationRequest>();

    private readonly HashSet<int>
        pendingRestoreOwners =
            new HashSet<int>();

    private Coroutine boardMutationCoroutine;
    private BoardMutationKind? activeBoardMutationKind;
    private BoardMutationRequest activeBoardMutationRequest;
    private int completedValidPlayerMoves;

    public bool HasPendingBoardMutation =>
        boardMutationCoroutine != null ||
        pendingBoardMutations.Count > 0;

    public bool IsCellPlayable(
        int column,
        int row)
    {
        if (column < 0 ||
            column >= width ||
            row < 0 ||
            row >= height)
        {
            return false;
        }

        return !minedCellOwners.ContainsKey(
            new Vector2Int(
                column,
                row
            )
        );
    }

    public bool IsCellMined(
        int column,
        int row)
    {
        return
            column >= 0 &&
            column < width &&
            row >= 0 &&
            row < height &&
            minedCellOwners.ContainsKey(
                new Vector2Int(
                    column,
                    row
                )
            );
    }

    public int GetMinedCellCountForOwner(
        int ownerInstanceId)
    {
        if (ownerInstanceId == 0)
        {
            return 0;
        }

        int count = 0;

        foreach (
            KeyValuePair<Vector2Int, int> entry
            in minedCellOwners)
        {
            if (entry.Value ==
                ownerInstanceId)
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

        int ownerInstanceId =
            owner.GetInstanceID();

        int safeMaximum =
            Mathf.Max(
                1,
                maximumOwnedMines
            );

        if (GetMinedCellCountForOwner(
                ownerInstanceId) >=
            safeMaximum)
        {
            return false;
        }

        if (waitForAnimationImpact &&
            HasPendingAnimationTimedMineRequest(
                ownerInstanceId))
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
                Kind =
                    BoardMutationKind
                        .MineRandomCell,

                OwnerActor = owner,
                OwnerInstanceId =
                    ownerInstanceId,

                MaximumOwnedMines =
                    safeMaximum,

                WaitForAnimationImpact =
                    waitForAnimationImpact
            }
        );

        TryStartBoardMutationProcessor();
        return true;
    }

    public bool NotifyMineAnimationImpact(
        EnemyActor owner)
    {
        if (owner == null)
        {
            return false;
        }

        int ownerInstanceId =
            owner.GetInstanceID();

        if (TryReleaseMineRequest(
                activeBoardMutationRequest,
                ownerInstanceId))
        {
            return true;
        }

        foreach (BoardMutationRequest request
                 in pendingBoardMutations)
        {
            if (TryReleaseMineRequest(
                    request,
                    ownerInstanceId))
            {
                return true;
            }
        }

        return false;
    }

    public void QueueRestoreMinedCells(
        int ownerInstanceId)
    {
        if (ownerInstanceId == 0 ||
            !pendingRestoreOwners.Add(
                ownerInstanceId))
        {
            return;
        }

        pendingBoardMutations.Enqueue(
            new BoardMutationRequest
            {
                Kind =
                    BoardMutationKind
                        .RestoreOwnerCells,

                OwnerInstanceId =
                    ownerInstanceId
            }
        );

        TryStartBoardMutationProcessor();
    }

    /*
     * Called exactly once after a successful player swap has completely
     * resolved. Cascades never call this method themselves, so a ten-step
     * cascade still advances enemy move counters by only one.
     */
    private void NotifyValidPlayerMoveCompleted()
    {
        completedValidPlayerMoves++;

        ValidPlayerMoveCompleted?.Invoke(
            completedValidPlayerMoves
        );
    }

    private bool HasMineableCell()
    {
        for (int row = 0;
             row < height;
             row++)
        {
            for (int column = 0;
                 column < width;
                 column++)
            {
                if (!IsCellPlayable(
                        column,
                        row))
                {
                    continue;
                }

                Gem gem =
                    GetGem(
                        column,
                        row
                    );

                if (gem != null &&
                    !IsGemPinned(gem))
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

    private bool TryReleaseMineRequest(
        BoardMutationRequest request,
        int ownerInstanceId)
    {
        if (!IsAnimationTimedMineRequestForOwner(
                request,
                ownerInstanceId))
        {
            return false;
        }

        request.AnimationImpactReached = true;
        return true;
    }

    private bool IsAnimationTimedMineRequestForOwner(
        BoardMutationRequest request,
        int ownerInstanceId)
    {
        return
            request != null &&
            request.Kind ==
                BoardMutationKind.MineRandomCell &&
            request.WaitForAnimationImpact &&
            request.OwnerInstanceId ==
                ownerInstanceId;
    }

    private void TryStartBoardMutationProcessor()
    {
        if (boardMutationCoroutine != null ||
            !isActiveAndEnabled)
        {
            return;
        }

        boardMutationCoroutine =
            StartCoroutine(
                ProcessBoardMutations()
            );
    }

    private IEnumerator ProcessBoardMutations()
    {
        bool acquiredBoardBusy = false;

        try
        {
            /*
             * If an enemy queued board work during a match/cascade, wait for
             * that player action to release the board. HasPendingBoardMutation
             * keeps new input and wave transitions out of the handoff.
             */
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
                activeBoardMutationKind =
                    request.Kind;

                switch (request.Kind)
                {
                    case BoardMutationKind.MineRandomCell:
                        yield return
                            ExecuteMineRequest(
                                request
                            );
                        break;

                    case BoardMutationKind.RestoreOwnerCells:
                        pendingRestoreOwners.Remove(
                            request.OwnerInstanceId
                        );

                        yield return
                            ExecuteRestoreRequest(
                                request.OwnerInstanceId
                            );
                        break;

                    case BoardMutationKind.PinRandomGem:
                        yield return
                            ExecutePinRequest(
                                request
                            );
                        break;

                    case BoardMutationKind.ReleaseOwnerPins:
                        pendingPinReleaseOwners.Remove(
                            request.OwnerInstanceId
                        );

                        yield return
                            ExecuteReleasePinsRequest(
                                request.OwnerInstanceId
                            );
                        break;
                }

                activeBoardMutationKind = null;
                activeBoardMutationRequest = null;
            }
        }
        finally
        {
            /*
             * Never leave global board input owned by this processor if the
             * coroutine is stopped or exits unexpectedly. Only clear isBusy if
             * this coroutine actually acquired it; while waiting, another board
             * operation still owns that flag.
             */
            activeBoardMutationKind = null;
            activeBoardMutationRequest = null;

            if (acquiredBoardBusy)
            {
                isBusy = false;
            }

            boardMutationCoroutine = null;
        }
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

        if (GetMinedCellCountForOwner(
                request.OwnerInstanceId) >=
            request.MaximumOwnedMines)
        {
            yield break;
        }

        /*
         * The request is already accepted and the board is now owned by this
         * mutation. For animation-timed abilities, wait for the impact frame,
         * but never allow a missing/interrupted Animation Event to own global
         * board input forever.
         */
        float animationWaitStartedAt =
            Time.realtimeSinceStartup;

        while (request.WaitForAnimationImpact &&
               !request.AnimationImpactReached)
        {
            if (request.OwnerActor == null ||
                request.OwnerActor.IsDefeated)
            {
                yield break;
            }

            if (Time.realtimeSinceStartup -
                animationWaitStartedAt >=
                AnimationImpactFailsafeSeconds)
            {
                Debug.LogWarning(
                    $"{request.OwnerActor.name} waited " +
                    $"{AnimationImpactFailsafeSeconds:0.##}s for its " +
                    "AbilityImpact Animation Event. Forcing the gameplay " +
                    "impact so the board cannot remain soft-locked.",
                    request.OwnerActor
                );

                request.AnimationImpactReached = true;
                request.OwnerActor
                    .EndSpecialAbilityAnimationAction();
                break;
            }

            yield return null;
        }

        List<Vector2Int> candidates =
            BuildMineableCellList();

        if (candidates.Count == 0)
        {
            yield break;
        }

        Vector2Int selectedCell =
            candidates[
                UnityEngine.Random.Range(
                    0,
                    candidates.Count
                )
            ];

        Gem minedGem =
            GetGem(
                selectedCell.x,
                selectedCell.y
            );

        if (minedGem == null ||
            IsGemPinned(minedGem))
        {
            yield break;
        }

        /*
         * Reserve ownership before any asynchronous presentation starts.
         * A second Miner processed after this one therefore cannot select
         * the same cell even when both became ready on the same player move.
         */
        minedCellOwners[selectedCell] =
            request.OwnerInstanceId;

        EnsureMiningVFX();

        CellMiningStarted?.Invoke(
            selectedCell.x,
            selectedCell.y,
            MiningTileFlashDuration
        );

        /*
         * Reuse the existing shard presentation without calling any gameplay
         * clear reporter. The burst is delayed by the same flash lead used by
         * ordinary matches, so it coincides with the mined gem disappearing.
         */
        GemMatchVFXRequested?.Invoke(
            new GemMatchVFXContext(
                minedGem.Type,
                1,
                0,
                new[]
                {
                    minedGem.transform.position
                },
                matchFlashDuration
            )
        );

        /*
         * Deliberately bypass every BoardClearResolved/combat/reward reporter.
         * The gem shatters visually, but grants no damage, healing, energy,
         * poison, Royal Decree hit, or other per-gem reward. A special gem is
         * also destroyed directly here and therefore never activates.
         */
        HashSet<Gem> minedGemOnly =
            new HashSet<Gem>
            {
                minedGem
            };

        yield return ClearMatches(
            minedGemOnly,
            null
        );

        yield return
            ResolveEnvironmentalBoardChange();
    }

    private List<Vector2Int>
        BuildMineableCellList()
    {
        List<Vector2Int> candidates =
            new List<Vector2Int>();

        for (int row = 0;
             row < height;
             row++)
        {
            for (int column = 0;
                 column < width;
                 column++)
            {
                if (!IsCellPlayable(
                        column,
                        row))
                {
                    continue;
                }

                Gem gem =
                    GetGem(
                        column,
                        row
                    );

                if (gem == null ||
                    IsGemPinned(gem))
                {
                    continue;
                }

                candidates.Add(
                    new Vector2Int(
                        column,
                        row
                    )
                );
            }
        }

        return candidates;
    }

    private IEnumerator ExecuteRestoreRequest(
        int ownerInstanceId)
    {
        List<Vector2Int> cellsToRestore =
            new List<Vector2Int>();

        foreach (
            KeyValuePair<Vector2Int, int> entry
            in minedCellOwners)
        {
            if (entry.Value ==
                ownerInstanceId)
            {
                cellsToRestore.Add(
                    entry.Key
                );
            }
        }

        if (cellsToRestore.Count == 0)
        {
            yield break;
        }

        foreach (Vector2Int cell
                 in cellsToRestore)
        {
            minedCellOwners.Remove(cell);

            CellRestored?.Invoke(
                cell.x,
                cell.y
            );
        }

        yield return
            ResolveEnvironmentalBoardChange();
    }

    private IEnumerator
        ResolveEnvironmentalBoardChange()
    {
        yield return CollapseAndRefillBoard();

        float settlePause =
            GetPostFallSettlePause();

        if (settlePause > 0f)
        {
            yield return new WaitForSeconds(
                settlePause
            );
        }

        HashSet<Gem> resultingMatches =
            FindAllMatches();

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
        if (GetComponent<BoardMiningVFX>() ==
            null)
        {
            gameObject.AddComponent<
                BoardMiningVFX
            >();
        }
    }
}
