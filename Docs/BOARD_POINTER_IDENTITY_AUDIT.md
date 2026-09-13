# Board pointer identity audit

Gameplay audit chunk 18 / validation backlog group 17.

## Status

IMPLEMENTED / SOURCE AND FINAL DIFF REVIEWED / UNITY VALIDATION DEFERRED / NOT MERGED.

Base inspected: `4e2e959186492636d44c91c33514d65aa1f724c1` (main after #134).
Branch: `fix/board-pointer-identity`.
No Unity compilation, test discovery/execution, actual InputSystem routing, touch device or Play Mode validation has run. Include this work in the user's final combined validation stage, not a separate testing/merge pass.

## Source-confirmed defect

The three Gem pointer handlers discarded PointerEventData.pointerId. BoardController retained only pointerStartGem and pointerStartPosition. A newer accepted pointer-down already replaced those fields, but the older pointer's later events were not associated with an identity.

Two consequences of the same defect:

1. Finger A presses one gem, finger B presses another, then A releases. The old EndPointer sees a gem mismatch and clears pointerStartGem, cancelling B's active gesture. B's later drag/up then has no valid starting contact.
2. Two fingers press the same gem at different screen positions. The old gem-reference check cannot distinguish their drag/up events. An older finger can use the newer finger's origin, consume its gesture or create an unintended swipe direction/tap.

These event sequences are supported by source inspection; no physical-phone reproduction or ordinary-play frequency is claimed. This is input attribution, not a demonstrated duplicate damage or reward bug.

## Smallest routing correction

- Gem forwards pointerId on down, drag and up.
- Three distinctly named pointer-aware entry points in BoardController.PointerInput associate the newest accepted down with its pointer ID before forwarding matching drag/up events to the existing logic.
- Preserve existing latest-accepted-down behavior. This is NOT a new first-finger lock, multi-swap queue or redesigned gesture recognizer.
- A rejected down cannot overwrite the current pointer ID. An old/mismatched up no longer clears a different accepted gesture.
- pointerStartGem remains the authoritative lifetime: existing accepted-swap, pin and external-input-block cleanup already nulls it. An old ID alone cannot revive a cancelled gesture.
- Existing two-argument BeginPointer/EndPointer/UpdatePointerDrag APIs remain unchanged for direct single-pointer callers/fixtures. Distinct names avoid breaking reflection that resolves the existing methods by name.
- Swipe threshold, tap selection, early-drag response, match acceptance, board locking, clear/turn reporting and rendering remain unchanged.

Only TWO production files:
- `Assets/_Game/Scripts/Board/BoardController.PointerInput.cs`
- `Assets/_Game/Scripts/Board/Gem.cs`

The final base-to-head Gem diff is only the three event-forwarding calls. An intermediate file replacement accidentally omitted unchanged special-presentation helpers; that omission was restored before the final diff review and is not present in the PR's final state. Do not validate the intermediate commit in isolation.

No production-file overlap or compile-time dependency on #135-#150. Integrate all previous audit fixes independently; existing dependency chains remain #142 -> #143 and #142 -> #146 -> #149.

## Authored tests — NOT RUN

`BoardPointerIdentityTests`: 18 NUnit cases across 13 methods.

Tests deliver PointerEventData through real Gem handlers into existing board pointer/selection logic, covering same/different-gem overlap, old up/drag rejection, preserved latest-down origin, subthreshold and consumed gestures, busy/pin/external-block cancellation, negative/positive pointer IDs, repeated replacements, no accepted down, and legacy direct APIs. Rejected/selection-only paths assert no clear or completed-turn events and no board ownership release.

The disposable hierarchy is inactive and includes its own inactive EventSystem. Outward swipes have no neighbor, deliberately avoiding swap coroutines. Pins/busy state are seeded only for guards; external input blocks use the real token API. No authored asset, preference, live singleton, clock, art or scene is modified.

These tests do NOT execute actual InputSystem device events, raycasts, legal swaps, cascades, enemy counters, UI click blocking or phone touch handling. Passing them later is not a substitute for those checks.

## Read-only reference checks

The project-level input-actions GUID in EditorBuildSettings.asset matches Assets/InputSystem_Actions.inputactions.meta. MainMenu and Game remain enabled scene entries. This does not prove the scene InputSystemUIInputModule's separately serialized action references or runtime raycaster routing; inspect those through Unity during combined validation. No input asset, scene, package or build setting was changed.

## Final combined validation backlog

1. Discover/run all 18 new cases with previous audit groups and relevant board/input/gameplay suites.
2. Use real EventSystem/InputSystem touch routing (simulated device injection is acceptable; mouse-only checks are insufficient for multi-touch). Press A, press B, release/drag A, then finish B, on different gems and on the same gem. The latest accepted contact must retain its own origin and control.
3. Verify normal one-finger/mouse tap selection, deselection, adjacent tap swaps, valid/invalid swipes, immediate drag-threshold response and exactly one completed turn after a valid resolved move. Up after a consumed drag must not trigger a second selection/swap.
4. Test overlapping touch during active cascades, Cracked Gems, pin/freeze placement, emergency reshuffle, upgrade choice, Game Over/recovery and Retry. No old contact may clear another gesture, bypass input blocks or restart board work. Preserve next-wave isolation.
5. Test application focus/pause and touch-cancel recovery with the actual input module; no gesture may remain stuck. This patch adds no new pause/input scheduler.
6. Inspect scene action bindings, Physics2DRaycaster, gem colliders and UI raycast blocking. Do not alter input assets, art, transforms, thresholds or board dimensions to make tests pass.
7. Run Tools/Validate-Unity.ps1 in the authorized final combined pass. Record real outcomes; unrun/blocked cases are not PASS.

**Bardley's cost stays at 1 energy**, intentionally approved for fast wave/enemy testing. Do not restore 80, change documented non-testing balance, or commit temporary fixture overrides.

The repository audit is still open: this pass closes pointer-event source attribution coverage, not the broader outstanding file/reference coverage reconciliation. No whole-repository bug-free certification is implied.
