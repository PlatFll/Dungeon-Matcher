# Forest Hybrid — Stage 3 board-frame review

Stage 3 complete; STOP for visual approval before Stage 4. No merge.
Branch: feature/forest-zone-hybrid-rebuild; draft PR #206.
Stages 1 and 2 are approved and remain byte-identical.

## Delivered

15 native PNG modules and 16 editable LibreSprite documents: four 23×23 carved corners, four 11-pixel-wide repeatable rails, an 11×11 centered peg, a 23×64 repeating side post, padded cap and spreading foot, two ivy groups and a separate support brace. The additional native document contains the cleaned 128×128 PixelLab source. Corner and trim close-ups are crops from actual Unity screenshots.

The frame is dynamically assembled with the existing ResponsiveModularFrameFitter, unchanged. Its 560×560 outer rectangle surrounds a full 512×512 board, with a 24-pixel inner inset so even the corner blocks never enter a cell. Rails tile at one source pixel per logical pixel. Side posts, caps, feet, braces and ivy are separate sprites, not a flattened image. All four corner carvings stay upright for consistent lighting.

## Sources and preservation

The current Game.unity gem GUIDs provide the six unchanged 64×64 gem PNGs. The existing approved Dungeon Stage16 Board_Cell64.png supplies the quiet cell benchmark; no new cell is authored or resized. The older Forest presentation still maps to the shared ReferenceDungeon EmptyCell family through BattlePresentationSkin, while its theme fallback retains log cells. This study changes neither mapping. Actual zone integration and safe-area mapping are later work.

The approved Stage 2 wood/ivy palette and quiet timber backing are reused without editing their source files. The original 410×414 Forest frame was inspected (23,167 distinct RGBA values) and copied into Sources for provenance. Its wood/vine theme is simplified into clean modular native pieces.

One PixelLab Pro Flash 128×128 source cost 6 generations; Forest cumulative total is 21. The actual Rattlebones PNG was passed in style_image.base64 for outline/detail/shading, with the approved Forest wood palette supplied separately. The receipt records prompt, seed, reference/guide hashes, returned IDs and charged usage. No additional generation was needed.

LibreSprite authored/exported every native document. The source's large light planes were reduced from 7 to 6 opaque colors; its native 11×11 spiral carving is retained inside the new corner plate. Straight rails, clear post joins and support contours were authored locally using the same material ramps. Leaves were extracted at source resolution with connected outlines and padding. No resampling or anti-aliasing was introduced.

Preserved: all approved Stage 1/2 artwork, their scenes and evidence; production assets, scenes, gameplay, characters, gems, shared HP/bottom HUD/resonance/buttons/settings and zone bindings. All 1,729 protected hashes match. Only Stage 3 candidate files, its art-only scene/fixture, and the staging checkpoint are added/updated.

## Executed verification

- Tools/Validate-Unity.ps1: PASS on Unity 6000.3.19f1; Diagnostics/Compilation.txt records the log.
- ForestHybridStage03Tests.BoardNativeScreens: PASS (1 test, 0 failures).
- Actual Unity captures at 608×608, 1216×1216 and 1824×1824: exact integer-scale equality.
- Independent original-PNG composite matches every pixel of the Unity board opening.
- Frame-on versus frame-off captures: zero changed pixels inside all 64 cells.
- Empty-board capture provided to inspect the rim and separation from cells.
- Five shell sizes pass: 46×46, 560×560, 561×559, 640×512, 411×689. 45×45 hides safely. These are shell stress cases, not gameplay layouts.
- Every enabled image has native simple-sprite dimensions or native tiling, Point sampling, integer placement and in-canvas bounds.
- All standalone contours have padding; rail/post repeat endpoints match; binary alpha and zero transparent RGB verified.
- All 16 LibreSprite documents reopen/export pixel-identically.
- No production combat, Android or real-device test at this art-only stage.

## Artistic verdict and review limits

The quieter warm timber connects to the approved portrait while the bright gems remain dominant. The spiral knots add woodland identity without the mockup's dense leaf/flower clutter. Four small ivy placements stay outside the board. This is a review candidate, not automatic final art approval.

1. Lanterns, mushrooms, flowers and further magic accents remain Stage 4; none were added merely to fill space.
2. The full approved upper environment, portrait, board and shared HUD will be assembled together in Stage 5. This isolated board study does not change their placements.
3. Actual production responsiveness, safe-area fit and interactions require the later integration scope; the current tests certify native shell geometry and pixel clearance only.

## Reproduce

Run Scripts/BuildNative.py from the worktree root with Foundry Python; run LibreSprite -b --script ArtSource/ForestHybrid/Stage03/Scripts/AuthorNative.js. Run the repository validator after C# edits, then explicit Unity EditMode fixture ForestHybridStage03Tests.BoardNativeScreens. Reopen with Scripts/VerifyNativeReopen.js and run Scripts/ValidateAssets.py. Scratch is isolated under ignored .utmp/forest-stage03. Do not run a scene builder over an unsaved production scene.
