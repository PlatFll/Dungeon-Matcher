using UnityEngine;

public readonly struct BoardMatchContext
{
    public GemType GemType { get; }

    public int GemCount { get; }

    public int CascadeDepth { get; }

    public BoardMatchContext(
        GemType gemType,
        int gemCount,
        int cascadeDepth)
    {
        GemType = gemType;

        GemCount =
            Mathf.Max(
                0,
                gemCount
            );

        CascadeDepth =
            Mathf.Max(
                0,
                cascadeDepth
            );
    }
}