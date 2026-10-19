# Stage 09 — DONE; stopped for user review

2026-10-19. Barrels, skull and loose bones only, following the user's Stage 08 approval and explicit authorization to increase the PixelLab budget for this stage. No Stage 10, production integration or merge.

## Delivered

| Module | Native canvas | Opaque colors | Placement | Occupied world bounds |
|---|---|---|---|---|
| Clutter_Barrel | 36×42 | 8 | (322,140) | [324,142,356,180] |
| Clutter_Skull | 24×22 | 5 | (110,160) | [112,162,132,180] |
| Clutter_Bones | 26×12 | 3 | (275,170) | [277,174,299,180] |

Three separate reusable assets, 12 meaningful colors across wood, dull iron and matte bone. Every occupied bottom meets floor contact **y180**; each canvas has transparent clearance. One barrel is enough for this preview; extra stacking would crowd the standing area. Both loose fragments remain individually readable within one small pair module. All source pixels remain native, with no scaling or resampling.

## PixelLab lineage and LibreSprite work

The current guide/register and actual Rattlebones, Farmer, Pan and relevant UI references were refreshed and verified against current source hashes. Both object requests received the actual native Rattlebones PNG through `style_image.base64`, copying outline/detail/shading but not palette or anatomy. Approved Stage 01–08 materials were inspected locally.

- Barrel: `create_object_pro_flash`, job **fb8fa0b1-967e-4d0e-aae8-0e40e3b891c6**, object **e8fc15d3-f3d7-4a82-88d8-c02824ea73bb**, seed **910901**, 64×64, one direction.
- Skull/bones: same endpoint, job **b170d4ee-f71b-4657-bc35-06a0eb50f4b5**, object **e13208f9-f411-4101-a823-c814d7ae2cbb**, seed **910902**, 64×64, one direction.

Each was quoted at **5 generation equivalents**. Both completed; authenticated balances **1019.20 → 1014.20 → 1009.20** confirm 5 each, 10 total. `get_object` does not expose itemized job billing, so the receipt correctly uses balance deltas. No paid retry, rotation or animation.

Immutable raw PNGs and complete requests are in `Sources/Stage09/`. Actual LibreSprite direct cel editing produced editable `.aseprite` files and independent PNG exports. No MCP bridge is claimed.

The barrel retains its entire contour and every wood pixel; only the iron was adjusted from a broad bright stripe into a cooler middle plane, right shadow and short top-left highlights. The skull retains its contour and dark sockets; a handful of cheek micro-shades were grouped into two simple matte planes. The loose fragments initially read like feet at native scene size. Their contours were repaired in LibreSprite into narrow shafts with clearer knuckled ends; this is explicitly a native shape correction, not a claim that their raw silhouettes were preserved. No new shadows, dirt, texture specks or glow were added.

## Validation actually performed

- Three native cel/PNG/candidate-import comparisons are RGBA-identical. Binary **0/255 alpha**, complete padded bounds, Point/FullRect/64 PPU, no mipmaps/compression/NPOT resizing.
- Barrel/skull source-alpha masks preserved after native crop/padding translation; barrel wood pixels unchanged. One connected barrel, one skull, and exactly two substantial loose-bone components, without isolated noise.
- All three props contact **y180**, with no opaque overlap in reserved UI areas or the established **20px diagnostic standing lanes** at x154,234,314. No overlapping ground-shadow layers were introduced.
- All **205 protected art files** and **215 previous placements** unchanged. Three new placements produce **218 total**. Pixels above y140 and from y180 downward are identical to Stage 08.
- Explicit Unity test `DungeonPixelPipelineStage09Tests.Stage09ClutterNativePreview`: **1 passed, 0 failed**, on the final corrected exports. Eight actual captures, normal/unlit/contact/restored at **360×640** and **1080×1920**, equal independent native composition with **zero RGBA differences**.
- `Tools/Validate-Unity.ps1`: **SUCCESS**, exit 0, Unity **6000.3.19f1**. Log: `C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-4c4c03b9-436d-4d90-9460-07f833b54be5.log`. Receipts retain tested hashes and trailing-whitespace-only normalization details.
- Full semantic scene review: **12 new serialized objects**, zero removed; existing changes only preview module/manifest bindings and the props child list. Production Game scene/build settings unchanged; all starting main-checkout user changes preserved.

## Artistic verdict and limits

Ready for user review. The wood is warm but restrained, iron reads as dull metal, and the skull/bones remain quiet ground clutter. The complete contact edges sit on the platform without floating or clipping. Material values, contours and texture density fit the approved assembly at native and enlarged size.

This is the isolated art-preview scene, not the live battle scene or a phone build. The standing lanes are diagnostics, not certification against every character's full silhouette; actor/device testing remains part of later integration. User art approval is still pending.

Stage 09 cost **10**, cumulative **45**. The user authorized this stage's extension beyond the original 35-generation budget; the working cap is increased by the quoted 10 to **45**, scoped to Stage 09. This does not authorize paid Stage 10 work. **STOP before `PROMPTS/10_GENERATE_PLAYER_FRAME_KIT.txt`.**
