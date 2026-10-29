Snowy Mountain Stage 4 — original PixelLab ledge and continuous cobblestone wall
============================================================================
Current review: Revision01/Review.html. Stop here; Stage 5 has not started.
No merge and no production scene/theme binding changes.

The user preferred the original PixelLab snow cap, thin stone lip, icicles and
small stacked cobblestones. This revision restores that rendering and extends
the same stone family behind the board and bottom HUD, down to the screen edge.

Six modular native assets: five 64x84 ledge slices and a 154x52 cobblestone
repeat. 99.8363% of the original upper 84 rows remain exact. Only 44 pixels in
the originally empty right export column are filled to close a transparent seam.
The wall uses intact source stones with four staggered courses. Shading stays
on complete stones and repeats periodically. Palette: original 24-color family;
wall 17 opaque colors. Binary alpha; every ASE reopened to exact PNG pixels.
No new PixelLab generation or credits spent. Original source and prior revision
remain preserved. No resampling, blur, flattened scene or recolored ledge.

Unity assembly uses the actual Game scene, Rattlebones and three Frostwake
enemies. Source row 34 follows the existing enemy feet line; the thin stone lip
starts 10 native pixels ahead of the feet. The wall starts after source row 84
and repeats across the full width to screen bottom. The lower backdrop image
is hidden only inside the disposable preview so the new wall can show through;
bottom buttons, panel, text, frame, gems and all geometry remain unchanged.

Validation: Tools/Validate-Unity.ps1 passed. The actual Game-scene review passed
720x1280, 1080x1920, 1080x2400, inset-safe 1080x2400 and 360x640. New sprites use
uniform whole-pixel scale. Source pixels, upper screenshot pixels, portrait
interior, shared geometry and all 9,051 protected file hashes remain exact.
All lower left/right/bottom perimeter pixels belong to the new wall. A measured
one-byte linear/sRGB screenshot conversion is allowed in color membership only,
explicitly approved by the user; source artwork checks retain zero tolerance.
Changed board-interior pixels are only the new backing visible through existing
tile transparency. No gem, tile or UI source art was altered.

Unity Editor previews only; no phone test. Existing 360x640 camera warning and
crowded labels, legacy board/enclosure art and its fractional board mapping are
retained baseline issues awaiting their respective stages, not claimed fixed.

Editable sources: Native/*.ase. Exports: Exports/*.png.
Native rebuild: Scripts/BuildNative.ps1 creates a fresh output copy to avoid
LibreSprite overwrite dialogs; it never overwrites the review's selected assets.
Native checks: Foundry Python Scripts/ValidateNative.py.
Unity capture: Tools/Test-ForestFoundation.ps1 -Filter
ForestFoundationPlayTests.SnowyMountainStage04OriginalStoneReview -Graphics
Screenshot/preservation check: Foundry Python Scripts/ValidateCaptures.py.

Branch codex/snowy-mountain-pixellab; draft PR #218. Stop for visual review.
