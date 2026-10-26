# Ironvein Excavation — modular environment review

**Stages 1–10 complete for user review. Not integrated into production Game. Not merged.**

Branch: `codex/ironvein-excavation-art-rebuild`, based on main `10445532e7a61f81b693f074050eb9ce264c1d22`.

Open [the comparison gallery](Review.html), [before/after Unity captures](Preview/BeforeAfter.png), or the [native asset board](Preview/NativeAssets.png).
Unity review scene: `Assets/_Game/Scenes/ArtPreview_IronveinV2_Final.unity` (excluded from build settings).

## Result

- 14 focused text-only PixelLab requests produced raw cavern, portal, timber, lantern, rail/floor, ore, portrait/board frame, support, cart, tool and banner sources.
- 40 separately importable native modules were prepared and cleaned in LibreSprite: editable `.ase` sources in `Native/`, PNGs in `Exports/`, immutable generations in `Raw/`.
- The supplied mockup guided theme and composition locally. **No image inputs were uploaded to PixelLab.** Golden sprites were inspected locally for rendering discipline; their geometry/palettes were not copied onto the architecture.
- The mine uses cool dark rock, quiet iron, warm timber, localized amber light and flat combat-floor modules. Side posts have uninterrupted shafts with separate sparse braces. Lower corners each have a grounded prop/rock formation.
- Portrait and board frames use separate corners, rails and connectors through the existing `ResponsiveModularFrameFitter`. The portrait recess overfills its unchanged opening and is clipped by `RectMask2D`.

## Stage record

| Stage | Completed output |
| --- | --- |
| 1 | `PLAN.md` locked before generation; authority hashes and 7,081-file preservation baseline |
| 2 | Cavern wall/recess, large portal plus distant portal, timber, lamps, flat rail platform, ore |
| 3 | Native portrait corners, rails and centered joints |
| 4 | Native board plates, rails and centered joints; existing board footprint retained |
| 5 | Separate side caps, bases, shafts, braces, ore joints and attached chains/lamps |
| 6 | Left ore cart/rock anchor and right tool/rock anchor; original HUD center retained |
| 7 | Restrained mining banners, ore and distant rail hint; no unnecessary prop clutter |
| 8 | LibreSprite material recolors, isolated highlight cleanup, border/padding cleanup and tile seam repair |
| 9 | Separate in-engine review scene; exact captured cells/gems/actor/UI rectangles |
| 10 | Native inspection, Unity captures, import/geometry checks, source round-trip, preservation and required Unity validator |

## Kept unchanged

Original characters and animation sources, faces, gems, cell art, shared control graphics and positions, bottom HUD graphics, player/board dimensions, enemy floor/HP registration, gameplay, saves, production scenes/themes and other zones. The original Ironvein artwork remains available and bound in Game until a separate integration is approved.

At 720×1280 the captured board remains **545.099×545.099** screen pixels at **(87.4505, 215.4505)**. The portrait remains **195×406** at **(21, 827)**. Existing board/gem fractional scaling is retained; this review does not silently shrink cells or alter the shared layout. New environment artwork uses uniform integer pixel scale at the captured resolutions.

## Verification actually performed

- Read-only actual Game preflight: Ironvein, Dungeon and Forest at 720×1280 and 720×1600; shared frame dimensions matched and layout validators reported no errors.
- Read-only actual Ironvein capture: exactly 64 current cells and 64 current gems, plus existing actors, health bars, indicators and portrait graphics, using a disposable save profile.
- Unity art assembly: all 40 imported opaque pixels and alpha match native exports exactly; Point filtering, uncompressed, no mipmaps, FullRect, 64 PPU. Frame rail/plate centerlines checked. Original opening geometry retained.
- Actual Unity camera screenshots at **360×640, 720×1280, 1080×1920, 360×800, 720×1600 and 1080×2400**. The larger/tall captures retain the measured layout rather than inventing a new board size.
- Every native asset has binary transparency, **1–15 opaque colors**, and an exact LibreSprite save/reopen PNG round-trip. Prepared silhouette masks are preserved by cleanup. Opposing wall edges and floor-strip horizontal edges match.
- **7,081 original files are byte-identical** to the starting baseline. Unity's incidental three consumable-meta formatting rewrites and TimeManager reserialization were restored to their exact baseline bytes.
- `Tools/Validate-Unity.ps1` completed successfully with Unity **6000.3.19f1** after the final editor implementation. `Preflight.xml` and `ContentFinal.xml` passed. Detailed results: `QA/FinalValidation.json`, `QA/UnityAssembly.json`, `QA/NativeAssets.json`, `QA/ProtectedCheck.json`.

These are **art-only Unity renders**, not new-art gameplay integration, animation testing, safe-area input testing or phone testing. No Android build was needed for this review-only scene. Existing tiny legacy Ironvein controls and actor scale were deliberately retained under the UI/layout lock; approval here does not silently change them.

## PixelLab accounting

**76 generations spent**, **459.2 remaining**, no purchase. Starting balance 535.2. Raw submission settings and job IDs: `Receipts/GenerationJobs.json`. Completed jobs/source hashes: `Receipts/CompletedJobs.json`. Aggregate usage: `Receipts/Usage.json`.

The larger portal was selected as the focal structure; the first smaller entrance was retained as a distant module. One later rail request improved the tunnel depth during QA. No full-screen image was used as final scene artwork.

## Reproduction and approval boundary

`Scripts/PrepareSources.py` performs lossless native extraction/padding. `Scripts/NativeCleanup.js` performs the actual material/pixel edits inside LibreSprite; `Scripts/RunLibre.ps1` runs it in a separate hidden editor. `Scripts/Roundtrip.js` reopens the editable sources. `Scripts/RunUnity.ps1` runs the preview builder or explicit capture fixture. Use the installed Foundry Python and LibreSprite paths documented in the local Foundry report.

All new assets are internally selected **review candidates**, not user-approved production replacements. The final artistic verdict is that the modules are crisp, coherent and ready for visual review. **User approval of the art and assembled treatment is still required; production integration and merge have not been performed.**
