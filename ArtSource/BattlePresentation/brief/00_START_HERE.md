# Dungeon Matcher — Three Zone Battle UI + Background Implementation Pack

## Goal
Implement the **battle presentation** for these 3 zones so they match their approved mockup targets:
1. Dungeon zone
2. Forest zone
3. Drowned Court / underwater zone

This pass is focused on:
- zone-specific battle backgrounds
- zone-specific battle frames and ornamentation
- top HUD environment framing / plaques
- board frame / player panel frame / enemy-area frame appearance
- shared bottom HUD + shared general top-right UI consistency
- environmental motion / life in each zone

## Important global rule
The **shared general UI and bottom HUD** must remain consistent across all 3 zones, matching the approved design language already established:
- guide button style
- settings button style
- bottom potion / bomb holders
- center ability / action panel styling
- general bottom HUD construction
- general frame material logic for shared non-zone-specific controls

Those should stay **the same family** across all three.

## What changes by zone
Zone-specific elements should change per mockup:
- background art
- zone frame materials / trim / ornamentation
- board border theming
- player side panel theming
- top wave plaque theming
- side decorations / props / banners / foliage / underwater growth
- zone-specific ambient animation and life

## Do NOT change
- character sprites
- enemy sprites
- gem sprites
- gem colors
- gameplay logic
- board contents
- character identities

## Approved visual targets included
See `references/`:
- `DUNGEON_ZONE_TARGET.png`
- `FOREST_ZONE_TARGET.png`
- `DROWNED_COURT_TARGET.png`

## Special zone motion notes from user
### Dungeon
- torches should move / sparkle and feel lit
- reduce the number of torches in the **top HUD area** versus earlier over-busy executions
- maintain the dramatic purple dungeon atmosphere

### Forest
- blue engravings on the wood should glow a bit
- lantern should sway back and forth when wind blows every now and then
- leaves around the board frame should move subtly
- waterfall in the background should visibly move so it feels alive

### Drowned Court
- bubbles should move upward in the background, pop, then reappear
- underwater vegetation should sway back and forth subtly
- overall scene should feel alive underwater

## Deliverables
- implemented dungeon battle presentation
- implemented forest battle presentation
- implemented underwater battle presentation
- shared bottom HUD / top-right shared UI preserved consistently
- ambient animation integrated cleanly
- before/after screenshots + comparison views + report
