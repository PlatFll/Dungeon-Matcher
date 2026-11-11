# Silver Royal Relic — Stage 3 bottom HUD

User direction: continue from selected **A / Royal bevel** and the Stage 2 top
controls. This is a native-asset review checkpoint under the original brief,
not runtime deployment. **Stop for approval before Stage 4. Do not merge.**

Open [Review.html](Review.html) for native and integer-zoom views, original
PixelLab sheets, editable sources, charge controls, item quantities, and five
plain backdrop colors. These are asset assemblies, **not Unity screenshots**.
The previous stage's real Unity captures remain linked separately.

## Delivered pieces

| Export | Native size | Opaque colors | Purpose |
|---|---:|---:|---|
| AbilityFrame | 176 × 66 | 13 | Silver plaque with a quiet empty center |
| AbilityFlourish | 176 × 66 | 1 | Optional restrained purple curls, separate layer |
| RattlebonesEmblem | 30 × 40 | 12 | Separate crowned skull, Rattlebones only |
| ItemFrame | 64 × 64 | 11 | Same template for potion and bomb |
| EnergyCasing | 147 × 24 | 11 | Empty casing with dark trough |
| EnergyFill | 107 × 8 | 4 | Independent clipped purple fill |

All six have transparent native PNGs, editable LibreSprite `.ase` documents,
and exact visible-RGBA reopen/export checks. The frame and icon have transparent
clearance; the fill intentionally reaches its horizontal extent for clipping.
No production artwork was resampled. Integer enlargement in review images and
the unchanged 24 × 24 consumable icons displayed at 2× are inspection/display
operations only.

## Generation and cleanup

Three PixelLab Pro Flash generations used selected A's **actual hosted image**
as their style input. A's parent generation used the user-approved Rattlebones
PNG. Exact requests, seeds, job IDs, downloads and charges are recorded in
`GenerationRequests.json`, `GenerationResults.json` and `QA/ReferenceLineage.json`.
Raw sources remain intact in `Raw/`.

LibreSprite wrote and saved every final pixel through `QA/NativeProduction.js`,
prepared by `produce.py`, then reopened each editable source for a roundtrip.
Cleanup extended or shortened straight rail/recess runs at native resolution,
retained the source bevels and jewelry, repaired the generated ability rail's
bottom-center split, darkened the recess, removed scratch-like gray interior
marks and made restrained connected purple curls. The generated crowned skull
is extracted at its original native size. The purple fill uses four intentional
horizontal bands, with no interpolation or transparency gradients.

**Usage: 18 PixelLab generations** (6 each), balance **3685.90 → 3667.90**.
Earlier exploration spent 29; Stage 2 spent 0; staged total is now **47**.
No new local reference images were uploaded in this stage: the direct style
source was already hosted on the same approved provider.

## Integration handoff, not yet executed

- Preserve existing shared button rectangles and anchors: ability 176 × 66,
  supplies 64 × 64, energy casing 147 × 24.
- Fill native rectangle: top-left `(20, 7)`, extent `107 × 8`. Clip width from
  the current energy owner; never scale a complete half-filled texture.
- Ability icon center `(88, 38)`; the sample skull is character-specific. Do not
  bake it into every character's universal button or replace runtime ability
  content. Retain existing accepted activation, pressed/disabled and ready
  presentation behavior during Stage 4.
- Existing potion/bomb PNGs are copied unchanged for review. Existing runtime
  quantity text and icons remain separate. Review labels are demonstration
  lettering, not proposed replacements for the game's font or count behavior.
- The six files remain in `ArtSource`, not `Assets` or `Resources`. Point import,
  current PPU, touch targets, overlays and actual gameplay fit must be verified
  during the explicitly separate Stage 4 integration checkpoint.

## Verification

`verify.py` passed: six native dimensions and binary alpha checks, exact
LibreSprite roundtrips, all fill pixels inside the dark trough, unchanged
reference and existing item hashes, and **9,182 protected Assets, Packages and
ProjectSettings files byte-for-byte unchanged**. Stage 2 sources are unchanged.
See [QA/Validation.json](QA/Validation.json).

The review page was inspected in the in-app browser at native 1×. Charge 0/65/100,
quantities 0/3/99, emblem visibility, and dark/snow/teal backdrop contrast were
checked through the actual page controls. This is browser asset inspection,
not cross-zone Unity acceptance. See [QA/BrowserReview.json](QA/BrowserReview.json).

This stage changes no C#, Unity import metadata, scene or serialized routing.
Unity compilation, runtime interaction and device testing were **not rerun**
for this art-only stage. Stage 2's validation applies only to its unchanged
temporary top-control preview; it does not validate these bottom controls in
the actual game.

Art verdict: coherent with selected A, silver dominant, quiet dark inset,
restrained gold focus and readable separate content. Pending user visual approval.
