# Forest roots, vines and crystal travel — 2026-10-03

Unity 6000.3.19f1, Windows Editor. The user approved Treant C, PixelLab root/vine
art and motion, live dungeon ↔ forest travel, and merging the forest stack.
The original dirty checkout is preserved; implementation used the existing
forest worktree on `codex/forest-vines-and-crystal-transition`.

## Implemented

- Treant C Old Stump is locked without changing its PNG. All seven additional
  roster designs remain stills only. Existing starter character animations are
  unchanged.
- Separate native 64×64 root tiers, a vine weave with 81.64% transparent pixels,
  nine spread frames and twelve recoil/retraction frames. Root art follows
  remaining durability. These visuals never authorize board mutations or damage.
- Entire apex formations trigger travel after cleanup and card selection.
  The saved destination uses its own random stream. With two eligible regions,
  dungeon and forest alternate; aquatic is unavailable.
- The approved pink crystal, PixelLab smoke, settings above the curtain, paused
  combat and durable scene restoration preserve the same run, board and resources.
- Legacy dungeon effect units remain seconds. Earlier isolated forest saves keep
  their loop and recorded timing profile, including saves without travel metadata.
- Fixed upgrade UI attaching to a persistent temporary canvas during scene reload;
  it now belongs to the gameplay scene and remains available at later card rewards.

## Executed validation

| Check | Result | Evidence |
|---|---|---|
| Affected forest, travel, shared combat/board, continuation and rendered layouts | **111 passed, 0 failed, 0 skipped** | [Combined XML](ForestTravel/fc709231-8ddc-4e25-b932-f61fbc712a06.xml) |
| Final legacy-save/profile correction and affected travel/continuation rerun | **14 passed, 0 failed, 0 skipped** | [Final XML](ForestTravel/e08a3485-2b52-48b3-9d37-d85a83d116cc.xml) |
| Required `Tools/Validate-Unity.ps1`, final runtime source | **Passed, exit 0** | [Unity log](ForestTravel/unity-validator.txt) |
| Native source/import audit | Passed | `ArtSource/Forest/VinesAndTransition/{stills,motion,usage}.json` |

There are **113 unique passing cases** across these runs. The later correction
changes only default travel-schema detection and the enable flag for older
isolated forest saves. The fourteen-case rerun covers those paths, both live
travel directions and continuation. The remaining earlier checks retain the same
source/configuration. No test was ignored and no timeout increased.

Travel tests verify: King escort survival delays travel; global depth continues;
board identities/types/specials and refill RNG carry over; HP/shield/energy and
the selected card survive; the Matriarch earns a milestone rather than another
King reward; settings receives actual raycast/click input over full smoke;
pending and committed trips survive suspension without rerolling or duplicate
rewards; a locked checkpoint file preserves source bytes and permits retry;
reduced-motion failed preparation releases input/time; effects count seconds
without accepted moves in the travel profile.

Commands use `Tools/Test-ForestFoundation.ps1 -Graphics -Filter ...`; the exact
selected test classes and every result are in the XML. Rendered checks cover
720×1280, 1080×1920, 1080×2400 and a simulated inset safe area. They do not
establish physical-device behavior.

## Visual inspection

- [Crystal and smoke](ForestTravel/04-crystal-smoke.png)
- [Settings above smoke](ForestTravel/01-settings-over-smoke.png)
- [Arrival in forest](ForestTravel/02-dungeon-to-forest.png)
- [Return to dungeon](ForestTravel/03-forest-to-dungeon.png)
- [Roots and open vines](ForestTravel/09-matriarch.png)
- [Warden](ForestTravel/08-warden.png)
- [Inset safe area](ForestTravel/07-forest-safearea.png)

The two root silhouettes read distinctly and gem colors remain visible through
the vine weave. Portrait captures show restored zone scenery/UI and grounded
characters. Test captures with large countdown values use quiet combat fixtures;
those values are not production tuning. The smoke is opaque over gameplay, with
the settings control above it. No main-menu/opened-settings reskin was introduced.

## Usage and limits

PixelLab balance was checked before production: **1726 → 1702 remaining**,
**274 → 298 used**. Actual use is **24 subscription generations**, including four
stills (20) and four animation requests (4). The first motion attempts were
rejected; selected hit motion combines corrected recoil poses with reversed
generated growth frames. Sources, jobs, hashes and frame timings are retained.
No credit purchases. Phase 5–6 totals **104/120**, leaving 16; the separate
additional-roster still allowance remains **50/60**.

Early travel iterations found the temporary-canvas card bug and the default
nested-JSON migration bug; both have passing regression coverage above. Other
failed iterations were test setup/synchronization errors (skipped milestone
history, nonsequential reward fixtures, capture before death cleanup, pausing
cleanup, and relying on an EditMode wait instead of elapsed game time). Local
iteration logs remain in `.utmp/ForestValidation`.

The earlier production baseline's **34 lifecycle/audio test failures** remain
documented in [the prior validation record](FOREST_PRODUCTION_PHASE_05_06.md).
This is an affected-suite result, not a claim that every repository test is green.
Forest still has six playable starter enemies, temporary eighteen-encounter visit
bands and temporary music. Expanded content, ten-minute pacing evidence, final
listening approval and Android/device testing remain open.
