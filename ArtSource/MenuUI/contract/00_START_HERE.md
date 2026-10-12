# Dungeon Matcher — Main Menu + Pause UI Implementation Pack

## Goal
Use the two approved mockups as the **visual target** for the first UI-art polish pass:
1. **Main Menu mockup**
2. **Pause / Settings menu mockup**

This pack is for Astra to:
- study the two reference images,
- extract their UI language,
- recreate the art as **native pixel assets**,
- integrate them cleanly into the game using **PixelLab + LibreSprite + Unity**,
- avoid goofy, blurry, stretched, or off-model results.

## Why start here
Yes — start with **main menu** and **pause/settings menu** first.

Reason:
- They define the **global UI style**.
- They establish the shared look of **buttons, frames, stone/metal borders, purple jewel accents, and typography layout**.
- They also define the **general non-zone-specific UI** that should stay consistent across the game.
- After that is locked, Astra can rework the zone UIs and battle screens to match a stable shared design system.

## Reference images to use
- Main menu reference: `/mnt/data/ghostwriter_images/generated/a_vertically_oriented_pixel_art_game_title_screen_5_batch_4.png`
- Pause/settings reference: `/mnt/data/ghostwriter_images/generated/a_pixel_art_video_game_screenshot_dark_fantasy_du_6_batch_5.png`

## Required workflow
1. Read the project's art direction files first.
2. Use the two mockups as the **exact look target**.
3. Rebuild the UI art as **clean native assets**, do **not** just crop a full screenshot and stretch it into production.
4. Use **PixelLab** where useful for reference-driven art generation / reconstruction.
5. Use **LibreSprite** to clean, simplify, align, unify, and finalize native pixel assets.
6. Integrate carefully in Unity.
7. Validate at actual in-game scale.

## Deliverables from this pack
- Main menu UI art implemented.
- Pause/settings menu UI art implemented.
- Shared general UI button style documented and applied.
- Shared settings / gear button style documented and applied.
- Shared large central action button / ability-button styling documented for future zone work.
- Validation screenshots comparing implementation vs reference.

## Important guardrails
- Do not redesign the concept.
- Do not make the result “inspired by” the mockups only loosely.
- Match the mockups closely while keeping the assets crisp, native, and production-ready.
- No blurry scaling, no resampling artifacts, no mixed pixel densities, no accidental smoothing.
