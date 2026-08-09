using System;
using System.Collections.Generic;
using UnityEngine;

public partial class BoardController
{
    public event Action BoardActivityStarted;

    private readonly struct HintMoveCandidate
    {
        public Gem SourceGem { get; }
        public Gem TargetGem { get; }

        public HintMoveCandidate(
            Gem sourceGem,
            Gem targetGem)
        {
            SourceGem = sourceGem;
            TargetGem = targetGem;
        }
    }

    /// <summary>
    /// Stops board hints and informs other systems that
    /// the player or an ability has interacted with the board.
    ///
    /// Future power-ups that alter gems should call this
    /// before changing the board.
    /// </summary>
    public void NotifyBoardActivity()
    {
        BoardActivityStarted?.Invoke();
    }

    public bool TryGetRandomHintMove(
        out Gem sourceGem,
        out Gem targetGem)
    {
        sourceGem = null;
        targetGem = null;

        if (isBusy ||
            HasPendingBoardMutation ||
            gems == null)
        {
            return false;
        }

        GemType[,] typeGrid =
            BuildCurrentTypeGrid();

        /*
         * GemType alone cannot distinguish an ordinary colored
         * gem from a crystal that retained that hidden color.
         */
        bool[,] crystalGrid =
            BuildCurrentCrystalGrid();

        List<HintMoveCandidate> candidates =
            new List<HintMoveCandidate>();

        for (int row = 0;
             row < height;
             row++)
        {
            for (int column = 0;
                 column < width;
                 column++)
            {
                if (column + 1 < width)
                {
                    AddHintCandidatesForSwap(
                        typeGrid,
                        crystalGrid,
                        column,
                        row,
                        column + 1,
                        row,
                        candidates
                    );
                }

                if (row + 1 < height)
                {
                    AddHintCandidatesForSwap(
                        typeGrid,
                        crystalGrid,
                        column,
                        row,
                        column,
                        row + 1,
                        candidates
                    );
                }
            }
        }

        /*
         * Validate the selected move against the actual current
         * board. If a candidate somehow became stale, remove it
         * and try another.
         */
        while (candidates.Count > 0)
        {
            int selectedIndex =
                UnityEngine.Random.Range(
                    0,
                    candidates.Count
                );

            HintMoveCandidate selectedMove =
                candidates[selectedIndex];

            candidates.RemoveAt(
                selectedIndex
            );

            if (!IsHintMoveStillValid(
                    selectedMove.SourceGem,
                    selectedMove.TargetGem))
            {
                continue;
            }

            sourceGem =
                selectedMove.SourceGem;

            targetGem =
                selectedMove.TargetGem;

            return true;
        }

        return false;
    }

    private void AddHintCandidatesForSwap(
        GemType[,] typeGrid,
        bool[,] crystalGrid,
        int firstColumn,
        int firstRow,
        int secondColumn,
        int secondRow,
        List<HintMoveCandidate> candidates)
    {
        if (!IsCellPlayable(
                firstColumn,
                firstRow) ||
            !IsCellPlayable(
                secondColumn,
                secondRow))
        {
            return;
        }

        Gem firstGem =
            GetGem(
                firstColumn,
                firstRow
            );

        Gem secondGem =
            GetGem(
                secondColumn,
                secondRow
            );

        if (firstGem == null ||
            secondGem == null)
        {
            return;
        }

        bool firstIsCrystal =
            firstGem.SpecialType ==
            GemSpecialType.ColorCrystal;

        bool secondIsCrystal =
            secondGem.SpecialType ==
            GemSpecialType.ColorCrystal;

        /*
         * A crystal can activate with any adjacent gem,
         * including another crystal.
         *
         * Shake the crystal toward its valid target rather than
         * pretending its hidden original color creates a match.
         */
        if (firstIsCrystal ||
            secondIsCrystal)
        {
            Gem crystalSource =
                firstIsCrystal
                    ? firstGem
                    : secondGem;

            Gem crystalTarget =
                firstIsCrystal
                    ? secondGem
                    : firstGem;

            candidates.Add(
                new HintMoveCandidate(
                    crystalSource,
                    crystalTarget
                )
            );

            return;
        }

        /*
         * Swapping two ordinary gems of the same color changes
         * nothing and cannot produce a new match.
         */
        if (firstGem.Type ==
            secondGem.Type)
        {
            return;
        }

        GemType firstType =
            typeGrid[
                firstColumn,
                firstRow
            ];

        GemType secondType =
            typeGrid[
                secondColumn,
                secondRow
            ];

        typeGrid[
            firstColumn,
            firstRow
        ] = secondType;

        typeGrid[
            secondColumn,
            secondRow
        ] = firstType;

        /*
         * Unlike HasMatchAt, these checks treat every crystal
         * cell as colorless and therefore unable to complete
         * an ordinary three-gem match.
         */
        bool createsMatchAtFirst =
            HasHintMatchAt(
                typeGrid,
                crystalGrid,
                firstColumn,
                firstRow
            );

        bool createsMatchAtSecond =
            HasHintMatchAt(
                typeGrid,
                crystalGrid,
                secondColumn,
                secondRow
            );

        typeGrid[
            firstColumn,
            firstRow
        ] = firstType;

        typeGrid[
            secondColumn,
            secondRow
        ] = secondType;

        /*
         * A match at the first position means the second gem
         * should move into the first position.
         */
        if (createsMatchAtFirst)
        {
            candidates.Add(
                new HintMoveCandidate(
                    secondGem,
                    firstGem
                )
            );
        }

        /*
         * A match at the second position means the first gem
         * should move into the second position.
         */
        if (createsMatchAtSecond)
        {
            candidates.Add(
                new HintMoveCandidate(
                    firstGem,
                    secondGem
                )
            );
        }
    }

    private bool[,] BuildCurrentCrystalGrid()
    {
        bool[,] crystalGrid =
            new bool[
                width,
                height
            ];

        for (int row = 0;
             row < height;
             row++)
        {
            for (int column = 0;
                 column < width;
                 column++)
            {
                Gem gem =
                    GetGem(
                        column,
                        row
                    );

                crystalGrid[
                    column,
                    row
                ] =
                    gem != null &&
                    gem.SpecialType ==
                        GemSpecialType.ColorCrystal;
            }
        }

        return crystalGrid;
    }

    private bool HasHintMatchAt(
        GemType[,] typeGrid,
        bool[,] crystalGrid,
        int column,
        int row)
    {
        /*
         * Mined cells are not board cells. They terminate match lines just
         * like null Gem references terminate the runtime match collector.
         */
        if (!IsCellPlayable(
                column,
                row))
        {
            return false;
        }

        /*
         * A color crystal is colorless for ordinary matching,
         * regardless of the GemType it had before conversion.
         */
        if (crystalGrid[
                column,
                row])
        {
            return false;
        }

        GemType type =
            typeGrid[
                column,
                row
            ];

        int horizontalCount =
            1 +
            CountMatchingHintTypes(
                typeGrid,
                crystalGrid,
                column,
                row,
                -1,
                0,
                type
            ) +
            CountMatchingHintTypes(
                typeGrid,
                crystalGrid,
                column,
                row,
                1,
                0,
                type
            );

        if (horizontalCount >= 3)
        {
            return true;
        }

        int verticalCount =
            1 +
            CountMatchingHintTypes(
                typeGrid,
                crystalGrid,
                column,
                row,
                0,
                -1,
                type
            ) +
            CountMatchingHintTypes(
                typeGrid,
                crystalGrid,
                column,
                row,
                0,
                1,
                type
            );

        return verticalCount >= 3;
    }

    private int CountMatchingHintTypes(
        GemType[,] typeGrid,
        bool[,] crystalGrid,
        int startingColumn,
        int startingRow,
        int columnDirection,
        int rowDirection,
        GemType type)
    {
        int count = 0;

        int column =
            startingColumn +
            columnDirection;

        int row =
            startingRow +
            rowDirection;

        while (column >= 0 &&
               column < width &&
               row >= 0 &&
               row < height &&
               IsCellPlayable(
                   column,
                   row
               ) &&
               !crystalGrid[
                   column,
                   row
               ] &&
               typeGrid[
                   column,
                   row
               ] == type)
        {
            count++;

            column +=
                columnDirection;

            row +=
                rowDirection;
        }

        return count;
    }

    public bool IsHintMoveStillValid(
    Gem sourceGem,
    Gem targetGem)
    {
        if (isBusy ||
            HasPendingBoardMutation ||
            gems == null ||
            sourceGem == null ||
            targetGem == null)
        {
            return false;
        }

        if (!IsCellPlayable(
                sourceGem.Column,
                sourceGem.Row) ||
            !IsCellPlayable(
                targetGem.Column,
                targetGem.Row))
        {
            return false;
        }

        /*
         * Confirm both objects still occupy the board cells
         * recorded by their current grid coordinates.
         */
        if (GetGem(
                sourceGem.Column,
                sourceGem.Row) != sourceGem ||
            GetGem(
                targetGem.Column,
                targetGem.Row) != targetGem)
        {
            return false;
        }

        int columnDistance =
            Mathf.Abs(
                sourceGem.Column -
                targetGem.Column
            );

        int rowDistance =
            Mathf.Abs(
                sourceGem.Row -
                targetGem.Row
            );

        if (columnDistance +
            rowDistance != 1)
        {
            return false;
        }

        /*
         * Any adjacent swap involving a crystal is valid.
         * This includes crystal + ordinary gem, crystal + bomb,
         * and crystal + crystal.
         */
        if (sourceGem.SpecialType ==
                GemSpecialType.ColorCrystal ||
            targetGem.SpecialType ==
                GemSpecialType.ColorCrystal)
        {
            return true;
        }

        if (sourceGem.Type ==
            targetGem.Type)
        {
            return false;
        }

        GemType[,] typeGrid =
            BuildCurrentTypeGrid();

        bool[,] crystalGrid =
            BuildCurrentCrystalGrid();

        int sourceColumn =
            sourceGem.Column;

        int sourceRow =
            sourceGem.Row;

        int targetColumn =
            targetGem.Column;

        int targetRow =
            targetGem.Row;

        GemType sourceType =
            typeGrid[
                sourceColumn,
                sourceRow
            ];

        GemType targetType =
            typeGrid[
                targetColumn,
                targetRow
            ];

        typeGrid[
            sourceColumn,
            sourceRow
        ] = targetType;

        typeGrid[
            targetColumn,
            targetRow
        ] = sourceType;

        return
            HasHintMatchAt(
                typeGrid,
                crystalGrid,
                sourceColumn,
                sourceRow
            ) ||
            HasHintMatchAt(
                typeGrid,
                crystalGrid,
                targetColumn,
                targetRow
            );
    }
}