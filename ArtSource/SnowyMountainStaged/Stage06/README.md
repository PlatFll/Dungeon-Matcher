# Snowy Mountain Stage 6 — board-side environment

Review candidate. Continues the accepted Stage 5 composition. Nothing is registered in the production zone yet. Stop for visual review before Stage 7. Do not merge.

## Artwork and composition

One text-only PixelLab Pro request produced four 128×128 modular-kit candidates for **10 generations**. Candidate 0 supplies the generated shapes. The target mockup and the current golden cast were inspected locally; no reference images were uploaded. Exact request, job ID and receipt: Raw/Generation.json.

Eight editable LibreSprite documents and matching native PNGs: left/right snowy timber supports (35×70), lantern (21×46), stag banner (40×70), snowy rock (38×41), ice cluster (39×54), repeat timber (14×18), optional rope-post pair (32×35). The rope-post pair is supplied but left out of this assembly to keep the gutters clear.

Native cleanup consolidates accidental dark shades into #0A0D11, aligns timber/snow ramps with the approved frame, and shifts aqua ice toward icy blue. Six redundant interior rock-plane columns were removed manually without interpolating the silhouette. The complete ice tip is preserved. All modules have at least one transparent native pixel around their occupied bounds.

The uprights attach beside the existing board frame and extend continuously down to their footings; the lantern arms project outward into the side gutters. Lantern chains and banner rods attach to these structures. The snowy rock/ice bases ground them beside the board's lower corners. Source assets remain separate; the scene is not flattened.

## Preservation and validation

- All 9,190 protected files retain their starting SHA-256 hashes, including the earlier approved stage sources and six preexisting local modifications.
- All eight .ase documents were saved, reopened and exported by LibreSprite. PNG round trips are exact, with binary alpha and 6–16 opaque colors per module.
- Tools/Validate-Unity.ps1 passed on Unity 6000.3.19f1.
- Actual production Game-scene preview fixture passed at 720×1280, 1080×1920, 1080×2400, inset-safe 1080×2400 and 360×640.
- Native integer scale, complete canvas clearance, no entry into the board opening, unchanged board sprites/transforms/colors, shared layout and enemy floor alignment are asserted in Unity.
- Paired screenshot comparison is byte-exact for the entire upper scene, playable board and bottom HUD. Only the board-side gutters differ.
- Existing 360×640 HUD crowding/camera warning, legacy gem-cell appearance and upper safe-area backing remain baseline behavior. This stage does not change them.
- Unity Editor evidence; no Android build or physical-device test was performed.

## Reproduce

From the repository root, run Scripts/BuildNative.ps1 with a fresh output directory. It opens working copies in LibreSprite, applies Scripts/BuildNative.js, saves .ase/PNG and reopens every .ase. Never pass an existing output directory.

Run Tools/Test-ForestFoundation.ps1 -Filter ForestFoundationPlayTests.SnowyMountainStage06SideDressingReview -Graphics for the paired Unity captures. Then run Scripts/Validate.py for native and exact pixel checks. The preview uses existing layout and modular frame helpers, with a disposable Stage 6 dressing root; production bindings, mechanics and other zones are untouched.

Review.html includes all actual Unity screenshots, local target comparison, native assets, raw candidates and validation links.
