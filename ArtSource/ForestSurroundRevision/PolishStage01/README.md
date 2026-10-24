# Forest polish — Stage 1 separator review

Candidate awaiting user review. Stop before Stage 2. No merge or production integration.

The previous ground edge read as an indistinct thin strip. This revision replaces its
front 360×16 native band with nine broad constructed mudstone blocks, using three
40×16 modules. The floor where enemies stand, board position and all UI remain fixed.
The native band is y=224–240 within the existing 360×240 top environment; it renders
at screen y=448–480 in the 720×1360 review. The existing portrait foliage and foreground
continue to render over it. No old asset, scene, prefab or production code is rewritten.

## Art decisions

The gray study was too tall and cracked. The olive mudstone source better relates to the clearing and wooden frame. Its double-course geometry is rebuilt into three broad single-block pieces with connected flat faces, native top bevels, shallow wear and occasional attached moss. Nine blocks make a deliberate full-width rhythm; no extra decoration is added.

Selected raw source: Mudstone_Pixen.png, job a21ef4e6-9058-47dc-807f-f5bf5fcde4cf.
The 64×32 image contained two masonry courses. Native cleanup removes the small
internal divisions and constructs three 40×16 single-block variations from its
outer contours and stone planes. Source rows/columns and every native edit are recorded.
The unselected gray Pixen study was too tall and cracked for this boundary.

PixelLab balance decreased by **7 subscription generations** (697.20 → 690.20),
with zero USD credits used. Two completed Pixen calls explicitly charged 1 each.
An additional 5-generation debit appeared after cancellation of the Pro Flash request,
matching its quoted cost; the service returned no finished image or refund confirmation.
The earlier Pro job remained at 64% for over 15 minutes and was cancelled without
an output. Receipts, cancellation/error responses and observed balance are retained.
Do not count either cancelled request as successfully delivered artwork.

Pro/Pro Flash received the actual verified Rattlebones style PNG but stalled.
Pixen has no image-reference input. Its result was therefore manually inspected
and cleaned against the actual current golden images and forest palette; no
image-conditioned lineage is claimed for the selected Pixen source.

LibreSprite writes the final native pixels, saves editable `.aseprite` sources,
exports PNGs and reopens/re-exports the documents for exact comparison.
`Diagnostics/NativeCleanup.json` records source coordinate selections, exact palette
mappings and every final pixel correction. Exported tiles are native 40×16 with
no image resampling or filtering. Opaque mortar is intentional at repeatable
tile edges; transparent padding would produce gaps in a full-width structural strip.

## Unity / pixel evidence

- `Tools/Validate-Unity.ps1`: passed with Unity 6000.3.19f1.
- `ForestSeparatorReviewTests.SeparatorReview`: passed.
- Actual captures: 720×1360 and 1440×2720, exact nearest-neighbor relationship.
- All 294 existing visible Image bindings and RectTransforms preserved.
- Before capture equals the prior unified-corner capture exactly.
- Every pixel above y=448 and below y=480 matches the before capture exactly.
- 6697 existing art/scene/script/resource/settings files hash-identical.
- All three native sources reopen to exactly the exported RGBA pixels.
- Tiles are fully opaque, point sampled, unmipped, uncompressed; seams verified against exports.
- Board remains 512×512 at (104,528); portrait/frame dimensions unchanged.

Scene: `Assets/_Game/Scenes/ArtPreview_ForestSurround_StoneSeparator.unity`.
Menu: Dungeon Matcher / Art Preview / Forest stone separator.
The scene is excluded from player build settings and uses static readability samples.
This is not live production combat, Android validation or real-device testing.

## Scope / next checkpoint

The annotated lower root joins and mushroom/rock attachments remain outstanding.
They belong to later requested stages and are not presented as fixed by this separator.
Stage 2 starts only after review. Branch: feature/forest-zone-side-surround-revision;
draft PR #209 targets feature/forest-zone-staged-rebuild. Main unchanged.

Scene-object audit: among 1,891 existing serialized objects, only the background
parent child-list gains one entry. The separator adds 38 serialized objects; none
of the old objects is deleted. Unity-added trailing whitespace was normalized
only in the new scene after capture; serialized values are unchanged.

The first pixel-check assumption treated the last eight strip rows as uncovered.
The existing portrait leaf actually extends to y=475. The final check compares
every visible tile pixel to the exact export and every overlapping foreground
pixel to the exact before capture; it also checks the unobstructed bottom seams.
No artwork or layout was moved to satisfy the test.
