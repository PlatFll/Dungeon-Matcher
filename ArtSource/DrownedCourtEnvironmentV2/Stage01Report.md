# Stage 01 — deep-water atmosphere and distant ruins

Status: **CANDIDATE — technically checked, awaiting user review.** Stage 02 has not begun. No production integration or merge.

Five independent layers: an 8-color dark-water base (360×240), a 4-color distant ruin layer (256×192), a 5-color nearer silhouette layer (256×192), separate light shafts (360×240, 13 colors), and eight quiet static particles (360×240, 1 color). These are staging-background assets, not the later foreground columns, statue, platform, frames or reef scenery.

PixelLab Pro Flash produced two original transparent ruin sources, with the verified native Rattlebones image supplied through style_image.base64 for rendering guidance. Each cost 9 generations; **18 used, 624.20 remaining**. The quoted total and balance delta agree. Full prompts, jobs, raw files and hashes are retained in RawPixelLab/Stage01/Lineage.json and the job ledger. No additional reference image was uploaded for these jobs.

LibreSprite authored the water, shafts and particles directly and cleaned both ruin layers. The far layer's bright central mound was removed to avoid competing with the future combatants; native dimensions stayed unchanged. The middle layer retains the raw alpha silhouette. Original PixelLab files are immutable. Five .ase files re-export to identical RGBA pixels. All layers have opaque or binary transparency, with no interpolation.

The existing art-only Unity scaffold uses five separate Images, uniform native scale 2 at 720px width, 3 at 1080px and 1 at 360px. It stays out of build settings and contains no BoardController. Unchanged existing Rattlebones, Reef Spearman, Hammerhead Bruiser and Pearl Thief sprites are temporary read-only contrast overlays; no characters were generated or edited. The saved preview scene has those overlays disabled by default. The board, portrait and lower HUD still show the Stage 00 staging shell; later asset families are deliberately absent.

## Verification

- Actual Unity camera captures: 360×640, 360×800, 720×1280, 720×1600, 1080×1920 and 1080×2400; plus before and layers-only renders.
- Unity importer checks: native dimensions, exact opaque RGB and alpha, Point, FullRect, 64 PPU, uncompressed, no mipmaps or NPOT scaling.
- Native cleanup checks: source/export pixel equality, binary transparency, unchanged imported PNG bytes; camera-rendered background pixels equal the nearest-neighbor native composite in the unobscured test region.
- Tall upper composition is pixel-identical to the normal upper composition. Tall staging uses the measured Stage 00 anchor deltas, retaining 545.099×545.099 board bounds and 195×406 portrait dimensions at 720px reference width.
- Production and settings preservation verified against the Stage 00 7,839-file hash snapshot. Incidental Unity importer rewrites to three pre-existing consumable metadata files are excluded/restored in this isolated checkout.
- Tools/Validate-Unity.ps1 run after the editor preview builder; see QA/Stage01/UnityValidationReceipt.json for the actual result and log path.

These are **static art-only Unity renders**, not new gameplay, safe-area interaction, Android or device tests. The legacy Court scaling and portrait-mask integration issues recorded in Stage 00 remain deferred to the separately approved production integration stage. Lower-HUD staging is the existing baseline snapshot; this stage does not certify tall runtime HUD layout. Animation, pause, reduced-motion and saved-run tests are not applicable to these static background candidates and were not run.

## Artistic assessment (subjective)

7/10 for this background-only stage: dark connected water bands and muted columns establish underwater depth, while the center remains quiet and the unchanged actors read clearly. The mockup's brighter central statue, substantial foreground architecture and coral richness belong to later stages; this is not claimed as a finished visual match. The stepped water bands are intentional native clusters, and the transparent shafts sit behind the ruins rather than tinting the characters or board. Optional fish were omitted to keep the composition restrained. User approval is still required.
