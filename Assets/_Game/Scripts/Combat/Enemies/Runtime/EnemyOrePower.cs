using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>A saved, non-stacking token consumed by the existing whole-basic sequence.</summary>
[DisallowMultipleComponent]
public sealed class EnemyOrePower : MonoBehaviour, IEnemyContinuationOwner
{
    private EnemyActor actor;
    private EnemyAutoAttack attack;
    private BoardController board;
    private IReadOnlyList<EnemyActor> allies;
    private float multiplier;
    private bool burstConsumed;
    public bool IsPowered => attack != null && attack.HasNextSequenceModifier(this);
    public bool BurstConsumed => burstConsumed;
    public event Action Changed;

    public void Initialize(EnemyActor owner, BoardController targetBoard, IReadOnlyList<EnemyActor> roster)
    {
        actor = owner; board = targetBoard; allies = roster;
        attack = owner.GetComponent<EnemyAutoAttack>();
        actor.Defeated += OnDefeated;
        if (attack != null) attack.NextSequenceModifiersChanged += OnModifiersChanged;
    }

    public bool Grant(float strength = 0)
    {
        if (actor == null || actor.IsDefeated || !actor.Definition.oreWeaponEligible || attack == null) return false;
        multiplier = Mathf.Max(IsPowered ? multiplier : 1f,
            strength > 0 ? strength : actor.Definition.oreAttackMultiplier);
        attack.SetNextSequenceModifier(this, multiplier);
        return true;
    }

    private void OnModifiersChanged(EnemyAutoAttack source)
    {
        if (!IsPowered) multiplier = 0;
        Changed?.Invoke();
    }

    private void OnDefeated(EnemyActor defeated)
    {
        if (!actor.Definition.releasesOreOnDefeat || burstConsumed) return;
        // Claim before any callback can reenter death/network work.
        burstConsumed = true;
        if (allies != null)
            foreach (var ally in allies)
                if (ally != null && ally != actor && !ally.IsDefeated)
                    ally.GetComponent<EnemyOrePower>()?.Grant();
        board?.TryQueueMineDrillPower(0, 1);
    }

    public void CaptureContinuation(EnemyCombatSnapshot saved, Func<EnemyActor, int> slotOf)
    { saved.oreNextMultiplier = IsPowered ? multiplier : 0; saved.oreBurstConsumed = burstConsumed; }

    public void RestoreContinuation(EnemyCombatSnapshot saved, Func<int, EnemyActor> enemyAt)
    {
        burstConsumed = saved.oreBurstConsumed;
        attack?.RemoveNextSequenceModifier(this);
        if (saved.oreNextMultiplier > 1f) Grant(saved.oreNextMultiplier);
    }

    private void OnDestroy()
    {
        if (actor != null) actor.Defeated -= OnDefeated;
        if (attack != null)
        {
            attack.NextSequenceModifiersChanged -= OnModifiersChanged;
            attack.RemoveNextSequenceModifier(this);
        }
    }
}
