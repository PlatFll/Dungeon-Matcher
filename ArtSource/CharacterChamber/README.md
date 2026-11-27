# Character chamber artwork

The 240 × 192 wall, torch, banner and dais are from PixelLab image job `06987370-1692-464f-8d9b-993682c19a0c`, seed 452188. The lower masonry repeats a crop of that same wall. The 256 × 256 fittings sheet is from PixelLab UI job `f99172e6-efd8-439c-997b-c373d6098e72`, seed 283531. Each job used 10 generations. Both originals are included here.

The six fittings provide the name plaque, stat tray, parchment, normal and selected roster cards, and the level-up control. Their borders also serve the progression strip, selected tab, library rows and reset dialog. The existing merchant title, back arrow, wallet, coin and stone trim, plus the global menu navigation icons, are reused. Four small stat glyphs were drawn locally. Roster cards use each playable PlayerDefinition's battle sprite. The large stats preview plays its battle Animator Controller on the same foot pivot. A small stepped shadow sits under that pivot on the painted top of the dais. The ability and affinity icons are the same ones referenced by combat.

Each `.ase` is an editable LibreSprite file. `export.py` records the component rectangles, border widths, purple face cleanup, masonry crop and glyph drawings. It saves each cleaned sprite through LibreSprite, reopens the export and compares every RGBA pixel. Run it with Python and Pillow, passing the path to the LibreSprite executable. The production PNGs in `Assets/_Game/Resources/UI/CharacterChamber` use Point sampling and the same texture import settings as the merchant plates.

[Looping previews from Unity](GroundedIdles.gif) show the four characters at the timings of their production clips. [Full character screens](FourCharacters.png) show the surrounding UI.
