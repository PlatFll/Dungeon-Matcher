# Stage 04 — underwater portrait frame and recess

**Candidate complete; awaiting user review. Stage 05 has not started. No production integration or merge.**

## Result and preserved geometry

Sixteen editable native assets: four 15×15 stone corners, four 7-pixel-thick repeatable rails, a 9×9 dull silver socket, two 64×64 recess tiles, barnacles, two sparse algae stems, a coral sprig and the cleaned cornerstone material study. Fourteen distinct pieces are used in the scene; RecessB and CornerMotif remain alternate/study assets.

The shared portrait outer rectangle remains **195×406** at (21,825), and the board stays **545.099×545.099**. The existing ResponsiveModularFrameFitter is reused without modification. Corners remain 15×15 native pixels while rails repeat; no mirrored lighting or stretched corner artwork. Its existing rounding maps the decorative native width 97.5 to 98. This one-reference-pixel decorative overhang does not change the portrait's layout rectangle or touch/UI bounds.

The dark fill overflows its inset mask by 128 reference pixels on every side. The mask overlaps under the opaque rails, eliminating fill gaps without tinting the battle environment. Marine accents attach outside the portrait, with no plant over Rattlebones's face. No banner was added.

## Generation and native work

Two original PixelLab Pro Flash 64×64 sources, five generations each. **10 generations spent; 600.20 → 590.20 remaining; 52 cumulative**. Actual verified Rattlebones PNG supplied as the rendering reference; anatomy and palette not copied. Raw files, complete prompts, job receipts and hashes are preserved in RawPixelLab/Stage04/Lineage.json.

LibreSprite authored the final .ase files and exact PNG exports. Strong cyan bevels were subdued, isolated recess noise removed, repeat boundaries repaired, and the original metal socket/barnacle pixels reused in separately authored native pieces. Whole source silhouettes were retained. No resampling, blur, flattening of the scene, or production replacement. Recess tiles use four opaque colors; the complete material family uses 21 meaningful opaque colors including metal, coral and algae.

## Executed verification

- Tools/Validate-Unity.ps1 **passed** with Unity 6000.3.19f1. Validator log preserved in QA/Stage04/UnityValidation.log.
- Preview builder passed import pixel/alpha equality, shared geometry, layer-order and native frame checks. Frame tested at three sizes without corner scaling; rail/socket centerlines stay aligned.
- All **16 LibreSprite sources** reopened and exported with exact RGBA equality. Unity PNGs byte-identical to exports; imported opaque pixels and alpha exact. Point, 64 PPU, FullRect, uncompressed, no mipmaps or NPOT resampling.
- **20 edge pairings** passed for recess repeats, rails and corner exits. Independent rendered corner comparison matches source pixels at 2× exactly.
- Magenta overflow probe: **54,656 changed pixels, zero outside the inset mask**. No recess leak into the environment.
- Six review sizes: 360×640, 360×800, 720×1280, 720×1600, 1080×1920, 1080×2400. Native environment/frame pixels use uniform 1×, 2× or 3× mapping. Normal/tall upper renders match.
- Before capture exactly equals accepted Stage03. All pixels outside the portrait/accent bounds are unchanged. **280 accepted Stage01–03 files and 7,839 protected production/settings files** remain byte-identical, along with five current authority/reference files.
- Only three old serialized scene objects differ in the new scene: canvas child list, portrait-mask child list, and hiding its previous flat recess. New scene has no BoardController and is excluded from build settings.
- An explicit Unity play-mode snapshot test loaded current Game with a disposable profile and passed (1/1). It captured the nine existing player/affinity/HP graphics at their measured positions and source sizes. These are replayed read-only in the HUD inspection mode. No production scene/save was written. Shield was not visible in that starting profile.

## Scope of the screenshots

Actual Unity **art-only camera renders**, not an integrated playable zone. The default contrast mode retains the same actor overlays as accepted Stage03. The separate HUD mode shows Rattlebones and the player HUD at the captured production 720×1280 sizes (Rattlebones 64×64); its preview camera proportionally scales that snapshot at other sizes. This does not certify the existing production actor/UI pixel mapping at every device size. No live combat, AIR/countdown behavior, safe-area input, pause/flooding/resume or Android/device test was performed. Those remain integration checks. No ambient effects were introduced.

## Artistic judgment — subjective

**8/10 for this portrait stage.** The cool architectural rails and dull sockets read as the Court counterpart of the established shared frame family. The quiet broad masonry planes support the ivory skull, red cape and bright affinity gem. Sparse attached growth provides underwater identity without obstructing the player. The mockup's fuller surrounding coral and lower scenery belong to later stages; the scene is intentionally unfinished outside this stage. Candidate status is retained for the user's visual decision.

Review: Preview/Stage04/Review.html. Manifest: Native/Stage04/Manifest.json. Exact checks: QA/Stage04/Verification.json. Builder assertions: QA/Stage04/UnityPreview.json. Disposable HUD test: QA/Stage04/HudSnapshotTests.xml.
