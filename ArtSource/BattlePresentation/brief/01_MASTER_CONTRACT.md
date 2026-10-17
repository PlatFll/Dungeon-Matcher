# MASTER CONTRACT — Three Zone Battle Presentation Pass

Read first:
- `Dungeon_Matcher_Art_Direction(2).txt`
- inspect `Dungeon_Matcher_Art_References(2).png`
- any current canonical art/UI documentation in the repo

Then use the included 3 reference images in `references/` as the exact visual targets for this task.

## Objective
Rebuild / implement the battle presentation for the 3 target zones so they match the supplied mockups closely while preserving clean native pixel-art standards and keeping the shared bottom HUD and shared button language consistent.

## Core instruction
The final result should feel like:
- one cohesive game
- one shared UI system
- three distinct themed battle spaces

## Shared UI lock
Across Dungeon / Forest / Drowned Court, keep the following visually consistent as one family:
- guide button frame
- settings button frame
- potion holder button frame
- bomb holder button frame
- center ability / action panel frame
- general bottom HUD construction
- resonance text strip styling if shared by current implementation

Do NOT let those drift into three separate styles.

## Zone-specific lock
Change zone-specific art to match the included references:
- dungeon stone / purple dungeon trim for Dungeon
- carved wood / greenery / magical forest trim for Forest
- submerged stone / coral / sea-growth / aquatic trim for Drowned Court

## Reconstruction rule
Do NOT paste a whole screenshot into the game.
Do NOT scale screenshots and call it done.
Instead:
1. study the references,
2. identify reusable modules,
3. reconstruct native assets,
4. clean in LibreSprite,
5. use PixelLab where helpful for reference-grounded reconstruction,
6. integrate cleanly in Unity.

## Production quality rules
- no blur
- no anti-aliased scaling artifacts
- no mixed pixel densities
- no noisy over-detail
- no visible seam bugs
- no stretched corners
- no off-style improvisation

## Scope guardrail
This task is about battle presentation and ambience only.
Do not modify characters, enemies, gems, or gameplay systems unless needed only for attaching ambient visual elements safely.
