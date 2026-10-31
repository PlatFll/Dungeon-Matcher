# Unified battle UI — integration review

The user approved the expanded native Ironvein cave for the actual game and requested the underwater stone controls in every zone. This branch retains the production `IronveinAssembly.cavernBackdrop` binding from PR #219, then unifies the existing Game scene's control presentation. No review scene is required at runtime. Nothing is merged.

## Art and ownership

- All five zones use the unchanged `ReferenceDungeon/SharedGuide`, `SharedSettings`, `SharedPotionHolder`, `SharedAbility` and `SharedEnergy` PNGs already used in Drowned Court. Both supply buttons intentionally use the same Court holder, with their existing potion/bomb icons.
- `BattlePresentationSkin.Controls` adds shared geometry to the existing presentation helper. `RunControlsUI`, `AbilityButtonUI`, `WaveTrackerUI` and `GameplayPixelLayoutController` retain ownership; no replacement input, board or combat controller was introduced.
- Top controls now use the normal integer Canvas scale instead of the Court's independent horizontal/vertical environment registration. The Settings source remains 45×40, with equal X/Y pixel scale; its gear is not stretched or duplicated.
- The wave is PixelLab candidate A, finished through LibreSprite. Its source side/corner pixels remain native; clean center columns extend the rails. A 144×34 body with 11/10/11/10 borders tiles to the available width, with a separate 16×19 attached skull. No generated letters: live text uses the existing native `PixelTextFitter` and supports long wave numbers.
- The body/ornament each use 15 opaque colors, binary alpha, Point filtering, uncompressed/mip-free imports. Editable ASEs and exact PNG save/reopen evidence are included.
- Existing board/portrait geometry, enemy floor lines, characters, health bars, zone art, gameplay rules, saves and settings-menu internals are preserved. The enemy intent view's existing wave-clearance clamp responds to the new header; its artwork, actor floor and slot constraints remain unchanged.
- At 360×640, native bottom controls fit below the playable grid, with energy in the existing free gap. The optional bottom caption is hidden at compact height, while Guide remains available. The pre-existing Editor-only reference-resolution warning is visible in compact captures and has not been hidden.

## Provenance and spending

`QA/Generation.json` records the exact PixelLab request and receipt. Inputs were the supplied control/wave screenshots plus the actual native Guide art. All inputs were sent through supported reference/style image fields. The live art guide and golden register hashes, Rattlebones source hashes and UI hashes are in `QA/OriginalSources.json`; retained source art is independently rechecked in `QA/PreservedArtwork.json`.

Job: `cb797467-9744-4c13-a308-9465a122c060`; Pro, 144×48, seed 110126, four candidates. Cost: **10 subscription generations**, no cash credits. Balance: 4788 → 4778. A is the implementation candidate; B/C/D remain source history, not approved assets. The newly generated wave still awaits the user's final visual review.

## Verification

Actual Unity Game captures cover Dungeon, Magical Forest, Drowned Court, Ironvein Excavation and Frostwake Peaks at 1080×2340, 720×1280, 360×640 and 1080×2400 with safe-area insets. The baseline run and final run are recorded in `QA/Validation.json`.

Checks include exact non-control artwork placement, board geometry and actor floors; shared asset bindings and identical native control dimensions across zones; whole physical pixels; safe-area/gem-grid clearance; no duplicate old frames; long wave labels; and existing enemy-counter clearance. Actual click callbacks exercise Guide/Settings, potion stock consumption, bomb targeting/cancellation and ability energy spending in each zone, using disposable profiles.

`Tools/Validate-Unity.ps1` is the mandatory compilation gate. This is automated Unity Editor validation, not Android or physical-device testing. See `QA/Validation.json` for final exit statuses and log paths.

Open `Preview/Review.html` for before/after selection and native artwork. `Preview/AllZones.png` is a nearest-neighbor overview of actual Game screenshots. Original approved art files remain byte-identical. The branch is based on the approved cave branch, preserving the separate PR #219 history; stop before merge.
