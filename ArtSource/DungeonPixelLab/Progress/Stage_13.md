# Stage 13 — DONE; stopped for user review

The bottom HUD now has a native modular resonance/ability panel, pale matching supply shells, a compact progress rail and live PHASE RESONANCE label in the isolated Unity review scene. Stage12 and all earlier artwork remain unchanged. No Stage14, production integration, phone build or merge.

## Art and lineage

One new 64×64 PixelLab silver-purple frame source, seed 913130, job 59d060d1-6e84-4140-bfaa-0849198412a3. The actual current Rattlebones PNG was supplied through style_image.base64 for outline/detail/shading, without palette copying. Farmer, Pan and current UI were inspected locally; this endpoint takes one style image, so supporting images were not claimed as transmitted.

LibreSprite cleaned the source's connected silver planes, native bolt/corner vocabulary and straight rails into 23 components: nine main frame parts (four 9×9 corners, four 5px rails, 7×7 connector), dark and pale 16×16 insets, a 19×17 violet ornament, nine compact progress frame parts (5×5 caps/connector, 3px rails), a dark track and a four-row purple fill. Native source rows guide rail profiles; broad corners are regularized around centered gold bolts. No resampling or blurred glow. Editable sources, raw source and exact exports are retained.

The 24×24 Potion/Bomb and 28×32 GlobalMenu CrownedSkull symbols are bound directly from their existing approved files. No character generation, icon redraw or icon palette change. Exact source hashes and roles are in Stage13_ReusedSymbols.json.

## Layout and preview behavior

The main panel is 118×44 at (121,563); the two supply shells are 47×43 at (69,564)/(245,564). These are rounded registrations from the existing production layout capture into this branch's established 360×640 preview. The compact rail is 98×14 at (131,545). Production hit rectangles and control semantics are untouched.

Panels reuse the existing native modular fitter. Backings overfill anchored interior masks. Native icons recenter on odd/even resized shells without scaling. The live bitmap label is separate from the art. The progress bar clips on integer pixel boundaries; default is empty, and half/full screenshots are explicitly marked test states. No fake charges, percentages or gameplay values are baked into any asset. No gameplay/input owners exist in this art scene.

The approved continuous wall and Stage06 support masonry remain visible around and beneath all controls, with no added black wedges or disconnected corner fragments.

## Executed verification

- Unity preview test passed: 1 test, 0 failures. Actual 360×640, 720×1280, 1080×1920 and tall 1080×2400 safe-area captures.
- All 23 ASE cels equal their exported PNGs and Unity copies. Binary alpha; 1–7 meaningful opaque colors per component; Point/FullRect/64PPU, uncompressed and unmipped imports. Exact palettes/dimensions/bounds in NativeChecks.json.
- Complete corners and native rail geometry, panel noncollision, overfilled masks, unchanged icon dimensions and centered icons checked. Supply resize cases include 46×44, 71×65 and 38×38; meter cases 99×15, 133×17 and 48×12.
- Empty/half/full and out-of-range meter input clipping checked. At nominal width the fill is 0/45/90 pixels. Fill is a presentation fixture, not a gameplay energy owner.
- Every pixel of actual 2×/3× captures equals nearest-neighbor native 1×; tall safe-area content equals the same 3× image.
- All 361 protected prior-art files retain their hashes. All 215,826 screen pixels outside the new bottom-HUD rectangles match approved Stage12 exactly, including the entire upper HUD and board.
- Scene review preserves all 1,459 existing object identities. 223 new bottom-HUD objects; no removals. Only the existing root module/manifest bindings and bottom layer child list change.
- Required Tools/Validate-Unity.ps1 result and final source hashes are recorded separately in ValidationReceipt.json. Main-checkout user files and production controls remain protected.

The first fixed-layout pass passed; code review then found icons did not recenter during shell resizing. Final preview-only native icon centering and additional containment/centering assertions resolve that edge case. No artwork changed during this correction.

## Verdict and spend

The cool silver rims, small warm bolts and quiet purple motif match the approved frame family; the unchanged symbols read cleanly at native scale. The center remains the focal control and the surrounding masonry stays continuous. This is a review candidate, not user approval or production-device verification. The inherited production-scaling caveat remains for the later integration stage.

Exactly five approved generations used; one paid job, no retry. Balance 994.20 → 989.20; cumulative staged spend 65. Source/request/receipt and ledger retained.

Review: ../Review.html. STOP. Next only when requested: PROMPTS/14_ASSEMBLE_ART_ONLY_UNITY_SCENE.txt.
