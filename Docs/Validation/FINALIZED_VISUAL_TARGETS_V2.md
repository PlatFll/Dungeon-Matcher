# Finalized visual targets v2 — review candidate

Date: 2026-09-28. Base: `651fe50`. Branch: `codex/finalized-visual-targets`.

The native dungeon and UI pass is implemented and validated in the production
scenes. All new art remains a **review candidate pending the user's approval of
these screenshots**. The supplied mockups remain reference images.

## Result and source files

- [Native source folder and rebuild instructions](../../ArtSource/FinalizedVisuals/README.md): 127 PNG exports with matching editable LibreSprite files.
- [Asset manifest](../../ArtSource/FinalizedVisuals/AssetManifest.json): dimensions, palettes, occupied bounds, alpha, hashes, pivots, PPU, slice borders, PixelLab jobs and candidate status.
- [Native inspection page](../../ArtSource/FinalizedVisuals/Validation/NativeInspection.html): original PNGs at 1× and nearest-neighbor enlargement on light and dark backgrounds.
- [Environment beside the current cast](../../ArtSource/FinalizedVisuals/Validation/NativeEnvironmentReview.png) and [UI review sheet](../../ArtSource/FinalizedVisuals/Validation/NativeReview.png).

The 512×384 environment contains 13 repeatable terrain tiles, 19 transparent
architecture/prop modules and a 48-cell baked export. The four wall rows, floor
row and foundation row reconstruct the master exactly. `Dungeon_Finalized`
preserves the existing unit Grid, four Tilemap roles, renderer orders, mask and
placement controller. `FinalizedDungeonPalette` exposes the terrain library.

The 46 UI exports supply large/small/gear control states, panels, wave plaque,
five left badges, separate health frame pieces/track/fill, teal energy and the
title/split-gem motif. Runtime labels use TextMeshPro. Enemy category selects
the bar style independently for each slot. The player always uses the heart.

PixelLab was confirmed connected and its balance checked before production:
823 generations initially, 663 after this pass; 160 consumed, $0 credit balance.
Focused edits corrected the Normal/Boss symbols, energy silhouette, arch and
crate transparency. LibreSprite supplied palette/cluster cleanup, native
assembly, identical state silhouettes and editable sources. No mockup pixels
were cropped into production art.

## Validation performed

| Check | Result |
| --- | --- |
| `ValidateAssets.py` | PASS: 127 native exports; PNG RGBA equals its editable source; binary alpha and required transparent backgrounds. |
| Native construction | PASS: 48 baked cells equal the 512×384 master; all 61 intended tile-neighbor pairs agree at their joining edges. |
| Control states | PASS: large/small/gear variants have identical dimensions and alpha silhouettes. Runtime highlighted, pressed and disabled sprite transitions checked. |
| Unity imports | PASS: Point, FullRect, 64 PPU, no mipmaps/compression; tile mapping, renderer order and masking verified. |
| `Tools/Validate-Unity.ps1` | PASS with Unity 6000.3.19f1, exit 0, after the final code/art changes. |
| `Tools/Review-FinalizedVisuals.ps1` | PASS: 28 actual production-scene captures, 14 cases at each portrait size. |
| Settled gameplay layout | PASS: zero reported issues at 1080×1920 and 1080×2400. |
| Health and energy | PASS: independent category badges and HP widths, a reused Normal-to-Boss slot, empty/partial/full energy cropping, and separate shield presentation. |
| Text migration | PASS: all active captured labels use TMP. 17 MainMenu and 7 character-prefab label replacements preserve component IDs; all non-label serialized blocks remain identical. |

Machine-readable evidence: [asset audit](../../ArtSource/FinalizedVisuals/Validation/AssetAudit.json),
[assembly/seams](../../ArtSource/FinalizedVisuals/Validation/EnvironmentAssembly.json),
[serialized migration](../../ArtSource/FinalizedVisuals/Validation/SerializedMigration.json),
[runtime report](../../ArtSource/FinalizedVisuals/Validation/Screens/environment-review.txt),
[1920 layout](../../ArtSource/FinalizedVisuals/Validation/Screens/1920-layout.txt),
[2400 layout](../../ArtSource/FinalizedVisuals/Validation/Screens/2400-layout.txt).

Full local logs:

- Unity compile: `%TEMP%/DungeonMatcher-UnityValidation-34775e4c-fc74-452c-8410-c3f23ad4ac92.log`.
- Runtime review: `.utmp/finalized-visual-review.log` in this checkout.

The runtime harness uses a disposable account profile and temporary character
selection. Rank screenshots bind real EnemyDefinitions through the existing
WaveController/EnemySlotUI APIs. They are presentation fixtures, not evidence of
a complete natural run to each rank. No Android build or device test was run.
The harness distinguishes startup layout diagnostics from the explicit settled
layout check; it excludes only the known Unity Search startup exception.

## Actual Unity screenshots

| Case | 1080×1920 | 1080×2400 |
| --- | --- | --- |
| Before this pass | [Before](../../ArtSource/FinalizedVisuals/Validation/Before-1080x1920.png) | [Before](../../ArtSource/FinalizedVisuals/Validation/Before-1080x2400.png) |
| Integrated game | [Game](../../ArtSource/FinalizedVisuals/Validation/Screens/1920-game.png) | [Game](../../ArtSource/FinalizedVisuals/Validation/Screens/2400-game.png) |
| Empty energy | [Empty](../../ArtSource/FinalizedVisuals/Validation/Screens/1920-empty-energy.png) | [Empty](../../ArtSource/FinalizedVisuals/Validation/Screens/2400-empty-energy.png) |
| Normal, Special, Miniboss / partial energy | [Ranks](../../ArtSource/FinalizedVisuals/Validation/Screens/1920-ranks-partial.png) | [Ranks](../../ArtSource/FinalizedVisuals/Validation/Screens/2400-ranks-partial.png) |
| Boss / full energy | [Boss](../../ArtSource/FinalizedVisuals/Validation/Screens/1920-boss-full.png) | [Boss](../../ArtSource/FinalizedVisuals/Validation/Screens/2400-boss-full.png) |
| Separate enemy shield | [Shield](../../ArtSource/FinalizedVisuals/Validation/Screens/1920-shield.png) | [Shield](../../ArtSource/FinalizedVisuals/Validation/Screens/2400-shield.png) |
| Settings / pause | [Settings](../../ArtSource/FinalizedVisuals/Validation/Screens/1920-settings.png) | [Settings](../../ArtSource/FinalizedVisuals/Validation/Screens/2400-settings.png) |
| Guide | [Guide](../../ArtSource/FinalizedVisuals/Validation/Screens/1920-guide.png) | [Guide](../../ArtSource/FinalizedVisuals/Validation/Screens/2400-guide.png) |
| Upgrade cards | [Upgrades](../../ArtSource/FinalizedVisuals/Validation/Screens/1920-upgrades.png) | [Upgrades](../../ArtSource/FinalizedVisuals/Validation/Screens/2400-upgrades.png) |
| Game over | [Game over](../../ArtSource/FinalizedVisuals/Validation/Screens/1920-gameover.png) | [Game over](../../ArtSource/FinalizedVisuals/Validation/Screens/2400-gameover.png) |
| Main menu | [Main menu](../../ArtSource/FinalizedVisuals/Validation/Screens/1920-mainmenu.png) | [Main menu](../../ArtSource/FinalizedVisuals/Validation/Screens/2400-mainmenu.png) |
| Character select | [Characters](../../ArtSource/FinalizedVisuals/Validation/Screens/1920-characters.png) | [Characters](../../ArtSource/FinalizedVisuals/Validation/Screens/2400-characters.png) |
| Gem mastery | [Mastery](../../ArtSource/FinalizedVisuals/Validation/Screens/1920-mastery.png) | [Mastery](../../ArtSource/FinalizedVisuals/Validation/Screens/2400-mastery.png) |
| Shop | [Shop](../../ArtSource/FinalizedVisuals/Validation/Screens/1920-shop.png) | [Shop](../../ArtSource/FinalizedVisuals/Validation/Screens/2400-shop.png) |
| Challenges | [Challenges](../../ArtSource/FinalizedVisuals/Validation/Screens/1920-challenges.png) | [Challenges](../../ArtSource/FinalizedVisuals/Validation/Screens/2400-challenges.png) |

## Visual comparison and limits

Compared directly with the [dungeon target](../ArtDirection/references/FinalizedVisualTargets/Dungeon_Battle_Area_Updated_Pixel_Target.png),
the native master retains the horned arch, grille, paired hanging details,
pillars, side furniture, broad platform and quiet foundation. Its repeated wall
stones are smaller and the palette is cooler/darker so the current cast stays
readable. The existing player panel covers much of the left dressing in play.
At 1080×1920 the established responsive viewport crops the upper arch; the
1080×2400 capture shows the crown. Actor transforms and projection were preserved.

Compared with the [UI target](../ArtDirection/references/FinalizedVisualTargets/Finalized_UI_Target_Player_Heart.png),
the native family retains purple material, sparse amethyst highlights, red HP,
teal energy and the opposed pink title facets. Control frames are narrower and
more ornamental at these native dimensions. Panel centers remain quiet for TMP
content. The title lettering and split seam were inspected manually.

Both portrait sets were inspected for clipping, baseline, contrast, fill bounds,
control-state tint and frame distortion. The last fixes removed residual black
alpha backgrounds, cleared old button tint, kept card rarity titles readable,
and replaced the procedural card placeholder with the native split-gem motif.
The shield fixture intentionally displays the existing shield art while hiding
the generated HP frame.

Combat rules, damage, shield rules, energy generation/spending, board resolution,
character sprites/animations, gems, VFX and locked board-frame colors are unchanged.
The separate Gideon work remains outside this branch. Final art approval and
physical-device review remain outstanding; the PR must stay unmerged.
