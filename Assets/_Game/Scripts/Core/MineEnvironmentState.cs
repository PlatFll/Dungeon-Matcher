using System;
using System.Collections.Generic;

public enum MineStoneStage { Brittle, Hardened, Obsidian }

/// <summary>Material history; structural durability remains owned by the board barricade.</summary>
[Serializable]
public sealed class MineStoneState
{
    public long id, ownerId;
    public MineStoneStage stage;
    public int ignoredMoves, lastHitMove = -1, lastAdvanceMove, bornMove;

    public void RecordHit(int move) { ignoredMoves = 0; lastHitMove = move; }
    public bool Advance(int move, int movesPerStage)
    {
        if (move <= lastAdvanceMove) return false;
        lastAdvanceMove = move;
        if (move <= bornMove || move <= lastHitMove || stage == MineStoneStage.Obsidian) return false;
        if (++ignoredMoves < Math.Max(1, movesPerStage)) return false;
        stage++; ignoredMoves = 0; return true;
    }
    public MineStoneState Copy() => (MineStoneState)MemberwiseClone();
}

[Serializable]
public sealed class MineEnvironmentState
{
    public const int CurrentVersion = 2;
    public int version = CurrentVersion, lastSettledMove;
    public long nextStoneId = 1;
    public List<MineDrillState> drills = new List<MineDrillState>();

    public static bool Supports(BoardCombatSnapshot board)
    {
        if (board == null) return true;
        var state = board.mine;
        if (state != null && (state.version > CurrentVersion || state.version < 0 ||
            state.nextStoneId < 1 || state.lastSettledMove < 0)) return false;
        var ids = new HashSet<long>();
        var machines = new HashSet<int>();
        if (state?.drills != null) foreach (var drill in state.drills)
            if (drill == null || drill.id < 1 || drill.id > 2 || !machines.Add(drill.id) ||
                drill.lane < 0 || drill.lane >= (drill.horizontal ? board.height : board.width) ||
                drill.charge < 0 || drill.lastPlayerAction < 0) return false;
        if (board.cells != null) foreach (var cell in board.cells)
        {
            if (cell == null || !cell.barricade || cell.barricadeStyle != EnemyBarricadeStyle.MineStone) continue;
            var stone = cell.mineStone;
            if (state == null || stone == null || stone.id <= 0 || stone.id >= state.nextStoneId ||
                !ids.Add(stone.id) || !Enum.IsDefined(typeof(MineStoneStage), stone.stage) ||
                stone.ignoredMoves < 0 || cell.maximumDurability != (int)stone.stage + 1 ||
                cell.durability <= 0 || cell.durability > cell.maximumDurability) return false;
        }
        return true;
    }
}

[Serializable]
public sealed class MineDrillState
{
    public int id, lane, charge, lastPlayerAction;
    public bool horizontal;
}
