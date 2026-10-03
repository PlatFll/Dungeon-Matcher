# Forest Phase 4 — technical review record

2026-10-02 · Unity 6000.3.19f1 · Windows Editor

## Review scope

The isolated forest prototype is ready for technical and playable review.
It is based on main at `d334b5278748dcd303ac931eb71b0aa7328dd15b`, on
`codex/forest-gameplay-foundation`. The original open checkout was preserved.
No merge, production release, full animation or later-phase work is included.

Six user-approved stills were compared with the current material/shading guide.
They remain byte-identical to the guide-colored Phase 3 sources: five 64×64
sprites and the declared 96×96 Matriarch. Native palette counts are 13, 14, 13,
14, 14 and 17 respectively; alpha is binary. Exact hashes, bounds and material
ramps are in `ArtSource/Forest/Approved/manifest.json`. The Unity import check
compares source/import bytes and verifies Point filtering, no mipmaps and no
compression. **Phase 4 PixelLab usage: 0 requests / 0 generations.**

## Delivered prototype

- Four playable starter kits, three looping development formations and separate
  development saves. Warden and Matriarch remain approved still references.
- One accepted-move coordinator around the existing board pipeline: player
  consequences first, stable surviving actors next, then expiry and control.
- Explicit basic cadences, move-based buffs/status/cooldowns, owned commands,
  no idle recharge, and versioned continuation with accepted-input replay.
- Fixed-target, interruptible +20 Mender heal with two future response moves and
  two recovery moves; no same-action basic or late completion after removal.
- Vines through canonical movable pins, warning queue and restriction budget;
  separate environment/enemy ownership, two-move growth grace and one spread
  per action. Clearing a parent immediately releases its pending reservation.
- Attributed Emerald +15% damage and a generic current-gem event for later cards.
- Accepted forest wood/UI assets through existing layout owners. Move intent
  follows the rendered sprite bounds at both tested portrait sizes.

Production dungeon runs and version-1 saves retain their recorded seconds
profile. Forest is ineligible for live selection. Menus, opened settings,
production encounter data, shield gating and existing character art retain
their established behavior.

## Executed tests

**297 unique behavioral test cases and one rendered scene check have passing
results across the runs below.** The combined run had one failing fixture;
its correction and successful rerun are retained explicitly. Counts are not
inflated by adding repeated tests or internal lifecycle assertions.

| Run | Actual result | Evidence |
|---|---|---|
| Combined forest + dungeon regression | 296 total: 295 passed, 1 failed, 0 skipped | [Original XML](ForestPhase4/regression.xml) |
| Corrected dungeon fixture + rendered forest scene | 2 passed, 0 failed, 0 skipped | [Rerun XML](ForestPhase4/recheck.xml) |
| Full recipient, non-stagger hit and deadline-lethal boundary | 1 passed, 0 failed, 0 skipped | [Boundary XML](ForestPhase4/channel-boundary.xml) |
| Required `Tools/Validate-Unity.ps1` | Passed, exit 0 | [Validator output](ForestPhase4/unity-validation.txt) |

[Evidence index](ForestPhase4/evidence.json) records raw-file hashes, local log
paths and the latest result for every unique test name. Full engine logs remain
in this worktree's `.utmp/ForestValidation/`; the required validator's full log
path is retained in its output file. NUnit XML above is copied from Unity,
including the superseded failure, rather than reconstructed test output.

### Forest coverage

- Sequential action identity; failed swaps; thinking without moves; no passive
  changes to enemy readiness, poison, stagger, Royal Decree, energy or supplies.
- Actual accepted swaps, a cascade of at least three stages and a special chain
  clearing more than 40 gems; each consumes only one manual action.
- Actual free Bardley casts, chains and supplies create no additional tick.
- Mender trigger/deadline/recovery; stable target through resume; deadline
  stagger and lethal damage; sub-threshold hits; full recipient without
  retargeting; dead recipient with reused slot; caster removal; single outcome.
- Actual vine growth, full child grace, at most one spread, shared capacity,
  simultaneous warning scheduling, owner cleanup, environmental survival,
  free parent clear and reservation release.
- Mid-action accepted-input replay produces the same board, enemy and player
  state exactly once. Unknown profiles preserve the durable save byte-for-byte.
- Board photograph keeps present vine ages/deadlines and independent owners.
- Color/source attribution matrix and real 30→35 Emerald / 30→30 Ruby packets.
- Royal command participant ownership, follow-up hits and interruption during
  command windup without a deadlock or additional ordinary attack.

The existing 279 dungeon cases cover Balance v1, revised-design continuation,
cards, damage results, game-over recovery, royal actions, ChronoShutter,
counterplay, gravity/pin reservations, wave/death lifecycle and pointer identity.

### Findings fixed during validation

Unity's JSON serializer can materialize a default nested clock object in an old
save. Profile selection now follows the schema version: version 1 stays legacy.
Spawn presentation also no longer resets restored move readiness.

A King-command participant staggered during windup cannot wait for a future
move while owning the current action. The move profile skips that strike and
releases its reservation. The legacy seconds path retains its existing wait.

The one failure in the final combined run was a dungeon test fixture that added
every Editor enemy asset to `seenMilestoneLeaders`. New forest references then
entered that artificial dungeon save. The fixture now uses its actual dungeon
database; its King→wave 31→save/resume→death checks pass without changing save
validation or adding forest enemies to production selection.

Visual inspection caught counters crossing enemy faces at the tall size, which
the general layout validator did not cover. Intent now follows actual rendered
sprite bounds, and the rendered test asserts separation at both sizes.

## Rendered evidence

Actual `Game` scene, approved static enemies and real runtime HUD:

- [720×1280 idle](ForestPhase4/01-forest-idle.png)
- [720×1280 channel intent](ForestPhase4/02-channel-intent.png)
- [1080×2400 channel intent](ForestPhase4/04-forest-tall.png)
- [Opened settings](ForestPhase4/05-settings-preserved.png)
- [Main menu](ForestPhase4/03-menu-preserved.png)
- [720×1280 layout report](ForestPhase4/layout-720x1280.txt)
- [1080×2400 layout report](ForestPhase4/layout-1080x2400.txt)

Both layout reports have zero validator errors. Screenshot channel state is
installed through the continuation owner to make intent repeatable; actual
trigger, deadlines and outcomes are exercised separately by the behavior tests.
The two sizes use the existing project's board scaling policy. No new claim of
integer board scaling or physical-device validation is made.

## Limits and next review

The repeated woodland study and existing chain/green warning overlays are
development presentation. They are not a finished production scenery set or
final vine art. No forest attack/channel animation is produced in this phase.

The formations are a focused test fixture, not the finished forest roster,
random crystal travel, aquatic content or proof of ten-minute visits. Cadences,
growth and damage are review settings. Automated tests do not establish human
fun, final balance, physical Android behavior or performance.

Use [the play instructions](../Forest/README.md). Review cadence, heal response,
vine counterplay, readability and save/resume before full animation production
or another phase. No permission to merge is inferred from still-art approval.
