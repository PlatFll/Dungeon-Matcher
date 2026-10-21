# Dungeon Matcher — Astra + PixelLab Hybrid Zone Rebuild Pack

This pack tells Astra exactly how to rebuild the dungeon zone art workflow using **PixelLab for hero assets** and **LibreSprite + Unity for cleanup, tiling, assembly, and validation**.

## Core decision
Do **not** ask PixelLab to regenerate the entire zone in one shot.

Instead:
- Keep the **clean structural logic** from the simpler side-view zone approach.
- Use **PixelLab** only for **hero assets / focal pieces / props / premium UI pieces**.
- Use **LibreSprite** to clean, normalize, tile, repalette, and repair output.
- Use **Unity** to assemble and test the scene.

## Main goal
Rebuild the dungeon zone so it:
- matches Dungeon Matcher's character/gem readability better
- feels clean and game-native
- keeps a 2D side-view/top-HUD readability
- avoids blur and over-detailed large baked backgrounds
- uses modular reusable assets

## What this pack contains
- A master execution prompt for Astra
- Branch and production rules
- Exact asset production order
- Stage-by-stage PixelLab prompt templates
- Validation checklist
- Acceptance criteria
- Progress ledger template

## Recommended branch
Use a fresh branch:
`feature/dungeon-zone-hybrid-pixellab-rebuild`

## Integration rule
Do not merge automatically.
Stop after each major milestone for review.
