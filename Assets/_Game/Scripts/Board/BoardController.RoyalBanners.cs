using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BoardController
{
    [Header("Royal Standard Bearer")]

    [SerializeField]
    [Tooltip(
        "Board artwork for the Royal Standard Bearer's falling banner. " +
        "Assign the final banner sprite here; gameplay still functions when empty."
    )]
    private Sprite royalBannerBoardSprite;

    [SerializeField, Range(0.5f, 1.25f)]
    [Tooltip("Banner size relative to one board cell.")]
    private float royalBannerBoardScale = 0.9f;

    private sealed class RoyalBannerState
    {
        public int BannerId;
        public int OwnerInstanceId;
        public Vector2Int Cell;
        public GameObject ViewObject;
        public SpriteRenderer Renderer;
        public Coroutine MoveRoutine;
        public bool ReachedBottom;
        public int PendingGravitySteps;
    }

    private readonly Dictionary<Vector2Int, RoyalBannerState>
        royalBannerCells =
            new Dictionary<Vector2Int, RoyalBannerState>();

    private int nextRoyalBannerId;

    public event Action<int> RoyalBannerRemoved;

    public bool IsCellRoyalBanner(
        int column,
        int row)
    {
        return royalBannerCells.ContainsKey(
            new Vector2Int(column, row)
        );
    }

    public int GetRoyalBannerCountForOwner(
        int ownerInstanceId)
    {
        if (ownerInstanceId == 0)
        {
            return 0;
        }

        int count = 0;

        foreach (RoyalBannerState state
                 in royalBannerCells.Values)
        {
            if (state != null &&
                state.OwnerInstanceId == ownerInstanceId &&
                !state.ReachedBottom)
            {
                count++;
            }
        }

        return count;
    }

    public int GetRoyalBannerIdForOwner(
        int ownerInstanceId)
    {
        if (ownerInstanceId == 0)
        {
            return 0;
        }

        foreach (RoyalBannerState state
                 in royalBannerCells.Values)
        {
            if (state != null &&
                state.OwnerInstanceId == ownerInstanceId &&
                !state.ReachedBottom)
            {
                return state.BannerId;
            }
        }

        return 0;
    }

    public bool TryQueuePlaceRoyalBanner(
        EnemyActor owner,
        Action<bool> completed = null)
    {
        if (owner == null ||
            owner.IsDefeated ||
            !owner.IsInitialized ||
            gems == null)
        {
            return false;
        }

        int ownerInstanceId = owner.GetInstanceID();

        if (GetRoyalBannerCountForOwner(ownerInstanceId) > 0 ||
            HasPendingRoyalBannerPlacement(ownerInstanceId))
        {
            return false;
        }

        if (BuildSafeTopRowBannerCells().Count == 0)
        {
            return false;
        }

        pendingBoardMutations.Enqueue(
            new BoardMutationRequest
            {
                Kind = BoardMutationKind.PlaceRoyalBanner,
                OwnerActor = owner,
                OwnerInstanceId = ownerInstanceId,
                Completed = completed
            }
        );

        TryStartBoardMutationProcessor();
        return true;
    }

    public void OrphanRoyalBannerForOwner(
        int ownerInstanceId)
    {
        if (ownerInstanceId == 0)
        {
            return;
        }

        foreach (RoyalBannerState state
                 in royalBannerCells.Values)
        {
            if (state != null &&
                state.OwnerInstanceId == ownerInstanceId)
            {
                /*
                 * The planted banner persists after its bearer dies. The
                 * separate BannerId keeps its aura lifecycle stable while zero
                 * prevents a recycled EnemyActor instance ID from owning it.
                 */
                state.OwnerInstanceId = 0;
            }
        }
    }

    private bool HasPendingRoyalBannerPlacement(
        int ownerInstanceId)
    {
        if (activeBoardMutationRequest != null &&
            activeBoardMutationRequest.Kind ==
                BoardMutationKind.PlaceRoyalBanner &&
            activeBoardMutationRequest.OwnerInstanceId ==
                ownerInstanceId)
        {
            return true;
        }

        foreach (BoardMutationRequest request
                 in pendingBoardMutations)
        {
            if (request != null &&
                request.Kind ==
                    BoardMutationKind.PlaceRoyalBanner &&
                request.OwnerInstanceId == ownerInstanceId)
            {
                return true;
            }
        }

        return false;
    }

    private IEnumerator ExecutePlaceRoyalBannerRequest(
        BoardMutationRequest request)
    {
        if (request == null ||
            request.OwnerActor == null ||
            request.OwnerActor.IsDefeated ||
            request.OwnerInstanceId == 0 ||
            GetRoyalBannerCountForOwner(
                request.OwnerInstanceId) > 0)
        {
            yield break;
        }

        List<Vector2Int> candidates =
            BuildSafeTopRowBannerCells();

        if (candidates.Count == 0)
        {
            yield break;
        }

        Vector2Int cell =
            candidates[UnityEngine.Random.Range(0, candidates.Count)];

        Gem replacedGem = GetGem(cell.x, cell.y);

        if (replacedGem == null ||
            replacedGem.SpecialType != GemSpecialType.None ||
            IsGemPinned(replacedGem))
        {
            yield break;
        }

        /*
         * The banner replaces one ordinary top-row gem as environmental board
         * manipulation. It is not a gem, grants no clear rewards, and cannot
         * later be matched or destroyed by special-gem effects.
         */
        yield return ClearMatches(
            new HashSet<Gem> { replacedGem },
            null
        );

        if (request.OwnerActor == null ||
            request.OwnerActor.IsDefeated)
        {
            /* The cast was interrupted before the standard was planted. */
            yield return ResolveEnvironmentalBoardChange();
            yield break;
        }

        nextRoyalBannerId =
            nextRoyalBannerId == int.MaxValue
                ? 1
                : nextRoyalBannerId + 1;

        RoyalBannerState state =
            new RoyalBannerState
            {
                BannerId = nextRoyalBannerId,
                OwnerInstanceId = request.OwnerInstanceId,
                Cell = cell
            };

        royalBannerCells[cell] = state;
        CreateOrRefreshRoyalBannerView(state);

        request.Succeeded = true;
    }

    private List<Vector2Int> BuildSafeTopRowBannerCells()
    {
        List<Vector2Int> candidates =
            new List<Vector2Int>();

        if (gems == null || height <= 0)
        {
            return candidates;
        }

        int topRow = height - 1;

        for (int column = 0;
             column < width;
             column++)
        {
            if (!IsCellPlayable(column, topRow))
            {
                continue;
            }

            Gem candidate = GetGem(column, topRow);

            if (candidate == null ||
                candidate.SpecialType != GemSpecialType.None ||
                IsGemPinned(candidate))
            {
                continue;
            }

            Vector2Int cell =
                new Vector2Int(column, topRow);

            RoyalBannerState simulation =
                new RoyalBannerState
                {
                    BannerId = -1,
                    OwnerInstanceId = int.MinValue,
                    Cell = cell
                };

            royalBannerCells[cell] = simulation;
            bool leavesPlayableMove = HasAvailableMove();
            royalBannerCells.Remove(cell);

            if (leavesPlayableMove)
            {
                candidates.Add(cell);
            }
        }

        return candidates;
    }

    /*
     * Gem destruction is reported at the end of the clear frame. Accumulate
     * every gravity opening from that clear first, then consume the complete
     * batch in Update before the board-resolution coroutine resumes. A vertical
     * clear or column bomb can therefore pull a standard through several rows
     * in one gravity resolution instead of limiting it to one row per clear.
     */
    public void NotifyGemDestroyedForRoyalBanners(
        int column,
        int row)
    {
        QueueRoyalBannerGravityOpening(column, row);
    }

    /*
     * Restoring a Miner hole happens synchronously immediately before an
     * environmental collapse. It opens one real gravity destination, so apply
     * that opening immediately rather than waiting for the next Update.
     */
    private void NotifyRoyalBannerSpaceOpened(
        int column,
        int row)
    {
        if (!IsStructurallyOpenForRoyalBannerGravity(
                column,
                row))
        {
            return;
        }

        QueueRoyalBannerGravityOpening(column, row);
        ResolvePendingRoyalBannerGravity();
    }

    private void QueueRoyalBannerGravityOpening(
        int column,
        int openedRow)
    {
        if (!IsStructurallyOpenForRoyalBannerGravity(
                column,
                openedRow) ||
            royalBannerCells.Count == 0)
        {
            return;
        }

        foreach (RoyalBannerState state
                 in royalBannerCells.Values)
        {
            if (state != null &&
                !state.ReachedBottom &&
                state.Cell.x == column &&
                openedRow < state.Cell.y)
            {
                state.PendingGravitySteps++;
            }
        }
    }

    private void Update()
    {
        ResolvePendingRoyalBannerGravity();
    }

    private void ResolvePendingRoyalBannerGravity()
    {
        if (royalBannerCells.Count == 0)
        {
            return;
        }

        List<RoyalBannerState> affected =
            new List<RoyalBannerState>();

        foreach (RoyalBannerState state
                 in royalBannerCells.Values)
        {
            if (state != null &&
                !state.ReachedBottom &&
                state.PendingGravitySteps > 0)
            {
                affected.Add(state);
            }
        }

        /*
         * Lower standards resolve first. An upper standard may never pass a
         * lower standard, while normal gems are still free to compact through
         * either standard's height during the authoritative board collapse.
         */
        affected.Sort(
            (left, right) =>
                left.Cell.y.CompareTo(right.Cell.y)
        );

        foreach (RoyalBannerState state in affected)
        {
            int requestedSteps = state.PendingGravitySteps;
            state.PendingGravitySteps = 0;

            for (int step = 0;
                 step < requestedSteps &&
                 !state.ReachedBottom;
                 step++)
            {
                if (!AdvanceRoyalBannerOneGravitySlot(state))
                {
                    break;
                }
            }
        }
    }

    private bool AdvanceRoyalBannerOneGravitySlot(
        RoyalBannerState state)
    {
        if (state == null ||
            state.ReachedBottom ||
            !royalBannerCells.TryGetValue(
                state.Cell,
                out RoyalBannerState current) ||
            current != state)
        {
            return false;
        }

        int column = state.Cell.x;
        int oldRow = state.Cell.y;
        int targetRow = -1;

        for (int row = oldRow - 1;
             row >= 0;
             row--)
        {
            if (IsCellRoyalBanner(column, row))
            {
                /* Standards never fall through one another. */
                return false;
            }

            if (!IsStructurallyOpenForRoyalBannerGravity(
                    column,
                    row))
            {
                continue;
            }

            Gem candidate = GetGem(column, row);

            if (candidate != null &&
                IsGemFixedByPin(candidate))
            {
                /* Fixed bolts/frozen gems are skipped just like gravity skips them. */
                continue;
            }

            targetRow = row;
            break;
        }

        if (targetRow < 0)
        {
            return false;
        }

        Vector2Int oldCell = state.Cell;
        Vector2Int targetCell =
            new Vector2Int(column, targetRow);

        Gem displacedGem = gems[column, targetRow];

        royalBannerCells.Remove(oldCell);

        gems[column, oldRow] = displacedGem;

        if (displacedGem != null)
        {
            /*
             * Keep the current transform position. CollapseAndRefillBoard will
             * animate the displaced gem from that visible position to its final
             * compacted row; only its authoritative grid slot changes here.
             * This is what allows gems above a standard to flow through its old
             * height and settle into open rows below it.
             */
            displacedGem.SetGridPosition(column, oldRow);
        }

        gems[column, targetRow] = null;
        state.Cell = targetCell;

        if (targetRow == 0)
        {
            /*
             * Gameplay occupancy ends immediately so the same collapse can
             * refill the bottom cell. Presentation/aura removal completes when
             * the falling banner visually reaches that row.
             */
            state.ReachedBottom = true;
            ScheduleRoyalBannerVisualMove(state, removeAtEnd: true);
        }
        else
        {
            royalBannerCells[targetCell] = state;
            ScheduleRoyalBannerVisualMove(state, removeAtEnd: false);
        }

        return true;
    }

    private bool IsStructurallyOpenForRoyalBannerGravity(
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

        Vector2Int cell = new Vector2Int(column, row);

        return
            !minedCellOwners.ContainsKey(cell) &&
            !barricadeCells.ContainsKey(cell);
    }

    private void CreateOrRefreshRoyalBannerView(
        RoyalBannerState state)
    {
        if (state == null)
        {
            return;
        }

        if (state.ViewObject == null)
        {
            state.ViewObject =
                new GameObject(
                    $"RoyalBanner_{state.BannerId}"
                );

            state.ViewObject.transform.SetParent(
                transform,
                false
            );

            state.Renderer =
                state.ViewObject.AddComponent<SpriteRenderer>();

            state.Renderer.sortingLayerName = "Gems";
            state.Renderer.sortingOrder = 24;
            state.Renderer.maskInteraction =
                SpriteMaskInteraction.VisibleInsideMask;
        }

        state.ViewObject.transform.localPosition =
            GetCellLocalPosition(state.Cell.x, state.Cell.y);

        if (state.Renderer == null)
        {
            return;
        }

        state.Renderer.sprite = royalBannerBoardSprite;
        state.Renderer.color = Color.white;
        state.Renderer.enabled = royalBannerBoardSprite != null;

        if (royalBannerBoardSprite == null)
        {
            Debug.LogWarning(
                "A Royal Banner was placed, but BoardController's Royal " +
                "Banner Board Sprite is not assigned. Gameplay and the aura " +
                "still work; assign the sprite in the BoardController Inspector.",
                this
            );

            return;
        }

        float extent = Mathf.Max(
            royalBannerBoardSprite.bounds.size.x,
            royalBannerBoardSprite.bounds.size.y
        );

        float desiredSize =
            cellSize * Mathf.Max(0.1f, royalBannerBoardScale);

        state.ViewObject.transform.localScale =
            extent > 0.0001f
                ? Vector3.one * (desiredSize / extent)
                : Vector3.one;
    }

    private void ScheduleRoyalBannerVisualMove(
        RoyalBannerState state,
        bool removeAtEnd)
    {
        if (state == null)
        {
            return;
        }

        if (state.MoveRoutine != null)
        {
            StopCoroutine(state.MoveRoutine);
            state.MoveRoutine = null;
        }

        state.MoveRoutine =
            StartCoroutine(
                MoveRoyalBannerVisual(
                    state,
                    GetCellLocalPosition(
                        state.Cell.x,
                        state.Cell.y
                    ),
                    removeAtEnd
                )
            );
    }

    private IEnumerator MoveRoyalBannerVisual(
        RoyalBannerState state,
        Vector3 targetPosition,
        bool removeAtEnd)
    {
        if (state == null ||
            state.ViewObject == null)
        {
            if (removeAtEnd)
            {
                CompleteRoyalBannerRemoval(state);
            }

            yield break;
        }

        Vector3 startPosition =
            state.ViewObject.transform.localPosition;

        float duration =
            CalculateFallDuration(
                startPosition,
                targetPosition
            );

        float elapsed = 0f;

        while (elapsed < duration &&
               state != null &&
               state.ViewObject != null)
        {
            float progress =
                duration > 0f
                    ? Mathf.Clamp01(elapsed / duration)
                    : 1f;

            state.ViewObject.transform.localPosition =
                Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    1f - Mathf.Pow(1f - progress, 3f)
                );

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (state != null &&
            state.ViewObject != null)
        {
            state.ViewObject.transform.localPosition = targetPosition;
        }

        if (state != null)
        {
            state.MoveRoutine = null;
        }

        if (removeAtEnd)
        {
            CompleteRoyalBannerRemoval(state);
        }
    }

    private void CompleteRoyalBannerRemoval(
        RoyalBannerState state)
    {
        if (state == null)
        {
            return;
        }

        royalBannerCells.Remove(state.Cell);

        if (state.MoveRoutine != null)
        {
            StopCoroutine(state.MoveRoutine);
            state.MoveRoutine = null;
        }

        if (state.ViewObject != null)
        {
            Destroy(state.ViewObject);
        }

        int bannerId = state.BannerId;
        state.ViewObject = null;
        state.Renderer = null;

        if (bannerId > 0)
        {
            RoyalBannerRemoved?.Invoke(bannerId);
        }
    }
}
