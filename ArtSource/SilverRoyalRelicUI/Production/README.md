# User-edited shared battle UI

The five PNGs in `Inputs` are exact copies of Ali's final Downloads files. Their
hashes and dimensions are in `Manifest.json`. Runtime copies are in
`Assets/_Game/Resources/UI/BattlePresentation/Silver`.

- Wave plaque: 176×66. Five lossless vertical strips allow native responsive
  rails without resizing its corners, central gold diamond or skull pendant.
- Settings: 56×56, square and native.
- Potion/bomb holder: 70×70, preserving the user's larger canvas.
- Ability frame: 176×66, separate from its icon.
- Rattlebones icon: 41×38 file, 26×36 visible drawing; centered by visible bounds.

The existing definition-owned bull, revolver and flute icons remain the current
choices for Minotaur, Kobold and Bardley. `OtherCharacterIcons` contains PNG and
editable LibreSprite copies, round-tripped with exact visible RGBA equality.
The source icons remain untouched. They are available for Ali's next edits.

No PixelLab generations or credits were used. No character, gem, environment,
board, health-frame, energy-bar or gameplay design changes are included.
The frame's existing ready lift/glow and button/energy owners are retained.
The original user-edited ASE and earlier candidates remain preserved.

Validation results and real Game screenshots are recorded in `QA` and `Preview`.
They are automated Unity evidence, not physical-device testing.
