# Restored guard idles — 2026-09-22

Subsequent follow-up: the user accepted this restored style and requested it for
the remaining enemies. Crossbow Guard then received a local contour/join repair;
the other three guard sources remain exact. Current evidence is in
`GROUNDED_ENEMY_IDLE_FAMILY.md`. The checks below describe the preceding restoration.

The user rejected the stronger motion in the preceding cast correction and
requested the earlier animations if available. Exact backups existed. This pass
selects only Crossbow Guard, Barricade Guard, Spear Guard and Siege Sergeant.
Town Marshal is a local leader and is outside the requested guard scope.

## Selected sources and integration

`ArtSource/GuardIdles/` preserves the four native files from
`ArtSource/RemainingCast/BeforeCorrection/Idles/` byte for byte. Their nine 64×64
frames, approved recolor palettes, 130 ms timing and 1.17-second loops are unchanged.
LibreSprite batch exports provide matching PNG sheets, GIF previews and JSON.
No pixels were redrawn and no PixelLab generation was needed.

The existing `CombatIdleImporter.ImportGuards` imports these sheets with Point
filtering, uncompressed FullRect sprites, 64 PPU and fixed bottom-center pivots.
Sprite-only Idle clips and controllers use the existing enemy presentation path.
Each of the four enemy definitions changes only `fallbackVisualSprite` and
`animationControllerOverride`. Combat rules, shared attack/ability presentation
and damage timing are unchanged. The rejected art remains archived and is not the
guard import source.

## Executed checks

- `ArtSource/GuardIdles/verify_sources.py` passed after import: exact native backup
  preservation, all 36 native/PNG/GIF frame pixels, approved palettes, binary alpha,
  source ground contact, frame timing, first/last pose equality and exact Unity
  sheet bytes. Hashes and provenance are in `ArtSource/GuardIdles/Verification.json`.
- Graphics-enabled Unity **6000.3.19f1** batch execution of
  `CombatIdleValidation.RunGuards` passed with exit code 0. The validator checked
  imported rectangles/pivots, sprite curves, clip length, frame order, controller
  defaults and the four serialized definition references.
- Actual production `Game` scene playback passed for all four guards, including
  live frame changes and gameplay pause. All nine poses were inspected at both
  **1080×1920** and **1080×2400**: 72 character/pose/resolution combinations.
  Checks cover stable centers and ground positions, integer texel scaling,
  portrait containment, health-bar clearance, UI-mask containment and shared
  enemy ground alignment.
- Captures for frames 1, 6 and 9 of each two-guard group were produced at both
  resolutions. Four representative captures were visually reviewed with
  Rattlebones visible in the production scene; no alignment or UI clipping issue
  was observed in those captures. The synchronized source preview also includes
  unchanged Rattlebones and native-size playback.
- `Tools/Validate-Unity.ps1` passed on the final C# source with Unity 6000.3.19f1.

The first runtime attempt loaded the scene while the test Game View still had
its small startup resolution. The harness now waits for its requested portrait
resolution before loading the scene and before each resize check. The subsequent
run passed; no production layout code was changed to accommodate the test.

Local execution evidence:

- `.utmp/guard-idle-unity.log`
- `.utmp/CombatIdles/guard-asset-validation.txt`
- `.utmp/CombatIdles/guard-play-validation.txt`
- `.utmp/CombatIdles/guards-{0|2}-{1920|2400}-frame{1|6|9}.png`
- Final compile log:
  `C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-06f17d28-c77f-4fdf-a9f1-a0a076edab46.log`

These checks establish preservation and Unity integration of the selected earlier
versions. They do not establish new visual approval or claim those drawings are
flawless. No physical Android/iOS device testing was performed. No other cast
animation was integrated or modified in this pass.
