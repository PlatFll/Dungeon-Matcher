# Underwater polish revision 02

All six requested polish stages completed as a separate review scene. Visual approval is pending. No merge or production Game-scene binding changes.

## Changes

- Coral: six additive source copies; 16 detached/noisy coral pixels removed from affected board-side and lower-reef clusters. Connected silhouettes and all other source pixels are retained exactly.
- Portrait interior: two quiet dark blue masonry tile variants are supplied; the scene uses variant A inside the existing overfilled mask. Frame, Rattlebones, placement and panel dimensions are unchanged.
- Lower background: seamless 64×32 submerged stone tiles fill behind and below the board. Two sparsely placed growth tiles add attached kelp without obstructing controls or text. The original flat backing remains in the prior scene.
- Bubbles: three native cels each at 8×8 and 12×12. Twelve staggered paths rise on whole pixels, drift gently and pop across the top arena, board gutters and lower environment. Paths use two different periods and varied visibility windows.
- Clam: one PixelLab source, cleaned into five 37×38 LibreSprite poses with a fixed foot. The small lower-left prop opens, holds and closes over 12 seconds, releasing two bubble trails while open.
- Final polish: the clam moved four native pixels left after a two-pixel text overlap was detected at 720×1280. Unity sprite keys initially extended the animation periods by one frame; the final clips have verified exact 8/12-second cycles.

## Preserved scope

27,228 pre-existing tracked files remain byte-identical. All 1,113 serialized blocks from Revision01 remain in the new scene. Existing changes are confined to seven approved sprite bindings, hiding the replaced blank lower backing, enabling the existing actor review overlay, and adding two environment roots. No existing transforms changed.

Board cells, frame artwork, dimensions, actors, text and shared controls remain unchanged. The original tall-preview spacers and existing board scaling remain unchanged. No source is deleted or permanently replaced; all 21 candidate PNGs have separate editable ASE files.

## Executed verification

- Unity 6000.3.19f1 built the candidate and produced 306 real camera captures across 360×640, 360×800, 720×1280, 720×1600, 1080×1920 and 1080×2400.
- All six before captures equal Revision01. 3,148,840 protected actor/UI/frame/tile pixel samples remain identical in revised stills.
- All 288 native motion frames preserve protected pixels. Motion is present in top, left, right, bottom and clam regions. Both screen proportions return to exactly the same pixels after the full 24-second loop.
- Portrait magenta mask probes stay inside the existing panel opening. Tiles are opaque, with matching repeat seams. Native exports have binary alpha and match every LibreSprite roundtrip and Unity import.
- All 26 clips and 26 controllers loop. Exact clip lengths are asserted in the builder. Point sampling, FullRect, 64 PPU, no mipmaps and no texture compression are verified.
- Tools/Validate-Unity.ps1 passed after the final editor-code change. Full log: QA/Revision02/UnityValidation.log.
- Browser gallery opened and rendered the before/after view and moving 24-second preview.

The animation gallery samples the actual Unity AnimationClips into lossless PNGs; it is not a CSS motion simulation. The GIF is an indexed convenience preview; the PNG sequence/native assets retain exact colors. These are art-only camera/clip checks, not interactive combat, Android, physical-device or production reduced-motion/pause integration tests.

## Cost and review files

PixelLab: **5 generations** for one clam seed (job 7a4a16e1-426b-43a9-837d-5d792460c7ba). Balance 553.20 → 548.20; cumulative pack spend 94. Actual Rattlebones PNG supplied as rendering reference; guide and golden hashes verified. No other generation calls in this revision.

- Review: Preview/Revision02/Review.html
- Before/after: Preview/Revision02/Before-After.png
- Motion: Preview/Revision02/Underwater-Motion.gif and Motion/Frame-*.png
- Editable assets: Native/Revision02/*.ase
- Native PNGs: Exports/Revision02/*.png
- Scene: Assets/_Game/Scenes/ArtPreview_DrownedCourtV2_Revision02.unity
- Builder: Assets/_Game/Scripts/Editor/DrownedCourtV2Polish02Preview.cs
- Verification: QA/Revision02/Verification.json and SerializedScope.json

Formal pack stages 09–12 have not been advanced. This completes the separately authorized six-stage polish request and stops for visual review.
