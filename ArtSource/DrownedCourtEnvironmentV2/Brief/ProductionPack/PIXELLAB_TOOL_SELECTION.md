# PixelLab choice guide — verify actual connected tools before making calls

Official docs: https://api.pixellab.ai/v2/docs and https://api.pixellab.ai/v2/llms.txt (refer to latest available API/MCP tools; names here are endpoint families, NOT an assumption that identical tool identifiers appear inside Astra's MCP).

Use the **most specific supported tool** for each purpose:

| Asset | Preferred capability family | Notes |
|---|---|---|
| Single ruin columns, trident statue, stone decorations, reef/coral props | 1-direction object, map object, or image generation with style references | Favor native-looking transparent parts; bigger objects allowed if justified; don't bake whole scene |
| Modular floor top, platform front fascia, transition terrain | Sidescroller connected tileset (`create-tileset-sidescroller`) if actually appropriate | If its tile grammar cannot express exact Court stone seams, generate a seed and complete exact modular joins in LibreSprite |
| Underwater wall masonry and recessed stone | Style-guided image/tileset seed, or simple native direct editing | Select 2–4 cohesive variants; avoid distinct RGB drift between segments |
| High-quality unique background focal sculpture | `generate-image-v2` or equivalent high-quality image/object tool | Keep statue separate from background; use dark silhouette for layering; do not generate a full HUD |
| Fine correction to selected art | `edit-images-v2`, inpaint where supported, otherwise LibreSprite pixel editing | Require localized edits preserving geometry rather than replacing good art |
| Seaweed sway, coral polyps, wisp/bubbles | `animate-with-text-v3` for individual sprite motion IF it preserves form; otherwise discrete native cels/timed transform animation | Keep anchors planted and no swelling/glow blur |
| Small simple accents | `create-image-pixen` or low-cost object seed | Don't spend large-image generations on tiny shell or bubbles |

**Request discipline:**
1. Examine current Astra MCP advertised tool schemas and allowed sizes before every new family. If endpoint names differ, use corresponding capabilities, never fabricate a tool call.
2. Query remaining subscription/credit balance and quoted cost; log job ID, provider tool/model, prompt, input images/hashes, size, billed equivalent, output source and selection.
3. Prefer actual native golden sprite(s) as style reference; compare concept target separately. Use an unzoomed/cleaned reference when necessary so a 941x1672 mockup doesn't mislead pixel scale.
4. Test one representative sprite before filling a whole family; keep good seed and make variations and explicit left/right variants without just mirroring text/symbols.
5. Use source sizes appropriate to the module (often 32x32, 64x64, 128x128 for props, >128 for exceptional architecture as supported). A large 512px model is useful only when it genuinely helps complex scenery and can be brought back to native game pixel density without resampling.
6. Assets must be editable and precisely placeable; no 'make this entire scene with every character, gem and UI in one image' prompt.

## PixelLab-ready prompt structure
```
SUBJECT: [one specific asset/family].
VIEW: front-facing 2D sidescroller dungeon environment, no isometric perspective.
MATERIAL: [teal submerged stone / corals / kelp / etc.], grouped top-left light/shadow.
SILHOUETTE: [specific readable shape and native-space extent].
ATTACHMENT/SEAMS: [stem in stone crevice / ledge anchor / repeat boundaries].
STYLE: clean Dungeon Matcher native pixel art; actual current Rattlebones/approved Court refs submitted through supported image-reference input.
BACKGROUND: isolated transparent sprite if appropriate; NO actors, UI, text or grid.
AVOID: fuzzy/smeared pixels, too many micro-cracks, coral sticker collage, busy gems overlay, bright aquarium stock illustration.
```
Use stage-specific `PIXELLAB DESCRIPTION` paragraphs; customize real dimensions based on current Unity source-pixel scale and tool capability.
