# Board frame and HUD verification — 2026-09-09

## Diagnosis in the actual Game scene

Before editing, the running editor showed `BoardFrame/TopLeftCorner` on `Default / 0`. Its loaded sorting-layer dropdown did not contain `BoardFrame`, although PR #113 had added that layer to the on-disk TagManager. The guard repeatedly replaced BoardVisuals' valid initialized sorting with the unavailable layer name and order zero. The frame consequently rendered behind board tiles and gems. This was an editor/project-settings mismatch, not evidence that sorting layers themselves cannot order these sprites.

The corner was unmasked, at local z=0, with Sprite-Lit-Default. Gems used Gems/0 (selected gems increment that order), cells BoardBackground/10, and background BoardBackground/1. Inspected obstacles and board effects also used Gems. The board hierarchy had no SortingGroup. Gems use the rectangular board SpriteMask; the frame must remain outside its clipping. The gameplay canvas is ScreenSpaceOverlay. Shader/render-queue inspection did not identify a competing render-pass cause.

BoardVisuals now assigns the established Effects/100 once, above board content and below WorldUI/overlay UI. The guard and extra layer are removed. Gem generation, board resolution, combat, shield, and ability spending are unchanged.

## HUD corrections

Three components competed over the bottom enclosure: AbilityButtonUI built an obsolete frame, BottomHudModularFrameController rebuilt it, and BottomHudPresentationTuner resized it every frame. The final 50-pixel corners were paired with 8-pixel borders despite the source 80:16 ratio requiring 10. Tiled Images also cropped the source strip instead of matching its scale to the corners.

The bottom controller now owns the existing 104-pixel enclosure, and the shared responsive fitter owns proportional edge geometry and tile density. The player enclosure now measures the arena's actual border before adding the intended six-pixel gap; its previous fixed inset assumed a 10-pixel arena border when the responsive frame used 16.

Source art/import settings were preserved (80x80 corners, 64x16 strips, 64 PPU, Point filtering, no mipmaps, uncompressed default import). Two pre-existing local PNG edits are excluded from this change. Captures show those local assets. Responsive board sizing and the existing bottom-corner footprint remain unchanged; this does not impose integer source-pixel magnification at every arbitrary screen size.

## Actual Play Mode checks

Unity 6000.3.19f1, Game scene, Windows editor renderer:

| Game View | Result | Cascaded clears |
| --- | --- | --- |
| 540x960 reference, final HUD | Passed board rendering and swaps; screenshots reviewed | 3 |
| 1080x1920 | Passed rendering, swaps, obstacles, UI join/density assertions | 4 |
| 1080x2400 | Passed rendering, swaps, obstacles, UI join/density assertions | 11 |
| 480x800 additional stress check | Passed; existing Pixel Perfect Camera below-reference warning remains | 3 |

The editor menu `Dungeon Matcher > Validation > Board Frame Play Mode` (Ctrl+Shift+F8) enters Play Mode and uses temporary fixtures in the real scene. It exercises the production swap/resolution coroutine: initial fill, outermost rows and columns, left/right falling, top spawning, row/column bombs, poison bombs, color crystals, and eight further legal moves. Later checks queue barricades, pinning, freezing, and mining through existing board APIs. It captures native Game View PNGs during motion and checks renderer ordering throughout. These are production-pipeline checks, not automated mouse/touch-input tests.

Visual review covered all four corners and sides, motion captures, special effects, obstacles, and complete HUD layouts. Board elements stayed beneath the decorative frame; overlay HUDs remained above world sprites. The board's symmetric inset and the top/bottom HUD boundaries remained intentional. The corrected modular joins showed no obvious missing strips or mismatched border bands. Pixel-perfect-camera warnings at 480x800 prevent treating that below-reference size as an unqualified pixel-perfect target.

Full capture sequences and renderer reports are local under `.utmp/FrameVerification`. Representative native screenshots are retained here:

- [Before HUD correction, 540x960](before-hud-540x960.png) — intermediate board-fix stage, not a pristine-main capture.
- [After, 540x960](after-540x960.png)
- [After with obstacles, 1080x1920](after-1080x1920.png)
- [After with obstacles, 1080x2400](after-1080x2400.png)
- [Below-reference stress check, 480x800](after-480x800.png)

`powershell -ExecutionPolicy Bypass -File Tools/Validate-Unity.ps1` completed successfully with Unity 6000.3.19f1 after closing the interactive editor. The first sandboxed attempt stalled initializing licensing; the retry with local licensing access exited zero. Validation log: `DungeonMatcher-UnityValidation-d4476b3e-2924-4c2c-8c14-d8d79ff728d0.log` in the local temporary directory. `git diff --check` also passed.
