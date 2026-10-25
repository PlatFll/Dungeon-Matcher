# Stage 02 — drowned monarch and submerged court architecture

**CANDIDATE — technical checks passed; awaiting user review.** Stage 03 has not started. No production integration or merge.

## Deliverables and lineage

Ten native editable assets: crowned trident monarch statue, broken court arch, cleaned whole-column seed, column capital, repeatable shaft, worn shaft, column base, leaning broken column, ruined ledge, and a connected rubble joint. The intact column is assembled from separate parts in Unity; the statue and arch remain independent sprites. The layer composition is an inspection artifact only.

Three original PixelLab Pro Flash sources cost **6 generations each / 18 this stage / 36 cumulative**. Account balance moved **624.20 → 606.20**. Each request supplied the verified native Rattlebones PNG as the actual style_image.base64 input, for rendering discipline only. The art guide/register and golden-image hashes, exact prompts, settings, palettes, seeds, jobs, raw hashes and completion receipts are recorded in RawPixelLab/Stage02/Lineage.json. No mockup or character replacement was generated.

LibreSprite converted the statue to six quiet stone shades and the column/arch to a coherent seven-shade stone family, simplifying 83/46/72 isolated interior pixels respectively. The three original alpha silhouettes are exact. Separate column connectors were centered and authored at native pixels; worn cracks keep both connector rows intact. The leaning broken variant uses explicit integer row offsets from the same column, with a native snapped top. The ledge and rubble are direct native masonry constructions using the cleaned stone palette. No interpolation or source rescaling occurred.

## Verification executed

- Tools/Validate-Unity.ps1 passed with Unity 6000.3.19f1. The Stage02 preview builder completed with its pixel/import/placement assertions passing.
- All ten .ase sources re-opened and exported identically, including RGBA pixels. All ten imported Unity PNGs are byte-identical to exports. Binary alpha, exact native dimensions, Point filtering, FullRect, 64 PPU, no mipmaps, NPOT rescaling or compression.
- Capital/shaft/worn/base connector rows match exactly. Native repeated stacks and side-by-side ledge tests have continuous joins. Touching top/bottom edges on these modules are intentional connection boundaries.
- All placed sprite canvases fit entirely within the 360×240 native upper scene; no cropped statue, trident, base or arch. Architecture is behind the read-only actors and shared controls. The portrait staging mask remains above environment.
- Actual Unity art-only camera captures at 360×640, 360×800, 720×1280, 720×1600, 1080×1920 and 1080×2400, plus before and layers-only views. New art renders uniformly at native 1×, 2× or 3×. An unobscured camera region equals the nearest-neighbor native composition exactly; normal/tall upper views are pixel-identical.
- The before render is identical to accepted Stage01. **62 accepted Stage01 files and all 7,839 protected production/settings files are unchanged.** The new scene is additive, excluded from build settings, and contains no BoardController. Shared 545.099×545.099 board and 195×406 portrait reference dimensions are retained.

These are static art-only renders. Characters are unchanged read-only contrast overlays. This stage does not run actual combat, live health/countdown UI, safe-area interaction, animation or Android/device tests; those integration checks remain later work. Board and portrait art, the standing floor and lower environments still use the Stage00 placeholders. No new character, gem, shared UI, gameplay, save, other-zone or production binding changes are included.

## Artistic review

Subjective **7.5/10 for this architecture stage**: the crown and trident establish the drowned royal court, the arch frames its lower carving, and intact/broken columns give readable asymmetric structure. The quiet statue colors recede behind the unchanged bright character faces. Large stone facets survive native 1× review. The modular joins are clean and the stone footings meet the existing floor guide. The mockup's coral, kelp, statue-adjacent dressing and complete floor are intentionally later stages; this is not a finished scene claim. Review the statue on its plain background and in the actual Unity composition before approving.

Review: Preview/Stage02/Review.html. Native sources and placements: Native/Stage02/Manifest.json. Verifier: Scripts/Stage02/VerifyStage02.py. The accepted Stage01 scene, artwork and previous review files remain intact.
