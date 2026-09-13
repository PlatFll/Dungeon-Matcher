# Royal Assault participant lifetime audit

Gameplay source audit chunk 15 / combined-validation backlog group 14.

Base: `4e2e959186492636d44c91c33514d65aa1f724c1` (main after PR #134).
Branch: `fix/royal-assault-waiting-participants`.

## Status

IMPLEMENTED / SOURCE AND DIFF REVIEWED / UNITY VALIDATION DEFERRED / NOT MERGED.

Compilation, NUnit discovery/execution, actual command scheduling, death presentation, device rendering and `Tools/Validate-Unity.ps1` have NOT RUN. The user requested a single combined validation stage after the audit; no individual validation stage is claimed here.

## Finding

`KingEnemyAbility.Assault` checks a participant once before waiting for the player's board resolution or that participant's stagger. The wait originally checked only owner cancellation, board busy and stagger. After the wait it checked the King, but did not recheck the waiting participant before using its actor and attempting its commanded attack.

A participant can die during an ongoing clear/poison tick and finish its death presentation while the command is suspended. The old path could keep waiting on a dead participant and then continue with a no-longer-valid participant reference. An unbound/null actor is an unguarded dereference in that continuation. Destroyed Unity objects can retain managed fields, so destruction alone is NOT proof that every occurrence throws. No ordinary-play crash rate, observed boss freeze, or guaranteed softlock is claimed.

The approved Royal Assault specification in `Docs/ROYAL_SPECIALS.md` says dead participants are skipped, living participants newly staggered during windup wait, and player board resolution finishes before the next commanded sequence starts. This fix consistently applies that distinction across the existing coroutine yield boundary.

## Correction

Only `Assets/_Game/Scripts/Combat/Enemies/Runtime/KingEnemyAbility.cs` changes production behavior:

- While waiting, require both the attack component and its actor to remain present, and the actor to remain undefeated.
- Recheck those conditions immediately after waiting, before releasing the actor action or attempting its command strike.
- Skip the lost participant; each later living participant still performs its own existing board/stagger wait.
- If no eligible participants remain, use the existing command completion/participant-release path. Do not release the board's busy flag.

No new coroutine, attack engine, pause manager, reservation model or pooling policy was introduced. The command windup, spacing, roster order, sequence multiplier, both-hit handling, blessings, Enrage, reinforcement thresholds, special cycle and normal cooldown handling are unchanged. The Captain implementation was inspected but is not modified by this PR.

## Deferred automated coverage

`Assets/_Game/Scripts/Editor/RoyalAssaultParticipantLifetimeTests.cs` contains **13 authored NUnit cases across nine methods**, with its `.meta` file.

Coverage:

- participant root destroyed after entering a busy-board wait, with the board still busy or subsequently idle;
- actual actor defeat during a board/stagger wait;
- missing actor reference and removed attack component during a wait;
- living participant still blocked by board, stagger or both;
- a later living participant remains reserved and waits independently after an earlier participant is lost;
- command owner cleanup during the wait does not commit successful cycle advancement;
- already-dead/destroyed participants retain the existing pre-wait skip behavior;
- final command bookkeeping, unspent reservation release and no command-owned changes to board busy/player HP.

These are synchronous EditMode fixtures. They explicitly seed inactive disposable actors, the remaining participant list and reservation state, and manually advance the actual `Assault` IEnumerator. They call real actor damage/action and command-release APIs where applicable. A live survivor in this inactive fixture intentionally rejects attack execution; that case checks continuation and cleanup, NOT a successful commanded strike. No `StartCoroutine` scheduling, real death VFX, whole command acceptance, timed damage, normal cooldown progression or frame ordering has been validated.

No PlayerPrefs, authored assets, live singleton, global RNG or clock state is modified. Do not alter assertions merely to get PASS; fixture failures and production failures must be distinguished in the final pass.

## Additional read-only inspection

Reviewed `RoyalBannerAuraRuntime`, the King's full runtime and the Captain command runtime against `ROYAL_SPECIALS.md` and the existing architecture. Inspected the current King's serialized command values and Royal Lancer participation/follow-up fields. The King's reinforcement reference to the Lancer matches the Lancer asset's `.meta` GUID. No data tuning or asset-reference changes were made. This is targeted reference coverage, NOT a claim that all repository GUIDs, prefabs or imported objects have been verified.

## Integration

No production-file overlap or compile-time dependency on PR #135 through #147. Behavioral regression coverage must include #138 attack lifecycle, #144 encounter/death cleanup, #145 recovery and #147 shared special availability. PR #143 and #146 remain separately dependent on #142.

## Final combined Play Mode checks

1. Run a real King Assault with eligible Royal allies. Start a long board clear during windup, then defeat a waiting participant and let its corpse actually disappear before the board settles. No attempted strike from the lost participant, stale wait or command-lifetime error.
2. Repeat with a newly staggered participant that dies while waiting; also test a surviving staggered participant. Surviving participants must wait rather than be skipped or attack early.
3. With another living participant later in the snapshot, confirm it acts once after its own readiness conditions clear, in original roster order. Verify the actual full two-hit Lancer sequence and blessed damage/consumption.
4. With no remaining living participants, confirm command bookkeeping completes once without unlocking an unresolved board. The King's subsequent special cycle and normal cooldown still work.
5. Cancel by King death/disable during windup, wait, either hit and return. No stale impact, leftover reservation or unwanted normal extra attack. Reuse the existing actor/command cleanup semantics; do not invent component suspension behavior.
6. Exercise actual death/revival/Retry and old-wave isolation with the integrated audit branches. No energy rewards or refunds are added by this fix.
7. Run all 13 cases plus previous audit suites, Royal Milestone/Play Mode and relevant Captain/attack tests, then `Tools/Validate-Unity.ps1` in the final combined validation stage.

Full repository source audit remains incomplete. The final coverage reconciliation and broader serialized-reference sweep are still outstanding.
