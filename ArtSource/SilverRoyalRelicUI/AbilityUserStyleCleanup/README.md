# Ability button: user-cleaned silver family

Review candidate, 2026-11-11. No production import, gameplay change, or merge.

The user supplied `SettingsButtonSilver.png`, `PowerUpTileSilver.png`, and
`WavePlaqueSilver.png` as their cleaned examples and requested the same native
LibreSprite treatment for the ability button. `Inputs/` preserves those exact
files and the previous ability artwork. Earlier stages and the user's open
editing documents are untouched.

## What the user's cleanup establishes

- Straight, continuous silver rails with consistent bevel thickness.
- Broad connected highlight and shadow planes, with fewer tiny transition marks.
- Compact gold accents centered on the frame, without extra silver halos.
- Clean corner joins and quiet interiors; detail should communicate structure.
- Consistent material ramps across controls, without recoloring the user's art.

These are scoped UI lessons, not a universal palette or a change to character
reference authority. The user's three PNGs are the direct authority for this
revision. The canonical art guide and Rattlebones reference remain unchanged.

## Revision

The ability retains its 176x66 canvas and #1C2438 board-colored inset. The user's
wave bevel/corner/jewelry pixels establish the cleaned frame. Only two straight
side runs are extended to fit the ability's taller interior: no corners, jewels,
or emblem are resized. The wave's hanging skull is not part of this control;
the lower silver rail continues cleanly behind the original crowned ability
emblem. Faint purple curls and the old irregular ornament halos are removed.

The existing 30x40 crowned-skull source is unchanged. Its layer origin changes
from (73,18) to (73,16), centering its opaque 26x36 drawing inside the new recess.

`Editable/AbilityButtonSilver.ase` has separate frame and emblem layers.
`Editable/AbilityFrameSilver.ase` contains the reusable empty frame.
Native transparent exports are in `PNG/`. `Preview/BeforeAfter.png` shows native
and nearest-neighbor enlarged comparisons alongside the unchanged user examples.

## Verification

LibreSprite performs the pixel writes, native saves and exports. The layered
assembly is saved by LibreSprite, reopened and exported again; visible pixels
match exactly. `build.py` records the authoring procedure and `QA/Validation.json`
records source/output hashes and measured checks:

- 176x66 canvas, opaque bounds (3,1)-(172,65), no clipped drawing.
- 14 meaningful opaque colors, alpha values exclusively 0 and 255.
- One connected frame silhouette; no detached stray opaque pixels.
- Original crowned skull pixels and all three user reference files unchanged.
- No resampling, no new PixelLab generation or spending.

Art-only asset inspection; Unity compilation, in-game interaction and device
testing are not required or claimed for this review copy. User approval is
required before replacing the staged ability artwork or importing it.
