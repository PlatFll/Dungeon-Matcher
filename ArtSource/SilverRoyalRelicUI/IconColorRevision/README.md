# User-cleaned ability icons — palette-only production revision

Ali's saved LibreSprite bull, revolver cylinder and flute are the source authority.
The production Game uses these recolored symbols through the existing three
CharacterAbilityDefinition sprite bindings. No runtime or gameplay code changes.

| Symbol | Palette authority | Opaque colors | Native canvas |
|---|---|---:|---|
| Minotaur bull | Current approved Minotaur portrait: warm fur, ivory horns, amber eyes | 7 | 48×40 |
| Kobold revolver cylinder | Canonical Pan Villager cool iron and shared dark separation | 4 | 48×40 |
| Bardley flute | Canonical Bardley/Rattlebones warm gold and shared dark separation | 4 | 48×40 |

All three retain the edited source's exact alpha mask, occupied bounds, native
size and placement. Only material colors changed. The matching white ready masks
light the drawn surfaces, leaving outlines and revolver chambers dark. Existing
native centering, ready lift, glow, pause and Reduced Motion remain authoritative.

`Inputs/` preserves the user's exact saved ASE files and native LibreSprite PNG
exports. `Editable/` contains colored LibreSprite documents; `Exports/` contains
transparent PNGs and ready masks. `color_icons.py` records the deliberate material
mapping and verifies exact LibreSprite export/reopen pixels. `Manifest.json`
records source/reference hashes, exact output palettes, dimensions and checks.
`Preview/ColoredIcons.png` compares the input and output at 1× and integer zoom.

The five user-approved silver control PNGs, Rattlebones icon, character art,
environments, gems, settings, menu and ability rules remain unchanged. Earlier
UI production work is already merged in PR #229. PixelLab spending: **0**.

Validation results and actual Game captures are recorded in `QA/Validation.md`
and `Preview/Gameplay/`. Previous production preview evidence is preserved.
