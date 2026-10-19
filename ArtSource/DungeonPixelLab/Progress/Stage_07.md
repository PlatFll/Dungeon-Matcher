# Stage 07 — DONE; stopped for user review

2026-10-19. Two wall torches and controlled local torchlight only, following the user's Stage 06 approval. PixelLab plus LibreSprite. No chains, banners, barrels, production integration, gameplay/UI changes, Stage 08 or merge.

## Delivered

| Module | Native canvas | Opaque colors | Role |
|---|---|---|---|
| Torch_Fixture | 32×56 | 7 | Three cool iron shades plus four small fire colors at the bowl opening |
| Torch_Flame | 32×56 | 4 | Complete yellow/orange flame and one small upper ember |
| Torch_Light_Left | 64×72 | 10 | Local warm brick clusters around the left torch |
| Torch_Light_Right | 56×72 | 13 | Local warm pillar/wall clusters around the right torch |

The fixture and flame share native registration at **(112,86)** and **(320,86)**. Both use the same front-facing drawing; no mirroring or inverted highlights. Light overlays sit at **(96,68)** and **(304,68)** beneath props/UI. Torches stay wall-mounted and clear of the unchanged floor. Twenty meaningful colors across fire, iron and lit masonry, with no universal character-palette limit imposed on the environment.

This is a static review kit. The optional flame animation was not added; no VFX behavior or timing changed.

## PixelLab lineage and native authoring

Current canonical guide v1.17.2 and golden register were refreshed. Actual Rattlebones, Farmer, Pan and applicable existing UI/source hashes match the main checkout. Rattlebones's native PNG was supplied through `style_image.base64` for outline/detail/shading, with palette copying false. Supporting cast and approved masonry were inspected locally.

One new `create_image_pro_flash` job: **69948254-6267-434c-b80a-f1bf562bdb73**, image **0fbab429-46c1-560d-93fc-74b703b3ed03**, seed **710701**. Native **64×64** source, quote **5**, completion receipt **5 charged**. Raw source is immutable; exact request, source/reference hashes and receipts are retained. No retry or provider substitution.

LibreSprite directly separated the flame from its fixture, preserved the fixture silhouette at its original pixel pitch, removed accidental orange marks from the mounting plate, simplified the iron into three broad cool tones, and repositioned one tiny ember with full clearance. Native trim/padding translation only; no scaling or interpolation. Independent LibreSprite PNG exports equal decoded native cels.

Light is authored as stepped opaque color clusters aligned to approved native wall/support modules. It is not a radial gradient, blurred glow, screenshot crop or permanent recolor of the base tiles. Different wall and pillar planes retain their geometry and material separation. Deep mortar/contour pixels remain transparent in the overlay. The fixture's fire opening remains a small emissive accent distinct from its iron body.

## Validation actually performed

- Four native source/PNG/import comparisons: exact equality, binary **0/255 alpha**, complete occupied bounds with transparent canvas clearance.
- Source fixture alpha silhouette preserved after its native translation; iron body has three colors, flame four. Exact full material palettes and raw/export/source hashes are in `Diagnostics/Stage07/PixelChecks.json`.
- Both light overlays recolor only permitted existing stone fields. Mortar and contours remain unchanged; no semi-transparent halos. Their occupied world bounds are **[104,76,153,131]** and **[312,76,358,132]**.
- Central combat band **x176–283** and the entire scene from **y142 downward** are pixel-identical to approved Stage 06. The flame's peak luminance remains more than 80 units above the strongest bounce-light color; bright accents are localized, not a washed-out wall.
- **168 protected art files** and all **194 prior placements** are unchanged. Six new placements produce 200 total pieces. All eight starting main-checkout user edits are preserved.
- Explicit Unity test `DungeonPixelPipelineStage07Tests.Stage07TorchNativePreview`: **1 passed, 0 failed**. Native dimensions, integer placement, 64 PPU, original anchors, safe canvas bounds and absence of gameplay/input owners checked.
- **Eight actual Unity captures**, normal/unlit/footprints/restored at **360×640** and **1080×1920**, match independent native composition with **zero RGBA differences**. Unlit hides only the two light overlays. Temporary diagnostic markers are absent from the saved scene.
- `Tools/Validate-Unity.ps1`: **SUCCESS**, exit 0, Unity **6000.3.19f1**, after the final editor change. Exact log and tested hashes are in `Diagnostics/Stage07/ValidationReceipt.json`.
- Full semantic scene review: 24 added serialized objects for six new image placements, zero removals. Existing changes are only module/manifest bindings and structural/props layer child lists; prior transforms and images are unchanged.
- Point, FullRect, 64 PPU, no mipmaps/compression/NPOT resizing. Production Game scene and build settings unchanged. Only inspected incidental Unity whitespace is restored in the isolated worktree.

## Artistic verdict and stop

**Ready for user review.** The compact flames add clear warm focal points on either side of the gate. The cool iron stays readable without excessive shine. Warmth follows a few local stone planes; the doorway, standing region and remaining wall stay quiet and cool. No clipped flame, blurred halo, broken mortar or floor contamination was found at native and enlarged views.

Actor assets are deliberately absent from this isolated art preview. Central stage preservation and footprint diagnostics support future actor readability; this does not claim production actor/device testing. Stage 00 production layout/scaling concerns remain deferred to integration.

Balance **1030.20 → 1025.20**. Cumulative pack **29/35**, leaving **6** before the soft-budget boundary. Stage 07 is DONE, pending user approval. **STOP before `PROMPTS/08_GENERATE_CHAINS_AND_BANNERS.txt`.**
