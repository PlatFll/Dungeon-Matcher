# Enemy damage-result audit — 2026-09-13

Gameplay audit chunk 4; validation backlog **Issue Group 3**.
Base: `4e2e959186492636d44c91c33514d65aa1f724c1` (main after PR #134).
Branch: `fix/enemy-damage-result-audit`.

**IMPLEMENTED / SOURCE-DIFF REVIEWED / UNITY VALIDATION DEFERRED / NOT MERGED.**
The user requested one combined validation pass after the remaining source audit, not individual validation runs for each PR. Add this branch alongside #135 and #136 to that later integration state. Do not merge now.

## Scope reviewed

The EnemyActor damage/mitigation/interception/defeat path; EnemyPoisonStatus application and tick reporting; EnemyStagger's HP/shield input subscriptions; normal gem-clear damage and Royal Decree consumers; relevant damage modifiers and documented Marshal/Sergeant/shield rules. Auto-attack timing and complete enemy-specific kits are not certified by this chunk.

## Confirmed reporting defect

EnemyActor already redirects a direct hit to the Marshal's living protector, with at most one hop. The defender loses shield/HP through its own damage path; poison bypasses interception.

CombatController and RoyalDecreeRuntime instead measured the originally selected actor's HP before/after that call. When the protector received damage:

- CombatController emitted EnemyDamagedByGemClear with the protected actor and zero HP loss, even though the protector lost HP.
- Royal Decree calculated zero HP loss and omitted HitResolved entirely.

This is a **wrong-recipient/wrong-amount hit-reporting defect, not evidence that intercepted damage itself was missing**. The old bool result was already true for a successful intercepted or shield-only hit, so the clear's successful-hit/energy classification must not be demoted or rewarded a second time.

## Fix and ownership

Add the immutable EnemyDamageResult snapshot with Recipient, HealthDamage, ShieldDamage and Applied. EnemyActor produces it from its existing damage calculation and passes the receiving actor's result back through one-hop interception. The snapshot is captured before HP/defeat listeners can change health or clear redirection.

New result-returning entry points:

- EnemyActor.ResolveDirectDamage(int)
- EnemyActor.ResolveDamageWithoutFeedback(int)

The existing public TryTakeDamage(int) and TryTakeDamageWithoutFeedback(int) bool signatures are retained as wrappers over the same path. There is no second health mutation pipeline, stored global last-hit state, new damage event, or caller-side redirection implementation.

CombatController and Royal Decree now report the actual recipient and captured HP loss. Their source/gem contexts retain the original board clear; a protector need not share that gem color. Royal Decree's mark/target selection is not reassigned merely because a protector intercepted the hit.

EnemyPoisonStatus uses the same outcome for tick HP reporting instead of recomputing net health after callbacks. Synchronous healing or another hit in a listener can otherwise make a before/after subtraction under- or over-report the original hit. This callback-isolation coverage is defensive hardening; this audit does not claim such a callback is currently observed in ordinary poison play.

## Contracts deliberately unchanged

- Weakness eligibility, selected-target upgrade calculation and actor-side mitigation order.
- One-hop interception, invalid/dead-protector fallback, no overkill spill back to the protected actor.
- Enemy shield cap 30, 25% shield reduction, ceiling rounding, reduced overflow and unreduced subsequent hits.
- Conditional defence is evaluated by the receiving actor at damage time.
- Existing actor event ordering, defeat/survival notifications and damage-feedback suppression.
- Combat EnemyDamagedByGemClear still reports HP loss: a successful shield-only hit reports zero HP, now with the correct recipient.
- Royal Decree HitResolved and poison TickDamageApplied remain HP-only; shield-only hits do not acquire new HP feedback events.
- Poison still bypasses interception and does not emit DamageReceived/ShieldDamaged stagger inputs.
- Bomb rewards, mastery, ability-energy amounts, targeting, durations, art, scenes, prefabs, serialized tuning, layout and pixel imports.

Target-conditional run-upgrade bonuses are still evaluated against the selected target before interception, as they were before this change. Whether those conditions should instead follow a protector is a separate design question; this reporting fix does not silently move them or apply them twice.

## Deferred tests

Added **24 authored EditMode cases** in EnemyDamageResultTests. They use inactive disposable GameObjects and in-memory definitions, configure only isolated gameplay state, and invoke the real actor/combat/ability methods. They do not run an encounter, render UI, or validate real-time poison timing. No saved assets or PlayerPrefs are changed.

Coverage:

- Direct HP, shield-only, exact shield break and shield overflow; separate unchanged actor events.
- Following unshielded hit and conditional defence preceding shield exactly once.
- Actual one-hop receiver, no second redirect, protector defeat/retreat cleanup, bounded overkill.
- Nonpositive damage, uninitialized/defeated recipients, and compatibility bool entry points.
- Per-hit snapshot isolation from simulated synchronous healing and additional damage.
- Suppressed damage bypass with no stagger-input events and intact health/shield state notifications.
- Combat recipient and original context, one hit report, shield-only success, wrong-weakness/inactive-wave rejection.
- Royal Decree redirected HP reports, unchanged mark, unchanged shield-only HP-event policy.
- Poison recipient/HP reporting, interception bypass and unchanged shield-only feedback.

## Final combined-validation checklist

1. On the final integrated audit state, compile and discover/run all 24 EnemyDamageResultTests. Reproduce the wrong-recipient cases against the baseline when practical.
2. In real Play Mode, summon a Marshal protector, clear the Marshal's weakness and use Royal Decree while he is protected. Check actual HP/shield, the recipient in hit events, hit cues, successful clear/energy outcome and exactly-once application. Do not require shield damage to appear as HP damage.
3. Kill the protector; the lethal result must name it, retreat must end, and subsequent damage must reach the Marshal. Preserve wave completion and independent summon lifetime.
4. Apply poison to a protected Marshal and a shielded enemy. Check damage stays on its poisoned owner, shield overflow/rounding is unchanged, ticks do not build stagger, and poison refresh/expiry/death cleanup work normally. This chunk did not change tick timing.
5. Run existing shield/Marshal/Sergeant/Royal/poison and stagger tests where available, plus the shared Gameplay Bomb Rewards, Gameplay Edge Cases, Gameplay Supplementary Cases and audit-branch tests.
6. Run Tools/Validate-Unity.ps1 under the repository workflow at the final validation stage. Keep pre-existing shader warnings separate; do not label unrun/blocked checks PASS.
7. Preserve local user work. Fix real regressions without changing balance or weakening assertions; review final integration diff before any authorized merge.

No Unity compilation, EditMode execution, Play Mode or visual testing was performed in this GitHub-only audit chunk. The full repository audit remains incomplete.
