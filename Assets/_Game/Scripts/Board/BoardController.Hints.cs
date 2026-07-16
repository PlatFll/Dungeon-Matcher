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
            gems == null)
        {
            return false;
        }

        GemType[,] typeGrid =
            BuildCurrentTypeGrid();

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
                        column,
                        row,
                        column,
                        row + 1,
                        candidates
                    );
                }
            }
        }

        if (candidates.Count == 0)
        {
            return false;
        }

        HintMoveCandidate selectedMove =
            candidates[
                UnityEngine.Random.Range(
                    0,
                    candidates.Count
                )
            ];

        sourceGem =
            selectedMove.SourceGem;

        targetGem =
            selectedMove.TargetGem;

        return
            sourceGem != null &&
            targetGem != null;
    }

    private void AddHintCandidatesForSwap(
        GemType[,] typeGrid,
        int firstColumn,
        int firstRow,
        int secondColumn,
        int secondRow,
        List<HintMoveCandidate> candidates)
    {
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
            secondGem == null ||
            firstGem.Type == secondGem.Type)
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
         * A match at the first position means the second
         * gem can move into the first position.
         */
        bool createsMatchAtFirst =
            HasMatchAt(
                typeGrid,
                firstColumn,
                firstRow
            );

        /*
         * A match at the second position means the first
         * gem can move into the second position.
         */
        bool createsMatchAtSecond =
            HasMatchAt(
                typeGrid,
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

        if (createsMatchAtFirst)
        {
            candidates.Add(
                new HintMoveCandidate(
                    secondGem,
                    firstGem
                )
            );
        }

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
}