# Restored guard idles — selected for Unity

The user rejected the stronger remaining-cast correction pass and requested the
animations from immediately before it. These four sources are exact native-file
copies from `../RemainingCast/BeforeCorrection/Idles/`: Crossbow Guard, Barricade
Guard, Spear Guard and Siege Sergeant. Town Marshal is a local leader, so he is
outside this guard-only integration. No new generation or pixel edits were needed.
The rejected files remain preserved for history, not as the active guard source.

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
bytes. `Verification.json` records the selected file hashes. These are preservation
checks, not a claim that the earlier art is flawless or that the rejected correction
was visually approved.

Run `CombatIdleValidation.ImportGuardsAndRun` in a graphics-enabled batch Unity
editor (without `-quit` or `-nographics`) for production-scene import/playback,
pause, all-pose grounding and portrait alignment checks. Reports and captures are
written under `.utmp/CombatIdles/guard*`. See
`Docs/Validation/RESTORED_GUARD_IDLES.md` for executed evidence.
