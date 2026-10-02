# Forest Phases 5–6 — verification and review gate

2026-10-02 · Unity 6000.3.19f1 · Windows Editor

Branch `codex/forest-enemy-production`, based on Phase 4 commit
`9906ae3fc06f8bd2763e249d3dd9eeeeab451b24`. The original user checkout and open
foundation project were preserved. No merge or device release is authorized.

## Actual results

| Check | Result | Evidence |
|---|---|---|
| Broad forest + dungeon selection | 329 total: 294 passed, 35 failed | [Original XML](ForestPhase5_6/broad-regression.xml) |
| Forest subset in that run | All 27 passed | Same XML: AcceptedMoveStateTests / ForestFoundationTests / ForestFoundationPlayTests |
| Same four failing lifecycle/audio classes on unchanged Phase 4 | 75 total: 41 passed, **same 34 failures** | [Baseline XML](ForestPhase5_6/baseline-lifecycle.xml) |
| Combined graphics check | Forest 3 passed; dungeon ability case timed out at King settlement | [Recorded failure](ForestPhase5_6/graphics-timeout.xml) |
| Unchanged baseline dungeon ability test with graphics | 1 passed | [Baseline graphics](ForestPhase5_6/baseline-dungeon-abilities.xml) |
| Isolated current dungeon ability test, unchanged timeout/assertions | 1 passed | [Current rerun](ForestPhase5_6/dungeon-abilities-rerun.xml) |
| Final forest battle/milestone/layout capture set | 3 passed | [Rendered suite](ForestPhase5_6/forest-visuals.xml) |
| Guide clipping / Refine button correction and final panels | 1 passed | [Final panel rerun](ForestPhase5_6/final-panels.xml) |
| Native image/import audit | 40 sheets / 304 frames passed | `ArtSource/Forest/Production/technical-audit.json` |
| Required Unity validator | Passed, exit 0, after final UI fixes | [Validator output](ForestPhase5_6/unity-validation.txt) |

Across latest results there are **332 unique cases: 298 passing and 34 unresolved
baseline failures**. Repeated tests do not inflate that total. The first dungeon
visual attempt used `-nographics` and reported an infeasible headless viewport;
the correctly rendered attempt then hit a King settlement timeout. Its isolated
rerun passed with no gameplay or test relaxation. That intermittent timeout is
retained, not silently reclassified as a success or given an unverified cause.

The 34 baseline failures are in CombatAudioTests, PlayerAbilityLifecycleTests,
RunUpgradeLifecycleTests and UpgradeIntermissionLifecycleTests. Their exact
names are in [the evidence index](ForestPhase5_6/evidence.json). Both checkout
runs produce the same failure set; these cases are not claimed fixed. No test
assertion was removed, weakened or marked ignored to get a green report.

## Behavior exercised

- Timed basics damage the player without a move; move counts stay unchanged.
  Board ownership, pause and specialist holds preserve seconds progress.
- Both version-2 profiles and existing version-1 continuation remain explicit.
  Hybrid restore keeps its clock interpretation and stored attack progress.
- Existing accepted-move tests cover long cascades, free skills/supplies,
  exactly-once identity, buffs/stagger/poison, target lifetime and replay.
- Mender's fixed ally, exact deadlines, lethal/stagger cancellation, no self heal,
  recovery and photo/counter boundaries remain covered.
- Warden warning/stagger cancellation; single protection multiplier for two
  anchors; no growth; exposed weakness packets; direct-hit distinction; photo
  cannot restore a completed anchor cast; exposure expiry.
- Matriarch natural readiness, fixed persistent target, anchor continuation,
  positive surviving-anchor heal exactly once, interruption/target death and
  solo self target. Release animation cannot issue a duplicate heal.
- Music clip/source metadata, one player, bounded crossfade source count, mute,
  game pause and background-pause handler, same-zone continuity and menu return.
- Existing damage/shield, upgrades, continuation, wave/death and dungeon command
  tests run in the broader selection. Existing graphics test exercises approved
  dungeon effects, shield clearance, missing-art fallback and King's two strikes.

## Rendered and browser review

Actual Game scene captures:

- [720×1280](ForestPhase5_6/01-forest-idle.png)
- [1080×2400](ForestPhase5_6/04-forest-tall.png)
- [1080×1920](ForestPhase5_6/06-forest-1080.png)
- [Inset safe area](ForestPhase5_6/07-forest-safearea.png)
- [Warden](ForestPhase5_6/08-warden.png), [Matriarch](ForestPhase5_6/09-matriarch.png)
- [Guide](ForestPhase5_6/10-guide.png), [upgrades](ForestPhase5_6/11-upgrades.png),
  [game over](ForestPhase5_6/12-game-over.png)
- [Opened settings](ForestPhase5_6/05-settings-preserved.png),
  [main menu](ForestPhase5_6/03-menu-preserved.png)

All tested battle layouts have zero layout-validator errors and assert that
intent labels remain above their rendered actors. Soil was placed against
measured foot contacts; actors were not rescaled to fit scenery. Tilemap placement
now floors cell coordinates before applying fractional offsets, avoiding the
half-cell rounding collisions that opened scenery gaps. A taller battle reserve
fits the native 96px apex without overlapping the wave plaque.

Panel inspection caught a solid stencil block over guide text; its text-only
viewport now uses RectMask2D. Refine buttons received the missing gameplay theme
binding. Final screenshots were inspected after those corrections.

The local HTML review page was checked in the in-app browser. Native canvases
are 96×64 / 96×96; 3× CSS sizes are 288×192 / 288×288. ChannelStart reaches Hold
and stays there until a selected Release/Interrupt. The browser loads the
137.142857s music with no media error. This verifies presentation/availability,
not subjective animation or audio quality.

## Limits

Music is an original **temporary** cue pending human listening and mix approval.
Background behavior was tested through its handler; no physical mobile background
run or Android performance/touch/audio test is claimed. Full forest content,
random live travel and ten-minute pacing remain later work. The existing 34
baseline failures and intermittent dungeon-test timeout remain visible above.

The [production inventory](../Forest/PRODUCTION_REVIEW.md) marks all art/audio
roles and gives exact kit settings, generation usage and source provenance.
Stop for art, motion, kit, full-screen and music review. No merge.
