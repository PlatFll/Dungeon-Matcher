# Board and combat feel — implementation

The October 12 Candy Crush Windows/Android research informed this original
Dungeon Matcher presentation pass. No King code, art, audio or vibration arrays
are included. BoardController and the existing combat owners retain all rules,
targets, rewards, shields, move ownership and special-chain semantics.

## Ordinary timing

New move-based play uses 135 ms swaps, a 30 ms invalid apex, the existing
50 + 30 ms clear flash/hold, a 40 ms ordinary postburst gate, fall multiplier 1,
25 ms refill stagger per column and 30 ms after landing. Distance compression,
acceleration, landing rebound/squash and primary special gates are retained.
Decorative shards outlive the gate and overlap refill.

The research's 524 → 425 ms one-cell sum was a nominal estimate, not a measured
touch-latency claim. Executed A/B measurements and regression evidence belong in
[the validation record](Validation/BOARD_FEEL_2.md).

Seconds-based combat keeps the previous timing budget. Replay tapes now record
`boardFeelVersion`: missing/zero means the previous profile; one means this pass.
An interrupted old tape keeps its profile through settlement and moves to the
current profile only after a successfully saved stable checkpoint. Unknown
versions preserve the save and reject recovery. Combat clock units never change.

## Impact, sound and vibration

One monotonically identified board presentation receipt describes a grouped
match at actual shatter. Match debris, audio and haptic requests share that
rendered frame; damage/reward reporting keeps its existing earlier boundary.
The old match component is an optional clip fallback, with no fixed-delay queue.
Missing or muted sound cannot suppress a permitted vibration.

The central six-voice mixer retains priorities, cascade semitone progression,
cooldowns and the 0.8 software gain budget. Nine repeated cues have four original
mono PCM variants each, chosen without immediate repetition using private
presentation randomness. Match/explosion clear counts add at most 18% gain
before the shared budget. This is not a true-peak limiter.

| Event | Vibration |
| --- | --- |
| Ordinary grouped match | One light Tick at shatter |
| Invalid return | Quiet Tick at reversal |
| Earned special / blocker destruction | Impact, coalesced with coincident explosions |
| Directional / area / beneficial special burst | One grouped Impact |
| Crystal | Light anticipation, stronger sweep-start/shatter peak |
| Accepted player skill | Cast |
| Player HP or shield damage | High-priority Impact |
| Landing / blocker damage / poison damage tick | Audio/visual detail; no vibration |

The haptic scheduler keeps the strongest priority in a batch. Higher-priority
contacts can promote past a detail cooldown after 40 ms; ordinary repeats retain
130 ms spacing (200 ms for detail), with at most six dispatch attempts per
one-second window. Rejected requests are dropped, never delayed. Android/iOS
retain their existing device-tuned system-respecting backend. Request/coalesced/
suppressed/budget-drop/dispatch counters describe software decisions, not a
measurement of the phone motor.

Pause, focus loss, vibration mute and snapshot restoration/replay suppress or
cancel feedback. Music, SFX, vibration and reduced motion remain independent.

## Readability and control

An accepted pointer-down immediately tints the gem and gives a small press
response; it does not select, spend a move, or bypass UI/pin/busy guards.
Cancellation restores the selected/resting appearance. Ordinary swipes still
commit at the existing threshold; Slippery retains inspect-on-drag and
commit-on-release. There is no input buffer.

Earned specials gather four small accents through the existing particle pool,
then briefly reveal their new shell. A preserved special receives no destruction
shards. Blocker hits retain the existing durability-change white flash; final
breaks have a separate contact cue and a few short neutral chips. Reduced Motion
omits these extra particles and press scaling, and softens the creation flash.
Current sprites, pixel filtering, board masks and character action clips remain.

## Capture and tuning

In Play Mode use **Dungeon Matcher → Validation → Start Board Feel Capture**,
play the target case, then **Save Board Feel Capture**. The bounded 8192-sample
ring writes to `.utmp/BoardFeel2/manual.csv` only when requested. Development
builds can attach `BoardFeelMetrics` and call `ExportCsv`; normal release calls
are compiled out. There is no always-on telemetry, network upload or file IO.

Rows include pointer acknowledgment, swap start/arrival, accepted move, clear,
fall start, landing, board settlement and final readiness, plus frame time and
the previous frame's recorded GC allocation bytes. Hold flags are board work=1,
external input token=2, combat consequences=4 and paused=8. These identify why
the last landing may precede input readiness. Dispatch timestamps are not
physical touch/audio/motor latency measurements.

Physical Android/iPhone vibration, speaker mix and low/mid/high-device frame
pacing still need hardware sessions. Use long cascades, all special families,
rapid invalid attempts, enemy impacts during feedback, mute/focus/pause and
Continue. Further pooling should respond to measured allocation/hitch data;
Gem destruction still owns obstacle cleanup and is not replaced with pooling.
