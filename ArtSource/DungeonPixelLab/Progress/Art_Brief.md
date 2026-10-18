# Candidate material and native-size brief

This is a working brief, not an amendment to the canonical art guide or golden register. Actual images and byte-level source audit: `Reference_Audit.json`. All three reference images and their production-sheet extractions match the current register. Rattlebones64x64 uses15 opaque colors; Farmer112x88 uses14; Pan112x88 uses16. Expanded transparent canvases do not change their one-source-pixel drawing pitch. No new member is added to the reference family.

Rattlebones supplies economical connected planes, clear contour turns and restrained top-left lighting. Farmer supports practical wood/cloth; Pan supports cool dull iron. No bone, cyan eyes, anatomy or royal costume is copied into architecture. Inspect native1x and nearest-neighbor4x PNGs beside the real Game View.

Flat orthographic FRONT/SIDE-ON dungeon: tall quiet violet masonry, one horizontal standing platform with narrow top edge and deeper front fascia. No receding perspective road. Portrait backing is a separate dark recessed material; board cell interiors stay contained. Extend the same outer wall around board/lower HUD. Draw only complete independent chains/banners/torches/props with padding; preserve future overlay ordering.

`Material_Candidates.json` and the swatch PNG contain six four-tone working ramps, ordered dark to light. Wall/lip/recess/jewels borrow existing guide UI colors; cool silver is sampled from the approved production PlayerFrame. Torch colors are a warm working proposal, not a change to any locked asset. Choose2–4 useful tones per material, not every swatch everywhere. These are NOT a global24-color quota. Existing gem colors remain locked.

Source discrepancy to retain: the guide's historical Wave counter table lists6 colors, but the current WaveCounterFinal PNG has9. The actual PNG was inspected and hashed; do not silently recolor it or rewrite the historical guide to hide the difference. The currently approved ReferenceDungeon PlayerFrame has74 opaque colors; that source's identity/material language is authority, not a requirement for future74-color corners. No existing UI pixels are changed.

Native size relationships to validate per family (not final designs):

| Asset family | Starting native canvas/assembly | Constraint |
|---|---|---|
| Wall repeats | 32x32 tile topology;64x64 module if using the64px Flash style reference | Keep one source-pixel mortar/edge scale; test 3x3 seams, do not enlarge32px art to fake64px detail. |
| Platform top/fascia |16 or32px sidescroller cells; assembled64-wide chunks as useful | One horizontal surface; sparse large slabs, no perspective. |
| Portrait material |32/64 repeating texture, separate masked recess | Texture scale stays native; panel stretches by tiling, not stretching pixels. |
| Gate/pillars |64x96 or96x128; pillar segments32x64 | Canvas follows required silhouette and clearance. |
| Chains/banner/torch/barrel |16x32,32x64,32x48 or fitting64px generation canvas | Trim/pad only with recorded anchors; keep full links/tails/flames. |
| UI |8–16px corners,16/32px rail repeats; control-native footprints | Assemble rails/9-slices without stretching corners; dynamic text remains live. |

The empty scaffold reserves360x640 logical pixels. Diagnostic1x and3x have exact pixel pitch. Future pieces must use integer coordinates and integer magnification. Test the actual production path separately because its existing flexible geometry may stretch native artwork. Stage00 diagnoses that path; it does not change layout or production rendering code.
