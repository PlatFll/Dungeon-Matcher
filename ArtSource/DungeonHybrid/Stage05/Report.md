# Stage 05 - full assembled Unity preview

Stage 05 is complete for user visual review. Stages 01-04 are user-approved.
No merge or production integration is authorized. No generation credits were spent.

[Review gallery](Review.html) | [Classic Unity screenshot](Previews/Unity-Classic-720x1280.png) | [Alternate dressing](Previews/Unity-Alternate-720x1280.png) | [Empty environment](Previews/Unity-Structure-720x1280.png) | [Exact asset ledger](AssetLedger.csv)

## Assembly

The new isolated scene is `Assets/_Game/Scenes/ArtPreview_DungeonHybrid_Stage05.unity`.
Open it directly, or rebuild with **Dungeon Matcher > Art Preview > Hybrid Stage 05 full assembled preview**.
The builder is `Assets/_Game/Scripts/Editor/DungeonHybridStage05Tests.cs`.

- Reused the complete approved Stage 04 top section: structural modules, floor/platform, focal gate, portrait, top controls and separate props. Its original child placement is preserved.
- Continued the existing wall tile pattern and side masonry behind the board and lower HUD. Complete masonry feet finish the lower corners.
- Reused all 64 approved 64x64 cells and their unchanged gem artwork at 1x. The existing modular board corners, rails and connectors fit around this grid at 2x, matching surrounding UI weight.
- Overfilled the board backing inside a mask to prevent wall fragments leaking between cells and frame. The portrait retains its approved clipped recess.
- Reused the Stage 03 lower HUD at 2x, with a static resonance fill. Added static current health-bar art to assess readability beside the approved cast.
- The editable scene contains separate Image sprites and modular repeat sections. Screenshots are review evidence, never a flattened production background.

The base assembly is 720x1280. Top scenery, frames and lower HUD use exact 2x source pixels; native 64px cells/gems and health samples use 1x. The 1440x2560 capture doubles the complete assembly exactly. This preview does not establish a production mapping for fractional resolutions, safe areas or other phone ratios.

## Validation performed

| Check | Result |
|---|---|
| Actual Unity art-preview Play Mode test | PASS: 1 explicit test, 8 captures across Classic, Alternate, Structure and SourceCheck views |
| Capture sizes | 720x1280 and 1440x2560 for every view |
| Whole-pixel render comparison | All four 1440 captures are byte-exact nearest-neighbor doubles of their 720 captures |
| Approved top section | Pixel-identical to Stage 04 with the optional new static health overlays hidden |
| Approved 64px board | Entire 512x512 cell/gem grid pixel-identical to the Stage 03 board study |
| Image layout checks | Point filtering, white untinted images, uniform integer scale/positions, native simple-image dimensions, tiled texel density, bounds and complete frame joins pass |
| Text/mask checks | No truncated labels; board recess edges have continuous approved outline; portrait source mask preserved |
| Source protection | All 1,058 protected files unchanged; all 8 pre-existing user edits in the main checkout unchanged |
| Sources used | 127 distinct PNG sources, all retained unchanged; zero new native artwork files |
| Repository validator | `Tools/Validate-Unity.ps1` PASS after the final editor change, Unity 6000.3.19f1, exit 0 |
| Production isolation | No gameplay/input owner in the preview; scene absent from build settings; no production scene, prefab, gameplay or art changes |

Machine evidence: `Diagnostics/Verification.json`, `Diagnostics/UnityTests.xml`, `Diagnostics/UnityValidation.json`, `Diagnostics/ProtectedHashes.json`, `AssemblyUsage.json`. Full local logs remain in `Diagnostics/UnityTests.log` and `Diagnostics/UnityValidation.log` (ignored by Git). An initial clone-name lookup failure is preserved in `Diagnostics/UnityTests-initial-clone-name-failure.xml`; it was fixed before the successful final run.

## Visual review

The quiet wall and flat platform remain intact, with the approved gate as the top focal point. Warm torches and muted banners support the cast without obscuring faces. The board has uninterrupted dark inner borders and centered modular joins. Repeated masonry continues behind it without torn patches. The lower HUD uses the approved restrained violet/silver family, and the bright gems remain distinct from the environment. Classic and alternate dressing were both inspected at native assembly size; no new blur, torn silhouettes or accidental frame gaps were found.

This is an actual Unity **art-only** Game View, using static Rattlebones, Farmer and Pan references and demonstration health/resonance values. It is not a playable-combat, Android-build or real-device verification. Final artistic acceptance belongs to the user; no numerical style score is claimed.

Stop here for Stage 05 approval. Dedicated branch: `feature/dungeon-zone-hybrid-pixellab-rebuild`; draft PR #205 remains unmerged.
