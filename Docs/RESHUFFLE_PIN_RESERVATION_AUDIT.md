# Emergency reshuffle / pending pin cancellation audit — 2026-09-14

Gameplay audit chunk 17 / validation backlog group 16.

## Status

IMPLEMENTED / SOURCE AND DIFF REVIEWED / UNITY VALIDATION DEFERRED / NOT MERGED.

Base: `4e2e959186492636d44c91c33514d65aa1f724c1` (main after #134).
Branch: `fix/reshuffle-pending-pin-cancellation`.
No production-file overlap or compile-time dependency on #135–#149. Exercise this together with #141's crystal-aware reshuffle, #147's shared availability/late Miner guards and all other audit groups.

The separate dependency chains remain #142 -> #143 and #142 -> #146 -> #149. No Unity compilation, test discovery/execution, timed queue scheduling, reshuffle animations or device testing has run here. Hold for the user's final combined validation stage.

## Finding

`ReshuffleBoard` first calls `ReleaseAllPinsForEmergencyReshuffle`. That routine cleared pending pin ownership and frozen-target metadata, but left each concrete `PinRandomGem` request's `TargetGem` in the shared mutation queue.

When entered with such requests pending, the later request could pin its old gem again after recovery, even though its reservation had been released. A Court Mage request was worse: `ExecutePinRequest` inferred frozen status from the now-cleared `pendingFrozenPinTargets` set, so a surviving queued freeze could become an ordinary fixed Crossbow-style bolt, using the wrong presentation and adjacency-break rules. The gem could remain an ordinary piece or have changed into a special by execution time.

This is a source-confirmed cancellation inconsistency under that queued-state precondition, not an observed normal-play occurrence or evidence that every automatic reshuffle reaches it. The live scheduler frequency is unverified. The existing pin execution still checked board membership and legal moves; no unconditional post-reshuffle dead-board claim is made.

## Correction

When the existing emergency release cancels reservations, null the concrete targets of queued `PinRandomGem` requests before clearing their metadata. Their existing executor then exits without mutation. Keep the requests in FIFO order so the processor retains its ordinary completion and board-ownership cleanup.

Do not cancel `TopUpMovablePins`: those requests choose new safe targets at execution time rather than retaining one of the cancelled reservations. Mining, restoration, telegraphs, barricades and standards likewise keep their existing queued work. Already-materialized bolts/chains/freezes still release through their existing cleanup.

Only production file: `Assets/_Game/Scripts/Board/BoardController.Pinning.cs`. Fourteen inserted lines including comments; the reviewed diff also exposes one nonfunctional final-newline difference. No new cancellation dispatcher, resolver, pin type, damage/refund, readiness reset, cadence or obstacle-lifetime policy was added. No art, scenes, prefabs, imports or balance assets changed.

## Authored regression coverage — NOT RUN

`Assets/_Game/Scripts/Editor/ReshufflePinReservationTests.cs` and its `.meta`: **12 NUnit cases across seven methods**.

Coverage: reserved bolt/freeze invalidation; a surviving target subsequently becoming a crystal; normal failed completion once through the production mutation iterator; actual `ReshuffleBoard` entry before the first animation wait; multiple owners and the no-materialized-pin early return; existing bolt/movable-chain/freeze release; a fresh reservation surviving an older cancelled request; unrelated FIFO requests/cancellation predicates retained; empty/repeated release; no direct clear rewards or completed-move signals.

Inactive disposable objects avoid automatic startup. Fixtures construct ordinary gems and use real public reservation methods/production cancellation and execution iterators. Applied pin-family state is seeded for cleanup checks. Only the cancellation-only processor path is driven synchronously; the new valid reservation is checked as a reservation, not as a successful visual cast. RNG is restored. No PlayerPrefs, Resources asset, authored scene, live singleton or Unity clock is modified.

These checks have not been compiled or run. They do not prove actual automatic dead-board detection, timed pin impact, full reshuffling/refill or whole-scene behavior.

## Final combined validation backlog

1. In a disposable Play Mode fixture, arrange an accepted resolution with pending Crossbow/Court Mage target reservations, then enter the actual emergency reshuffle path. Let normal queue processing resume. Released targets must not be repinned, and a Mage freeze must never become a bolt. Verify the setup really includes an outstanding concrete reservation rather than an already-materialized pin.
2. Repeat with two owners and with a surviving target that moves or becomes a special. The old request must not affect the replacement state or erase a later legitimate reservation.
3. Confirm fresh post-recovery Crossbow/Court Mage casts still work, with correct pin/freeze art and adjacency rules. No refund, extra cast or cadence reset is introduced by emergency cleanup.
4. Confirm existing bolts, Captain chains and Mage freezes release as before. A distinct execution-time Captain top-up may still run afterward and must use the current legal-move guard.
5. Retain queued mining/restoration, warning resolutions and persistent obstacles. Verify complete refill, no stranded input/queue ownership, exactly-once failed completion, no clear rewards from cancellation and no next-wave damage leakage.
6. Run all 12 new cases alongside #141/#147 and earlier audit groups, existing board/obstacle/Royal/gameplay suites, and `Tools/Validate-Unity.ps1` only during the final combined validation stage.

## Confirmed user testing setting

Bardley's serialized energy cost of **1 is intentional** for accelerating waves and checking upcoming enemies. Keep it at 1 throughout audit/integration/validation unless the user later requests otherwise. It is not a bug or remaining decision. Do not rewrite the documented non-testing 80-energy balance/C# initializer. This decision is also recorded in #149's audit and PR conversation; #149's current documentation-only head is `3ebbb9c08f916efdf5160df714938677e9a24de5`.

## Read-only coverage in this pass

Inspected the slot binding/defeat-to-wave notification path and health/shield presentation through the shield-break cleanup, plus Gem initialization/special replacement and the board's reshuffle entry/candidate paths. No additional slot-level defect was established in those inspected sections. This is not a claim that every presentation file or serialized GUID is checked. The full source audit remains incomplete; the final coverage/reference reconciliation remains outstanding.
