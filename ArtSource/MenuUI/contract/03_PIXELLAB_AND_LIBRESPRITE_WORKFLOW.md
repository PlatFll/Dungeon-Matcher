# PIXELLAB + LIBRESPRITE WORKFLOW

## Goal
Use PixelLab and LibreSprite in a way that produces **native clean pixel assets**, not messy approximations.

## Recommended workflow

### Step 1 — Analyze the mockups
For each reference:
- identify repeated UI modules,
- identify likely reusable pieces,
- identify one-off decorative pieces,
- note spacing, padding, and composition relationships.

### Step 2 — Break the design into modules
Astra should derive / reconstruct the following asset families.

#### Family A — shared structural pieces
- stone/metal panel corners
- horizontal panel segments
- vertical panel segments
- inner dark panel fill textures
- jewel studs / corner gems
- skull crests / icon plates

#### Family B — menu buttons
- large primary button frame
- medium secondary button frame
- icon plate area
- label area
- pressed / highlighted / hover variants if needed by project conventions

#### Family C — settings modal
- large overlay frame
- top header plaque
- settings row module
- green ON toggle module
- gray OFF toggle module
- green action button variant
- gray action button variant
- red destructive button variant

#### Family D — decorative environment art for main menu
- throne room background elements
- purple banners
- torches / braziers / flames
- archway / skull portal backdrop
- pedestal / rug
- treasure / bones / props where needed

## Step 3 — Use PixelLab correctly
PixelLab may be used to:
- generate clean candidate pieces matching the reference look,
- help reconstruct repeated ornamental elements,
- help redraw background chunks or UI chunks,
- provide clean style-consistent modular art.

But Astra must NOT blindly trust the first output.
Every generated piece must be reviewed.

## Step 4 — Final cleanup in LibreSprite
Every asset must be finalized with pixel hygiene:
- align to pixel grid,
- remove accidental blur,
- remove anti-aliased fringe,
- clean clusters,
- simplify noisy ramps,
- unify outline weight,
- unify highlight logic,
- ensure corners, seams, and tiling edges are crisp.

## Step 5 — In-engine integration
When slices are imported into Unity:
- make sure sprite import settings preserve nearest-neighbor appearance,
- disable filtering that causes blur,
- verify pivot / slicing / anchors,
- assemble panels correctly,
- verify scale in the actual target resolution,
- verify the overlay background dimmer behaves correctly.

## Step 6 — Validate against reference
Take side-by-side screenshots and compare:
- frame thickness,
- border rhythm,
- corner design,
- text spacing,
- button hierarchy,
- environment composition,
- clarity at actual gameplay size.

## Important warning
If a piece cannot be recreated cleanly through modular art, Astra should stop and explicitly note the blocker rather than forcing a bad-looking solution.
