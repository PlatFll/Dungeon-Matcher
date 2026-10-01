# Open PR integration — 2026-10-01

The user authorized reviewing all open Dungeon Matcher PRs and merging those that preserve the game.

## Inventory and ancestry

Six PRs were open: #143, #146, #149, #169, #170 and #171. Remote main was `651fe50266a8be6fd5a9682202c99d83b09f2153`.

- #143 (`219275d`), #146 (`4aad282`) and #149 (`3ebbb9c`) are already ancestors of main. Their commits do not introduce a new diff; their draft PRs remained open against old dependency branches.
- #169 (`81e37d6`) adds Gideon and board memory.
- #170 (`229edb8`) adds the finalized presentation, Thaleah and combat feedback.
- #171 (`1e278d4`) builds on #170 with endless combat, shield gating, clean combat amounts and approved enemy motions.

No review objections or pull-request workflow runs were returned for these heads. Local Unity validation is the repository's required gate. The user's open Unity checkout and its uncommitted work were preserved; integration used the existing separate validation checkout.

## Combined review corrections

- Resolved the character-menu conflict by retaining rounded HP/hit/shield stats and ChronoShutter's named ability label.
- Migrated Gideon's move counter and character-tab references to the shared TextMeshPro/Thaleah path. The original PR's legacy Text references do not work with #170's controls. The rendered Gideon check now verifies the shared font and pixel fitter.
- Aligned Developing Fluid's asset, importer, description and exact grant/resume assertions to 10 shield, consistent with #171's five-point rule. Cancellation and duplicate-grant assertions remain intact.
- Made the cap fixture begin with a real crystal response. A random board can have legal swaps but no useful attack/heal option, in which case the counterplay guard correctly refuses mining. Exact global/per-owner cap, useful-response and death-cleanup checks are retained.
- Deferred the opt-in Gideon visual runner's Play Mode entry until editor startup callbacks complete. Two immediate-entry attempts failed in Unity's search indexing callback; the deferred run passed without suppressing that exception or changing game behavior.
- Gave the rendered Archbishop healing fixture a durable wounded recipient. Random refill cascades had legitimately killed its 30-HP Farmer after the heal. The fixture retains its net-healing assertion and additionally checks one exact healing pulse per surviving rune, as specified in `ROYAL_SPECIALS.md`; normal refill damage and the subsequent blessing still execute. The first additional assertion incorrectly expected one pulse for the whole set and was corrected to the documented per-rune behavior.
- Preserved both documentation additions and all three feature histories. No main history was rewritten.

## Verification

The combined regression selection passed **278/278**, zero failed or skipped. It covers combat, progression/economy, continuation, board ownership, disruption, existing lifecycle cases and Gideon's real five/six-move rewind. Results: `.utmp/pr-integration-regression.xml` and `.log` in the separate validation checkout.

All **9 focused save checks** passed, including brief and persistent destination locks, duplicate settlement and the existing write-failure cases. Results: `.utmp/pr-save-lock-tests.xml` and `.log`.

The graphics-enabled Gideon review passed for all three characters at 720×1280 and 1080×2400: live idle/cast, five accepted moves, hold counter 5 through 0, rewind/recovery, imported flash/steam frames and character menu. Thirty screenshots are in `.utmp/GideonVisuals/`; the current `.utmp/gideon-visuals.log` has no startup search exception. The 720p menu and both hold layouts were visually inspected for text fit and counter placement.

The graphics-enabled enemy ability suite passed **3/3**, zero failed or skipped, after the healing fixture correction. It verifies production metadata, stale/duplicate/paused cues, actual casts and healing pulses, shield contact, both King thrusts with one final refill, missing-art fallback and disable/death cleanup. Results: `.utmp/enemy-abilities-tests.xml` and `.log`; 32 captures in `.utmp/EnemyAbilitiesReview/`.

The required `Tools/Validate-Unity.ps1` passed on **Unity 6000.3.19f1**, exit 0, after all source changes. Log: `%TEMP%/DungeonMatcher-UnityValidation-d51a8a49-cd3f-421b-85ec-3d12dccb46b8.log`. The final changes after the 278-test run affect only the two opt-in rendered test harnesses, which were rerun successfully, and documentation; production code is unchanged.

Integration preserves the commit ancestry of all three current PRs and excludes unrelated user edits and Unity-generated settings/metadata churn. The older draft PRs #143/#146/#149 were closed after proving that their complete heads were already in main. The user authorized the remaining main merges following these gates.

The first combined run passed 273/276 tests. The menu case exposed the legacy Text references above. The cap case depended on the random opening's useful responses. A reward-settlement case encountered a Windows file replacement failure (`Unable to remove the file to be replaced`); the save code correctly rejected the transaction without crediting the wallet. All six focused cases then passed, including all four reward reasons, without changing save code. The final complete run checks those paths again.

A second broad run passed 275/276 and repeated the file replacement failure during a different reward case. The correction retries the same flushed replacement up to three times (20/40/60 ms) only for Windows sharing/lock errors 32/33 and error 1175. Other failures still reject immediately; persistent contention exhausts the bounded retry and preserves the original in-memory wallet/journal. This adds no repeated reward calculation, debit or event publication. New tests hold the real destination file briefly and persistently to check exactly-once credit and failure atomicity.

The retry excludes partial-move errors 1176/1177. Microsoft's [ReplaceFileW contract](https://learn.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-replacefilew) states that error 1175 retains both original filenames, making a retry of the same prepared transaction safe. The test runs do not identify which external process briefly held the file.

The existing full-discovery limitation remains documented in `GIDEON_GLASS.md`: 49 direct-Edit-Mode lifecycle fixtures also fail on unchanged base. The supported regression selection includes their actual Play Mode lifecycle wrapper; no failing assertion is suppressed. Android/device and human pacing checks remain separate.
