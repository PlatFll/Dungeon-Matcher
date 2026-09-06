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
    }
    public sealed class LaneThreat
    {
        public EnemyActor Owner { get; internal set; }
        public int Row { get; internal set; }
        public int Column { get; internal set; }
        public int DueMove { get; internal set; }
        public bool Ended { get; internal set; }
        internal bool Queued;
    }
    private readonly List<GemSetThreat> gemSetThreats = new List<GemSetThreat>();
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
        foreach (var threat in gemSetThreats) threat.Targets.Remove(gem);
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
    public void CancelLaneThreat(LaneThreat threat) { if (threat != null) threat.Ended = true; }

    public bool TryQueueMarkGemSet(EnemyActor owner, int count, int moves, bool restoration,
        Action<GemSetThreat> completed, Func<bool> cancelled)
    {
        if (owner == null || owner.IsDefeated || gems == null) return false;
        var request = new BoardMutationRequest { Kind = BoardMutationKind.MarkGemSet,
            OwnerActor = owner, TargetCount = Mathf.Max(1, count), WarningMoves = Mathf.Max(1, moves),
            RestorationPresentation = restoration, IsCancelled = cancelled };
        request.Completed = success => completed?.Invoke(success ? request.SetThreat : null);
        pendingBoardMutations.Enqueue(request);
        TryStartBoardMutationProcessor();
        return true;
    }
    private void ExecuteMarkGemSet(BoardMutationRequest request)
    {
        if (!TelegraphOwnerCanExecute(request.OwnerActor)) return;
        gemSetThreats.RemoveAll(t => t.Ended || t.Owner == null || t.Owner.IsDefeated);
        var candidates = new List<Gem>();
        for (int y = 0; y < height; y++) for (int x = 0; x < width; x++)
        {
            Gem gem = GetGem(x,y);
            if (!IsOrdinaryGemOnBoard(gem)) continue;
            bool marked = false;
            foreach (var existing in gemSetThreats) if (existing.Targets.Contains(gem)) { marked = true; break; }
            if (!marked) candidates.Add(gem);
        }
        if (candidates.Count == 0) return;
        var threat = new GemSetThreat { Owner = request.OwnerActor,
            DueMove = completedValidPlayerMoves + request.WarningMoves,
            RestorationPresentation = request.RestorationPresentation };
        while (threat.Targets.Count < request.TargetCount && candidates.Count > 0)
        {
            int index = UnityEngine.Random.Range(0, candidates.Count);
            threat.Targets.Add(candidates[index]); candidates.RemoveAt(index);
        }
        // Metadata only: no swap/match legality changes, so no new dead board.
        gemSetThreats.Add(threat);
        request.SetThreat = threat; request.Succeeded = true;
        EnsureTelegraphPresentation();
        GemSetMarked?.Invoke(threat);
    }
    public bool TryQueueResolveGemSet(GemSetThreat threat, Action pulse,
        Action<bool> completed, Func<bool> cancelled)
    {
        if (threat == null || threat.Ended || threat.Queued || completedValidPlayerMoves < threat.DueMove) return false;
        threat.Queued = true;
        pendingBoardMutations.Enqueue(new BoardMutationRequest { Kind = BoardMutationKind.ResolveGemSet,
            OwnerActor = threat.Owner, SetThreat = threat, Pulse = pulse,
            Completed = completed, IsCancelled = cancelled });
        TryStartBoardMutationProcessor(); return true;
    }
    private IEnumerator ExecuteResolveGemSet(BoardMutationRequest request)
    {
        var threat = request.SetThreat;
        if (threat != null) threat.Queued = false;
        if (!TelegraphOwnerCanExecute(request.OwnerActor)) yield break;
        if (threat == null || threat.Ended || request.OwnerActor == null || request.OwnerActor.IsDefeated) yield break;
        CancelGemSetThreat(threat); // consume before any callbacks
        bool cleared = false;
        foreach (Gem gem in threat.Targets)
        {
            if (request.OwnerActor == null || request.OwnerActor.IsDefeated ||
                (request.IsCancelled != null && request.IsCancelled())) break;
            if (!IsEnvironmentalOrdinaryGem(gem)) continue;
            yield return ClearMatches(new HashSet<Gem> { gem }, null);
            cleared = true;
            if (request.OwnerActor != null && !request.OwnerActor.IsDefeated) request.Pulse?.Invoke();
        }
        if (cleared) yield return ResolveEnvironmentalBoardChange();
        request.Succeeded = true;
    }
    public bool TryQueueMarkLanes(EnemyActor owner, int moves, Action<LaneThreat> completed, Func<bool> cancelled)
    {
        if (owner == null || owner.IsDefeated || gems == null) return false;
        var request = new BoardMutationRequest { Kind = BoardMutationKind.MarkLanes,
            OwnerActor = owner, WarningMoves = Mathf.Max(1,moves), IsCancelled = cancelled };
        request.Completed = success => completed?.Invoke(success ? request.Lanes : null);
        pendingBoardMutations.Enqueue(request); TryStartBoardMutationProcessor(); return true;
    }
    private void ExecuteMarkLanes(BoardMutationRequest request)
    {
        if (!TelegraphOwnerCanExecute(request.OwnerActor)) return;
        request.Lanes = new LaneThreat { Owner = request.OwnerActor,
            Row = UnityEngine.Random.Range(0,height), Column = UnityEngine.Random.Range(0,width),
            DueMove = completedValidPlayerMoves + request.WarningMoves };
        request.Succeeded = true;
        EnsureTelegraphPresentation(); LanesMarked?.Invoke(request.Lanes);
    }
    public bool TryQueueResolveLanes(LaneThreat threat, Action impact, Action<bool> completed, Func<bool> cancelled)
    {
        if (threat == null || threat.Ended || threat.Queued || completedValidPlayerMoves < threat.DueMove) return false;
        threat.Queued = true;
        pendingBoardMutations.Enqueue(new BoardMutationRequest { Kind = BoardMutationKind.ResolveLanes,
            OwnerActor = threat.Owner, Lanes = threat, Pulse = impact, Completed = completed, IsCancelled = cancelled });
        TryStartBoardMutationProcessor(); return true;
    }
    private IEnumerator ExecuteResolveLanes(BoardMutationRequest request)
    {
        var threat = request.Lanes;
        if (threat != null) threat.Queued = false;
        if (!TelegraphOwnerCanExecute(request.OwnerActor)) yield break;
        if (threat == null || threat.Ended || request.OwnerActor == null || request.OwnerActor.IsDefeated) yield break;
        threat.Ended = true;
        bool cleared = false;
        var visited = new HashSet<Gem>();
        for (int lane = 0; lane < 2; lane++)
        {
            if (request.OwnerActor == null || request.OwnerActor.IsDefeated) break;
            bool row = lane == 0;
            int length = row ? width : height;
            LaneSlash?.Invoke(row, row ? threat.Row : threat.Column,
                length * (matchFlashDuration + matchWhiteHoldDuration + GetResponsiveMatchPostBurstDelay()));
            for (int i = 0; i < length; i++)
            {
                if (request.OwnerActor == null || request.OwnerActor.IsDefeated ||
                    (request.IsCancelled != null && request.IsCancelled())) break;
                Gem gem = GetGem(row ? i : threat.Column, row ? threat.Row : i);
                if (!IsEnvironmentalOrdinaryGem(gem) || !visited.Add(gem))
                {
                    yield return new WaitForSeconds(matchFlashDuration + matchWhiteHoldDuration + GetResponsiveMatchPostBurstDelay());
                    continue;
                }
                yield return ClearMatches(new HashSet<Gem> { gem }, null);
                cleared = true;
            }
        }
        if (request.OwnerActor != null && !request.OwnerActor.IsDefeated) request.Pulse?.Invoke();
        if (cleared) yield return ResolveEnvironmentalBoardChange();
        request.Succeeded = true;
    }
    private void EnsureTelegraphPresentation()
    {
        if (GetComponent<BoardTelegraphVFX>() == null) gameObject.AddComponent<BoardTelegraphVFX>();
    }
}
