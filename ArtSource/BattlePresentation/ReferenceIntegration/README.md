# Reference reconstruction — 2026-10-15

The user approved the right-hand Dungeon art-only assembly and requested its
integration, followed by Forest and Drowned Court kits built the same way.

## Delivered scope

- Dungeon uses 36 individual approved PNG modules in `ReferenceDungeon/`.
  `DungeonArtOnly.png` and `BoardWell.png` are excluded from production imports.
  Wall, door, floor and decorations remain separate; characters, gems, labels,
  HP/shields, item icons and ability icons are live game content.
- Existing presentation owners fit the kit to the current safe-area layout.
  The previous Dungeon vista/ambient overlays are suppressed to avoid doubling
  landmarks. Its static reference torches remain visible.
- The approved silver Guide, settings, supply, ability and energy sources form
  one shared family across Dungeon, Forest and Court. Interaction uses tinting
  of those same plates, without switching to an older silhouette on hover.
- Forest/Court are **source candidates for visual review**, outside Assets.
  Each has layered scenery, panel, blank header, distinct frame corners/beams,
  empty board wells, side dressing and the same six shared controls.
- No new generation calls. Main menu, opened settings, character/gem drawings,
  combat, board resolution/timing and all five pre-existing local changes are
  preserved. No merge authorization was supplied for this continuation.

## Review

- [Dungeon in Unity](DungeonUnity.png)
- [Forest comparison](../ForestReferenceKit/Review.html)
- [Drowned Court comparison](../CourtReferenceKit/Review.html)
- [Original approved Dungeon assembly](../DungeonReferenceKit/Review.html)

The source grid is 470x836, sampled consistently from the supplied 941x1672
mockups. This is a declared production grid, not a measurement of an unknown
original native canvas. PNG dimensions, visible palette counts, binary alpha,
hashes, editable-source pixel equality, module assembly and six landmark patches
per new zone are independently checked in the respective `checks.json` files.

## Production adjustments and remaining differences

The board remains a square 8x8 gameplay grid. The reference rim's 364x369 opening
is fitted around it, a 1.37% opening-aspect correction. Responsive scenery fits
may produce uneven physical widths for source pixels under Point filtering;
there is no smoothing. Physical module edges are snapped and validated. The
original target's actors, counters, HP bars and gem drawings are not replacements
for the game's approved live assets.

Surfaces concealed by mockup characters and labels cannot be extracted. Forest
uses source foliage/wood/water/ground clusters in those areas; Court combines
source ruin texture with traced stone courses. The new hidden paving is more
regular than the original exposed stones. These differences are shown in native
comparisons and overlays and still require visual approval. Technical success
does not establish an exact visual match or user approval.

## Validation

`validation.json` records final commands, results and source hashes. Tests cover
three zones, three portrait configurations (including safe-area insets), all
five ability symbols, pause/reduced motion, idle snapshot/RNG preservation,
accepted swaps and scene cleanup, plus existing board-feel/input regressions.
Actual Unity captures are used; audio is muted. No Android-device testing is
claimed. The import copies native PNG bytes without re-authoring the source.

Final result: **40/40 tests passed**, no failures or skips, and
`Tools/Validate-Unity.ps1` completed successfully with Unity 6000.3.19f1.
The test XML and nine layout reports are committed. Complete logs remain local
in `evidence/`; their SHA-256 hashes are recorded in `validation.json`.
Both browser galleries loaded all 30 images; their overlay controls were checked.

## Rebuild

1. Run `BuildReferenceZones.py` with the configured Python runtime.
2. Run each new kit's `Build.ps1` through LibreSprite.
3. Run `ReviewReferenceKits.py` for source verification and review pages.
4. `Tools/Import-ReferenceDungeon.ps1` imports **only approved Dungeon modules**.
5. Run `Tools/Test-ReferencePresentation.ps1` and `Tools/Validate-Unity.ps1`.

The immutable earlier Dungeon review/receipt remains approval history. Its old
source-only verifier intentionally rejects subsequent Unity changes; this
integration uses the new import-byte and actual-scene checks instead.
