SNOWY MOUNTAIN — ORIGINAL SIDES, PLACEMENT CORRECTION
2026-10-31. Review only; no merge.

The preceding EdgeRevision was rejected for a doubled, overlapping appearance. Its added rock backing is removed completely. The approved pre-overlay assembly is restored from 727ea800. Existing board-side structures and their narrow lower connectors move horizontally only, with five native canvas pixels beyond each physical screen edge. The silhouette determines the actual visible clipping. Original large corner formations and snow aprons retain their placement.

No new renderers, duplicated backing, PNG edits, scaling or recoloring. The same approved modular structure remains on each side. All PNG/ASE sources and the approved upper scene, board, gems, frames, enemies and bottom controls are unchanged. PixelLab usage: zero.

Tools/Validate-Unity.ps1 passed. SnowyMountainSidePeekReview passed at 720x1280, 1080x1920, 1080x2400, inset-safe 1080x2400 and 360x640. Assertions cover original renderer count and sprite references, horizontal-only translation, no backing layer, native scale, binary alpha, no gem overlap, unchanged UI/geometry and no scenery input targets. Only the shifted side structures may clip horizontally within the five-native-pixel allowance. Default original-stage validation remains strict. Corner bounds and vertical clearance remain strict.

Paired screenshots are actual Game-scene Unity editor captures. The complete upper region and center board/lower HUD are pixel-identical before/after. Existing compact-view shared HUD crowding and editor pixel-camera warning remain. This is not a phone test or a production zone replacement.

Reproduce: Tools/Test-ForestFoundation.ps1 -Filter ForestFoundationPlayTests.SnowyMountainSidePeekReview -Graphics, after Tools/Validate-Unity.ps1. Scripts/Validate.py checks saved evidence and protected sources.

Draft PR #218. Stop for review. No Stage 5, production binding or merge.
