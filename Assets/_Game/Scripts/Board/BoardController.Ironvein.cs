using System;
using System.Collections.Generic;
using UnityEngine;

public partial class BoardController
{
    private MineEnvironmentState mine;
    public MineEnvironmentState Mine => mine;
    public event Action MineChanged;
    private ZoneDefinition MineZone => RunSession.Current?.Zone?.Definition;
    private bool UsesMine => MineZone?.maturesStone == true;
    private int MineMove => Math.Max(completedValidPlayerMoves, CombatMoveClock.EffectAction);
    public int MineStoneCount
    {
        get { int count = 0; foreach (var s in barricadeCells.Values) if (IsMineStone(s)) count++; return count; }
    }
    private static bool IsMineStone(BarricadeCellState state) => state?.Style == EnemyBarricadeStyle.MineStone;

    public void InitializeMine()
    {
        if (!UsesMine) return;
        mine ??= new MineEnvironmentState { lastSettledMove = completedValidPlayerMoves };
        mine.version = MineEnvironmentState.CurrentVersion;
        // Foundation checkpoints predate machines; add the initial two only once.
        if (mine.drills == null) mine.drills = new List<MineDrillState>();
        if (mine.drills.Count == 0)
        {
            mine.drills.Add(new MineDrillState { id = 1, horizontal = true, lane = height / 2 });
            mine.drills.Add(new MineDrillState { id = 2, horizontal = false, lane = width / 2 });
        }
    }

    public bool TryQueuePlaceMineStones(EnemyActor owner, int count, int ownerCap,
        MineStoneStage stage = MineStoneStage.Brittle, Action<bool> completed = null,
        Func<bool> cancelled = null, bool waitForImpact = false)
    {
        if (!UsesMine || !Enum.IsDefined(typeof(MineStoneStage), stage)) return false;
        InitializeMine();
        return TryQueuePlaceBarricades(owner, count, ownerCap, (int)stage + 1,
            EnemyBarricadeStyle.MineStone, false, true, completed, cancelled, waitForImpact);
    }

    private int RemainingMineCapacity => Math.Max(0, MineZone.maximumMineStones - MineStoneCount);

    private MineStoneState NewMineStone(EnemyActor owner, int durability)
    {
        InitializeMine();
        return new MineStoneState { id = mine.nextStoneId++, ownerId = owner?.PersistentId ?? 0,
            stage = (MineStoneStage)Mathf.Clamp(durability - 1, 0, 2), bornMove = MineMove,
            lastAdvanceMove = MineMove };
    }

    private void AdvanceMineStones(int move)
    {
        if (!UsesMine) return;
        InitializeMine();
        if (move <= mine.lastSettledMove) return;
        mine.lastSettledMove = move;
        foreach (var entry in barricadeCells)
        {
            var state = entry.Value;
            if (!IsMineStone(state) || state.MineStone == null) continue;
            if (state.MineStone.Advance(move, MineZone.mineMovesPerStage))
            {
                state.MaximumDurability = state.RemainingDurability = (int)state.MineStone.stage + 1;
                CreateOrRefreshBarricadeView(entry.Key, state);
                StartBarricadeMaterialization(state);
            }
        }
        MineChanged?.Invoke();
    }

    private MineEnvironmentState CaptureMine() => mine == null ? null :
        JsonUtility.FromJson<MineEnvironmentState>(JsonUtility.ToJson(mine));
    private int MineOwnerInstance(MineStoneState stone)
    {
        if (stone == null || RunSession.Current?.Waves == null) return 0;
        foreach (var actor in RunSession.Current.Waves.ActiveEnemies)
            if (actor != null && !actor.IsDefeated && actor.PersistentId == stone.ownerId) return actor.GetInstanceID();
        return 0;
    }
    private void RestoreMine(MineEnvironmentState saved)
    {
        mine = UsesMine && saved != null ? JsonUtility.FromJson<MineEnvironmentState>(JsonUtility.ToJson(saved)) : null;
        InitializeMine();
        MineChanged?.Invoke();
    }

    // Photographs may rewind ordinary gems, but never resurrect a consumed stone,
    // repair durability or reverse material age. Keep current cell occupancy too.
    private void ReconcileMineMemory(BoardCombatSnapshot saved, List<Vector2Int> openings)
    {
        foreach (var cell in saved.cells)
        {
            if (cell.barricade && cell.barricadeStyle == EnemyBarricadeStyle.MineStone)
            {
                cell.barricade = cell.hasGem = false; cell.mineStone = null;
                openings.Add(new Vector2Int(cell.x, cell.y));
            }
        }
        foreach (var pair in barricadeCells)
        {
            if (!IsMineStone(pair.Value)) continue;
            var cell = saved.cells.Find(c => c.x == pair.Key.x && c.y == pair.Key.y);
            var state = pair.Value;
            cell.hasGem = cell.mined = cell.banner = cell.pinned = cell.frozen = cell.movable = false;
            cell.crumbleRestoreMove = 0; cell.barricade = true;
            // Mine source identity is persistent, independent of the caller's
            // slot-based continuation or instance-based photograph key space.
            cell.barricadeOwner = -1;
            cell.barricadeStyle = EnemyBarricadeStyle.MineStone;
            cell.durability = state.RemainingDurability; cell.maximumDurability = state.MaximumDurability;
            cell.mineStone = state.MineStone.Copy();
            openings.RemoveAll(p => p == pair.Key);
        }
    }

    private Sprite MineStoneSprite(BarricadeCellState state)
    {
        var art = GameplayThemeSkin.Current?.mineStoneStages;
        int stage = (int)(state.MineStone?.stage ?? MineStoneStage.Brittle);
        return art != null && stage < art.Length && art[stage] != null ? art[stage] : GetBarricadeFallbackSprite();
    }
}
