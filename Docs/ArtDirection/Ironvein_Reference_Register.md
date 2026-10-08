# Ironvein Excavation — reference and production register

2026-10-08. **VISUAL REVIEW PENDING.** Implementation and generation were
authorized. Internal selections, measured pixels and passing tests do not imply
the user approved these new character designs, motion or temporary music.

## Authority

The current shared guide (`Dungeon_Matcher_Art_Direction.txt`, v1.11) owns outline,
material clusters, top-left light and native pixel rendering. Existing individual
Miner, approved humanoid/forest sources and the historical reference board were
inspected. The supplied full-screen cave mockup owns composition: dark recesses,
warm orange ore and lanterns, timber, minecart/rails and industrial machinery.
Its 941×1672 concept pixels are not runtime sprite sources.

## Candidate locks for this review branch

| Group | Exact authority / status |
| --- | --- |
| 13 enemies + Rivet Turret | `ArtSource/Ironvein/Selected/*.png`; individual measurements in `Production/selected-technical.json`; candidate material cleanup preserves source silhouettes |
| Native action canvases | `Inputs/`; 80×80 ordinary, 112×112 large actors; integer transparent padding preserves drawn size and boot anchors |
| 108 clips | `Production/motion-selections.json` and exported `Motion/`; original frames retained in `RawMotion/`; exact frame selection/contact timing |
| Grand Delver experiment | `Variants/` pilot/reserve suit and their clips; optional gameplay remount flag remains disabled by default |
| Cave modules | `Scene/`; selected `cave_depth` and `cave_floor_v2`, timber, rail junction, cart, ore, lamps, winch, pump and banner |
| Cave scenes | `Ironvein_Railhead`, `Ironvein_Pumpworks`, `Ironvein_DeepShaft`; same existing battle-floor/viewport owners |
| Gameplay UI | `UI/Prepared/technical.json` and source receipts; quiet charcoal surround, distinct steel insets, square cells and restrained copper/iron borders |
| Stones / machines / VFX | `Mechanics/*/technical.json`; material and damaged variants, distinct Core, small/fixed drill, fuse, ore and break frames |
| Audio | `Audio/score.json`, `render-report.json`, original temporary 120-second cue and twelve mono effects; **LISTENING REVIEW PENDING** |

The source README records rejected takes and reproduction steps. No original
source is relabelled as an approved replacement. Selected actor stills use exact
palette maps where needed; all alpha masks and source pixels outside those maps
are checked. Local neutral-background galleries show individual PNGs at 1× and
integer zoom beside approved references. A composite does not establish the
individual dimensions, palette counts, alpha or scaling: consult the measurements.

## Actual screen proof

Four Unity captures: 720×1280, 1080×1920, 1080×2400 and 1080×1920 with safe
insets. Gallery: `.utmp/Ironvein/ScreenReview/Review.html`; still and motion
galleries sit beside it in `ArtReview` and `MotionReview`. These are actual
Unity scenes, not mockup overlays. They include three actors, large Grand Delver,
stones/fuses, fixed drills, existing six gem colors, player/ability and controls.

Native UI dimensions were corrected to the existing layout: 80×80 L corner,
64×16 edge, 128×32 wave plaque, 144×32 energy frame and 146×16 player slices.
All runtime imports use Point filtering, no mipmaps and uncompressed textures.
The original player, skill art, gems, HP/rank family, main menu and opened options
remain their existing approved assets. Character silhouettes and weapons retain
their authored native size; big attacks use extra transparent canvas space.

Remaining visual review: dense three-actor overlaps, ore/stone readability on a
physical phone, impact/recovery feel, music character and volume. No device or
human approval is claimed. Technical execution is recorded separately in
[the validation record](../IronveinExcavation/VALIDATION.md).

## Generation accounting

Ironvein-only approval: 400 total, at most 300 initially, 100 reserved. Exact
quoted usage **295.2 across 185 requests**, including one cancelled request
conservatively. The 2026-10-08 provider balance reports 935 used / 1,064 remaining,
compared with 640 used at the initial check; the provider rounds fractional usage.
Purchased-credit balance remains $0. No reserve used, no purchases, no pending
remote jobs. Full receipts and the per-request ledger are retained.
