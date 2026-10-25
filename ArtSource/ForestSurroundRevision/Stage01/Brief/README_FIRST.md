# Astra Forest Adjustment Pack — Keep Top HUD, Adjust the Rest

This pack is for Astra to revise the **magical forest zone** while preserving the parts that are already working well.

## Core instruction
**Do not touch the top HUD / upper battle section.**
Use the current Astra-generated forest screen as the base. Preserve the upper section, and revise only the board surroundings and lower HUD surroundings so they move closer to the target mockup.

## Reference files
- `references/01_current_astra_forest_generation.png` = current in-game-looking Astra result.
- `references/02_target_mockup_forest.png` = target mood / target direction for the board surroundings and lower decorative environment.

## What is locked
Everything in the upper section is considered locked unless a tiny seam correction is absolutely required. See `02_LOCKED_REGION_RULES.md`.

## What should change
Main changes are:
- side surroundings around the board
- lower left and lower right surroundings around the bottom HUD
- decorative balance: thicker roots, fewer leafy masses, more mushrooms / wisps / flowers / stone anchors

## Working order
1. Read `00_STARTER_PROMPT.txt`
2. Read `01_SCOPE_AND_BRANCH_RULES.md`
3. Read `02_LOCKED_REGION_RULES.md`
4. Follow stages 1 through 4 in order
5. Stop at review points before any production merge
