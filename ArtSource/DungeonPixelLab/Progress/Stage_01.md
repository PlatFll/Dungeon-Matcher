# Stage 01 — DONE; stopped for user review

2026-10-19. Wall masonry family only. Draft PR #203, branch `feature/dungeon-zone-pixellab-asset-library`. Stage 02 and later have not started. No merge or production scene integration.

## Delivered

- One new **64×64 PixelLab Pro Flash source**, preserved unchanged in `Sources/Stage01/Wall_Raw_PixelLab.png`.
- Four **48×48 native LibreSprite wall variants**: quiet, subtle violet stone, restrained bevel wear, and deeper inset stone. They are distinct edits, not cyclic shifts. Each course is 12 native pixels high, with a 24-pixel brick pitch.
- Ten matching edge/corner/viewport closure pieces, all exact native subsets. Four 24×24 corners, two 24×48 sides, two 48×24 ends, and 48×16 / 24×16 bottom closures. These close the material field without adding pillars, artificial borders, or a floor family.
- Fourteen editable `.aseprite` documents and fourteen native PNG exports. The six-color family is opaque: alpha 255 throughout.
- A 6×4 alternating field at native 1× and nearest-neighbor 4×; source-versus-cleaned contact sheets; real Unity Game View captures at 360×640 and 1080×1920.
- The existing isolated `ArtPreview_DungeonPixelLab` now contains this wall family only. It remains outside build settings and contains no actors, gems, input or gameplay owners. Production Dungeon art and UI layout remain unchanged.

## Source and cleanup decisions

Current guide v1.17.2 and golden-register hashes were rechecked, including exact current production extractions. Actual Rattlebones, Farmer, Pan and applicable UI PNGs were viewed. The actual native Rattlebones PNG bytes were supplied to PixelLab's `style_image` with outline/detail/shading enabled and color-palette copying disabled. Supporting references informed visual review; Flash accepts only one style image, so no claim is made that Farmer/Pan were uploaded.

Job `6a393bed-69f0-45d1-a4e4-0ef2987ea1a6`, image `455f94be-a14b-5ca2-9866-a9f70d5543f5`, seed 310101. Exact prompt, payload, quote and receipts are in `Stage01_PixelLab_Receipt.json` / `Stage01_PixelLab_Completion.json`. Official tool capabilities were checked before dispatch.

The generated result supplied useful quiet rectangular stone planes, but its outer edges did not form a reliable repeat. LibreSprite copied its complete native brick cluster (source region x14–37, y23–32), repaired mortar and slim bevels, and assembled a new 48×48 repeat. This is **native cluster reassembly and editing, not a scaled-down 64×64 image**. The original brick pixel pitch is retained. Broad source wedges were restrained and the base plane cooled/darkened to sit behind HUD content. Variants alter one interior stone or a connected bevel segment; shared perimeter profiles stay locked.

Material colors: mortar `#151423`, recess `#201D33`, indigo face `#28283F`, slim lit bevel `#35324F`, subdued violet face `#302940`, violet bevel `#413451`. These are local wall candidates, not changes to the canonical global palette.

## Validation actually performed

- All 14 decoded LibreSprite RGBA cels equal their separately batch-exported PNGs exactly. New Unity candidate PNGs equal source exports byte-for-byte.
- All 16 ordered variant pairings have exact matching left/right and top/bottom boundary pixels. All half-module/corner closures reconstruct their specified source portions. No cyclic-shift duplicates.
- All 112 placed preview sprites fit fully within the 360×640 field. Independent coverage check: zero overlaps, zero holes, no cropped sprite bounds.
- Explicit Unity test `DungeonPixelPipelineStage01Tests.Stage01WallNativePreview`: **1 passed, 0 failed**. Real Game View at 1× and 3×; integer alignment, exact native dimensions, white tint, no gameplay owners, unchanged Game scene and build settings.
- Independent predicted native assembly versus actual Unity captures: **zero differing RGBA pixels** at both sizes (230,400 and 2,073,600 pixels respectively).
- Import settings: Point, FullRect, 64 PPU, no mipmaps/compression/NPOT resizing or Android override. `Tools/Validate-Unity.ps1`: **SUCCESS**, Unity 6000.3.19f1. Receipts and exact tested hashes: `Diagnostics/Stage01/`.
- All eight pre-existing modified/untracked files in the user's main checkout retain their starting hashes. Three incidental whitespace-only consumable metadata rewrites in the clean worktree were inspected and restored.

Initial native scripting attempt used batch mode, where this build did not provide an active sprite. The established hidden native-script process fixed that. Full-canvas cel allocation was then corrected and independently checked before accepting exports. Failed drafts were never imported. No claim is made that an MCP editor bridge was used; the actual tool was the installed LibreSprite native editor and batch exporter.

## Artistic verdict and limits

**Ready for user review.** The wall is quiet, cool and coherent at native size, with readable staggered stones and restrained top-left bevels. It borrows the cast's economical connected rendering, not Rattlebones's subject or palette. No cracks, torch glow, props, perspective floor, new portrait inset or decorative noise are baked in. Its regular masonry rhythm is deliberate; small accent/wear variants interrupt repetition without changing architecture. Visual approval remains the user's decision.

The Unity evidence is from the isolated art preview, **not the live battle or a phone build**. Stage 00's production per-axis scaling issue is still unresolved and must be handled at the later authorized integration stage. This stage does not certify final dressed-scene lighting, later materials, or gameplay placement.

## Cost and stop

One generation job; quoted and actually charged **5 generation-equivalents**. Balance changed from **1,054.20 to 1,049.20**. No retry. Cumulative staged-pack cost **5 / 35**, with 30 remaining in the soft budget. Free metadata/result/balance calls are logged separately.

Stage 01 is DONE. Candidate art remains pending user review. **STOP before Stage 02.**
