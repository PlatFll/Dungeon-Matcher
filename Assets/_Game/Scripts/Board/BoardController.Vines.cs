using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class VineNodeSnapshot
{
    public int gemId, bornMove, ownerSlot = -1, limit = 3;
    public long ownerId;
    public bool environmental, nonSpreading;
}

public partial class BoardController
{
    private const int EnvironmentalVineOwner = -2147483000;
    private readonly List<VineNodeSnapshot> vineNodes = new List<VineNodeSnapshot>();
    public int VineCount { get { PruneVines(); return vineNodes.Count; } }
    public int RestrictionCount => pinnedGemOwners.Count + pendingPinTargetOwners.Count + ReservedVineCount;
    private int ReservedVineCount
    {
        get
        {
            int count=0;
            foreach(var threat in gemSetThreats)
                if(threat.Vine && !threat.Ended && (threat.Environmental || (threat.Owner!=null && !threat.Owner.IsDefeated)))
                    foreach(var gem in threat.Targets) if(IsEnvironmentalOrdinaryGem(gem) && !IsGemPinned(gem)) count++;
            return count;
        }
    }
    private bool IsReservedVine(Gem gem)
    {
        foreach(var threat in gemSetThreats) if(threat.Vine && !threat.Ended && threat.Targets.Contains(gem)) return true;
        return false;
    }
    private Gem FindVineGem(int identity)
    {
        for(int y=0;y<height;y++) for(int x=0;x<width;x++)
        { var gem=GetGem(x,y); if(gem!=null && gem.BoardIdentity==identity) return gem; }
        return null;
    }
    private EnemyActor VineOwner(long id)
    {
        if(RunSession.Current?.Waves==null) return null;
        foreach(var enemy in RunSession.Current.Waves.ActiveEnemies) if(enemy!=null && enemy.PersistentId==id && !enemy.IsDefeated) return enemy;
        return null;
    }
    private void PruneVines()
    {
        vineNodes.RemoveAll(node => { var gem=FindVineGem(node.gemId); return gem==null || !IsGemPinned(gem) || !movablePinnedGems.Contains(gem); });
    }
    public bool TryQueueVineWarning(EnemyActor owner, int count, int ownerLimit, Action<GemSetThreat> completed, bool nonSpreading = false)
    {
        if(owner==null || owner.IsDefeated || RestrictionCount>=BalanceV1.Current.maximumGlobalChains) return false;
        int available=Mathf.Min(count,ownerLimit-GetPinnedGemCountForOwner(owner.GetInstanceID()),BalanceV1.Current.maximumGlobalChains-RestrictionCount);
        if(available<=0) return false;
        var request=new BoardMutationRequest { Kind=BoardMutationKind.MarkGemSet,OwnerActor=owner,TargetCount=available,
            WarningMoves=1,Vine=true,MaximumOwnedPins=ownerLimit,NonSpreadingVine=nonSpreading };
        request.Completed=success=>completed?.Invoke(success?request.SetThreat:null);
        EnqueueBoardMutation(request); TryStartBoardMutationProcessor(); return true;
    }
    // Ritual anchors use the same safe-cell selection, capacity, movable pins,
    // cleanup and continuation as growing roots. Only their lifetime differs.
    public bool TryQueueVineAnchors(EnemyActor owner, int count, Action<bool> completed)
    {
        return TryQueueVineWarning(owner,count,count,threat =>
        {
            if(threat==null) { completed?.Invoke(false);return; }
            threat.DueMove=completedValidPlayerMoves;
            if(!TryQueueResolveVines(threat,completed)) { CancelGemSetThreat(threat);completed?.Invoke(false); }
        },true);
    }
    public int OwnedVineCount(EnemyActor owner)
    {
        PruneVines();
        return owner==null?0:vineNodes.FindAll(n=>!n.environmental && n.ownerId==owner.PersistentId).Count;
    }
    public bool IsVineGem(Gem gem, out bool anchor)
    {
        var node=gem==null?null:vineNodes.Find(n=>n.gemId==gem.BoardIdentity);
        anchor=node!=null && node.nonSpreading;
        return node!=null && IsLiveVine(node.gemId);
    }
    public bool TryQueueResolveVines(GemSetThreat threat, Action<bool> completed)
    {
        if(threat==null || !threat.Vine || threat.Ended || threat.Queued || completedValidPlayerMoves<threat.DueMove ||
            (!threat.Environmental && (threat.Owner==null || threat.Owner.IsDefeated))) return false;
        threat.Queued=true;
        EnqueueBoardMutation(new BoardMutationRequest { Kind=BoardMutationKind.ResolveVines,OwnerActor=threat.Owner,
            OwnerInstanceId=threat.Environmental?EnvironmentalVineOwner:threat.Owner.GetInstanceID(),
            SetThreat=threat,MaximumOwnedPins=threat.VineLimit,EnvironmentalPin=threat.Environmental,Completed=completed });
        TryStartBoardMutationProcessor(); return true;
    }
    private IEnumerator ExecuteResolveVines(BoardMutationRequest request)
    {
        var threat=request.SetThreat;
        if(threat==null || threat.Ended) yield break;
        CancelGemSetThreat(threat);
        if(!threat.Environmental && !TelegraphOwnerCanExecute(request.OwnerActor)) yield break;
        if(threat.ParentGemId>0 && !IsLiveVine(threat.ParentGemId)) yield break;
        foreach(var gem in threat.Targets)
        {
            if(!IsEnvironmentalOrdinaryGem(gem) || IsGemPinned(gem) || RestrictionCount>=BalanceV1.Current.maximumGlobalChains) continue;
            var pin=new BoardMutationRequest { OwnerActor=request.OwnerActor,OwnerInstanceId=request.OwnerInstanceId,
                MaximumOwnedPins=request.MaximumOwnedPins,TargetGem=gem,MovablePin=true,EnvironmentalPin=request.EnvironmentalPin };
            yield return ExecutePinRequest(pin);
            if(!pin.Succeeded) continue;
            vineNodes.Add(new VineNodeSnapshot { gemId=gem.BoardIdentity,bornMove=completedValidPlayerMoves,
                ownerId=request.OwnerActor!=null?request.OwnerActor.PersistentId:0,environmental=request.EnvironmentalPin,limit=request.MaximumOwnedPins,nonSpreading=threat.NonSpreading });
            request.Succeeded=true;
        }
    }
    private bool IsLiveVine(int gemId)
    {
        var gem=FindVineGem(gemId);
        return gem!=null && IsGemPinned(gem) && movablePinnedGems.Contains(gem);
    }
    public IEnumerator AdvanceVineNetworks(int move)
    {
        PruneVines();
        // At most one already-warned child grows globally in one accepted action.
        bool growthUsed=false;
        var growth=new List<GemSetThreat>(gemSetThreats);
        growth.Sort((a,b)=>a.DueMove!=b.DueMove?a.DueMove.CompareTo(b.DueMove):a.ParentGemId.CompareTo(b.ParentGemId));
        foreach(var threat in growth)
        {
            if(!threat.Vine || threat.ParentGemId<=0 || threat.Ended) continue;
            if(!IsLiveVine(threat.ParentGemId) || (!threat.Environmental && (threat.Owner==null || threat.Owner.IsDefeated)))
            { CancelGemSetThreat(threat); continue; }
            if(move<threat.DueMove) continue;
            if(growthUsed) { threat.DueMove=move+1; continue; }
            if(TryQueueResolveVines(threat,success=>growthUsed=success))
            {
                while(IsBusy) yield return null;
                var parentNode=vineNodes.Find(n=>n.gemId==threat.ParentGemId);
                if(parentNode!=null) parentNode.bornMove=move;
            }
        }
        // Snapshot excludes this action's newborns and never recursively spreads.
        foreach(var node in new List<VineNodeSnapshot>(vineNodes))
        {
            if(node.nonSpreading) continue;
            if(move<node.bornMove+1) continue;
            if(RestrictionCount>=BalanceV1.Current.maximumGlobalChains) { node.bornMove=move;continue; }
            bool pending=false;
            foreach(var existing in gemSetThreats) if(existing.Vine && !existing.Ended && existing.ParentGemId==node.gemId) pending=true;
            if(pending) continue;
            var parent=FindVineGem(node.gemId); if(parent==null) continue;
            var owner=node.environmental?null:VineOwner(node.ownerId);
            if(!node.environmental && owner==null) continue;
            int ownerKey=node.environmental?EnvironmentalVineOwner:owner.GetInstanceID();
            if(GetPinnedGemCountForOwner(ownerKey)>=node.limit) { node.bornMove=move; continue; }
            var candidates=BuildSafePinnableGemList();
            candidates.RemoveAll(g=>!IsEnvironmentalOrdinaryGem(g) || IsReservedVine(g) ||
                Math.Abs(g.Column-parent.Column)+Math.Abs(g.Row-parent.Row)!=1 || IsProtectedWarningTarget(g));
            if(candidates.Count==0) { node.bornMove=move; continue; }
            var warning=new GemSetThreat { Owner=owner,Vine=true,Environmental=node.environmental,VineLimit=node.limit,
                ParentGemId=node.gemId,DueMove=ReserveWarningDeadline(1) };
            warning.Targets.Add(candidates[GameplayRandom.Range(0,candidates.Count)]);
            node.bornMove=move; gemSetThreats.Add(warning); EnsureTelegraphPresentation(); GemSetMarked?.Invoke(warning);
        }
    }
    private bool IsProtectedWarningTarget(Gem gem)
    {
        foreach(var pair in gemPairThreats) if(!pair.Ended && (pair.First==gem || pair.Second==gem)) return true;
        foreach(var set in gemSetThreats) if(!set.Ended && set.Targets.Contains(gem)) return true;
        foreach(var lane in laneThreats) if(!lane.Ended && (lane.Row==gem.Row || lane.Column==gem.Column)) return true;
        return false;
    }
    public void QueueEnvironmentalVine(Gem target)
    {
        if(!IsEnvironmentalOrdinaryGem(target) || IsGemPinned(target) || IsProtectedWarningTarget(target)) return;
        var warning=new GemSetThreat { Vine=true,Environmental=true,VineLimit=3,DueMove=completedValidPlayerMoves };
        warning.Targets.Add(target);gemSetThreats.Add(warning);TryQueueResolveVines(warning,null);
    }
    public void RemoveVineSource(EnemyActor owner)
    {
        int key=owner!=null?owner.GetInstanceID():EnvironmentalVineOwner;
        foreach(var warning in new List<GemSetThreat>(gemSetThreats))
            if(warning.Vine && (owner!=null?warning.Owner==owner:warning.Environmental)) CancelGemSetThreat(warning);
        QueueReleasePinnedGems(key);
    }
    public List<VineNodeSnapshot> CaptureVines(Func<int,int> ownerSlot)
    {
        PruneVines();var saved=new List<VineNodeSnapshot>();
        foreach(var node in vineNodes)
        {
            var copy=JsonUtility.FromJson<VineNodeSnapshot>(JsonUtility.ToJson(node));
            var actor=VineOwner(node.ownerId);copy.ownerSlot=actor==null?-1:ownerSlot(actor.GetInstanceID()); saved.Add(copy);
        }
        return saved;
    }
    public void RestoreVines(List<VineNodeSnapshot> saved)
    {
        vineNodes.Clear();
        if(saved!=null) foreach(var node in saved)
            vineNodes.Add(JsonUtility.FromJson<VineNodeSnapshot>(JsonUtility.ToJson(node)));
        foreach(var node in vineNodes) if(node.environmental)
        { var gem=FindVineGem(node.gemId); if(gem!=null && IsGemPinned(gem)) pinnedGemOwners[gem]=EnvironmentalVineOwner; }
        PruneVines();
    }
    public GemSetThreat RestoredVineCast(EnemyActor owner) => gemSetThreats.Find(t=>t.Owner==owner && t.Vine && t.ParentGemId==0 && !t.Ended);
}
