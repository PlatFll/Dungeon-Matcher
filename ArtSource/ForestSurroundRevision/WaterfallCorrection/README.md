# Forest waterfall correction

The previous waterfall animation had only five low-contrast single-pixel streaks and did not read clearly in the assembled scene. The new LibreSprite source has 24 native 40×144 poses with broader connected light/shadow ribbons. These move downward by one pixel per pose at 12 fps and close exactly after two seconds. Painting is restricted to the original bright water plane above and below the bridge; banks, trees and bridge pixels remain fixed.

Both long comet-wisp instances are inactive in a new copied scene. Their source artwork and the previous review scene are retained. No replacement wisps were added. The other light/foliage tracks are unchanged.

Scene: `Assets/_Game/Scenes/ArtPreview_ForestSurround_Waterfall.unity`.
Previous scene: `Assets/_Game/Scenes/ArtPreview_ForestSurround_Living.unity`, unchanged.
Source: `Native/Waterfall_Flow.aseprite`; all native exports are in `Exports/`.

## Verified

- `Tools/Validate-Unity.ps1`: passed with Unity 6000.3.19f1.
- `ForestWaterfallRevisionTests.WaterfallReview`: passed in Unity with 96 captures of each version at matching times.
- Pixel comparison: no changes outside the water and removed comet regions. All remaining motion, characters, board and HUD pixels match at each sampled time.
- All 24 water poses are visible, loop exactly in actual Unity water pixels, and reopen/export from LibreSprite with exact authored pixels and binary alpha.
- Uniform native scale, viewport clearance, Point filtering, no mipmaps and Reduced Motion checks pass. 1440×2720 is exact pixel doubling of 720×1360.
- 6,982 prior files remain hash-identical. Scene audit preserves every old object and changes only the two comet active flags, the water sprite and its track/frame references.
- No production gameplay integration, Android build or physical-device testing is claimed. No PixelLab calls or credits used.

`Review.html` contains actual Unity recordings and a looping water-only detail. The eight-second scene recording is not advertised as a seamless loop for the independently timed lights. Full raw frame pairs remain under `.utmp/forest-waterfall/` locally.

Draft PR #209 remains unmerged. Stop for user review.
