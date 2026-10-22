# Branch and Asset Rules

## Branch
Create and work on:
`feature/dungeon-zone-hybrid-pixellab-rebuild`

## Non-destructive rules
- Do not overwrite stable production assets blindly.
- Preserve existing approved art unless the new asset clearly replaces it.
- Do not merge automatically.
- Stop for review after each milestone.

## Asset rules
### Keep as-is or close to as-is
Use these as the stable structural language:
- clean side-view battleground composition
- clean wall tile logic
- cleaner floor/platform logic
- modular board-frame assembly approach
- modular portrait-frame assembly approach

### Generate with PixelLab
Use PixelLab for:
- main gate / archway
- premium top HUD elements
- portrait frame hero pieces if needed
- board frame hero pieces if needed
- torches
- chains
- banners
- barrels
- vases / jars
- skull piles / bone props
- small thematic hero props

### Do NOT ask PixelLab for
- a full assembled zone image as the production source
- a full large environment with all assets baked together
- giant backgrounds that need to be cut into production tiles afterward
- whole board + whole HUD + full environment in one output

## Cleanup rules in LibreSprite
For every generated asset:
- preserve silhouette and identity
- reduce over-detail if necessary
- remove blur/implied anti-aliasing
- fix broken pixels and jagged accidental noise
- normalize palette and shading to Dungeon Matcher style
- ensure clean readability at native scale

## Unity rules
Build an art-only validation scene that:
- loads the zone background and HUD pieces
- contains no gameplay logic changes unless required for art placement
- allows quick screen captures and seam checks
- allows asset swaps without damaging production gameplay
