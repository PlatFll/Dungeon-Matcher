using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class GuildSupportAbility : MonoBehaviour, IEnemySpecialAbilityRuntime
{
    private EnemyActor owner;
    private BoardController board;
    private IReadOnlyList<EnemyActor> allies;
    private EnemyStagger stagger;
    private EnemySpecialActionAvailability opportunity;
    private Coroutine cast;
    private bool LivingAlly(EnemyActor ally) => ally != null && ally != owner && ally.IsInitialized && !ally.IsDefeated;

    public void InitializeSpecialAbility(EnemyActor actor, BoardController grid, IReadOnlyList<EnemyActor> roster)
    {
        Release();
        owner = actor;
        board = grid;
        allies = roster;
        stagger = actor.GetComponent<EnemyStagger>();
        opportunity = new EnemySpecialActionAvailability(this, actor, grid, TryCast);
        actor.SpecialBecameReady += Ready;
        actor.AnimationActionReleased += Ready;
        actor.Defeated += OwnerDefeated;
        if (stagger != null) stagger.StaggerApplied += Interrupted;
    }

    private void Ready(EnemyActor actor) => opportunity?.RequestExecution();
    private void Interrupted(EnemyStagger source, float duration, float stored) => owner.ClearGuildTaunt();
    private void OwnerDefeated(EnemyActor actor) => Release();

    public static EnemyActor WoundedAlly(EnemyActor healer, IReadOnlyList<EnemyActor> roster)
    {
        // The roster may compact after a defeat. Prefer the actual continuation slot on ties.
        EnemyActor best = null;
        int bestSlot = int.MaxValue;
        for (int i = 0; i < roster.Count; i++)
        {
            var ally = roster[i];
            if (ally == null || ally == healer || !ally.IsInitialized || ally.IsDefeated || ally.CurrentHealth >= ally.MaxHealth) continue;
            int slot = RunSession.Current?.Waves?.ContinuationSlot(ally) ?? i;
            long difference = best == null ? -1 : (long)ally.CurrentHealth * best.MaxHealth - (long)best.CurrentHealth * ally.MaxHealth;
            if (difference < 0 || (difference == 0 && (slot < bestSlot || slot == bestSlot && ally.PersistentId < best.PersistentId)))
            { best = ally; bestSlot = slot; }
        }
        return best;
    }

    private bool CanTaunt() => allies.Any(LivingAlly) && !allies.Any(a => a != null &&
        (a.GuildTauntMoves > 0 || a.IsRedirectingDamage)) && board.CanImmediatelyMatch(owner.AssignedGemType);

    private bool TryCast()
    {
        if (cast != null || board.IsBusy || !owner.IsSpecialReady || Time.timeScale <= 0) return false;
        bool taunt = owner.Definition.SpecialAbilityKind == EnemySpecialAbilityKind.GuildTaunt;
        var target = taunt ? null : WoundedAlly(owner, allies);
        if ((taunt ? !CanTaunt() : target == null) || !owner.TryBeginSpecialAbilityAnimationAction()) return false;
        cast = StartCoroutine(Perform(taunt, target));
        return true;
    }

    private IEnumerator Perform(bool taunt, EnemyActor target)
    {
        yield return null;
        try
        {
            int motion = owner.StartSpecialMotion();
            if (motion > 0) yield return owner.WaitForSpecialMotionBeat(motion);
            if (owner.IsDefeated || stagger?.IsStaggered == true || motion > 0 && !owner.IsSpecialMotionCurrent(motion)) yield break;
            if (taunt)
            {
                if (!CanTaunt()) yield break;
                owner.StartGuildTaunt(CombatMoveClock.EffectAction);
            }
            else if (!LivingAlly(target) || target.RestoreHealth(10) == 0) yield break;
            CombatMoveClock.RecordCommandedBasic(owner);
            owner.AnnounceCommittedCast();
            owner.NotifySpecialAbilityUsed();
            owner.ResetSpecialCounter();
            if (motion > 0) yield return owner.WaitForSpecialMotionComplete(motion);
        }
        finally { cast = null; owner?.EndSpecialAbilityAnimationAction(); }
    }

    private void Release()
    {
        opportunity?.Dispose();
        opportunity = null;
        if (owner == null) return;
        owner.SpecialBecameReady -= Ready;
        owner.AnimationActionReleased -= Ready;
        owner.Defeated -= OwnerDefeated;
        if (stagger != null) stagger.StaggerApplied -= Interrupted;
        if (cast != null) { StopCoroutine(cast); cast = null; owner.EndSpecialAbilityAnimationAction(); }
        owner.ClearGuildTaunt();
    }
    private void OnDisable() => Release();
}
