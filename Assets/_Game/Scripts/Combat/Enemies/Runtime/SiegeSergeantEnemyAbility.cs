using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(EnemyActor))]
public sealed class SiegeSergeantEnemyAbility : MonoBehaviour, IEnemySpecialAbilityRuntime
{
    private EnemyActor actor;
    private BoardController board;
    private EnemyStagger stagger;
    private BoardController.GemPairThreat warning;
    private bool preferFortification = true;
    private bool actionPending;
    private bool released;
    private int ownerId;
    private int retryAfterMove = -1;

    public void InitializeSpecialAbility(EnemyActor initializedEnemy,
        BoardController initializedBoard, IReadOnlyList<EnemyActor> activeEnemies)
    {
        Cleanup();
        actor = initializedEnemy;
        board = initializedBoard;
        released = false;
        actionPending = false;
        preferFortification = true;
        retryAfterMove = -1;
        if (actor == null || board == null || actor.Definition == null) return;
        ownerId = actor.GetInstanceID();
        stagger = actor.GetComponent<EnemyStagger>();
        actor.IncomingDamageMultiplier = GetIncomingDamageMultiplier;
        actor.Defeated += HandleDefeated;
        board.ValidPlayerMoveCompleted += HandleMove;
        // Optional visuals subscribe to board events; they never choose targets.
        if (board.GetComponent<BoardHammerThreatVFX>() == null)
            board.gameObject.AddComponent<BoardHammerThreatVFX>();
    }

    private float GetIncomingDamageMultiplier()
    {
        return !released && board != null && actor != null &&
            board.GetBarricadeCountForOwner(ownerId) > 0
            ? 1f - actor.Definition.BarricadeDamageReduction : 1f;
    }

    private void HandleMove(int move)
    {
        CheckWarning();
        // Queue the due strike before input can open again, even though the
        // final move still holds IsBusy. The shared queue waits for settlement.
        TryResolveWarning();
    }

    private void Update()
    {
        if (released || actor == null || actor.IsDefeated || board == null) return;
        CheckWarning();
        TryResolveWarning();
        if (actionPending || warning != null || !actor.IsSpecialReady ||
            board.IsBusy || board.HasPendingBoardMutation ||
            actor.HasAnimationActionInProgress ||
            (stagger != null && stagger.IsStaggered) ||
            board.CompletedValidPlayerMoves <= retryAfterMove) return;

        if (!actor.TryBeginSpecialAbilityAnimationAction()) return;
        actionPending = true;
        bool queued;
        if (preferFortification && board.GetBarricadeCountForOwner(ownerId) <
            actor.Definition.MaximumOwnedBarricades)
        {
            queued = board.TryQueuePlaceBarricades(actor,
                actor.Definition.BarricadesPerUse,
                actor.Definition.MaximumOwnedBarricades,
                actor.Definition.BarricadeDurability,
                actor.Definition.BarricadeStyle, true, true, CompleteFortification,
                () => released);
        }
        else
        {
            // At the cap, use the hammer rather than banking instant replacement
            // barricades. Normally the successful casts strictly alternate.
            queued = board.TryQueueMarkGemPair(actor,
                actor.Definition.HammerWarningMoves, CompleteWarning, () => released);
        }
        if (!queued) CompleteAction(false);
    }

    private void CompleteFortification(bool success)
    {
        // No legal placement also allows the hammer on the next move.
        preferFortification = false;
        CompleteAction(success);
    }

    private void CompleteWarning(BoardController.GemPairThreat markedPair)
    {
        if (released)
        {
            board?.CancelGemPairThreat(markedPair);
            return;
        }
        warning = markedPair;
        if (markedPair != null) preferFortification = true;
        CompleteAction(markedPair != null);
    }

    private void CompleteAction(bool success)
    {
        if (actor == null || released) return;
        if (success)
        {
            actor.NotifySpecialAbilityUsed();
            actor.ResetSpecialCounter();
        }
        else retryAfterMove = board.CompletedValidPlayerMoves;
        actionPending = false;
        actor.EndSpecialAbilityAnimationAction();
    }

    private void CheckWarning()
    {
        if (warning == null || board == null) return;
        if (!board.IsGemPairThreatValid(warning))
        {
            board.CancelGemPairThreat(warning);
            warning = null;
        }
    }

    private void TryResolveWarning()
    {
        if (released || actionPending || warning == null || actor == null ||
            actor.IsDefeated || actor.HasAnimationActionInProgress ||
            (stagger != null && stagger.IsStaggered) ||
            board.CompletedValidPlayerMoves < warning.DueMove) return;
        if (!actor.TryBeginSpecialAbilityAnimationAction()) return;
        actionPending = true;
        // Use the same wave/category/individual damage scale as the normal hit.
        int damage = Mathf.RoundToInt(actor.Definition.HammerBaseDamage *
            actor.RuntimeStats.DamageMultiplier);
        if (!board.TryQueueResolveGemPair(actor, warning, damage, success =>
            {
                warning = null;
                actionPending = false;
                if (actor != null) actor.EndSpecialAbilityAnimationAction();
            }))
        {
            actionPending = false;
            actor.EndSpecialAbilityAnimationAction();
        }
    }

    private void HandleDefeated(EnemyActor defeated) => Cleanup();

    private void Cleanup()
    {
        if (released) return;
        released = true;
        if (board != null)
        {
            board.ValidPlayerMoveCompleted -= HandleMove;
            board.CancelGemPairThreat(warning);
            board.OrphanBarricadesForOwner(ownerId);
        }
        warning = null;
        if (actor != null)
        {
            actor.Defeated -= HandleDefeated;
            actor.IncomingDamageMultiplier = null;
            actor.EndSpecialAbilityAnimationAction();
        }
    }

    private void OnDisable() => Cleanup();
    private void OnDestroy() => Cleanup();
}
