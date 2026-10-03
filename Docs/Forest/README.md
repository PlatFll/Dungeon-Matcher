# Forest starter production — Phases 5–6

Branch: `codex/forest-enemy-production`, stacked on the unmerged Phase 4
`codex/forest-gameplay-foundation`. Six starter kits and their battle animation
sets, modular woodland, timber gameplay UI and a temporary original cue are ready
for review. The user's original checkout and open foundation project are separate.

## Play

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
- `usage-reconciliation.json`: actual **80 / 90 initial / 120 total** generations.
  Ten initial generations remain unused; the 30 correction reserve is untouched.

Serve the repository locally with Python's HTTP server to open the review page;
its manifest loading needs HTTP. The current review server uses port 8881.
`Tools/forest_export_production.py` reproduces native exports. Run **Import
production art and kits** in the Forest menu to rebuild Unity bindings.

## Review gate and limits

Review faces/equipment, attack contact, channel interruption, ground placement,
HUD readability, kit counterplay and the original temporary cue. Music is **not
final**: human listening/mix approval remains open. Device performance and Android
audio/touch behavior also remain unverified.

This is a starter test roster, not the complete forest or proof of ten-minute
visits. Full content, live random crystal travel, further progression and aquatic
work remain later scope. No public mode selector, forced waits, HP padding or
run cap was added. Stop for review; no merge or next phase is authorized.
