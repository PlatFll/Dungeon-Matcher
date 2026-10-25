# STAGE 10 — Assemble native modular Drowned Court art in isolated Unity test scene

**Execute this stage ONLY.** Read `START_HERE.md`, `GLOBAL_LOCKS_AND_ART_DIRECTION.md`, `REFERENCE_MAP_AND_MATERIALS.md`, `PIXELLAB_TOOL_SELECTION.md`, `QA_AND_APPROVAL.md` and the current repository authorities first. Reference images are examples for art direction only, NOT native sprites. Work in the dedicated isolated worktree.

## Objective
Use approved assets from Stages 01–09 to assemble `Assets/_Game/Scenes/ArtPreview_DrownedCourtV2.unity` (adapt to actual project conventions), without replacing live Court presentation.

## Layer order
1. Dark remote depth and long water rays (quiet layers; no game tint).
2. Distant columns, monarch statue and sunken arch.
3. Front court masonry and pillars.
4. Flat battle dais/standing ledge (under actor stage).
5. Zone player frame shell + masked dark inset backing.
6. Zone board outer frame and stone gutters (actual empty protected board cells shown).
7. Attached coral/kelp/shell/barnacle/rock clusters outside interactive board/HUD.
8. Optional ambient bubble/light sprites in dedicated layers.
9. Shared Wave/Guide/settings/potion/resonance/bomb UI shells (referenced from current game unchanged).

## Scene test matrix
360x640, 360x800, 720x1280, 1080x1920, 1080x2400 and meaningful safe-area insets where practical. Pixel scale must be integer; don't assume Point import fixes fractional layout transformations. Verify HUD/gameplay bounding boxes unchanged relative to current zone standard and all ornaments stay in legal gutters. Allow preview with actual existing characters/gems bound for read-only comparison, but do not export those into environment art.

## Deliverables
Actual editor captures at 1x and higher integer scale, comparison to mockup and current Dungeon/Forest, stage asset map and layering screenshot, list of missing pieces or art problems. No production integration yet. Stop.

## Required stage handoff
Create/update candidate asset files, manifest entries, PixelLab usage ledger, a native 1× contact sheet, nearest-neighbor zoom inspection, reference comparison and actual Unity art-only scene screenshot when applicable. Distinguish objective PASS/FAIL from artistic review. Describe exact preserved files and any unresolved defects. Do NOT merge or proceed to the next stage without explicit user approval.
