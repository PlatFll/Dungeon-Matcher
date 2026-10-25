# Global contracts — every stage

## 1. Protect the unified game
**LOCKED:** `Game` gameplay rules and run state, board grid sizes/pitches/cells/gem sprites, all characters/enemies/animations, HP/shield/weakness/AIR overlays, attacks and response clock, Wave/Guide/settings/button logic, global bottom resonance HUD and potion/bomb shells, font and text APIs, other zones and menu/settings screens. The Drowned Court mockup depicts unique characters and stylized labels—those are *not* new character specifications or permission to change gameplay.

**ALLOWED:** Drowned Court/underwater environment tilemaps/visuals, architectural elements, zone-specific portrait frame shell/backing, zone-specific *outer* board frame/surround and environmental gutters, lower side scenery behind shared HUD, modular props, sparse animated ambient effects. Make all art additive/candidate until approved.

## 2. Drowned Court identity
Sunken Marches / Drowned Court: ancient ruined underwater royal court, luminous blue-green depth, distant trident-bearing monarch statue, old carved stone arches, columns with coral and kelp rooted in crevices, restrained shell and barnacle accretions, tiny blue bubbles and pale light shafts. Combatants are in a clear court floor lane, without a distracting layer of water over their faces or gems. Distant fish silhouettes are optional and dim; this is not a reef aquarium, pirate tavern, or generic mermaid palace.

## 3. Shared visual language
Crisp native source pixels, 64 PPU where current repo architecture requires it, no blur/interpolation, readable silhouettes, restrained top-left material shading, dark contiguous outlines and purposeful connected clusters. Reuse exact current Court UI/stone ramps where useful; create material-specific blue stone, teal water, muted coral pink, green kelp and bone/shell accents. **Do not** impose a single 15-color character palette on environment. Let gems remain brighter than sea life.

Do not force giant 512x512 images for small icons or tiles. Use exact native source scale and Unity's integer pixel mapping; avoid independently stretching x/y. Transparent standalone props must be isolated and have appropriate source alpha. Keep original reference bytes and compare edited source/export pairs.

## 4. Architectural integration, not stickers
Barnacles attach to joints and ledges, coral stems grow from cracks/rock pockets, seaweed roots enter gravel or mortar, skeleton/skull props rest on surfaces, bubbles emerge from vents/crevices. Avoid two unconnected stone islands stacked vertically, props that float, repeating obvious copy-pasted reef clusters, random 'coral wallpaper', broken modular joins, and coral encroaching on the playable gems.

## 5. Camera and UI
Game is a portrait mobile match-3 battle. Background uses 2D frontal/side-view ruins with a readable horizontal enemy-standing ledge; **not isometric floor tiles or receding floor grids**. Keep zone board outer geometry exactly lined up with actual 8x8 grid. Character portrait interior is a distinct quiet recessed underwater stone field clipped inside the frame. Shared top controls and bottom UI remain physically unchanged; environment only fills behind/around them.

The underwater mockup top sign says 'DROWNED COURT'; do not replace the functional `WAVE N` live widget. Only introduce a separate zone title if it is already an existing game presentation behavior; no new functional UI.

## 6. Non-destructive implementation
Start from current repo main and preserve current user dirty files/worktrees. Build on a new topic branch `feature/drowned-court-pixellab-environment-v2` or report chosen unique name. Do not overwrite existing approved raw assets/GUIDs. Keep raw PixelLab study separately, selected editable LibreSprite sources, exports, catalog, previews and QA. Do not modify `Game.unity` or build settings in art-only stages; no merge until explicit approval.

## 7. Budget/authorization discipline
PixelLab spending is allowed within the active user-authorized stage and available plan, but no assumption of unlimited credits. In Stage 00 query balance and tool costs. At each stage quote planned jobs, record real usage and accepted/rejected results. Spend on quality improvements where needed; avoid redundant generation and re-prompting for outputs that can be fixed precisely in LibreSprite. If credit cost is unexpectedly large or authorization is unclear, report before incurring it. Do not claim success from a 90% algorithmic 'style score'.

## 8. Stage completion rule
One focused stage per user authorization. Show actual selected source/native 1x, nearest-neighbor enlargement, assembled Unity screenshot, before/after and pass/fail technical checks; mark artistic judgment separately. Stop for review after each stage. Only final accepted assets enter optional production integration.
