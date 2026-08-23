using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BoardController
{
    [Header("Barricade Obstacles")]

    [SerializeField]
    [Tooltip(
        "Level-1 barricade artwork. Used by wooden Villager barricades and by " +
        "damaged level-2 stone barricades after their first hit."
    )]
    private Sprite woodenBarricadeSprite;

    [SerializeField]
    [Tooltip(
        "Level-2 stone barricade artwork. A stone barricade downgrades to the " +
        "level-1 wooden sprite after taking its first hit."
    )]
    private Sprite stoneBarricadeSprite;

    private sealed class BarricadeCellState
    {
        public int OwnerInstanceId;
        public int RemainingDurability;
        public int MaximumDurability;
        public EnemyBarricadeStyle Style;
        public GameObject ViewObject;
        public SpriteRenderer Renderer;
    }

    private static readonly Vector2Int[]
        BarricadeHitDirections =
        {
            Vector2Int.left,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.up
        };

    private readonly Dictionary<Vector2Int, BarricadeCellState>
        barricadeCells =
            new Dictionary<Vector2Int, BarricadeCellState>();

    private Sprite barricadeFallbackSprite;

    public bool IsCellBarricaded(
        int column,
        int row)
    {
        return
            column >= 0 &&
            column < width &&
            row >= 0 &&
            row < height &&
            barricadeCells.ContainsKey(
                new Vector2Int(
                    column,
                    row
                )
            );
    }

    public int GetBarricadeCountForOwner(
        int ownerInstanceId)
    {
        if (ownerInstanceId == 0)
        {
            return 0;
        }

        int count = 0;

        foreach (
            KeyValuePair<Vector2Int, BarricadeCellState> entry
            in barricadeCells)
        {
            if (entry.Value != null &&
                entry.Value.OwnerInstanceId ==
                    ownerInstanceId)
            {
                count++;
            }
        }

        return count;
    }

    public void OrphanBarricadesForOwner(
        int ownerInstanceId)
    {
        if (ownerInstanceId == 0)
        {
            return;
        }

        foreach (
            KeyValuePair<Vector2Int, BarricadeCellState> entry
            in barricadeCells)
        {
            if (entry.Value != null &&
                entry.Value.OwnerInstanceId ==
                    ownerInstanceId)
            {
                /*
                 * Zero means the obstacle no longer participates in a living
                 * enemy's ownership cap. Its durability and board state remain
                 * authoritative until the player actually breaks it.
                 */
                entry.Value.OwnerInstanceId = 0;
            }
        }
    }

    public bool TryQueuePlaceBarricades(
        EnemyActor owner,
        int barricadesPerUse,
        int maximumOwnedBarricades,
        int durability,
        EnemyBarricadeStyle style)
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
                maximumOwnedBarricades
            );

        int currentlyOwned =
            GetBarricadeCountForOwner(
                ownerInstanceId
            );

        int alreadyQueued =
            GetPendingBarricadePlacementCount(
                ownerInstanceId
            );

        int remainingCapacity =
            safeMaximum -
            currentlyOwned -
            alreadyQueued;

        if (remainingCapacity <= 0)
        {
            return false;
        }

        int availableCells =
            CountBarricadableCells();

        if (availableCells <= 0)
        {
            return false;
        }

        int requestedCount =
            Mathf.Min(
                Mathf.Max(1, barricadesPerUse),
                remainingCapacity,
                availableCells
            );

        pendingBoardMutations.Enqueue(
            new BoardMutationRequest
            {
                Kind =
                    BoardMutationKind
                        .PlaceBarricades,

                OwnerActor = owner,
                OwnerInstanceId =
                    ownerInstanceId,

                BarricadeCount =
                    requestedCount,

                MaximumOwnedBarricades =
                    safeMaximum,

                BarricadeDurability =
                    Mathf.Max(1, durability),

                BarricadeStyle = style
            }
        );

        TryStartBoardMutationProcessor();
        return true;
    }

    private int GetPendingBarricadePlacementCount(
        int ownerInstanceId)
    {
        int count = 0;

        if (activeBoardMutationRequest != null &&
            activeBoardMutationRequest.Kind ==
                BoardMutationKind.PlaceBarricades &&
            activeBoardMutationRequest.OwnerInstanceId ==
                ownerInstanceId)
        {
            count +=
                activeBoardMutationRequest
                    .BarricadeCount;
        }

        foreach (BoardMutationRequest request
                 in pendingBoardMutations)
        {
            if (request == null ||
                request.Kind !=
                    BoardMutationKind.PlaceBarricades ||
                request.OwnerInstanceId !=
                    ownerInstanceId)
            {
                continue;
            }

            count += request.BarricadeCount;
        }

        return count;
    }

    private int CountBarricadableCells()
    {
        int count = 0;

        for (int row = 0;
             row < height;
             row++)
        {
            for (int column = 0;
                 column < width;
                 column++)
            {
                if (CanBarricadeCell(
                        column,
                        row))
                {
                    count++;
                }
            }
        }

        return count;
    }

    private List<Vector2Int>
        BuildBarricadableCellList()
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
                if (!CanBarricadeCell(
                        column,
                        row))
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

    private bool CanBarricadeCell(
        int column,
        int row)
    {
        if (!IsCellPlayable(
                column,
                row))
        {
            return false;
        }

        Gem gem =
            GetGem(
                column,
                row
            );

        return gem != null &&
               !IsGemPinned(gem);
    }

    private IEnumerator ExecutePlaceBarricadesRequest(
        BoardMutationRequest request)
    {
        if (request == null ||
            request.OwnerActor == null ||
            request.OwnerActor.IsDefeated ||
            request.OwnerInstanceId == 0)
        {
            yield break;
        }

        int remainingCapacity =
            request.MaximumOwnedBarricades -
            GetBarricadeCountForOwner(
                request.OwnerInstanceId
            );

        if (remainingCapacity <= 0)
        {
            yield break;
        }

        List<Vector2Int> candidates =
            BuildBarricadableCellList();

        int placementCount =
            Mathf.Min(
                request.BarricadeCount,
                remainingCapacity,
                candidates.Count
            );

        if (placementCount <= 0)
        {
            yield break;
        }

        List<Vector2Int> selectedCells =
            new List<Vector2Int>(
                placementCount
            );

        HashSet<Gem> gemsToDestroy =
            new HashSet<Gem>();

        for (int index = 0;
             index < placementCount;
             index++)
        {
            int candidateIndex =
                Random.Range(
                    0,
                    candidates.Count
                );

            Vector2Int selectedCell =
                candidates[candidateIndex];

            candidates.RemoveAt(
                candidateIndex
            );

            Gem coveredGem =
                GetGem(
                    selectedCell.x,
                    selectedCell.y
                );

            if (coveredGem == null ||
                IsGemPinned(coveredGem) ||
                !IsCellPlayable(
                    selectedCell.x,
                    selectedCell.y))
            {
                continue;
            }

            BarricadeCellState state =
                new BarricadeCellState
                {
                    OwnerInstanceId =
                        request.OwnerInstanceId,

                    RemainingDurability =
                        request.BarricadeDurability,

                    MaximumDurability =
                        request.BarricadeDurability,

                    Style =
                        request.BarricadeStyle
                };

            /*
             * Reserve the structural cell before the gem disappears. Gravity
             * and refill therefore treat every accepted barricade as blocked
             * for the whole mutation, including multi-barricade casts.
             */
            barricadeCells[selectedCell] =
                state;

            selectedCells.Add(
                selectedCell
            );

            gemsToDestroy.Add(
                coveredGem
            );
        }

        if (selectedCells.Count == 0)
        {
            yield break;
        }

        /*
         * A barricade destroys the gem underneath as environmental board
         * manipulation. It deliberately bypasses combat/reward reporters, so
         * this destruction grants no damage, energy, healing or special proc.
         */
        yield return ClearMatches(
            gemsToDestroy,
            null
        );

        foreach (Vector2Int cell
                 in selectedCells)
        {
            if (barricadeCells.TryGetValue(
                    cell,
                    out BarricadeCellState state))
            {
                CreateOrRefreshBarricadeView(
                    cell,
                    state
                );
            }
        }

        yield return
            ResolveEnvironmentalBoardChange();
    }

    /*
     * A resolved clear damages each orthogonally adjacent barricade at most
     * once, even when several gems from the same clear touch the same cell.
     * A level-2 stone barricade therefore drops to the level-1 wooden visual
     * after its first hit and is destroyed by the second distinct clear.
     */
    private void DamageBarricadesAdjacentToClears(
        HashSet<Gem> clearedGems,
        HashSet<Gem> ignoredGems = null)
    {
        if (clearedGems == null ||
            clearedGems.Count == 0 ||
            barricadeCells.Count == 0)
        {
            return;
        }

        HashSet<Vector2Int> cellsHit =
            new HashSet<Vector2Int>();

        foreach (Gem gem in clearedGems)
        {
            if (gem == null ||
                (ignoredGems != null &&
                 ignoredGems.Contains(gem)))
            {
                continue;
            }

            Vector2Int gemCell =
                new Vector2Int(
                    gem.Column,
                    gem.Row
                );

            foreach (Vector2Int direction
                     in BarricadeHitDirections)
            {
                Vector2Int adjacentCell =
                    gemCell + direction;

                if (barricadeCells.ContainsKey(
                        adjacentCell))
                {
                    cellsHit.Add(
                        adjacentCell
                    );
                }
            }
        }

        if (cellsHit.Count == 0)
        {
            return;
        }

        List<Vector2Int> orderedHits =
            new List<Vector2Int>(
                cellsHit
            );

        orderedHits.Sort(
            CompareBarricadeCells
        );

        foreach (Vector2Int cell
                 in orderedHits)
        {
            if (!barricadeCells.TryGetValue(
                    cell,
                    out BarricadeCellState state) ||
                state == null)
            {
                continue;
            }

            state.RemainingDurability--;

            if (state.RemainingDurability <= 0)
            {
                RemoveBarricadeInternal(
                    cell
                );
            }
            else
            {
                CreateOrRefreshBarricadeView(
                    cell,
                    state
                );
            }
        }
    }

    private static int CompareBarricadeCells(
        Vector2Int first,
        Vector2Int second)
    {
        int rowComparison =
            first.y.CompareTo(
                second.y
            );

        return rowComparison != 0
            ? rowComparison
            : first.x.CompareTo(
                second.x
            );
    }

    private void RemoveBarricadeInternal(
        Vector2Int cell)
    {
        if (!barricadeCells.TryGetValue(
                cell,
                out BarricadeCellState state))
        {
            return;
        }

        barricadeCells.Remove(
            cell
        );

        if (state != null &&
            state.ViewObject != null)
        {
            Destroy(
                state.ViewObject
            );
        }
    }

    private void CreateOrRefreshBarricadeView(
        Vector2Int cell,
        BarricadeCellState state)
    {
        if (state == null)
        {
            return;
        }

        if (state.ViewObject == null)
        {
            state.ViewObject =
                new GameObject(
                    $"Barricade_{cell.x}_{cell.y}"
                );

            state.ViewObject.transform.SetParent(
                transform,
                false
            );

            state.Renderer =
                state.ViewObject.AddComponent<
                    SpriteRenderer
                >();

            state.Renderer.sortingLayerName =
                "Gems";

            state.Renderer.sortingOrder = 20;

            state.Renderer.maskInteraction =
                SpriteMaskInteraction
                    .VisibleInsideMask;
        }

        state.ViewObject.transform.localPosition =
            GetCellLocalPosition(
                cell.x,
                cell.y
            );

        Sprite sprite =
            GetBarricadeSprite(
                state
            );

        state.Renderer.sprite = sprite;

        bool usingFallback =
            sprite == GetBarricadeFallbackSprite();

        state.Renderer.color =
            usingFallback
                ? GetBarricadeFallbackColor(
                    state
                )
                : Color.white;

        float spriteExtent =
            sprite != null
                ? Mathf.Max(
                    sprite.bounds.size.x,
                    sprite.bounds.size.y
                )
                : 1f;

        float desiredSize =
            cellSize * 0.88f;

        float scale =
            spriteExtent > 0.0001f
                ? desiredSize / spriteExtent
                : desiredSize;

        state.ViewObject.transform.localScale =
            Vector3.one * scale;
    }

    private Sprite GetBarricadeSprite(
        BarricadeCellState state)
    {
        bool isLevelTwoStone =
            state != null &&
            state.Style ==
                EnemyBarricadeStyle.Stone &&
            state.RemainingDurability >= 2;

        if (isLevelTwoStone &&
            stoneBarricadeSprite != null)
        {
            return stoneBarricadeSprite;
        }

        if (woodenBarricadeSprite != null)
        {
            return woodenBarricadeSprite;
        }

        return GetBarricadeFallbackSprite();
    }

    private Sprite GetBarricadeFallbackSprite()
    {
        if (barricadeFallbackSprite == null)
        {
            barricadeFallbackSprite =
                Sprite.Create(
                    Texture2D.whiteTexture,
                    new Rect(
                        0f,
                        0f,
                        Texture2D.whiteTexture.width,
                        Texture2D.whiteTexture.height
                    ),
                    new Vector2(0.5f, 0.5f),
                    1f
                );

            barricadeFallbackSprite.name =
                "Runtime_Barricade_Placeholder";
        }

        return barricadeFallbackSprite;
    }

    private static Color GetBarricadeFallbackColor(
        BarricadeCellState state)
    {
        bool isLevelTwoStone =
            state != null &&
            state.Style ==
                EnemyBarricadeStyle.Stone &&
            state.RemainingDurability >= 2;

        return isLevelTwoStone
            ? new Color(0.42f, 0.43f, 0.46f, 1f)
            : new Color(0.55f, 0.30f, 0.14f, 1f);
    }
}
