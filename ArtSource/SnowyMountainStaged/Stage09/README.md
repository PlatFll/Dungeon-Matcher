# Snowy Mountain Stage 9 — final assembly review

All approved Stage 2–8 art is assembled in the actual Game scene for final visual review. Rattlebones, three live Frostwake enemies, actual gems, shared controls and the unchanged 8×8 board are present. Production zone bindings are untouched. Nothing is merged.

## What was kept and polished

The alpine pass, portrait kit, approved snow ledge, muted cobblestone continuation, snowy timber/iron board frame, lantern/banner side supports, grounded lower corners and subtle atmosphere all retain their native artwork. Portrait masking, shared board dimensions, actor floor, health bars and control placement remain owned by the existing systems. The enclosing top/bottom HUD frames stay removed.

Final safe-area review exposed an old cave-texture strip above the 384×320 vista. A separate 384×96 native sky/cliff extension now fills that margin. It was authored and exported in LibreSprite using only the approved vista's colors; its bottom row exactly matches the vista's top row. The approved vista is unchanged, at the same scale and position. The extension is a new Stage 9 review candidate. No PixelLab generation was needed.

## Review outputs

- Review.html: full assembled Game screenshot, live playback, target comparison, all five viewport sizes and top-margin before/after.
- Assets.html and ASSET_LIST.md: 56 separate native assets and their placements; original editable sources remain in their owning stages.
- Preview/LiveAssembly.webp: 96 real Unity screenshots with live character idles and existing gem-hint motion; an 8-second excerpt that repeats for review, not an authored character loop.
- Preview/Atmosphere.webp: isolated 12-second ambient loop; combat frozen only for exact paired comparisons.
- QA/Assembly-*.json: measured native source dimensions, sprite names, groups, sorting and screen bounds from the actual scene.

## Validation

Unity compilation and SnowyMountainStage09AssemblyReview passed at 720×1280, 1080×1920, 1080×2400, inset-safe 1080×2400 and 360×640. All 56 LibreSprite sources reopen/export pixel-identically. All 9,485 protected files remain unchanged. Both WebP previews retain the captured PNG pixels losslessly.

The frozen atmosphere pass verifies the board/frame, portrait, controls, bars and center HUD in every one of 96 frames. Live playback verifies all three enemy idles advance, the floor remains fixed, and board cell occupancy, gem identities, sprites, scales and colors remain unchanged. The first live check incorrectly treated the game's intended hint nudge as a mutation; the corrected check preserves that existing motion and verifies logical state separately. The failed first-run evidence is retained.

## Limits and approval gate

360×640 still has preexisting shared HUD crowding and Unity's development/editor pixel-camera warning. This final art pass does not redesign the shared UI or change cross-zone board sizing. The existing gem-cell artwork and board sampling also remain unchanged. These are visible limitations, not a phone-readiness pass. No Android build or real-device test was performed. Production registration and that shared layout follow-up remain separate work after visual approval.

Artist verdict: the selected pieces now form a coherent cold alpine scene. Snow caps read as accumulation, cold iron/timber frames remain sturdy, side lanterns provide restrained warm accents, and the lower corners are grounded without obscuring the center controls. The target mockup guides identity/materials; the actual 8×8 board and approved shared controls intentionally retain the game's layout.

## Reproduce

Run Tools/Test-ForestFoundation.ps1 -Filter ForestFoundationPlayTests.SnowyMountainStage09AssemblyReview -Graphics from this checkout, then Scripts/Validate.py. To reproduce the separate sky candidate without overwriting artwork, run `Scripts/BuildSky.py --libresprite <path-to-libresprite.exe>` with the Foundry Python environment. It writes to a fresh temporary folder and compares exact PNG pixels. Stage 9 reuses the accepted assembly helpers rather than creating a second runtime system. All mutations are temporary and restored by fixture cleanup.

Stopped after final Stage 9 review. Draft PR #218 remains unmerged.
