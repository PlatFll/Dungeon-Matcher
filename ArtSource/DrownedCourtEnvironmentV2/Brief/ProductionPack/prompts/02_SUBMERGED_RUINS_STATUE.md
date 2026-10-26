# STAGE 02 — Sunken columns, giant trident statue, and broken arch

**Execute this stage ONLY.** Read `START_HERE.md`, `GLOBAL_LOCKS_AND_ART_DIRECTION.md`, `REFERENCE_MAP_AND_MATERIALS.md`, `PIXELLAB_TOOL_SELECTION.md`, `QA_AND_APPROVAL.md` and the current repository authorities first. Reference images are examples for art direction only, NOT native sprites. Work in the dedicated isolated worktree.

**Use** `reference_crops/03_Central_Court_Ruins_Reference.png`. This family gives Court its unique identity and should not disappear behind too much coral.

## Generate independent hero architecture
- A large **distant** crowned drowned monarch/sea-king STONE STATUE holding a trident. It is a building-scale silent carved statue, not an enemy character, not vivid sea-person skin, and not a playable sprite.
- Sturdy teal-stone whole pillar kit: capital, repeatable shaft, worn shaft, foot/base.
- Broken/leaning pillar variant, partially collapsed ledge and old court archway; rubble joint pieces for connecting to the wall.
- Optional small trident relief or cutout ornament to be used infrequently, but not shared control icons.

### PixelLab description A — trident monarch statue
> Monumental ancient sea-king statue carved entirely from weathered teal-gray stone, wearing a simple ancient crown, gripping one long trident, upright solemn silhouette visible in the distance through deep water. 2D frontal side-scroller game background prop, full silhouette, thick readable carving volumes, three or four distinct stone planes with sparse worn chisel edges. No flesh, no live eyes, no character costume, no gem board, no text, no UI, transparent or isolated background. Should look like an old submerged royal memorial viewed from far behind the enemies, not a foreground hero.

### PixelLab description B — repeatable submerged column
> Ancient underwater royal court stone pillar components in blue-green slate masonry: broad capital with stepped ring, straight repeating shaft, worn shaft variation and stable square base. Large connected stone blocks with top-left facet light and dark grout, a few cracks, tiny algae grown from 1–2 joints only. Front-facing 2D side-view, modular pieces that join exactly; restrained pixel art, no foliage wall, no glows, no background, no text.

### PixelLab description C — ancient arch
> Broken underwater court arch of cool teal stone voussoirs, round top and thick side supports, ancient carved masonry with blue shadow recesses, a few barnacles in real crevices, single independent game environment object seen straight-on from side-view. Readable broad blocks, not a full scene, no characters or writing.

## Critical placement
Statue should be **distant and lower-contrast** (behind enemies), with trident discernible; no bright outlines competing with enemy faces. Columns side-frame the open stage. Connect architectural pieces with credible mortar joints and base support; don't stack random floating props.

## Validation
Show statue on plain background and in layer composition at gameplay scale; no glyph/health UI overlap; verify native pixel scale/anchors. Stop.

## Required stage handoff
Create/update candidate asset files, manifest entries, PixelLab usage ledger, a native 1× contact sheet, nearest-neighbor zoom inspection, reference comparison and actual Unity art-only scene screenshot when applicable. Distinguish objective PASS/FAIL from artistic review. Describe exact preserved files and any unresolved defects. Do NOT merge or proceed to the next stage without explicit user approval.
