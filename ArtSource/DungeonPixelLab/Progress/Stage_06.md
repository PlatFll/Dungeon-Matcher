# Stage 06 — DONE; stopped for user review

2026-10-19. Pillars, support brackets and continuous side masonry only. The user approved Stage 05 and requested this stage using PixelLab and LibreSprite, preserving approved art. Draft PR #203; no production integration, gameplay/UI changes, Stage 07 or merge.

## Delivered

Seven native LibreSprite sources and independent PNG exports:

| Module | Native size | Purpose |
|---|---|---|
| Support_Capital | 28×16 | Complete generated cap with two clear top rows |
| Support_Core | 28×28 | Two repeatable 14-pixel stone courses |
| Support_Foot | 28×24 | Intact stepped base and eight authored neck rows |
| Support_Corbel | 28×20 | Small connected bracket under the fascia |
| Support_Corner_Left / Right | 28×28 each | Staggered return masonry for continuous side gutters |
| Support_Edge_Foot | 28×8 | Finished bottom edge, preserving the gutter width |

Three 28×180 pillar assemblies at x0, x96 and x324 keep the established portrait/back-wall relationships and meet the y180 standing plane. Three brackets overlap the fascia at y210 and end at y230. Both side gutters run continuously from y212 to y640. No isolated masonry chunks beside the board; the future board/HUD overlays remain later-stage work. All 138 existing placements are retained; 56 structural placements are added.

## PixelLab source and native cleanup

Current canonical guide v1.17.2, golden register, actual Rattlebones/Farmer/Pan and applicable frame source hashes were verified against the main checkout. Actual native Rattlebones PNG bytes were sent in `style_image.base64`, with outline/detail/shading true and palette copying false. Supporting cast and approved environment art were inspected locally. Rattlebones supplies rendering discipline, not anatomy or palette.

One new `create_image_pro_flash` job, **92555034-f78c-4284-8577-3c2b62b63b35**, image **f0fc35e1-f6b0-5ab2-9899-cebd7b0210d8**, seed **610601**. Native **64×96** source was quoted at **5**, and completion confirms **5 charged**. Immutable source and the full reference payload/quote/completion receipt are retained. No retries or alternate provider.

LibreSprite directly authored all native cels and independently exported the PNGs. Generated capital/foot silhouettes remain at their original pixel pitch; the straight shaft becomes a repeatable two-course module. Broad faces are darkened to the approved masonry family, top-left highlights are placed deliberately, incidental shade chatter is removed, and complete source profiles become practical brackets and corner returns. The gutter variants extend the generated stone-course language with authored native faces, not screenshot crops. **No scaling, interpolation or blurred shading.** Background-layer flags are normalized using the existing helper without pixel changes.

## Validation actually performed

- Seven decoded native cels equal their independently exported PNGs byte for byte; imported candidate PNGs match those exports.
- **Six opaque colors combined**, four or five per piece; alpha is **0/255** (the solid edge-foot is entirely opaque). Source hashes, exact palettes, dimensions and occupied bounds are in `Diagnostics/Stage06/PixelChecks.json`.
- Capital/core/core/foot joining masks agree exactly. Every pillar has continuous native coverage from its top cap to its ground-contact row; each base contact is solid.
- Both gutter assemblies have **428 uninterrupted rows** and connected stone courses. All sprites fit their complete canvas rectangles. Tile connection edges intentionally touch adjacent modules; this is not sprite clipping.
- **139 protected art files** are byte-identical. All **138 prior placements** are unchanged. Production Game scene and build settings remain unchanged, and all eight starting main-checkout user edits are preserved.
- Explicit Unity test `DungeonPixelPipelineStage06Tests.Stage06SupportNativePreview`: **1 passed, 0 failed**. Verifies 194 pieces, integer bounds, 64 PPU, white tint, native dimensions, original floor/fascia/gate anchors, pillar floor contact and no gameplay/input owners.
- Actual Unity normal, temporary-footprint and restored screenshots at **360×640** and **1080×1920** match independent native composition with **zero RGBA differences**. Temporary markers are not saved in the scene.
- `Tools/Validate-Unity.ps1`: **SUCCESS**, exit 0, Unity **6000.3.19f1**. Exact log and tested hashes are in `Diagnostics/Stage06/ValidationReceipt.json`.
- Full semantic scene-diff review: 224 added serialized objects (56 support images and their components), no removals. Only existing module/manifest bindings and the structural layer's child list change; no previous transform or image changes.
- Point, FullRect, 64 PPU, no mipmaps/compression/NPOT resizing. Only inspected incidental whitespace changes are restored in the isolated worktree.

## Artistic verdict and stop

**Ready for user review.** Quiet violet supports now make the stage read as built architecture. Narrow shafts stay subordinate to the doorway and character area; small brackets connect to the platform rather than floating. The corner-return courses continue around the future board/lower-HUD area with the same restrained top-left light. No broken seam, clipped cap/base or noisy texture was found in native and enlarged review.

This is the isolated art-preview scene, not the production battle or a phone build. Torches and later dressing remain intentionally absent. Existing Stage 00 production scaling/layout concerns remain deferred to integration.

Balance **1035.20 → 1030.20**. Cumulative pack spend **24/35**, leaving **11** before the soft-budget boundary. Stage 06 is DONE, pending user approval. **STOP before `PROMPTS/07_GENERATE_TORCHES_AND_LIGHT.txt`.**
