using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BoardController
{
    // A warning follows Gem identity, never a coordinate that refill can reuse.
    public sealed class GemPairThreat
    {
        public Gem First { get; internal set; }
        public Gem Second { get; internal set; }
        public int DueMove { get; internal set; }
        public bool Ended { get; internal set; }
        internal EnemyActor Owner;
        internal bool ResolutionQueued;
    }

    public int CompletedValidPlayerMoves => completedValidPlayerMoves;
    public event Action<GemPairThreat> GemPairMarked;
    public event Action<EnemyActor, Vector3, Vector3, float> GemPairImpact;

    public bool IsOrdinaryGemOnBoard(Gem gem)
    {
        return gem != null && GetGem(gem.Column, gem.Row) == gem &&
            IsCellPlayable(gem.Column, gem.Row) &&
            gem.SpecialType == GemSpecialType.None && !IsGemPinned(gem);
    }

    public bool IsGemPairThreatValid(GemPairThreat threat)
    {
        return threat != null && !threat.Ended &&
            threat.Owner != null && !threat.Owner.IsDefeated &&
            IsOrdinaryGemOnBoard(threat.First) &&
            IsOrdinaryGemOnBoard(threat.Second);
    }

    public void CancelGemPairThreat(GemPairThreat threat)
    {
        if (threat != null) threat.Ended = true;
    }

    // Caller owns enemy cadence; the board chooses targets only once it owns
    // the mutation queue. A preceding Miner or barricade cast cannot stale them.
    public bool TryQueueMarkGemPair(EnemyActor owner, int warningMoves,
        Action<GemPairThreat> completed, Func<bool> isCancelled = null)
    {
        if (owner == null || owner.IsDefeated || !owner.IsInitialized || gems == null)
            return false;
        BoardMutationRequest request = new BoardMutationRequest
        {
            Kind = BoardMutationKind.MarkGemPair,
            OwnerActor = owner,
            WarningMoves = Mathf.Max(1, warningMoves),
            IsCancelled = isCancelled
        };
        request.Completed = success => completed?.Invoke(
            success ? request.PairThreat : null);
        pendingBoardMutations.Enqueue(request);
        TryStartBoardMutationProcessor();
        return true;
    }

    public bool TryQueueResolveGemPair(EnemyActor owner, GemPairThreat threat,
        int playerDamage, Action<bool> completed)
    {
        if (!IsGemPairThreatValid(threat) || threat.Owner != owner ||
            threat.ResolutionQueued || completedValidPlayerMoves < threat.DueMove)
            return false;
        threat.ResolutionQueued = true;
        pendingBoardMutations.Enqueue(new BoardMutationRequest
        {
            Kind = BoardMutationKind.ResolveGemPair,
            OwnerActor = owner,
            PairThreat = threat,
            PlayerDamage = Mathf.Max(0, playerDamage),
            Completed = completed
        });
        TryStartBoardMutationProcessor();
        return true;
    }

    private void ExecuteMarkGemPair(BoardMutationRequest request)
    {
        if (request.OwnerActor == null || request.OwnerActor.IsDefeated) return;
        List<Vector2Int> candidates = BuildBarricadableCellList(true);
        List<Vector2Int> pair = ChooseStraightCellRun(candidates, 2);
        if (pair == null) return;
        request.PairThreat = new GemPairThreat
        {
            First = GetGem(pair[0].x, pair[0].y),
            Second = GetGem(pair[1].x, pair[1].y),
            DueMove = completedValidPlayerMoves + request.WarningMoves,
            Owner = request.OwnerActor
        };
        request.Succeeded = true;
        GemPairMarked?.Invoke(request.PairThreat);
    }

    private IEnumerator ExecuteResolveGemPair(BoardMutationRequest request)
    {
        GemPairThreat threat = request.PairThreat;
        if (!IsGemPairThreatValid(threat))
        {
            CancelGemPairThreat(threat);
            yield break;
        }

        Vector3 first = threat.First.transform.position;
        Vector3 second = threat.Second.transform.position;
        HashSet<Gem> targets = new HashSet<Gem> { threat.First, threat.Second };
        threat.Ended = true; // Consume before callbacks; one strike, one damage call.
        request.Succeeded = true;
        GemPairImpact?.Invoke(request.OwnerActor, first, second,
            matchFlashDuration + matchWhiteHoldDuration);
        if (combatController != null && combatController.PlayerActor != null)
            combatController.PlayerActor.TryTakeDamage(request.PlayerDamage);

        // Environmental removal: no clear report, rewards, adjacent-obstacle
        // damage or special activation. Resulting cascades use the normal path.
        yield return ClearMatches(targets, null);
        yield return ResolveEnvironmentalBoardChange();
    }

    private static List<Vector2Int> ChooseStraightCellRun(
        List<Vector2Int> candidates, int length)
    {
        HashSet<Vector2Int> legal = new HashSet<Vector2Int>(candidates);
        List<List<Vector2Int>> runs = new List<List<Vector2Int>>();
        // Positive directions enumerate every horizontal/vertical run once.
        foreach (Vector2Int start in candidates)
        {
            foreach (Vector2Int direction in new[] { Vector2Int.right, Vector2Int.up })
            {
                List<Vector2Int> run = new List<Vector2Int>();
                for (int offset = 0; offset < length; offset++)
                {
                    Vector2Int cell = start + direction * offset;
                    if (!legal.Contains(cell)) break;
                    run.Add(cell);
                }
                if (run.Count == length) runs.Add(run);
            }
        }
        return runs.Count > 0 ? runs[UnityEngine.Random.Range(0, runs.Count)] : null;
    }
}
