# Snowy Mountain Stage 7 — lower environment

Review candidate, continuing approved Stage 6. This stage adds lower-left and lower-right environmental dressing only. Stop for user review before Stage 8. Do not merge or register this as production zone artwork yet.

## Result

Two separate snow-covered stone formations anchor the lower screen corners. The left has blue ice behind the rock; the right carries a small snowy supply crate. Existing Stage 6 rope posts connect the corners to the mountain-pass setting. The center resonance panel, potion/bomb controls, labels and their spacing remain unchanged. No enclosing HUD frame has been reintroduced.

PixelLab generated original native sources; LibreSprite consolidated their palettes into the accepted snow/stone/wood ramps. Shape and opacity masks remain exact apart from occupied-bounds extraction and one transparent pixel of padding. Four editable .ase documents reopen to byte-identical PNGs: left rock 62×55 / 7 colors, right rock 62×57 / 8 colors, crate 36×35 / 12 colors, reused ice 39×54 / 14 colors. RopePosts is reused directly from Stage 6. All alpha is binary; all silhouettes fit their canvases.

## Usage and lineage

21 PixelLab generations total (4910 → 4889 remaining): 10 for an oversized first kit rejected rather than scaled down; 10 for the native 64×64 rock candidates (4 and 13 selected); 1 for the 40×40 crate. Original candidates and request receipts are preserved in Raw. No images were uploaded; target and golden references were inspected locally according to the pack. No paid image-generation provider was substituted. QA/PixelLabUsage.json records the complete ledger.

## Verification

- Tools/Validate-Unity.ps1 passed on Unity 6000.3.19f1.
- Actual Game-scene preview test passed with Rattlebones, three live enemies and gems at 720×1280, 1080×1920, 1080×2400, inset-safe 1080×2400 and 360×640.
- Native integer scale, full canvas clearance, central text lane clearance and no added input targets are asserted.
- Shared HUD sprite identities, colors, positions, labels and interaction settings remain exact; board sprite states, floor line and shared layout remain exact.
- Paired screenshots verify the entire upper scene, board, approved Stage 6 dressing, central bottom lane and opaque control pixels are unchanged.
- All 9,253 protected files retain their starting hashes, including six preexisting local modifications.
- Visual review: the lower corners are grounded and balanced, with broad readable snow/stone shapes. At 360×640 the existing controls naturally cover part of the scenery; controls are drawn above it. Existing small-screen HUD crowding/camera warning, cell appearance and tall-safe-area backing remain baseline limitations. This stage does not fix those unrelated layout issues.
- Unity Editor evidence only; no Android build or physical-device claim.

## Reproduce and review

Run Scripts/BuildNative.ps1 with a fresh output directory to edit working copies in LibreSprite, save editable documents and reopen them for export. Run Tools/Test-ForestFoundation.ps1 -Filter ForestFoundationPlayTests.SnowyMountainStage07LowerEnvironmentReview -Graphics for the paired captures, then Scripts/Validate.py for native, preservation and pixel comparisons.

Review.html contains before/after lower crops, all five full Unity views, native-size assets, raw sources, palette counts and validation links. The temporary preview uses existing layout helpers; no production prefab or runtime zone registration is changed.
