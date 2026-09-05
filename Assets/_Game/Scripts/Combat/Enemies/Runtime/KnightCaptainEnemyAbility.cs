using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(EnemyActor))]
public sealed class KnightCaptainEnemyAbility : MonoBehaviour, IEnemySpecialAbilityRuntime
{
    [SerializeField, Min(0.1f)] private float commandWindup = 0.8f;
    [SerializeField, Min(0f)] private float strikeSpacing = 0.2f;
    private EnemyActor actor;
    private BoardController board;
    private IReadOnlyList<EnemyActor> roster;
    private readonly List<EnemyAutoAttack> participants = new List<EnemyAutoAttack>();
    private readonly HashSet<EnemyActor> commandLocks = new HashSet<EnemyActor>();
    private bool released, pending, preferChains = true;
    private int retryAfterMove = -1;

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

    private void Update()
    {
        if (released || pending || actor == null || actor.IsDefeated || board == null ||
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
        if (!board.TryQueueTopUpMovablePins(actor, 3, success =>
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
        var attack = actor.GetComponent<EnemyAutoAttack>();
        if (attack == null || !attack.TryReserveCommand(this)) return false;
        participants.Add(attack);
        foreach (var ally in roster)
        {
            if (ally == null || ally == actor || ally.IsDefeated ||
                ally.Definition == null || !ally.Definition.CrownSoldier) continue;
            var allyAttack = ally.GetComponent<EnemyAutoAttack>();
            if (allyAttack != null && allyAttack.TryReserveCommand(this))
            {
                if (ally.TryBeginSpecialAbilityAnimationAction())
                {
                    participants.Add(allyAttack);
                    commandLocks.Add(ally);
                }
                else allyAttack.ReleaseCommand(this);
            }
        }
        if (!actor.TryBeginSpecialAbilityAnimationAction())
        {
            ReleaseParticipants();
            return false;
        }
        pending = true;
        commandLocks.Add(actor);
        actor.NotifySpecialAbilityUsed();
        StartCoroutine(Command());
        return true;
    }

    private IEnumerator Command()
    {
        yield return new WaitForSeconds(commandWindup);
        if (released || actor == null || actor.IsDefeated) yield break;
        actor.EndSpecialAbilityAnimationAction();
        commandLocks.Remove(actor);
        foreach (var attack in participants)
        {
            if (released || actor == null || actor.IsDefeated) yield break;
            if (attack == null || attack.EnemyActor == null || attack.EnemyActor.IsDefeated) continue;
            attack.EnemyActor.EndSpecialAbilityAnimationAction();
            commandLocks.Remove(attack.EnemyActor);
            if (attack.PerformCommandStrike(this))
            {
                while (!released && attack != null && attack.IsAttackSequenceInProgress)
                    yield return null;
                if (attack != null) attack.ReleaseCommand(this);
                yield return new WaitForSeconds(strikeSpacing);
            }
        }
        ReleaseParticipants();
        Complete(true, true);
    }

    private void Complete(bool success, bool nextPreferChains)
    {
        if (released || actor == null) return;
        if (success)
        {
            // Command telegraphs at wind-up; chains announce after acceptance.
            if (!nextPreferChains) actor.NotifySpecialAbilityUsed();
            actor.ResetSpecialCounter();
            preferChains = nextPreferChains;
        }
        else retryAfterMove = board.CompletedValidPlayerMoves;
        pending = false;
    }

    private void ReleaseParticipants()
    {
        foreach (var lockedActor in commandLocks)
            if (lockedActor != null) lockedActor.EndSpecialAbilityAnimationAction();
        commandLocks.Clear();
        foreach (var attack in participants)
        {
            if (attack == null) continue;
            if (attack.IsCommandReservedBy(this))
            {
                attack.ReleaseCommand(this);
            }
        }
        participants.Clear();
    }

    private void HandleDefeated(EnemyActor enemy) => Cleanup();
    private void Cleanup()
    {
        released = true;
        StopAllCoroutines();
        ReleaseParticipants();
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
