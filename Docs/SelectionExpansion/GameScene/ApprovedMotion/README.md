# Approved enemy animations in the Game scene

Unity 6000.3.19f1 rendered all 13 clips on the production Game canvas at 7356a2c804e0c9702f4f48f1b811509006c41875. Each animation below is cropped from the Game view. The [interactive gallery](Review.html) also includes the full scene. Open the HTML locally to switch characters and clips.

| Enemy | Idle | Revised attack or heal |
|---|---|---|
| Guild Swordsman | ![Guild Swordsman Idle](guild_swordsman_idle-detail.webp) | Previously approved basic |
| Guild Bulwark | ![Guild Bulwark Idle](guild_bulwark_idle-detail.webp) | ![Guild Bulwark Basic attack](guild_bulwark_basic-detail.webp) |
| Elven Chirurgeon | ![Elven Chirurgeon Idle](guild_chirurgeon_idle-detail.webp) | ![Elven Chirurgeon Heal](guild_chirurgeon_special-detail.webp) |
| Nacre Halberdier | ![Nacre Halberdier Idle](nacre_halberdier_idle-detail.webp) | ![Nacre Halberdier Basic attack](nacre_halberdier_basic-detail.webp) |
| Elven Pathblade | ![Elven Pathblade Idle](elven_pathblade_idle-detail.webp) | ![Elven Pathblade Basic attack](elven_pathblade_basic-detail.webp) |
| Orc Slinghunter | ![Orc Slinghunter Idle](orc_slinghunter_idle-detail.webp) | ![Orc Slinghunter Basic attack](orc_slinghunter_basic-detail.webp) |
| Iceborn Axeman | ![Iceborn Axeman Idle](iceborn_axeman_idle-detail.webp) | ![Iceborn Axeman Basic attack](iceborn_axeman_basic-detail.webp) |

## Slinghunter combat checks

- Slinghunter contacts=1 · attributed=1 · HP 100->100 · shield 5->0 · move=1
- Slinghunter contacts=1 · attributed=1 · HP 100->95 · shield 0->0 · move=1

Both checks used an accepted board swap and the normal attack sequence. They counted the orc’s resolved attack contacts and the player’s attributed damage events through recovery. The shield test used the game’s minimum shield grant of five.

The final focused Unity suite passed 18 of 18 cases. The first run passed 17 of 18. Its shield test expected a one-point grant, which the game correctly rounded to five. The corrected fixture verifies the actual shield grant, one contact, its attribution and the remaining HP after recovery. The production combat code was unchanged by the test correction.

[Per-clip capture and validation record](evidence.json)
