# Forest starter production — Phases 5–6

Six starter kits, native animation sets, modular woodland, timber gameplay UI and
a temporary original cue are integrated. Live dungeon ↔ forest crystal travel is
now connected. The extra seven approved roster designs remain stills only.

## Play

Normal Play starts in the dungeon and travels after the King formation to the
forest, then returns after the Matriarch formation. Global depth, board, build and
resources carry over. Settings remains usable over the smoke. See
[the travel contract](CRYSTAL_TRAVEL.md).

For direct development testing:

Open this branch's project in **Unity 6000.3.19f1**. Choose **Dungeon Matcher →
Forest → Play new isolated test**. It creates a separate save under
`.utmp/ForestPlaytest` with three test Potions and Bombs. Stop Play Mode, then
choose **Resume isolated test** to reopen it. Suspend to Menu also works.
Production gold, supplies and character progression are preserved.

The **Fixtures** submenu starts any of the six enemies alone, Rootbinder with
attackers, Warden with Scout/Trailguard, or Matriarch with attackers/Rootbinder.
The default encounter demonstrates Trailguard + Mender + Scout. Thirteen small
formations loop through the existing endless pipeline. Mender alone deliberately
has no eligible heal recipient; damage Matriarch/allies to demonstrate her AoE Renew.
The revised root/vine rules are in FOUNDATION_CONTRACT.md; exposure is retired.

New tests use **seconds basics and move-based abilities**. Earlier move-profile
saves keep their recorded rules. Buffs, stagger, poison and supply cooldowns retain
the Phase 4 move semantics. Paused inspection and accepted board resolution stop
the basic countdown; a special hold resumes its stored seconds afterward.

## Review materials

- [Production inventory and kit values](PRODUCTION_REVIEW.md)
- [Timing and save contract](FOUNDATION_CONTRACT.md)
- [Timer ownership/profile details](CLOCK_IMPLEMENTATION.md)
- [Current root revision evidence](../Validation/FOREST_ROOT_REVISION.md)
- [Earlier production evidence](../Validation/FOREST_PRODUCTION_PHASE_05_06.md)
- `ArtSource/Forest/Production/Selected/Review.html`: true-timing animation player,
  native/2×/3× display, still frames, modular art, Unity captures and music.
- `ArtSource/Forest/Production/Selected/animation-manifest.json`: every native
  frame's dimensions, palette count, alpha, bounds, contact row, hash and timing.
- `ArtSource/Forest/Production/Raw`: original service outputs; `Selected`: repaired
  exports. `Inputs/MenderPilot` preserves the already-approved pilot source.
- Original production `usage-reconciliation.json`: **80 generations**. Subsequent
  root/vine/smoke work used 24, bringing Phase 5–6 to **104/120**, with 16 remaining.
  See `ArtSource/Forest/VinesAndTransition/usage.json` and its native art checks.

Serve the repository locally with Python's HTTP server to open the review page;
its manifest loading needs HTTP. The current review server uses port 8881.
`Tools/forest_export_production.py` reproduces native exports. Run **Import
production art and kits** in the Forest menu to rebuild Unity bindings.

## Review gate and limits

Review faces/equipment, attack contact, channel interruption, ground placement,
HUD readability, kit counterplay and the original temporary cue. Music is **not
final**: human listening/mix approval remains open. Device performance and Android
audio/touch behavior also remain unverified.

The six playable enemies remain a starter roster, not proof of a complete forest
or ten-minute visits. Expanded content and aquatic work remain later scope. No
public mode selector, forced waits, HP padding or run cap was added. The user
authorized merging the current root/vine and crystal-travel work. The additional
seven stills have no new animation or kit approval.
