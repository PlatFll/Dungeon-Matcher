# Dungeon presentation polish — 2026-09-21

Working branch: `codex/dungeon-presentation-polish`, stacked on local-enemy
PR #161 (`cbd7f65`). This is a reviewable art/feedback pass, not a merge or release.

## Source and implementation scope

- Reviewed the current v1.4 art guide, unchanged character reference board,
  Rattlebones idle and supplied mockup. The text attachment mentioned a recording
  without embedding it. A matching local recording, `Screen Recording 2026-09-21
  040724.mp4`, was inspected: it shows the local-enemy browser preview, not Unity
  gameplay or player casts. No missing source was invented.
- Preserved all four supplied native VFX in `ArtSource/TileVfx/Originals/`.
  Final native files, exact horizontal sheets, JSON exposures and GIFs use 64×64,
  thirteen 30 ms frames (390 ms). Original palettes are retained. Every family has
  clean binary alpha and a canvas margin; the generic tail's initial right-edge
  clipping was corrected before final exports.
- Native LibreSprite repairs affect six existing local-enemy clips: candle and
  boot connections, rigid basket motion, an articulated berry throw and a rounded
  folded rear boot. All 71 production local-enemy frames remain palette-safe,
  with unchanged timings, ready poses, pivots and canvas sizes. Miner's attack and
  ability and the earlier four characters are unchanged. Before-pass files are
  preserved in `Originals/BeforePresentationPolish/`.
- Ten native presentation assets supply dungeon masonry/props, the pink-gem logo,
  menu doorway and 24×24 consumables. The existing tilemap prefab and UI layout
  owners retain placement authority. Props use the shared floor; the player fill
  follows the frame section, not the narrower portrait content stack.
- Board events snapshot actual destroyed tile centers at shatter. Preserved
  rewards/protected crystals are excluded; overlap selects one deterministic
  family per cell. Optional rendering does not add gameplay waits or damage.
- Thirteen original synthesized sounds use a six-voice, priority-limited mix.
  Android/iOS haptics and independent persistent vibration settings are included.
  A review found and fixed stale delayed match audio surviving pause.

## Executed source and automated checks

- Final `Tools/Validate-Unity.ps1`: **PASS, exit 0**, Unity 6000.3.19f1.
  Log: `%TEMP%/DungeonMatcher-UnityValidation-003de783-88dd-4383-8e85-08d84958d439.log`.
- `ArtSource/LocalEnemies/Scripts/validate.py`: PASS, all 71 native/PNG/GIF frames,
  timings, approved colors, alpha, margins, planted contact and ready-pose closure.
- `check_polish.py` against preserved before-pass sources: PASS for six repaired
  clips; no remaining large detached body/prop components. Actual native drawings
  were inspected frame by frame at native/integer zoom.
- `ArtSource/Presentation/Scripts/validate.py`: PASS, all ten static exports and
  four VFX families match native pixels, with valid alpha/palettes/timings/margins.
  This caught a native background-layer export issue; normal transparent layers
  were committed after reopening in LibreSprite, then exported again.
- Focused Unity EditMode run: **105 passed, 0 failed, 0 skipped** across new
  `TileBurstPresentationTests` / `CombatAudioTests` and affected damage, special,
  chain-attribution and wave-lifecycle tests. `.utmp/presentation-tests.xml`.
- `Tools/Test-Balance.ps1`: **243 passed, 0 failed, 0 skipped**.
  `.utmp/balance-editmode.xml`. This overlaps the targeted run; counts are not
  additive. It includes the existing Play Mode lifecycle wrapper.
- `AudioPersistenceValidation.Write` then `ReadAndRestore`, in separate Unity
  processes: PASS. Independent music/SFX/vibration values persisted and original
  key presence/values were restored. `.utmp/audio-persistence-report.txt`.
- `LocalEnemyAnimationValidation.Run`: PASS. Actual frame-five damage/mining/
  barricade effects, simultaneous-ready abilities, pause, recovery, missing-event
  fallback, cancellation and 14 earlier attack lifecycle scenarios. All eight
  clips/71 poses checked at 1080×1920 and 1080×2400 for shared ground, fixed center,
  health-bar clearance and integer character pixels. `.utmp/LocalEnemies/`.

## Runtime visual evidence

`DungeonPresentationValidation.Run` uses the actual Game/MainMenu scenes and
accepted board area-clear resolver. Its isolated profile and high-HP scene targets
keep real damage/clear/refill logic active without advancing into a card draft.
Batch focus is simulated and SFX temporarily unmuted only for cue observation.
The initial tall run correctly killed its target and entered the wave-two draft;
that fixture defect was diagnosed from the saved state and corrected, not ignored.

Final scene/cue run: **PASS, 784 assertions**, with twelve native screenshots at
1080×1920 and 1080×2400. All four families rendered one correctly centered,
64-pixel-footprint effect on each of nine actual affected cells; pause froze
frames and sound requests, then cleanup and board settlement completed. Menu
logo/art kept integer physical pixels, did not overlap controls, and remained
unique through repeated Home navigation. Game/menu screenshots were visually
reviewed for source transparency, floor placement, player-frame fill and contrast.

All **13 sound mappings** produced actual playback requests. Actor cases exercised
real damage/heal/shield, shield-plus-HP overflow deduplication, scheduled HP and
shield-only poison ticks, accepted Rattlebones/Bardley casts, rejected repeated
Royal Decree, actual Bardley refill landings, mute/unmute and pause. Shared enemy
ability audio used the real committed-effect notification with a temporary Miner
definition; that audio case did not execute mining itself (the local-enemy runtime
suite did). The delayed-match case verified both a positive playback control and
no stale cue after resuming.

Results and screenshots are in `.utmp/DungeonPresentation/`; cue and tile events
are in `audio-cues.tsv` and `runtime-events.tsv`. A `CuePlayed` event proves an
actual `AudioSource.Play` request, not that a human heard it.

## Limits

No physical Android/iOS deployment, touch/haptic feel or speaker/headphone listening
approval is claimed. Android/iOS conditional C# paths passed static compilation;
the iOS native plugin still needs an actual Xcode/device build. Automated tests and
visual review do not establish performance across all phones or human game feel.

The user's existing `AGENTS.md` changes are excluded from this work. Gameplay
balance, HP/shield calculations, ability costs and authoritative board ownership
remain unchanged.
