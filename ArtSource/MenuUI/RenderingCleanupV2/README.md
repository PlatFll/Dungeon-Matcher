# Main-menu shading cleanup V2

## What the audit established

The live owner is `MainMenuArtwork.Initialize`, which creates
`MenuDungeonArtwork` from `UI/GlobalMenu/HallTall` at 360×800, offset (0,−80).
The logo, character, account labels, buttons and icons are separate layers.
The hall's fire and portal effects are static painted pixels; only Rattlebones
uses an animated idle here.

V1 did reach Unity: the native PNG and production resource were byte-identical,
and the rendered colors confirmed **455 of its 693 edited native pixels** were
visible at 1080×1920. The other 238 were occluded or could not be confirmed by
that comparison. Its selection accepted only detached 1–14-pixel fragments,
so larger connected bands and mottling remained. Its character also appeared
in different idle frames. The visual improvement was too small, despite valid
exports. See `evidence/source-audit.json` for measured evidence and bindings.

## Current edit

The starting point is the current implemented V1 background, preserved under
`before/`. LibreSprite edits the existing source cels at native resolution.
The selected edits consolidate redundant secondary highlight strips, small
interior stains and mottling in stair faces, floor paving, one warm pier and
small arch/pedestal surfaces. Main light/shadow masses, lit primary edges and
structural cracks retain their original pixels. Already quiet wall areas remain
unchanged. Wood, metal, ornaments, carpet, flames and portal effects are preserved.

The three-region pilot was inspected at native size and 4× before extending
the same approach. An earlier trial added larger shadow wedges. Enlarged review
showed that they looked like new marks on the stone; that trial was rejected.
The final recipe consolidates existing tones without introducing those wedges.
No image model, global palette operation, resampling, blur or sharpening was used.

**Art judgment:** the final stone bevels are less striped, with fewer secondary
glints competing with the broad forms. The original aged edges, masonry depth
and purple/warm lighting remain. This is still a restrained change in a full
menu; the stone close-ups and unobscured environment show it more clearly.
That assessment is subjective and remains for the user's approval.

## Review artifacts

- [Environment-only Unity before/after](review/Unity-1920-environment.png)
- [Full Unity menu before/after](review/Unity-1920-full.png)
- [Tall full menu comparison](review/Unity-2400-full.png)
- [Normal final menu](captures/1920-after-full.png)
- [All selected stone close-ups at 4×](review/native-samples-4x.png)
- [Left paving](review/native-left-paving-4x.png) / [right paving](review/native-right-paving-4x.png)
- [Pilot samples](review/pilot-samples-4x.png)
- [Hall edit overlay](review/native-Hall-overlay.png) / [HallTall edit overlay](review/native-HallTall-overlay.png)
- Native [before Hall](before/Hall.png) / [after Hall](native/Hall.png),
  [before HallTall](before/HallTall.png) / [after HallTall](native/HallTall.png)
- Full source at 4×: [Hall before](review/native-Hall-before-4x.png) /
  [Hall after](review/native-Hall-after-4x.png),
  [HallTall before](review/native-HallTall-before-4x.png) /
  [HallTall after](review/native-HallTall-after-4x.png)

The environment-only Unity view hides all separate menu children, including
the character and UI, while using the same stage and image geometry. Static
painted fire remains identical in both views. Both complete menus hold the
actual approved `Rattlebones_Idle_00` sprite. There are no moving fire/particle
layers in this menu. Captures are made in the same disposable session with
matching resolution, state, framing and pose; no production animation behavior
is changed. Full-resolution originals are retained under `captures/`.

## Exact source changes and preservation

| Files | Change |
| --- | --- |
| `ArtSource/MenuUI/native/Hall.aseprite` and `Hall.png` | Existing 360×640 hall, selected stone shading only |
| `ArtSource/MenuUI/native/HallTall.aseprite` and `HallTall.png` | Same edits in top 640 rows of the existing 360×800 source; extension unchanged |
| `Assets/_Game/Resources/UI/GlobalMenu/Hall.png` and `HallTall.png` | Identical copies of those native exports |
| `Assets/_Game/Scripts/Editor/MenuUiReviewTests.cs` | Editor-only matched-background capture; no runtime/menu behavior changes |
| `Tools/Build-MenuArt.ps1` | Preserves finalized V2 sources after the historical builder |

Both source PNGs have **835 changed opaque pixels relative to V1**, **zero alpha
changes**, and the same **38-color palette**. ASE pixels equal their PNG exports.
All deep seams and their immediate neighboring pixels are locked. Separate
predefined focal-object and primary-edge regions are also pixel-identical.
Opaque alpha alone is not used as proof of internal geometry preservation.
The exact masks, palette populations, hashes and old/new pixels are recorded in
`evidence/native-pixel-checks.json`, `native-protected-geometry.png`, and the two
`native-*-edits.json` ledgers. No major shape, position, silhouette or UI element
was changed. Scene files, runtime UI code, character art, fonts, logo, control
assets, all import settings, GUIDs and pivots retain their original bytes.

`baseline.json` preserves source/reference hashes and unrelated user files.
The actual current Rattlebones golden and production idle were inspected and
hash-verified against the current register. The art guide/register are unchanged.
No new global art rule or reference approval is created by this experiment.

## Reproduce and validate

`Build.ps1 -Stage pilot` or `-Stage native` runs `ShadingPlan.js` and `Edit.js`
inside LibreSprite on copied original `.aseprite` cels. `Verify.py pilot` and
`Verify.py native` independently decode ASE and check exact pixel, palette,
alpha, protected-edge and unrelated-file preservation. `Verify.py native
--install` copies only the two existing backgrounds and editable sources.
`Review.py` builds inspection sheets; it never edits production art.

`Capture.ps1` runs the existing menu suite plus the controlled capture check,
with editor audio muted and disposable account data. `ReviewUnity.py` then
checks that unobscured Unity pixels equal the expected nearest-neighbor native
source, and that full-menu differences occur only at edited background pixels.
`Tools/Validate-MenuArt.py` checks all 54 native/production/import pairs.
`Tools/Validate-Unity.ps1` is required for the editor capture-code change.
Actual final test results are retained under `evidence/`; no physical-device
display test or subjective user approval is claimed.

Final results: **Unity validator passed; 54/54 native/import pairs passed;
all four MenuUiReviewTests passed with audio muted.** Both 1080×1920 and
1080×2400 environment captures equal the native source pixels exactly at 3×.
The complete-menu pairs have **zero differences outside the edited background
pixels**: the same character frame, text, buttons and layout are visible.

**Remaining visual concern:** 349 of the 835 changed native cells are visible
in each complete-menu capture; the other 486 are covered by the existing UI.
All 835 are visible in the environment-only captures. The normal-menu change
therefore remains modest. This is measured exposure, not a numerical score for
art quality. The close-ups are the clearest place to judge the shading treatment.

PR #198 remains the dedicated review branch. Unrelated working files remain
excluded. Await user visual approval; do not merge.
