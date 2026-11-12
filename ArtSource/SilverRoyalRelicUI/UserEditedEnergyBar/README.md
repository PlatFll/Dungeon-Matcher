# User-edited silver energy bar

The user saved `AbilityBarSilver.ase` and requested actual-game integration on
2026-11-12. `Ali_AbilityBarSilver.ase` is that exact saved file; its LibreSprite
export replaces the shared `EnergyFrameSilver.png` used by all five zones.

- Canvas: 147 x 24; 10 opaque colors; binary alpha.
- The existing 117 x 8 live-fill window remains clear at x15..131, y8..15.
- 172 RGBA pixels differ from the earlier frame; no further cleanup or recolor.
- Runtime PNG matches the LibreSprite export byte for byte; Unity GUID and
  Point/uncompressed import settings are unchanged.
- Other shared UI PNGs, energy logic, layouts and character art are unchanged.
- Previous frame and original editable source are retained here.

The larger wave, Settings, power-up holder and item-icon documents opened for
the user are editing copies. Their production source PNGs remain unchanged;
the approved enlargement is still controlled by the existing shared UI layout.
No PixelLab generations were used.

Runtime verification is recorded in `Validation.md` after the Unity checks.
