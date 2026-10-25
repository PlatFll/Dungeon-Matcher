# Focused revision 01 — background palette and integrated board sides

Candidate for user review. No merge, gameplay integration or Stage09 work.

## Scope

The narrow columns and separately placed Stage06/08 plants were visually detached. The revision uses wider fluted stone sections, recessed ledges, kelp wrapping across the column faces, coral growing from dark sockets, and small shell/barnacle contacts. Fourteen separate native modules preserve the current gutters and frame dimensions. The left and right sides vary their colony locations and kelp direction without changing the board.

Thirty-seven additive background exports use exact role-based palette maps: clearer cyan/aqua distant light, deeper blue water shadows, and more saturated teal foreground ruins. The background silhouettes, alpha, dimensions, placement and pixel clusters are unchanged. The player recess and existing lower reefs are recolored, not redesigned. Board/portrait frame artwork, wave/Guide/Settings controls, actors, cells, shared bottom HUD, text and all object transforms are preserved.

## Executed validation

- LibreSprite authoring and all 51 ASE-to-PNG roundtrips passed; binary alpha, exact native import colors, Point/FullRect/64PPU/uncompressed/unmipped settings.
- Unity 6000.3.19f1 produced 30 static captures across 360×640, 360×800, 720×1280, 720×1600, 1080×1920 and 1080×2400.
- All six before captures equal Stage08. Isolated protected-region renders match before/after exactly. 3,148,840 opaque UI/actor/frame/tile samples remain identical in the final scenes.
- Side modules map at exact integer 1×/2×/3×. No source-pixel occlusion, frame overlap, crop, board encroachment or missing edge pixels.
- 37 exact palette-only recolors preserve masks and geometry. 7839 protected production files, 881 prior-stage files and five authorities remain byte-identical.
- Tools/Validate-Unity.ps1 completed successfully. Full validator log: QA/Revision01/UnityValidation.log.

These are actual art-only Unity camera captures, not gameplay, Android or physical-device tests. The existing tall-preview spacer regions are retained and recolored; changing their dimensions is outside this request.

## Source and cost

No new PixelLab calls or credits. Existing Stage06 PixelLab coral silhouette reused; structural modules and exact recolors authored in LibreSprite. Prior cumulative spending remains 89 generations. Original approved assets and prior scenes remain available, unchanged.

Scene: Assets/_Game/Scenes/ArtPreview_DrownedCourtV2_Revision01.unity. Gallery: Preview/Revision01/Review.html. All final artwork remains pending user approval.
