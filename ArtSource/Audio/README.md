# Combat sound source

`generate_combat_sfx.py` synthesizes thirteen original short mono PCM sounds using
only Python's standard library. It does not use downloaded samples or a service.
Run it from any directory to rebuild `Assets/_Game/Resources/Audio/Combat/*.wav` and
the measured duration/peak/RMS/hash manifest alongside this file.

The sound family uses small bell-like gem transients, dry wood/bone impacts,
compact filtered-noise explosions, a bubbling poison puff, and short musical
healing/shield/ability cues. Bardley has a plucked C-minor flourish; Rattlebones has
a crown chime over a dry rattle. All tails are below 570 ms. Preserve the deliberate
1.5 ms onset and 15 ms final fade when replacing assets.

Every clip peaks at -3.35 dBFS. `CombatSoundMix` applies category gains, cooldowns,
six-voice priority allocation and a 0.8 combined gain budget. This leaves margin
for the existing independent music loop. Gem landings and poison ticks stay
quiet; player damage, explosions and player casts lead the mix. Same-frame tile
bursts are grouped by family; tile count never multiplies the number of voices.

The existing match-break controller owns match grouping/flash delay and delegates
playback to the common mix; it retains its original clip as a missing-library
fallback. Other sound cues consume committed actor/board events. All feedback is
suppressed during restore/replay, pause and focus loss. Audio never changes damage
or board timing. Music, SFX and vibration preferences are independent.

Android uses `View.performHapticFeedback` on the UI thread (system settings
respected, no VIBRATE permission). iOS uses light/medium UIKit impact generators.
No long `Handheld.Vibrate` fallback is used. Grouped landings are capped at one
soft pulse per 200 ms; impacts/casts take priority in a frame. Pausing, muting or
backgrounding cancels pending native requests. Unsupported devices quietly omit
haptics. Native hardware feel still requires physical-device verification.

Platform references:
- https://developer.android.com/develop/ui/views/haptics/haptic-feedback
- https://developer.apple.com/documentation/uikit/uiimpactfeedbackgenerator
