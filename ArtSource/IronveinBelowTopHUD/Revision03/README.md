# Revision03 - broad walking surface

Only the walking floor is revised. The user explicitly chose to keep the exact enemy floor line. The separator at native y206..225, actors, health bars, board and every other original placement remain unchanged. The walking plane extends backward from y186 to y174 and remains fixed at y206 in front. There are approximately three native pixels between the current feet and separator; this revision deliberately preserves that constraint.

Three 64x32 modular slabs join the approved Drowned Court rear/front floor courses, map their five material colors to cave slate, and normalize the outer repeat seams in LibreSprite. Actual input PNG hashes and palette edits are recorded in Lineage.json and Scripts/Cleanup.js. No fresh PixelLab generation and no resampling. ASE sources, exported PNGs and reopened roundtrips are included.

The new art-only Unity scene copies Revision02, disables only its twelve old walking tiles, and adds six tile placements. The last tile clips to the screen width without stretching. The original separator group stays active. Source scenes, source art and production Game remain untouched. Preview scene is excluded from builds.

Executed checks: QA/Unity.json, Assets.json, SceneDelta.json and PixelScope.json. Captures at 360x640, 720x1280 and 1080x1920 are actual static Unity camera renders, not live combat or device testing. Required validator result is recorded separately in QA/Validation.json. Stop for visual review; do not merge.
