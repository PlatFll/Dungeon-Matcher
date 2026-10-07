using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BoardController
{
    public sealed class GemSetThreat
    {
        public readonly List<Gem> Targets = new List<Gem>();
        public EnemyActor Owner { get; internal set; }
        public int DueMove { get; internal set; }
        public bool Ended { get; internal set; }
        internal bool Queued;
        public bool RestorationPresentation { get; internal set; }
        public bool Vine { get; internal set; }
        public bool NonSpreading { get; internal set; }
        public bool Environmental { get; internal set; }
        internal int VineLimit = 3, ParentGemId;
        public bool PlayerInterrupted { get; internal set; }
        public bool CancelOnAnyTargetLost { get; internal set; }
        public string Label { get; internal set; }
        public int RootDurability { get; internal set; }
        public bool RootSpreading { get; internal set; }
        public EnemyBarricadeStyle RootStyle { get; internal set; }
    }
    public sealed class LaneThreat
    {
        public EnemyActor Owner { get; internal set; }
        public int Row { get; internal set; }
        public int Column { get; internal set; }
        public bool ColumnStruck { get; internal set; }
        public bool RowStruck { get; internal set; }
        public int DueMove { get; internal set; }
        public bool Ended { get; internal set; }
        internal bool Queued;
    }
    private readonly List<GemSetThreat> gemSetThreats = new List<GemSetThreat>();
    private readonly List<LaneThreat> laneThreats = new List<LaneThreat>();
    private bool deferRoyalBannerGravity;
    public event Action<GemSetThreat> GemSetMarked;
    public event Action<LaneThreat> LanesMarked;
    public event Action<bool, int, float> LaneSlash;

    private static bool TelegraphOwnerCanExecute(EnemyActor owner)
    {
        if (owner == null || owner.IsDefeated) return false;
        var stagger = owner.GetComponent<EnemyStagger>();
        return stagger == null || !stagger.IsStaggered;
    }
    internal void CancelTelegraphGem(Gem gem)
    {
        foreach (var threat in new List<GemSetThreat>(gemSetThreats))
        {
            bool removed=threat.Targets.Remove(gem);
            if(removed && threat.CancelOnAnyTargetLost) CancelGemSetThreat(threat);
            if(threat.Vine && (threat.ParentGemId==gem.BoardIdentity || threat.Targets.Count==0)) CancelGemSetThreat(threat);
        }
    }

    public bool IsEnvironmentalOrdinaryGem(Gem gem) => gem != null &&
        GetGem(gem.Column, gem.Row) == gem && IsCellPlayable(gem.Column, gem.Row) &&
        gem.SpecialType == GemSpecialType.None;

    public void CancelGemSetThreat(GemSetThreat threat)
    {
        if (threat == null) return;
        threat.Ended = true;
        gemSetThreats.Remove(threat);
    }
    public void CancelLaneThreat(LaneThreat threat) { if (threat != null) { threat.Ended = true; laneThreats.Remove(threat); } }

    public bool TryQueueMarkGemSet(EnemyActor owner, int count, int moves, bool restoration,
        Action<GemSetThreat> completed, Func<bool> cancelled, bool compact=false, bool cancelOnAnyTargetLost=false,string label=null)
    {
        if (owner == null || owner.IsDefeated || gems == null) return false;
        var request = new BoardMutationRequest { Kind = BoardMutationKind.MarkGemSet,
            OwnerActor = owner, TargetCount = Mathf.Max(1, count), WarningMoves = Mathf.Max(1, moves),
            RestorationPresentation = restoration, IsCancelled = cancelled,
            CompactTargets=compact,CancelOnAnyTargetLost=cancelOnAnyTargetLost,ThreatLabel=label };
        request.Completed = success => completed?.Invoke(success ? request.SetThreat : null);
        EnqueueBoardMutation(request);
        TryStartBoardMutationProcessor();
        return true;
    }
    private void ExecuteMarkGemSet(BoardMutationRequest request)
    {
        if (!TelegraphOwnerCanExecute(request.OwnerActor)) return;
        gemSetThreats.RemoveAll(t => t.Ended || (!t.Environmental && (t.Owner == null || t.Owner.IsDefeated)));
        var clearable = ImmediatelyClearableOrdinaryGems();
        var candidates = new List<Gem>();
        for (int y = 0; y < height; y++) for (int x = 0; x < width; x++)
        {
            Gem gem = GetGem(x,y);
            if (!IsOrdinaryGemOnBoard(gem) || (!request.CompactTargets && !clearable.Contains(gem))) continue;
            bool marked = false;
            foreach (var existing in gemSetThreats) if (existing.Targets.Contains(gem)) { marked = true; break; }
            if (request.Vine && (IsGemPinned(gem) || IsProtectedWarningTarget(gem))) marked=true;
            if(request.RootDurability>0 && !CanHostRoot(gem)) marked=true;
            if(request.CompactTargets && IsProtectedWarningTarget(gem)) marked=true;
            if (!marked) candidates.Add(gem);
        }
        if (candidates.Count == 0) return;
        var threat = new GemSetThreat { Owner = request.OwnerActor,
            DueMove = ReserveWarningDeadline(request.WarningMoves),
            RestorationPresentation = request.RestorationPresentation, Vine=request.Vine,VineLimit=request.MaximumOwnedPins,NonSpreading=request.NonSpreadingVine,
            RootDurability=request.RootDurability,RootStyle=request.BarricadeStyle,RootSpreading=request.RootSpreading,
            CancelOnAnyTargetLost=request.CancelOnAnyTargetLost,Label=request.ThreatLabel };
        int allowed=request.TargetCount;
        if(request.RootDurability>0) allowed=Mathf.Min(allowed,BalanceV1.Current.maximumGlobalStructures-minedCellOwners.Count-barricadeCells.Count);
        if(allowed<=0) return;
        if(request.CompactTargets)
        {
            var answers=candidates.FindAll(g=>clearable.Contains(g));
            if(answers.Count==0) return;
            var first=answers[GameplayRandom.Range(0,answers.Count)];
            threat.Targets.Add(first);candidates.Remove(first);
            candidates.RemoveAll(g=>Mathf.Abs(g.Column-first.Column)+Mathf.Abs(g.Row-first.Row)>1);
        }
        while (threat.Targets.Count < allowed && candidates.Count > 0)
        {
            int index = GameplayRandom.Range(0, candidates.Count);
            threat.Targets.Add(candidates[index]); candidates.RemoveAt(index);
        }
        // Metadata only: no swap/match legality changes, so no new dead board.
        gemSetThreats.Add(threat);
        request.SetThreat = threat; request.Succeeded = true;
        EnsureTelegraphPresentation();
        GemSetMarked?.Invoke(threat);
    }
    public bool TryQueueResolveGemSet(GemSetThreat threat, Action pulse,
        Action<bool> completed, Func<bool> cancelled, Func<int, int, IEnumerator> targetSequence = null,
        bool pulseBeforeClear=false)
    {
        if (threat == null || threat.Ended || threat.Queued || completedValidPlayerMoves < threat.DueMove) return false;
        threat.Queued = true;
        EnqueueBoardMutation(new BoardMutationRequest { Kind = BoardMutationKind.ResolveGemSet,
            OwnerActor = threat.Owner, SetThreat = threat, Pulse = pulse, TargetSequence = targetSequence,
            Completed = completed, IsCancelled = cancelled,PulseBeforeClear=pulseBeforeClear });
        TryStartBoardMutationProcessor(); return true;
    }
    private IEnumerator ExecuteResolveGemSet(BoardMutationRequest request)
    {
        var threat = request.SetThreat;
        if (threat != null) threat.Queued = false;
        if (!TelegraphOwnerCanExecute(request.OwnerActor)) yield break;
        if (threat == null || threat.Ended || request.OwnerActor == null || request.OwnerActor.IsDefeated) yield break;
        CancelGemSetThreat(threat); // consume before any callbacks
        // Keep the persisted target order. Snapshot survivors before any removal
        // or callback so the sequence cannot acquire newly spawned gems.
        var survivors = threat.Targets.FindAll(IsEnvironmentalOrdinaryGem);
        // An all-or-nothing threat fizzles when any target became invalid even
        // without a clear callback (for example conversion into a special).
        if(threat.CancelOnAnyTargetLost && survivors.Count!=threat.Targets.Count)
        { request.Succeeded=true;yield break; }
        if(request.PulseBeforeClear && survivors.Count>0) request.Pulse?.Invoke();
        bool cleared = false;
        deferRoyalBannerGravity = true;
        try
        {
            for (int index = 0; index < survivors.Count; index++)
            {
                if (request.OwnerActor == null || request.OwnerActor.IsDefeated ||
                    (request.IsCancelled != null && request.IsCancelled())) break;
                Gem gem = survivors[index];
                if (!IsEnvironmentalOrdinaryGem(gem)) continue;
                yield return ClearMatches(new HashSet<Gem> { gem }, null);
                cleared = true;
                if (request.OwnerActor == null || request.OwnerActor.IsDefeated ||
                    (request.IsCancelled != null && request.IsCancelled())) break;
                if (request.TargetSequence != null) yield return request.TargetSequence(index, survivors.Count);
                else if(!request.PulseBeforeClear) request.Pulse?.Invoke();
            }
        }
        finally { deferRoyalBannerGravity = false; }
        ResolvePendingRoyalBannerGravity();
        if (cleared) yield return ResolveEnvironmentalBoardChange();
        request.Succeeded = true;
    }
    public bool TryQueueMarkLanes(EnemyActor owner, int moves, Action<LaneThreat> completed, Func<bool> cancelled)
    {
        if (owner == null || owner.IsDefeated || gems == null) return false;
        var request = new BoardMutationRequest { Kind = BoardMutationKind.MarkLanes,
            OwnerActor = owner, WarningMoves = Mathf.Max(1,moves), IsCancelled = cancelled };
        request.Completed = success => completed?.Invoke(success ? request.Lanes : null);
        EnqueueBoardMutation(request); TryStartBoardMutationProcessor(); return true;
    }
    private void ExecuteMarkLanes(BoardMutationRequest request)
    {
        if (!TelegraphOwnerCanExecute(request.OwnerActor)) return;
        request.Lanes = new LaneThreat { Owner = request.OwnerActor,
            Row = GameplayRandom.Range(0,height), Column = GameplayRandom.Range(0,width),
            DueMove = ReserveWarningDeadline(request.WarningMoves) };
        laneThreats.Add(request.Lanes);
        request.Succeeded = true;
        EnsureTelegraphPresentation(); LanesMarked?.Invoke(request.Lanes);
    }
    public bool TryQueueResolveLanes(LaneThreat threat, Action impact, Action<bool> completed, Func<bool> cancelled)
    {
        if (threat == null || threat.Ended || threat.Queued || completedValidPlayerMoves < threat.DueMove) return false;
        threat.Queued = true;
        EnqueueBoardMutation(new BoardMutationRequest { Kind = BoardMutationKind.ResolveLanes,
            OwnerActor = threat.Owner, Lanes = threat, Pulse = impact, Completed = completed, IsCancelled = cancelled });
        TryStartBoardMutationProcessor(); return true;
    }
    private IEnumerator ExecuteResolveLanes(BoardMutationRequest request)
    {
        var threat = request.Lanes;
        if (threat != null) threat.Queued = false;
        if (!TelegraphOwnerCanExecute(request.OwnerActor)) yield break;
        if (threat == null || threat.Ended || request.OwnerActor == null || request.OwnerActor.IsDefeated) yield break;
        bool cleared = false;
        var visited = new HashSet<Gem>();
        deferRoyalBannerGravity = true;
        try
        {
            for (int lane = 0; lane < 2; lane++)
            {
                if (request.SpecialMotionId > 0 && !MotionCancelled(request))
                    yield return request.OwnerActor.WaitForSpecialMotionBeat(request.SpecialMotionId, lane + 1);
                else if (lane > 0) yield return new WaitForSeconds(.35f);
                if (request.OwnerActor == null || request.OwnerActor.IsDefeated || MotionCancelled(request) ||
                    (request.IsCancelled != null && request.IsCancelled())) break;
                bool row = lane == 1;
                if (row) threat.RowStruck = true; else threat.ColumnStruck = true;
                LaneSlash?.Invoke(row, row ? threat.Row : threat.Column, .18f);
                var targets = new HashSet<Gem>();
                int length = row ? width : height;
                for (int i = 0; i < length; i++)
                {
                    Gem gem = GetGem(row ? i : threat.Column, row ? threat.Row : i);
                    if (IsEnvironmentalOrdinaryGem(gem) && visited.Add(gem)) targets.Add(gem);
                }
                if (targets.Count > 0)
                {
                    yield return ClearMatches(targets, null);
                    cleared = true;
                }
            }
            CancelLaneThreat(threat);
            if (request.OwnerActor != null && !request.OwnerActor.IsDefeated && !MotionCancelled(request) &&
                (request.IsCancelled == null || !request.IsCancelled())) request.Pulse?.Invoke();
            // Recovery also belongs to this transaction. Nothing falls between
            // strikes, and cancellation still settles any holes already produced.
            if (request.SpecialMotionId > 0 && !MotionCancelled(request))
                yield return request.OwnerActor.WaitForSpecialMotionComplete(request.SpecialMotionId);
        }
        finally { deferRoyalBannerGravity = false; }
        ResolvePendingRoyalBannerGravity();
        if (cleared) yield return ResolveEnvironmentalBoardChange();
        request.Succeeded = true;
    }
    private void EnsureTelegraphPresentation()
    {
        if (GetComponent<BoardTelegraphVFX>() == null) gameObject.AddComponent<BoardTelegraphVFX>();
    }
}
