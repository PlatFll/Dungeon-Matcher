# Validation — colored user-edited ability icons

- Native artwork: all three 48×40 canvases, exact edited alpha masks and occupied bounds preserved. Binary alpha; 7/4/4 meaningful opaque colors.
- LibreSprite: saved colored ASEs reopened to byte-identical decoded RGBA exports. Original saved user ASEs remain unchanged.
- Source integrity: checked 9207 pre-existing Assets/ProjectSettings/Packages files. Only the three ability definitions changed (icon and ready-icon GUIDs). All five user-supplied silver controls and all character/environment/gem art remain exact.
- Unity 6000.3.19f1 compilation: Tools/Validate-Unity.ps1 passed, exit 0. Log retained as UnityCompile.log.
- Actual Game scene: existing SilverRoyalRelicProductionTests passed. Five zones at 360×640, 720×1280, 1080×1920, and 1080×2400 with simulated safe area, plus the three recolored characters at 720×1280. Centering, native scale, margins, live wave label, board clearance, Settings, supplies, accepted ability activation, ready glow/lift, pause and Reduced Motion checked.
- Result: C:/Users/USER/.codex/worktrees/menu-ali-edited-background/Dungeon Matcher/.utmp/ThreeZonePresentation/reference-tests-20261112-021250.xml; copied as UnityTests.xml. This is one aggregate Unity test with 23 layout cases and 8 character/zone ready-state cases, not 31 separately discovered tests.
- Screenshots: Preview/Gameplay/ contains fresh actual Game renders; GameplayCharacters.png is a nearest-neighbor contact sheet. Original production gallery evidence restored byte-for-byte after collection.
- PixelLab generations: 0. No Android build or physical-device test was run for this palette-only revision.
