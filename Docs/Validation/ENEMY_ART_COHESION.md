# Enemy art cohesion, west-facing motion and canvas repairs

2026-10-11. User-authorized implementation of the cast audit, retaining the
three-quarter camera and directing faces/actions screen-left.

## Result

64 enemy forms, including 62 current definitions and two disabled Grand Delver
alternates, now use 395 expanded native sheets and 2,951 retained sprite IDs.
Most cells add 24/20/24/4 pixels left/top/right/bottom. The reserve remount uses
288×144 cells to contain the entire incoming machine. No drawing was downscaled.
The disabled forms remain disabled. All 353 animation clip files are byte-identical:
keys, timing, impact/completion events and controller ownership are preserved.

Selected material planes are quieter across the cast. Localized changes restore
Snapvine's rooted wilt, the Mage's face, King's
readable sword/body under narrower trails, Shield Knight ornament and complete
weapon/finial/effect edges. Reversed Puffer/Lantern attack poses, Scout/Moray death
poses and Delver pilot/remount faces now face west. Backswings, recoil and falling
trajectories can extend right without turning the face or contact direction east.
Three-quarter and transitional near-front views retain the existing camera.

Rattlebones and all player art remain unchanged. Species, equipment, palettes
and identity landmarks remain authoritative. The golden register points to exact
current extractions; proposed additional reference memberships remain proposals.
There are no gameplay, balance, board, shield or combat-event changes.

## Puffer face restoration — user correction, 2026-10-11

The user rejected the cohesion pass's relocated eye and added mouth. The native
recipe now preserves the original expression across every affected state:
45 drawings in seven sheets, with 1,784 pixels restored exactly from the originals.
West-facing corrections, expanded canvases and detached-artifact repairs remain.
The live gallery and Highlights have been refreshed. Exact affected frames and
pixel comparisons are recorded in
[the restoration check](EnemyArtCohesion/puffer-face-restoration.json).

Native/export checks were rerun for all 395 sheets: exact RGBA, original exposures,
binary alpha and no outer-edge contacts. Every changed Puffer pixel equals its
immutable original. Production copies match exports; import metadata and all 353
clips are unchanged. No new generation spending. Unity was not rerun: this
correction changes only art pixels, the native recipe and documentation.

## Executed verification — initial cohesion pass, before face restoration

| Check | Executed result |
|---|---|
| Native cels vs PNG sheets | 395 files / 2,951 cells: exact RGBA equality, original exposures, binary alpha, no outer-edge contacts; all atlases ≤4096 px |
| Production staging | All 395 PNGs equal native exports; all metadata changes limited to rectangles, pivots, alignment and maximum texture size; sprite IDs/GUIDs retained |
| Unity asset validation | 17,494 assertions passed: actual imported dimensions, Point/uncompressed/unmipped/FullRect/64 PPU, fixed four-pixel floor pivot, all clip references resolved |
| Production Game scene | 5,902 pose/portrait cases passed at 1080×1920 and 1080×2400: stable integer pixels, fixed center/floor, visible bounds within screen and parent masks |
| Layout negative controls | Transparent canvas overhang accepted; deliberately moving opaque enemy artwork outside the safe area rejected |
| NUnit asset/motion regressions | Eight tests passed: three Ironvein, one Forest roster, one Forest import/taxonomy, two ability-motion, one King's Judgment native/contact test |
| NUnit gameplay playback | Both complete attack and ability fixtures passed: 16 authored basic attacks, actual impact sprites, duplicate protection, pause, return to idle, death/disable cancellation, shield fallback, King's column/row contact and settlement |
| Required repository gate | `Tools/Validate-Unity.ps1` completed successfully with Unity 6000.3.19f1 after final C# changes |
| Reference/player protection | Eight current reference and production hashes verified; 22 distinct bound player definition/controller/clip/texture files matched the immutable audit |

The first combat run exposed stale fixture assumptions. The seconds-based attack
fixture now loads the established version-one checkpoint and isolates each enemy
from other attacks/summons. The King cue fixture gives its two actors sufficient
health to survive legitimate refill cascades. Damage/event assertions remain;
no runtime rules were weakened. The layout validator previously counted empty
padding as outside artwork; it now checks actual alpha bounds for containment
and still uses the full rectangle for native integer-scale checks.

The seven unaffected tests passed in `regression-initial.xml` (that run also
records the two subsequently corrected failures). Both corrected gameplay tests
passed in `regression-playmode.xml`; the separately named Judgment asset fixture
passed in `regression-king-assets.xml`. There were no skipped tests in these runs.
See [validation ledger](EnemyArtCohesion/validation-ledger.json) for source hashes,
commands and original log paths. Full local logs remain at those locations.
The ledger's raw source hashes describe the files when tested, before Git
checkout. Git may normalize Windows line endings; portable Git blob IDs and
the tested source-tree commit are also recorded for checkout-independent
comparison. Those engine results describe the initial implementation before the
art-only Puffer correction above; they are not new engine validation of its pixels.
The whitespace check passes for authored changes. Immutable original Unity
metadata retains its existing trailing spaces on empty serialized fields; those
source bytes were preserved instead of reformatted to satisfy a text-only check.

## Direction-only follow-up (2026-10-11)

The previous all-west claim missed right-facing source drawings; Unity was not
rotating them into place. The shared UI visual/root transforms and material
sampling do not apply an enemy-specific reflection. This correction is baked
into native LibreSprite cels and their production PNG exports.

All 64 forms / 2,951 unique cells were inspected on labeled source contact sheets.
Six forms required 85 whole-cel reflections in 19 sheets: Seismic Smith 30,
Rail Switcher 36, Breakwater Captain 7, Briar Archer 3, Elven Mender 4 and Elven
Thornkeeper 5. Existing west-facing attacks are retained, as are frontal
transitions and backward leans with west-facing heads. There is no face repaint,
palette adjustment, resampling, new drawing, PixelLab generation or timing edit.
Puffer and the other 2,866 unselected cells remain unchanged.

Executed verification:

- `verify_facing.py --production`: all selected RGBA cels equal exact horizontal
  reflections of commit `0b03efb`; all other cels and unselected PNG sheets match
  that baseline. All 395 production PNGs match their current native exports.
  All import metadata and 353 clips retain their Git content (LF/CRLF normalized).
- `verify_candidates.py`: all 395 native files / 2,951 cels match exports, exact
  exposures and canvas dimensions; binary alpha; no outer-edge contact; no atlas
  exceeds the 4096 import limit. Reflection preserves the prior clear margins.
- Corrected Smith attacks/death, Rail attack and all four other corrected death
  sequences received frame inspection; source playback controls were sampled.
- The eight current reference images and production-source hashes still match
  the golden register. Rattlebones/player assets and restored Puffer are unchanged.

[Focused before/after player](../../ArtSource/EnemyArtCohesion/FacingReview.html)
embeds exact before sheets from Git and loads the current exports. It covers all
48 ready/animation states of the six forms. `facing_corrections.json` records the
zero-based selection; `facing-validation.json` records hashes and full coverage.
Older Highlights and Unity screenshots below document the earlier milestone.

Unity validation/runtime playback was not rerun for this art-only follow-up:
no Unity metadata, clips, controllers or C# changed. Earlier runtime results do
not establish runtime visual acceptance of the newly reflected poses. Full-pose
reflection changes prop handedness in corrected drawings; existing exposures
are retained, and no newly interpolated transition drawings are claimed.

## Visual evidence and limits

[Before/after player](../../ArtSource/EnemyArtCohesion/Review.html) covers every
bound enemy state at its source exposures, with native and integer zoom, light
and dark stages and a Rattlebones reference. The original full-cast source-frame
audit, all updated ready comparisons and the repaired/reoriented sequences were
reviewed. The corrected Puffer inflation family, Snapvine death, Smith hammer
repairs, King's trails, Mage face, banner finials and both Delver alternates
received explicit frame review. Selected source playback was sampled; this is
not a claim that all 353 clips were watched continuously in Unity.

[Highlights](EnemyArtCohesion/Highlights.png) are unchanged source-cell extracts
assembled for inspection at 3× nearest-neighbor, with before-art anchor aligned.
Eight final Game screenshots in this evidence folder show Puffer, Snapvine,
Seismic Smith and Royal Standard Bearer at both heights. The all-frame scene
fixture swaps visuals on the real first enemy host; its 35 HP/rank/intent display
belongs to that host. It proves art placement, not each displayed actor's kit.
The gameplay fixtures separately exercise the named actual kits and events.

No physical-device test or new human visual approval is claimed. The scene
coverage uses the standard first slot for the full cast, with other slots covered
by the narrower attack/ability fixtures; it is not every three-enemy combination.
Numerical empty margins alone do not establish repaired anatomy: source-edge
decisions and the actual repaired contours are retained separately.

## Sources and spending

`ArtSource/EnemyArtCohesion/` contains immutable originals, editable LibreSprite
sources, exact exports, material assignments, selected/rejected PixelLab outputs,
submitted images, job receipts and metadata recipes. Pixel mutations were authored
through LibreSprite's native cel API and exports; inspection scripts assemble
review layouts and verify bytes without authoring production pixels.

125 general masked results were reviewed: 89 selected, 36 rejected. Nine Smith
repairs were selected separately. Two whole-still studies were rejected; no new
identity replaced an approved character. The Snapvine wilt uses the existing
approved target as identity input, with native removal of an unwanted twig.
137 paid jobs consumed 815 PixelLab generations; final observed balance was 223.
No purchase was made. The generation ledger preserves guide v1.12 at dispatch;
the guide/register became v1.13 after implementation and v1.13.1 after the
user-directed Puffer face restoration.

Historical roster importers reference old rectangles and must not overwrite
these updated enemy assets. Use the current native/staging workflow. The user's
unrelated Court throne prefab and three consumable metadata edits are excluded.
