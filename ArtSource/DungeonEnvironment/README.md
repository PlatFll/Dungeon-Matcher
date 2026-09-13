# Dungeon Matcher environment art

- `Source_Original.aseprite`: protected copy of the original open LibreSprite scene.
- `Dungeon_Tile_Library.aseprite`: final 64 x 64 tile library, authored with native LibreSprite pixel operations without resampling or antialiasing.
- `Dungeon_Scene_Preview.aseprite`: layered composition preview; layers match BackWall, Architecture, Floor and BackDecor.
- `Dungeon_Scene_Preview.png`: flattened preview at native resolution.
- `Dungeon_Matcher_Environment_Final.gpl`: final 14-color environment palette.

The approved wall geometry is preserved. Its stone clusters, mortar and highlights were mapped separately into the final warm/neutral palette.

## Library coordinates

Rows and columns below start at zero at the upper-left of the sheet. Every cell is 64 x 64 pixels.

| Row | Columns | Contents |
| --- | --- | --- |
| 0–3 | 0–7 | Connected BackWall; preserve the complete 8 x 4 arrangement |
| 4 | 0–7 | Eight compatible Floor Edge variants |
| 5 | 0–4 | Four normal Foundation variants and one lightly cracked variant |
| 6 | 0–3 | Pillar base, shaft A, shaft B, capital |
| 6 | 4–6 | Arch left, crown, right |
| 6–7 | 7 | Alcove top and bottom |
| 7 | 0 | Buttress |
| 8 | 0–5 | Chain, wall ring, empty bracket, monster relief, loose stones, hanging fixture |

Floor variants share the same boundary profile. Foundation variants share left/right profiles and a mortar joint at their top/bottom boundaries, allowing arbitrary stacking. Use all four normal variants; place the cracked variant occasionally.

Unity exports are in `Assets/_Game/Art/Backgrounds/BattleArea/EnvironmentFinal/`. The existing `ConnectedBackwall/Connected_Backwall.png` retains its original GUID and slicing. All tiles use 64 PPU, Point filtering, no mipmaps, no compression and full rectangle sprites. Tile assets have no colliders.

The scene preview shows the central eight columns and two foundation rows. The actual prefab extends the floor to 16 columns and the foundation four rows below it for viewport overscan. Floor and Architecture have an authored local Y offset of 48/64 units. The upper 48 pixels of each floor tile form the walkable plane above the shared baseline; the lower 16 pixels form its front edge. This gives characters room to stand at different depths at both tested portrait heights without changing layout code. Paint the source prefab in Prefab Mode; preserve unit Grid scale and runtime-owned masking.
