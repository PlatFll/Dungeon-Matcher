# Stage 07 — unified lower reef corners

Review candidate complete. Stage06 accepted by the user's Continue. Stage08 remains unstarted. No production integration or merge.

## Result

One large connected 80×96 reef formation per side: broad teal boulders and ruined masonry, rooted kelp, muted pink coral and sparse shell accents. The left has an attached starfish; the right contains a partly buried stone skull/relic. Both formations join the accepted side columns and sit on a quiet six-pixel seabed. There are no separate stacked rock islands or floating coral pieces.

Seven assets: LeftReef, RightReef, RuinConnector (an unchanged native copy of the accepted shaft), two seamless seabed lengths and two small rubble transitions. The standard preview uses 20 modules; tall uses 26. Extra shaft modules fill the tall-screen distance without stretching artwork. All source silhouettes and 80×96 canvases were preserved. LibreSprite cleaned only warm salt flecks on the new masonry and authored the supporting native pieces. The right formation moved down three native pixels after QA caught a single cap pixel behind the board frame; final clearance is zero overlap at every size.

## Executed checks

- Tools/Validate-Unity.ps1 passed, Unity 6000.3.19f1. QA/Stage07/UnityValidation.log contains the complete log. The final placement-only correction was subsequently recaptured by Unity; C# remained unchanged.
- 19 real Unity static art-only captures: before/after/isolation at 360×640, 360×800, 720×1280, 720×1600, 1080×1920 and 1080×2400, plus layers-only composition.
- Seven LibreSprite sources re-export identically; Unity source PNGs and imported opaque pixels/alpha are exact. Point, FullRect, 64PPU, uncompressed, unmipped, no NPOT resampling. Binary alpha; 17 meaningful colors across the family.
- All seven assets are connected; exactly two main reef masses and two continuous reef-to-column structures. Native seam checks pass. Both corners overlap supporting masonry and the seabed in standard and tall layouts.
- 323,932 rendered new pixels match exact native 1×/2×/3× compositions. No asset is clipped or hidden behind the frame. 402,044 frame pixels and 277,480 accepted side-art pixels stay unchanged. The central bottom HUD rectangle remains pixel-identical. Differences are restricted to the lower environmental sides and seabed.
- 733 accepted Stage01–06 files, 7839 protected production/settings files, and five authority/reference files are byte-identical. Only the original canvas child list changes in the new scene; existing serialized objects are preserved.

This is a static art-only scene, not playable integration. Cells are shown without gems as in the accepted preview. No animation, gameplay, AIR/flooding, pause/restore, safe-area interaction, Android or device validation is claimed. Existing cell mapping and the tall preview's open spacer regions remain unchanged by this focused stage.

## Cost and artistic review

Two original PixelLab Pro Flash 80×96 sources, five generations each. **10 spent; 568.20 → 558.20 remaining; 84 cumulative.** Current actual Rattlebones PNG provided through the supported style input for rendering only. Native reference hashes, prompts, receipts, final charges and source hashes are in RawPixelLab/Stage07/Lineage.json. No regeneration was required.

**Subjective score: 8/10 for this stage.** The corners now read as weight-bearing underwater ruins with attached marine life, closer to the reference's coherent masses. The brighter relic remains small; the shared HUD remains dominant. The native result is quieter and less crowded than the screen-resolution mockup. Final artistic approval belongs to the user.

Gallery: Preview/Stage07/Review.html. Native sources: Native/Stage07. Scene: Assets/_Game/Scenes/ArtPreview_DrownedCourtV2_Stage07.unity. QA runner uses the bundled Python runtime with Pillow and NumPy, not the minimal Foundry venv.
