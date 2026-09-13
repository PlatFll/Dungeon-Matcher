# Gameplay audit chunk 3 — Player ability lifecycle

Base: `4e2e959186492636d44c91c33514d65aa1f724c1` (main after PR #134).
Branch: `fix/player-ability-lifecycle-audit`.
Validation-backlog group: **2**, after group 1 / PR #135.

## Status

**IMPLEMENTED / SOURCE-DIFF REVIEWED / UNITY VALIDATION DEFERRED / NOT MERGED.**

At the user's request, do not run a separate Astra validation handoff for this PR. Add it to the combined validation state after the repository audit. No Unity compilation, EditMode execution, Play Mode test, or phone test has run for this change here. Adding tests is not a passing test result.

This branch is independent of PR #135: it starts at the same main baseline and does not modify the crystal/bomb planner files. Keep both PRs unmerged until the agreed combined validation/review stage.

## Reviewed scope

This pass inspected PlayerAbilityController, PlayerAbilityEnergy, CrackedGemsRuntime, RoyalDecreeRuntime, their definition/interface contracts, PlayerActor initialization, the AbilityButtonUI state/availability consumer, and the relevant architecture/design rules. This is a focused lifecycle pass, not a claim that every ability, menu, input path, or game-over interaction has been exhausted.

Only `Assets/_Game/Scripts/Combat/Player/PlayerAbilityController.cs` changes production behavior. No serialized fields, public API signatures, character assets, runtime implementations, board locks, balance values, or UI layout are changed.

## Defect 1 — Re-enable loses runtime state-change forwarding

`OnDisable` unsubscribed from the retained runtime's StateChanged event. On re-enable, RefreshRuntime usually found the exact same instance and returned before subscribing. Runtime completion/cancellation/target state notifications could therefore stop reaching consumers of PlayerAbilityController.StateChanged.

The current ability button polls availability every frame, so this finding does **not** prove that the entire button becomes frozen. The confirmed defect is the missing event forwarding; actual visual impact must be assessed during combined testing.

**Fix:** the unchanged-runtime path reuses the existing unsubscribe-then-subscribe helper before its refresh notification. Repeated refreshes cannot accumulate duplicate handlers. RefreshRuntime and subscription installation remain inactive while the coordinator is disabled, and normal OnEnable restores them.

## Defect 2 — Unavailable activation owners were accepted

CanActivate and TryActivate checked player health, energy and runtime-specific rules but not whether the coordinator or selected runtime component was enabled/active. CrackedGemsRuntime and RoyalDecreeRuntime do not enforce their own component-enabled state in CanActivate, so the coordinator could report readiness and invoke a disabled implementation. For Cracked Gems, the accepted resolution is run by the separate BoardController, so disabling the ability runtime alone did not prevent board work from starting.

The stored runtime is an IPlayerAbilityRuntime reference. Its ordinary null comparison also did not recognize a destroyed Unity component, leaving a stale wrapper eligible for interface calls until a refresh replaced it.

**Fix:** activation requires an active/enabled coordinator and an available runtime; Unity-object liveness is checked before invoking a cached interface. State queries, cancellation and refresh handle destroyed wrappers safely. Missing-script components are skipped during discovery. Disabled runtime components are not forcibly re-enabled or duplicated. Re-enabling the existing runtime makes it usable again without replacing it.

These are lifecycle edge cases, not a claim that a normal uninterrupted session always encounters them.

## Preserved behavior

- Spend energy only after runtime acceptance; reject without spending.
- Prevent a second activation while a runtime is active.
- Preserve existing defeat, reinitialization and disable cancellation rules; no new energy refunds.
- BoardController still owns an accepted Cracked Gems resolution until it settles. Runtime/UI cancellation must not abruptly stop that board coroutine.
- No new board-busy or modal-input policy, shield change, bomb rule, stagger tuning, targeting rule, character cost, or art/layout change.

## Deferred regression suite

Added `PlayerAbilityLifecycleTests` (15 authored EditMode tests) and the editor-only `PlayerAbilityLifecycleProbe` with both .meta files. The probe intentionally omits component-availability guards so the tests verify the coordinator, not a duplicate test implementation of its fix. Fixtures clone the current Skeleton definition/ability, use temporary GameObjects, do not load/change gameplay scenes, and do not write PlayerPrefs or source assets.

Coverage:
- accepted/repeated activation and exact energy debit;
- insufficient energy and runtime rejection without a debit;
- disabled coordinator, inactive player root, disabled/re-enabled runtime;
- destroyed runtime wrapper and replacement/rebinding;
- five coordinator enable cycles with exactly one forwarded runtime notification;
- repeated refresh, disabled refresh, completion notification;
- single cancellation without refund on disable, defeat cancellation, and reinitialization.

## Add to final combined validation

1. Compile the integrated audit state and discover/run all 15 PlayerAbilityLifecycleTests alongside the existing suites; compare relevant regressions with baseline when practical.
2. In Play Mode, test both RattleBones/Royal Decree and Bardley/Cracked Gems: funded cast, insufficient energy, repeated click, completion, defeat, and restart.
3. With temporary runtime objects only, toggle the coordinator and ability runtime separately. While unavailable the UI must not offer a cast, direct controller.TryActivate must return false, and energy/board state must not change. Re-enable and confirm casting works.
4. Toggle the coordinator repeatedly, then observe one runtime completion or cancellation notification rather than zero or duplicates. Verify destroyed/replaced runtime references fail safely and rebind correctly.
5. During an accepted Bardley sequence, confirm existing board ownership remains held through explosions, refill/cascades and completion even when runtime state is cancelled. Preserve ordinary-versus-special ability-energy entitlements and next-wave isolation from PR #134.
6. Preserve player state, preferences, art, scene serialization and rendering. Run the shared required Tools/Validate-Unity.ps1 on the final combined state; no per-PR Unity run is claimed here.

Remaining repository areas still require their own audit chunks. Do not mark the full scan complete from this PR.
