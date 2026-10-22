# Forest Stage 4 — secondary props

Status: complete, awaiting user visual approval. Stop before Stage 5. No merge.

The approved forest needed small warm and magical focal accents, not more foliage.
Three separate PixelLab-derived props were cleaned at their original pixel scale in LibreSprite.
The existing Motion/Lantern and Motion/Rune were inspected and retained unchanged as historical production assets; their busier metal/high-glow treatment was not imported into this quieter wood family.

| Native module | Canvas | Opaque colors |
| --- | --- | --- |
| Lantern_Hanging | 34×66 | 9 |
| Mushroom_Cluster | 35×35 | 10 |
| Rune_Plaque | 24×39 | 7 |

Lanterns sit above the cast and on outer board posts. Mushrooms sit at ground edges and post feet. One dim rune decorates the right tree and another the left board support. See Placements.json for exact native coordinates. No added flowers, signposts, extra vines or variants were needed.

## What changed

- New ArtSource/ForestHybrid/Stage04 source, native documents, scripts, receipts and review evidence.
- New Assets/_Game/Art/ForestHybrid/Stage04 PNGs and Point/FullRect/64 PPU/uncompressed imports.
- Two independent Stage04 art-preview scenes load copies of the saved approved studies and add separate prop Images. The saved original scenes are never rebuilt or overwritten.
- One explicit editor test fixture for actual Unity captures and prop geometry.

Lantern highlights were consolidated; the mushroom caps retain their squat shape with quieter color/foliage clusters. The plaque uses one connected dim rune. No source resizing, interpolation or semi-transparent glow.

## Verified

- Unity editor art-preview test: 1 passed, 0 failed. UnityTests.xml records the run.
- Upper captures: 470×288, 940×576, 1410×864. Board: 608×608, 1216×1216, 1824×1824.
- 2× and 3× images equal exact nearest-neighbor enlargements of the native captures.
- Props-hidden captures equal prior approved Stage02/Stage03 screenshots pixel-for-pixel.
- Independent PNG compositing reproduces every pixel of both props-on Unity screenshots. All prop canvases lie inside the viewport; no crop or tint.
- Portrait interior, both cast reference rectangles and all 512×512 board pixels are unchanged.
- Binary alpha, zero hidden RGB, two-pixel transparent perimeter, native plan/export equality, and reopened LibreSprite document/export equality pass for all three assets.
- 1830 protected file hashes unchanged, including approved Stages 1–3, game assets, gems, golden references, shared UI and production bindings.
- Tools/Validate-Unity.ps1 result is recorded separately in Diagnostics/RepositoryValidation.txt after completion.

This is art-only Unity rendering verification. No actual combat, Android build, phone, adaptive production layout or Stage 5 full-zone assembly was tested here. No runtime gameplay/presentation owner was changed.

## Spending and lineage

One Pro Flash 96×96 generation; job f94be040-e170-4c7f-87c6-23986cbfc42d, saved asset 35da8e27-7d7e-5925-bc81-ecd333b077b6; **5 generations charged**, **26 cumulative Forest generations**. A palette-name validation rejection before submission created no job or charge. The raw output, request, receipt and actual reference hashes are preserved in Sources/ and Diagnostics/.

The actual 64×64 Rattlebones golden PNG was sent through style_image as the rendering authority. The Forest palette and reference mockup govern materials/theme; no Rattlebones anatomy, royal palette or character redesign.

## Handoff

Review Review.html for on/off Unity comparisons, native props, enlarged details and raw/clean comparison. Keep PR #206 draft on feature/forest-zone-hybrid-rebuild. Await approval before Stage 5; do not merge or change production bindings.
