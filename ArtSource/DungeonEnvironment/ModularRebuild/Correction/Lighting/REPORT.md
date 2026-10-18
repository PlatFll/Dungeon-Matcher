# Dungeon upper-scene lighting correction

Status: native art updated in the isolated modular Unity preview; awaiting user visual review. No merge or production-scene integration.

## Scope and result

Nineteen existing modules received color/shading adjustments in LibreSprite: four BattleWall tiles, twelve floor tiles, both TorchLitMasonry sprites, and RightTopPillar's already-warm stone clusters. The remaining 131 exports are unchanged. No pixels were added, removed, scaled or moved. Doorway, torches, chains, banners, barrels, bones, frames and UI placement are preserved.

- Cooler, slightly darker far-wall palette separates the arch and brightened combat floor.
- Existing torch-lit masonry now has restrained red-brown outer planes and warmer orange highlights nearest the flame.
- The rear/middle/near floor families have distinct value ramps. A few complete existing stone clusters below the torches carry warm reflected color; no extra cracks or detail were added.
- The unchanged foreground lip now reads against the brighter standing surface.

## Visual iteration

Pass 1: depth, central-stage clarity, readable stones and shape preservation passed visual comparison. The broad orange brick faces were too assertive, so localized warmth did not yet pass.

Pass 2: softened broad warm faces, retained orange lit edges, and slightly extended the subtle floor reflection. All five requested visual checks now pass internal review: deeper upper background, warmer localized torch light, clearer combat stage, readable quiet stone faces, and preserved shapes/layout. The modular brick geometry remains more regular than the generated mockup by design; no redesign was authorized. Artistic acceptance remains the user's decision.

## Verification

- Exact canvas, alpha-mask and connected color-cluster boundary equality against the pre-lighting baseline for all 19 edited assets.
- Binary transparency and native LibreSprite cel / exported PNG / Unity-copy equality for all 150 modules.
- 224 horizontal/vertical seam pairings passed across seven tile families.
- Scene YAML, placement manifests, assembly/editor/test C#, build settings and six registered golden-reference/production images remain byte-identical to the starting baseline.
- Two existing Unity EditMode tests passed after the final pass, with fresh camera renders at 720x1280, 1080x1920, 1080x2400 and 1080x2400 with safe-area insets. Integer native placement, portrait/board clipping, all-module usage, save-file isolation and release-build isolation passed.
- Render differences are confined to the upper scene; lower board/HUD pixels are unchanged in all four captures.
- No C# or Unity serialization changed, so the broader compiler-only validator was not rerun for this art-only correction. The prior compiler evidence remains attached to the unchanged source. These captures are EditMode evidence, not live-combat/device tests.
- Zero generation calls or credits for this lighting correction.

## Reproduce and review

`Correction/LightingCorrection.js` reads immutable `Correction/Lighting/Before/` exports and applies native LibreSprite color assignments. `Build.ps1` applies this pass last; it does not generate new art. Use `Correction/ValidateLighting.py --import` to verify locked geometry and copy only verified modified PNGs, then run `DungeonModularPreviewTests` in Unity. `Correction/ReportLighting.py` regenerates the comparison gallery from actual captures.

Review: `Review/Lighting.html`. Technical evidence: `Correction/Lighting/Validation.json`, `UnityRenderChecks.json`, and `Evidence/UnityTests.xml`. The first trial remains in `Correction/Lighting/PassOneUnity.png`.
