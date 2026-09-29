# HUD, Thaleah and combat feedback — 2026-09-29

## Result

- Bottom HUD uses the same dark masonry sprite, tiled rendering and tint as the player panel.
- Energy is shown as a teal bar. The ability and two supply buttons share a baseline and symmetric horizontal spacing.
- New PixelLab potion/bomb icons sit on bright lavender tiles. Normal, highlighted, pressed and disabled tile states share identical silhouettes. Counts sit clear of the icons.
- Thaleah Fat is the shared font for game text and numbers: menus, wave plaque, HP/shields, timers, cards, guides, supply counts and floating feedback. A native 16-point bitmap atlas uses Point filtering, no mipmaps and a bitmap shader. `PixelTextFitter` chooses whole physical-pixel glyph sizes that fit each container; guides scroll at a readable size. The painted logo remains brand artwork.
- Healing displays `+amount` in green; shield gains display `+amount` in bright blue. Normal damage is white, poison dark green, critical gold, block blue-gray, miss gray and stagger amber. Amounts come from existing actor events after clamping/mitigation. Text rises above actors and fades through the existing pool.

## Evidence

| Check | Result |
| --- | --- |
| `Tools/Validate-Unity.ps1` | PASS with Unity 6000.3.19f1. Log: `%TEMP%/DungeonMatcher-UnityValidation-f216c995-d1d0-4bb7-a7bc-b105a8deeeff.log`. |
| `Tools/Review-FinalizedVisuals.ps1` | PASS: 38 actual scene captures at 1080×1920 and 1080×2400. |
| Runtime typography audit | Every visible label in all 38 cases uses Thaleah with a bitmap material, Point atlas, complete glyph coverage, integer physical glyph scaling and measured container fit. Three-digit `WAVE 999` fits the plaque. |
| HUD checks | Matching player/bottom tile treatment; no `EnergyAmount`; symmetric controls; tile state references present. |
| Actor feedback | Both actors emit exactly one white damage number and one green `+5`; oversized healing reports only the missing `+2`; full HP emits no zero number. Shield gains show `+10` in blue; shield damage reports actual loss. Re-enabling the controller twice retains subscriptions without duplicates. |
| Poison and lifecycle | Real `CombatController.ApplyPoisonToAllEnemies` installs the established status/presenter. A status tick emits one dark green `-3`; fade alpha decreases before pooled release. Existing hit-flash/poison VFX remain active. |
| Native art | Six exports have native 24×24 or 32×32 dimensions, binary alpha and exact game-export matches. Four tile states have identical alpha. Editable `.aseprite` sources are retained. |

Runtime checks use disposable account/character scopes and production scenes. They do not overwrite the user's profile. No Android device test was performed.

## Art and license

[Native art review](../../ArtSource/HudTypography/Validation/native-art-review.png),
[asset audit](../../ArtSource/HudTypography/Validation/art-audit.json),
[PixelLab lineage](../../ArtSource/HudTypography/manifest.json).
Four PixelLab generations were used (663 → 659 remaining), including one tile correction.

Thaleah by Rick Hoppmann / Tiny Worlds is credited in the combat guide and in
[the font attribution](../../Assets/_Game/Fonts/Thaleah/ATTRIBUTION.md).
The original TTF is unmodified; the atlas maps typographic punctuation to this face's ASCII equivalents.

## Screenshots

[1920 gameplay](../../ArtSource/HudTypography/Validation/Screens/1920-game.png) ·
[2400 gameplay](../../ArtSource/HudTypography/Validation/Screens/2400-game.png) ·
[healing](../../ArtSource/HudTypography/Validation/Screens/1920-healing-green.png) ·
[shield](../../ArtSource/HudTypography/Validation/Screens/1920-shield-blue.png) ·
[poison](../../ArtSource/HudTypography/Validation/Screens/1920-poison-green.png) ·
[guide](../../ArtSource/HudTypography/Validation/Screens/1920-guide.png) ·
[main menu](../../ArtSource/HudTypography/Validation/Screens/1920-mainmenu.png) ·
[shop](../../ArtSource/HudTypography/Validation/Screens/2400-shop.png).

Full runtime reports and text-size/container measurements are retained beside these screenshots.
