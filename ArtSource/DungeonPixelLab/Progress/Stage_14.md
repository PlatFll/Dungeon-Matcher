# Stage 14 — DONE; stopped for review

The complete approved art library is assembled in `Assets/_Game/Scenes/ArtPreview_DungeonPixelLab.unity`. It remains an isolated art scene, with no characters, enemies, gems, input or combat owners and no release build-list entry. The 102 native candidates, all raw/LibreSprite sources, exports and import settings are unchanged. All 456 protected art-file hashes pass. No generation calls or new image editing occurred.

## Assembly and corrections

All 13 families are represented: general wall, portrait recess, shallow floor top, raised fascia, gate/arch/hardware, support stone, torches/light, chains/banners, clutter, portrait frame, board frame, top plaques, and bottom HUD. The 360×640 scene draws 383 modular Images using 91 library sprites and three existing UI symbols. Other edge/recess variants and inactive meter parts remain available in the 102-piece library; they are not falsely reported as visible. Nothing uses a flattened screenshot or render-texture background.

Far gate and pillars now render before the floor/fascia; props follow, then masked frames and UI. This repairs 279 junction pixels at the floor and corbel seams, without editing a source pixel or moving the approved standard composition. The saved scene retains all 1,682 object identities, adds/removes none, and changes seven serialized objects for parenting/order and automatic screen fitting.

The approved 360×640 proportions remain the baseline. Play-mode resizing uses integer scaling and extends logical height up to 800. Top scenery and plaques stay anchored to the safe top; lower HUD stays anchored to the safe bottom; the unchanged board centers in the extra vertical space. Existing full wall tiles and support repeats fill the extension, with complete footer pieces. Only repeating field tiles are cropped at viewport boundaries; complete props, frames and banners stay contained. The portrait backing overfills its separate interior mask.

This is a preview-only layout adapter following the current live layout owner's top/bottom anchoring and centered-board behavior. It does not fix or replace the production owner's wider minimum-width/fractional environment mapping recorded in Stage00. Direct production integration and phone validation are separate work. Safe areas smaller than the 360×640 minimum cannot display this native reference without clipping/scaling; this preview reports that limit instead of resampling the artwork.

## Executed evidence

- Actual Unity Game-view captures: 360×640, 720×1280, 1080×1920, 360×800, 1080×2400, 1080×2400 with 90px top/bottom insets, and 1084×2404 with an odd safe rectangle (2,91,1081,2221).
- Focused Unity test: 1 passed, 0 failed. It verifies auto-resize, exact native image dimensions, integer geometry, safe bounds, layer ordering, 64 board cells, masked overfilled recess, complete props and no forbidden gameplay owners.
- 2×/3× screenshots are exact nearest-neighbor copies of native captures, including full-height tall. Odd safe-area content exactly matches the aligned safe-area capture.
- All 88,932 visible board pixels, 9,438 visible bottom-HUD pixels and live label glyphs are unchanged after their tall-screen translation. Transparent gaps correctly expose different background rows; validation uses actual sprite alpha and panel masks rather than treating the wall as HUD artwork.
- Thirteen source-to-camera comparisons prove every opaque native source pixel matches its rendered crop, covering props, frame corners, floor fascia, board cell, gear and unchanged icons. See `Previews/Stage14/Source-vs-Unity.png`.
- All 102 current library PNGs match retained exports. Guide/register and actual current Rattlebones/Farmer/Pan reference and production hashes match the current register.
- Required `Tools/Validate-Unity.ps1` passed with Unity 6000.3.19f1 after the scene identity-preserving serialization pass. Final receipt records exact file/log hashes.
- Production `Game.unity`, build settings, shared control/layout code and all eight dirty user files in the main checkout remain unchanged.

The initial test harness needed an assertion compatible with the installed NUnit version and an import check that respects the locked UI symbols' existing 100PPU metadata. Those symbols use explicit native dimensions, which are tested. A full-rectangle HUD comparison also included transparent background; it was replaced by the stronger visible-pixel/mask comparison described above, with all coverage retained. Final results above are from the corrected tests.

## Visual mismatch review

| Category | Result / screenshot |
|---|---|
| Seams and layering | Gate/pillar-to-floor and corbel-to-fascia overlap repaired. `Top-3x.png` exposes these joints. |
| Silhouette / clipping | Whole banners, props and corners remain contained. Portrait backing fills its clipped chamber. |
| Floor perspective | One shallow horizontal top and vertical raised fascia; no isometric plane or repeated floor rows. |
| Tall extension | Continuous masonry replaces empty bands. Extra wall space above/below the fixed native board is deliberate; `Assembly-360x800.png`. |
| Rendering / values | Approved quiet violet walls, contrasting blue-dark cells, localized warm torch clusters and silver UI are preserved. |
| Optional later art polish | Wall repetition is more apparent in tall-screen gaps, and torch-light clusters remain the approved angular treatment. No new material defect required an unapproved redraw. These are review observations for Stage15 only if requested. |
| Production mapping | Still unresolved outside this art-only scene; the Stage00 live-layout caveat remains. No phone-build or gameplay-integration claim. |

Review: `../Review.html`. Earlier sources and galleries remain archived. Stage14 spend: **0**; cumulative staged spend **65**. Last verified balance **989.20** is historical from Stage13, not a new balance query. STOP; Stage15 remains NOT_STARTED. No merge.
