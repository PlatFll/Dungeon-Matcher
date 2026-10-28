# Revision04 - centered walking floor and separator alignment

The previous floor ended only a few native pixels in front of the enemies, leaving their feet at its edge. The current user request authorizes correcting that alignment. Dungeon, Forest and Court bind scenery around the common actor/health-bar floor; see QA/LayoutReferences.json. This revision applies that ownership to the Ironvein review.

The approved floor and separator placements move down 14 native pixels as complete sprites. Walking floor: y188.0..220.0, measured enemy floor: y204.0, separator: y220.0..239.0. The actor feet are centered, and the separator meets the floor below. No artwork, colors, scale, sprite reference or original active state changes. Actors, health bars, countdowns, weakness icons, board and all controls retain exact geometry.

Only fifteen existing RectTransforms move in the copied art-only scene. No objects added/removed; source scenes, approved art and production Game remain untouched. Zero generation spending. This stage performs layout only, so it reuses the prior native LibreSprite sources without editing them.

Actual static Unity captures at 360x640, 720x1280 and 1080x1920; independent source hashes and serialized comparison; required compiler result in QA/Validation.json. These are not live combat/device tests or production integration. Review.html contains the before/after and other-zone layout comparisons. Stop for approval; do not merge.
