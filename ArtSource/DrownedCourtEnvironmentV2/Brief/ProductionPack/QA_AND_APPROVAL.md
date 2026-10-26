# Technical + art-direction quality gates (every stage)

## Objective/automated tests — all required
- Every accepted isolated asset has expected native dimensions, consistent pivots/PPU, binary alpha where applicable, valid opaque PNG/Unity import pixel equivalence, and an editable LibreSprite source that re-exports identically.
- Point filtering, no compression, no mipmaps, FullRect, no NPOT resampling and correct Unity import metadata.
- Repeatable stone/rail/floor pieces join without unintentional seams; legal neighbor pairs tested in rows/columns. Check true repeating scenes, not only isolated tile edges.
- No fuzzy interpolation or independent X/Y source-pixel stretching at actual standard and tall phone resolutions.
- Coral and barnacle growth is structurally attached to stone/root, not hovering, clipping, or unconvincing sticker overlap. All stones/columns and contextual supports actually connect.
- Board's 8x8 gem cells, touch targets and puzzle rules are unchanged. Portrait background contained inside the frame; shared HUD remains unobstructed.
- Layer order: distant background < wall/columns/statue < standing floor < actors/health < floating ambient outside UI; correct occlusion and intentional parallax.
- Reduced Motion, pause, focus loss, saved run restore and flooding must not leave active ambient effects or animated overlays in incorrect state.

## Human artistic judgment — explicitly subjective
- Full game captures at native-size review and nearest-neighbor zoom beside current Dungeon & Forest references and mockup.
- Readable compact characters and bright faceted gems remain stronger focal areas than corals and light shafts.
- Coral is richer at outside edges/lower corners and calmer behind actors/board; no wallpaper or random 'aquarium' clutter.
- Distinct underwater ruined court: massive distant statue, columns and subdued reef growth; not merely Forest green recolor.
- Palette/shading consistent across assets of same material, controlled highlights, dark material separations and clean pixel clusters.
- No false automated '90% similarity' proof; record subjective 1–10 target quality estimate + evidence. If visually weak, do targeted correction/re-generation, not broad scene wipe.

## Screens and reports
Each stage deliver: before/after crop, selected native PNG(s), enlarged NN preview, checked sprite atlas/edge layout where relevant, real Unity art-only capture with asset installed, lineage/cost ledger, QC outcomes, known issues, ACCEPT/CANDIDATE/REJECTED status. Capture at 360x640 + 360x800 logical and real Game/phone resolutions where possible: 720x1280, 1080x1920, 1080x2400 plus a safe inset. Preserve reference compositions and functionality; disclose any test not executed.

## Approval handling
User approval of a stage authorizes the next stage only. Approval of still art doesn't automatically approve animation or production integration. Stage12 production binding requires separate explicit authorization; PR merge needs further approval. Final accepted visual direction triggers accurate canonical guide/reference entry/changelog only as appropriate, not a new global rule for other zones.
