# ASSET EXTRACTION / RECONSTRUCTION PLAN

## This task is NOT “copy screenshot, paste into game”
Instead, reconstruct the mockup into a reusable asset set.

## Target asset list

### Main menu
1. Main title logo (or a very close production-ready recreation if needed)
2. Throne room background illustration
3. Purple banners
4. Pedestal / platform
5. Purple carpet / rug strip
6. Large primary menu button frame
7. Medium menu button frame
8. Small icon plates / icon holders
9. Gear/settings button frame
10. General UI jewel accents
11. Optional flame / torch sprite set if useful

### Pause/settings menu
1. Large modal frame
2. Modal top skull crest/header
3. Settings row module
4. Toggle ON module
5. Toggle OFF module
6. Resume button frame (green)
7. Neutral action button frame (gray)
8. Destructive action button frame (red)
9. Divider ornament / skull separator
10. Backdrop dimmer treatment

## Preferred implementation method
Where possible, build frames as reusable modular pieces:
- corners
- edges
- centers
- endcaps
- trims

This makes later scaling / layout changes cleaner.

## Shared “global UI” rules to lock during this pass
After this implementation, Astra should treat the following as the global non-zone-specific UI direction:
- general button construction
- gear/settings button style
- menu panel frame language
- metal/stone border language
- jewel accent language
- destructive / affirmative button color logic
- ability-button framing direction (to be aligned later against battle UI updates)

## Notes on text
Do not bake long strings into art unless project architecture truly requires it.
Prefer to bake:
- panel art,
- icon plates,
- decorative frames,
- ornaments,
while keeping most labels as text rendered by the game if practical.

If text rendering cannot match the reference well enough in-engine, document the issue and propose the safest production-ready fallback.
