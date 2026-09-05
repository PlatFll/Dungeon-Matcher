using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BoardController
{
    private readonly HashSet<Gem> movablePinnedGems =
        new HashSet<Gem>();

    /*
     * Frozen gems deliberately share the authoritative pin ownership/grid-lock
     * infrastructure: both block manual swaps, while only movable Captain
     * chains participate in gravity. The separate set preserves the Court
     * Mage's distinct break/visual rules.
     */
    private readonly HashSet<Gem> frozenPinnedGems =
        new HashSet<Gem>();

    private readonly HashSet<Gem> pendingFrozenPinTargets =
        new HashSet<Gem>();

    private bool IsGemFixedByPin(Gem gem) =>
        IsGemPinned(gem) &&
        !movablePinnedGems.Contains(gem);

    public bool IsGemFrozen(Gem gem) =>
        gem != null &&
        (frozenPinnedGems.Contains(gem) ||
         pendingFrozenPinTargets.Contains(gem));

    public void ReleaseMovablePinOnReplacement(Gem gem)
    {
        if (movablePinnedGems.Contains(gem) ||
            frozenPinnedGems.Contains(gem))
        {
            ReleasePinInternal(gem);
        }
    }

    public bool TryQueueTopUpMovablePins(
        EnemyActor owner,
        int cap,
        System.Action<bool> completed,
        System.Func<bool> cancelled)
    {
        if (owner == null ||
            owner.IsDefeated ||
            !owner.IsInitialized ||
            gems == null)
        {
            return false;
        }

        pendingBoardMutations.Enqueue(
            new BoardMutationRequest
            {
                Kind = BoardMutationKind.TopUpMovablePins,
                OwnerActor = owner,
                OwnerInstanceId = owner.GetInstanceID(),
                MaximumOwnedPins = Mathf.Clamp(cap, 1, 3),
                Completed = completed,
                IsCancelled = cancelled,
                MovablePin = true
            }
        );

        TryStartBoardMutationProcessor();
        return true;
    }

    private IEnumerator ExecuteTopUpMovablePins(
        BoardMutationRequest request)
    {
        while (request.OwnerActor != null &&
               !request.OwnerActor.IsDefeated &&
               (request.IsCancelled == null ||
                !request.IsCancelled()) &&
               GetPinnedGemCountForOwner(
                   request.OwnerInstanceId) <
               request.MaximumOwnedPins)
        {
            Gem target = null;

            foreach (Gem candidate
                     in BuildSafePinnableGemList())
            {
                if (IsOrdinaryGemOnBoard(candidate))
                {
                    target = candidate;
                    break;
                }
            }

            if (target == null)
            {
                yield break;
            }

            request.TargetGem = target;
            yield return ExecutePinRequest(request);

            if (!pinnedGemOwners.ContainsKey(target))
            {
                yield break;
            }

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

    [Header("Court Mage Freeze")]

    [SerializeField]
    [Tooltip(
        "Overlay drawn above a frozen gem. Assign the final ice artwork here. " +
        "Frozen gameplay works even while this field is empty."
    )]
    private Sprite frozenGemOverlaySprite;

    /*
     * Pins belong to Gem objects instead of board coordinates. Fixed pins and
     * freezes stay at their exact coordinate while gravity compacts every other
     * gem through that height into lower available rows. Movable Captain chains
     * remain attached to their gem while that gem falls normally.
     */
    private readonly Dictionary<Gem, int> pinnedGemOwners =
        new Dictionary<Gem, int>();

    private readonly Dictionary<Gem, int> pendingPinTargetOwners =
        new Dictionary<Gem, int>();

    private readonly HashSet<int> pendingPinReleaseOwners =
        new HashSet<int>();

    public bool IsGemPinned(Gem gem)
    {
        return gem != null &&
               (pinnedGemOwners.ContainsKey(gem) ||
                pendingPinTargetOwners.ContainsKey(gem));
    }

    public void CancelPointerInteraction(Gem gem)
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

    private bool IsCellPinned(int column, int row)
    {
        return IsGemPinned(GetGem(column, row));
    }

    private bool IsSwapBlockedByPin(
        int firstColumn,
        int firstRow,
        int secondColumn,
        int secondRow)
    {
        return IsCellPinned(firstColumn, firstRow) ||
               IsCellPinned(secondColumn, secondRow);
    }

    public int GetPinnedGemCountForOwner(int ownerInstanceId)
    {
        if (ownerInstanceId == 0)
        {
            return 0;
        }

        CleanupDestroyedPinEntries();
        int count = 0;

        foreach (KeyValuePair<Gem, int> entry
                 in pinnedGemOwners)
        {
            if (entry.Value == ownerInstanceId)
            {
                count++;
            }
        }

        foreach (KeyValuePair<Gem, int> entry
                 in pendingPinTargetOwners)
        {
            if (entry.Value == ownerInstanceId)
            {
                count++;
            }
        }

        return count;
    }

    public int GetFrozenGemCountForOwner(int ownerInstanceId)
    {
        if (ownerInstanceId == 0)
        {
            return 0;
        }

        CleanupDestroyedPinEntries();
        int count = 0;

        foreach (Gem gem in frozenPinnedGems)
        {
            if (gem != null &&
                pinnedGemOwners.TryGetValue(
                    gem,
                    out int owner) &&
                owner == ownerInstanceId)
            {
                count++;
            }
        }

        foreach (Gem gem in pendingFrozenPinTargets)
        {
            if (gem != null &&
                pendingPinTargetOwners.TryGetValue(
                    gem,
                    out int owner) &&
                owner == ownerInstanceId)
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
        if (!CanQueuePin(owner, maximumOwnedPins,
                out int ownerInstanceId,
                out int safeMaximum))
        {
            return false;
        }

        List<Gem> candidates = BuildSafePinnableGemList();

        if (candidates.Count == 0)
        {
            return false;
        }

        Gem selectedGem =
            candidates[Random.Range(0, candidates.Count)];

        return QueueReservedPin(
            owner,
            ownerInstanceId,
            safeMaximum,
            selectedGem,
            frozen: false
        );
    }

    public bool TryQueueFreezeRandomGem(
        EnemyActor owner,
        int maximumOwnedFrozenGems)
    {
        if (!CanQueuePin(owner, maximumOwnedFrozenGems,
                out int ownerInstanceId,
                out int safeMaximum) ||
            GetFrozenGemCountForOwner(ownerInstanceId) >= safeMaximum)
        {
            return false;
        }

        List<Gem> safePins = BuildSafePinnableGemList();
        List<Gem> candidates = new List<Gem>();

        foreach (Gem gem in safePins)
        {
            if (gem != null &&
                gem.SpecialType == GemSpecialType.None &&
                !IsGemFrozen(gem))
            {
                candidates.Add(gem);
            }
        }

        if (candidates.Count == 0)
        {
            return false;
        }

        Gem selectedGem =
            candidates[Random.Range(0, candidates.Count)];

        return QueueReservedPin(
            owner,
            ownerInstanceId,
            safeMaximum,
            selectedGem,
            frozen: true
        );
    }

    private bool CanQueuePin(
        EnemyActor owner,
        int maximumOwnedPins,
        out int ownerInstanceId,
        out int safeMaximum)
    {
        ownerInstanceId = 0;
        safeMaximum = Mathf.Max(1, maximumOwnedPins);

        if (owner == null ||
            owner.IsDefeated ||
            !owner.IsInitialized ||
            gems == null ||
            HasStructuralBoardMutationInFlight())
        {
            return false;
        }

        ownerInstanceId = owner.GetInstanceID();

        return GetPinnedGemCountForOwner(ownerInstanceId) <
               safeMaximum;
    }

    private bool QueueReservedPin(
        EnemyActor owner,
        int ownerInstanceId,
        int safeMaximum,
        Gem selectedGem,
        bool frozen)
    {
        if (selectedGem == null ||
            IsGemPinned(selectedGem))
        {
            return false;
        }

        pendingPinTargetOwners[selectedGem] = ownerInstanceId;

        if (frozen)
        {
            pendingFrozenPinTargets.Add(selectedGem);
        }

        pendingBoardMutations.Enqueue(
            new BoardMutationRequest
            {
                Kind = BoardMutationKind.PinRandomGem,
                OwnerActor = owner,
                OwnerInstanceId = ownerInstanceId,
                MaximumOwnedPins = safeMaximum,
                TargetGem = selectedGem
            }
        );

        TryStartBoardMutationProcessor();
        return true;
    }

    public void QueueReleasePinnedGems(int ownerInstanceId)
    {
        if (ownerInstanceId == 0 ||
            GetPinnedGemCountForOwner(ownerInstanceId) <= 0 ||
            !pendingPinReleaseOwners.Add(ownerInstanceId))
        {
            return;
        }

        pendingBoardMutations.Enqueue(
            new BoardMutationRequest
            {
                Kind = BoardMutationKind.ReleaseOwnerPins,
                OwnerInstanceId = ownerInstanceId
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

        Gem selectedGem = request.TargetGem;
        bool frozenPin =
            selectedGem != null &&
            pendingFrozenPinTargets.Remove(selectedGem);

        if (selectedGem != null)
        {
            pendingPinTargetOwners.Remove(selectedGem);
        }

        if (request.OwnerActor == null ||
            request.OwnerActor.IsDefeated ||
            request.OwnerInstanceId == 0 ||
            selectedGem == null)
        {
            yield break;
        }

        int currentOwnedCount =
            frozenPin
                ? GetFrozenGemCountForOwner(request.OwnerInstanceId)
                : GetPinnedGemCountForOwner(request.OwnerInstanceId);

        if (currentOwnedCount >= request.MaximumOwnedPins)
        {
            yield break;
        }

        if (!IsCellPlayable(
                selectedGem.Column,
                selectedGem.Row) ||
            GetGem(
                selectedGem.Column,
                selectedGem.Row) != selectedGem ||
            pinnedGemOwners.ContainsKey(selectedGem) ||
            (frozenPin &&
             selectedGem.SpecialType != GemSpecialType.None))
        {
            yield break;
        }

        pinnedGemOwners[selectedGem] = request.OwnerInstanceId;

        if (frozenPin)
        {
            frozenPinnedGems.Add(selectedGem);
        }
        else if (request.MovablePin)
        {
            movablePinnedGems.Add(selectedGem);
        }

        if (!HasAvailableMove())
        {
            movablePinnedGems.Remove(selectedGem);
            frozenPinnedGems.Remove(selectedGem);
            pinnedGemOwners.Remove(selectedGem);
            yield break;
        }

        if (frozenPin)
        {
            FrozenGemOverlayView frozenView =
                selectedGem.GetComponent<FrozenGemOverlayView>();

            if (frozenView == null)
            {
                frozenView =
                    selectedGem.gameObject.AddComponent<
                        FrozenGemOverlayView>();
            }

            if (frozenGemOverlaySprite == null)
            {
                Debug.LogWarning(
                    "Court Mage froze a gem, but BoardController's Frozen " +
                    "Gem Overlay Sprite is not assigned. Freeze gameplay still " +
                    "works; assign the overlay sprite in the Inspector.",
                    this
                );
            }

            frozenView.Initialize(frozenGemOverlaySprite);
            request.Succeeded = true;
            yield break;
        }

        PinnedGemOverlayView view =
            selectedGem.GetComponent<PinnedGemOverlayView>();

        if (view == null)
        {
            view =
                selectedGem.gameObject.AddComponent<
                    PinnedGemOverlayView>();
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
            pinShakeDistanceInCells * cellSize,
            pinShakeDuration
        );

        yield return view.PlayBoltImpact();
        request.Succeeded = true;
    }

    private IEnumerator ExecuteReleasePinsRequest(
        int ownerInstanceId)
    {
        RemovePendingPinReservationsForOwner(ownerInstanceId);

        List<Gem> pinsToRelease = new List<Gem>();

        foreach (KeyValuePair<Gem, int> entry
                 in pinnedGemOwners)
        {
            if (entry.Key != null &&
                entry.Value == ownerInstanceId)
            {
                pinsToRelease.Add(entry.Key);
            }
        }

        foreach (Gem pinnedGem in pinsToRelease)
        {
            ReleasePinInternal(pinnedGem);
        }

        if (pinsToRelease.Count > 0)
        {
            yield return ResolveEnvironmentalBoardChange();
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

        foreach (BoardMutationRequest request
                 in pendingBoardMutations)
        {
            if (request != null &&
                request.Kind != BoardMutationKind.PinRandomGem)
            {
                return true;
            }
        }

        return false;
    }

    private List<Gem> BuildSafePinnableGemList()
    {
        CleanupDestroyedPinEntries();
        List<Gem> candidates = new List<Gem>();

        for (int row = 0; row < height; row++)
        {
            for (int column = 0; column < width; column++)
            {
                if (!IsCellPlayable(column, row))
                {
                    continue;
                }

                Gem candidate = GetGem(column, row);

                if (candidate == null ||
                    IsGemPinned(candidate))
                {
                    continue;
                }

                pinnedGemOwners[candidate] = int.MinValue;
                bool leavesPlayableMove = HasAvailableMove();
                pinnedGemOwners.Remove(candidate);

                if (leavesPlayableMove)
                {
                    candidates.Add(candidate);
                }
            }
        }

        return candidates;
    }

    private void BreakPinsAdjacentToMatches(
        HashSet<Gem> matches)
    {
        if (matches == null ||
            matches.Count == 0 ||
            pinnedGemOwners.Count == 0)
        {
            return;
        }

        List<Gem> pinsToRelease = new List<Gem>();

        foreach (KeyValuePair<Gem, int> pin
                 in pinnedGemOwners)
        {
            Gem pinnedGem = pin.Key;

            if (pinnedGem == null ||
                movablePinnedGems.Contains(pinnedGem) ||
                frozenPinnedGems.Contains(pinnedGem))
            {
                continue;
            }

            foreach (Gem matchedGem in matches)
            {
                if (matchedGem == null)
                {
                    continue;
                }

                int distance =
                    Mathf.Abs(pinnedGem.Column - matchedGem.Column) +
                    Mathf.Abs(pinnedGem.Row - matchedGem.Row);

                if (distance <= 1)
                {
                    pinsToRelease.Add(pinnedGem);
                    break;
                }
            }
        }

        foreach (Gem pinnedGem in pinsToRelease)
        {
            ReleasePinInternal(pinnedGem);
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

        pendingPinTargetOwners.Remove(destroyedGem);
        pendingFrozenPinTargets.Remove(destroyedGem);

        if (!pinnedGemOwners.TryGetValue(
                destroyedGem,
                out int actualOwner))
        {
            frozenPinnedGems.Remove(destroyedGem);
            movablePinnedGems.Remove(destroyedGem);
            return;
        }

        if (expectedOwnerInstanceId != 0 &&
            actualOwner != expectedOwnerInstanceId)
        {
            return;
        }

        pinnedGemOwners.Remove(destroyedGem);
        frozenPinnedGems.Remove(destroyedGem);
        movablePinnedGems.Remove(destroyedGem);
    }

    private void ReleasePinInternal(Gem pinnedGem)
    {
        bool wasFrozen = frozenPinnedGems.Remove(pinnedGem);
        movablePinnedGems.Remove(pinnedGem);
        pendingFrozenPinTargets.Remove(pinnedGem);

        if (pinnedGem == null ||
            !pinnedGemOwners.Remove(pinnedGem))
        {
            return;
        }

        if (wasFrozen)
        {
            FrozenGemOverlayView frozenView =
                pinnedGem.GetComponent<FrozenGemOverlayView>();

            if (frozenView != null)
            {
                frozenView.ReleaseVisual();
                Destroy(frozenView);
            }

            return;
        }

        PinnedGemOverlayView view =
            pinnedGem.GetComponent<PinnedGemOverlayView>();

        if (view != null)
        {
            view.ReleaseVisual();
            Destroy(view);
        }
    }

    private void RemovePendingPinReservationsForOwner(
        int ownerInstanceId)
    {
        List<Gem> reservationsToRemove = new List<Gem>();

        foreach (KeyValuePair<Gem, int> entry
                 in pendingPinTargetOwners)
        {
            if (entry.Value == ownerInstanceId)
            {
                reservationsToRemove.Add(entry.Key);
            }
        }

        foreach (Gem reservedGem in reservationsToRemove)
        {
            pendingPinTargetOwners.Remove(reservedGem);
            pendingFrozenPinTargets.Remove(reservedGem);
        }
    }

    private void ReleaseAllPinsForEmergencyReshuffle()
    {
        pendingPinTargetOwners.Clear();
        pendingFrozenPinTargets.Clear();

        if (pinnedGemOwners.Count == 0)
        {
            return;
        }

        List<Gem> allPinnedGems =
            new List<Gem>(pinnedGemOwners.Keys);

        foreach (Gem pinnedGem in allPinnedGems)
        {
            if (pinnedGem != null)
            {
                ReleasePinInternal(pinnedGem);
            }
        }

        CleanupDestroyedPinEntries();
    }

    private void CleanupDestroyedPinEntries()
    {
        movablePinnedGems.RemoveWhere(
            gem => gem == null ||
                   !pinnedGemOwners.ContainsKey(gem));

        frozenPinnedGems.RemoveWhere(
            gem => gem == null ||
                   !pinnedGemOwners.ContainsKey(gem));

        pendingFrozenPinTargets.RemoveWhere(
            gem => gem == null ||
                   !pendingPinTargetOwners.ContainsKey(gem));

        if (pinnedGemOwners.Count > 0)
        {
            List<Gem> destroyedKeys = null;

            foreach (Gem gem in pinnedGemOwners.Keys)
            {
                if (gem != null)
                {
                    continue;
                }

                destroyedKeys ??= new List<Gem>();
                destroyedKeys.Add(gem);
            }

            if (destroyedKeys != null)
            {
                foreach (Gem destroyedGem in destroyedKeys)
                {
                    pinnedGemOwners.Remove(destroyedGem);
                    movablePinnedGems.Remove(destroyedGem);
                    frozenPinnedGems.Remove(destroyedGem);
                }
            }
        }

        if (pendingPinTargetOwners.Count > 0)
        {
            List<Gem> destroyedReservations = null;

            foreach (Gem gem in pendingPinTargetOwners.Keys)
            {
                if (gem != null)
                {
                    continue;
                }

                destroyedReservations ??= new List<Gem>();
                destroyedReservations.Add(gem);
            }

            if (destroyedReservations != null)
            {
                foreach (Gem destroyedGem in destroyedReservations)
                {
                    pendingPinTargetOwners.Remove(destroyedGem);
                    pendingFrozenPinTargets.Remove(destroyedGem);
                }
            }
        }
    }
}
