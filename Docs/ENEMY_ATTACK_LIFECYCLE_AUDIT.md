# Enemy attack lifecycle audit — 2026-09-13

Gameplay audit chunk 5 / validation backlog group 4.
Base: `4e2e959186492636d44c91c33514d65aa1f724c1` (main after PR #134).
Branch: `fix/enemy-attack-lifecycle-audit`.

**Implemented and source/diff reviewed. Unity compilation, automated execution and visual testing have NOT RUN.** The user requested one combined validation stage after the source scan. Do not run/merge this change separately now.

## Reviewed scope

Reviewed EnemyAutoAttack's complete normal/timed/follow-up/command/cancellation paths; KnightCaptainEnemyAbility command reservation and cleanup; KingEnemyAbility assault ordering and cleanup; PlayerActor defeat/revive API; the relevant GameOver presentation entry path; and existing architecture / Royal command requirements. This does not certify every enemy special or the complete game-over UI lifecycle.

## 1. Automatic attack loop was not restored on re-enable

`OnDisable` calls `StopAttacking`, but EnemyAutoAttack had no `OnEnable` restart. An already initialized automatic attacker could be re-enabled and remain inert until some separate caller explicitly restarted it. Initializing an inactive object had the same problem: startup was rejected while inactive and later enabling did not retry.

**Fix:** OnEnable attempts the existing TryStartAttacking path only when attackAutomatically is configured. Existing actor/player initialization and defeat guards reject premature/dead startup. Existing coroutine guard prevents duplicates. Manual mode remains manual; an explicit StopAttacking on an enabled object does not start a per-frame restart loop. Re-enable uses the existing cold-start policy (including waitBeforeFirstAttack), not a new cooldown balance rule.

This is a confirmed lifecycle path defect, not evidence that every normal encounter currently toggles its enemy components.

## 2. Command resume state was not completely reset

ReleaseCommand deliberately sets resumeCooldown when resuming a reservation. StopAttacking cleared the owner and timer but left resumeCooldown, commandMadeReady, commandStrike, reservedAttackTime and temporary command launch fields intact. A staged resume can survive a full stop/reinitialization, causing AttackLoop to retain a zero timer instead of establishing the normal initial interval.

A bounded reproduction is an attacker configured for manual startup: reserve/release a command to stage a cooldown resume, fully stop, then initialize/start again. The old resume flag must not survive a new attack lifecycle. This is an edge case; no claim is made that it happens on every player revival.

**Fix:** distinguish full stop/reset from legitimate command release. Full stop clears all temporary reservation/launch state. Normal ReleaseCommand retains its existing unspent/consumed cooldown semantics and persistent damage/speed/blessing dictionaries are not changed by StopAttacking.

## Related ownership guards

- TryStartAttacking now refuses to restart the normal countdown while a command still reserves it. Previously an external/repeated startup call could run that countdown beneath the reservation.
- Full stop is guarded while it releases the actor's animation action. That release invokes listeners synchronously; reentrant start/reserve/attack requests cannot acquire new attack ownership inside the ongoing stop. Reentrant Stop is harmless.
- Null is never a command owner. Previously ReferenceEquals(null, null) let the unreserved state pass IsCommandReservedBy(null), PerformCommandStrike(null) and ReleaseCommand(null). Normal King/Captain callers use non-null owners; this is API-contract hardening, not a claim they passed null during ordinary play.

## Production scope and unchanged behavior

Only `Assets/_Game/Scripts/Combat/Enemies/Runtime/EnemyAutoAttack.cs` changes production behavior. No serialized fields are renamed. No damage, attack interval, blessing multiplier, shield, stagger, follow-up delay, command windup/spacing, board lock, wave selection, art, prefab, scene or import changes.

Normal command release still restores an unspent reservation's stored cooldown; a consumed command restarts the full cooldown after its existing sequence/return requirements. Pending-hit IDs, one-shot impact checks, existing timeout fallbacks and two separate follow-up damage instances remain intact. This does not redesign whether an already-started attack is interrupted by stagger.

Existing player defeat/revive subscriptions are retained. No new revival feature or game-over overlay reset is implemented. Full GameOver presentation/revival integration remains a separate audit area; actor-level revival tests do not certify that UI.

## Deferred validation

New opt-in menu: `Dungeon Matcher > Validation > Enemy Attack Lifecycle`.

The validator contains **14 scenarios**, not 14 NUnit tests. It enters a disposable Game-scene Play Mode session with scene reload enabled. Synthetic actors are kept outside the real wave roster and source data/PlayerPrefs are untouched. Runtime stats are injected only as fixture setup to avoid installing optional enemy artwork; EnemyAutoAttack.Initialize, its actual Unity coroutines, impact APIs, PlayerActor damage/revive and EnemyActor defeat APIs are exercised. Hit return acknowledgements are explicit test callbacks, not actual animation proof. A long fixture-only timeout prevents missing synthetic art from firing the fallback during checks. Actual animations/timeouts require the final encounter regression pass.

Scenarios:
1. Component disable/re-enable restarts automatic attacks.
2. Whole fixture-root disable/re-enable does the same.
3. Manual mode stays stopped; explicit start/stop remains usable.
4. Initialization while disabled starts normally on later enable.
5. Five enable cycles and repeated starts keep a single coroutine.
6. Normal reservation freezes its timer and rejects unrelated restart/release.
7. makeReady reservation restores its unspent stored timer on release.
8. Staged command resume is cleared by full stop/reinitialization.
9. Synchronous actor-action release cannot start/reserve/hit during full stop.
10. Null owner operations cannot acquire or release commands.
11. Player death cancels a pending command; revive restarts cooldown; old hit IDs are rejected.
12. Enemy defeat prevents restart and invalidates pending hit IDs.
13. A completed single-hit command retains impact/return ownership and avoids an extra stored attack.
14. A completed two-hit command retains two distinct hits/IDs/returns and restarts normal cooldown only afterward.

Final combined validation must also use real Captain and King encounters: cancel during windup, between hits and during return; verify dead allies are skipped, unspent allies resume stored cooldown, consumed allies restart cooldown, specials do not steal reserved actions, blessing affects both eligible hits once, and existing stagger/board-wait behavior is preserved. Verify normal spawn/retry and actor revival separately from any game-over UI limitations.

Run shared gameplay/Royal/Captain/stagger/bomb regressions, previous audit groups, this menu validator and `Tools/Validate-Unity.ps1` on the final integration state. Do not mark an unrun or blocked check PASS. Fix fixture defects without weakening intended gameplay assertions.

## Integration

Independent branch off the same validated baseline as #135, #136 and #137. Its production file does not overlap those PRs. Integrate all pending audit PRs before the final combined validation; leave main unchanged until that stage is approved. The full repository source audit is still incomplete.
