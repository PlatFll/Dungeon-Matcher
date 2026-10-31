# Snowy Mountain — Stage 5 review

This checkpoint darkens and mutes the existing cobblestone backing, removes the two obsolete outer HUD enclosures in the disposable Unity preview, and adds the Stage 5 native timber/iron/snow board-frame kit. It does not register new production art, alter gameplay, merge, or begin Stage 6 side dressing.

## Artwork

- `CobbleWall_Muted.png`: a 154×52, 17-color material recolor of the approved wall. Every source color has one deterministic destination; dimensions, alpha and masonry geometry are exact. Mean HSV value decreases from 114.81 to 75.59 (34.2%); mean saturation from 0.4764 to 0.3496 (26.6%). The bright approved snow ledge is untouched.
- Four 18×18 iron corner brackets; the inward 5×5 quadrant is open so the bracket cannot cover a board cell. The generated bolt is retained within a simplified upright plate.
- Four timber rails: horizontal 32×8 and vertical 8×32. Native strips are cleaned and tiled, never stretched. Generator clamps are removed from the repeating wood; separate 8×8 connectors align to rail centers.
- Separate 32×8 snow rail, 18×18 snow corner and 16×8 icicle accents. Snow sits on the outside of the frame; icicles hang from its lower outer corners.
- Thirteen `.ase` sources, native PNG exports and reopened PNGs. Binary alpha; no resampling or semitransparent edge pixels.

PixelLab: one Pro job (`1bd66f5e-7f6b-460c-9afd-5fad06f60a80`), four 128×128 variants, candidate 2 selected, **10 generations used**, 4,920 left after this job. Prompt/receipt in `Raw/Generation.json`. The pack's text-only generation method was followed; no reference uploads. Raw candidates are preserved. `SelectedFrame.png` duplicates candidate 2 under a nonnumbered name to prevent LibreSprite's image-sequence dialog.

## Assembly and preservation

`SnowyMountainStage05Tests.cs` loads the actual Game scene, Rattlebones, and three actual Frostwake enemies. The existing `DungeonPixelLabBattleArt.Frame` and `ResponsiveModularFrameFitter` own the new rim. `BoardLayoutController` still owns the board size. The rim uses the established 26-native-pixel outer allowance; no cells, gems, enemy positions, health bars or controls move.

Only the outer `BattleArenaFrame` and `GeneratedBottomHudFrame` are hidden. Portrait, WAVE/GUIDE/settings and shared bottom control frames remain. The existing alpine plate's overscan is revealed around the removed top enclosure without changing its placement or pixels. The lower wall covers the entire screen behind the UI, including safe-area gutters.

Earlier approved stages and production resources are not edited. The starting manifest protects 9,108 files, including the six preexisting local modifications. The new preview is isolated to editor test code and `ArtSource/SnowyMountainStaged/Stage05`.

## Verification and reproduction

Run from the worktree, prefixing commands with `rtk`:

```powershell
rtk proxy powershell -ExecutionPolicy Bypass -File ArtSource/SnowyMountainStaged/Stage05/Scripts/BuildNative.ps1
rtk proxy powershell -ExecutionPolicy Bypass -File Tools/Validate-Unity.ps1
rtk proxy powershell -ExecutionPolicy Bypass -File Tools/Test-ForestFoundation.ps1 -Filter ForestFoundationPlayTests.SnowyMountainStage05BoardFrameReview -Graphics
```

The LibreSprite build always uses a fresh destination, saves editable documents, then reopens and exports them. It never overwrites approved artwork. Python QA scripts inspect outputs and compose review sheets only; native artwork edits happen in LibreSprite.

Actual Unity capture coverage: 720×1280, 1080×1920, 1080×2400, 1080×2400 with a 24px side / 96px lower / 120px upper safe inset, and 360×640. Checks include shared layout/feet/bars/buttons, all board sprite identities/transforms/colors, integer environmental and frame pixel scale, portrait clipping, binary alpha, exact editor roundtrips and protected-file hashes. Screenshot color membership allows only the previously user-approved one-byte Unity linear/sRGB conversion; native files and geometry remain exact.

The existing 360×640 camera warning / crowded labels and legacy cell appearance / board sampling are retained baseline behavior. These are Unity Editor screenshots, not Android or real-device tests.

Open `Review.html` for paired screenshots and native assets. Machine-readable evidence is under `QA/` and `Preview/Measurements.json`.

**Stop here for visual review. No Stage 6 and no merge.**
