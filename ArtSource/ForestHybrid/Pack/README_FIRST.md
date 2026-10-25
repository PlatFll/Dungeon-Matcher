# Astra Forest Zone Rebuild Pack

This pack tells Astra how to rebuild the **Magical Forest zone** for Dungeon Matcher using the existing finished dungeon scene as the implementation baseline and the forest mockup as the visual target.

## Goal
Build the forest zone backgrounds and frames so the forest area feels like the dungeon-zone quality level and UI structure, while matching the magical forest theme from the reference mockup.

## Core direction
- **Reference 1 (`01_forest_mockup_reference.png`)** = visual target for the forest zone theme, mood, framing language, and forest-specific art ideas.
- **Reference 2 (`02_current_dungeon_zone_reference.png`)** = implementation-quality benchmark and structural baseline. This is the current in-game dungeon scene whose readability, assembly discipline, and UI consistency should be matched.

## Important high-level rule
The **bottom HUD, settings screen, and global buttons remain unified across all zones**.
Do **not** redesign or replace the unified bottom HUD, settings, or global shared buttons for this task.

## What should change for the Forest zone
Only the **zone-specific art** should be built/reworked:
- top battle background / combat backdrop
- top HUD background environment behind enemies
- player frame zone treatment
- board frame zone treatment
- surrounding environmental side dressing around the board
- forest props, flora, lanterns, hanging elements, mushrooms, carved wood, runes, roots/vines, etc.

## What should stay the same globally
- bottom HUD layout and assets
- resonance HUD panel
- potion button shell
- bomb button shell
- global settings/menu button family
- overall gameplay layout proportions unless explicitly needed for forest assembly

## Process requirement
Astra should execute the staged prompts one-by-one, stopping after each stage for review.
