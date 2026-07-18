using System;
using UnityEngine;

public readonly struct
    GemMatchVFXContext
{
    public GemType GemType { get; }

    public int GemCount { get; }

    public int CascadeDepth { get; }

    public Vector3[] WorldPositions
    {
        get;
    }

    public float BurstDelay { get; }

    public GemMatchVFXContext(
        GemType gemType,
        int gemCount,
        int cascadeDepth,
        Vector3[] worldPositions,
        float burstDelay)
    {
        GemType =
            gemType;

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

        WorldPositions =
            worldPositions ??
            Array.Empty<Vector3>();

        BurstDelay =
            Mathf.Max(
                0f,
                burstDelay
            );
    }
}