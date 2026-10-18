# Stage 00 — DONE; stopped for review

2026-10-19 local date. Branch `feature/dungeon-zone-pixellab-asset-library`, base main `1ee1fdb02e409c9d6b17785d84a76d36bab6342c`. Reused the clean isolated `dungeon-top-hud-pixellab` worktree. Stages01–16 remain NOT_STARTED. No merge, live integration, generation, reference upload, or gameplay/layout change.

## Completed gate

- Read START_HERE, FIRST_MESSAGE, global rules and only Stage00; extracted the complete pack unchanged for later use.
- Read current canonical guide/register and Foundry workflow. Viewed actual native Rattlebones/Farmer/Pan and UI examples. All three golden hashes and production extractions match. Exact paths, sizes, palettes and SHA-256 values: `Reference_Audit.json`.
- Authenticated PixelLab balance and free capability/price queries succeeded. Active Tier2; **1,054.20 equivalents before and after**, $0 fallback balance. **Zero paid jobs, zero charge**. Tool-specific costs/reference routes/limits: `Tool_Selection.md`; exact runtime schemas/receipts: `PixelLab_Capabilities.json`.
- Created empty art-only Unity scene using only existing preview infrastructure. No previous artwork is accepted as new kit art. No actors/enemies/gems/gameplay owners or build-settings entry.
- Native32x32 six-color binary-alpha diagnostic probe saved to LibreSprite, reopened/exported byte-exact by decoded pixels. Source wall64x32/four colors used only in a transient diagnostic.
- Captured real Game Views of current Dungeon at720x1280 and1080x1920; exact-scaffold/probe views at360x640 and1080x1920. New local material swatches and size brief remain candidates, not global approvals.
- `Progress/State.json`, ledger and manifest written. All eight starting modified/untracked main-checkout files retain their original hashes.

## Pixel-scale diagnosis

**Verified:** Current scenery source pixels are stretched independently by axis. At1080x1920 the barred door's61x87 source occupies181x231 screen pixels (**2.9672x / 2.6552x**). The23x56 torch occupies68x148 (**2.9565x / 2.6429x**). The top-left board corner is approximately2.5173x / 2.8592x. All72 sampled Simple renderers have at least one noninteger source-pixel pitch at both captured phone resolutions. Canvas scale itself is integer (2 at1080,1 at720); that does not guarantee its child artwork is integer-scaled. `ReferenceKitSurface`, `ApprovedDungeonBattleArt.BoardRect`, and `DungeonPresentationArt.UpdateApprovedDungeon` map source art into flexible production bounds. Point sampling preserves sharp color transitions but repeats source rows/columns unevenly: distorted clusters/line weights, not proof of bilinear blur.

**Verified controlled comparison:** The existing wall PNG and diagnostic probe survive source -> imported sprite -> real Game View with **0 differing opaque RGB pixels at1x and3x** (28,320 compared screen pixels total). LibreSprite native save/reopen/export is also pixel-exact. Direct live door/torch comparisons match nearest-resized source colors on97.23%/98.27% of the opaque predicted pixels. The remainder is not silently declared blur: layered occlusion and sampling phase are not fully separated. Source and actual live crops are provided for inspection.

**Ruled out for sampled bindings/configuration:** All74 sampled bindings use64PPU, Point, FullRect, no mipmaps, no texture compression and no active Android texture override. Actual imported formats areRGB24/RGBA32. The active camera targets the screen, URP renderScale=1, MSAA=false and dynamic resolution=false. No lower-resolution camera target was observed. The exact control capture demonstrates no resampling in that tested path. HDR is enabled on the production camera; do not infer all post-processing/device paths are certified from these checks.

**Source shading / alpha:** Wall uses4 opaque colors and no transparency; barred door14 and torch20, both binary alpha. No semi-transparent source-AA fringe exists in these samples. Opaque color count alone cannot rule out excessive micro-shading or prove artistic quality across the whole119-asset old kit. No claim that imported Point filtering can repair a painted/noisy source. Future PixelLab originals still require native visual/cluster review.

**360x640 distinction:** A physical360x640 *production Game* run failed with `Gameplay pixel layout cannot fit screen ... board (568.26,568.26)`. See `Diagnostics/Baseline360-Failure.xml`. This is a verified current-layout limitation, not a passing game capture or a request to change gameplay. The independent art-only360x640 scaffold DOES pass. Its future3x1080 mapping cannot be assumed to match the existing live layout automatically. Resolve that mapping explicitly at the later approved assembly/integration stage without moving UI.

**Unknown:** The phone build's exact revision, device framebuffer/upscaler, OS screenshot resampling and any settings differing from this clean-main Windows capture. The supplied blurry reference is JPEG; its resizing/compression history is unknown, so it cannot establish native-pixel fidelity or a runtime cause. We have not built or tested a new APK. Unusual source micro-shading across unsampled assets is not exhaustively certified. No production blur fix is claimed by this setup stage.

## Validation and artifacts

- Required `Tools/Validate-Unity.ps1`: SUCCESS on final editor code (6000.3.19f1); hashed receipt in `Diagnostics/ValidationReceipt.json`, full local log `Diagnostics/UnityValidation.log` (ignored).
- Explicit opt-in Unity test `Stage00BaselineAndExactPixelScaffold`:1passed/0failed. Normal test suites skip this capture to preserve future art/checkpoints. `Diagnostics/UnityTests.xml`. Verifies captures, empty scaffold, no gameplay owners, integer pixel scale and unchanged scene/build-settings hashes.
- Independent exact PNG checks: `Diagnostics/PixelChecks.json`; repeat with Foundry Python and `Scripts/verify_stage00.py` from this checkout.
- Initial harness compile issue (renderer activity API) and domain-reload guard issue were fixed; the passing evidence is from the final source. The genuine unsupported360px production attempt remains as explicit failure evidence.
- Unity reserialized three unrelated consumable metadata files and TimeManager in the initially clean worktree; those four incidental changes were inspected and restored before committing. User main-checkout edits were never restored/replaced.
- Pixel probe: `Diagnostics/PixelScaleProbe.png`, `Native/Stage00_PixelScaleProbe.aseprite`, `Exports/Stage00_PixelScaleProbe.png`. Swatches: `Diagnostics/MaterialCandidates.png`; material/nativesize reasoning: `Progress/Art_Brief.md`.
- `Review.html` shows actual unresampled screenshots, exact source/live crops, and1x/4x reference views. The existing wall/cast/UI images are diagnostic/reference evidence only; no new environmental art has been accepted.

Source generation/candidate art count:0. Seam checks for new art: not applicable yet. Artistic verdict: the crisp reference language and technical path are established; the current production scene still has uneven native-pixel scaling. New asset art has not been made or visually approved.

Next prompt: `PROMPTS/01_GENERATE_DUNGEON_WALL_TILES.txt`. **STOPPED.** Wait for that prompt; no automatic continuation.
