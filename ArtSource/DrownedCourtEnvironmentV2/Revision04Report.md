# Drowned Court Revision04 — floor continuity and living details

Status: **candidate for user review**, on the existing branch / draft PR #212. No merge or production binding.

## Requested changes

- Removed the artificial horizontal divider between the rear and front floor courses. Twelve new native module copies join the surfaces while retaining vertical stone joints and the outer platform lip. Sixteen scene tiles use these copies; floor geometry and enemy registration are unchanged.
- Replaced the compressed old closed-clam pose with a new rounded, ribbed PixelLab shell. PixelLab generated the still and hinge motion. LibreSprite cleanup removed an erroneous upper-lid pearl/socket, seated the pearl in the lower bowl, stabilized the lower 17 rows and reduced interpolation shades to 11–14 purposeful colors per frame. The 64×64 canvas preserves native pixels; nine opening poses reverse for closing, with a long closed rest and brief open hold.
- Two integrated coral modules now sway one native pixel at their upper tips. Their lower attachment pixels and stonework stay fixed. The two tracks have different 12/24-second timing with long still periods. All other corals and existing bubble tracks remain unchanged.

## Source preservation and scope

All existing PNG/ASE sources, animation clips, controllers, prefabs and review scenes are preserved. The new scene is `Assets/_Game/Scenes/ArtPreview_DrownedCourtV2_Revision04.unity`, excluded from builds. `Game.unity`, production presentation code, gameplay, characters, gems, UI layout and prior enemy-floor alignment are unchanged by this revision. Formal stages 09–12 have not advanced.

Rattlebones remains the rendering authority, with the supplied clam images used for bivalve shape. Input hashes are in `QA/Revision04/StartingState.json`; requests, receipts and rejected trials are retained in `RawPixelLab/Revision04/Generation.json`. Reference images are not production assets.

## PixelLab usage

Starting balance: 548.20 generations. Ending balance: 535.20. **13 generations spent**: 10 for the 16 still candidates, plus three animation jobs at 1 each. First two motion trials were rejected for pearl drift, overextension and unwanted particles. The third supplied the selected hinge motion; native cleanup remains documented in the LibreSprite scripts. Cumulative pack spending: **107 generations**. No OpenAI image-generation calls.

## Verification

- Required `Tools/Validate-Unity.ps1`: PASS, Unity 6000.3.19f1.
- Actual Unity art-only captures at 360×640, 720×1280 and 1080×1920, both closed and open.
- 388 existing layout rectangles preserved; 334 unaffected sprite bindings preserved.
- 5,847,296 protected pixels compared exactly across before/after screenshots; no changes outside the authorized floor, two coral regions and clam canvas.
- Native PNG import pixels verified against the exported files. Binary transparency, palette counts, clam canvas clearance and fixed lower-shell registration passed.
- 288 actual Unity samples at 12 fps; the complete 24-second ambient cycle returns to identical pixels.
- Prior 25-case production enemy-floor evidence remains in Revision03; runtime source is unchanged. This revision does **not** claim new production gameplay, Android or real-device testing.

## Review and reproduction

- [Synchronized before/after Unity gallery](Preview/Revision04/Review.html)
- [Lossless actual Unity motion](Preview/Revision04/Unity-Animation.webp)
- [Native cleaned clam strip](Native/Revision04/Clam-FrameStrip.png)
- [Validation details](QA/Revision04/Verification.json)
- Editable individual ASE files and timing JSON: `Native/Revision04`.
- Rebuild the Unity review using **Dungeon Matcher → Art → Drowned Court V2 → Build Life Revision 04**.

Stop for user visual approval. Do not merge.
