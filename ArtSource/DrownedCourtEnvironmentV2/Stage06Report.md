# Stage 06 — side columns and anchored marine growth

Complete as a review candidate. Stage05 accepted by the user's Continue. Stage07 not started; no merge or production integration.

## Result

Two continuous stone columns flank the existing board, with broad teal block planes, sparse fluting, small ledges, two kelp holdfasts, two coral colonies, two tiny barnacle patches and two integrated dark vent outlets. Left/right growth is vertically staggered rather than wallpapered. No hanging banners or bubbles were added.

**17 native assets; 12 distinct sprites placed as 34 modules.** The kit has a cap/base, three 31×32 shafts, a 31×18 short shaft, two shelf modules, 31×68 kelp, 33×57 coral, a 9×13 barnacle patch and an 11×11 vent. Three cleaned raw studies and two registered kelp tip/stem pieces are retained. The overlapping tip/stem layers recompose exactly; motion is deferred to Stage09.

Pillar modules meet at identical 14-pixel-wide shaft profiles. The planted root/stalk contacts overlap their shelves. The coral and kelp outer branches were shifted inward by three native pixels, and the column foot wings shortened, so all placed opaque pixels clear the approved frame. This was native LibreSprite editing, with no image resampling. The raw kelp reached its lower canvas edge; the final module adds transparent padding and a completed dark contact contour.

The existing board opening, tile pitch, 64 cell bindings, gems, portrait, characters, top environment and shared lower HUD are unchanged. Side modules use uniform 1×/2×/3× native scale and follow the established tall-board offset. They remain separate Unity Images behind the unchanged frame/cells. The scene is excluded from build settings.

## Executed verification

- Tools/Validate-Unity.ps1 passed with Unity 6000.3.19f1; log in QA/Stage06/UnityValidation.log.
- 19 actual Unity art-only camera captures: before/after and side-only isolation at 360×640, 360×800, 720×1280, 720×1600, 1080×1920 and 1080×2400.
- 17 exact LibreSprite .ase roundtrips; Unity PNG files byte-identical and imported opaque pixels/alpha exact. Point, FullRect, 64 PPU, uncompressed, unmipped, no NPOT scaling. 17 opaque colors across the material family, binary alpha.
- 25 legal cap/shaft/base seam pairings pass. Exactly two connected stone masses. Eight documented attachments have positive overlap with supporting stone; no floating final prop components.
- 277,480 rendered side pixels match the native composition exactly at 1×/2×/3×. **Zero side pixels hidden by the frame** at all six sizes. All placed opaque bounds stay inside the viewport and outside the gem grid.
- 402,044 existing frame pixels remain unchanged. Before captures exactly match Stage05 at every size; differences are confined to the two authorized board gutters. Top environment, portrait, cells and lower UI remain pixel-identical.
- **576 accepted Stage01–05 files**, **7839 protected production/settings files**, and five current authority/reference files are byte-identical. Only the original canvas child list differs in the new scene; all old serialized objects are preserved.

These are static art-only renders, not a playable zone integration. No new gameplay, animation, AIR/flooding, pause/resume, safe-area interaction, Android or physical-device tests are claimed. The existing fractional mapping of the production Court cells remains unchanged; this stage verifies the new side artwork's native mapping only.

## PixelLab and art judgment

Three original Pro Flash jobs: pillar 64×128 at 6 generations, kelp and coral 64×64 at 5 each. **16 generations spent; 584.20 → 568.20 remaining; 74 cumulative.** Actual current Rattlebones PNG supplied as the rendering reference, with anatomy and palette excluded from copying. Exact requests, receipts, costs, source hashes and authority hashes are in RawPixelLab/Stage06/Lineage.json. No regeneration was needed.

**Subjective art score: 8/10 for this side-architecture stage.** The continuous pillars support the Court's royal ruin identity; muted growth adds marine character while keeping the board quiet. The composition is intentionally calmer than the fully dressed mockup. Rich lower reef masses and ambient motion remain later stages. Candidate status awaits the user's visual approval.

Gallery: Preview/Stage06/Review.html. Native attachment proof: Preview/Stage06/Attachments-Native.png. Exact checks: QA/Stage06/Verification.json. Scene: Assets/_Game/Scenes/ArtPreview_DrownedCourtV2_Stage06.unity.
