# Stage 1 — existing production UI audit

Base: main `a5a9d9cff79c85d232b2354a4c06305819901f40`. Work is isolated on
`codex/silver-royal-relic-ui`. This stage produces review candidates only.

The supplied composite contains the top and bottom reference families. The brief
mentions two images; only one image was attached in this request. It is sufficient
for Stage 1, and its original pixels are preserved in Inputs/UserTarget.png.
No reference screenshot is cropped into a finished asset.

## Existing bindings and native sizes

| Component | Native artwork / live size | Owner and separation |
|---|---|---|
| Wave | Stone/WaveBody.png, 176×48; live width capped at 176, height 48 | WaveTrackerUI reads WaveController and renders live TMP text. BattlePresentationSkin.Controls places a tiled/sliced body, with separate 16×19 SharedWavePendant at center y=-41. |
| Settings | Stone/Settings.png, 56×56 square | RunControlsUI creates the Button, then the global skin positions it. Gear currently baked into the PNG. Settings overlay/callback remains separate. |
| Energy | ReferenceDungeon/SharedEnergy.png, 147×24 | AbilityButtonUI builds a separate frame and fill; live maximum trough width is 107 with 8-pixel height. Charge remains controller-owned. |
| Ability | Stone/AbilityPlate and AbilitySocket, 176×66 | Existing Button/controller owns availability and activation. StoneAbilityButtonView owns separate per-character engraving/glow, 1px horizontal/3px vertical pop, pause and Reduced Motion. |
| Potion and bomb | Stone/Supply.png, shared 64×64 | Same frame template; PlaceSupply anchors at center ±132. Existing icon is fitted inside 48×48; quantities are live text. |

Top controls are children of GameplayPixelLayoutController.TopHud, anchor/pivot
(0,1), with uniform local scale. Settings is 8px from the right at y=-12.
Wave starts at y=-10 and preserves an 8px gap before Settings. Its left bound
reserves the portrait's 29% share plus 6px. The label has 16px horizontal insets.
The shared controls bypass ReferenceKitSurface/NativePixelUiSurface distortion.
CanvasScaler selects whole physical-pixel scale; GameplayPixelGrid snaps positions.
Small-screen wave width can shrink through native repeatable center pieces.
Future production must preserve corners/center ornaments while adapting that span.

Bottom ability and supply anchors are centered; BottomControlY handles the short
HUD. Energy sits above the ability at y=46. The serialized Game.unity ability
rectangle is 176×64, but ApplyUnifiedSkin authoritatively updates it to 176×66.
No scene or runtime fields were changed in this candidate stage.

Current imports inspected: Point, 64 PPU, FullRect, uncompressed, no mipmaps.
Guide controls, portraits, board frames/cells, health bars, cast, gems, zones and
gameplay remain outside this task. Stage 1 does not create bottom-HUD replacements.

## Style exploration scope

The current canonical guide records matte stone as the approved production
family. The new user brief explicitly requests silver royal relic candidates;
it supersedes material direction for these six components only after approval.
The guide and reference register remain unchanged until a selection is approved.

Three coherent wave/settings pairs explore medium bevels, slender moon-silver
and substantial forged facets. Native PixelLab generation uses the same approved
Rattlebones rendering input, with palette copying disabled. Silver/gold material
colors come from the user's candidate ramp. The screenshots guide composition
locally, and were not sent to PixelLab or used as finished source pixels.

Stage 1 has no production integration or fresh Unity screenshot requirement.
Later staged approvals remain required before top/bottom production and live
cross-zone integration. Existing artwork hashes are retained for comparison.
