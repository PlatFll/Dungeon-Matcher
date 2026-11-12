using System;
using System.Collections.Generic;
using UnityEngine;

// Optional native artwork only. Board rules, rewards and timing never depend on these frames.
public static class BombIdleArt
{
    const string Root = "VFX/BombRefinement/Idles/";
    static readonly Dictionary<GemSpecialType, Sprite[]> sequences = new Dictionary<GemSpecialType, Sprite[]>();
    static readonly Vector2Int[] shake = { new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(0,0), new Vector2Int(-1,0), new Vector2Int(0,0), new Vector2Int(0,1), new Vector2Int(0,0), new Vector2Int(0,-1) };
    static Sprite row, column;

    public static Sprite DirectionalStill(GemSpecialType type)
    {
        if (type == GemSpecialType.RowBomb) return row != null ? row : (row = Resources.Load<Sprite>(Root + "Row"));
        if (type == GemSpecialType.ColumnBomb) return column != null ? column : (column = Resources.Load<Sprite>(Root + "Column"));
        return null;
    }
    public static Sprite Frame(GemSpecialType type, float elapsed, Sprite fallback)
    {
        if (PresentationPreferences.ReducedMotion) return fallback;
        string family = type == GemSpecialType.PoisonBomb ? "Poison" : type == GemSpecialType.HealingBomb ? "Healing" : type == GemSpecialType.ColorCrystal ? "Star" : null;
        if (family == null) return fallback;
        if (!sequences.TryGetValue(type, out var frames))
        {
            frames = Resources.LoadAll<Sprite>(Root + family);
            Array.Sort(frames, (a,b) => string.CompareOrdinal(a.name,b.name));
            sequences.Add(type, frames);
        }
        if (frames.Length == 0) return fallback;
        float duration = type == GemSpecialType.ColorCrystal ? .08f : .1f;
        return frames[Mathf.FloorToInt(Mathf.Max(0, elapsed) / duration) % frames.Length];
    }
    public static Vector3 SocketOffset(GemSpecialType type)
    {
        // Native 64px source coordinates: Poison (32,38), Healing (32,36), Row (32,32), Column (32,31).
        float y = type == GemSpecialType.PoisonBomb ? -6 : type == GemSpecialType.HealingBomb ? -4 : type == GemSpecialType.ColumnBomb ? 1 : 0;
        return new Vector3(0,y / 64f,0);
    }
    public static Vector3 ShakeOffset(GemSpecialType type, float elapsed)
    {
        if (PresentationPreferences.ReducedMotion || (type != GemSpecialType.RowBomb && type != GemSpecialType.ColumnBomb)) return Vector3.zero;
        var p = shake[Mathf.FloorToInt(Mathf.Max(0, elapsed) / .08f) % shake.Length];
        // Move casing and its gem together; never rotate or stretch native pixels.
        return new Vector3(p.x,p.y,0) / 64f;
    }
}
