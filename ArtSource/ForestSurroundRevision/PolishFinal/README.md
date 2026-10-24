# Magical Forest — final surround polish, Stages 3–6

Final review candidate. User authorized completion of every remaining stage in one
run; this supersedes the earlier per-stage stop instruction. **No merge.**

## Completed work

- Stage 3: two 88×180 corner modules integrate pale/red mushrooms, stems, soil
  contacts, moss and small flowers into the existing stone/root formations. The
  approved upper root/crown section is retained at its exact native registration.
  Old combined layers and separate mushroom accents remain preserved in earlier
  scenes, and are disabled only in this final review variant.
- Stage 4: a paired set of sweeping roots, two hanging-vine modules and two
  attached growth patches enrich the lower surroundings. One main stone mass
  remains per side. Wood roots carry the composition; foliage supports them.
  The weaker left branch studies were rejected. The stronger right S-curve study
  supplies the paired root variants, with native end completion, cleanup, and
  different flower accents. Nothing is resampled. Broad top-lit planes remain
  consistent on the opposite-side variant.
- Stage 5: one 24×24 four-color spirit sprite appears at four separate placements.
  Preview motion uses a six-second staggered fade and one native pixel of hover.
  Reduced motion holds a stable position and opacity. No rotation, scaling,
  texture blur or subpixel movement is used.
- Stage 6: art review, native integrity, source preservation, layering, pixel
  scale, locked-region comparison and Unity compilation/render/motion checks.

## Sources and workflow

PixelLab produced eleven completed image jobs. The two corner edits were conditioned
on exact native assemblies of the current root/stone/attachment assets. New
branch, vine, growth and spirit studies were generated individually; no full
scene repaint. The current Rattlebones, Farmer and Pan PNGs were hash-verified and
inspected for rendering discipline, not copied as environment palettes.

Nine modular PNGs and nine editable `.aseprite` files were written through
LibreSprite native pixel scripts. Each was reopened and re-exported, then compared
exactly. Raw generations, edit inputs, request receipts, selected/rejected-source
ledger, cleanup plans and scripts are retained alongside the exports. Controlled
material ramps are used; the larger combined corner sprites include the retained
leaf-crown colors and multiple stone/wood/plant/mushroom materials.

PixelLab balance: **688.20 → 677.20**, 11 subscription generations this continuation,
zero USD credits. Latest six-stage polish total: **20 debited**, including Stage 1's
previously reported cancelled-request debit. No generation remains in progress.

## Executed validation

- `Tools/Validate-Unity.ps1`: passed, Unity 6000.3.19f1.
- `ForestFinalPolishReviewTests.FinalReview`: passed.
- Actual Unity captures for Stages 3, 4 and 5/final at 720×1360, plus final 1440×2720.
- Final large capture is exact nearest-neighbor doubling of the smaller capture.
- Before capture matches the approved Stage 2 capture exactly.
- Upper HUD/battle/separator, board cells/painted frame and bottom controls/text
  remain pixel-identical; no changed pixel falls outside authorized dressing.
- All 303 existing visible Image bindings/rectangles remain intact.
- All 6721 existing art/scene/script/resource/settings files remain hash-identical.
- Every new native sprite has binary alpha, one connected silhouette and clear
  canvas margins. All nine LibreSprite save/reopen/export comparisons are exact.
- Twelve sampled Unity motion frames change only the new spirit-light pixels;
  loop closure and static reduced-motion behavior pass.

`Assets/_Game/Scenes/ArtPreview_ForestSurround_FinalPolish.unity` is a separate
review scene, excluded from player builds. The motion helper is guarded by
`UNITY_EDITOR`; it is not a new production ambient system. Captures contain static
approved actor/UI samples. No production combat, Android build or physical-device
test is claimed for this art-review stage.

## Review

Open `Review.html` for before/final comparisons, intermediate stage captures,
actual Unity motion playback, raw target reference, native assets and editable
sources. Prior scenes remain available. Draft PR #209 on
`feature/forest-zone-side-surround-revision` is updated for final visual review.
Stop here; no merge or next task is authorized by this report.
