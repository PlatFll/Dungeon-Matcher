# Dungeon lower-corner foundations — review candidate

The user rejected the small jar/barrel/rubble/bone groups in PR #233 and requested large connected corner formations comparable in coverage to the Snowy Mountain corners.

## Result

Two 112×128 native buttress foundations replace the separate prop groups, the previous column feet and their overlapping lower candles. Each is one connected silhouette containing broad fitted stone courses, an inset candle niche and a short attached chain. The upper stem uses the current Side_Shaft pixels; an authored shoulder joins it to the generated foundation. Existing columns terminate at that native joint. The foot reaches the physical bottom edge, with outer bedrock intentionally peeking off-screen. The background remains behind the live controls.

Only the lower corner formations and their local masonry joins change in this revision. The previously extended upper pillars, board size, gems, actors, enemy floor line, UI sizes and gameplay fixes are retained. Original approved PNGs are untouched. This is bound to the actual Game scene on the review branch, not merged to main or user-approved artwork.

## PixelLab and LibreSprite lineage

- Rattlebones: rendering discipline. Dungeon Side_Shaft: purple masonry. Snowy Mountain Corner_Left: connected silhouette/coverage only.
- The user explicitly approved those three image uploads to PixelLab. Current reference paths/hashes: `ReferenceHashes.json`.
- Round 1: two 128×192 sources, 20 generations, rejected as too thin/stair-like. Preserved in `Raw/Left.png` and `Raw/Right.png`.
- Round 2: Pro job `cd9a65c5-5255-4f51-b39a-9fb7926938d6`, 128×128, four candidates, 10 generations. Candidate 0 selected as left foundation.
- Right companion: Pixen edit `672c782d-bd44-4db7-b2a5-39108cdf168c`, 1 generation. Changes handedness while retaining top-left lighting.
- Actual calls/receipts: `Generation.json`, `Generation-Round2.json`, `Generation-Right.json`.
- **31 generations total**, balance 3627.90 → 3596.90, $0 credits spent.
- Native cleanup executed in real LibreSprite via `Scripts/BuildNative.js`. Duplicate purple shades were consolidated into the existing side-stone ramp. Candle/iron accents remain quiet. The separate cap was removed and the new native column connection was drawn. No scaling, blur or semi-transparent AA.
- Editable sources: `Native/`. Exact PNG exports: `Exports/`. Run the script from the repository root with LibreSprite batch mode. The transparent blank is an authoring canvas, not a game asset.

## Verification and review

Both PNGs are 112×128, nine opaque colors, binary alpha and one 4-connected component. Imports are Point, FullRect, 64 PPU, uncompressed/unmipped. Export bytes equal their production candidate copies. Reopened LibreSprite sources reproduce the exact native pixels.

`DungeonCornerAndPillarActualGamePhonePreview` passed in the actual Game scene at 720×1280, 1080×1920, 1080×2400, 1080×2400 with simulated safe insets, and 360×640. Checks cover native scale, one sprite per side, no legacy prop group/stacked foot, joint alignment, physical screen coverage, no opaque corner pixels in the playable board, foreground UI ordering and exact control geometry. Geometry is sampled after three stable rendered frames because invisible enemy-inspection buttons follow the actor anchor during layout updates; the initial pre-render comparison was invalid. The exact assertions were retained.

Successful test evidence: `Unity-preview-tests.xml`; numeric/source checks: `ArtValidation.json`, `Verification.json`. Original rejected-layout captures: `Before/`. New screenshots: `Preview/`. The before/after gallery explicitly labels different board/animation states in historical captures. A separate same-paused-frame toggle confirms that the upper scene and board are pixel-identical when only the new foundations are hidden.

Required `Tools/Validate-Unity.ps1` passed with Unity 6000.3.19f1. Log: `C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-46acc7ea-0c6d-438c-9f42-32896a9444e1.log`.

No Android build or physical-device test. No merge. Awaiting the user's visual review.
