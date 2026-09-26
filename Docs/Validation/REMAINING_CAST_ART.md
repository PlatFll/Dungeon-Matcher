# Remaining cast correction validation — 2026-09-22

**Subsequent user decision:** this stronger motion pass was rejected as worse.
The evidence below describes checks that ran; it does not establish visual
acceptance. The immediately preceding four guard sources were selected for Unity.
See `RESTORED_GUARD_IDLES.md` for that separate integration.

Scope: seventeen supplied static sprites recolored, then seventeen idle animations
authored in LibreSprite. RattleBones_FluidIdle is preserved as the motion reference.
Sources and machine-readable evidence live in `ArtSource/RemainingCast/`.

The first pass's palette/export checks passed but missed internal anatomy cuts,
incomplete prop selections and weak articulation. Those earlier checks did not
establish visual acceptance. This record describes the corrected files; previous
native outputs are preserved in `BeforeCorrection/` for direct comparison.

## Executed checks

- `review_recolors.py`: fifteen stills preserve their original alpha masks. The
  requested Royal Lancer/Arbalist helmet enlargements are explicit exceptions:
  changes stay inside their head regions, retain the exact previous palette and
  leave all pixels outside those regions unchanged. All canvases remain 64×64.
- `inspect_idles.py`: all 153 native frames have binary alpha, the expected palette,
  fixed dimensions and timing, preserved original captures, and exact ready-pose
  return. No frame introduces an additional disconnected cluster of three or more
  pixels. This threshold is a diagnostic, not a substitute for visual inspection.
- `validate_exports.py`: all 17 native stills equal their visible PNG pixels; all
  153 animation frames equal both their sheet regions and decoded GIF pixels.
  Every exposure is 130 ms, every loop is 1170 ms, and all PNG sheets are 576×64.
  Sheet JSON uses portable image filenames. Every frame uses only that character's
  recolored-still palette; no opaque color drift or partial alpha was found.
- `validate_geometry.py`, also called by the export validator: each exposed prop
  region matches its native part drawing under a single translation, with no
  rescaling or lost pixels. This includes both lower crossbow loops. The regions
  exclude upper surfaces legitimately hidden by the dipping head; their exact
  definitions and per-character pixel counts are recorded in the script/report.
  Every pixel of the complete rigid part stays inside the canvas at that offset.
- Grounding: visible sole regions on row 63 remain pixel-identical. All remaining
  body-sole pixels are also checked against exact prop compositing where a bow or
  carried shield passes in front of a foot. This replaces the earlier assumption
  that every bottom-row pixel near a foot was itself a foot. First and last frames
  equal the corrected recolored ready pose exactly; each loop has eight distinct
  drawings and a deliberate duplicate ready exposure across the seam.
- Preservation: hashes of all native original captures remain unchanged. The user's
  original RattleBones_FluidIdle.ase also matches the captured pixels and timings.

## Visual review

Compared all nine poses of all seventeen corrected idles against Rattlebones on
the correction boards, and reviewed synchronized playback in faction groups with
native-size and enlarged views on light/dark backgrounds. Inspected separated
native body/head/prop drawings and the King's arm/cape, both enlarged helmets,
Arbalist lower crossbow, Marshal wrist joins and complete helmet/hood outlines.
The torso now compresses continuously, with shoulder roll and a restrained neck
pivot; visible eyes close on the low beat. The King/Minotaur retain heavier acting.
The corrected passes share the reference's compact lift, compressed blink and
recovery. This is an art-direction judgment for review, not an automated guarantee
or the user's final approval. `Review/Correction/AllFrames_*.png` and the preview's
Pass selector make the full comparison available.

## Limits

This is a reviewable art-source pass. The user's final visual approval is pending.
No Unity asset assignment, serialized import, C# change, runtime test, Android
build, or device test was performed in this pass. Unity validation is not required
for these source-only art and documentation changes. Prior runtime validation
belongs to its own recorded milestone, not to these new animations.
