# MASTER CONTRACT — Main Menu + Pause UI Pass

You are working inside the Dungeon Matcher project.

You must first read:
- `Dungeon_Matcher_Art_Direction(2).txt`
- inspect `Dungeon_Matcher_Art_References(2).png`

Then use these two images as the exact visual target for this task:
- Main menu reference: `/mnt/data/ghostwriter_images/generated/a_vertically_oriented_pixel_art_game_title_screen_5_batch_4.png`
- Pause/settings reference: `/mnt/data/ghostwriter_images/generated/a_pixel_art_video_game_screenshot_dark_fantasy_du_6_batch_5.png`

## Objective
Recreate and implement the **main menu UI** and **pause/settings menu UI** so they match the provided mockups as closely as possible while remaining clean native pixel art appropriate for the live game.

## Non-negotiables
- Preserve crisp native pixels.
- No blurry or anti-aliased scaling.
- No “close enough” approximations if the reference clearly shows the intended structure.
- Build production-ready assets, not painted-over screenshots.
- Match the UI look: chunky stone/metal frames, purple jewel accents, readable contrast, strong silhouettes, centered composition, clean spacing, and polished pixel-art lighting.
- Ensure the result looks intentional, premium, and game-ready.

## Implementation philosophy
Treat the references as a **target sheet**, not as a full UI atlas.
Astra should:
1. Analyze the mockups.
2. Identify repeated modules.
3. Reconstruct them as reusable pieces.
4. Finalize them in LibreSprite.
5. Integrate them into Unity.
6. Verify that the in-game result matches the mockups.

## Hard rule: do not brute-force the screenshot
Do NOT simply place the full screenshots in the game.
Do NOT rely on scaling the screenshots down or up.
Do NOT preserve any hidden compression or interpolation noise.
Instead:
- derive reusable slices,
- rebuild missing edges/corners when necessary,
- clean everything to native pixel standards.
