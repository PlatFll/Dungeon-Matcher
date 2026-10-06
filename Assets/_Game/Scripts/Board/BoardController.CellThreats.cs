using System;
using System.Collections.Generic;
using UnityEngine;

public partial class BoardController
{
    /// <summary>Fixed-cell response marks. Answers persist even after refill or vine regrowth.</summary>
    public sealed class CellResponseThreat
    {
        public EnemyActor Owner { get; internal set; }
        public int DueMove { get; internal set; }
        public bool RequiresVine { get; internal set; }
        public bool Answered { get; internal set; }
        public bool Ended { get; internal set; }
        internal bool Queued;
        public readonly List<Vector2Int> Cells = new List<Vector2Int>();
    }
    private readonly List<CellResponseThreat> cellResponseThreats = new List<CellResponseThreat>();
    public event Action<CellResponseThreat> CellResponseMarked;
    public CellResponseThreat RestoredCellResponse(EnemyActor owner) => cellResponseThreats.Find(t=>t.Owner==owner && !t.Ended);
    public void CancelCellResponse(CellResponseThreat threat)
    { if(threat!=null) { threat.Ended=true;cellResponseThreats.Remove(threat); } }

    public bool TryQueueCellResponse(EnemyActor owner,int count,int moves,bool requiresVine,Action<CellResponseThreat> completed)
    {
        if(owner==null || owner.IsDefeated || gems==null) return false;
        var request=new BoardMutationRequest {Kind=BoardMutationKind.MarkCellResponse,OwnerActor=owner,
            TargetCount=Mathf.Clamp(count,1,3),WarningMoves=Mathf.Max(2,moves),RequiresVine=requiresVine};
        request.Completed=ok=>completed?.Invoke(ok?request.CellThreat:null);
        EnqueueBoardMutation(request);TryStartBoardMutationProcessor();return true;
    }
    private void ExecuteMarkCellResponse(BoardMutationRequest request)
    {
        if(!TelegraphOwnerCanExecute(request.OwnerActor)) return;
        cellResponseThreats.RemoveAll(t=>t.Ended || t.Owner==null || t.Owner.IsDefeated);
        var candidates=new List<Vector2Int>();
        var clearable=ImmediatelyClearableOrdinaryCells();
        for(int y=0;y<height;y++) for(int x=0;x<width;x++)
        {
            var gem=GetGem(x,y);var cell=new Vector2Int(x,y);
            if(!IsEnvironmentalOrdinaryGem(gem) || IsProtectedWarningTarget(gem) ||
                cellResponseThreats.Exists(t=>t.Cells.Contains(cell)) || (request.RequiresVine && !IsCellVined(x,y))) continue;
            candidates.Add(cell);
        }
        var answers=candidates.FindAll(c=>clearable.Contains(c));
        if(answers.Count==0) return; // Every warning has at least one immediate legal answer.
        var threat=new CellResponseThreat {Owner=request.OwnerActor,RequiresVine=request.RequiresVine,
            DueMove=ReserveWarningDeadline(request.WarningMoves)};
        var first=answers[GameplayRandom.Range(0,answers.Count)];
        threat.Cells.Add(first);candidates.Remove(first);
        // Heavy response targets form a compact group, not scattered board-wide marks.
        if(!request.RequiresVine) candidates.RemoveAll(c=>Mathf.Abs(c.x-first.x)+Mathf.Abs(c.y-first.y)>1);
        while(threat.Cells.Count<request.TargetCount && candidates.Count>0)
        { int pick=GameplayRandom.Range(0,candidates.Count);threat.Cells.Add(candidates[pick]);candidates.RemoveAt(pick); }
        cellResponseThreats.Add(threat);request.CellThreat=threat;request.Succeeded=true;
        EnsureTelegraphPresentation();CellResponseMarked?.Invoke(threat);
    }
    public bool TryQueueResolveCellResponse(CellResponseThreat threat,Action<int,bool> impact,Action<bool> completed)
    {
        if(threat==null || threat.Ended || threat.Queued || completedValidPlayerMoves<threat.DueMove) return false;
        threat.Queued=true;
        EnqueueBoardMutation(new BoardMutationRequest {Kind=BoardMutationKind.ResolveCellResponse,OwnerActor=threat.Owner,
            CellThreat=threat,CellResponseImpact=impact,Completed=completed});
        TryStartBoardMutationProcessor();return true;
    }
    private void ExecuteResolveCellResponse(BoardMutationRequest request)
    {
        var threat=request.CellThreat;
        if(threat==null || threat.Ended) return;
        threat.Queued=false;
        if(!TelegraphOwnerCanExecute(request.OwnerActor)) return;
        int remaining=0;
        foreach(var cell in threat.Cells) if(!threat.RequiresVine || IsCellVined(cell.x,cell.y)) remaining++;
        CancelCellResponse(threat); // Consume before callbacks; no gem clears or extra reward pipeline.
        request.Succeeded=true;request.CellResponseImpact?.Invoke(remaining,threat.Answered);
    }
    private void AnswerCellResponse(Vector2Int cell,bool vineRemoved)
    {
        foreach(var threat in cellResponseThreats)
        {
            if(threat.Ended || (threat.RequiresVine && !vineRemoved)) continue;
            if(threat.Cells.Remove(cell)) threat.Answered=true;
        }
    }

    private HashSet<Vector2Int> ImmediatelyClearableOrdinaryCells()
    {
        var cells=new HashSet<Vector2Int>();
        foreach(var response in GetImmediateResponses())
        {
            if(response.UsesSpecial) continue;
            foreach(var gem in response.Clears)
            {
                if(!IsOrdinaryGemOnBoard(gem)) continue;
                // ResponseOption keeps gem identity. Fixed overlays and thorn
                // edges need the occupied cell AFTER the proposed swap.
                cells.Add(response.FinalCell(gem));
            }
        }
        return cells;
    }
}
