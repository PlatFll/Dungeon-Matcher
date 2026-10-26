# Drowned Court: rear architecture and shared enemy floor

Status: ready for user visual review on draft PR #212. Not merged.

## Visual change

The upper ruin assembly moves back **16 native pixels**, without resizing,
recoloring or editing any source sprite. Eight copies of existing floor-top
modules add one rear course, so raised column/arch bases remain grounded.
The front floor and all actor/UI placements in the art scene are preserved.
The previous review scene remains intact; Revision03 is a separate scene.

The builder verified 379 original rectangles and all 345 original image
bindings. The complete area below native row 240 is pixel-identical before
and after at 360×640, 720×1280 and 1080×1920: 2,016,000 protected pixel samples.
The existing 24-second ambient loop has 288 actual Unity PNG frames and an
exact start/end match. No source PNG, ASE, animation, controller or prefab
was edited. No PixelLab generations were spent; cumulative pack spend stays 94.

## Shared production floor correction

Previously, only Dungeon/Forest applied the HP-relative native floor inset.
The other zones skipped it, while older HUD enclosures added another vertical
offset. The existing BattleFloorAnchor, EnemySlotUI and pixel character
presenter now use the same registration in all five zones. The layout owner
exposes its existing enclosure offset for compensation; it does not move the
board, portrait, controls or other panels. Character scale, artwork, source
pivots, gameplay, combat, saves and gem rules remain unchanged.

Independent pre-change evidence was captured from untouched HEAD
`f801cc121292d8fb509e85108f7f4ac143a57381`, with seven original runtime-file
hashes recorded in `QA/Revision03/OriginalMatrixSource.json`. Current working
files were backed up and restored byte-for-byte after that capture.

The initial new test incorrectly assumed a zero sprite snap offset and
identical legacy enclosure positions. With explicit user approval, its oracle
was corrected using the independent pre-change captures. Exact full-rectangle
assertions, HP-row comparisons and strict floor tolerance remain; no failure
is suppressed. All measured floor coordinates actually match exactly.

| Screen / safe area | Shared source floor, physical Y from bottom | Zones / enemies |
|---|---:|---:|
| 720x1280 | 872 | 5 / 15 |
| 1080x1920 | 1404 | 5 / 15 |
| 1080x2400 | 1588 | 5 / 15 |
| 1080x2400-safe | 1468 | 5 / 15 |
| 360x640 | 444 | 5 / 15 |

The inset safe-area case uses Rect(24, 96, 1032, 2184) at 1080×2400.

## Executed validation

- Real Game Play Mode: **25 cases, 3 live enemies each**, covering Dungeon,
  Magical Forest, Drowned Court, Ironvein and Frostwake at four resolutions
  plus an inset safe area. All floor and HP-row assertions passed.
- Complete board/portrait rectangles equal their independent original
  captures exactly. Dungeon/Forest retain their approved feet and HP placement.
- Existing GameplayPixelLayoutValidator reports zero errors in all cases.
- `SharedEnemyFloorTests.AllZonesShareTheApprovedDungeonFloor`: 1 passed,
  0 failed, 0 skipped; the single test contains the full 25-case matrix.
- Required `Tools/Validate-Unity.ps1`: passed, Unity 6000.3.19f1.
- Native visual/source preservation and loop checks: passed.
- No new Android build or physical-device test was performed.

## Review and reproduction

- [Before/after and lossless animation gallery](Preview/Revision03/Review.html)
- [Verification measurements](QA/Revision03/Verification.json)
- [Final test XML](QA/Revision03/After-Tests.xml)
- [Compiler evidence](QA/Revision03/UnityValidation.log)
- Scene: `Assets/_Game/Scenes/ArtPreview_DrownedCourtV2_Revision03.unity`.
- Rebuild via **Dungeon Matcher → Art → Drowned Court V2 → Build Depth Revision 03**.

The revised underwater artwork is still an **art-only review scene**. The
separate Game screenshots verify the shared production floor fix using each
zone's currently bound environment. The new underwater library has not been
silently imported into Game. Formal pack stages 09–12 have not been advanced.
