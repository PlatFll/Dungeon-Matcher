# Focused revision - mine floor and lower crystals

User-authorized changes to the Stage07 art-only assembly: replace the upper gray brick platform with a flat native PixelLab mine-floor tileset; hide three upper lanterns, wheel and cart; hide the two loose board-side amber deposits; add two large distinct grounded crystal formations beside the bottom HUD. This explicitly revises the prior top-area lock only for these items.

The cave background, timber, portrait, board frame, actors, gems, cells and shared controls retain their source artwork, placement and geometry. Original scenes and assets are preserved. The new scene is excluded from builds and does not integrate into production Game.

Six native assets retain editable LibreSprite files, exports, exact reopened roundtrips and raw generation alternatives. The floor has separate 32x8 walking-surface and 32x19 bedrock variants. The two crystal assets are 56x64, with transparent clearance. Native placement uses uniform 2x scale in the existing 720-unit reference canvas; no image resampling. Surface begins behind the unchanged enemy feet and retains the previous platform's lower edge.

Unity checks compare before/after at 360x640, 720x1280 and 1080x1920, enforce the restricted change regions, preserve original Graphic geometry/references, and compare protected opaque UI/actors/grid pixels exactly. Source hashes, alpha, tile seams, imported pixels and LibreSprite roundtrips are checked independently. See QA/Unity.json, Assets.json, SceneDelta.json and Validation.json. These are static art-only checks, not live gameplay/device validation.

PixelLab: 26 generations (1 rejected standard floor + 15 selected Pro floor + 10 crystal source). No reference uploads. Review Review.html and Preview/Comparison.png. Stop for visual approval; do not merge.
