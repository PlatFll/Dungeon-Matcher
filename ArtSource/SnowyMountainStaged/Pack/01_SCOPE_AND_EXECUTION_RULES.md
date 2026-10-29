# Scope and Execution Rules

## Goal
Create a game-ready snow mountain battle scene that feels like it belongs to the same finished game as the current:
- underwater zone
- enchanted forest zone
- ironvein cave / excavation zone
- dungeon zone

## Primary target
Match the exact scene language of `references/00_TARGET_SNOWY_MOUNTAIN_SCENE.png`:
- icy cliffside environment
- distant snowy mountains and bridge/path in the top battle background
- cold wood + iron + snow material mix
- board frame and portrait frame integrated into the zone
- snow piles, icicles, hanging lanterns, banners, rope fence posts, crystals, and alpine rock dressing
- bottom shared HUD kept structurally familiar to the rest of the game

## Required workflow
1. Read the pack.
2. Execute one stage at a time.
3. Generate assets with PixelLab using the stage prompt text.
4. If a result is weak, regenerate until it is strong enough.
5. Clean or recolor as needed after generation.
6. Assemble and test.
7. Stop at the end of the stage and show results.

## Important constraints
- Preserve the overall game readability.
- Keep the gem board clean and readable.
- Keep the shared UI family intact.
- The snow zone should feel colder, brighter, and more open than dungeon or cave, but still from the same game.
- The top HUD buttons (wave, guide, settings) must remain in-family with the other zones.
- No merge at the end.

## Generation policy
Astra may use:
- multiple PixelLab generations for one asset
- variant exploration within a stage
- large-piece generation when useful (for example: a full board-side environment strip, a full top background plate, or a full bottom environmental dressing piece)
