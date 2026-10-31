# Snowy Mountain Stage 8 — atmosphere

Review candidate on the approved Stage 7 scene. Stop before Stage 9 or merging. No production zone binding, character, gameplay, shared HUD or earlier artwork is changed.

## Atmosphere

Sparse snow drifts through the upper mountain background and the outer board gutters. Three tiny source shapes vary the flakes: a two-pixel mote, three-pixel diagonal fleck and five-pixel lozenge. Integer native positions preserve hard pixels; different phases prevent synchronized movement. Three very brief glints are attached to existing ice tips. They never enter the board frame or playable gem grid. The center lower HUD stays quiet.

Twenty snow paths and three glint anchors use a deterministic 12-second cosmetic cycle. Some upper paths are omitted automatically where the short viewport has insufficient clear space. Snow opacity eases only at path endpoints; source alpha stays binary and there are no blurred halos. Glints use six native cels at 125 ms, followed by a long rest. Reduced Motion hides the atmosphere and reproduces the accepted Stage 7 scene exactly.

This is a disposable Editor Game-scene preview, sampled at authored times while combat is frozen for comparison. It does not wire the new atmosphere into the live production zone yet. Future integration must use the existing pause-aware presentation clock and Reduced Motion preference. No gameplay RNG, board mutation, input targets or mechanic timing is involved.

## Asset provenance

One text-only PixelLab Pixen generation produced the 32×32 raw sheet (4889 → 4888 remaining). The generated gray background was opaque. LibreSprite removed it, simplified the generated motif cores into native ambient-sized clusters, and consolidated colors to #ECFAFD / #B8D9E8 / #668FA6. No downsampling, blur or interpolated resizing. The six glint poses are hand-authored in LibreSprite from the generated sparkle motif. Nine editable .ase documents and PNG frames are retained; each is 8×8 with 1–3 meaningful colors or intentional blank rest, binary alpha and complete canvas clearance. Every reopened .ase exports exactly the same pixels.

Target and current golden images were inspected locally and source hashes verified. No reference upload; no other provider used. All original sources, prompt, model and job receipt are in Raw/Generation.json. Approved Stage 1–7 textures remain byte-identical.

## Verification

- Tools/Validate-Unity.ps1 passed on Unity 6000.3.19f1.
- Actual Game preview passed at 720×1280, 1080×1920, 1080×2400, inset-safe 1080×2400 and 360×640.
- 96 actual Unity frames cover a complete 12-second cycle. Their lossless WebP retains every captured pixel exactly.
- Every animation frame preserves the board/frame, portrait, controls, health bars, central lower text and energy lane pixel-for-pixel. Only 240–292 pixels differ per 720×1280 frame (under 0.04%).
- Native scale, canvas clearance, board exclusion, loop closure, RNG preservation and Reduced Motion stability are asserted in Unity.
- All 9,325 protected files retain their starting hashes.
- Existing 360×640 crowding/camera warning, legacy tile appearance and tall-safe-area backing are baseline behavior. No Android build or physical-device claim.

## Reproduce

Scripts/BuildNative.ps1 uses LibreSprite on working copies in a fresh output folder and reopens every document. Tools/Test-ForestFoundation.ps1 -Filter ForestFoundationPlayTests.SnowyMountainStage08AtmosphereReview -Graphics captures the temporary assembled scene. Scripts/Validate.py checks native integrity, source preservation, all five views and all 96 animation frames.

Review.html shows the actual Unity loop, frame scrubbing, before/after and Reduced Motion views, native effect assets, and validation evidence. Stage 9 has not started.
