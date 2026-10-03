# Zone effects and Royal Arbalist — 2026-10-03

Implemented on `codex/zone-hazard-lifecycle` from main `a49a21b`.
The user authorized implementation and merge. Work and Unity validation used an
isolated checkout; the open primary project and its three existing consumable
metadata edits were preserved.

## Fixes and behavior

- Environmental vines were explicitly removed by `WaveCompleted`, which also
  precedes the perk draft. They now live for the zone visit. Scene destruction
  also queued delayed global vine cleanup; travel now cleans the detached
  destination checkpoint before reveal, and old dungeon saves discard forest
  overlays during restoration.
- Dungeon crumbling tiles use the existing board mutation queue, mining
  occupancy and normal refill pipeline. Every four accepted moves, one or two
  safe ordinary cells shake with their gems, flash white and break. After two
  further accepted moves they flash white, materialize and refill. This cadence
  is initial tuning. Shared structural caps and useful-response checks can
  reduce or skip a pulse.
- Hole deadlines survive save/resume and cannot be rewound by Gideon's photo.
  Zone travel removes source-zone vines, roots and holes in an atomic checkpoint;
  a failed save leaves the source board and durable checkpoint intact.
- Royal Arbalist is Special, casting up to two chains every four moves, capped
  at two owned. A dedicated 1.16-second clip reuses approved native drawings,
  with contacts at 380/700 ms. Its basic attack, stats and art pixels retain
  their existing values. No paid generation was used.
- Edge-case review also fixed a paused shake moving for one extra frame and
  chains surviving ability disable. Death between chain contacts cancels the
  second shot. Restoring a mined cell snapshot no longer replays its break flash.
- Arbalist's disruptor classification required two recipe updates: allow its
  one disruptor in `royal-fireline`; replace it with the same-cost Swordsman in
  `court-formation` to retain the two-disruptor limit. Global caps are unchanged.

The board remains authoritative; VFX only presents events. Existing damage,
shield gating, timed basic attacks, accepted-move abilities and forest kits
continue through their established systems.

## Executed checks

Unity **6000.3.19f1**, Windows Editor. **163 unique tests have passing latest
results**, deduplicated by full test name across the following runs:

| Run | Result | Evidence |
| --- | --- | --- |
| Forest roster, travel, picker, clock, motion and chain reservations | 89/89 passed | [XML](ZoneEnvironment/broad-regression.xml) |
| Combat, shields, specials, photos, banners and zone hazards | 80/82 passed before two fixture corrections | [Original XML](ZoneEnvironment/combat-regression-before-fixture-update.xml) |
| All ten new environment cases plus both corrected fixtures | 12/12 passed | [XML](ZoneEnvironment/final-affected-checks.xml) |
| All authored recipes and 100 seeds of constrained formations across waves 1–30 | 2/2 passed | [XML](ZoneEnvironment/encounter-data.xml) |
| Required `Tools/Validate-Unity.ps1`, final runtime and serialized data | Passed, exit 0 | `DungeonMatcher-UnityValidation-70de80bf-6c29-4042-bf90-300dab4866ae.log` in the local user's Temp directory |

The two fixture corrections retain their intended assertions. The deterministic
photo/refill test isolates independent zone interference; the new photo/hole
case exercises that interaction separately. The endless-King test now uses the
portrait Game view and follows the new scene's `RunSession` through live travel,
instead of retaining the destroyed source controller. Initial implementation
runs also exposed fixture setup issues and the paused-shake defect; those were
corrected before the passing runs above.

The ten new cases cover wave and perk persistence, paused draft continuation,
cleanup beneath transition smoke, exact two-move restoration across resume,
safe targeting, shared Miner limits, photo interactions, failed travel writes,
two animation contacts, death between contacts and disabled-owner cleanup.
Existing regression cases cover the new forest kits, root/vine behavior, saves,
King action ownership, shield gating and normal chain counterplay.

## Visual evidence

Actual Unity 1080×1920 captures were inspected:

- [Vines during perks](ZoneEnvironment/forest-vines-during-perks.png)
- [Clean destination](ZoneEnvironment/clean-dungeon-arrival.png)
- [White break](ZoneEnvironment/dungeon-white-break.png), [holes](ZoneEnvironment/dungeon-broken-tiles.png)
- [White restoration](ZoneEnvironment/dungeon-white-return.png), [refilled tiles](ZoneEnvironment/dungeon-tiles-returned.png)
- [Special Arbalist and two chains](ZoneEnvironment/royal-arbalist-two-chains.png)

These are targeted automated and Editor visual checks, not a new full-suite or
physical-device certification. The previously documented 34 unrelated baseline
lifecycle/audio failures were not reclassified as passing. Human tuning of the
four-move hazard cadence and on-device motion/visibility remain open.
