using System;
using System.Collections.Generic;
using UnityEngine;

public partial class BoardController
{
    public event Action<GemMatchVFXContext>
        GemMatchVFXRequested;

    public event Action<BombVFXContext>
        BombVFXRequested;

    private void ReportMatchesToVFX(
        HashSet<Gem> matches,
        int cascadeDepth)
    {
        if (matches == null ||
            matches.Count == 0)
        {
            return;
        }

        List<List<Gem>> matchGroups =
            BuildConnectedMatchGroups(
                matches
            );

        foreach (
            List<Gem> group
            in matchGroups)
        {
            if (group == null ||
                group.Count < 3)
            {
                continue;
            }

            Gem firstGem =
                group[0];

            if (firstGem == null)
            {
                continue;
            }

            Vector3[] worldPositions =
                new Vector3[group.Count];

            for (int index = 0;
                 index < group.Count;
                 index++)
            {
                Gem gem =
                    group[index];

                worldPositions[index] =
                    gem != null
                        ? gem.transform.position
                        : Vector3.zero;
            }

            GemMatchVFXRequested?.Invoke(
                new GemMatchVFXContext(
                    firstGem.Type,
                    group.Count,
                    cascadeDepth,
                    worldPositions,
                    matchFlashDuration
                )
            );
        }
    }

    private void ReportBombClearSetToVFX(
        HashSet<Gem> expandedClearSet,
        BoardClearSource clearSource)
    {
        if (expandedClearSet == null ||
            expandedClearSet.Count == 0)
        {
            return;
        }

        /*
         * These are the two current pathways where a row or
         * column bomb in the expanded set genuinely detonates.
         *
         * Double-crystal sweeps can erase a bomb without firing
         * its directional effect, so they intentionally do not
         * produce a row/column beam here.
         */
        if (clearSource != BoardClearSource.Bomb &&
            clearSource != BoardClearSource.ColorCrystal)
        {
            return;
        }

        EnsureBombVFXController();

        foreach (Gem gem in expandedClearSet)
        {
            if (gem == null ||
                (
                    gem.SpecialType !=
                        GemSpecialType.RowBomb &&
                    gem.SpecialType !=
                        GemSpecialType.ColumnBomb
                ))
            {
                continue;
            }

            BombVFXRequested?.Invoke(
                new BombVFXContext(
                    gem.SpecialType,
                    gem.transform.position,
                    matchFlashDuration
                )
            );
        }
    }

    private void EnsureBombVFXController()
    {
        if (GetComponent<BombVFXController>() !=
            null)
        {
            return;
        }

        /*
         * The effect is completely runtime-generated, so the
         * board does not need a new prefab reference or scene
         * setup. Adding the component here also means existing
         * scenes automatically gain the VFX the first time a
         * real row/column bomb detonates.
         */
        gameObject.AddComponent<
            BombVFXController
        >();
    }
}
