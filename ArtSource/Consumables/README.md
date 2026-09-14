# Consumable artwork

LibreSprite editable `.ase` files and deterministic LibreSprite JavaScript sources accompany the imported PNGs. The slot reuses the supplied Allstuff frame's clean corner; potion and Bomb were redrawn in its palette. No letters or quantities are painted into the sprites. Quantities and cooldowns are Unity text.

The icons have a transparent 24x24 canvas and render at 48x48 (2x). The frame has a 32x32 canvas and renders at 64x64 (2x). Imports: Sprite, Point, no compression, no mipmaps, Full Rect, 64 PPU. Source reference was supplied by the user; it is not flattened into gameplay UI.

Scripts use the [LibreSprite scripting API](https://github.com/LibreSprite/LibreSprite/blob/master/SCRIPTING.md). `ReferenceSlot.png` is the 34×34 source crop from the supplied Allstuff image. Rebuild with `powershell -ExecutionPolicy Bypass -File Tools/Build-ConsumableArt.ps1 -LibreSpritePath <path-to-libresprite.exe>`. The helper runs the drawing scripts, then uses LibreSprite's CLI crop/export to save native-size `.ase` and PNG files. The editable `.ase` files can also be opened and edited directly.
