# Matching corner masonry correction

The user rejected replacing the original smooth side borders with rough stone. The original `Support_Corner_Left` and `Support_Corner_Right` sprites and every live side-wall object are now retained in the candidate view. Only two bottom-corner additions are placed over the existing environment; the masked portrait remains banner-free.

The two 58 x 88 additions were authored and exported in LibreSprite using the exact five-color ramp, 14-pixel courses, broad matte faces and restrained top/left highlights of the approved side stones. Their first two courses match the source pixels exactly. Larger lower courses use staggered joints to keep brick sizes consistent. They are distinct from the rejected rough PixelLab candidate, which remains archived as rejected history. No further generation calls or credits were used.

All source pixels are native, alpha is binary, and PNG-to-ASE-to-PNG round trips are exact. The outside edge connects to the existing side wall; the stepped inside edge and top/bottom remain complete. Original side-stone files are hash-identical. Imports use Point filtering, FullRect, 64 PPU and no compression or mipmaps.

The existing actual-Game capture fixture passed at 720x1280, 1080x1920, 1080x2400, a tall safe-area profile and 360x640. It checks original side-wall objects remain enabled, complete added-sprite bounds, portrait masking, visible gems, three live enemies and health/intent UI, Guide/Settings and an accepted swap. Board/section/health geometry exactly matches the pre-integration baseline. Screenshots were visually reviewed at normal and small sizes. These are Unity runtime checks, not physical-device testing; no new Android build was run.

The corrected assets are an editor-only visual-review candidate; production resources remain unchanged. Editable sources, native exports, captured screenshots and evidence are in this folder. Draft PR #205 remains unmerged.

`Tools/Validate-Unity.ps1` passed with Unity 6000.3.19f1, exit 0. The preservation audit confirms 5,298 protected files and eight existing main-checkout edits remain unchanged. This revision reran one five-profile live-scene fixture; earlier regression results were not rerun.
