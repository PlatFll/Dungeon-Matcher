# Forest Hybrid — Stage 2 portrait review

Stage 2 complete; awaiting user approval. STOP before Stage 3. No merge.
Branch: feature/forest-zone-hybrid-rebuild; draft PR #206.
Stage 1 was approved by the user's instruction to continue to Stage 2.

## Delivered

13 native PNG modules and 14 editable LibreSprite documents: four 15×15 corners, four repeatable 9-pixel-wide rails, a 7×7 connector, a 32×32 timber interior, two padded ivy clusters, and a 28×44 antler pennant. The additional editable document is the cleaned 96×128 source.

The current 135×258 Forest portrait reference rectangle is used in a separate 470×288 art study. Existing ResponsiveModularFrameFitter centers the pegs and rails without stretching their source pixels. The timber backing overfills by 32 pixels on every side and is clipped by RectMask2D. Ivy remains outside the clipping parent so its contours are complete.

Rattlebones is his unchanged native 64×64 ready sprite. The affinity indicator is the existing 32×32 emerald bound in EnemyWeaknessIndicatorConfig, not a resized board gem. The existing ModularHealthBarUI and Player style display at native dimensions; no shared HP code or art was changed. The preview uses the component's required PlayerHPBarFill child name so the value source cannot cover the native HP frame.

## Kept and changed

Every Stage 1 asset, source, scene and capture is byte-identical. Its modules are reassembled in the new study to the current Forest reference width. All protected production assets, scenes, guides, golden references and finalized shared UI are unchanged (1,155 hashes verified). The actual playable Forest retains its previous presentation. No gameplay, global HUD, settings, board frame or zone binding changed.

The older production Forest frame/backing/banner were inspected and copied into Sources as immutable design references. The frame has 9,708 distinct RGBA values and a noisy export; its wood/vine motif is retained in the new candidate without replacing that original.

PixelLab made one 96×128 frame source: 6 generations charged; Forest cumulative total 15. Actual Rattlebones PNG bytes were supplied as the supported style image. Prompt, seed, reference path and job receipt are in Sources/PixelLabJob.json. The generated white center was removed and its 16 opaque colors consolidated to 9 deliberate tones. Local native edits supplied straight repeatable rails, centered joints, backing and pennant. All final drawings were authored/exported and reopened in LibreSprite. No further generations were used.

## Executed checks

- Tools/Validate-Unity.ps1: PASS, Unity 6000.3.19f1; log location in Diagnostics/Compilation.txt.
- ForestHybridStage02Tests.PortraitNativeScreens: PASS (1 test, 0 failures).
- Actual Unity captures: 470×288, 940×576 and 1410×864; exact integer-scale image equality.
- Frame cases: 30×30, 128×224, 135×258, 163×287, 129×257. Corner/rail/connector centers aligned; 29×29 undersized shell hides safely. These test the shell, not content reflow at unusably small sizes.
- Backing tint probe: zero changed pixels outside the portrait mask; no overfill leakage.
- All 13 PNGs: binary alpha, clean transparent RGB, recorded dimensions/palettes, padded standalone contours, exact repeat endpoints.
- All 14 LibreSprite documents: exact PNG roundtrip.
- 1,155 protected file hashes unchanged.
- The preview scene is excluded from build settings and has no gameplay owners or input EventSystem.
- No Android build, real-device test or production combat validation at this stage.

During fixture verification, a Unity coroutine closure did not survive play-mode domain reload, and the provisional HP value child used the wrong lookup name. Both preview-only issues were corrected and the affected test/captures rerun. Production runtime code was untouched.

## Artistic assessment and review limits

The warm frame reads against the cooler forest, while the dark timber leaves Rattlebones and the existing gem clearly dominant. Wood shading is restrained and ivy does not cross the face or HP area. The antler motif follows the supplied mockup and remains a candidate for approval.

1. Board framing and additional forest props remain later stages.
2. Production safe-area mapping and live combat/UI interactions are not validated by this isolated study.
3. The frame's ornament density and pennant are ready for the user's visual decision; technical checks are not art approval.

## Reproduce

From the worktree root: run Scripts/BuildNative.py with Foundry Python, then LibreSprite -b --script ArtSource/ForestHybrid/Stage02/Scripts/AuthorNative.js. Run the repository validator after editor-code changes; run the explicit Unity EditMode fixture above; reopen with Scripts/VerifyNativeReopen.js; run Scripts/ValidateAssets.py. The scripts use ignored .utmp/forest-stage02 scratch. Run the scene builder only in this isolated worktree with no unsaved production scene.
