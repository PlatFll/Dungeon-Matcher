# Approved settings integration

The user authorized integrating the reviewed settings UI, edited main menu and
water effect into the actual game on 2026-11-09. This folder preserves 37 exact
PNG/LibreSprite source pairs from Settings Stages 05–07. The historical stage
notes retain their original review-only status; this document records the later
integration authorization. `manifest.json` records the exact source hashes.

`SettingsMenuArt` assembles the approved frame modules at their native dimensions:
304×532 for battle, 304×348 for home, 264×44 preference rows, and 248×48 / 248×44
action buttons. Text remains live Thaleah. Painted icon and text bounds are
centered as one group with a 12-native-pixel gap, including safe-area scale changes.
The same settings presentation is called by MainMenuArtwork and RunControlsUI;
the existing controllers still own pause, audio, motion, vibration, confirmation,
suspend, retry and exit behavior. Missing optional battle art keeps its fallback.

The existing main-menu navigation assets are the same ones reused in the approved
review. Ali's edited Hall/HallTall source and the light-gray shared battle UI remain
unchanged. No new PixelLab generation or approved-art modification is needed.

Native source verification: run `validate.py` with the Sprite Foundry art Python.
Runtime coverage is in MenuUiReviewTests; actual Unity captures go to
`.utmp/MenuUI/after`. `evidence/runtime-validation.json` and the two original test
XMLs record all five runtime checks passing on their latest execution, with 14
actual Unity captures. The first all-zone run clicked during a Game-view resize;
the rerun waits for the established layout pass and retains identical assertions.

Final mandatory compilation passed with `Tools/Validate-Unity.ps1`:
`DungeonMatcher-UnityValidation-2ef6c9ce-266b-4ba8-9b7e-78f47a570314.log`.
The original 54-asset validator and its four protected-file checks also pass.
No Android build or physical-device execution was performed in this integration.
