# Lighter gray shared controls

The user accepted the new stone shapes and requested the previous lighter gray.
This revision maps only native colors in LibreSprite, preserving all 13 source
dimensions, alpha masks, chipped contours, engraving shapes and placement.
The wave remains up to 176×48, Settings 56×56, supply tiles 64×64 and ability
plate/socket 176×66. The palette comes from MaterialRevision/Inputs, retaining
the older ability/supply main face gray #838091. The ready engravings restore
their previous colors and retain their existing animation and white glow.

[Review actual Unity screenshots and the native comparison](Preview/Review.html).

No PixelLab generation or credits were used. Approved shape sources are copied
unchanged to Before; editable ASEs, exact exports, reopened exports and the
actual LibreSprite pixel-remapping script are included. QA/NativeValidation.json
records color maps, exact mask checks, bounds and source/export hashes. There
are no runtime, gameplay, layout, importer or scene changes in this revision.

Actual Game evidence covers all five zones and four existing screen profiles.
Previous screenshots are preserved in MaterialRevision; only the new captures
are stored here. This branch remains unmerged, on PR #221.

Executed verification: StoneBattleUiTests.After passed all 20 actual Game cases
(five zones × 720×1280, 1080×2340, 360×640 and 1080×2400 simulated safe area).
Native source masks, dimensions and LibreSprite round trips are exact. All nine
protected starting files and all preceding review screenshots were restored
byte-for-byte after capture. Runtime C#, importer metadata and scenes have no
task changes. Art-only scope does not require the separate compilation validator;
the actual Unity play-mode capture suite ran successfully. No Android build or
physical-device test. Pre-existing compact camera warning remains visible.
