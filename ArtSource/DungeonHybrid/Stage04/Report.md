# Stage 04 — props and dressing

Completed for user review on 2026-10-22 (Asia/Amman). Stages 01–03 are approved. Stage 05 has not started. **No merge or production integration.**

Open `Review.html` for actual Unity captures, cast visibility, two dressing arrangements, native asset files and equal-scale raw/clean comparisons.

## Delivered

Seven separately generated PixelLab Pro Flash sources, cleaned in LibreSprite and exported as ten sprites:

| Family | Native exports | Cleanup |
|---|---|---|
| Torch | 24×54 | Consolidated iron shadows/highlights; retained warm flame clusters. |
| Chain | 14×25 anchor, 14×14 repeat, 14×14 end | Consistent violet iron; modular complete links and transparent clearance around assembled chain. |
| Banner | 37×52 | Quiet plum cloth, muted gold emblem, intact pointed hem. |
| Barrel | 40×45 | Broad warm wood ramps and restrained cool iron hoops. |
| Jar | 40×37 | Six controlled matte violet-clay colors. |
| Skull / bones | 31×30 and 32×28 | Six muted bone colors; the short fat bones retain their original shape. |
| Rubble | 48×29 | Five subdued dungeon stone colors. |

All artwork remains native size; no resampling, interpolation, semitransparent AA or new scene-sized texture. Every source silhouette is preserved during palette cleanup. Skull and bone components are separated by their disconnected masks without losing pixels. Complete standalone props receive 2px transparent clearance. Chain connection edges deliberately touch; complete assembled chains have 2px clearance on every outer edge.

The seven raw sources used **35 subscription generations** (7 × 5); balance changed from 966.20 to 931.20. The actual authorized Rattlebones reference upload was reused. Requests, job IDs, native files, hashes and a generation receipt are included. No further generation is running or planned for this stage.

## Preserved / scene placement

All 765 protected files, including the approved 102-asset production library and earlier hybrid work, remain byte-identical. The eight pre-existing main-checkout changes are also unchanged. No production scene, presentation prefab, character, gem, gameplay system, other zone or build setting was edited.

`Assets/_Game/Scenes/ArtPreview_DungeonHybrid_Stage04.unity` opens a copy of the approved Stage 03 top study and adds separate native prop sprites. New props render behind the static cast and UI. Two alternatives demonstrate skull/bones/barrel or jar/rubble without filling the floor with every prop. Floor contact is native y183 within the existing y178–189 platform surface; ground props have transparent padding below.

The right chain was shortened by one repeat after visual inspection to separate its end from the gate crest. The stage's wall, platform, gate, portrait mask and frame geometry remain unchanged. The art-only scene is not included in production build settings.

## Verification actually performed

- LibreSprite native JS edits, editable ASE saves, CLI crops without scaling, then ASE reopen/export pixel equality.
- Independent PNG checks: dimensions, source alpha-mask preservation, binary alpha, 5–11 colors per export, full standalone clearance, intact skull/bone component reconstruction.
- Modular chain checks with zero, one, two and six repeats; connected seams and complete endpoint clearance.
- One explicit Unity art-preview test passed, producing **15 actual Game View screenshots**: five views at 360×240, 720×480 and 1080×720.
- Screenshots at 2× and 3× match exact nearest-neighbor replication of their native captures. The no-prop baseline exactly matches the approved Stage 03 screenshot. Uncovered gate, fascia and character-face regions remain pixel-identical.
- Unity test assertions cover point filtering, native dimensions, integer positions, bounds, tiled-image sampling, text truncation, draw order and absence of gameplay/input owners.
- `Tools/Validate-Unity.ps1` passed with Unity **6000.3.19f1** after the final chain-placement correction. Evidence: `Diagnostics/UnityValidation.json`, `UnityTests.xml`, `Verification.json`; full local logs sit beside them.

This is an art-only Unity runtime preview with static approved cast, **not production combat integration, Android testing or real-device testing**. Numeric checks supplement the visual review; the new props remain candidates until the user approves.

## Files / handoff

- `Sources/`: seven raw PixelLab PNGs and exact request/job records.
- `Native/`: seven cleanup documents and ten padded modular ASE documents.
- `Exports/`: ten transparent native PNGs.
- `Previews/`: native catalog, comparison board, golden references, chain assembly checks and actual Unity screenshots.
- `Scripts/`: reproducible LibreSprite cleanup/export and independent review/verification scripts.
- `Modules.json`, `AssetLedger.csv`: dimensions, color counts, hashes and intended Unity paths.
- `Assets/_Game/Scripts/Editor/DungeonHybridStage04Tests.cs`: isolated scene builder and preview test.

Dedicated branch: `feature/dungeon-zone-hybrid-pixellab-rebuild`. Draft PR #205. Stop here for Stage 04 visual approval; do not start Stage 05 or merge.
