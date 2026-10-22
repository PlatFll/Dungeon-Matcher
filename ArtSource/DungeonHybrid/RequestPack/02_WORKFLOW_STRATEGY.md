# Workflow Strategy

## Why the old workflow failed
The failing workflow kept asking the generator to solve too many problems at once:
- whole zone composition
- perspective consistency
- modularity
- tileability
- frame consistency
- prop placement
- text/UI consistency
- clean pixel treatment

That produces blur, inconsistencies, and backgrounds that feel “almost right” but not production ready.

## New workflow
Split the problem into 3 layers:

### Layer 1 — Stable modular tiles
Build or preserve these in a simpler, hand-cleanable form:
- wall tiles
- floor/platform tiles
- support pillars / vertical side tiles
- plain backing tiles

### Layer 2 — Hero architecture
Generate/upgrade selectively:
- gate / archway
- premium HUD architecture
- premium frame pieces

### Layer 3 — Dressing / props
Generate individually:
- torches
- chains
- banners
- barrels
- vases / jars
- bones / skulls
- small environmental props

## Tool assignment
### PixelLab best uses
- hero props
- premium asset pieces
- focal dungeon architecture
- visual variants and upgrades

### LibreSprite best uses
- palette cleanup
- native-pixel cleanup
- tile extraction
- seam repair
- shading simplification
- color normalization
- spritesheet preparation

### Unity best uses
- layout assembly
- seam tests
- readability validation
- scale checks
- comparison captures
