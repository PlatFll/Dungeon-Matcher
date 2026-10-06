using System.Collections.Generic;
using UnityEngine;

public enum CasterTargetKind { Gem, Cell, Row, Column }
public struct CasterBoardTarget
{
    public EnemyActor Owner;
    public CasterTargetKind Kind;
    public Gem Gem;
    public Vector2Int Cell;
    public int Lane;
    public static CasterBoardTarget OnGem(EnemyActor owner,Gem gem)=>new CasterBoardTarget{Owner=owner,Kind=CasterTargetKind.Gem,Gem=gem};
    public static CasterBoardTarget OnCell(EnemyActor owner,Vector2Int cell)=>new CasterBoardTarget{Owner=owner,Kind=CasterTargetKind.Cell,Cell=cell};
}
public partial class BoardController
{
    // Read-only projection of existing authoritative threats; no new lifecycle,
    // deadlines, selection, damage or save state in the presentation layer.
    public void CollectCasterTargets(List<CasterBoardTarget> result)
    {
        result.Clear();
        foreach(var pair in gemPairThreats) if(IsGemPairThreatValid(pair))
        {result.Add(CasterBoardTarget.OnGem(pair.Owner,pair.First));result.Add(CasterBoardTarget.OnGem(pair.Owner,pair.Second));}
        foreach(var set in gemSetThreats) if(!set.Ended && !set.Environmental && LiveCaster(set.Owner))
            foreach(var gem in set.Targets) if(IsEnvironmentalOrdinaryGem(gem)) result.Add(CasterBoardTarget.OnGem(set.Owner,gem));
        foreach(var threat in cellResponseThreats) if(!threat.Ended && LiveCaster(threat.Owner))
            foreach(var cell in threat.Cells) result.Add(CasterBoardTarget.OnCell(threat.Owner,cell));
        foreach(var lane in laneThreats) if(!lane.Ended && LiveCaster(lane.Owner))
        {
            if(!lane.RowStruck)result.Add(new CasterBoardTarget{Owner=lane.Owner,Kind=CasterTargetKind.Row,Lane=lane.Row});
            if(!lane.ColumnStruck)result.Add(new CasterBoardTarget{Owner=lane.Owner,Kind=CasterTargetKind.Column,Lane=lane.Column});
        }
        // Resolve consumes a set before clearing its targets sequentially. Keep
        // identity on surviving targets until that same mutation has finished.
        var active=activeBoardMutationRequest;
        if(active?.Kind==BoardMutationKind.ResolveGemSet && !active.Succeeded &&
            active.SetThreat!=null && LiveCaster(active.OwnerActor))
            foreach(var gem in active.SetThreat.Targets)
                if(IsEnvironmentalOrdinaryGem(gem)) result.Add(CasterBoardTarget.OnGem(active.OwnerActor,gem));
        // A fixed pin reservation is already a selected target during its cast.
        AddPendingCasterTarget(activeBoardMutationRequest,result);
        foreach(var request in pendingBoardMutations) AddPendingCasterTarget(request,result);
    }
    private void AddPendingCasterTarget(BoardMutationRequest request,List<CasterBoardTarget> result)
    {
        if(request==null || request.Succeeded || !LiveCaster(request.OwnerActor) || request.TargetGem==null ||
            GetGem(request.TargetGem.Column,request.TargetGem.Row)!=request.TargetGem ||
            (request.AnimationActionId>0 && request.OwnerActor.ActiveSpecialAbilityAnimationActionId!=request.AnimationActionId))return;
        result.Add(CasterBoardTarget.OnGem(request.OwnerActor,request.TargetGem));
    }
    private static bool LiveCaster(EnemyActor actor)=>actor!=null && !actor.IsDefeated && actor.isActiveAndEnabled;
    public Vector3 CasterTargetLocalPosition(CasterBoardTarget target)
    {
        switch(target.Kind)
        {
            case CasterTargetKind.Gem:return transform.InverseTransformPoint(target.Gem.transform.position);
            case CasterTargetKind.Cell:return GetCellLocalPosition(target.Cell.x,target.Cell.y);
            case CasterTargetKind.Row:return GetCellLocalPosition(Width-1,target.Lane)+Vector3.right*CellSize*.68f;
            default:return GetCellLocalPosition(target.Lane,Height-1)+Vector3.up*CellSize*.68f;
        }
    }
}
