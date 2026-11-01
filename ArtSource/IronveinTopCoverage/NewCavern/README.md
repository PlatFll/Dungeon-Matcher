# Expanded native Ironvein cave — review candidate

The user rejected the first zoom-to-cover implementation and explicitly
requested a new similar PixelLab backdrop sized for the whole upper screen.
The original cave and Rattlebones reference were approved for upload. The user
then also approved the prepared 480×352 outpainting canvas and its edit mask.

## Native construction

The canvas is 480×352, sized against the measured phone/safe-area extents of up
to 400×311 native pixels. LibreSprite places the original 240×160 cave at
(180,192), without resampling. PixelLab generates only the surrounding ceiling
and walls; the core is locked by the mask. The result is reopened in LibreSprite
for generated-region palette cleanup, native ASE/PNG export and exact roundtrip
verification. The original cave pixels must remain exact.

The runtime uses the normal environment pixel scale, bottom aligned behind the
existing walking surface and centered on the physical screen. Extra ceiling
and side margins provide coverage; no zoom-to-fit remains. Characters, board,
portrait, controls, gameplay and other zones retain their existing owners.
Only the optional backdrop binding is added to the existing Ironvein assembly.

## PixelLab lineage and spending

- First Pro generation: `b6f0411e-7058-4991-8cf2-ece996b6d72d`, 25 generations.
  Rejected: enlarged features and flat letterbox strips. Preserved under Raw.
- Masked native expansion: `1c0fdcf8-aa09-4e78-be13-02117ab7a63d`, 25
  generations actually used. Source is the approved original, not the rejected generation.
- Actual input PNGs, mask, requests, hashes and receipts are retained here.
- Existing production PNGs are never overwritten; this is an additive candidate.

## Verification

- Native export: 480×352, 20 opaque colors, alpha 255 throughout. Original
  240×160 pixels remain exact; reopened LibreSprite ASE exports the same pixels.
- PixelLab spending: 50 generations total (25 rejected + 25 selected); 4,788 remain.
- Actual Game capture test passed: eight Ironvein sizes/safe-area cases plus
  Dungeon, Forest, Court and Frostwake at 1080×2340. All 12 exact baseline
  snapshots retain UI placement, enemy floor positions, board geometry and other
  existing scenery. Guide/Settings functionality also passed.
- Full cave coverage, normal integer native scale and locked original core are
  asserted in Unity. No zoom-to-cover or interpolated scenery pixels.
- `Tools/Validate-Unity.ps1` passed with Unity 6000.3.19f1. Logs and XML are in QA.
- Visual review checked the ordinary, short and tall/safe-area Game captures:
  connected cave ceilings replace the repeated upper wall; characters remain
  readable. The generated upper cavern adds depth, while the original passage
  and lower tunnel remain intact. Final aesthetic approval remains with the user.
- The pre-existing 360×640 reference-resolution warning/shared compact layout
  remains visible in its editor capture; this is not an overall UI layout redo.
- Nine unrelated settings/assets were restored byte-for-byte from the task's
  initial backups after all Unity jobs. No original art asset was overwritten.

Evidence: `Preview/After`, `Preview/Review.html` and `QA/Validation.json`.
Original pre-fix captures remain under `../Preview/Before`; rejected zoom
captures stay in `../Preview/After`. No Android build or physical-device test
was run for this revision. Nothing is merged.
