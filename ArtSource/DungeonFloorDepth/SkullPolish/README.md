# Dungeon ground skull — final production polish

User authorized making the ground skull smaller and more muted, verifying the actual Dungeon zone, then merging PR #222.

LibreSprite resizes the visible skull from 27×26 to 21×20 native pixels (approximately 23% smaller in each dimension), within the unchanged 31×30 canvas. The bottom occupied row remains 27. Its six bright warm bone shades become a quieter gray-violet ramp; brightest RGB changes from 213/203/181 to 151/146/153. A narrower native contact shadow fits the new footprint. Alpha stays binary, export/reopen is exact, and Unity draws the result at the same integer native pixel scale.

The original Stage04 skull, character drawings, golden references and all other art are preserved. Production DungeonPixelLab.asset now binds the new Skull.png through its existing hybrid module list. No scene-only overlay or alternate preview system is used. Source, before-art, ASE files, exports, roundtrip checks and actual Game captures are retained here. PixelLab spending: zero.

## Verification

Actual Game-scene tests passed at 720×1280, 1080×1920, 1080×2400, inset safe-area 1080×2400 and 360×640, plus two underwater reference profiles. The test asserts the live skull's production asset path, captures frozen old/new skull renders, and finds zero changed pixels outside the skull/contact area at every Dungeon size. Full-width floor/edge gap checks, unchanged portrait and UI/actor/board geometry, accepted swap and Settings checks also pass.

Tools/Validate-Unity.ps1 passed on the final source; its receipt is in QA. No Android build or physical-device test is claimed.

[Final Unity comparison](Preview/Review.html) · [Native comparison](Preview/NativeComparison.png)
