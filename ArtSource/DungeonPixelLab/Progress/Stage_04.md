# Stage 04 — DONE; stopped for user review

2026-10-19. Front platform fascia only, following the user's approval of Stage 03. Draft PR #203. No Stage 05, production integration, gameplay/UI layout changes or merge.

## Delivered

- Five editable LibreSprite documents and independent PNG exports: `Fascia_Left` **42×24**, `Fascia_Right` **62×24**, `Fascia_Mid_A`, `Fascia_Mid_B`, and `Fascia_Anchor` **64×24**.
- Six placed modules make a **360×24** front course at native **(0,190)**. It extends behind the portrait, beneath the unchanged floor at y178–189. The portrait/frame draw above it. The wider anchor interrupts repetitive joints without changing height.
- Broad vertical violet stone planes, restrained upper-left bevels, sparse connected corner wear, and a continuous dark underside. Twenty-two opaque rows plus two transparent footer rows let the unchanged wall continue behind it. No additional floor rows or perspective recession.
- Actual Unity normal, silhouette/contact and restored captures at **360×640** and **1080×1920**; native/2× source-versus-cleaned contact sheets; all legal neighbor joins; full-width strip.

## PixelLab origin and LibreSprite work

Stage 04 explicitly directs reuse of Stage 03's generated source when it already contains side-view edge geometry. The source qualifies: PixelLab sidescroller tileset **fd7b1bf8-270d-447c-a135-177884748b06**, base tile **74904d1b-9197-4c89-afd7-c88fd563734b**, includes the vertical support brick at **x96–111, y23–27**.

The original sheet and metadata are copied byte-identically into `Sources/Stage04/`. No old production artwork is repackaged as newly generated art. Native LibreSprite construction uses the generated brick's edge/shadow language, maps its material roles to the accepted platform ramp, extends quiet planes into larger structural stones, and deliberately redraws joins/chips. These are substantial native cleanup/construction edits, not untouched PixelLab outputs or rescaled tiles.

Five meaningful opaque colors: `#151423`, `#241F3A`, `#382D55`, `#4E3D6C`, `#67537D`. Four are shared with the approved floor; the deepest contact shade belongs to the existing wall family. No face/character palette is imposed on stone.

Actual current Rattlebones, Farmer, Pan and applicable UI pixels were inspected; guide/register/reference and production source hashes match. Stage 03's specialized endpoint has no image-reference input. References were checked locally; no image upload is claimed. Official tool documentation and a free live balance call were checked. **Zero new jobs, zero charge**, balance **1041.20 → 1041.20**; cumulative pack **13/35** remains unchanged. No Sprite Fusion or OpenAI generation.

LibreSprite authored native cels and saved five .aseprite files. The existing background-layer flag helper changes only layer flags; independent LibreSprite batch exports match the decoded native cels exactly. Source and export logs remain local under Diagnostics/Stage04.

## Validation actually performed

- Five native cels equal PNG exports exactly; import candidates are byte-identical to exports. All alpha is **0 or 255**; no interpolation/resampling or partially transparent edges.
- **16/16 legal neighbor combinations** have identical connecting profiles, including all middle/anchor combinations and both ends. End canvases are authored at their final widths, not clipped.
- Six fascia rectangles exactly cover x0–359 with no overlapping rectangles or gaps. The opaque front course is continuous; the approved floor touches it directly at y190. No sprite bounds exceed the preview.
- **104 protected source/native/export/import/frame files** remain byte-identical. All 129 prior placements are unchanged; only six new fascia placements were appended.
- Explicit Unity test `DungeonPixelPipelineStage04Tests.Stage04FasciaNativePreview`: **1 passed, 0 failed**. Native sizes, integer positions, 64 PPU, white tint, consistent course height/width and absence of gameplay/input owners were checked.
- Plain temporary geometric silhouettes meet the unchanged **y180** floor contact. They are removed for the restored captures and are absent from the saved scene. No new actor assets.
- Six actual Unity captures (normal/contact/restored at 1×/3×) match the independent native assembly with **zero RGBA differences**, including portrait overlap and transparent footer.
- `Tools/Validate-Unity.ps1`: **SUCCESS**, exit 0, Unity **6000.3.19f1**. Exact tested hashes and the log receipt are in `Diagnostics/Stage04/ValidationReceipt.json`.
- Point, FullRect, 64 PPU, unmipped/uncompressed and no NPOT resizing. Production Game scene and build settings are unchanged. The main checkout's eight starting user edits remain unchanged. Three incidental whitespace-only consumable metadata rewrites were restored only in the isolated art worktree.

## Artistic verdict and limits

**Ready for user review.** The front reads as the thickness of one horizontal stage, distinct from the shallow walking plane. Larger connected stone planes remain quiet at 1×; the underside separates the stage from the wall below without isolated floating chunks. The portrait covers the hidden continuation cleanly. No visible join gaps, height changes or torn masonry were found.

This is an isolated Unity art preview, not a production battle/phone build. Gate, props, board and later families are intentionally absent. Stage 00's production layout/scaling concerns are still deferred.

Stage 04 is DONE, pending user visual approval. **STOP before `PROMPTS/05_GENERATE_GATE_ARCH_DOOR.txt`.**
