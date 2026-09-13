# Upgrade intermission lifecycle audit

Gameplay audit chunk 10 / validation backlog group 9.

## Status and dependency

IMPLEMENTED / SOURCE AND DIFF REVIEWED / NOT UNITY VALIDATED / NOT MERGED.
Compilation, NUnit discovery/execution, Play Mode, rendering and device tests have NOT RUN. The user requested one combined validation stage after the repository audit; do not launch a separate validation stage now.

Main inspected: `4e2e959186492636d44c91c33514d65aa1f724c1` (after #134).
This branch is stacked on **PR #142**, head `93d0582ab4fadde089da606d9690ba0cc03063b9`, branch `fix/run-upgrade-lifecycle-audit`. It consumes that PR's authoritative `RunUpgradeRuntime.RunReset` event instead of inventing another run-reset authority.
Branch: `fix/upgrade-choice-intermission-lifecycle`.

The incremental production diff is only `RunUpgradeCoordinator.cs` and `UI/RunUpgrades/UpgradeChoiceUI.cs`. Integrate #142 before this change. If #142 is amended, bring its final version forward and review the combined diff; the opening SHA is not a permanent test target.

## Source findings

### 1. Repeated UI configuration can leave an invisible intermission lock

`RunUpgradeBootstrap` may revisit an installed system. Previously `UpgradeChoiceUI.Configure` unconditionally called `Hide`, clearing the selection delegate, even for the same Canvas. The coordinator retained its progression flag and disposable board-input token. If the choice was already displayed, its open coroutine had finished: nothing reopened it or released the hold.

Correction: same-Canvas configuration preserves an existing view and its choice. Same-reference coordinator configuration is also idempotent. A genuinely changed coordinator binding cleans up the OLD UI/board/token before replacing references.

This is a repeat-installation/lifecycle path, not a claim that every ordinary wave triggers it.

### 2. Hidden/unavailable views and new runs did not revoke coordinator ownership

The UI's public Hide cleared the callback but did not inform the coordinator. Disabling the UI component also did not revoke its child buttons' callbacks. The coordinator listened for player death, but not player reinitialization or explicit run reset. A held old choice could therefore outlive a same-scene run boundary, or leave an invisible modal gate after a view was hidden.

Correction: UI Hide publishes one idempotent Hidden notification after clearing its state. Disable/destroy and observed external overlay deactivation use Hide. The coordinator releases its own token when its view disappears and invalidates old selections on player initialization, defeat, explicit RunReset, changed binding or disable.

Cancellation follows the coordinator's pre-existing disable/failure policy: abandon the interrupted choice without awarding a card, release its gate, and let WaveController decide progression. No cancel button, free reroll, automatic card choice or new advancement path is added. Same-reference installation does NOT count as cancellation.

### 3. Selection lacked a session/offer boundary

The coordinator only tested a general holding flag and runtime eligibility. It did not retain which definitions were actually offered or associate the callback with one specific choice. It also set selectionCommitted AFTER TryApply, although TryApply invokes synchronous UpgradeChanged callbacks.

Correction: retain the actual offered definitions and an opaque per-choice session; reject stale callbacks, unoffered definitions, inactive coordinators, invalid player state and mismatched/active encounters. Reserve selection before TryApply's callbacks; restore selectability after a normal rejected application. An older returning callback cannot release a newer choice's gate or hide/re-enable a replacement UI session.

Ordinary rapid clicks were already guarded by UpgradeChoiceUI.selectionPending and button interactability. The additional reentrancy/retained-callback tests are defensive boundary coverage, not evidence that ordinary double-clicking previously awarded two upgrades.

UI Show also rejects empty/null entries, missing handlers and unavailable components without leaving an empty blocking overlay. This is API-contract coverage; the normal generator already avoids empty offers in the coordinator.

## Preserved behavior

- Exactly one accepted offered card, existing drafting/eligibility/stacking, five-wave cadence and card values.
- WaveController remains the only wave advancement/spawn authority. Its #134 settle-first gate is unchanged.
- No changes to HP/shield storage, energy, bombs, abilities, stagger, encounter RNG, PlayerPrefs/mastery, scenes, prefabs, artwork, card dimensions/colors or pixel imports.
- External input tokens remain reference-counted: abandoning this choice does not release another system's block.
- #142's repeated runtime binding retains cards, HP baseline, draft stream and mechanic bookkeeping.

## Deferred automated coverage

`UpgradeIntermissionLifecycleTests`: **31 authored NUnit cases across 23 test methods**, including parameterized cases. NOT DISCOVERED OR RUN HERE.

Coverage: normal acceptance and repeat rejection; synchronous callback reentry; unoffered/newly-ineligible definitions; same-run rebinding; changed board binding; explicit reset and player initialization/death; idempotent Hidden notification and unrelated token preservation; UI component/root disable, destruction and external overlay hiding; disabled coordinator rebinding and five enable cycles; reset while waiting for the board; replacement intermissions/views during callbacks; retained old card callbacks; invalid Show inputs; one/two/three choices; mismatched/active wave rejection; UI rejection/retry.

Fixtures create disposable actors and definitions and a minimal Canvas/button hierarchy, avoiding actual font/art generation. They use the real draft, Show/Bind/Hide, selection, TryApply, reset and actor APIs. To remain EditMode-safe they inject HandleWaveCompleted's held-state fields and manually advance the existing open coroutine at yield boundaries; button clicks invoke Button.onClick rather than physical pointer input. These tests do not prove real wave scheduling, generated UI layout, rendering or touch behavior. They do not modify authored assets/PlayerPrefs and do not destroy a live singleton to satisfy fixture setup.

## Final combined validation backlog

1. Integrate #142 before this branch, alongside #135-#141 and later audit fixes. Verify final refs and all overlapping edits before testing.
2. Discover/run all 31 cases, #142's 20 cases, existing six RunUpgradeSystemTests and the shared audit regressions.
3. In real Game Play Mode reach waves 5/10, choose one card and confirm exactly one stack, hidden modal, released input and one next-wave transition only after board settlement. Test a multi-cascade wave-ending clear.
4. Repeat same-run bootstrap/configuration while the actual choice is visible: preserve the same choices, callback, cards, draft state and held input. Do not reroll or hide it.
5. Test UI/component/Canvas/overlay disable, explicit Hide, destruction, coordinator disable, changed dependency binding, actor initialization and explicit RunReset while waiting/open. No invisible lock, stale selection or free card. Preserve other owners' input tokens.
6. Test rejection and rapid taps using actual pointer input. Confirm a rejected choice does not consume the opportunity, and callback reentrancy cannot apply a second card. Confirm stale callbacks cannot affect a later intermission.
7. Recheck Game Over/scene Retry, real card layout/pixel presentation and no interference with bomb/stagger/wave fixes. Run required Tools/Validate-Unity.ps1 on the final integrated state following AGENTS.md.

Source review also followed WaveController's spawn loop, death completion and progression gate boundary to identify the intermission's context. No spawn-loop change is included. The deeper partial-spawn, stale death-callback, summon and Game Over audits remain outstanding; the full repository audit is not complete.
