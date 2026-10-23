# Forest Hybrid — Stage 1 review candidate

Status: Stage 1 complete; awaiting user visual approval. STOP before Stage 2.
Branch: feature/forest-zone-hybrid-rebuild
Base: main 250fc87a (merged Dungeon PR #205).
Scope: isolated forest battle-background foundation. The playable Forest remains on its previous presentation.

## Delivered

15 native modular PNG sprites and 15 editable LibreSprite documents:
- Far forest depth, side extensions, canopy repeat and stepped overlap.
- Independent left/right trunks, canopy masses and two simple future hanging-support anchors.
- Two flat ground-surface variants and two separate earth/root fascia variants.
- Quiet understory repeat.

The assembly is made from separate Unity Image objects at 64 PPU. The preview viewport is 360x240 logical pixels. Every sprite is drawn at native size, with no tint, filtering, compression, mipmaps or nonuniform scaling. Repeats may continue past the viewport; independent trunks, foliage and support sprites have padded intact silhouettes.

The horizontal ground has an explicit y=210 standing baseline. Rattlebones, Farmer and Pan are static reference figures at their unchanged native sizes. This is an art-only scene, not a gameplay test.

## Kept and changed

Kept byte-for-byte: existing Dungeon and Forest production presentation, all shared bottom HUD/buttons/settings art, all existing scenes, canonical guide/register and golden reference files in the protection manifest (954 files). No production scene, zone mapping, cast, gem, gameplay or global UI was edited.

New: the 15 candidate foundation assets, their editable sources and build scripts, a dedicated art-preview scene and editor capture fixture.

PixelLab: one 256x160 Pro Flash far-depth source, 9 generations actually charged. Rattlebones was supplied through the supported style-image input for rendering discipline, not anatomy or palette. Job and seed are recorded in Sources/PixelLabJob.json.

LibreSprite: all 15 final documents authored/exported in native pixels. The distant source's 24 colors were deliberately consolidated to 9 quieter depth tones without scaling or changing its pixel geometry. The other 14 modules were authored locally from explicit native pixel plans. No second generation, character generation or production replacement.

The original dense Forest backdrop is preserved; none of its UI cutout fragments were copied into the new foundation.

## Verification actually performed

- Tools/Validate-Unity.ps1: PASS on Unity 6000.3.19f1.
- ForestHybridStage01Tests.FoundationNativeScreens: PASS.
- Six actual Unity Game View captures: 360x240, 720x480 and 1080x720, with and without reference figures.
- Native screenshot → nearest-neighbor 2x/3x screenshot equality: exact.
- All 15 editable documents reopened in LibreSprite; exported pixels equal the delivered PNGs.
- Binary alpha, padded standalone silhouettes, ground A/B joins in both orders, full background coverage and equal cast floor contact: PASS.
- 954 protected file hashes unchanged. Only scoped new files remain in Git.
- Source colors: far layer 24 → 9; no blur or resampling.
- No Android build or real-device test; no actual gameplay validation at this stage.

## Remaining three review concerns

1. This is the foundation only. Portrait/board wood frames and their actual HUD overlaps await their approved stages; they have not been redesigned here.
2. The trunks, anchors and ground intentionally remain restrained. Distinctive lanterns, vines and small magical props belong to Stage 4; the stag is deferred to avoid adding a focal competitor.
3. Actual phone/safe-area framing and live enemy/UI interactions need the later assembly/integration stage. The current engine evidence validates the isolated native foundation, not production responsiveness.

## Reproduce

From the worktree root:
1. Run ArtSource/ForestHybrid/Stage01/Scripts/BuildNative.py with the Foundry art Python.
2. Run LibreSprite -b --script ArtSource/ForestHybrid/Stage01/Scripts/AuthorNative.js.
3. Run Tools/Validate-Unity.ps1 after editor code changes.
4. Run the explicit Unity EditMode fixture ForestHybridStage01Tests.FoundationNativeScreens.
5. Run LibreSprite -b --script ArtSource/ForestHybrid/Stage01/Scripts/VerifyNativeReopen.js.
6. Run ArtSource/ForestHybrid/Stage01/Scripts/ValidateAssets.py with the Foundry art Python.

The helper build creates scratch canvases in ignored .utmp/forest-stage01. Never run the new preview scene builder against a dirty production scene.
