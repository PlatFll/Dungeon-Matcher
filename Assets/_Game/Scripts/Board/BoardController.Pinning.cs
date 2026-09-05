using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BoardController
{
    private readonly HashSet<Gem> movablePinnedGems = new HashSet<Gem>();

    private bool IsGemFixedByPin(Gem gem) => IsGemPinned(gem) && !movablePinnedGems.Contains(gem);

    public void ReleaseMovablePinOnReplacement(Gem gem)
    {
        if (movablePinnedGems.Contains(gem)) ReleasePinInternal(gem);
    }

    public bool TryQueueTopUpMovablePins(EnemyActor owner, int cap,
        System.Action<bool> completed, System.Func<bool> cancelled)
    {
        if (owner == null || owner.IsDefeated || !owner.IsInitialized || gems == null) return false;
        pendingBoardMutations.Enqueue(new BoardMutationRequest
        {
            Kind = BoardMutationKind.TopUpMovablePins, OwnerActor = owner,
            OwnerInstanceId = owner.GetInstanceID(), MaximumOwnedPins = Mathf.Clamp(cap, 1, 3),
            Completed = completed, IsCancelled = cancelled, MovablePin = true
        });
        TryStartBoardMutationProcessor();
        return true;
    }

    private IEnumerator ExecuteTopUpMovablePins(BoardMutationRequest request)
    {
        while (request.OwnerActor != null && !request.OwnerActor.IsDefeated &&
            (request.IsCancelled == null || !request.IsCancelled()) &&
            GetPinnedGemCountForOwner(request.OwnerInstanceId) < request.MaximumOwnedPins)
        {
            Gem target = null;
            foreach (Gem candidate in BuildSafePinnableGemList())
                if (IsOrdinaryGemOnBoard(candidate)) { target = candidate; break; }
            if (target == null) yield break;
            request.TargetGem = target;
            yield return ExecutePinRequest(request);
            if (!pinnedGemOwners.ContainsKey(target)) yield break;
            request.Succeeded = true;
        }
    }
    [Header("Crossbow Guard Pin")]

    [SerializeField]
    [Tooltip(
        "Global bolt/pin overlay drawn above any gem hit by Bolt Shot."
    )]
    private Sprite pinnedGemOverlaySprite;

    [SerializeField, Range(0.15f, 1f)]
    [Tooltip(
        "Brightness multiplier applied to a gem while it is pinned."
    )]
    private float pinnedGemBrightness = 0.55f;

    [SerializeField, Min(0.02f)]
    private float pinMaterializeDuration = 0.10f;

    [SerializeField, Range(0f, 0.15f)]
    [Tooltip(
        "Horizontal impact shake measured in board-cell widths."
    )]
    private float pinShakeDistanceInCells = 0.04f;

    [SerializeField, Min(0.02f)]
    private float pinShakeDuration = 0.12f;

    /*
     * Pins belong to the Gem object instead of a board coordinate. The gem is
     * deliberately kept in the normal match grid so it can still complete a
     * match. Gravity treats the pinned Gem as a fixed island and lets every
     * other gem in the column move past its height into lower empty cells.
     */
    private readonly Dictionary<Gem, int>
        pinnedGemOwners =
            new Dictionary<Gem, int>();

    /*
     * A Bolt Shot chooses its target before entering the serialized mutation
     * queue. This reservation prevents multiple guards (or a Miner processed
     * later on the same player turn) from racing for the same Gem.
     */
    private readonly Dictionary<Gem, int>
        pendingPinTargetOwners =
            new Dictionary<Gem, int>();

    private readonly HashSet<int>
        pendingPinReleaseOwners =
            new HashSet<int>();

    public bool IsGemPinned(
        Gem gem)
    {
        return gem != null &&
               (
                   pinnedGemOwners.ContainsKey(gem) ||
                   pendingPinTargetOwners.ContainsKey(gem)
               );
    }

    public void CancelPointerInteraction(
        Gem gem)
    {
        if (pointerStartGem == gem)
        {
            pointerStartGem = null;
        }

        if (selectedGem == gem)
        {
            ClearSelection();
        }
    }

    private bool IsCellPinned(
        int column,
        int row)
    {
        Gem gem =
            GetGem(
                column,
                row
            );

        return IsGemPinned(gem);
    }

    private bool IsSwapBlockedByPin(
        int firstColumn,
        int firstRow,
        int secondColumn,
        int secondRow)
    {
        return
            IsCellPinned(
                firstColumn,
                firstRow
            ) ||
            IsCellPinned(
                secondColumn,
                secondRow
            );
    }

    public int GetPinnedGemCountForOwner(
        int ownerInstanceId)
    {
        if (ownerInstanceId == 0)
        {
            return 0;
        }

        CleanupDestroyedPinEntries();

        int count = 0;

        foreach (
            KeyValuePair<Gem, int> entry
            in pinnedGemOwners)
        {
            if (entry.Value ==
                ownerInstanceId)
            {
                count++;
            }
        }

        foreach (
            KeyValuePair<Gem, int> entry
            in pendingPinTargetOwners)
        {
            if (entry.Value ==
                ownerInstanceId)
            {
                count++;
            }
        }

        return count;
    }

    public bool TryQueuePinRandomGem(
        EnemyActor owner,
        int maximumOwnedPins)
    {
        if (owner == null ||
            owner.IsDefeated ||
            !owner.IsInitialized ||
            gems == null)
        {
            return false;
        }

        /*
         * Do not select a target against a board that is already being
         * structurally changed by mining/restoration/pin release. Leaving the
         * enemy ready lets it retry on the next valid player turn. Multiple
         * Bolt Shots themselves are safe because their targets are reserved.
         */
        if (HasStructuralBoardMutationInFlight())
        {
            return false;
        }

        int ownerInstanceId =
            owner.GetInstanceID();

        int safeMaximum =
            Mathf.Max(
                1,
                maximumOwnedPins
            );

        if (GetPinnedGemCountForOwner(
                ownerInstanceId) >=
            safeMaximum)
        {
            return false;
        }

        List<Gem> candidates =
            BuildSafePinnableGemList();

        if (candidates.Count == 0)
        {
            return false;
        }

        Gem selectedGem =
            candidates[
                Random.Range(
                    0,
                    candidates.Count
                )
            ];

        if (selectedGem == null ||
            IsGemPinned(selectedGem))
        {
            return false;
        }

        pendingPinTargetOwners[selectedGem] =
            ownerInstanceId;

        pendingBoardMutations.Enqueue(
            new BoardMutationRequest
            {
                Kind =
                    BoardMutationKind
                        .PinRandomGem,

                OwnerActor = owner,
                OwnerInstanceId =
                    ownerInstanceId,

                MaximumOwnedPins =
                    safeMaximum,

                TargetGem =
                    selectedGem
            }
        );

        TryStartBoardMutationProcessor();
        return true;
    }

    public void QueueReleasePinnedGems(
        int ownerInstanceId)
    {
        if (ownerInstanceId == 0 ||
            GetPinnedGemCountForOwner(
                ownerInstanceId) <= 0 ||
            !pendingPinReleaseOwners.Add(
                ownerInstanceId))
        {
            return;
        }

        pendingBoardMutations.Enqueue(
            new BoardMutationRequest
            {
                Kind =
                    BoardMutationKind
                        .ReleaseOwnerPins,

                OwnerInstanceId =
                    ownerInstanceId
            }
        );

        TryStartBoardMutationProcessor();
    }

    private IEnumerator ExecutePinRequest(
        BoardMutationRequest request)
    {
        if (request == null)
        {
            yield break;
        }

        Gem selectedGem =
            request.TargetGem;

        if (selectedGem != null)
        {
            pendingPinTargetOwners.Remove(
                selectedGem
            );
        }

        if (request.OwnerActor == null ||
            request.OwnerActor.IsDefeated ||
            request.OwnerInstanceId == 0 ||
            selectedGem == null)
        {
            yield break;
        }

        if (GetPinnedGemCountForOwner(
                request.OwnerInstanceId) >=
            request.MaximumOwnedPins)
        {
            yield break;
        }

        if (!IsCellPlayable(
                selectedGem.Column,
                selectedGem.Row) ||
            GetGem(
                selectedGem.Column,
                selectedGem.Row) !=
                selectedGem ||
            pinnedGemOwners.ContainsKey(
                selectedGem))
        {
            yield break;
        }

        /*
         * Ownership becomes authoritative before presentation starts. A later
         * board mutation therefore sees this gem as fixed immediately.
         */
        pinnedGemOwners[selectedGem] =
            request.OwnerInstanceId;
        if (request.MovablePin) movablePinnedGems.Add(selectedGem);

        /*
         * Defensive recheck. With structural mutations gated before target
         * reservation this should remain true, but never keep a pin that would
         * eliminate the final legal move if another system changed the board.
         */
        if (!HasAvailableMove())
        {
            movablePinnedGems.Remove(selectedGem);
            pinnedGemOwners.Remove(
                selectedGem
            );

            yield break;
        }

        PinnedGemOverlayView view =
            selectedGem.GetComponent<
                PinnedGemOverlayView
            >();

        if (view == null)
        {
            view =
                selectedGem.gameObject.AddComponent<
                    PinnedGemOverlayView
                >();
        }

        if (pinnedGemOverlaySprite == null)
        {
            Debug.LogWarning(
                "Crossbow Guard pinned a gem, but BoardController's " +
                "Pinned Gem Overlay Sprite is not assigned. Gameplay pinning " +
                "will still work; assign the overlay sprite in Unity.",
                this
            );
        }

        view.Initialize(
            selectedGem,
            this,
            request.OwnerInstanceId,
            pinnedGemOverlaySprite,
            pinnedGemBrightness,
            pinMaterializeDuration,
            pinShakeDistanceInCells *
                cellSize,
            pinShakeDuration
        );

        yield return
            view.PlayBoltImpact();
    }

    private IEnumerator ExecuteReleasePinsRequest(
        int ownerInstanceId)
    {
        RemovePendingPinReservationsForOwner(
            ownerInstanceId
        );

        List<Gem> pinsToRelease =
            new List<Gem>();

        foreach (
            KeyValuePair<Gem, int> entry
            in pinnedGemOwners)
        {
            if (entry.Key != null &&
                entry.Value ==
                    ownerInstanceId)
            {
                pinsToRelease.Add(
                    entry.Key
                );
            }
        }

        foreach (Gem pinnedGem
                 in pinsToRelease)
        {
            ReleasePinInternal(
                pinnedGem
            );
        }

        if (pinsToRelease.Count > 0)
        {
            /*
             * A released gem may have been suspended above empty space while
             * pinned. Let gravity settle the column, then resolve any cascade.
             */
            yield return
                ResolveEnvironmentalBoardChange();
        }
    }

    private bool HasStructuralBoardMutationInFlight()
    {
        if (activeBoardMutationKind.HasValue &&
            activeBoardMutationKind.Value !=
                BoardMutationKind.PinRandomGem)
        {
            return true;
        }

        foreach (
            BoardMutationRequest request
            in pendingBoardMutations)
        {
            if (request != null &&
                request.Kind !=
                    BoardMutationKind.PinRandomGem)
            {
                return true;
            }
        }

        return false;
    }

    private List<Gem>
        BuildSafePinnableGemList()
    {
        CleanupDestroyedPinEntries();

        List<Gem> candidates =
            new List<Gem>();

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

                Gem candidate =
                    GetGem(
                        column,
                        row
                    );

                if (candidate == null ||
                    IsGemPinned(candidate))
                {
                    continue;
                }

                /*
                 * Never fire a bolt that immediately removes the player's
                 * final legal move. Temporarily reserve this candidate, ask the
                 * normal move detector, then undo the simulation.
                 */
                pinnedGemOwners[candidate] =
                    int.MinValue;

                bool leavesPlayableMove =
                    HasAvailableMove();

                pinnedGemOwners.Remove(
                    candidate
                );

                if (leavesPlayableMove)
                {
                    candidates.Add(
                        candidate
                    );
                }
            }
        }

        return candidates;
    }

    /*
     * Called once for each real match before its clear presentation. A match
     * breaks a bolt when the pinned Gem is in that match or one orthogonal
     * cell away. Diagonal matches do not break the bolt.
     */
    private void BreakPinsAdjacentToMatches(
        HashSet<Gem> matches)
    {
        if (matches == null ||
            matches.Count == 0 ||
            pinnedGemOwners.Count == 0)
        {
            return;
        }

        List<Gem> pinsToRelease =
            new List<Gem>();

        foreach (
            KeyValuePair<Gem, int> pin
            in pinnedGemOwners)
        {
            Gem pinnedGem =
                pin.Key;

            if (pinnedGem == null)
            {
                continue;
            }

            foreach (Gem matchedGem
                     in matches)
            {
                if (matchedGem == null)
                {
                    continue;
                }

                int distance =
                    Mathf.Abs(
                        pinnedGem.Column -
                        matchedGem.Column
                    ) +
                    Mathf.Abs(
                        pinnedGem.Row -
                        matchedGem.Row
                    );

                if (distance <= 1 && !movablePinnedGems.Contains(pinnedGem))
                {
                    pinsToRelease.Add(
                        pinnedGem
                    );

                    break;
                }
            }
        }

        foreach (Gem pinnedGem
                 in pinsToRelease)
        {
            ReleasePinInternal(
                pinnedGem
            );
        }
    }

    public void NotifyPinnedGemDestroyed(
        Gem destroyedGem,
        int expectedOwnerInstanceId)
    {
        if (destroyedGem == null)
        {
            CleanupDestroyedPinEntries();
            return;
        }

        pendingPinTargetOwners.Remove(
            destroyedGem
        );

        if (!pinnedGemOwners.TryGetValue(
                destroyedGem,
                out int actualOwner))
        {
            return;
        }

        if (expectedOwnerInstanceId != 0 &&
            actualOwner !=
                expectedOwnerInstanceId)
        {
            return;
        }

        pinnedGemOwners.Remove(
            destroyedGem
        );
    }

    private void ReleasePinInternal(
        Gem pinnedGem)
    {
        movablePinnedGems.Remove(pinnedGem);
        if (pinnedGem == null ||
            !pinnedGemOwners.Remove(
                pinnedGem))
        {
            return;
        }

        PinnedGemOverlayView view =
            pinnedGem.GetComponent<
                PinnedGemOverlayView
            >();

        if (view != null)
        {
            view.ReleaseVisual();
            Destroy(view);
        }
    }

    private void RemovePendingPinReservationsForOwner(
        int ownerInstanceId)
    {
        List<Gem> reservationsToRemove =
            new List<Gem>();

        foreach (
            KeyValuePair<Gem, int> entry
            in pendingPinTargetOwners)
        {
            if (entry.Value ==
                ownerInstanceId)
            {
                reservationsToRemove.Add(
                    entry.Key
                );
            }
        }

        foreach (Gem reservedGem
                 in reservationsToRemove)
        {
            pendingPinTargetOwners.Remove(
                reservedGem
            );
        }
    }

    /*
     * Emergency anti-softlock policy: reshuffling a board breaks every active
     * bolt first. A reshuffle is already a global board correction, and moving
     * a supposedly pinned gem during that correction would violate the pin's
     * visual/gameplay contract.
     */
    private void ReleaseAllPinsForEmergencyReshuffle()
    {
        pendingPinTargetOwners.Clear();

        if (pinnedGemOwners.Count == 0)
        {
            return;
        }

        List<Gem> allPinnedGems =
            new List<Gem>(
                pinnedGemOwners.Keys
            );

        foreach (Gem pinnedGem
                 in allPinnedGems)
        {
            if (pinnedGem != null)
            {
                ReleasePinInternal(
                    pinnedGem
                );
            }
        }

        CleanupDestroyedPinEntries();
    }

    private void CleanupDestroyedPinEntries()
    {
        movablePinnedGems.RemoveWhere(gem => gem == null || !pinnedGemOwners.ContainsKey(gem));
        if (pinnedGemOwners.Count > 0)
        {
            List<Gem> destroyedKeys =
                null;

            foreach (Gem gem
                     in pinnedGemOwners.Keys)
            {
                if (gem != null)
                {
                    continue;
                }

                if (destroyedKeys == null)
                {
                    destroyedKeys =
                        new List<Gem>();
                }

                destroyedKeys.Add(gem);
            }

            if (destroyedKeys != null)
            {
                foreach (Gem destroyedGem
                         in destroyedKeys)
                {
                    pinnedGemOwners.Remove(
                        destroyedGem
                    );
                }
            }
        }

        if (pendingPinTargetOwners.Count > 0)
        {
            List<Gem> destroyedReservations =
                null;

            foreach (Gem gem
                     in pendingPinTargetOwners.Keys)
            {
                if (gem != null)
                {
                    continue;
                }

                if (destroyedReservations == null)
                {
                    destroyedReservations =
                        new List<Gem>();
                }

                destroyedReservations.Add(gem);
            }

            if (destroyedReservations != null)
            {
                foreach (Gem destroyedGem
                         in destroyedReservations)
                {
                    pendingPinTargetOwners.Remove(
                        destroyedGem
                    );
                }
            }
        }
    }
}
