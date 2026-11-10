# Stage 2 validation — 2026-11-11

- Unity `6000.3.19f1`; `Tools/Validate-Unity.ps1` completed with exit 0.
- Final compilation log: `UnityCompilation.log` (local, ignored by Git).
  Original log filename: `DungeonMatcher-UnityValidation-addc2430-4852-4ea2-b044-bd0d09ff26e3.log`.
- `Tools/Test-ReferencePresentation.ps1 -Filter SilverRoyalRelicTopHudTests.Stage02`
  completed with exit 0: **1 passed, 0 failed, 0 skipped**. The single test
  covers five real Game capture pairs and the layout/interaction assertions
  described in the handoff. This is not a claim that all 943 discovered tests ran.
- Final XML: [UnityTests.xml](UnityTests.xml), originally
  `reference-tests-20261111-021046.xml`; corresponding full local log is
  `UnityTests.log` (ignored by Git).
- All six native exports matched reopened LibreSprite sources exactly in visible
  RGBA, retained binary alpha and used only colors from selected A.
- Independent preservation check: all **9,164** pre-existing files under Assets,
  ProjectSettings and Packages remain byte-identical. All reference PNGs and the
  golden register remain unchanged. Only new Stage 2 asset/editor files were
  added; the canonical guide and project-state documentation record the selection.
- Unity reserialized three existing consumable importers with whitespace and
  migrated TimeManager's timestep serialization during validation. These
  unrelated automatic disk changes were inspected and restored to the exact
  starting bytes after Unity exited. No runtime source or artwork was altered.
- Visual review inspected small, normal, tall/safe-area and underwater captures:
  clean bevels, no ornament clipping, square gear button and readable wave text.
- No Android build, physical-device run or full five-zone acceptance this stage.
- PixelLab generations: **0 Stage 2**, **29 Stage 1**.

Review page: run `../start_review.ps1`, then open
`http://127.0.0.1:61347/Stage02/Review.html`.
