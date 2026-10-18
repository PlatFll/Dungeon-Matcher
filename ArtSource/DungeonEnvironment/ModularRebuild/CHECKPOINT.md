# Dungeon modular rebuild checkpoint

Branch: `codex/dungeon-modular-scene-rebuild`, isolated worktree from main `b82bf80a`.
Scope: complete supplied rebuild brief; preview only, no production replacements or merge.

Completed: read current direction/register and actual golden images; verify hashes;
preserve immutable ZIP/branch rules; compare existing native sources; four 64x64
LibreSprite floor candidates, exact native exports and all 16 horizontal/vertical
edge pairings pass. First repeated layout rejected as too obvious; native offsets
add variant joint spacing while retaining source slab contours. 11 opaque colors.
Floor internal artistic assessment: 9/10 compatibility with the approved native
floor's connected violet planes and top-left bevels; this is subjective and still
requires the final Unity composition check. Not a user approval.

Important: the ZIP full reference is pixel-identical to the approved mockup, but
its crop boxes are inconsistent with that image. Current native source positions
govern; never import those contaminated crops as final assets.

Next: build/inspect wall, frame and prop modules; create independent
ArtPreview_DungeonModular scene, using shared layout/pixel helpers without live
combat owners. Verify seams/clipping/saves, actual Unity captures at portrait
sizes/safe areas, and preserve per-asset before/after evidence. Run repository
Unity validator before opening a review PR. Do not merge.
