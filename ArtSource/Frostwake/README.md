# Frostwake source and review

The eleven still identities were explicitly approved by the user for animation.
The golden reference images and source guide v1.13.2 were actual PixelLab inputs;
`reference-lineage.json`, `still-jobs.json` and the approval/technical manifests
preserve source provenance. Guide v1.14 subsequently records the approval.

Open `MotionReview.html` for native key poses and timed 3x GIF previews.
`Review.html` preserves the still-approval comparison. PNGs establish actual
native dimensions; a gallery screenshot is not a measurement of individual assets.

## Selected exports

- `approved/`: approved identity cels padded to the fixed 128x104 motion canvas.
- `production/animation-manifest.json`: 70 clips / 501 exposures, authored
  contact timing and return poses. Imported PNGs match these exports exactly.
- `motion/`, `motion-queue.json`, `motion-palette-queue.json`: 416 generated
  cels and exact lineage for each approved 15-color palette.
- `cleanup/`: free PixelLab exact edits of unwanted hit spikes, Champion
  release colors and the three ice shells' transparent windows.
- `environment/FullMountainPass.png`, `FullWanderingHallV2.png`,
  `FullGlacierGateV2.png`: final 512x384 scenery sources. Earlier dimensions
  remain source history. The first full Hall tiled incorrectly; the first full
  Gate contained annotation text. Neither is bound in Unity.
- `theme/`: native frame cells and quiet material corrections. The frame and
  wave copy operations, material maps, guarded alpha edits and receipts are
  preserved as `*-decisions.json` / `*-receipt.json`. Request JSON with embedded
  image payloads is reconstructible and ignored by Git.
- `audio/`: original temporary cue, awaiting listening feedback.

`Tools/Export-FrostwakeMotion.py`, `Prepare-FrostwakeTheme.py`, the scoped Unity
importers and `Check-FrostwakeAssets.py` reproduce and check the selected exports.
Do not rerun unrelated historical zone importers: they predate expanded approved
enemy canvases. Original approved characters and golden memberships are protected.

## Actual PixelLab usage

147.5 subscription generations: 66 stills, 1 still palette batch, 56 motion,
22 environment/UI/effect attempts including revisions, 2.5 motion palette passes.
Free exact edits cost zero. Final balance reports 26 remaining / 1973 used;
the service rounds its balance display. No credits purchased. All jobs completed.
Still approval does not imply human approval of motion, scenery or temporary audio.
Validation and release limitations are tracked in `Docs/Frostwake/WORKLIST.md`.
