using System;
using System.Collections.Generic;
using UnityEngine;

public partial class BoardController
{
    private bool restoreInsteadOfGenerate;
    public Func<Gem,Gem,bool> BeforePlayerSwap { get; set; }
    public void ReplayPlayerSwap(int x,int y,int targetX,int targetY)
    {
        if(IsBusy || IsExternalInputBlocked || GetGem(x,y)==null || GetGem(targetX,targetY)==null)
            throw new InvalidOperationException("Saved board action could not be accepted.");
        StartCoroutine(TrySwap(GetGem(x,y),GetGem(targetX,targetY)));
    }
    public void PrepareContinuation() { restoreInsteadOfGenerate=true; isBusy=true; }
    public bool CanCaptureContinuation => gems!=null && !IsBusy && !HasPendingBoardMutation;

    public BoardCombatSnapshot CaptureContinuation(Func<int,int> ownerSlot)
    {
        if(!CanCaptureContinuation) throw new InvalidOperationException("Board is resolving an action.");
        var saved=new BoardCombatSnapshot { width=width,height=height,moves=completedValidPlayerMoves,nextBanner=nextRoyalBannerId };
        for(int y=0;y<height;y++) for(int x=0;x<width;x++)
        {
            var cell=new Vector2Int(x,y); var gem=GetGem(x,y);
            var value=new BoardCellSnapshot { x=x,y=y,hasGem=gem!=null };
            if(gem!=null)
            {
                value.type=gem.Type; value.special=gem.SpecialType;
                if(pinnedGemOwners.TryGetValue(gem,out int pin))
                {
                    value.pinned=true; value.pinOwner=ownerSlot(pin);
                    value.frozen=frozenPinnedGems.Contains(gem); value.movable=movablePinnedGems.Contains(gem);
                }
            }
            if(minedCellOwners.TryGetValue(cell,out int mine)) { value.mined=true; value.mineOwner=ownerSlot(mine); }
            if(barricadeCells.TryGetValue(cell,out var barricade))
            {
                value.barricade=true; value.barricadeOwner=ownerSlot(barricade.OwnerInstanceId);
                value.durability=barricade.RemainingDurability; value.maximumDurability=barricade.MaximumDurability;
                value.barricadeStyle=barricade.Style;
            }
            if(royalBannerCells.TryGetValue(cell,out var banner))
            {
                value.banner=true; value.bannerOwner=ownerSlot(banner.OwnerInstanceId); value.bannerId=banner.BannerId;
                value.reachedBottom=banner.ReachedBottom;
            }
            saved.cells.Add(value);
        }
        foreach(var pair in gemPairThreats) if(IsGemPairThreatValid(pair))
            saved.warnings.Add(new BoardWarningSnapshot { kind=0, owner=ownerSlot(pair.Owner.GetInstanceID()),dueMove=pair.DueMove,
                targets=new List<int> { CellIndex(pair.First),CellIndex(pair.Second) } });
        foreach(var set in gemSetThreats) if(!set.Ended && set.Owner!=null && !set.Owner.IsDefeated)
        {
            var warning=new BoardWarningSnapshot { kind=1,owner=ownerSlot(set.Owner.GetInstanceID()),dueMove=set.DueMove,restoration=set.RestorationPresentation };
            foreach(var gem in set.Targets) if(gem!=null && GetGem(gem.Column,gem.Row)==gem) warning.targets.Add(CellIndex(gem));
            saved.warnings.Add(warning);
        }
        foreach(var lane in laneThreats) if(!lane.Ended && lane.Owner!=null && !lane.Owner.IsDefeated)
            saved.warnings.Add(new BoardWarningSnapshot { kind=2,owner=ownerSlot(lane.Owner.GetInstanceID()),dueMove=lane.DueMove,row=lane.Row,column=lane.Column });
        return saved;
    }
    private int CellIndex(Gem gem) => gem.Row*width+gem.Column;
    private Gem SavedGem(int index) => GetGem(index%width,index/width);

    public void RestoreContinuation(BoardCombatSnapshot saved, Func<int,EnemyActor> ownerAtSlot)
    {
        if(gems!=null || saved.width!=width || saved.height!=height || saved.cells.Count!=width*height)
            throw new InvalidOperationException("Saved board does not match the scene layout.");
        gems=new Gem[width,height]; completedValidPlayerMoves=saved.moves; nextRoyalBannerId=saved.nextBanner;
        foreach(var value in saved.cells)
        {
            var cell=new Vector2Int(value.x,value.y);
            if(value.hasGem)
            {
                var gem=CreateGem(value.x,value.y,value.type,GetLocalPosition(value.x,value.y));
                gem.SetSpecialType(value.special);
                if(value.pinned)
                {
                    int owner=ownerAtSlot(value.pinOwner)?.GetInstanceID() ?? 0;
                    pinnedGemOwners.Add(gem,owner);
                    if(value.movable) movablePinnedGems.Add(gem);
                    if(value.frozen)
                    {
                        frozenPinnedGems.Add(gem);
                        gem.gameObject.AddComponent<FrozenGemOverlayView>().Initialize(frozenGemOverlaySprite);
                    }
                    else gem.gameObject.AddComponent<PinnedGemOverlayView>().Initialize(gem,this,owner,
                        pinnedGemOverlaySprite,pinnedGemBrightness,0,0,0);
                }
            }
            if(value.mined)
            {
                minedCellOwners.Add(cell,ownerAtSlot(value.mineOwner)?.GetInstanceID() ?? 0);
                CellMiningStarted?.Invoke(value.x,value.y,0);
            }
            if(value.barricade)
            {
                var barrier=new BarricadeCellState { OwnerInstanceId=ownerAtSlot(value.barricadeOwner)?.GetInstanceID() ?? 0,
                    RemainingDurability=value.durability,MaximumDurability=value.maximumDurability,Style=value.barricadeStyle };
                barricadeCells.Add(cell,barrier); CreateOrRefreshBarricadeView(cell,barrier);
            }
            if(value.banner)
            {
                var banner=new RoyalBannerState { BannerId=value.bannerId,OwnerInstanceId=ownerAtSlot(value.bannerOwner)?.GetInstanceID() ?? 0,
                    Cell=cell,ReachedBottom=value.reachedBottom };
                royalBannerCells.Add(cell,banner); CreateOrRefreshRoyalBannerView(banner);
            }
        }
        foreach(var warning in saved.warnings)
        {
            var owner=ownerAtSlot(warning.owner);
            if(owner==null) throw new InvalidOperationException("Saved warning has no living owner.");
            if(warning.kind==0)
            {
                var pair=new GemPairThreat { Owner=owner,First=SavedGem(warning.targets[0]),Second=SavedGem(warning.targets[1]),DueMove=warning.dueMove };
                gemPairThreats.Add(pair); GemPairMarked?.Invoke(pair);
            }
            else if(warning.kind==1)
            {
                var set=new GemSetThreat { Owner=owner,DueMove=warning.dueMove,RestorationPresentation=warning.restoration };
                foreach(int index in warning.targets) { var gem=SavedGem(index); if(gem!=null) set.Targets.Add(gem); }
                gemSetThreats.Add(set); EnsureTelegraphPresentation(); GemSetMarked?.Invoke(set);
            }
            else
            {
                var lane=new LaneThreat { Owner=owner,DueMove=warning.dueMove,Row=warning.row,Column=warning.column };
                laneThreats.Add(lane); EnsureTelegraphPresentation(); LanesMarked?.Invoke(lane);
            }
        }
        isBusy=false;
    }
    public GemPairThreat RestoredPair(EnemyActor owner) => gemPairThreats.Find(t=>t.Owner==owner&&!t.Ended);
    public GemSetThreat RestoredSet(EnemyActor owner) => gemSetThreats.Find(t=>t.Owner==owner&&!t.Ended);
    public LaneThreat RestoredLanes(EnemyActor owner) => laneThreats.Find(t=>t.Owner==owner&&!t.Ended);
}
