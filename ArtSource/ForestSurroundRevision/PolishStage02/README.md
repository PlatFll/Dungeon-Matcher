# Forest polish — Stage 2 root / branch joins

Review candidate; stop before Stage 3. No merge or production integration.

## Problem and correction

The two lower boughs ended in flared triangular tips. Those separate tips overlapped
the pre-existing stone roots with mismatched tangents and exposed old spur outlines.
Two image-conditioned PixelLab edits rebuild the connection as continuous bark curves.
LibreSprite cleanup restores the existing upper leaf crowns, reconciles wood ramps,
removes detached generated scraps and saves the repaired roots as separate native layers.
The old bough-5 instances and combined stone/root instances are disabled only in this
new review scene; every original source and earlier scene is preserved.

Four modular exports:
- Root_Left / Root_Right: 88×180, 19 opaque colors each, one connected silhouette each.
- StoneAnchor_Left: 85×101; StoneAnchor_Right: 86×105; 13 colors each.

The stone layers are separated from the original roots so obsolete wooden spurs do
not survive under the repair. Every original colored stone-plane pixel is unchanged:
1,868 on the left and 3,378 on the right. The small stone recess exposed by removal of
the disconnected right spur is filled with the existing stone ramp. This restores the
surface behind removed wood; it does not move or redesign the corner stone masses.
Each assembled root/stone pair has one connected silhouette. Pixel connectivity is
supporting evidence; the actual Unity comparison is the visual review authority.

## Generation and native lineage

PixelLab Pixen edited two 88×184 transparent native assemblies of the current bough
and stone source files. `References/InputAssembly.json` records exact positions/hashes.
No image scaling was used. PixelLab received the actual target assemblies, not a
flattened screen. Current Rattlebones/Farmer/Pan references were hash-verified and
visually inspected for rendering discipline. The source-image target governs placement.
Both requests completed at 1 generation each: **2 generations**, balance
690.20 → 688.20, zero USD credits. Total debited across this latest polish pass is 9,
including the Stage 1 service/cancellation discrepancy documented there.

Generated stone changes were discarded. Original stone material pixels remain exact.
Editable `.aseprite` files and PNGs were written through LibreSprite, then reopened
and re-exported for exact RGBA comparison. Alpha is binary; all occupied bounds have
at least two pixels of clearance. The two final 88×180 root canvases remove only
empty rows from the 88×184 generation canvas; no rendered pixels are cropped off.

## Verification

- `Tools/Validate-Unity.ps1`: passed with Unity 6000.3.19f1.
- `ForestRootJoinReviewTests.RootJoinReview`: passed.
- Actual Unity screenshots at 720×1360 and 1440×2720, exact nearest-neighbor relationship.
- Before capture matches the approved Stage 1 separator capture exactly.
- Top 1,000 rows, board cells and painted frame, bottom HUD/buttons/text and existing
  mushroom/moss accent pixels remain identical.
- All 303 prior visible Image bindings/RectTransforms preserved.
- 6708 existing art/scene/script/resource/settings files remain hash-identical.
- No screenshot differences outside the old/new authorized root-and-anchor masks.
- All four LibreSprite source reopen/export comparisons pass.

Scene: `Assets/_Game/Scenes/ArtPreview_ForestSurround_RootJoins.unity`.
Menu: Dungeon Matcher / Art Preview / Forest continuous lower root joins.
Excluded from player builds. Captures use static approved character/UI samples;
this is not live production combat, Android testing or real-device evidence.

## Next checkpoint

The red-circled root joins are this stage's revision. Mushroom/greenery attachment
and richer corner integration remain Stage 3 work. No new wisps or animations.
Branch feature/forest-zone-side-surround-revision, draft PR #209. Stop for review.
