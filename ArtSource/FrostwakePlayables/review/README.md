# Character still approval batch

Status: **direction liked; revisions requested**, 2026-10-11.
No animation has been commissioned from these candidates.

The user requested a kind, less angry Minotaur face and keys on his belt,
and a more distinctive Kobold design. Preserve this first batch as history;
the follow-up candidates and exact edit requests live in `../revision-02/`.
This feedback does not yet approve either still for animation.

Open `NativeComparison.png`: all sprites share a common floor at native 1×
and nearest-neighbour 3×. Transparent margins were cropped only for this review
layout. The individual candidate PNGs retain their original canvas dimensions.
The comparison does not substitute for the individual file checks recorded in
`technical-checks.json`.

## Recommended candidates

- Dungeon Attender: Minotaur Pro candidate 02. Native 80×80 canvas, drawn bounds
  62×65, 13 opaque colors, binary alpha. Broad shoulders, fists, curved horns,
  ochre attendant apron, brass buckle and warm brown fur. The comparison orcs
  occupy 56×57 and 55×56 native pixels. Retains a larger silhouette without
  resampling the drawing.
- Kobold Gunner: Kobold Pro candidate 01. Native 64×64 canvas, drawn bounds
  50×59, 15 opaque colors, binary alpha. Red reptilian snout/tail, eye wrap,
  listening hand, teal scavenger clothes and one small revolver. No cowboy hat.

Exact PixelLab cleanup reduced palettes without dithering, merged near-black
pixels into the canonical `#0A0D11` outline and consolidated similar brown
shades on the Minotaur. It did not resize, alter alpha, or redraw anatomy.

Other generated candidates are retained under `minotaur/` and `kobold/` as
unselected alternatives. They are not additional approved characters.

## Generation accounting

PixelLab confirmed the account before production: 223 subscription generations,
$0 credits. The two Pro calls reported 15 + 10 generations for 16 candidates
each. Two palette operations reported 0.1 generations each. Exact workbench
edits are free. Total reported tool charges: **25.2 generations**.
The account balance afterward reports **198 remaining** (integer display).
No credit purchase or animation charge has occurred.

Full generation inputs include actual reference-image data, with source hashes
in `../input-manifest.json`; request/receipt files record the job IDs. The
reference register remains unchanged: proposing these characters does not make
them golden references.
