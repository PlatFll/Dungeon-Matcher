using System.Collections.Generic;
using UnityEngine;

/// <summary>Plays supplied pixel frames at board-owned cell snapshots. Never changes board timing or state.</summary>
[DisallowMultipleComponent, RequireComponent(typeof(BoardController))]
public sealed class TileBurstVFXController : MonoBehaviour
{
    private sealed class Burst
    {
        public SpriteRenderer renderer;
        public TileBurstLibrary.Sequence sequence;
        public int frame;
        public float elapsed;
    }

    private BoardController board;
    private TileBurstLibrary library;
    private readonly List<Burst> active = new List<Burst>();
    private readonly Stack<Burst> pool = new Stack<Burst>();
    private const int MaximumBursts = 256;

    private void Awake()
    {
        board = GetComponent<BoardController>();
        library = Resources.Load<TileBurstLibrary>("VFX/TileBursts");
    }

    private void OnEnable()
    {
        if (board == null) board = GetComponent<BoardController>();
        board.TileBurstVFXRequested -= Play;
        board.TileBurstVFXRequested += Play;
    }

    public bool HasSequence(TileBurstKind kind) => library != null && library.Find(kind) != null;

    private void Play(TileBurstVFXContext context)
    {
        var sequence = library != null ? library.Find(context.Kind) : null;
        if (sequence == null || context.TileCount == 0) return;
        foreach (Vector3 position in context.WorldPositions)
        {
            if (active.Count >= MaximumBursts) ReleaseAt(0);
            Burst burst = pool.Count > 0 ? pool.Pop() : CreateBurst();
            burst.sequence = sequence;
            burst.frame = 0;
            burst.elapsed = 0f;
            burst.renderer.transform.localPosition = board.transform.InverseTransformPoint(position);
            burst.renderer.transform.localScale = Vector3.one *
                (board.CellSize / (sequence.frames[0].rect.width / sequence.frames[0].pixelsPerUnit));
            burst.renderer.sprite = sequence.frames[0];
            burst.renderer.enabled = true;
            active.Add(burst);
        }
    }

    private Burst CreateBurst()
    {
        var go = new GameObject("TileBurst");
        go.transform.SetParent(board.transform, false);
        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sortingLayerName = "Gems";
        renderer.sortingOrder = 12;
        renderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        return new Burst { renderer = renderer };
    }

    private void Update()
    {
        for (int i = active.Count - 1; i >= 0; i--)
        {
            var burst = active[i];
            burst.elapsed += Time.deltaTime;
            while (burst.elapsed >= burst.sequence.durations[burst.frame])
            {
                burst.elapsed -= burst.sequence.durations[burst.frame];
                burst.frame++;
                if (burst.frame >= burst.sequence.frames.Length) break;
            }
            if (burst.frame >= burst.sequence.frames.Length) ReleaseAt(i);
            else burst.renderer.sprite = burst.sequence.frames[burst.frame];
        }
    }

    private void ReleaseAt(int index)
    {
        var burst = active[index];
        active.RemoveAt(index);
        burst.renderer.enabled = false;
        burst.renderer.sprite = null;
        burst.sequence = null;
        pool.Push(burst);
    }

    private void OnDisable()
    {
        if (board != null) board.TileBurstVFXRequested -= Play;
        while (active.Count > 0) ReleaseAt(active.Count - 1);
    }

    private void OnDestroy()
    {
        while (pool.Count > 0)
        {
            var renderer = pool.Pop().renderer;
            if (renderer == null) continue;
            if (Application.isPlaying) Destroy(renderer.gameObject);
            else DestroyImmediate(renderer.gameObject);
        }
    }
}
