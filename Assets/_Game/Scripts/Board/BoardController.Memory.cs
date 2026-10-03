using System;
using System.Collections.Generic;
using UnityEngine;

public partial class BoardController
{
    private SavedRandom refillRandom;
    private bool legacySharedRandom;
    private int nextGemIdentity;

    public event Action<int> ValidPlayerMoveAccepted;
    public event Action BoardStateRestored;

    private void NotifyValidPlayerMoveAccepted() =>
        ValidPlayerMoveAccepted?.Invoke(completedValidPlayerMoves + 1);

    private SavedRandom RefillRandom => refillRandom ??=
        new SavedRandom(GameplayRandom.Range(1, int.MaxValue));
    public uint RefillRandomState => legacySharedRandom ? 0 : RefillRandom.State;
    private int BoardRandomRange(int min, int max) => legacySharedRandom
        ? GameplayRandom.Range(min, max) : RefillRandom.Next(min, max);
    private float BoardRandomRange(float min, float max) => legacySharedRandom
        ? GameplayRandom.Range(min, max) : min + (max - min) * (float)RefillRandom.NextDouble();
    private void RestoreRefillRandom(uint state)
    {
        // Existing v1 checkpoints have no board stream. Keep their old replay
        // behavior; newly started runs always own a separate board generator.
        legacySharedRandom = state == 0;
        refillRandom = state == 0 ? null : new SavedRandom(state);
    }

    public BoardCombatSnapshot CaptureBoardMemory(Func<int, int> ownerKey)
    {
        var saved = CaptureContinuation(ownerKey);
        // Telegraphs are pending enemy actions. Their deadlines and surviving
        // target identities remain in the present combat timeline.
        saved.warnings.Clear();
        return saved;
    }

    public bool IsValidBoardMemory(BoardCombatSnapshot saved)
    {
        if (saved == null || saved.width != width || saved.height != height ||
            saved.cells == null || saved.cells.Count != width * height || saved.refillRandom == 0)
            return false;
        var positions = new HashSet<int>();
        var identities = new HashSet<int>();
        var banners = new HashSet<int>();
        foreach (var cell in saved.cells)
        {
            if (cell == null || cell.x < 0 || cell.x >= width || cell.y < 0 || cell.y >= height ||
                !positions.Add(cell.y * width + cell.x)) return false;
            int blockers = (cell.mined ? 1 : 0) + (cell.barricade ? 1 : 0) + (cell.banner ? 1 : 0);
            if (blockers > 1 || cell.hasGem == (blockers > 0) ||
                (cell.pinned && !cell.hasGem) || ((cell.frozen || cell.movable) && !cell.pinned) ||
                (cell.frozen && cell.movable)) return false;
            if (cell.hasGem && (cell.identity <= 0 || !identities.Add(cell.identity) ||
                !Enum.IsDefined(typeof(GemType), cell.type) || !Enum.IsDefined(typeof(GemSpecialType), cell.special))) return false;
            if (cell.barricade && (cell.durability <= 0 || cell.maximumDurability < cell.durability ||
                !Enum.IsDefined(typeof(EnemyBarricadeStyle), cell.barricadeStyle))) return false;
            if (cell.banner && (cell.bannerId <= 0 || !banners.Add(cell.bannerId))) return false;
        }
        return true;
    }

    // Atomic state replacement at the same settled boundary used by continuation.
    // This never enters the clear/cascade pipeline and never emits a clear reward.
    public bool TryRestoreBoardMemory(BoardCombatSnapshot memory, Func<int, EnemyActor> ownerAtKey)
    {
        if (!CanCaptureContinuation || ownerAtKey == null || !IsValidBoardMemory(memory)) return false;
        var saved = JsonUtility.FromJson<BoardCombatSnapshot>(JsonUtility.ToJson(memory));
        // A photograph restores the layout, never root durability, solved roots,
        // or the independent present-day zone growth deadline.
        var currentRoots=new Dictionary<int,BarricadeCellState>();
        foreach(var root in barricadeCells.Values) if(IsRoot(root)) currentRoots[root.RootId]=root;
        var reopenedMines = new List<Vector2Int>();
        foreach(var cell in saved.cells)
        {
            if(cell.barricadeStyle!=EnemyBarricadeStyle.Root && cell.barricadeStyle!=EnemyBarricadeStyle.Heartroot) continue;
            if(!cell.barricade) continue;
            if(!currentRoots.TryGetValue(cell.rootId,out var root) || !Living(ownerAtKey(cell.barricadeOwner)))
            { cell.barricade=false;reopenedMines.Add(new Vector2Int(cell.x,cell.y)); }
            else
            {
                cell.durability=root.RemainingDurability;cell.openRootSides=root.OpenRootSides;
                // Preserve earned openings as well as durability; a photograph
                // cannot put its older vine back over an exposed root side.
                foreach(var direction in BarricadeHitDirections)
                    if((root.OpenRootSides&SideBit(direction))!=0 && !IsCellVined(cell.x+direction.x,cell.y+direction.y))
                        saved.vines?.RemoveAll(n=>n.x==cell.x+direction.x && n.y==cell.y+direction.y);
            }
        }
        if(saved.vines!=null) saved.vines.RemoveAll(n=>
            (n.rootId>0 && !currentRoots.ContainsKey(n.rootId)) || (!n.environmental && !Living(VineOwner(n.ownerId))));
        foreach (var cell in saved.cells)
        {
            bool environmental=saved.vines!=null && saved.vines.Exists(n=>n.gemId==cell.identity && n.environmental);
            if (cell.pinned && !environmental && !Living(ownerAtKey(cell.pinOwner)))
                cell.pinned = cell.frozen = cell.movable = false;
            // Pins/mines expire with their owner; barricades and planted banners
            // persist as orphaned board objects under the existing enemy rules.
            if (cell.mined && (cell.crumbleRestoreMove>0 || !Living(ownerAtKey(cell.mineOwner))))
            {
                cell.mined = false;
                cell.crumbleRestoreMove = 0;
                reopenedMines.Add(new Vector2Int(cell.x, cell.y));
            }
        }
        // Timed zone holes stay in the present timeline. Photos cannot shorten
        // their two-move lifetime or resurrect an already returned tile.
        foreach(var entry in crumbleRestoreMoves)
        {
            var cell=saved.cells.Find(c=>c.x==entry.Key.x && c.y==entry.Key.y);
            cell.hasGem=cell.pinned=cell.frozen=cell.movable=cell.barricade=cell.banner=false;
            cell.mined=true;cell.crumbleRestoreMove=entry.Value;
            reopenedMines.RemoveAll(p=>p==entry.Key);
        }
        isBusy = true;
        NotifyBoardActivity();
        pointerStartGem = null;
        ClearSelection();
        var pairs = new Dictionary<GemPairThreat, Vector2Int>();
        foreach (var pair in gemPairThreats)
            pairs[pair] = new Vector2Int(pair.First != null ? pair.First.BoardIdentity : 0,
                pair.Second != null ? pair.Second.BoardIdentity : 0);
        var sets = new Dictionary<GemSetThreat, List<int>>();
        foreach (var set in gemSetThreats)
        {
            var targets = new List<int>();
            foreach (var gem in set.Targets) if (gem != null) targets.Add(gem.BoardIdentity);
            sets[set] = targets;
        }
        for (int y = 0; y < height; y++) for (int x = 0; x < width; x++)
            if (gems[x,y] != null) gems[x,y].RetireForStateRestoration();
        pinnedGemOwners.Clear(); frozenPinnedGems.Clear(); movablePinnedGems.Clear();
        foreach (var cell in minedCellOwners.Keys) CellRestored?.Invoke(cell.x, cell.y);
        minedCellOwners.Clear();
        foreach (var barrier in barricadeCells.Values)
        {
            CancelBarricadeVisualRoutine(barrier);
            if (barrier.ViewObject != null) barrier.ViewObject.SetActive(false);
            DestroyBarricadeView(barrier);
        }
        barricadeCells.Clear();
        foreach (var banner in royalBannerCells.Values)
        {
            if (banner.MoveRoutine != null) StopCoroutine(banner.MoveRoutine);
            if (banner.ViewObject != null) { banner.ViewObject.SetActive(false); Destroy(banner.ViewObject); }
        }
        royalBannerCells.Clear(); royalBannerClearBatchReady = false;
        gems = new Gem[width,height];
        RestoreRefillRandom(saved.refillRandom);
        nextRoyalBannerId = Mathf.Max(nextRoyalBannerId, saved.nextBanner);
        RestoreSnapshotCells(saved, key => Living(ownerAtKey(key)) ? ownerAtKey(key) : null);
        RestoreVines(saved.vines);
        foreach (var cell in reopenedMines)
            CreateGem(cell.x, cell.y, ChooseRestoredOpeningType(cell.x, cell.y), GetLocalPosition(cell.x, cell.y));
        var restored = new Dictionary<int, Gem>();
        foreach (var gem in gems) if (gem != null) restored.Add(gem.BoardIdentity, gem);
        foreach (var pair in pairs)
        {
            restored.TryGetValue(pair.Value.x, out var first);
            restored.TryGetValue(pair.Value.y, out var second);
            pair.Key.First = first; pair.Key.Second = second;
            if (!IsGemPairThreatValid(pair.Key)) CancelGemPairThreat(pair.Key);
        }
        foreach (var set in sets)
        {
            set.Key.Targets.Clear();
            foreach (int id in set.Value)
                if (restored.TryGetValue(id, out var gem) && IsEnvironmentalOrdinaryGem(gem)) set.Key.Targets.Add(gem);
            if (set.Key.Targets.Count == 0) CancelGemSetThreat(set.Key);
            else GemSetMarked?.Invoke(set.Key);
        }
        isBusy = false;
        RootsChanged?.Invoke();
        BoardStateRestored?.Invoke();
        return true;
    }

    private static bool Living(EnemyActor owner) => owner != null && !owner.IsDefeated;

    private GemType ChooseRestoredOpeningType(int x, int y)
    {
        var allowed = new List<GemType>();
        for (int type = 0; type < gemSprites.Length; type++)
        {
            bool matched = false;
            for (int axis = 0; axis < 2; axis++)
                for (int start = -2; start <= 0; start++)
                {
                    bool same = true;
                    for (int offset = start; offset <= start + 2; offset++)
                    {
                        if (offset == 0) continue;
                        var neighbor = GetGem(x + (axis == 0 ? offset : 0), y + (axis == 1 ? offset : 0));
                        same &= neighbor != null && (int)neighbor.Type == type;
                    }
                    matched |= same;
                }
            if (!matched) allowed.Add((GemType)type);
        }
        return allowed.Count > 0 ? allowed[BoardRandomRange(0, allowed.Count)] : GetRandomGemType();
    }

    public List<int> ActiveRoyalBannerIds()
    {
        var ids = new List<int>();
        foreach (var banner in royalBannerCells.Values) if (!banner.ReachedBottom) ids.Add(banner.BannerId);
        return ids;
    }
}
