# Enemy-special execution guards / menu transition source pass

Gameplay audit chunk 14; validation backlog group 13.
Base: `4e2e959186492636d44c91c33514d65aa1f724c1` (main after #134).
Branch: `fix/enemy-special-execution-guards`.

**IMPLEMENTED / SOURCE AND DIFF REVIEWED / UNITY VALIDATION DEFERRED / DO NOT MERGE.**
The user requested one combined validation stage after the source audit. Compilation,
NUnit discovery/execution, real animation timing, Play Mode and device tests have NOT RUN.
These are source-confirmed failure paths, not observations of their runtime frequency.

## 1. Disabled special runtimes can still execute through retained delegates

`EnemySpecialActionAvailability.CanRetryReadyAbility` validated the actor, board and
ready state, but not `coroutineHost.isActiveAndEnabled`. That check existed only in
retry-coroutine creation. Several existing consumers (Miner, Crossbow Guard, Court
Mage, ordinary Barricade and Shielding Allies runtimes) keep actor/board subscriptions
until defeat/destruction. Their readiness/action callbacks can therefore enter the
synchronous execution path while the component or its GameObject is disabled.

For example, the Shielding Allies runtime can receive the actor's actual readiness
event while disabled and grant ally/self shields; it does not need its own Update or
a new coroutine to do so. Board-mutating consumers can request work from the separate
BoardController host instead.

**Fix:** require an active/enabled host in the existing shared retry/execution guard.
The callback is rejected without spending readiness, acquiring an action or queuing a
new mutation. The same guard also protects an already-created deferred poll. Disposal,
actor validity, stagger and board-idle rules are unchanged. After re-enable, existing
legitimate readiness/action/move retries remain available; this does not introduce a
new OnEnable cast, refund or universal suspension/recovery scheduler.

This is a lifecycle edge case, not a claim that standard encounters deliberately
disable their special components. The change does not cancel an already-accepted
board operation or change persistent-obstacle lifetime on disable.

## 2. Miner can mutate the board after owner loss at the impact-wait boundary

`ExecuteMineRequest` checked owner death before and inside its animation wait. If the
impact flag became true and the owner died/was destroyed before the next continuation,
the while condition became false and the in-loop check never ran again. It could create
a new hole for a dead/missing Miner. The normal defeat cleanup may already have seen
zero owned holes; later destruction may restore a temporarily created hole, but a
permanent leak or its frequency is not assumed here.

**Fix:** recheck the existing null/defeated owner conditions immediately after the wait,
before target selection, RNG consumption, events or cell mutation. This also covers
owner death caused by an action-release observer on the existing timeout path.
Preserve the timeout, animation impact API, cap, selection and environmental settlement.
The outer mutation processor still owns busy state and serial cleanup; a canceled
request does not unlock or interrupt other accepted board work.

## Scope

Only two production files change:
- `Assets/_Game/Scripts/Combat/Enemies/Runtime/IEnemySpecialAbilityRuntime.cs`
- `Assets/_Game/Scripts/Board/BoardController.Mining.cs`

The production diff adds one host-availability condition and one post-wait owner guard,
plus comments. No serialization, art, UI appearance, card values, damage/shield balance,
stagger tuning, cadence, mine cap, warning rules, mastery or random-stream policy changes.
No production-file overlap/compile-time dependency on #135-#146. Integrate behavioral
regressions with those groups; #143 and #146 still separately require #142.

## Authored regression coverage — NOT RUN

`EnemySpecialExecutionGuardTests`: 19 NUnit cases across eight methods.
- disabled component and inactive root cannot execute or consume readiness;
- one/five re-enable cycles followed by an explicit legitimate retry;
- disposed, defeated, destroyed-host, uninitialized and not-ready guards;
- deferred poll after disabling its host;
- actual Shielding Allies readiness/action-release path and exact shield amounts;
- dead/destroyed Miner after acknowledged/unacknowledged impact wait;
- living immediate/timed mining still commits once, including repeated impact signals;
- another owner's animation impact cannot release the active request.

Fixtures seed disposable actor gameplay fields, use actual readiness/shield/damage and
queue/impact APIs, and manually advance coroutine boundaries. They do not start the
board scheduler or execute shatter/refill. The helper is accessed by reflection because
it is internal to the runtime assembly. RNG is restored; no PlayerPrefs, authored data,
live singleton or saved scene is modified. Existence of the tests is not a PASS.

## Menu source coverage (no saving change made)

Read MainMenuController, CharacterSelectMenuController, GemMasteryMenuController and
GemMasteryLoadout against the previously reviewed settings/registry/resolver paths.
Checked EditorBuildSettings: MainMenu and Game are enabled. Normal navigation refreshes
selection on entry and mastery uses implemented-reward checks. No additional ordinary
preference-saving defect was established in this pass. Exception handling for a failed
MainMenu LoadScene and invalid/missing menu references remains a robustness observation,
not a claimed cold-launch test or a change in this PR. Full serialized menu/prefab/GUID
verification and real cold-launch/Retry persistence remain in the final sweep/validation.

## Final combined validation additions

1. Compile/discover/run all 19 cases together with previous audit groups.
2. In actual encounters disable the runtime and separately its root before readiness;
   test Shield Knight, Miner, Crossbow Guard, Court Mage and barricade users. No NEW
   special effect may start from retained callbacks while unavailable. Re-enable and
   exercise an existing retry; no duplicate cast or consumed charge on rejection.
3. Test real stagger expiration with a busy board, disabled host and later legal retry.
   Do not weaken the existing stagger gate or steal board ownership.
4. Hold a Miner request at the animation wait, acknowledge impact, then defeat/despawn
   the owner before the board continuation. No new hole, clear rewards or softlock.
   Also test ordinary completion, missing-event timeout, death before impact, and two
   Miners with independent impact signals and cleanup.
5. Existing holes restore through queued cleanup. Canceling a new mine must not remove
   another owner's holes. Accepted shatter/refill/cascades still settle normally once.
6. Preserve #134 wave-ending utility, #140 restored-hole/standard gravity, #144 death and
   encounter isolation, #145 recovery and all prior energy/ability ownership fixes.
7. Test actual menu Back/Start/mastery navigation and cold-launch/Retry preferences in
   the combined pass. Preserve the user's current preferences; disposable test values
   require restoration. No unimplemented Damage Bomb unlock or palette/UI redesign.
8. Run required `Tools/Validate-Unity.ps1` per AGENTS.md in the final authorized pass.

The full repository source audit is still incomplete. Final cross-system/static and
serialized-reference coverage remains; no percentage or all-files certification is
inferred from the number of PRs or authored tests.
