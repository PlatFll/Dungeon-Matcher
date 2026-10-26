# Stage 08 — restrained marine details

Complete as a review candidate. Stage07 accepted by the user's Continue. Stage09 not started. No merge or production integration.

## Selection and placement

The accepted kit already supplies branching coral, kelp, shell/barnacle groups, a starfish and the lower-right relic. Duplicating those families would add clutter. One new compact tube-sponge/coral pocket fills a quiet right-column interval. Its common stone foot overlaps supporting masonry; no floating white dots, repeated mirrored colonies or added banners.

PixelLab supplied a 64×64 transparent source with a 24×29 occupied shape. LibreSprite lowered only the blue stone highlight and copied every selected pixel at 1:1 into a 28×33 canvas with two-pixel padding. The source silhouette and palette roles remain intact. Three mauve cup forms, a small coral fork and grouped shell collars form one connected prop. The 64×64 study remains separate.

A free LibreSprite trident-fragment trial was **rejected from assembly**: it read as a tiny glyph rather than convincing weathered stone. Its editable source and unused candidate export remain explicitly marked REJECTED for review history. Only the sponge pocket is placed. No new skull, starfish, kelp or large coral generation was needed.

## Verification

- Tools/Validate-Unity.ps1 passed with Unity 6000.3.19f1. Full log: QA/Stage08/UnityValidation.log.
- 19 actual Unity art-only captures include before/after/isolation at 360×640, 360×800, 720×1280, 720×1600, 1080×1920 and 1080×2400.
- Three editable-source roundtrips exact, including the rejected study. Two selected assets, one placed detail. Binary alpha; 14 combined opaque colors; exact Unity import pixels. Point, FullRect, 64 PPU, uncompressed, unmipped and no NPOT resampling.
- Exact native 1×/2×/3× pixel mapping: 14,392 samples. Every pocket pixel visible; zero frame overlap or clipping. 356 native pixels overlap supporting stone, including 38 foot pixels in both layouts. Source silhouette and padded extraction are exact; final shape is one connected component.
- 402,044 existing frame pixels unchanged. Before renders equal accepted Stage07 exactly; all changed pixels belong to this one new pocket. Board cells, portrait, top scene, bottom reef/HUD and left side remain pixel-identical. Existing right-column pixels under the new ornament are intentionally occluded; the approved sprite files are unchanged.
- 829 accepted Stage01–07 files, 7839 protected production/settings files and five authority/reference files byte-identical. Existing serialized objects preserved; only the new scene's canvas child list gains the additive detail root.

These are static art-only renders with the previously accepted empty board cells. No gameplay, animation, AIR/flooding, pause/restore, safe-area interaction, Android or physical-device verification. Existing board scaling and open tall-preview spacer regions were preserved.

## Cost and judgment

One PixelLab Pro Flash 64×64 job. **5 generations spent; 558.20 → 553.20 remaining; 89 cumulative.** Actual current Rattlebones PNG supplied via the supported style input for outline/shading/detail, without copying its anatomy or palette. Exact request, receipt, result and source/reference hashes are in RawPixelLab/Stage08/Lineage.json.

**Subjective stage score: 8/10.** The small cup shapes add a distinct reef form while leaving the architectural rhythm, negative space and interface hierarchy intact. This deliberately avoids adding every optional family. User visual approval remains pending.

Gallery: Preview/Stage08/Review.html. Scene: Assets/_Game/Scenes/ArtPreview_DrownedCourtV2_Stage08.unity. Native QA runner uses bundled Python with Pillow and NumPy.
