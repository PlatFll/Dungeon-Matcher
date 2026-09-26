using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Dungeon Matcher/Tile Burst Library")]
public sealed class TileBurstLibrary : ScriptableObject
{
    [Serializable]
    public sealed class Sequence
    {
        public TileBurstKind kind;
        public Sprite[] frames = Array.Empty<Sprite>();
        public float[] durations = Array.Empty<float>();
        public bool IsUsable
        {
            get
            {
                if (frames == null || durations == null || frames.Length == 0 || frames.Length != durations.Length) return false;
                for (int i = 0; i < frames.Length; i++)
                    if (frames[i] == null || durations[i] <= 0f || float.IsNaN(durations[i]) || float.IsInfinity(durations[i])) return false;
                return true;
            }
        }
    }

    public Sequence[] sequences = Array.Empty<Sequence>();
    public Sequence Find(TileBurstKind kind)
    {
        if (sequences != null)
            foreach (Sequence sequence in sequences)
                if (sequence != null && sequence.kind == kind && sequence.IsUsable) return sequence;
        return null;
    }
}
