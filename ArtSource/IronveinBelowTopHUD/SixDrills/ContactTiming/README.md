# Ironvein drill contact timing

The previous implementation entered the cell before pausing. The corrected bit approaches the near face of each later occupied tile, holds position while spinning for 140 ms after a 35 ms approach, then waits for canonical visible shatter to finish before its 75 ms entry movement. The first cell instead shatters immediately while launch travel begins. Empty/already-cleared cells do not resist. Empty sockets, terminal burst, reload, rewards, charge rules, board ownership, final settlement and all source artwork remain unchanged.

`Review.html` presents the actual Unity capture with row detail, full view and frame scrubbing. `ContactSequence.gif` is a compact two-row preview. `FullSequence.gif` shows the full assembly. These are the real board/drill systems displayed with the existing approved art-review scene; actor/HUD snapshot remains static. Production Game has the mechanic, while the full new environment remains a separate integration step. The original capture is preserved one folder above.

## Verification

11 affected checks passed in `5ac0083a-73e6-4a41-817b-5c06a817f85f.xml`, including both-direction contact/instant-launch tests, visible target destruction before entry, unchanged position while spin frames advance, pause/reload, all-six overlap/Obsidian handling, no-view gameplay, normal match timing/results and boss response windows. Full local XML/logs are under `.utmp/ForestValidation/`. `Verification.json` lists every test. `Tools/Validate-Unity.ps1` completed successfully; log: `C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-075b6b49-b1a2-4358-a111-5696e924b8a6.log`.

Movement uses the same scaled clock as the board. Contact tests observe arrival with a strict deadline and verify stationary spinning at fixed 60/30 Hz. The initial harness incorrectly treated a WaitForSeconds yield as an elapsed-time observation; diagnostics showed it advanced only one editor tick. The corrected tests use explicit clock predicates and retain their stationary-spin, intact-target and visible-shatter assertions. The full affected set was successfully rerun. Stone views are removed at the board-owned break moment before entry, including instant removal in the launch cell.

No extra generations, sprite edits, Android build, physical-device test or merge. Draft PR #215 awaits review.
