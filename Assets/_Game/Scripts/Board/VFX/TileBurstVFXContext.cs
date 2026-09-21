using System;
using System.Collections.Generic;
using UnityEngine;

public enum TileBurstKind { Explosion, Poison, Shield, Healing }

/// <summary>A presentation snapshot of cells actually destroyed at one shatter.</summary>
public readonly struct TileBurstVFXContext
{
    public TileBurstKind Kind { get; }
    public IReadOnlyList<Vector3> WorldPositions { get; }
    public int TileCount => WorldPositions?.Count ?? 0;

    public TileBurstVFXContext(TileBurstKind kind, Vector3[] positions)
    {
        Kind = kind;
        WorldPositions = Array.AsReadOnly(positions == null
            ? Array.Empty<Vector3>() : (Vector3[])positions.Clone());
    }
}
