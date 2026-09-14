using System;
using System.Collections.Generic;

public enum RunActionKind { Swap, Ability, Potion, Bomb, ChooseCard, RefineDraft }
[Serializable] public sealed class RunRecordedAction
{
    public RunActionKind kind;
    public int x, y, targetX, targetY;
    public string card;
    public RunUpgradeTheme theme;
}
[Serializable] public sealed class RunReplayFrame
{
    public float delta;
    public List<RunRecordedAction> actions = new List<RunRecordedAction>();
}
[Serializable] public sealed class RunReplayTape
{
    public List<RunReplayFrame> frames = new List<RunReplayFrame>();
}
