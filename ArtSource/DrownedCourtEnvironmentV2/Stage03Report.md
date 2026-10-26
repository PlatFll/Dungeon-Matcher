# Stage 03 — submerged court walkway and front platform

**Candidate complete; awaiting user review. Stage 04 not started. No production integration or merge.**

## Result

Twelve modular floor pieces plus one cleaned PixelLab material seed: three 64×16 walkway variants, an 8×16 filler and two 16×16 end corners; three 64×26 fascia variants, an 8×26 filler and two 16×26 end corners. A 360×42 native platform is assembled from 16 separate sprites. Unequal slab widths, sparse connected wear and a subdued lower face keep the actor lane calm. The previously accepted rubble joint remains the small right-side rubble accent.

The native walkway occupies y198–214; the existing enemy-foot baseline remains y205.5 within that lane (global Unity y865). The fascia occupies y214–240. The floor spans the full upper scene width behind the unchanged portrait mask. Columns, arch, actors and shared UI render above it. All Stage01/02 artwork, sources, scenes and previous review images remain byte-identical.

## PixelLab and LibreSprite

One original 128×64 PixelLab Pro Flash source, job 5a52f95f-8825-486e-ae23-bfe198ae817c. The actual verified Rattlebones PNG was supplied in style_image for rendering discipline only. The accepted Stage02 stone palette was supplied separately. Cost: **6 generations; 606.20 → 600.20 remaining; 42 total for Stages01–03**. No later-stage generation occurred.

The raw PixelLab seed is preserved. LibreSprite softened its brightest bevel shade and removed isolated interior specks while retaining the exact alpha silhouette. Exact native row profiles, deliberate internal joints, worn-bevel samples and end corners were authored in LibreSprite. Six opaque colors across the cleaned source and modules, from the accepted stone ramp. No interpolation or resampling. PNG working canvases are blank source scaffolds, not generated art. Every final sprite has an editable .ase source.

## Executed technical checks

- Unity 6000.3.19f1: `Tools/Validate-Unity.ps1` completed successfully. The Stage03 preview builder passed its actual import/pixel/placement/layer/geometry assertions.
- Thirteen .ase sources reopened and exported with exact RGBA equality. Imported PNG files match exports byte-for-byte; imported opaque pixels and alpha match. Binary alpha; Point filtering, 64 PPU, FullRect, no compression, mipmaps or NPOT scaling.
- All 50 valid horizontal module-edge pairings match exactly. The assembled 360×42 platform is fully opaque with no gaps. Connector/end canvases intentionally touch their boundaries; no part of the assembled floor lies outside the upper scene.
- Actual Unity camera captures at 360×640, 360×800, 720×1280, 720×1600, 1080×1920 and 1080×2400, plus before and layers-only views. New artwork is uniformly 1×, 2× or 3×. Sampled scene/floor regions equal the native nearest-neighbor composition exactly; normal/tall upper captures are identical.
- Before capture equals the accepted Stage02 capture. Lower scene pixels and opaque portrait interior are unchanged. The existing RectMask2D shader blends its one-pixel boundary with the new floor; the mask, dimensions and interior remain unchanged. This existing UI boundary is not new sprite anti-aliasing.
- **160 accepted Stage01/02 files and 7,839 protected production/settings files unchanged.** In the new scene, the only changes to existing serialized objects are the canvas child list (new floor root) and hiding the obsolete floor guide. No BoardController; scene excluded from build settings. Board remains 545.099×545.099 and portrait 195×406 reference units.

These are static art-only Unity renders with unchanged character contrast overlays. This stage does not test live combat, health/countdown/AIR overlays, safe-area input or Android/device behavior. Board and portrait frame art and lower scenery remain earlier placeholders; they belong to later authorized stages. Final live overlay alignment must be tested during production integration.

## Artistic verdict

Ready for floor-stage review: the level lane reads clearly beneath all three enemy silhouettes, broad front blocks match the approved column stone, and restrained joints avoid a noisy receding floor grid. The quiet values keep the actors brighter than their support. The mockup's ornamental coral, kelp and surrounding dressing are later stages; this is deliberately an uncluttered structural floor, not a finished-zone claim. User approval remains pending.

Review: Preview/Stage03/Review.html. Native inventory and placements: Native/Stage03/Manifest.json. Exact verification: QA/Stage03/Verification.json. Unity and native logs, source receipts and cost evidence remain in QA/Stage03 and RawPixelLab/Stage03.
