# Ali's edited main-menu background

The user edited and saved `MenuBackground-Ali.aseprite` in LibreSprite and
authorized its production integration and merge on 2026-11-08. This directory
preserves that exact layered document, its lossless LibreSprite PNG export,
and the production-ready native exports. `manifest.json` records hashes.

`Hall.png` contains the complete 360x640 user-authored composite, unchanged.
The first 640 rows of `HallTall.png` are identical. Its additional 160 rows
continue the quiet paving and carpet by copying the original [0,480,360,640]
floor section at [0,640]; no source pixels are rescaled or recolored.
The editable original remains 360x640 and retains all layers.

Production continues to load `UI/GlobalMenu/HallTall` through `MainMenuArtwork`.
Both existing texture GUIDs and their Point/uncompressed/unmipped import settings
are preserved. The character idle, wordmark, settings, live labels, buttons,
navigation, saved runs, gameplay and zone artwork remain separate and unchanged.
`Tools/Build-MenuArt.ps1` now reapplies these approved sources after its historical
constructor so a future rebuild cannot silently restore the older hall.

Earlier background versions remain in Git and their existing source directories.
No image-generation calls or credits were used for this integration.

Verification results and actual Unity captures are recorded in `Validation.md`.
