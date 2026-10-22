# Dungeon masonry alignment

The bottom corners previously used a physical-bottom anchor while the side tiles repeated from the combat floor and stayed inset with the gameplay viewport. That produced mismatched mortar rows and side strips. Corner sprites also overlapped the lower side tiles.

The existing presentation owner now lays out side stones on one 28px repeat aligned to the physical bottom. Each source contains two 14px courses. Left/right columns attach to the physical display edges, independent of safe-area UI. A local mask clips only the first partial architectural course where it meets the floor; no source pixels are stretched or recolored. Corner padding remains outside the display. In the editor review, the corners replace exactly three lower repeats on each side and join the upper row at the same boundary.

All approved artwork, source images, palette colors, board geometry, controls, actors and gameplay remain unchanged. No generations were used. The new corner additions remain review-only and are not selected in production resources. Nothing is merged.

## Verification

Three current graphics-enabled Unity fixtures pass: production Dungeon combat, candidate masonry combat, and the existing Dungeon/Forest/Court presentation regression. Each Dungeon view covers six screen/safe-area profiles, including left/right insets, and contains Rattlebones, three live enemies, gems, health/intents and live controls. Existing board/health layout is checked against its baseline.

Unity assertions check physical edge contact, consistent 28px repeat, contiguous non-overlapping rows, exact corner join and removal of precisely the six replaced tiles. Full-screen camera-clear detection remains enabled. The independent native-pixel check compares every opaque stone pixel along the board/HUD edges and corner silhouette with unchanged source pixels, allowing only one byte of known GPU channel rounding. Diagnostics/PixelJoins.json records its result and preservation audit. Required compiler validation is recorded separately.

Previews are actual Game captures; LibreSprite only crops the close-ups without resizing or editing artwork. This is not a physical-phone test, and no new Android build was performed.

Final results: native pixel comparisons passed for 1,895,252 opaque pixels across all 12 captures. All 5,298 protected files and eight pre-existing main-checkout edits remain unchanged. `Tools/Validate-Unity.ps1` passed after the final code changes (Unity 6000.3.19f1, exit 0).
