# Lighter gray shared controls

The user accepted the new stone shapes and requested the previous lighter gray.
This revision maps only native colors in LibreSprite, preserving all 13 source
dimensions, alpha masks, chipped contours, engraving shapes and placement.
The wave remains up to 176×48, Settings 56×56, supply tiles 64×64 and ability
plate/socket 176×66. The palette comes from MaterialRevision/Inputs, retaining
the older ability/supply main face gray #838091. The ready engravings restore
their previous colors and retain their existing animation and white glow.

[Review actual Unity screenshots and the native comparison](Preview/Review.html).

No PixelLab generation or credits were used. Approved shape sources are copied
unchanged to Before; editable ASEs, exact exports, reopened exports and the
actual LibreSprite pixel-remapping script are included. QA/NativeValidation.json
records color maps, exact mask checks, bounds and source/export hashes. There
are no runtime, gameplay, layout, importer or scene changes in this revision.

Actual Game evidence covers all five zones and four existing screen profiles.
Previous screenshots are preserved in MaterialRevision; only the new captures
are stored here. The user approved this revision for production across all
five zones and authorized merging PRs #219, #220 and #221 on 2026-11-01.

Executed verification: StoneBattleUiTests.After passed all 20 actual Game cases
(five zones × 720×1280, 1080×2340, 360×640 and 1080×2400 simulated safe area).
Native source masks, dimensions and LibreSprite round trips are exact. All nine
protected starting files and all preceding review screenshots were restored
byte-for-byte after capture. Runtime C#, importer metadata and scenes have no
task changes. Art-only scope does not require the separate compilation validator;
the actual Unity play-mode capture suite ran successfully. No Android build or
physical-device test. Pre-existing compact camera warning remains visible.

## Production integration verification — 2026-11-01

The enabled `Assets/_Game/Scenes/Game.unity` scene uses the common runtime path:
`BattlePresentationSkin` loads `UI/BattlePresentation/Stone` before zone fallbacks;
`GameplayPixelLayoutController`, `WaveTrackerUI`, `RunControlsUI` and
`AbilityButtonUI` place the same controls in Dungeon, Magical Forest, Drowned
Court, Ironvein and Frostwake. The four serialized ability definitions supply
their engraving masks. No art-preview scene is required for these bindings.

At source commit `90333e9a3419499f40157865ce7c76c941412266`, all 13 current PNGs
match the native QA hashes. The final `QA/UnityLayouts.xml` remains valid for
the unchanged runtime and artwork: 20 actual Game cases across five zones and
four resolution/safe-area profiles. The six character/ability checks in the
preceding material revision passed; that initial report also retains a layout
failure, subsequently fixed and superseded by this final passing layout matrix.

`Tools/Validate-Unity.ps1` completed with exit 0 using Unity 6000.3.19f1 before
merge. Its complete local log is
`C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-869094a6-7f92-4841-81a0-4a25774237fd.log`.
This approval handoff changes documentation only. No generation credits,
Android build, or real-device test were used.
