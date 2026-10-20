# Stage 15 — final pixel QA and handoff

Status: **Stage15 approved by the user for Stage16 integration testing on 2026-10-20**.
This document preserves the Stage15 art-only evidence. See [the Stage16 report](Progress/Stage_16.md) and [actual gameplay gallery](Stage16Review.html) for the current integration review. Draft PR #203 remains unmerged.

## Requested correction
Removed only the extra right Support_Corbel instance at native (324,210). Its source sprite and the other two supports remain intact. The before/after Unity captures differ by 267 pixels, all within that instance's footprint. The newly exposed wall is continuous. No approved artwork was recolored, reshaped, regenerated or resampled.

## Technical evidence
- 102 library PNGs: exact match to editable native cels, exported PNGs, Unity copies, and **102 fresh LibreSprite command-line re-exports**. No editor document was altered. All binary alpha; per-asset dimensions, occupied bounds, colors, hashes and GUIDs are in the final manifest.
- All 456 protected art/source/metadata files hash-identical to the Stage14 baseline. All 102 importer settings checked in Unity: Point, FullRect, 64 PPU, no mipmaps, no NPOT rescale, uncompressed, no Android override. The three unchanged existing HUD icons retain their established 100 PPU import; explicit native image dimensions preserve their pixels.
- 87 legal edge comparisons; 10 wall closure/crop checks; matching support joins; 3,088 uninterrupted gutter rows. Local torch overlays affect 2,847 approved stone pixels without replacing mortar. Portrait pipes have equal centered five-pixel profiles.
- Nine responsive frames tested at original, +1/+1 and +20/+32 sizes: 27 cases. Corners retain native size, placement stays integer, and components stay within frame bounds.
- All 64 board cells match their native 35×35 source. Oversized magenta portrait probe: zero changed pixels outside the enabled mask; disabling it produces 18,522 outside pixels, confirming the test can detect leakage. Removing the probe restores the exact final image.
- One fresh Unity test passed, capturing seven standard/tall/safe-area configurations. 360×640 matches 720×1280 and 1080×1920 exactly at integer scales; 360×800 matches 1080×2400. Odd safe-area dimensions preserve the content pixels. Board, bottom HUD and text pixels remain exact after translation.
- Thirteen selected source/camera comparisons cover props, tiles, frame corners and existing HUD symbols. These are sampled rendering comparisons, separate from the exhaustive 102-asset file/import audit.
- Tools/Validate-Unity.ps1 ran with Unity 6000.3.19f1 after scene identity preservation; Unity returned 0 with no validator failure markers. See the validation receipt for the log and source hashes.
- Saved scene preserves all 1,678 remaining serialized objects with no additions. Only the requested object's four component records were removed; the manifest binding was updated. The tested and delivered scenes are equivalent apart from preserved IDs and token-identical whitespace.

## Artistic verdict
The approved library is clean and coherent at native size. Dark cool masonry supports the lighter metal frames, violet stone supports and warm torch accents. The quiet portrait recess separates its future actor from the wall, while small skull, bone and barrel clusters remain secondary.

Rattlebones remains the rendering authority: crisp contours, deliberate compact clusters and controlled highlights. Farmer and Pan support the same restrained material handling. The environment does not borrow their anatomy or impose their palettes on every material. The first accepted stone, metal and fabric ramps remain unchanged.

The intended scene reads as a flat side-view dungeon with a shallow standing ledge, clear board boundary and separate modular UI. It is deliberately less textured and less softly lit than the historical rejected composite/mockup. Torch lighting has angular pixel clusters rather than a soft atmospheric falloff. Repetition is visible in the quiet wall, especially in taller gaps; that is a tradeoff of the approved reusable modules, not hidden by a new texture pass. The chain seed needed substantial native reconstruction earlier; this handoff does not represent it as successful raw art.

The final removal improves the right board corner's visual continuity. No new visual defect requiring a change to approved sprites was found in this pass. This is an internal artistic assessment, not a substitute for the user's final approval.

## Scope and remaining blockers
This is an **art-only Unity preview**, not the production Dungeon battle or a phone build. Game.unity, build settings, existing runtime/gameplay owners, menus, other zones, characters and gems were not changed by Stage15. The scene contains no RunSession, BoardController, actors, interactive buttons or event system; no gameplay/save behavior was exercised. Existing dirty files in the user's main checkout are preserved.

At the Stage15 handoff, production layout mapping remained a separate integration task: the Stage00 minimum-width/fractional-layout caveat must be resolved and verified against the actual gameplay UI and target phones before claiming a production-ready integration. That approval was subsequently given; Stage16 evidence is linked above. No merge is authorized here.

## Handoff
- [Review gallery](Review.html): actual final Unity PNGs, tall/safe-area views, before/after removal, historical rejected-build comparison, native contacts, raw-versus-clean pairs and golden references.
- [Manifest](Progress/Asset_Manifest.json): stage15_assets holds all 102 final entries with raw-job lineage, editable/export/Unity paths, palette, size, GUIDs and QA status. Historical stage records are retained.
- [Ledger](Progress/PixelLab_Ledger.csv): 14 paid source requests across the pack, 65 billed generation-equivalents total. Stage15: zero generation calls and zero spending. Free balance query returned 989.20.
- [Library audit](Diagnostics/Stage15/LibraryQA.json), [pixel validation](Diagnostics/Stage15/PixelValidation.json), [mask proof](Diagnostics/Stage15/MaskAndRestoration.json), [scope review](Diagnostics/Stage15/FinalReview.json), [validator receipt](Diagnostics/Stage15/ValidationReceipt.json).
- [Actual Unity GIF](Previews/Stage15/Unity-Meter-QA-Preview.gif): 17 lossless frames of a temporary meter-fill QA demonstration. It is not gameplay or a new production animation. Final still has the meter restored to zero.

To inspect: open Assets/_Game/Scenes/ArtPreview_DungeonPixelLab.unity in the isolated branch. It is intentionally excluded from release Build Settings. For verification, the explicit DungeonPixelPipelineStage15Tests.AssemblyPreview test recreates captures; then run Scripts/verify_stage15.py using the Foundry Python environment and Tools/Validate-Unity.ps1. Test rebuilding changes transient scene IDs; preserve serialized identities before committing a rebuilt preview.

**Stage15 review was accepted for integration testing. Stage16 gameplay visual approval remains pending.**
