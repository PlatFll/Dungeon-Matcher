# Remaining cast art validation — 2026-09-22

Scope: seventeen supplied static sprites recolored, then seventeen idle animations
authored in LibreSprite. RattleBones_FluidIdle is preserved as the motion reference.
Sources and machine-readable evidence live in `ArtSource/RemainingCast/`.

## Executed checks

- `review_recolors.py`: all 17 recolored stills preserve the original 64×64 alpha
  masks, bounds and placement. Only existing guide RGB colors are used.
- `inspect_idles.py`: all 153 native frames have binary alpha, the expected palette,
  fixed dimensions and timing, preserved original captures, and exact ready-pose
  return. No frame introduces an additional disconnected cluster of three or more
  pixels. This threshold is a diagnostic, not a substitute for visual inspection.
- `validate_exports.py`: all 17 native stills equal their visible PNG pixels; all
  153 animation frames equal both their sheet regions and decoded GIF pixels.
  Every exposure is 130 ms, every loop is 1170 ms, and all PNG sheets are 576×64.
  Sheet JSON uses portable image filenames. Every frame uses only that character's
  recolored-still palette; no opaque color drift or partial alpha was found.
- Grounding: the selected sole regions on source row 63 remain pixel-identical in
  every frame. First and last frames equal the recolored ready pose exactly.
- Preservation: hashes of all native original captures remain unchanged. The user's
  original RattleBones_FluidIdle.ase also matches the captured pixels and timings.

## Visual review

Compared original/recolored stills and every idle contact sheet; checked palette
separation, face identity, head/body rhythm, exposed-eye blinks, rigid props and
planted feet. Compared playback with the unchanged Rattlebones reference in the
synchronized local preview at native size and enlarged nearest-pixel views.
Checked the low pose on light and dark backgrounds. Corrected split pole/shaft
outlines, the Marshal's scalp contour, the Spear Guard's shield contour, and flag
attachment before the final export. Helmets retain their original visor designs;
heavy King/Minotaur poses retain a smaller compression amplitude.

## Limits

This is a reviewable art-source pass. The user's final visual approval is pending.
No Unity asset assignment, serialized import, C# change, runtime test, Android
build, or device test was performed in this pass. Unity validation is not required
for these source-only art and documentation changes. Prior runtime validation
belongs to its own recorded milestone, not to these new animations.
