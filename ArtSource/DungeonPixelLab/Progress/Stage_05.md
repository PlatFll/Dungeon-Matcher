# Stage 05 — DONE; stopped for user review

2026-10-19. Central gate, arch and door only, following the user's Stage 04 approval. Draft PR #203. No Stage 06, props, production integration, gameplay/UI layout changes or merge.

## Delivered

- One new PixelLab-original **96×112** gate/arch source, preserved unchanged.
- Three editable LibreSprite layers and independently exported PNGs: `Gate_Arch`, `Gate_Leaf`, and `Gate_Hardware`, each **96×112** with shared native registration.
- One intact semicircular stone arch with emphasized keystone, straight piers, five indigo bars, one restrained brace and two small bronze fittings. The arch remains one coherent piece; no needless fragments.
- Placement at native **(177,72)**: occupied bounds **[187,84,261,180]**, center **x224**, seated on the unchanged **y180** floor contact. No room, floor, portrait or UI movement.
- Actual Unity normal, three-footprint and restored captures at **360×640** and **1080×1920**; raw/clean/layer contact sheet at 1×/2×; isolated assembly at 1×/4×.

## PixelLab lineage and native cleanup

Current guide v1.17.2, register, actual Rattlebones/Farmer/Pan and applicable UI pixels were rechecked against current source hashes. Rattlebones's actual native 64×64 PNG bytes were submitted through Pro Flash's supported `style_image.base64` input. Outline/detail/shading were enabled; palette copying was disabled. Supporting cast and current wall were inspected locally, not claimed as additional uploaded images.

One `create_image_pro_flash` job: **07e64fd5-124c-40ff-9587-e56e14c67cf0**, image **a5896943-e280-5429-8bb2-8ce89eb4f41e**, seed **510501**. Native 96×112 size was quoted at **6** generation equivalents; completion confirms **6 charged**. Raw SHA-256: `774d3d73d5bf0ad1e7b05ea799a415c13960c22d17e4cb150084f809b98e6737`. Exact payload, reference hash, completion receipt and source bytes are retained.

LibreSprite directly edits native cels. The generated crown's exterior envelope remains at the same native coordinates. Twelve authored rows extend only the straight lower piers/bars to the established stage height; there is no scaled crown or interpolated artwork. Stone uses a quieter five-tone violet ramp related to the approved wall/platform. Bar faces are straightened into clean indigo clusters. Four isolated lower-pier specks are removed, one clean stone-course joint crosses the extension, and the base is closed into one continuous contact row. Native bronze fitting silhouettes remain separate overlays above their attachment points.

All three sources share a padded canvas, making arch/leaf/hardware registration explicit. Background-layer flags are normalized using the existing helper without changing pixels. Separate LibreSprite PNG exports equal decoded cels byte for byte.

## Validation actually performed

- Native cel/PNG/import equality for all three pieces. **Binary alpha 0/255**, no AA or resampling. Every occupied bound has canvas clearance.
- Arch **5**, leaf **5**, hardware **3** meaningful colors; **12 unique opaque colors combined**. Material roles, not a global character palette quota.
- Combined occupied bounds **74×96** inside the 96×112 canvas. Crown outer-envelope equality checked row by row. All **74 pixels** of the architecture's bottom contact row are opaque.
- **126 protected files** and all **135 prior placements** are unchanged. Only three new registered layers were added.
- Explicit Unity test `DungeonPixelPipelineStage05Tests.Stage05GateNativePreview`: **1 passed, 0 failed**. Checks native dimensions, integer alignment, 64 PPU, white tint, existing floor/fascia anchors, safe bounds and absence of gameplay/input owners.
- Three temporary **20×2** footprint markers at x154/234/314 meet y180. No actor assets or fake characters. Markers are removed before restored captures and absent from the saved scene.
- Actual normal/footprint/restored captures at 1×/3× match independent native composition with **zero RGBA differences**. This includes full bronze overlays, clean arch registration and floor contact.
- The first test used an exact floating-point Vector2 equality for projected gate bounds; the resulting subpixel arithmetic difference was corrected to the preview's existing 0.01-pixel numeric tolerance. The stricter rendered RGBA comparisons remain exact. The affected test and required validator were rerun; initial failure evidence is retained.
- `Tools/Validate-Unity.ps1`: **SUCCESS**, exit 0, Unity **6000.3.19f1**, after the final editor change. See `Diagnostics/Stage05/ValidationReceipt.json` for exact tested hashes and log.
- Point, FullRect, 64 PPU, no mipmaps/compression/NPOT resizing. Production Game scene and build settings unchanged. Main checkout's eight starting user edits preserved; only inspected incidental whitespace changes are restored in the isolated worktree.

## Artistic verdict and stop

**Ready for user review.** A compact, readable gate sits firmly on the horizontal stage. The arch has larger quiet planes than the surrounding masonry; the darker barred opening recedes without tunnel perspective. The subtle bronze anchors remain small accents. No stretched curve, cut-off cap/base, broken join, floating foot or busy surface texture was found at native size.

This is an isolated Unity art preview, not a production battle or phone build. Props and later families remain absent; existing Stage 00 production layout/scaling concerns remain deferred.

One paid call, no generation retry. Balance **1041.20 → 1035.20**; cumulative pack **19/35**, leaving **16** before the soft-budget boundary. No Sprite Fusion or OpenAI generation.

Stage 05 is DONE, pending user approval. **STOP before `PROMPTS/06_GENERATE_PILLARS_SUPPORT_STONE.txt`.**
