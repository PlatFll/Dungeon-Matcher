# Visual reference breakdown and production materials

## What is in the user mockup

**Top scene**: deep cyan light shafts, gradually fading ocean depths, colossal drowned monarch statue holding trident, broken temple pillars and leaning ancient columns, stone arch through court, distant fish silhouettes and bubbles; focal three enemy positions on a restrained ancient walkway.

**Left player portrait**: cool stone and iron frame with darker submerged inset backing, purple skull banner as mockup decoration, emerald affinity gem location, coral, algae/kelp and barnacle growth attached at exterior edges. Keep actor/gem/HP/overlay systems live and unchanged.

**Board**: 8x8 dark cells with bright six gem families. Outer frame/side architecture is old teal-blue carved stone, chipped corners and bolts, sparse marine growth and bubble trails in the *gutters*. Do not modify gems, cells or interaction hits.

**Lower screen**: underwater blue stone, raised reef stones, pink coral clusters, green kelp tendrils, starfish, shells/barnacles and old submerged skull/skeletal relic at lower right. These form two large coherent anchored lower corner masses that frame, but DO NOT replace or obscure, shared resonance/potion/bomb panels.

## Material direction (start from current game's Court ramps; these are ROLES not invented exact colors)
- Deep water recess: near-black indigo/blue-green, dominant quiet background.
- Distant water: muted cyan/blue haze through large, simple planes, no full-screen soft shader tint on gems.
- Ancient carved stone: dark blue slate shadows, medium turquoise-teal base, small upper-left pale-blue bevels; more architectural than organic.
- Corals: muted coral-pink/magenta with sparse warm lit tips; selective focal patches outside board.
- Kelp/algae: deep bottle green shadow, mid moss/teal body, occasional green-lit edges; strands have visible holdfasts.
- Metals: dull silver/blue-gray iron and sparing gold/bronze detail; preserve global UI frame family.
- Barnacles/shells: warm pale ivory/desaturated sand, tightly grouped to rocks, no white confetti noise.
- Bubbles and bioluminescence: small cobalt/icy-cyan clusters and hollow arcs (limited ring sizes), no blurry HUD overlays.

## Relative density
1. **Quietest**: behind characters, portrait interior, inside gem grid, resonance text/controls.
2. **Medium**: underwater wall courses, shadowed temple columns, board exterior frame.
3. **Richest**: distant trident statue silhouette, occasional coral pockets, lower corners and outside-board ruins.

## Reference crops and asset requests
Crops in `reference_crops/` exist to keep Astra focused (top stage, central ruins, player portrait, board sides, bottom corners). They are **screen-resolution art references only**, not native sprites. Obtain valid source-pixel references before feeding to PixelLab; use appropriate supported reference inputs. PixelLab docs recommend unzoom when using enlarged pixel art; verify against original to avoid altering shapes.

## Shared reference rules
- Current dungeon screenshot = quality/geometry benchmark, not a purple palette to copy to Court.
- Current forest screenshot = coherent side-dressing benchmark, not roots/leaves aesthetic to transplant into underwater ruins.
- Historical static art board = supplementary only; current repository golden files are authoritative.
- User mockup = desired imagery/material/composition, not literal runtime layout or character design.
