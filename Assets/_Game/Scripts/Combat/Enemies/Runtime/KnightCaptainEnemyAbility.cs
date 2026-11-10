using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(EnemyActor))]
public sealed class KnightCaptainEnemyAbility : MonoBehaviour, IEnemySpecialAbilityRuntime, IEnemyContinuationOwner, IAcceptedMoveEnemyAbility
{
    [SerializeField, Min(0.1f)] private float commandWindup = 0.8f;
    private EnemyActor actor;
    private BoardController board;
    private IReadOnlyList<EnemyActor> roster;
    private bool released, pending, preferChains = true;
    private int retryAfterMove = -1;
    public void CaptureContinuation(EnemyCombatSnapshot saved, System.Func<EnemyActor,int> slotOf)
    { saved.preferPrimary=preferChains; saved.retryAfterMove=retryAfterMove; }
    public void RestoreContinuation(EnemyCombatSnapshot saved, System.Func<int,EnemyActor> enemyAt)
    { preferChains=saved.preferPrimary; retryAfterMove=saved.retryAfterMove; }

    public void InitializeSpecialAbility(EnemyActor enemy, BoardController initializedBoard,
        IReadOnlyList<EnemyActor> activeEnemies)
    {
        Cleanup();
        actor = enemy;
        board = initializedBoard;
        roster = activeEnemies;
        released = false;
        pending = false;
        preferChains = true;
        retryAfterMove = -1;
        if (actor != null) actor.Defeated += HandleDefeated;
    }

    public void ResolveAcceptedMove() => Update();
    private void Update()
    {
        if (!CombatMoveClock.CanOffer(actor)) return;
        if (Time.timeScale<=0 || released || pending || actor == null || actor.IsDefeated || board == null ||
            !actor.IsSpecialReady || board.IsBusy || board.HasPendingBoardMutation ||
            actor.HasAnimationActionInProgress || board.CompletedValidPlayerMoves <= retryAfterMove) return;
        var stagger = actor.GetComponent<EnemyStagger>();
        if (stagger != null && stagger.IsStaggered) return;
        if (preferChains) TryChains(true);
        else if (!TryCommand()) TryChains(false);
    }

    private void TryChains(bool allowCommandFallback)
    {
        if (!actor.TryBeginSpecialAbilityAnimationAction()) return;
        pending = true;
        actor.PrepareSpecialMotion();
        if (!board.TryQueueTopUpMovablePins(actor, actor.Definition.ChainCap, success =>
        {
            if (released) return;
            actor.EndSpecialAbilityAnimationAction();
            pending = false;
            if (success) Complete(true, false);
            else if (!allowCommandFallback || !TryCommand()) Complete(false, preferChains);
        }, () => released))
        {
            actor.EndSpecialAbilityAnimationAction();
            pending = false;
            if (!allowCommandFallback || !TryCommand()) Complete(false, preferChains);
        }
    }

    private bool TryCommand()
    {
        bool hasAlly = false;
        foreach (var ally in roster)
            if (ally != null && ally != actor && ally.IsInitialized && !ally.IsDefeated) hasAlly = true;
        if (!hasAlly || !actor.TryBeginSpecialAbilityAnimationAction()) return false;
        pending = true;
        actor.StartSpecialMotion();
        actor.NotifySpecialAbilityUsed();
        StartCoroutine(Command());
        return true;
    }

    private IEnumerator Command()
    {
        if (actor.SpecialMotionId > 0) yield return actor.WaitForSpecialMotionComplete(actor.SpecialMotionId);
        else yield return new WaitForSeconds(commandWindup);
        if (released || actor == null || actor.IsDefeated) yield break;
        if (actor.GetComponent<EnemyStagger>()?.IsStaggered == true)
        {
            actor.EndSpecialAbilityAnimationAction(); Complete(false, preferChains); yield break;
        }
        foreach (var ally in roster)
        {
            if (ally == null || ally == actor || !ally.IsInitialized || ally.IsDefeated) continue;
            ally.GetComponent<EnemyAutoAttack>()?.AdvanceReadinessWithoutAttack();
            ally.AdvanceSpecialReadiness();
        }
        actor.AnnounceCommittedCast("Advance the Ranks");
        actor.EndSpecialAbilityAnimationAction();
        Complete(true, true);
    }

    private void Complete(bool success, bool nextPreferChains)
    {
        if (released || actor == null) return;
        if (success)
        {
            // Command telegraphs at wind-up; chains announce after acceptance.
            if (!nextPreferChains) {actor.AnnounceCommittedCast("Chain Volley");actor.NotifySpecialAbilityUsed();}
            actor.ResetSpecialCounter();
            preferChains = nextPreferChains;
        }
        else retryAfterMove = board.CompletedValidPlayerMoves;
        pending = false;
    }

    private void HandleDefeated(EnemyActor enemy) => Cleanup();
    private void Cleanup()
    {
        released = true;
        StopAllCoroutines();
        if (actor != null)
        {
            actor.Defeated -= HandleDefeated;
            actor.EndSpecialAbilityAnimationAction();
            if (board != null) board.QueueReleasePinnedGems(actor.GetInstanceID());
        }
    }
    private void OnDisable() => Cleanup();
    private void OnDestroy() => Cleanup();
}
