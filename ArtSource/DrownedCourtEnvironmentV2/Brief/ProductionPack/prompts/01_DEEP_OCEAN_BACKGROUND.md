# STAGE 01 — Deep-water atmosphere and distant court silhouettes

**Execute this stage ONLY.** Read `START_HERE.md`, `GLOBAL_LOCKS_AND_ART_DIRECTION.md`, `REFERENCE_MAP_AND_MATERIALS.md`, `PIXELLAB_TOOL_SELECTION.md`, `QA_AND_APPROVAL.md` and the current repository authorities first. Reference images are examples for art direction only, NOT native sprites. Work in the dedicated isolated worktree.

**Use** `references/01_Drowned_Court_Visual_Target.png` and `reference_crops/01_Top_Battle_Backdrop_Reference.png`, `03_Central_Court_Ruins_Reference.png`. Compare with current Dungeon/Forest scenic detail density.

## Generate separate layers
1. **Deep ocean base**: quiet dark indigo-blue underwater gradient *represented through controlled native-color bands/large pixel clusters*, not a blurred filter.
2. **Mid-depth temple silhouette**: overlapping dim columns, submerged structures, distant arch/rubble, subtle algae edges.
3. **Upper light shafts and particulate**: few distinct cyan beams descending from upper center; wide simple shapes, no full-screen soft haze painted over characters/board.
4. **Optional subdued fish silhouettes**: tiny far-depth shapes, very low contrast; no schools crossing combat text.

### PixelLab description A — deep ocean backdrop
> Underwater royal-court dungeon far background for a portrait pixel-art fantasy match-3 battle. View straight-on as a 2D side-scroller environment. Vast deep teal and navy water, three distinct layers of distant ruined pillars and subaquatic silhouettes disappearing into blue haze, gentle shafts of pale cyan light from upper center, small quiet negative space behind the future combatants. Native crispy pixels, large connected shading groups, restrained detail and clear value hierarchy matching Dungeon Matcher characters. No coral wallpaper, no splash-art paint texture, no gem grid, no characters, no UI, no writing, no perspective grid.

### PixelLab description B — distant ruined silhouette layer
> Standalone backdrop layer of distant ancient submerged stone columns and broken sea-temple masonry, mostly dark blue silhouettes, a few soft cyan rim highlights, simple readable forms, long atmospheric depth, front-facing 2D side-view, no foreground props or characters. Modular horizontal background layer suitable for repeating or extending on tall phones.

## Construction notes
Generate either isolated depth layers or modular pieces, not one 941×1672 wallpaper. If light shafts are easier as native authored transparent overlays, build in LibreSprite and label source as direct edit, not generated. Avoid baked fish/shimmers inside the main background if animation is planned. Preserve a calm combat band.

## Validation
Compare distinct depths at native size; test normal/tall views and actor silhouettes overlaid for READ ONLY without touching actor art; no full-screen blue color filter covering gems. Stop.

## Required stage handoff
Create/update candidate asset files, manifest entries, PixelLab usage ledger, a native 1× contact sheet, nearest-neighbor zoom inspection, reference comparison and actual Unity art-only scene screenshot when applicable. Distinguish objective PASS/FAIL from artistic review. Describe exact preserved files and any unresolved defects. Do NOT merge or proceed to the next stage without explicit user approval.
