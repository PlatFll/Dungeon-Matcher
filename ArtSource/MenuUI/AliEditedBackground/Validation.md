# Integration verification — 2026-11-08

- Preserved the saved 8-layer LibreSprite document byte-for-byte (SHA-256 in
  `manifest.json`). Exported its complete visible composite using LibreSprite.
- All 230,400 authored RGBA pixels match `Hall.png` and the first 640 rows of
  `HallTall.png` exactly. Both runtime PNGs match their archived native exports.
  Both images are fully opaque; native scale and existing import GUIDs/settings
  are unchanged. `native-verification.json` records the exact checks.
- `Tools/Validate-Unity.ps1`: **passed**, Unity 6000.3.19f1.
  Local full log: `C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-71680b9d-f130-41cd-91b7-90703e01d3bb.log`.
- Existing `MenuUiReviewTests`: **3 passed, 0 failed, 0 skipped**:
  `HomeNavigationPreferencesAndSavedRunContinue`,
  `NativeMenuLayoutsFitSafeAreasAndEveryPlayable`, and
  `CaptureMenusAndPreservePause`. Tests run the real MainMenu and Game scenes
  with disposable account/selection state, verify unobstructed button hit areas,
  preferences, pause, saved-run continuation, Point filtering, text fit and
  integer native scaling. XML and full test log are in `evidence/`.
- Visually inspected actual Unity menu captures at 1080x1920 and 1080x2400.
  Also captured 720x1280 safe-area layout. Stairs, wall mounts and props match
  the user-authored composite; character, logo and interactive controls remain
  independent. These are Unity editor runtime screenshots, not device tests.
- Historical `Tools/Validate-MenuArt.py` completed its 54 native ASE/PNG/import
  checks, then **failed** its old unrelated starting-file checksum for
  `DrownedCourt_Throne.prefab`. The prefab has no Git diff from the main base
  (629831e8). The historical checker/expectations were left unchanged; this
  integration does not claim that entire legacy command passed.
- No C# gameplay/runtime changes, scene layout changes, zone artwork changes,
  character edits or generation spending. Unity test/import-only project
  settings and metadata rewrites are excluded from the PR.

Physical-device display/touch testing was not performed.
