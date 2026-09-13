# Game Over recovery and cleanup audit

Gameplay audit chunk 12 / validation backlog group 11.

Base reviewed: `4e2e959186492636d44c91c33514d65aa1f724c1` (main after #134).
Branch: `fix/game-over-recovery-lifecycle`.

**IMPLEMENTED / SOURCE AND DIFF REVIEWED / UNITY VALIDATION DEFERRED / NOT MERGED.**
No Unity compilation, NUnit discovery/execution, timed VFX, Play Mode, scene loading or device testing has run in this editing environment. Include this work in the user's combined validation stage after the source audit. Do not treat authored tests as passing tests.

## Scope inspected

- PlayerActor damage/defeat, Initialize, TryRevive and the existing debug revival command.
- GameOverPresentationController subscriptions, pause, unscaled sequence, generated overlay, visual hiding, Retry and destruction.
- PlayerCombatFeedback death flash and pause/recovery boundary.
- PlayerPanelUI initialization/revival observers and affinity presentation.
- PlayerAbilityController cancellation and CrackedGemsRuntime's retained board ownership.
- RuntimeScenePresentationBootstrap's existing per-scene/retry installation.
- Relevant GAME_DESIGN and ARCHITECTURE ownership guidance. This does not establish new revival availability/costs or new-run design.

## Source-confirmed defects

### Existing actor recovery leaves Game Over active

PlayerActor.TryRevive restores living state/HP and publishes Revived. Initialize publishes Initialized with new actor state. GameOverPresentationController listened only to Defeated, so either recovery could leave Time.timeScale at zero, the overlay accepting raycasts, sequenceStarted true, the sprite hidden/white and its animation paused. A recovery during the unscaled sequence also left that old sequence running. The existing inspector command `Prototype/Revive With 50 HP` is one concrete API entry point.

Correction: listen to this player's Revived/Initialized events; stop only the controller's presentation coroutines; clear its latch, retry callback and generated overlay; restore the image/affinity visibility that it changed; release the death flash/animation through PlayerCombatFeedback's existing cleanup. On enable, reconcile actor state rather than relying only on one-time Start. A late Defeated notification for an already-recovered player is rejected.

This does not add a revival button, free revive, health grant, energy refund or run reset. Ordinary revival remains the existing actor operation. Normal full-scene Retry was already reinstalled by RuntimeScenePresentationBootstrap; no duplicate installer was added.

### Interrupted or removed presenter retains ownership

OnDisable only unsubscribed. Disabling the root could stop its coroutines but strand the freeze; disabling the component could leave unscaled presentation work running. OnDestroy restored time/particles but left the generated Canvas-child overlay and its blocker behind when the component/player was removed without unloading that Canvas.

Correction: shared idempotent cleanup runs on disable, destruction and recovery. It clears only this controller's generated objects/callbacks and visual changes. A still-dead actor can start a fresh death presentation when the observer is re-enabled. Cleanup itself never revives the actor or releases another system's board-input token.

### Retry failure changes state before validating the destination

The old Retry latched, disabled the button and unfroze gameplay before validating the active scene. An invalid scene returned without making the button usable again; a failed LoadScene had no rollback. This is an error-path finding, not evidence that the currently configured Game scene is missing from the build.

Correction: validate the active scene/loadability before changing freeze/button state; require the current completed death-menu state for acceptance. If LoadScene throws, restore the death freeze and allow retry while logging the failure. Normal successful retry keeps the existing active-scene/Single-load behavior.

## Pause and presentation boundaries

Cleanup restores the exact captured time scale, including an already-paused value, rather than forcing a previously paused clock to 1. It does not overwrite a newer nonzero time-scale assignment and is idempotent. This is local ownership cleanup, not a new global/ref-counted pause manager; two independent systems assigning identical zero values after acquisition are not distinguishable by this small guard.

The original panel colors, size, drop/easing formula, particle counts and timings remain unchanged. The shared feedback exit method reuses StopDamageFeedback: it does not force the character back to a stale layout position when no hit shake is running. Missing/destroyed optional captured visuals cannot prevent releasing the clock in cleanup's finally block.

No gameplay actors, board coroutines, wave schedulers, upgrade runtimes or ability-energy stores are reset from Game Over presentation. Accepted Bardley work stays owned by BoardController; clearing a presentation is not permission to prematurely unlock that work.

## Incremental production files

- `Assets/_Game/Scripts/UI/GameOverPresentationController.cs`
- `Assets/_Game/Scripts/Combat/Player/Presentation/PlayerCombatFeedback.cs`

No production-file overlap or compile-time dependency on PRs #135–#144. Test the behavior together, especially ability cleanup (#136), enemy attack revival (#138), run/choice reset (#142/#143), and encounter/death completion (#144). PR #143 still separately requires #142.

## Authored tests — NOT RUN

`GameOverRecoveryTests`: 28 NUnit cases across 21 test methods, including parameterized cases.

Coverage: actual actor revival/reinitialization events; rejected revival; cleanup messages and component removal; exact pause restoration, no overwrite/reacquisition; own-overlay removal; image/affinity restoration; layout position preservation; destroyed visual references; stale/foreign events; one/five subscription cycles; disabled subscriptions; recovery reconciliation; unrelated board/input/energy ownership; retry readiness/stale retry rejection; unchanged menu easing.

These are synchronous EditMode fixtures. They seed a death-menu state so no timed coroutine starts, use a minimal Canvas/button and inactive optional feedback fixtures, and explicitly invoke lifecycle messages in several tests. They do not test actual shader flash/animation resumption, elapsed death phases, real stopped coroutines, successful scene loading or the load-failure rollback. They preserve Time.timeScale and Unity RNG in teardown, do not write PlayerPrefs or source assets, and must not delete live actors to meet fixture preconditions. Skips are not passes.

## Final combined Play Mode backlog

1. Normal death -> white silhouette -> burst -> dimmer/drop -> selectable Retry. Repeat with RattleBones and Bardley and across five scene retries. Confirm runtime installation occurs once, the selected character/mastery persists and temporary run state follows its existing new-run rules.
2. Use the existing TryRevive/debug command at each phase: white hold, burst, menu descent, completed menu. Verify living HP/shield comes only from PlayerActor; no old overlay, frozen clock, hidden character/affinity or stuck animation. Wait beyond every old phase to prove no stale coroutine hides the recovered player again. Die a second time afterward.
3. Reinitialize the player with a different HP override/character during each phase; verify old presentation cancels without overriding new actor data or duplicating run/ability resets.
4. Disable/re-enable the controller and its root at each phase. Remove the component while retaining the Canvas. No orphan blocker/particles or owned clock freeze. Re-enable while still defeated must present Game Over once; re-enable after recovery must not reopen it.
5. Die during Royal Decree, a bomb/crystal chain and Bardley Cracked Gems. No cancelled ability restarts or energy refund. On an authorized actor revive, board-owned accepted resolution remains locked until it genuinely settles; next-wave isolation and exactly-once rewards remain intact. On Retry, the old scene cannot deliver callbacks to the new scene.
6. Preserve other modal/input locks and a pre-existing pause/slow-time value; repeated cleanup must not steal ownership.
7. Verify normal Retry and its failure preflight/rollback with a disposable fixture, without committing altered build settings or scenes. A rejected/stale/duplicate retry must not load, unfreeze or disable the usable current menu incorrectly. Expected failure logs must remain reported, not suppressed.
8. Inspect actual white-flash reset, resumed idle, current layout position and pixel sharpness at the established portrait ratios. Do not redesign the panel or change art/palettes.

Run the 28 cases with prior audit groups, relevant existing gameplay/ability/stagger/upgrade/wave tests and `Tools/Validate-Unity.ps1` in the final authorized combined pass. Full repository audit remains incomplete.
