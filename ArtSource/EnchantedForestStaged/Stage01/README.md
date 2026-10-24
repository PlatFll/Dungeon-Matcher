# Enchanted Forest — Stage 1 review

Status: candidate awaiting user review. **Stop before Stage 2. No merge.**

Branch: feature/forest-zone-staged-rebuild, based on preserved PR #207 head
0db189f43ed230b920bace2be18d104f357a28d2. Pack preserved under ../Brief/.
Main, PR #207 and all approved artwork remain unchanged.

## Result

The pack's forest reference directs a deeper teal grove, distant bridge/waterfall,
restrained lantern warmth and a quiet, flat enemy clearing. This stage replaces
only scenery in a new art-only review scene. Existing Rattlebones, three Forest
enemies, portrait, HP/countdowns and shared top buttons are static readability samples.
The board, shared sizing corrections and lower HUD are not edited or rebuilt.

PixelLab: one Pro source at 360×240, **15 generations**, job
793ad468-b410-4198-b50b-54ed0785c7f5.
The actual supplied reference and current verified Rattlebones PNG were submitted,
with Rattlebones restricted to rendering discipline rather than palette/anatomy.
Diagnostics/Request.json, Receipt.json and NativeValidation.json record lineage.

LibreSprite: native cleanup merged one redundant dark tone, lowered the waterfall's
brightest value, calmed two floor colors, and consolidated 189 tiny disconnected
foliage highlights in selected regions. Source: 28 colors; cleaned assembly: 27.
No scaling, blur or dithering. Every editable document was saved, reopened and
re-exported; pixels match exactly.

| Separate module | Native size | Opaque colors |
|---|---:|---:|
| Grove_Background | 360×200 | 27 |
| Combat_Ground | 360×40 | 19 |
| Wisp_Far_A | 16×16 | 2 |
| Wisp_Far_B | 16×20 | 2 |
| Wisp_Near | 18×22 | 1 |

Wisp cores are isolated with transparent padding for future motion. Surrounding
scenery stays static. Lantern, branches and waterfall remain part of this background
study; final decor separation/animation belongs to later stages. No animation made.

## Preview and validation

Open Review.html for native 1× and nearest-neighbor 2×/3× comparisons, isolated
modules and actual Unity captures.
Scene: Assets/_Game/Scenes/ArtPreview_EnchantedForest_Stage01.unity.
Menu: **Dungeon Matcher / Art Preview / Enchanted Forest staged 01**.
Excluded from player build settings; no gameplay systems run in this scene.

- Tools/Validate-Unity.ps1: passed, Unity 6000.3.19f1.
- EnchantedForestStage01Tests.TopBackgroundReview: passed.
- Native Unity background equals cleaned PNG after RGBA decoding.
- 1440×960 capture equals exact nearest-neighbor 2× of the 720×480 capture.
- All five new assets: Point, FullRect, 64 PPU, no mipmaps/compression.
- Grove/ground fully opaque; wisp alpha values are 0 or 255.
- Modules reconstruct the complete cleaned background exactly.
- All 24,100 previous tracked files matched starting hashes before the task-status
  entry in PROJECT_STATE.md. Three automatic whitespace-only Unity importer changes restored.
- All characters, approved art, gameplay, shared geometry and lower HUD preserved.
- No Android or real-device test. This is not production Forest integration.

The initial check reduced unchanged UI to half its authored review scale and exposed
a pre-existing half-pixel HP track position. Final review keeps that UI at its authored
720px scale and exact 2× enlargement; the new environment has a separate pixel-exact
native 360×240 capture. Both test records are retained. No existing UI geometry or
existing test was changed to hide this distinction.

## Artistic verdict and next stage

A coherent enchanted clearing with stronger forest depth, readable combat silhouettes
and small controlled magical accents. This is a review candidate, not an automatic
approval. The richer portrait vines and board surroundings are deferred to their
specified stages. **Wait for approval before Stage 2.**

Editable documents: Native/. To repeat cleanup, prepare blank canvases with
Scripts/PrepareCanvases.py, then run Scripts/AuthorNative.js in LibreSprite from the
repository root. Scripts/ReopenNative.js checks persistence.
