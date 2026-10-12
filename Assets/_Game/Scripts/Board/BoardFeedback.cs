using System;
using UnityEngine;

public enum BoardFeedbackKind
{
    Swap, InvalidSwap, MatchImpact, SpecialCreated, BlockerHit, BlockerBreak,
    CrystalAnticipation, CrystalImpact
}

/// <summary>Presentation receipt, never a clear/reward or a completion gate.</summary>
public readonly struct BoardFeedback
{
    public readonly long Id;
    public readonly BoardFeedbackKind Kind;
    public readonly int Count, Cascade;
    public BoardFeedback(long id, BoardFeedbackKind kind, int count = 1, int cascade = 0)
    { Id = id; Kind = kind; Count = count; Cascade = cascade; }
}

public partial class BoardController
{
    public event Action<BoardFeedback> FeedbackPresented;
    public event Action<Gem> SpecialGemForming;
    public event Action<Vector3> BlockerShattered;
    private long feedbackSequence;
    private void PresentFeedback(BoardFeedbackKind kind, int count = 1, int cascade = 0)
    {
        FeedbackPresented?.Invoke(new BoardFeedback(++feedbackSequence, kind, count, cascade));
    }
}
