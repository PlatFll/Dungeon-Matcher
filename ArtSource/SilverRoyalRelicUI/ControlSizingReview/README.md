# Larger shared Silver controls — review candidate

The user requested opening the ability bar in LibreSprite and approximately 50%
larger wave, Settings and supply holders. All existing sprite files are preserved;
only shared presentation sizing/placement changes. No generation credits spent.

- Wave: up to 264×99 logical pixels instead of 176×66; straight rails shorten
  on constrained screens, with contiguous modular joins and uniform ornaments.
- Settings: 84×84 instead of 56×56; remains square.
- Supply holders: 105×105 instead of 70×70; potion/bomb icons 72×72 instead of
  48×48, centered with count labels inside the holders. The nine-unit ability
  gap is preserved, and a six-unit caption gap is reserved.
- Ability frame 176×66 and energy bar 147×24 remain unchanged.
- 360×640 retains compact controls. Top widths below 400 logical pixels and
  bottom HUDs shorter than 156 use the compact family rather than overlap.

Point filtering is preserved. The requested 1.5× display factor produces hard,
alternating pixel widths on 1× canvases; it gives exact 3× source pixels on 2×
canvases. It does not resample PNGs. The validator still enforces uniform exact
requested ratios, contiguous wave rails, safe bounds and unchanged native policy
for the board, actors, ability frame and bar.

## Verification

`Tools/Validate-Unity.ps1`: passed Unity 6000.3.19f1 after the final corrections.
Log: `C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-a70c37a3-cb6c-4195-8e9b-f867dba741f6.log`.

`Tools/Test-ReferencePresentation.ps1 -Filter SilverRoyalRelicProductionTests`:
1 fixture passed, no failures/skips. It visits all five zones at 360×640,
720×1280, 1080×1920 and 1080×2400 with simulated safe insets, then all four
characters at 720×1280. Checks include source bindings, square/uniform sizes,
centered icons, contained counts, wave seam joins/rail density, caption spacing,
board clearance, live WAVE 1/WAVE 99999 fitting, Settings/pause, potion use,
bomb targeting/cancel, ability activation/spending, ready state and Reduced Motion.
Result: `.utmp/ThreeZonePresentation/reference-tests-20261112-032506.xml`.

Two earlier runs correctly caught icon-centering drift and caption overlap;
runtime placement was corrected and both checks remain strict. No test bypasses.
All Silver PNG hashes are unchanged; Rattlebones' registered hash also matches.
No Android build or real-device test was performed for this sizing-only change.

## Review

[Before/after actual Game captures](BeforeAfter.png) · [Native screenshot crops](ControlsCloseup.png).
These are two separate gameplay runs; board contents and idle poses may differ.
The complete local gallery contains 20 paired captures and a native-pixel view:
`http://127.0.0.1:57126/LargerControls/Review.html`.
Raw captures and reports: `.utmp/SilverControlSizing/`.

A separate unchanged editable copy of the ability-bar frame is open in LibreSprite:
`C:/Users/USER/.codex/visualizations/2026/10/10/01a127fb-8a65-7d73-9061-df9dc2d9843a/AbilityBarSilver.ase`.
Existing user editing tabs were preserved. This is on the current UI review branch,
not merged to main; stop for visual review.
