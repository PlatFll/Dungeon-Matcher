# Dungeon standing-floor depth

The user requested a wider Dungeon standing surface like Drowned Court. The
previous surface was one 12-pixel course. Two staggered rear courses now use the
same approved native 32×12 stones, producing 36 pixels of depth. The new rows
render behind the existing architecture and only inside the enemy-side floor
region. The original front course, separator, actor soles, health bars, portrait,
board and controls remain fixed. No source PNG, palette, importer, manifest,
scene, gameplay rule or other zone is changed. No generation credits were used.

The production owner is `DungeonPresentationArt.BuildHybridEnvironment`, used by
the enabled Game scene. [Actual Unity comparison](Preview/Review.html).
This is a review candidate on `codex/dungeon-deeper-battle-floor`, not merged.

Validation: actual Game scene with Rattlebones, three enemies and 64 gems.
Dungeon profiles: 720×1280, 1080×1920, 1080×2400, 1080×2400 with safe-area
insets, and 360×640. Drowned Court reference captures: 720×1280 and the safe-area
profile. Native scale, connected floor courses, shared feet, layout, one accepted
swap and Settings passed. Frozen before/after screenshots show zero changed
pixels outside the added floor and zero changes inside the portrait mask in all
five Dungeon profiles. UI/board geometry remains exact. All original floor and
golden-reference hashes are preserved in QA/References.json. Nine protected local
files were preserved byte-for-byte after Unity.

The first capture exposed a portrait-edge sliver and an unsettled unscaled
button fade. The added floor is now masked to the enemy region; the fixture
waits for the existing control transition to finish before exact comparisons.
No tolerance was added to the pixel checks.

No Android build or physical-device test. The pre-existing compact camera
reference-resolution warning remains outside this floor-only correction.

The final `Tools/Validate-Unity.ps1` compilation gate passed with Unity
6000.3.19f1. Full local validator log: `C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-66706d65-37af-4173-abf5-3412a95f7e54.log`.
