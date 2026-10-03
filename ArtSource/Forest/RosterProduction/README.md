# Expanded forest roster production

User authorization: seven approved enemies, their final kits, and two review
concepts in each of seven blocker families. 160 subscription generations total,
120 initial and 40 reserved. No purchases. See budget.json for confirmed usage
and pending costs; a pending generation is committed spend.

## References and native scale

Inputs are exact approved PNG pixels with transparent clearance added:
96×80 canvases for the six 64×64 designs, 128×112 for Treant's approved 96×96
Old Stump variant C. Source pixels are never resampled. The original stills and
their hashes remain in ../RosterConcepts/.
Selected/*/Ready.png places the same opaque reference pixels on the shared foot
row. This removes the original Treant PNG's empty bottom rows from its displayed
ground offset without changing its canvas dimensions or resampling its body.
grounding-checks.json records byte equality of the opaque bounding rectangles.

requests.json and special-requests.json contain the prompts. jobs.json and
concept-jobs.json record actual tool results, including uncharged capacity
errors. results.json records returned download URLs. Raw files retain PixelLab
outputs. UnusedDuplicates keeps the three accidental duplicate samples apart.
The original Bloomcaller basic and cast had blue streaks and are excluded;
AutoAttackCorrected and AbilityCorrected replace them. Treant's original death
detached its twig. DeathCorrected supplies the selected collapse poses; its
elongated-twig intermediate poses are omitted. Drummer's first cast failed under
server load; AbilityRetry records the replacement request. Bloomcaller's corrected
cast still had an isolated cyan orb and clipped staff pose; those two frames are
omitted. Native side/top clearance checks pass on every selected character frame.

Selected sprite sheets use binary alpha, the approved character's own colors,
and whole-pixel baseline alignment. Export records dimensions, visible colors,
alpha values, bounds, frame durations, contact events and file hashes. It does
not change silhouettes by rescaling. The contact sheets are review enlargements.

## Reproduction

- Tools/forest_roster_prepare.py: transparent input padding and base prompts.
- Tools/forest_roster_download.py: download completed jobs, retaining source bytes.
- Tools/forest_roster_review.py: raw frame contact sheets.
- Tools/forest_roster_export.py: selected native sheets, timings and motion gallery.
- Tools/forest_roster_gallery.py: blocker comparison page and per-PNG measurements.
- Tools/Import-ForestRoster.ps1: import definitions; -Animations imports sheets.
- Tools/forest_blocker_export.py: selected dense-vine growth/recoil and approval ledger.
- Tools/Import-ForestRoster.ps1 -Blockers: bind the seven user-selected obstacle designs.

The BlockerReview.html page shows all 14 exact 64×64 concepts at native and 3×
scale. Vines/chains additionally overlay a real gem. The user selected Wood A,
Stone B, Chain A, Thorn A, Roots B/B and Vines B on 2026-10-03. The selected stills
are imported unchanged; other concepts remain source-only.

MotionReview.html supports animation playback, pause and frame stepping beside
the approved ready pose. The manifest also drives Unity animation contact events.
Final acceptance requires inspecting the clips and the actual Unity presentation.

VineMotionReview.html shows the 405 ms growth and 315 ms recoil. The standalone
growth request barely moved, so growth reverses the useful recoil poses and ends
on the exact selected still. Both clips contain nine native frames. Actual final
account usage is 121 generations from the 160 ceiling; one failed cast was not
charged, and no jobs remain pending.
