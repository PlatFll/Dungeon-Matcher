using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using UnityEngine;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using Unity.Profiling;
#endif

public enum BoardFeelPhase { Frame, PointerAcknowledged, SwapStarted, SwapArrived, Accepted, ClearImpact, FallStarted, Landing, BoardSettled, Ready }

/// <summary>Opt-in development capture. Bounded storage, no file IO or per-frame
/// strings while playing. Measures dispatch/render frames, never hardware latency.</summary>
public sealed class BoardFeelMetrics : MonoBehaviour
{
    [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
    public static void Mark(BoardController board, BoardFeelPhase phase, int count = 0)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        board.GetComponent<BoardFeelMetrics>()?.Record(phase, count);
#endif
    }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private struct Sample
    {
        public double Time, GameTime;
        public float FrameMs;
        public long Allocated;
        public int Frame, Action, Count, Hold;
        public BoardFeelPhase Phase;
    }
    private readonly Sample[] samples = new Sample[8192];
    private int next, size;
    private BoardController board;
    private ProfilerRecorder allocations;
    private bool wasReady;
    private void OnEnable()
    {
        board = GetComponent<BoardController>();
        if (board == null) { enabled = false; return; }
        board.ValidPlayerMoveAccepted += Accepted;
        board.ValidPlayerMoveCompleted += Settled;
        board.GemsLanded += Landed;
        allocations = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
    }
    private void OnDisable()
    {
        if (board != null)
        {
            board.ValidPlayerMoveAccepted -= Accepted;
            board.ValidPlayerMoveCompleted -= Settled;
            board.GemsLanded -= Landed;
        }
        allocations.Dispose();
    }
    private void Accepted(int action) => Record(BoardFeelPhase.Accepted);
    private void Settled(int action) => Record(BoardFeelPhase.BoardSettled);
    private void Landed(int count) => Record(BoardFeelPhase.Landing, count);
    private void LateUpdate()
    {
        Record(BoardFeelPhase.Frame);
        bool ready = !board.IsBusy && !board.IsExternalInputBlocked && Time.timeScale > 0;
        if (ready && !wasReady) Record(BoardFeelPhase.Ready);
        wasReady = ready;
    }
    private void Record(BoardFeelPhase phase, int count = 0)
    {
        // Bit flags explain why the last landing is not necessarily input-ready.
        int hold = (board.IsBusy ? 1 : 0) | (board.IsExternalInputBlocked ? 2 : 0) |
            (CombatMoveClock.Current?.IsBlockingWaveProgression == true ? 4 : 0) |
            (Time.timeScale <= 0 ? 8 : 0);
        samples[next] = new Sample { Time = Time.realtimeSinceStartupAsDouble, GameTime = Time.timeAsDouble, FrameMs = Time.unscaledDeltaTime * 1000f,
            Allocated = allocations.Valid ? allocations.LastValue : -1, Frame = Time.frameCount,
            Action = board.CompletedValidPlayerMoves, Count = count, Hold = hold, Phase = phase };
        next = (next + 1) % samples.Length; size = Mathf.Min(size + 1, samples.Length);
    }
    public string ExportCsv()
    {
        var text = new StringBuilder("realTime,gameTime,frame,phase,completedAction,count,holdFlags,frameMs,previousFrameGcBytes\n");
        for (int i = 0; i < size; i++)
        {
            var s = samples[(next - size + i + samples.Length) % samples.Length];
            text.AppendFormat(CultureInfo.InvariantCulture, "{0:F6},{1:F6},{2},{3},{4},{5},{6},{7:F3},{8}\n",
                s.Time, s.GameTime, s.Frame, s.Phase, s.Action, s.Count, s.Hold, s.FrameMs, s.Allocated);
        }
        return text.ToString();
    }
#endif
}
