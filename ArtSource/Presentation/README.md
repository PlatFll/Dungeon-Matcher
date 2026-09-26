# Dungeon presentation artwork

Native sources and PNG exports for the 2026-09-21 presentation pass. All pixel
drawing is performed inside LibreSprite by `Scripts/paint_native.js`. Run
`Scripts/finish_native.js` afterward to reopen and commit transparent normal
layers, then export with the LibreSprite CLI. `Scripts/validate.py` checks exact
native/export pixels and alpha. No generated mockup is used as a game background.

- `DungeonBackdropTile`: seamless, low-contrast 64×64 violet masonry for the
  outside gutters, bottom HUD and darkened player frame.
- `Torch` / `TorchAlt`, `RoyalBanner`, `Barrel`, `SkullPile`: 64×64 dungeon dressing
  added to the existing battle environment's tilemap prefab. Alternate torch
  art is an editable option; the current tilemap uses the first pose.
- `DungeonMatcherLogo`: 210×88 custom native lettering with warm bevels and a
  prominent pink story gem between the two title lines.
- `MenuDungeon`: 320×480 authored masonry, archway and prop composition. Its
  main decorations remain inside the central tall-phone crop; the dark doorway
  gives the menu controls clear space.
- `Potion` / `Bomb`: polished 24×24 consumables replacing the existing resource
  PNGs while preserving their Unity GUIDs. Current editable versions live here;
  earlier `ArtSource/Consumables` files remain historical originals.

`DungeonPresentationArtImporter.Run` imports Point/FullRect/uncompressed/no-mip
sprites and adds `AtmosphereProps` to `Dungeon_Default.prefab`. Original masonry,
architecture, floor, native scale and mask ownership remain in that prefab.

The menu uses integer physical source-pixel scaling and intentional cropping.
Runtime text remains Unity text; only the explicitly requested game-title logo
is drawn into art. Backgrounds never receive pointer input or own combat layout.
