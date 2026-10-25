# Snug Forest frame and shared zone sizing

The 512 x 512 Forest tile opening is unchanged. The old frame used a 24-native-pixel inset although its 23px corner/11px rail geometry needs 17px, leaving 14 screen pixels of empty space per side. `ResponsiveModularFrameFitter.FollowOpening` now derives the target from the actual opening. The trailing foliage moved six native pixels down so it cannot cover a tile. No PNG was edited.

The actual Game scene formerly sized its board and portrait from zone padding, minimum battle heights and skin registrations. One shared layout contract now preserves the Dungeon board sizing across all five zones. Portrait rectangles use the same registration. Legacy player rails repeat native pixels horizontally; skin ornament/rail thickness remains distinctive. This is equal playable opening and portrait slot geometry, not identical leaf/stone silhouettes.

The approved Forest assembly remains art-only. The shared geometry code is exercised in actual Game scenes using each existing production zone skin. New Forest production-art integration is not claimed or authorized here. Main and primary checkout remain untouched; PR #207 stays draft and unmerged.

## Verification

- Repository `Tools/Validate-Unity.ps1` completed successfully with Unity 6000.3.19f1.
- Corrected Forest preview test passes at 720x1360 and 1440x2720; all four rail-to-tile gaps are zero and the second capture is exact nearest-neighbor 2x.
- Tiles/gems, top assembly and entire lower HUD crop are pixel-identical to the approved Checkpoint 5 capture.
- 25 actual Game cases pass the complete presentation validator: five zones at five viewport/safe-area profiles, three enemies each, including each zone's apex where available.
- 20,999 pre-existing protected files retain exact hashes. Four automatic Unity serialization files are restored to Git HEAD. No generations or source art edits.
- At 360x640 only, the legacy HUD controls move nine logical pixels up, narrow HP labels use the existing compact treatment, and the optional instructional caption hides in legacy zones (the approved Dungeon hint remains unchanged) to avoid covering the controls. Normal lower HUD placement/art is unchanged. Board fitting remains uniform fractional on narrow displays, matching the existing approved policy.
- The final compact-only recheck passed all five zones after preserving the existing Dungeon hint. The 360px images include Unity Editor's below-reference-resolution warning overlay.
- No Android build or physical-device test was performed.

| Viewport | Playable board, screen pixels | Portrait slot, screen pixels | Matching zones |
|---|---|---|---|
| 1080x1920 | 814.495 x 814.495 | 292 x 562 | 5/5 |
| 720x1280 | 545.099 x 545.099 | 195 x 406 | 5/5 |
| 1080x2400 | 814.495 x 814.495 | 292 x 812 | 5/5 |
| 1080x2400-safe | 796.475 x 796.475 | 286 x 806 | 5/5 |
| 360x640 | 241.465 x 241.465 | 97 x 230 | 5/5 |

Three older `ForestVisualPlayTests` remain failing and were **not weakened**: the old environment ID assertion expects `forest-woodland` instead of the existing `presentation-forest`; two legacy move-intent assertions require the label bottom above the sprite, while the existing presenter centers it at the sprite top (16 screen pixels of overlap). All three failures were reproduced on unchanged HEAD with this fix temporarily removed. `Diagnostics/LegacyBaseline.xml` and `Diagnostics/FinalMatrix.xml` preserve that evidence. The current three-enemy sizing checks use the real current zone launch and full presentation validator.

Review [the gallery](Review.html), [measured geometry](Corrected/Geometry.json), [machine report](REPORT.json), [final Unity tests](Diagnostics/Verified.xml), and the isolated scene `Assets/_Game/Scenes/ArtPreview_ForestReworkV2_FrameFit.unity`.
