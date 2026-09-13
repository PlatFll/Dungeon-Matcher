# Run upgrade lifecycle audit — chunk 9 / backlog group 8

Base: `4e2e959186492636d44c91c33514d65aa1f724c1` (main after PR #134).
Branch: `fix/run-upgrade-lifecycle-audit`.

**IMPLEMENTED / SOURCE REVIEWED / UNITY VALIDATION DEFERRED / DO NOT MERGE.**

The user requested a single integrated Unity validation stage after the repository source audit. Compilation, NUnit discovery/execution, Play Mode and device tests have **NOT RUN** for these changes. Added tests are not passing results.

## Reviewed scope

Read the run-upgrade bootstrap, runtime, coordinator, definition/catalog/types, draft generator, resolver and gameplay hooks; existing RunUpgradeSystemTests; board upgrade-scope signals; player HP/initialization and energy storage; the actual Thicker Hide asset; and relevant design, progression and architecture contracts. Numeric modifier order, wave-choice cadence and immutable upgrade data remain unchanged.

This pass fixes lifecycle consistency rather than retuning card strength. It does not certify every card combination, UI intermission race, extreme numeric bound, or additive-scene setup. Detailed remaining mechanic-specific energy interactions and whole-scene death/retry behavior remain audit work, not implied PASS results.

## 1. Upgrade event effects stopped after the hooks were re-enabled

`RunUpgradeGameplayHooks.OnDisable` removed all subscriptions. `OnEnable` restored only its static Current pointer, not those subscriptions. Previously configured hooks could therefore stop receiving wave rewards, crystal bonus events, shield-break events, cracked damage adjustments and special-clear scope notifications after re-enable. Resolver-only numeric modifiers could still work, making the loss partial rather than all cards disappearing.

**Fix:** reconnect idempotently through the existing Subscribe method and reject subscription installation while disabled. A same-wave enable does not reset once-per-wave usage or replay energy/shield rewards. Resynchronize the observed shield without treating it as a shield break. Discard an unfinished special-clear cache rather than applying stale Bombsmith/Chain Reaction scope to a later clear.

These are explicit component lifecycle cases. No claim is made that ordinary uninterrupted encounters routinely disable this component.

## 2. Same-scene upgrade reset forgot ownership but left temporary maximum HP

`RunUpgradeRuntime.ResetRun` cleared the selected cards and erased the cached base maximum, but did not undo their applied maximum-HP contribution or notify the mechanic observer. Example with a 100-HP fixture: Thicker Hide raises max HP to 120; old ResetRun leaves 120 while reporting no owned cards; a later application can treat the boosted maximum as the unmodified base. Hook counters could likewise survive an explicit reset within the same wave.

**Fix:** preserve the original baseline long enough to restore it through `PlayerActor.SetMaximumHealth(..., healAddedAmount: false)`, clear ownership/RNG, and publish RunReset. Reset clamps HP if necessary; it does not heal damage, revive, grant/remove shield, change energy storage, spawn a wave or reset mastery. RunRevision lets re-enabled hooks recognize a reset they did not observe while disabled.

Player.Initialize is a different boundary: the actor has already installed the NEW character/override maximum. That callback clears prior card ownership without restoring the old baseline over the new value, then publishes the reset notification. Initial configuration before Player.Initialize remains supported.

This fixes explicit same-scene reset/rebind paths. Normal scene reload already constructs a new run; it is not evidence that every ordinary Retry previously kept cards or extra HP.

## 3. Repeat binding could erase cards or recapture boosted HP

The bootstrap can call EnsureInstalled from both scene-load and initial-load callbacks, and on later scene loads. Runtime.Configure always reset ownership, while hooks.Configure reset mechanic counters and captured current (possibly boosted) HP again. A repeat binding of an existing run could erase cards/reseed its draft, rearm once-per-wave effects, or let an unrelated later upgrade compound an HP bonus after hook rebinding.

**Fix:** repeated Configure calls with the same references only ensure subscriptions. Explicit ResetRun / player reinitialization / rebinding a different run remain reset boundaries. When hooks bind to a changed dependency within the same run, they read the runtime's unmodified BaseMaximumHealth instead of adopting boosted current maximum HP. Rebinding the runtime to another player restores the old bound player's baseline before capturing the new player's baseline.

This is a binding-contract correction, not a new additive-scene architecture. It neither creates nor certifies support for multiple simultaneous gameplay runs.

## Files / preserved behavior

Production changes are limited to:
- `Assets/_Game/Scripts/RunUpgrades/RunUpgradeRuntime.cs`
- `Assets/_Game/Scripts/RunUpgrades/RunUpgradeGameplayHooks.cs`

The existing resolver, TryApply modifier arithmetic, hook reward constants, card definitions, weighted draft algorithm and encounter RNG are unchanged. No scene, prefab, art, import, mastery preference, shield balance, stagger or board-resolution changes. No production-file overlap with PR #135–#141 at the reviewed baseline.

## Authored EditMode coverage — 20 cases, NOT RUN

`RunUpgradeLifecycleTests` creates synthetic ScriptableObjects and disposable actors. It exercises real Configure/ResetRun/Initialize/HP/shield APIs and component enable/disable callbacks. Wave and clear notifications are deliberately delivered through their event delegates; this does not substitute for live spawning or actual bomb rendering.

Cases cover:
- reset/reapply with and without active hooks; no HP compounding or free healing;
- HP clamping with shield/energy unchanged; no revival;
- new HP overrides with either player-initialization observer order;
- repeated binding preserving cards, draft stream and same-wave counters;
- different-player rebinding and disconnection of the previous actor;
- exactly one reset notification and independent draft reseeding;
- reset while hooks are disabled and later state reconciliation;
- one/five enable cycles with exactly one subsequent wave reward;
- disabled configuration not reconnecting rewards;
- Emergency Plating remaining once-per-wave across re-enable;
- stale clear-scope cleanup and fresh scope reception;
- exactly one Chromatic Conductor bonus delivery after re-enable;
- explicit reset clearing per-run mechanic caches without a wave event;
- hook rebinding not adopting upgraded HP as its base;
- flat/additive/multiplicative stacking at one/two stacks, unchanged after reset.

Test reflection and synthetic state are confined to Editor code. No PlayerPrefs, authored assets, gameplay RNG or live run is modified. The tests require EditMode with no active RunUpgradeRuntime/RunUpgradeGameplayHooks singleton; do not destroy another run to satisfy that precondition.

## Final combined validation backlog

Add this branch to the integration state with #135–#141. Do not validate/merge separately now.

1. Compile and discover all 20 new cases, plus the existing six RunUpgradeSystemTests and all previous audit groups.
2. Play a real run with Thicker Hide, Prepared Casting, Chromatic Conductor, Emergency Plating and Cracked Gems upgrades. Temporarily disable/re-enable hooks in an isolated Play Mode fixture; verify subsequent effects still occur once and no reward is replayed simply by enabling.
3. Exercise same-wave re-enable, wave advance while hooks are disabled, explicit ResetRun, repeated same-run Configure, different dependency rebinding, player reinitialization with another HP override/character and scene Retry.
4. Use valid current/max HP fixtures. Do not fix a test by changing production card values, adding impossible HP, weakening assertions or granting free health/energy.
5. Verify genuine new runs clear temporary cards; ordinary wave transitions and actor revival are not silently redefined as new runs. Preserve mastery/preferences and encounter RNG isolation.
6. Check actual bomb/ability damage, all energy entitlements, HP/shield UI, exactly-once awards, old-wave/new-wave isolation and no stale intermission/input lock. Prior bomb/stagger tests remain required.
7. Run `Tools/Validate-Unity.ps1` using the repository workflow during the final authorized combined pass. Record exact tested integration commit and distinguish unrun/blocked from PASS.

Known Aegis Reservoir grant/cap discrepancy remains a separate balance decision and is unchanged. Mechanic-specific bonus-energy composition was not rebalanced in this lifecycle patch.
