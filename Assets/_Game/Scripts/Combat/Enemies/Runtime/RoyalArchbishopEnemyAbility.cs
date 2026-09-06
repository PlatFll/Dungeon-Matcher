using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class RoyalArchbishopEnemyAbility : MonoBehaviour, IEnemySpecialAbilityRuntime
{
    private EnemyActor actor;
    private BoardController board;
    private IReadOnlyList<EnemyActor> roster;
    private EnemySpecialActionAvailability availability;
    private BoardController.GemSetThreat runes;
    private readonly List<EnemyAutoAttack> blessed = new List<EnemyAutoAttack>();
    private bool released = true, pending, preferRunes = true;
    private int retryAfterMove = -1;

    public void InitializeSpecialAbility(EnemyActor enemy, BoardController initializedBoard, IReadOnlyList<EnemyActor> enemies)
    {
        Cleanup(); actor = enemy; board = initializedBoard; roster = enemies;
        released = false; pending = false; preferRunes = true; retryAfterMove = -1;
        actor.Defeated += Defeated;
        availability = new EnemySpecialActionAvailability(this, actor, board, TryCast);
    }
    private bool CanAct() => !released && !pending && actor != null && !actor.IsDefeated &&
        board != null && !board.IsBusy && !actor.HasAnimationActionInProgress &&
        (actor.GetComponent<EnemyStagger>() == null || !actor.GetComponent<EnemyStagger>().IsStaggered);
    private void Update()
    {
        if (!CanAct()) return;
        if (runes != null && !runes.Ended && board.CompletedValidPlayerMoves >= runes.DueMove)
        {
            if (!actor.TryBeginSpecialAbilityAnimationAction()) return;
            pending = true;
            if (!board.TryQueueResolveGemSet(runes, HealPulse, success => EndAction(), () => released)) EndAction();
            return;
        }
        if (actor.IsSpecialReady && board.CompletedValidPlayerMoves > retryAfterMove) availability.RequestExecution();
    }
    private bool TryCast()
    {
        if (!CanAct() || !actor.IsSpecialReady) return false;
        if (!preferRunes && TryBless()) return true;
        if (runes == null || runes.Ended)
        {
            if (!actor.TryBeginSpecialAbilityAnimationAction()) return false;
            pending = true;
            if (board.TryQueueMarkGemSet(actor, actor.Definition.RoyalMarkCount, actor.Definition.RoyalMarkMoves,
                true, result =>
                {
                    EndAction();
                    if (released) return;
                    if (result != null) { runes = result; Complete(false); }
                    else if (!TryBless()) retryAfterMove = board.CompletedValidPlayerMoves;
                }, () => released)) return true;
            EndAction();
        }
        if (TryBless()) return true;
        retryAfterMove = board.CompletedValidPlayerMoves;
        return false;
    }
    private bool TryBless()
    {
        int count = 0;
        foreach (var ally in roster)
        {
            if (ally == null || ally == actor || ally.IsDefeated) continue;
            var attack = ally.GetComponent<EnemyAutoAttack>();
            if (attack == null || attack.HasNextSequenceModifier(this)) continue;
            attack.SetNextSequenceModifier(this, actor.Definition.BenedictionDamageMultiplier);
            if (!blessed.Contains(attack)) blessed.Add(attack);
            EnemyBlessingView.Show(attack, this, actor.Definition.BenedictionHaloSprite);
            if (++count >= actor.Definition.BenedictionTargets) break;
        }
        if (count == 0) return false;
        Complete(true); return true;
    }
    public static EnemyActor SelectTriageTarget(EnemyActor healer, IReadOnlyList<EnemyActor> enemies)
    {
        if (healer == null || healer.Definition == null || enemies == null) return null;
        var data = healer.Definition;
        bool woundedAlly = false;
        foreach (var enemy in enemies)
            if (enemy != null && enemy != healer && enemy.CanReceiveDamage &&
                1f - enemy.HealthNormalized >= data.TriageMeaningfulWound) woundedAlly = true;
        EnemyActor best = null; float score = 0;
        foreach (var enemy in enemies)
        {
            if (enemy == null || !enemy.CanReceiveDamage || enemy.CurrentHealth >= enemy.MaxHealth || enemy.Definition == null) continue;
            float need = (1f - enemy.HealthNormalized) * data.TriageRankWeights[(int)enemy.Definition.Category];
            if (enemy == healer && woundedAlly) need *= data.TriageSelfWeightWithWoundedAllies;
            // Stable authoritative roster order breaks ties; visuals never select targets.
            if (need > score) { best = enemy; score = need; }
        }
        return best;
    }
    private void HealPulse()
    {
        if (released) return;
        var target = SelectTriageTarget(actor, roster);
        if (target != null) target.RestoreHealth(Mathf.Max(1,
            Mathf.RoundToInt(target.MaxHealth * actor.Definition.RestorationHealFraction)));
    }
    private void Complete(bool nextRunes)
    {
        actor.NotifySpecialAbilityUsed(); actor.ResetSpecialCounter(); preferRunes = nextRunes;
    }
    private void EndAction()
    {
        pending = false;
        if (actor != null) actor.EndSpecialAbilityAnimationAction();
    }
    private void Defeated(EnemyActor enemy) => Cleanup();
    private void Cleanup()
    {
        released = true; availability?.Dispose(); availability = null;
        if (board != null) board.CancelGemSetThreat(runes);
        runes = null;
        foreach (var attack in blessed) if (attack != null) attack.RemoveNextSequenceModifier(this);
        blessed.Clear();
        if (actor != null) { actor.Defeated -= Defeated; actor.EndSpecialAbilityAnimationAction(); }
    }
    private void OnDisable() => Cleanup();
    private void OnDestroy() => Cleanup();
}
