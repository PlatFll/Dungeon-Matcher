# Forest production review — Phases 5–6

2026-10-02. These six designs remain the approved reference identities. Their
stills are unchanged. Production motion, scenery, UI and temporary music await
the user's review; this is the starter test roster.

## Playable kit values

Values below are definition-level test settings before existing run scaling.
Weakness assignment remains independent of skin/clothing, race and zone gem.

| Enemy | Rank | HP | Basic | Interval | Special/counter |
|---|---|---:|---:|---:|---|
| Elven Scout | Normal | 60 | 5 | 3s | Bow pressure; focus weakness/stagger |
| Orc Trailguard | Normal | 90 | 15 | 4.5s | One heavy axe hit, existing shield gate |
| Elven Mender | Special | 75 | 5 | 5s | Ready after 2 moves; fixed other ally below 75%, +20 after 2 future responses; interrupt/kill target; 2 recovery moves |
| Orc Rootbinder | Special | 75 | 5 | 5.5s | One 1-hit Root, four nonspreading vines; clear a side vine then hit through it |
| Barkhide Warden | Miniboss | 150 | 15 | 4.5s | One 2-hit spreading Root; all allies take 25% less damage while it lives |
| Briar Matriarch | Boss | 240 | 10 | 5s | Two 2-hit Heartroots; Renew (2 moves, 20 +20/root AoE heal), Surge (advance vines), Harvest (3 moves, 20 +5/vine damage, consumes vines) |

Revised 2026-10-03: vines only obstruct visuals. Both Heartroots destroyed causes
normal stagger. Exposure and its weakness bonus are removed. Invalid Mender
targets fizzle without retargeting or stagger. Exact board/save/timing rules:
[foundation contract](FOUNDATION_CONTRACT.md). Numerical bases remain test tuning.

## Animation delivery and source checks

40 clips / 304 selected frames. All six have Idle, AutoAttack, Hit and Death.
Mender, Rootbinder, Warden and Matriarch also have ChannelStart, ChannelHold,
Release and Interrupt. Interrupt intentionally reuses recovery drawings with a
separate amber cancellation cue; it is not a new paid clip or heal event.

Ordinary sheets use a fixed **96×64** canvas with transparent side padding around
the original 64px body. Matriarch uses **96×96**. PPU 64; pivot bottom-center;
Point sampling; uncompressed; no mipmaps; Full Rect; binary alpha. The six source
palettes remain exact, with 13/14/13/14/14/17 colors in their approved stills.
Moving frames use subsets, not an assertion of identical masks or palette counts.

Every actual PNG has its own hash, bounds, occupied contact row and palette count
in `Selected/animation-manifest.json`. Idle feet are at row 63 / 95 throughout.
One-pixel generated idle drift was repaired without resampling. Rootbinder's
upright staff tip was repaired to wood and a split branch; raw output is retained.
Unrequested cyan particles were removed from Matriarch recovery. First/last ready
poses retain the exact approved drawing; side padding never scales the body.

New idles remove the generated duplicate closing frame. The already-approved
Mender pilot is reused with its documented 130ms endpoint hold; it is not silently
retimed. The HTML player uses declared millisecond durations and shows native,
2× and 3× scales. GIFs are convenience previews and have GIF timing quantization.
ChannelStart moves into Hold; only the state outcome chooses Release/Interrupt.

The basic attack sets use the existing guarded impact/completion events. Special
clips have no gameplay events. Death drawings run within the existing death
presentation duration instead of adding an arbitrary combat wait.

## Actual gameplay-screen inventory

| Visible role / hierarchy owner | Binding and status |
|---|---|
| Global/overscan outside framed sections | Existing accepted vertical `General_Timber_v1`; complete |
| Battle backing and surround | Existing battle frame + woodland `BackWall`; one backing, no duplicate illustration; complete |
| Distant forest | Separate 256×256 `Tall_Woodland`, repeated at native world scale; complete |
| Midground | `Architecture` ancient tree and `BackDecor` separate quiet ruin; complete |
| Combat soil | `Floor` 128×64 soil module, extended behind actual integer-size foot contacts; complete |
| Foreground | `AtmosphereProps` separate fern; complete |
| Board backplate/cells | Three opaque 64×64 rounded-square endgrain cells; mirrored/rotated native variants; edge-to-edge with wood-backed corners; complete |
| Cell targeting/selection | Existing gem highlight/target owners preserved; hostile warnings/roots are independent overlays; complete |
| Board and battle border/corners | Accepted joined-wood pieces with native tiled edges; complete |
| Wave/zone presentation | Accepted wave plaque, runtime wave text; Emerald resonance label/leaf in free bottom hint space; complete |
| Player and bottom-HUD backing | Separate accepted dark horizontal `Panel_Timber_v1`; complete |
| Health / rank / affinity / weakness | Accepted simple neutral health shells, original fills, vibrant rank badges and small gem borders retained; complete |
| Ability / energy | Independent timber shell/energy frame; existing native center glyphs extracted from approved icons; fill mask and cost unchanged; complete |
| Supplies | Bright wooden tile states with existing Potion/Bomb glyphs and counters; complete |
| Guide and in-run settings button | Independent timber/gear theme states; complete |
| Guide, upgrades, game-over/retry panels | Independent tiled theme sprites; text remains runtime; complete |
| Opened settings / main menu | Existing presentation retained; excluded from forest theming |
| Hostile vines / roots | Distinct vine edge, amber anchor knot, blinking gold warning; gem centers readable; complete |
| Heal / interruption | Small separate leaf and amber cancellation bursts, existing combat-number events; complete |
| Music | Original authorized temporary cue integrated; final listening/mix approval **pending** |

Intentional changes from the small accepted proof: the study has become separate
native woodland/ground/prop modules; soil now reaches the measured foot plane;
the battle section reserves 320 logical pixels for the approved larger apex.
The log silhouette fills its cell instead of presenting a circular disk. Actor
transforms, pixel density, shared gem art and the menu/settings assets are kept.

## Music brief, provenance and playback status

**Forest Lanterns — TEMP review**: 137.142857 seconds, 112 BPM, 64 bars in four
sections. Original note/arrangement data and local synthesis are in
`Tools/render_forest_temp_music.py` and `Production/Audio/score.json`. No downloaded
track, sample library, named-game composition or paid music service was used.
These project-created sources and WAV accompany the review; no external track
license is required by a third-party source. Do not label this temporary cue final.

Direction: light reed melody, plucked wood, seed chimes and restrained percussion;
stable combat pulse with harmonic/voicing variation across the four sections.
Keep repeated two-minute cycles tolerable across substantial forest visits.
Final review should judge melody fatigue, loop transition and important attack/
interrupt cues over the cue, then decide whether to retain, revise or replace it.

WAV: stereo 44.1kHz, 16-bit; rendered peak 0.68, RMS about 0.123, zero clipped
samples. Loop-boundary step 0.00208 versus maximum adjacent step about 0.263.
Unity import streams Vorbis at quality 0.8 with original sample rate/channels.
The existing music preferences/volume remain authoritative. A 0.75s crossfade
uses at most two sources temporarily, then clears the outgoing clip; entering a
boss in the same zone does not restart the loop. Pause/background requests pause
both sources; menu return selects the existing menu track.

Automated tests cover source count, clip metadata, mute, pause/background handler,
same-zone continuity and crossfade completion. Numerical audio checks do not
establish listening quality or physical-device background behavior. Human music
approval and on-device SFX/music mix remain explicit open items.

## Generation usage and review boundary

Actual balance: 1856 → 1776 remaining; 144 → 224 used. **80 generations** used
across 51 successful jobs; four rate-limit refusals were not queued or charged.
Reported/provisional per-job costs reconcile to that balance change. Initial
allowance 90, total ceiling 120, 30 reserved after review. No credit purchases.
`usage-reconciliation.json` records every job, including rejected/refined outputs.

Subsequent approval: Treant C is locked, root/vine art and motion are integrated,
and the user authorized connected crystal travel and merging. The additional
24 generations bring Phase 5–6 usage to **104/120**; see
`ArtSource/Forest/VinesAndTransition/usage.json`. Aquatic, expanded roster kits,
ten-minute pacing evidence and final music approval remain open. See
[travel details](CRYSTAL_TRAVEL.md).
