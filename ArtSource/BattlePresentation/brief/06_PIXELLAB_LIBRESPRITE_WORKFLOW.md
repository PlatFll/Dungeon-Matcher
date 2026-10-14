# PixelLab + LibreSprite Workflow

## Goal
Use the included mockups as reference targets, but produce clean native production assets.

## Step 1 — Analyze each reference
For each of the 3 zone targets:
- identify reusable frame modules,
- identify one-off decorative pieces,
- identify moving ambient elements,
- identify which parts are shared UI vs zone-specific.

## Step 2 — Reconstruct modular pieces
Suggested families:
- player panel frame pieces
- board border pieces
- enemy-area/top framing pieces
- wave plaque pieces
- zone ornamental pieces
- shared button holder pieces (same across zones)
- shared bottom ability panel pieces (same across zones)

## Step 3 — Use PixelLab selectively
Use PixelLab where useful to help reconstruct:
- background chunks
- ornamental frame parts
- plaques
- decorative props
- zone environment details

But Astra must review and not trust raw outputs blindly.

## Step 4 — Clean in LibreSprite
Every asset must be finalized in LibreSprite for:
- pixel-grid alignment
- cleanup of blur / AA artifacts
- cleaner ramps / clusters
- edge cleanup
- seamless modular joins
- animation frame cleanup where needed

## Step 5 — Integrate in Unity
Verify:
- point filtering / nearest neighbor
- no interpolation blur
- no seam bugs
- stable pivots / anchors
- animation loops feel alive but restrained

## Step 6 — Compare against target
Produce actual in-engine side-by-side comparisons.
