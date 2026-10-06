using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Optional data-selected cast. No production enemy is assigned this kind.</summary>
public sealed class ApplyPlayerStatusEnemyAbility : MonoBehaviour, IEnemySpecialAbilityRuntime
{
    private EnemyActor actor;
    private BoardController board;
    private EnemySpecialActionAvailability availability;
    private Coroutine casting;
    public void InitializeSpecialAbility(EnemyActor owner, BoardController initializedBoard, IReadOnlyList<EnemyActor> enemies)
    {
        Dispose(); actor = owner; board = initializedBoard;
        availability = new EnemySpecialActionAvailability(this, actor, board, TryCast);
        actor.SpecialBecameReady += Request;
        actor.AnimationActionReleased += Request;
        actor.Defeated += Died;
    }
    private void Request(EnemyActor _) => availability?.RequestExecution();
    private bool TryCast()
    {
        var player = actor.GetComponent<EnemyAutoAttack>()?.PlayerTarget ?? RunSession.Current?.Player;
        if (casting != null || player == null || !player.CanReceiveDamage || actor.Definition.appliedPlayerStatus == null ||
            board.IsBusy || Time.timeScale <= 0 || !actor.TryBeginSpecialAbilityAnimationAction()) return false;
        casting = StartCoroutine(Cast(player));
        return true;
    }
    private IEnumerator Cast(PlayerActor player)
    {
        // Always yield once so even a missing motion cannot leave a completed
        // synchronous coroutine assigned as an in-flight cast.
        yield return null;
        try
        {
            int motion = actor.StartSpecialMotion();
            if (motion > 0) yield return actor.WaitForSpecialMotionBeat(motion);
            if (!actor.IsDefeated && actor.GetComponent<EnemyStagger>()?.IsStaggered != true &&
                (motion == 0 || actor.IsSpecialMotionCurrent(motion)) && player.Statuses.Apply(actor.Definition.appliedPlayerStatus, actor))
            {
                actor.AnnounceCommittedCast();actor.NotifySpecialAbilityUsed(); actor.ResetSpecialCounter();
            }
            if (motion > 0) yield return actor.WaitForSpecialMotionComplete(motion);
        }
        finally { casting = null; actor?.EndSpecialAbilityAnimationAction(); }
    }
    private void Died(EnemyActor _) => Dispose();
    private void OnDisable() => Dispose();
    private void Dispose()
    {
        availability?.Dispose(); availability = null;
        if (actor != null)
        {
            actor.SpecialBecameReady -= Request; actor.AnimationActionReleased -= Request; actor.Defeated -= Died;
        }
        if (casting != null) { StopCoroutine(casting); casting = null; actor?.EndSpecialAbilityAnimationAction(); }
    }
}
