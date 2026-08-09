using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BoardController
{
    private const float MiningTileFlashDuration =
        0.10f;

    public event Action<int>
        ValidPlayerMoveCompleted;

    public event Action<int, int, float>
        CellMiningStarted;

    public event Action<int, int>
        CellRestored;

    private enum BoardMutationKind
    {
        MineRandomCell,
        RestoreOwnerCells
    }

    private sealed class BoardMutationRequest
    {
        public BoardMutationKind Kind;
        public EnemyActor OwnerActor;
        public int OwnerInstanceId;
        public int MaximumOwnedMines;
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
        int maximumOwnedMines)
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
                    safeMaximum
            }
        );

        TryStartBoardMutationProcessor();
        return true;
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

                if (GetGem(
                        column,
                        row) != null)
                {
                    return true;
                }
            }
        }

        return false;
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
        /*
         * If a Miner died during a match/cascade, wait for that player action
         * to release the board. HasPendingBoardMutation keeps new input from
         * entering during the one-frame handoff.
         */
        while (isBusy)
        {
            yield return null;
        }

        isBusy = true;

        while (pendingBoardMutations.Count > 0)
        {
            BoardMutationRequest request =
                pendingBoardMutations.Dequeue();

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
            }
        }

        isBusy = false;
        boardMutationCoroutine = null;
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

        if (minedGem == null)
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
                        row) ||
                    GetGem(
                        column,
                        row) == null)
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
