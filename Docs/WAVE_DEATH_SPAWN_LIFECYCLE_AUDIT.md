# Wave, summon and death-completion lifecycle audit

Gameplay audit chunk 11 / validation backlog group 10.
Base: `4e2e959186492636d44c91c33514d65aa1f724c1` (main after #134).
Branch: `fix/wave-death-spawn-lifecycle`.

**IMPLEMENTED / SOURCE AND DIFF REVIEWED / UNITY VALIDATION DEFERRED / NOT MERGED.**
Compilation, NUnit discovery/execution, Play Mode and rendering have not run. Include this with the other outstanding audit groups in the user's final combined validation state. Do not run a separate local validation now or interpret authored tests as passing results.

## Source-confirmed findings

### Interrupted or unavailable death presentation could strand wave completion

`WaveController.HandleEnemyDefeated` adds a pending-death count and waits for `EnemyLifecycleVFX` to call back. `EnemyLifecycleVFX.OnDisable` stopped the coroutine without acknowledging completion. The wave could wait forever after the last enemy was already defeated. A surviving disabled corpse under the spawn anchor could also keep a summon slot unavailable.

`PlayDeathEffect` also accepted a request without checking `visualRoot` / `enemyImage`, although `DeathRoutine` dereferenced both. In that missing-reference case the coroutine could fail instead of invoking the callback. The checked general enemy prefab currently DOES assign these references and uses 0.08s white hold / 0.42s death duration; this is not a claim that ordinary prefab deaths all fail. No prefab or timing change was made.

Correction: reject unavailable presentation before accepting ownership, letting the existing caller fallback clean up. Accepted deaths have one consumable completion acknowledgement. Disable interrupts visuals but still acknowledges the wave owner; it does not falsely publish a completed-animation event. Normal finish consumes callback state before observers and acknowledges in finally even when a DeathFinished observer throws. Mid-death loss of image/root no longer breaks the death routine's direct dereferences. A new spawn effect cannot discard an already accepted death callback.

### Old death callbacks could modify a replacement encounter

`ClearCurrentWave` reset a shared integer counter but did not identify the encounter associated with each deferred callback. Defeated actors have already left `EnemySlotUI.CurrentEnemy`, so their death presentation can remain alive after current slots are cleared. An old completion could then decrement a newer encounter's pending-death count and complete it before its own death presentations finished.

Correction: capture an encounter identity and a per-death consumed flag. The callback cleans up its own actor but changes counters/completion only for the original encounter, once. Ignore null/foreign/duplicate roster-defeat deliveries. Clear invalidates identity before cleanup, even when the wave number is unchanged. Old completion/transition callbacks cannot resume a replaced encounter solely because it reused that number.

### Clear during staged spawning did not cancel the spawn loop

`ClearCurrentWave` canceled next-wave progression but not `waveSpawnCoroutine`. A loop waiting between enemies could wake and repopulate the cleared encounter. A synchronous spawn callback could similarly clear or replace the encounter while the old spawn method continued publishing state.

Correction: public clear cancels the outstanding spawn handle and revokes its ownership. `SpawnCurrentWave` clears before starting its replacement routine, so clear does not stop the new routine from inside itself. Token/wave checks around iterations and spawn callbacks reject old continuations. The public starter retains a coroutine handle only while its own attempt is still pending, including synchronous completion/replacement cases.

Also publish WaveStarted at the end of staged spawning BEFORE checking a previously deferred completion. Previously, sufficiently fast deaths/partial spawn failures could yield WaveCompleted followed by WaveStarted. Normal timing remains at the end of the existing spawn loop, not at the first enemy or after a new artificial delay.

### Summoning could use slots still owned by staged spawning

`HasFreeEnemySlot` and `TrySummonEnemy` treated empty anchors as available during `isSpawningWave`. The remaining loop could then bind a planned enemy into that slot or clear an unused slot, destroying a newly registered summon. Such a destroyed summon could remain in `activeEnemies` without its normal defeat event.

Correction: no summon acceptance/availability until the staged spawn loop has released those slots. Existing empty-anchor checks remain. Public summon acceptance rejects disabled controllers and unavailable/dead players. A binding callback that replaces the encounter cannot register its old created actor in the new roster. Normal post-spawn summoning still uses CreateEnemy, current weakness selection, scaling and independent lifetime. This is an ordering/API edge case, not a claimed frequency in ordinary Marshal encounters.

## Preserved scope

Three production files:
- `Assets/_Game/Scripts/Combat/Enemies/Waves/WaveController.cs`
- `Assets/_Game/Scripts/Combat/Enemies/Waves/WaveController.Summoning.cs`
- `Assets/_Game/Scripts/Combat/Enemies/Presentation/EnemyLifecycleVFX.cs`

No damage, shields, stagger, attacks, enemy definitions, weights, milestone windows, draft RNG, mastery, board resolution, layout, art, scenes, prefabs or imports changed. No second spawner or board resolver. PR #134's board/gate transition check remains in place. The VFX file has a nonfunctional final-newline difference that can be normalized during integration.

This branch starts from main and has no production-file overlap with #135-#143. All groups still need behavioral integration testing. #143 independently retains its dependency on #142.

## Authored coverage — not run

`WaveDeathLifecycleTests`: 24 NUnit cases across 21 methods.

Disposable synchronous EditMode fixtures cover encounter-scoped/once-only acknowledgement, old same-number callbacks, pending spawns and living members blocking completion, clear revocation, foreign defeat rejection, stale transition/completion callbacks, normal/interrupted/reentrant/throwing presentation completion, missing visuals, spawn-effect rejection during a pending death, and summon availability around staged spawning/anchor cleanup.

The tests invoke real helper/API and coroutine code with injected states, null actor cleanup handles and minimal inactive objects. They do NOT execute the real timed staged-spawn coroutine, actual prefab death cleanup, complete real summons, scene retry or next-wave progression. Do not represent them as 24 Play Mode reproductions. No preferences or source assets are edited.

## Final combined validation backlog

1. Compile and discover all 24 cases together with earlier audit tests and relevant existing wave/Marshal/Royal/gameplay validators.
2. Kill the final enemy normally; disable its death VFX, disable its root, or remove image/root during the hold. Each accepted death must release pending completion exactly once; missing references must use immediate fallback without a softlock. Test actual destruction from inside the callback and optional-art-free prefabs.
3. Replace/clear an encounter while a corpse is still animating. Start new deaths, then let the old callback finish. It must not decrease the new pending count or publish a new wave's completion.
4. Clear after the first enemy of a multi-enemy staged spawn, including from EnemySpawned. Wait beyond all original spawn delays: no ghost enemies or old WaveStarted/Completed events. Replacing with the same wave number must behave as a distinct encounter.
5. Test zero/nonzero spawn delays, one/two/three enemies, partial creation failure, and very fast kills. Valid waves publish start before completion, once. Failed creation must not register null/phantom actors or claim successful combat; do not silently skip guaranteed bosses or reroll content to conceal invalid assets.
6. Attempt summons while staged slots are reserved: reject without changing roster/slot/weakness RNG. After spawning ends and an anchor is truly empty, a valid summon succeeds once and stays an independent enemy until defeated. Dying-anchor children still block premature reuse.
7. Check WaveStarted-dependent upgrade energy/once-per-wave bookkeeping, #142-#143 choices/resets, #138 attack startup, #137 defeat reporting, and #139 Marshal readiness on the integrated state. Preserve board settlement and no old-clear damage to next-wave enemies.
8. Run the shared required `Tools/Validate-Unity.ps1` only in the final combined validation pass. Record actual tested commit and unresolved errors; never weaken assertions to obtain PASS.

## Limits / remaining audit scope

This does not design a new recovery policy for an entirely invalid enemy catalog or missing required scene wiring. It is not general enemy pooling support, a full controller suspension/resume feature, or certification of arbitrary exceptions from every gameplay observer. Spawn visual loss outside the death path, Game Over/revival/retry, and remaining cross-system checks remain audit areas. The full repository audit is incomplete.
