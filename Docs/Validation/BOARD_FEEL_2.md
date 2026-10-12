# Board and combat feel validation — 2026-10-12

Implemented against main `b3e1edd68d8423ef8dd9defaa8cd1ed160f2aeb9` in an isolated
worktree. The user's existing menu/environment edits remained in their original
checkout. No King assets or code were imported.

## Executed checks

- **Required Unity validator passed** on the final C# and audio assets with
  Unity 6000.3.19f1: `Tools/Validate-Unity.ps1`, exit 0. Full local log:
  `C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-d9138916-07af-407e-ac16-54eb20e27a16.log`.
- Broad graphics-enabled NUnit run: **223 cases, 212 passed, 11 failed, 0 skipped**.
- Unchanged-main comparison of those failing fixtures: **12 cases, 3 passed,
  9 failed, 0 skipped**. Production files were restored to the exact base for
  this comparison, then the feature checkpoint was restored.
- Final affected/retry run: **50 passed, 0 failed, 0 skipped**. Includes all
  18 new policy/resource tests, all nine new gameplay tests, audio mix/loading,
  hybrid timing, proper ability lifecycle setup, and both non-baseline failures.
- Across broad and final runs, using the latest result per unique case:
  **236 passed; nine remaining failures all reproduced on unchanged main**.
  This is not a claim that the entire repository suite is green.
- Original audio generation: all 47 exports verified for SHA-256 manifest
  agreement, distinct content, mono 16-bit 44.1 kHz PCM, peak ceiling, quiet
  boundaries and residual DC. All 47 importer GUIDs are distinct. The original
  thirteen base wave files are byte-identical to main.

[Machine-readable evidence](BoardFeel2/evidence.json) records every case result,
failure summary, source XML path and hash. Complete XML/log pairs remain under
the worktree's `.utmp/ForestValidation/`, with run IDs recorded in that file.

## Coverage and remaining baseline failures

The passing regressions cover pointer ownership/cancellation, Slippery/manual
slides, invalid swaps without rewards, grouped shatter timing, missing optional
presentation, mute/focus/pause, special creation, a 30 Hz multi-cascade chain,
old/new interrupted journals, version-1 seconds saves without a move clock,
crystal validity and chain attribution, barricades/gravity, reshuffle pins,
damage results, unified combat, multiple player abilities and continuation.

Nine unchanged-main failures remain:

- The old `BalanceLifecyclePlayTests` reflection wrapper invokes the ability
  fixture without its newer `UnitySetUp` coroutine. The actual
  `PlayerAbilityLifecycleTests` run through NUnit passed in the final run.
- Seven ChronoShutter scene cases fail before their assertions because their
  fixture starts with an unsupported 640×480 GameView. Their gameplay assertions
  remain unverified in this run; no layout errors were suppressed.
- `AttenderPaysOnceRetriesDurableObstacleAndRespectsCardDamage` reports the same
  null reference on unchanged main. Other sequence/Attender cases passed.

The first broad run also had a screenshot-write failure and a stagger-fixture
null reference. Both passed on main and in the final feature rerun, without
changing their assertions. Earlier headless attempts were not used as scene
validation; the final gameplay runs use `-Graphics` and portrait fixtures.

## Measured timing and visual review

The final fixed-60-Hz A/B fixture took **1.467 s with the old profile and 1.383 s
with the new profile** from swap start to complete readiness: five frames,
approximately 83 ms shorter. Board, player, enemy health and completed-action
results were identical. This is one fixture, not a universal percentage or a
measurement of finger-to-screen latency. A separate run with fewer combat
consequences measured 0.717 → 0.633 s with the same five-frame difference.

The final phase capture shows why nominal animation durations do not equal
whole-action latency: swap start-to-arrival occupied 167 ms in both profiles in
this frame-quantized fixture; clear-to-fall shortened from 133 to 100 ms, and
subsequent fall/settle phases account for the remaining gain. Combat ownership
still holds input after landing. No decorative effect releases that ownership.

Recorded CSVs: [old profile](BoardFeel2/profile-a.csv),
[new profile](BoardFeel2/profile-b.csv), and
[30 Hz chain](BoardFeel2/chain-30hz.csv). The chain recorded five clear/fall phases
and one accepted/completed action. Fixed-step frame timing is synthetic; GC/frame
capture is available for later real-device profiling.

Inspected actual portrait screenshots of pointer acknowledgment, shatter,
special creation and the ready board at `.utmp/BoardFeel2/`. The earned special
remains visible and survives the clear; small debris stays within the board.
Screenshots establish presentation/layout evidence, not a subjective motion or
listening verdict.

## Hardware limits

Physical Android/iPhone vibration, motor/audio alignment, speaker/headphone mix,
and low/mid/high-phone frame pacing were not tested. Haptic counters establish
software scheduling and suppression only. Existing device-tuned native backends
remain in place. Further optimization should follow captured hardware evidence.
