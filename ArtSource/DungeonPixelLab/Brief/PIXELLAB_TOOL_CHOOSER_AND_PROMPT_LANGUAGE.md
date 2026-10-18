PixelLab-first asset-generation method notes (verify live tool schemas before use)
===========================================================================
Official docs: https://api.pixellab.ai/mcp/docs | https://api.pixellab.ai/v2/llms.txt
Official website: https://www.pixellab.ai/docs/tools/create-tileset

Use the ACTUAL tool surface exposed to Astra, not unverified function names.

Terrain:
- SIDE-ON platform transitions: PixelLab's sidescroller tileset generator / API `create-tileset-sidescroller` when supported. Standard sidescroller commonly uses 16px/32px cells, not arbitrary 64. This is not the top-down or isometric tool.
- Background WALL masonry pattern: supported text-to-image or tile-group / style-generation tool with correct native pixel dimensions; create tileable or manually construct seamless repeats.
- An individual 64px panel tile is not itself proof that adjacent tiles will match. Prove seams after cleanup.

Props:
- Transparent object generation/map object for barrel, torch, chain, banner or skull pile. Keep one object identity per source when possible, not a complete decorative scene.
- Use image style references when tool exposes a proper input. A full game screenshot is not an acceptable *style image*; use actual current native golden PNG and accepted material swatches. Do not copy Rattlebones's exact silhouette.

UI:
- Use a dedicated UI/image generation tool if available for *single* frame corners, rivets, hardware, skull crest or center emblem. Reconstruct frame rails/9-slice building blocks in LibreSprite. Keep text dynamic/live in Unity, don't embed unreliable PixelLab letters.
- The approved current game UI frame shapes and gem colors are the existing production-identity authority; don't invent a generic game UI kit or recolor existing locked gems.

Aster's asset-description scaffold (adapt per phase):
"ONE isolated native-resolution pixel-art [object / 32px sidescroller tile] for a charming retro 2D fantasy dungeon mobile game. [exact shape/material/functional connections]. Dark cool violet stone with deep indigo mortar; limited connected top-left highlight and lower-right shadow clusters, crisp single-source-pixel edges, controlled 2-4 material tones plus outline when applicable. No gradients, noise/dither, extra ornaments, anti-aliasing, or scattered cracks. Flat orthographic side view, not oblique/isometric, no background unless specified. Compatible source-pixel scale with approved Dungeon Matcher Rattlebones render discipline, but separate appropriate environment palette. [required alpha/canvas/seam constraints]"

Cost pitfall:
- Official docs describe Standard sidescroller tileset costing about 2-3 generations; experimental Pro tileset jobs can cost much more. Read actual tool estimated charge before generating. The user's Tier 2 max canvas 512x512 is per eligible image feature; do NOT request 512 for a 32px tile just because it is available.
- Add to ledger with returned generation/job IDs, exact request sizes, named style refs used, generations debited, decision, and reason. Never paste API tokens into files, logs or Git.
