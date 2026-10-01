# Gideon Glass validation

Date: 2026-09-27. Branch: `codex/gideon-glass`. Base: `651fe50`.

The [2026-10-01 integration review](PR_INTEGRATION_2026_10_01.md) supersedes this historical checkpoint for the combined release: Gideon uses the shared Thaleah text path and Developing Fluid grants 10 shield under the current five-point combat rules. The original evidence below is retained with its original values.

## Gate record

1. Read the complete brief, art authorities, exact working cast, live Rattlebones idle and current board/ability/card/continuation owners before editing.
2. The first simplified drawing was rejected. Art gates were reopened using the user's second, definitive design. The current 64×64 native drawing preserves that design's silhouette. Static audit: 16 opaque colors, binary alpha, 1,606 occupied pixels, one connected silhouette, bounds `(8,3)–(56,64)`, fixed row-63 contact. Reviewed at 1×, nearest-neighbor enlargement, dark/light backgrounds and beside the exact cast.
3. Reviewed all nine idle frames and live 130 ms playback beside Rattlebones in LibreSprite. Fixed a neck gap by coordinating head/shoulder displacement. Feet, cane and grip remain planted; the loop seam is pixel-identical.
4. Reviewed all cast/hold/recovery frames and actual playback before gameplay implementation. Ten-frame cast: 850 ms, one 40 ms lens flash and one three-frame steam release. Dedicated static hold. Four-frame recovery: 370 ms. Cast→hold, hold→recovery and recovery→idle boundaries match exactly.
5. Base gameplay, edge cases and two character cards passed targeted real Play Mode tests before broad regression. The final focused rerun passed all eight tests, including all three pin states and the assigned button asset. [Focused results](GideonGlass/gideon-tests.xml), [regression comparison](GideonGlass/regression-summary.json).
6. Integrated graphics review passed at 720×1280 and 1080×2400. The harness observed live idle and the actual flash frame, played five real moves through hold→rewind→recovery, checked countdown/affinity separation and identical cast scale/baseline, and rendered the character menu. Inspected the resulting full screenshots and exact-pixel crops. Fixed a counter/affinity overlap, supplied a lens emblem in the existing ability-button frame, and aligned the third character tab and its typography. The local screenshots include the user's current background edits; those edits are preserved outside this feature.
7. Final source/export audit passed all 24 character animation frames: 64×64, approved palette, binary alpha, exact native/import pixels, fixed feet/cane and matching transition endpoints. The 176×64 button copy also has binary alpha after correcting faint pixels inherited from its old frame. [Art audit](GideonGlass/art-audit.json). Reran the complete graphics review after that correction; it passed. Reviewed the complete relevant source/serialized diff and new files, including RNG isolation, owner remapping, input ownership, warning lifetime and card save boundaries. No known Gideon defects remain.

Final required `Tools/Validate-Unity.ps1` passed on Unity **6000.3.19f1**, exit **0**, after all source and asset corrections. Log: `%TEMP%/DungeonMatcher-UnityValidation-8d237742-1211-4a3d-86aa-ff2c347f756a.log`. No merge was performed.

## Executed gameplay checks

`ChronoShutterTests` runs the production Game scene with disposable profiles and temporary character selection. Durable-enemy fixtures permit five/six real moves without ending the encounter. Tests cover:

- Accepted activation, exact energy spend, rejected recast and invalid-swap rejection.
- Five manual turns; cascade callbacks count once; zero is visible while the fifth turn resolves.
- Original colors, specials and logical identities restored; every repeated move produces the same board colors/specials and refill RNG state.
- No extra clear, damage, energy or gold event on restoration; completed move counters and unrelated gameplay RNG persist.
- Saving with three moves remaining and during a long fifth double-crystal chain; resume produces the same final board, RNG, energy and gold once.
- Existing special types, mines, barricade durability, standards and pins restored without deferred Gem destruction triggering physical clears. Dead-owner effects expire; persistent obstacles orphan.
- Deterministic reshuffles; malformed photographs rejected without board mutation.
- Current enemy warning deadlines and logical targets preserved. A consumed hammer warning cannot return or strike again.
- Pause, defeat, encounter completion and explicit cancellation clean up the photograph and only its own input lock.
- Long Exposure requires six turns. Developing Fluid grants 12 shield once; saving during rewind and resuming does not grant it twice. Cancellation grants nothing. Cards and photographs do not leak into a later character/run.

## Broad regression and runner limits

The full Edit Mode discovery run executed **390 tests: 341 passed, 49 failed, 0 skipped**. All eight Gideon tests passed. The existing `BalanceLifecyclePlayTests` wrapper also passed **108 lifecycle cases in actual Play Mode**; these cases are part of that one NUnit wrapper result, not an extra 108 NUnit tests.

To investigate the 49 failures, created an isolated detached checkout of unchanged base `651fe50` and ran the complete suite there: **382 tests: 333 passed, 49 failed, 0 skipped**. The failure identities match exactly. These are the documented direct-Edit-Mode limitation in CrackedCenterRewardTests, CrackedChainUpgradeScopeTests, PlayerAbilityLifecycleTests, RunUpgradeLifecycleTests and UpgradeIntermissionLifecycleTests: they require normal Play Mode callbacks and the disposable/unlocked fixture. The supported Play Mode wrapper covers their original setup, assertions and teardown without disabling any assertion. See the prior [Balance lifecycle explanation](BALANCE_V1_VALIDATION.md).

Full local evidence: `.utmp/gideon-all-initial.xml/.log`, `.utmp/gideon-baseline-tests.xml/.log`, `.utmp/gideon-baseline-comparison.txt`, `.utmp/balance-lifecycle-play.txt`. The baseline checkout is `.utmp/gideon-baseline`; no feature or user files were reverted to run this comparison.

## Integrated art evidence

- [Current cast at the same in-game scale](GideonGlass/cast-comparison.png).
- [Flash, steam, hold and recovered idle in the actual HUD](GideonGlass/flow-details.png).
- [720×1280 game / countdown](GideonGlass/game-720.png), [1080×2400 game / countdown](GideonGlass/game-1080.png).
- [Character menu](GideonGlass/menu-720.png).

Full graphics outputs are in `.utmp/GideonVisuals`; run log: `.utmp/gideon-visuals.log`. The native animated review `ArtSource/GideonGlass/ChronoShutter_Flow.gif` is separate source-art playback with a simulated counter; it is not evidence of gameplay.

Initial compilation found two static helper calls to the new board-owned RNG. Converting those private helpers to instance methods fixed compilation; `Tools/Validate-Unity.ps1` then passed (log suffix `5fb08f31-cd69-4356-8877-67c382a152f5`). A lifecycle test initially assumed defeat used an external board lock; inspection showed it did not. The corrected assertion checks preservation of another owner's lock and disposal of the ability's own lock; it passed.

## Reproduction

- `Tools/Run-GideonValidation.ps1 -Mode Tests` — focused Gideon suite.
- `Tools/Run-GideonValidation.ps1 -Mode Tests -TestFilter All` — full discovery, including the documented direct-Edit-Mode failures above.
- `Tools/Test-Balance.ps1` — the established supported regression selection, including the real lifecycle wrapper.
- `Tools/Validate-Unity.ps1` — required Unity compilation validation.
- `Tools/Run-GideonValidation.ps1 -Mode Visuals` — graphics-enabled production scene and menu review.

No Android/device run or human pacing study has been performed. Initial character/card numbers remain tuning hypotheses.
