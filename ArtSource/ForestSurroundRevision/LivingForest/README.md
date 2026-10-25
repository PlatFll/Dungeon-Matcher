# Living Magical Forest — review copy

The outer green sprig on the right corner is removed. The unchanged corner source remains in `PolishFinal`; the revised source is `Native/Corner_Right_NoSprig.aseprite`.

Three five-pose foliage layers receive occasional 3.1-second gusts, separated by 9.7–12.5-second periods. Their anchors, palettes and native scale are preserved. Light silhouettes include a compact mote, curled crescent, long comet and the existing flame. Eight lower placements and the upper spirit/firefly layers have independent fade and drift rhythms. Fades range from quick 2.1-second fireflies to slower 8.9-second spirits. Their native position changes are integer pixels, never subpixel transforms.

Two tiny blue lights were separated from a copied upper backdrop. The other upper spirit layers and gold firefly already existed separately. A second gold firefly adds a different rhythm. The waterfall uses a 12-pose low-contrast overlay, clipped to existing water pixels; the bridge, foliage and shore shapes are unchanged.

## Sources and scope

- Separate scene: `Assets/_Game/Scenes/ArtPreview_ForestSurround_Living.unity`.
- Previous source scene: `Assets/_Game/Scenes/ArtPreview_ForestSurround_FinalPolish.unity`, unchanged.
- 11 editable LibreSprite documents; 43 native PNG poses under `Exports/` and the isolated Unity art folder.
- 5 PixelLab generations, 3 selected studies, 2 rejected. Raw outputs, receipts and reference hashes are preserved.
- Source alpha is binary; intentional Unity opacity animates light fades. No sprite resampling or scaling baked into the artwork.
- Board, gems, characters, portrait frame, UI, combat mechanics, saves and all production scene bindings are unchanged.

## Verified

- `Tools/Validate-Unity.ps1`: successful Unity compilation.
- `LivingForestReviewTests.MotionReview`: passed, actual Unity Play Mode render captures from the copied art scene.
- 720×1360 and 1440×2720: exact pixel doubling; Point filtering and no mipmaps; no truncated UI text.
- 288 captures over 24 seconds: no native canvas or viewport clipping; varying light opacity/position; foliage rests for most of the time; all water poses visited.
- Exact pixel comparisons protect the board, frame, portrait, bottom controls and all HUD text throughout the sequence. Changes stay inside explicitly authorized motion/removal regions.
- All 6,746 snapshotted prior art/scene/script/resource/settings files unchanged.
- Pause and Reduced Motion produce stable cosmetic poses.
- No Android build or physical-device test in this art-review pass. This is not a claim of production gameplay integration.

## Review and reuse

Open `Review.html` through the existing loopback gallery. The video contains actual Unity frames at 12 fps. In the scene, opacity updates every Unity frame; source poses remain discrete. Video stops after 24 seconds because independent rhythms do not share that loop length.

Open the copied scene and Play to inspect continuously. The editor-only component intentionally cannot alter a production build. Native GIFs/strips show the pose family, while the scene controls the holds, fades and drift. Existing assets/scenes remain the rollback baseline.

`Diagnostics/NativeVerification.json`, `Validation.json`, `MotionEvidence.json`, `SceneAudit.json`, `ReferenceHashes.json` and `Costs.json` retain evidence and source lineage. Raw motion captures and full local logs are in the worktree scratch/diagnostics directories.

Draft PR #209 remains unmerged. Stop for user visual approval.
