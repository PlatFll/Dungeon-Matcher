# STAGE 00 — Preflight, source audit, and zero-spend pixel pipeline

**Execute this stage ONLY.** Read `START_HERE.md`, `GLOBAL_LOCKS_AND_ART_DIRECTION.md`, `REFERENCE_MAP_AND_MATERIALS.md`, `PIXELLAB_TOOL_SELECTION.md`, `QA_AND_APPROVAL.md` and the current repository authorities first. Reference images are examples for art direction only, NOT native sprites. Work in the dedicated isolated worktree.

## Deliverables
- Confirm branch/worktree, git status and current Drowned Court scene integration points. Read `Docs/DROWNED_COURT.md` (this already has underwater animations/physics/roster; no gameplay work allowed), current forest/dungeon production binding notes, golden-reference register, `AGENTS.md`.
- Check PixelLab connection and available v2/MCP tool schemas, actual supported reference input fields, sizes and generation/credit balance. **Zero paid calls in this stage**.
- Make protected asset/GUID hashes for shared bottom/top UI, board/gems, characters and zone prefabs; identify existing Court art that is worth keeping. Do not treat unused historical candidate art as approved reference.
- Locate active 8x8 board layout, combat lane baseline, portrait interior mask, world scaling and safe-area rendering. Note whether Court currently uses an old native kit or new presentation route, and avoid creating a second parallel runtime owner.
- Save screenshot comparison: current Court, Dungeon and Forest, at same logical Game view. Build empty scene `ArtPreview_DrownedCourtV2` with shared HUD shells and blank layout placeholders only, WITHOUT screenshot backdrops or actors/gems in the backing assets.
- Prepare a per-family tool decision table (PixelLab generation / exact edit / reuse / LibreSprite) and rough credit estimate; no assumption of unlimited plan usage.

## Success
A verified plan and blank pixel-perfect preview scaffold, no changes to production art/gameplay/other zones, no PixelLab generation charge. Stop.

## Required stage handoff
Create/update candidate asset files, manifest entries, PixelLab usage ledger, a native 1× contact sheet, nearest-neighbor zoom inspection, reference comparison and actual Unity art-only scene screenshot when applicable. Distinguish objective PASS/FAIL from artistic review. Describe exact preserved files and any unresolved defects. Do NOT merge or proceed to the next stage without explicit user approval.
