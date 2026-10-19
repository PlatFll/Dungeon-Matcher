# Stage 11 — board complete; portrait verdict superseded

The subsequent user review rejected the visible rail centering. See [the corrected pipe pixels and current validation](Portrait_Pipe_Correction.md). Details below preserve the historical Stage11 implementation and evidence.

2026-10-19. Corrected Stage10 portrait construction, then built Stage11 board frame and empty cells. Draft PR #203; no production integration, Stage12 or merge.

## Portrait correction

The prior assembly used unequal transparent padding and fixed canvas origins for left/right rails and clamps. The backing used a fixed rectangle that stopped short of the lower frame. The old mask test checked spill but did not check full coverage or visible centerlines.

LibreSprite now saves four 13×13 jewel corners, 24×7 horizontal rails, 7×20 vertical rails and a 7×7 clamp. The existing HP shell and heart remain byte-identical. The shared `ResponsiveModularFrameFitter` has an opt-in mode for upright native pieces; existing production callers retain the original mode. Frame corners remain native, rails tile, and paired connector rows use identical integer coordinates with a centered middle connector.

The portrait keeps its 99×190 outer footprint at (9,20). The 79×170 interior at (19,30) uses the unchanged Stage02 center material, overfilled by a full tile beyond every side and clipped by an anchored RectMask2D. No change to the approved banner, character source or surrounding art.

## Stage11 art

One new 64×64 PixelLab source yields a native LibreSprite family: four 17×17 jewel corners, 32×9 top/bottom rails, 9×32 side rails, a 9×9 connector and a separately authored quiet 35×35 navy cell. Ten components; 68 total candidates in the staged library. Rails retain the generated stone profile and seam vocabulary with duplicate flat-shade rows simplified; jewel sockets are regularized for native modular joins. No resampling, blur, screenshot extraction, full-scene generation or new gems.

The empty 8×8 field retains the established origin (40,238) and 35px pitch: exactly 280×280. Frame outer bounds (27,225,306,306) provide intact corners around the original interior and clear the platform's y214 bottom. This is a candidate preview, not a change to production board geometry.

## Source authority and spend

Guide v1.17.2, current golden register and actual Rattlebones/Farmer/Pan plus current locked board UI were inspected and hashed. Actual native Rattlebones pixels were submitted through Flash style_image, with only outline/detail/shading copied. UI references were compared locally and their material ramp informed the palette; Flash accepts one native style image.

Job bd11c1c8-fd00-4c2d-b3a2-7c2b33f2c2f7; image 21520419-f0ac-50bb-9d96-c817132fe295; seed 911101. One paid call, no retry. User approved five extra generations. Actual charge 5; balance 1004.20 → 999.20; cumulative 55. Portrait correction cost 0. Source and exact request are preserved.

## Executed checks

- 21 current portrait/board ASE cels decode exactly to exported PNGs; Unity copies match. Binary transparency, no intermediate alpha. Ten board pieces use 5–11 opaque colors; exact palettes/dimensions in NativeChecks.json.
- 210 protected previous-art files and all golden references unchanged. HP and heart native/export hashes unchanged.
- Portrait Unity test: 1 passed, 0 failed. Four dimensions (99×190, 100×191, 119×231, 60×90), at 1× and 3×; matching clamp centerlines, centered middle, full interior coverage, zero outside leakage, native dimensions and undersize recovery.
- Board Unity test: 1 passed, 0 failed. 360×640, 1080×1920 and 1080×2400 with safe-area offsets; all 64 cells, pitch, frame bounds and prior-art dimensions verified. The initial test harness lost local baseline strings across Unity's domain reload; persisted hash files corrected that harness failure. First-run evidence is retained.
- Every rendered board cell matches the exported tile exactly. The 3× render equals nearest-neighbor enlargement of 1×; tall-screen content equals 3×. Earlier artwork outside the authorized portrait/board bounds is RGBA-identical.
- Serialized scene review: only the old portrait components/recess are removed, new portrait/board internals added, and root bindings/top-UI child list changed. Other objects remain semantically unchanged.
- Tools/Validate-Unity.ps1: SUCCESS with Unity 6000.3.19f1. Receipt records source hashes and exact log.
- Production Game scene/build settings and the eight dirty main-checkout user files are preserved.

## Artistic verdict and limits

The portrait now has a centered, repeatable metal construction and uninterrupted recessed backing. The board has quiet navy cells, restrained violet stone and clear corner fittings; its surface stays subordinate to future gems. Reviewed at native size and integer enlargement. Rattlebones in the optional readability captures is a temporary unchanged source specimen, not a saved actor.

The new art remains in the isolated preview scene. Production layout and phone integration are later work; Stage00's production pixel-layout caveat remains. Earlier numbered tests and fixed manifests are historical snapshots; current verification uses DungeonPixelPipelineStage11Tests and Scripts/verify_stage11.py.

STOP. Next prompt, only after approval: PROMPTS/12_GENERATE_TOP_HUD_UI_PLAQUES.txt.
