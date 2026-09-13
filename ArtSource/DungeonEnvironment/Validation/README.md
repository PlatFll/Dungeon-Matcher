# Final environment validation — 2026-09-13

- Actual `Game.unity` Play Mode visually inspected at 1080 x 1920 and 1080 x 2400. The final screenshots show RattleBones with the revised walkable platform and a settled board.
- The platform's deeper flagstone surface resolves the visible separation between feet and the original narrow ledge through artwork and authored offsets. Characters stand at different depths on the plane at the two viewport heights.
- Both views preserve character/gem readability, separation from the purple frames, sharp pixels, sparse scenery, continuous masonry and clipping within the battle frame. Runtime reports found one active environment, with legacy Tilemap renderers suppressed.
- `powershell -ExecutionPolicy Bypass -File Tools/Validate-Unity.ps1`: **PASS**, Unity 6000.3.19f1. Completed successfully after the temporary editor utility was removed.
- `BattleBackgroundViewportValidation.Run` in an isolated graphics-enabled batch editor: **PASS**, exit 0. Log explicitly reports both 1080 x 1920 and 1080 x 2400 passed. This checks the existing technical mask/placement assumptions separately from visual inspection of the real scene.
- `Art_Verification.json` records all floor/foundation variant pairings, palette/alpha checks, source layer names and preservation of the approved wall geometry under its cluster recolor.
- Complete prefab review found changes only in the three newly painted Tilemaps and the authored Floor/Architecture Y offsets. The BackWall map, Grid, renderers, sorting, source masks and runtime components are unchanged.

No gameplay or runtime code changed. The original character selection was restored after testing.
