# STAGE 04 — Underwater player portrait frame and inset recess

**Execute this stage ONLY.** Read `START_HERE.md`, `GLOBAL_LOCKS_AND_ART_DIRECTION.md`, `REFERENCE_MAP_AND_MATERIALS.md`, `PIXELLAB_TOOL_SELECTION.md`, `QA_AND_APPROVAL.md` and the current repository authorities first. Reference images are examples for art direction only, NOT native sprites. Work in the dedicated isolated worktree.

**Use** `reference_crops/02_Left_Portrait_Reference.png` and current approved player frame placement/UI behavior.

## Generate forest-independent Court materials
- Ancient teal-grey masonry player frame corners and sides, with shared silver/stone hardware where appropriate.
- Darker, quieter water-worn interior wall/recess tile set, clipped INSIDE portrait frame only.
- Selective small coral stems, algae vines and clusters of barnacles that physically attach to the *outer* frame joints, not overlay the portrait.
- Frame silhouette modular: 4 corners, repeatable horizontal/vertical rails, optional brace/sockets. Never stretch corners.

### PixelLab description A — court frame cornerstone
> Submerged ancient royal palace player-portrait UI frame **corner piece** in chunky blue-green carved stone and dull metal, recognizable L-shaped 2D UI framing, 1–2 small barnacle clusters embedded into exterior cracks, tiny algae sprig at outer edge, clean dark interior-facing boundary, readable native pixels. Intended as a four-corner modular UI sprite, no text, no heart, no character, no full panel.

### PixelLab description B — portrait recess material
> Quiet dark marine-blue block masonry inset for a Dungeon Matcher character portrait. Long broad shadowed stone slabs and sparse joints, low contrast to keep tiny skull/cape/HP readable. Flat seamless 2D pixel-art fill material, no props or glow, no gradient blur. Designed to tile strictly inside the frame's clipped interior.

## Critical
Actor, affinity gem and HP/shield controls are immutable. Any sea-green backing must not reduce Rattlebones contrast. Do not repaint bottom controls.

## Validation
Test Rattlebones in panel at actual render scale, portrait mask overflow with bright magenta probe if possible, no leaf/coral over face, no frame drift. Stop.

## Required stage handoff
Create/update candidate asset files, manifest entries, PixelLab usage ledger, a native 1× contact sheet, nearest-neighbor zoom inspection, reference comparison and actual Unity art-only scene screenshot when applicable. Distinguish objective PASS/FAIL from artistic review. Describe exact preserved files and any unresolved defects. Do NOT merge or proceed to the next stage without explicit user approval.
