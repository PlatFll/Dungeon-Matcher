# Stage 08 — DONE; stopped for user review

2026-10-19. Chains and crowned-skull banners only, following the user's approval of Stage 07. PixelLab sources and direct native LibreSprite editing. No Stage 09, production integration or merge.

## Delivered

| Module | Native canvas | Opaque colors | Role |
|---|---|---|---|
| Hanging_Banner | 36×64 | 9 | Complete plum pennant, crowned skull, rod and clips |
| Hanging_Chain_Anchor | 12×12 | 4 | Ceiling mount and first edge-on connection |
| Hanging_Chain_Repeat | 12×12 | 4 | Open face-on link and edge-on connector; 12px repeat pitch |
| Hanging_Chain_End | 12×12 | 4 | Complete closed end link with four rows of bottom clearance |

One banner size serves all three placements: **(40,32)** in the portrait, **(140,58)** and **(282,58)** around the gate. Two chains start at **(118,0)** and **(242,0)**; each uses anchor, four repeats, and end, for a **12×72** canvas with occupied bounds **[2,2,10,68]**. Nine meaningful colors across cloth, crest and iron. Nothing was scaled or resampled.

## Source provenance and limitations

Current guide/register and actual Rattlebones, Farmer, Pan and applicable UI references were checked against current source hashes. Rattlebones governs rendering discipline, not environment anatomy or palette. The previous stages remain the approved material authority.

1. Pro Flash job **3527e875-8178-47c1-9b1c-0e949298e4c9**, seed **810801**, **96×64**, quoted and charged **5**. Actual native Rattlebones was supplied through the supported style input. This request asked for both props but returned only the banner. Its gray cloth was corrected to dark plum in LibreSprite; the rod received restrained iron shading. The full silhouette and crest pixels were preserved after native crop/padding translation.
2. Targeted chain-only PixFlux job **c8a67f85-e4ad-4437-8f5e-1a6a48230204**, seed **810802**, **16×64**, documented and charged **1**. This endpoint has no style-reference parameter; golden PNGs were inspected locally, not falsely claimed as uploaded. Its raw result was a clipped two-pixel strand, not a finished usable chain. LibreSprite repair substantially reconstructed the open face-on links, regularized their pitch, and added complete mounting and termination pieces. The raw limitation is visible in the comparison sheet. No claim of preserving its silhouette is made.

Both raw PNGs and exact requests are immutable in `Sources/Stage08/`. Editable `.aseprite` sources and independently exported PNGs are retained. Direct native cel authoring ran in the actual LibreSprite executable, not a claimed MCP bridge session. No screenshot extraction, other provider, blur, interpolation or additional paid retry.

## Validation actually performed

- Four native cel/PNG/import comparisons: exact RGBA equality, binary **0/255 alpha**. Full banner and assembled-chain bounds have transparent clearance. Repeat and connector modules intentionally touch their joining edges.
- Banner alpha silhouette and crest colors/pixels match its raw native source. Crest center **x17**, within half a pixel of canvas center **17.5**. Rod and clips remain connected; the complete bottom point is inside its canvas.
- Repeat top/bottom rows and anchor/end joins match exactly. The assembled chain is a single connected metal component, with open link holes and a regular **12px** pitch.
- All **185 protected art files** and **200 prior placements** unchanged. Fifteen new placements produce **215 total**. The composition below **y122** is pixel-identical to Stage 07, including the entire gate base, floor, fascia and lower masonry.
- Zero opaque pixels overlap reserved Wave, Wave-skull, Guide or Gear bounds. These controls are reserved for later stages, not newly imported here. The portrait banner stays completely inside its inset and beneath the existing frame layer.
- Explicit Unity test `DungeonPixelPipelineStage08Tests.Stage08HangingNativePreview`: **1 passed, 0 failed**. Actual captures at **360×640** and **1080×1920**, normal/unlit/contact/restored, all match independent native composition with **zero RGBA differences** across eight images.
- `Tools/Validate-Unity.ps1`: **SUCCESS**, exit 0, Unity **6000.3.19f1**. Log: `C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-4a3792e4-f5bc-4298-8a2b-bda675926237.log`. Source hashes and normalization receipt are recorded in `Diagnostics/Stage08/ValidationReceipt.json`.
- Point/FullRect/64 PPU, no mipmaps, compression or NPOT resizing. All existing transforms and images preserved in the semantic scene review. Production Game scene, build settings and main-checkout user work preserved. This is an isolated Unity art preview, not a device build.

## Artistic verdict and stop

Ready for user review. The muted crest reads at native scale; quiet plum cloth complements the approved masonry without competing with flame accents. The thin iron chains are continuous and restrained, and the full pennant points are visible. No clipping, torn motifs, soft pixels or control interference found in native/enlarged review. User approval remains pending; numerical checks are not art approval.

Balance **1025.20 → 1019.20**. Stage cost **6**, cumulative **35/35**. Further paid stages require approval to raise the pack's soft budget; the account itself has remaining allowance. **STOP before `PROMPTS/09_GENERATE_BARRELS_SKULLS_BONES.txt`.** No Stage 09 work has started.
