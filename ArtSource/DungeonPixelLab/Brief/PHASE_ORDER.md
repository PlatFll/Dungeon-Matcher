# Phase order — give Astra ONE prompt at a time

00. **Setup and pixel pipeline** — `PROMPTS/00_SETUP_AND_PIXEL_PIPELINE.txt` — No paid calls
01. **Battle wall tiles** — `PROMPTS/01_GENERATE_DUNGEON_WALL_TILES.txt` — PixelLab generation
02. **Portrait recess material** — `PROMPTS/02_GENERATE_SEPARATE_PORTRAIT_RECESS.txt` — PixelLab generation
03. **Flat 2D floor top** — `PROMPTS/03_GENERATE_FLAT_SIDESCROLLER_FLOOR.txt` — PixelLab sidescroller tileset
04. **Raised platform front fascia** — `PROMPTS/04_GENERATE_FRONT_PLATFORM_FASCIA.txt` — Existing seed or narrow PixelLab generation
05. **Gate and arch** — `PROMPTS/05_GENERATE_GATE_ARCH_DOOR.txt` — PixelLab object/image
06. **Pillars and supports** — `PROMPTS/06_GENERATE_PILLARS_SUPPORT_STONE.txt` — PixelLab object/image
07. **Torch and warm light** — `PROMPTS/07_GENERATE_TORCHES_AND_LIGHT.txt` — PixelLab object/image
08. **Chains and banners** — `PROMPTS/08_GENERATE_CHAINS_AND_BANNERS.txt` — PixelLab objects
09. **Barrels, skulls, bones** — `PROMPTS/09_GENERATE_BARRELS_SKULLS_BONES.txt` — PixelLab objects
10. **Player portrait frame** — `PROMPTS/10_GENERATE_PLAYER_FRAME_KIT.txt` — PixelLab ornament plus LibreSprite repeat
11. **Board frame and empty cells** — `PROMPTS/11_GENERATE_BOARD_FRAME_AND_CELLS.txt` — PixelLab corner/cell seed
12. **Top Wave/Guide/gear frames** — `PROMPTS/12_GENERATE_TOP_HUD_UI_PLAQUES.txt` — PixelLab UI seed
13. **Bottom resonance HUD** — `PROMPTS/13_GENERATE_BOTTOM_HUD_RESONANCE.txt` — PixelLab UI seed
14. **Unity art-only assembly** — `PROMPTS/14_ASSEMBLE_ART_ONLY_UNITY_SCENE.txt` — No generation unless explicitly authorized
15. **QA and draft PR handoff** — `PROMPTS/15_FINAL_PIXEL_QA_AND_HANDOFF.txt` — Bounded repair
16. **Optional integration after approval** — `PROMPTS/16_OPTIONAL_AFTER_USER_APPROVAL_INTEGRATE.txt` — User approval required

Each stage stops before the next. Stage 16 is OPTIONAL and requires separate user approval.
