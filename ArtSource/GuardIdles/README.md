# Restored guard idles — selected for Unity

The user accepted the restored motion as the target for the rest of the cast.
The subsequent family pass repairs Crossbow Guard's lower bow contour and body
joins while retaining this cadence and exact ready pose. His preceding file is in
`BeforeCrossbowRepair/`. Barricade Guard, Spear Guard and Siege Sergeant remain
exact copies from `../RemainingCast/BeforeCorrection/Idles/`.

The rejected stronger motion remains archived. Current wider scope and evidence:
`../RemainingCast/SelectedIdles/README.md` and
`Docs/Validation/GROUNDED_ENEMY_IDLE_FAMILY.md`.

Each selected idle retains nine 64×64 frames at 130 ms, a 1.17-second loop, the
existing recolor palette and identical first/last ready poses. The user's supplied
eight-frame PixelLab prompt is the direction for future generation; this restoration
does not resample or redraw the selected nine-frame backups.

`CombatIdleImporter.ImportGuards` imports only these four PNG sheets, creates
Point/FullRect sprites with fixed bottom-center pivots and sprite-only looping
clips, and updates each existing enemy definition's fallback sprite and controller.
It preserves gameplay fields, attack/ability timing and generic action feedback.
Unity sheets: `Assets/_Game/Art/CombatIdles/`.
Unity clips/controllers: `Assets/_Game/Animations/CombatIdles/`.

Use `Export.ps1` for native LibreSprite exports. `verify_sources.py` checks exact
backup preservation, all exported pixels, palettes, alpha, timing and Unity sheet
bytes. `Verification.json` records the selected file hashes and distinguishes the
repaired crossbow from the three exact restored sources. The stronger correction
remains rejected.

Run `CombatIdleValidation.ImportGuardsAndRun` in a graphics-enabled batch Unity
editor (without `-quit` or `-nographics`) for production-scene import/playback,
pause, all-pose grounding and portrait alignment checks. Reports and captures are
written under `.utmp/CombatIdles/guard*`. See
`Docs/Validation/RESTORED_GUARD_IDLES.md` for executed evidence.
