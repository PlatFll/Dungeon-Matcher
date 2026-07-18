using System;
using System.Collections.Generic;
using UnityEngine;

public partial class BoardController
{
    public event Action<GemMatchVFXContext>
        GemMatchVFXRequested;

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
}