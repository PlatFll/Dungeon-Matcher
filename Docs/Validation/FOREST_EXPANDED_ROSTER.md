# Expanded forest roster and selected blockers — 2026-10-03

Unity 6000.3.19f1, Windows Editor. Implementation is isolated on
`codex/forest-roster-production`; the user's open main checkout and its three
existing consumable import changes are preserved. The user approved merging
PR #176 on 2026-10-03. The merge checkpoint changes documentation only; gameplay,
art, serialized state and validation tooling match the tested `ebbac09` commit.

## Delivered

- Seven approved designs, including Old Stump Treant C, with 39 imported clips:
  idle, basic attack, hit/death and the appropriate casts or held warning poses.
  Basic impacts and completed casts use existing actor events and ownership.
- The seven [approved kits](../Forest/EXPANDED_ROSTER.md), nine additional test
  encounters and eight live formations. Basic attacks still count seconds;
  special readiness and response windows count accepted moves.
- User-selected Wood A, Stone B, Chain A, Thorn A, Roots B/B and Vines B.
  Native blocker stills are imported unchanged. Dense vines have nine spread
  and nine recoil poses. Thorn's smooth side follows the saved safe direction.
- Continuation saves rage, summon ownership, rhythm duration/targets, bark armor
  state, thorn sides and fixed-cell warning answers. Cleared vine shots remain
  canceled through regrowth and Continue. Shared deadlines and structure
  exclusions protect advertised response cells.

## Executed checks

There are **75 unique passing test cases** across the runs below. The final
37-case run covers the grounding, active-warning protection and imported assets
after their last changes. Other earlier passing cases retain their tested
source/configuration. Counts and original failures remain in the saved XML.

| Run | Result | Notes |
| --- | --- | --- |
| [Broad run](ForestExpandedRoster/f844e3dc-e6df-4e92-999a-9cc4a3061bf8.xml) | 65 passed, 1 failed | Forest, accepted-move state, obstacle gravity, motion and travel. The failed multi-enemy playback test reused a saved fixture during its scene loop. |
| [Playback and portrait run](ForestExpandedRoster/381f8e90-b109-4a25-9290-e148e01ca606.xml) | 12 passed | Replaced that loop with seven individually isolated actor playback tests. Also checks selected blockers, Treant channel ownership and three existing portrait/UI visual tests. |
| [Final affected run](ForestExpandedRoster/ab8c2a01-739c-413f-913a-d5ab97e50c66.xml) | **37 passed, 0 failed, 0 skipped** | Final roster kits, motion, native imports, barricade gravity and zone travel. |
| [Required Unity validator](ForestExpandedRoster/unity-validator.txt) | **Passed, exit 0** | `Tools/Validate-Unity.ps1`, Unity 6000.3.19f1, final source and serialized state. |

Commands use `Tools/Test-ForestFoundation.ps1 -Graphics -Filter ...`; the final
filter is `ForestFoundationPlayTests.Roster;ForestFoundationTests;ForestRosterMotionTests;BarricadeBannerGravityTests;ZoneTravelTests`.
An earlier attempt (`935a2a4b-b68e-4f68-bc93-83ae753d63b9.log`) repeatedly timed
out reconnecting to Unity's licensing service and produced no results. Only
that isolated runner was stopped. The successful rerun used normal licensing
access; the user's open Unity process remained running.

Each actor playback test observes exactly one damage event at its authored basic
contact and recovery to Idle. Other tests cover strict below-half rage, no reset
on healing, full summon slots, one living owned summon, caller death, rhythm
refresh/expiry/multiple casters, pause, thorn provenance and a real safe-side
swap, canceled volley shots, bark shield gating/stagger, Bough reduction without
gem deletion, warning interruption and save/resume.

## Art and visual evidence

Native assets and source measurements are under
`ArtSource/Forest/RosterProduction/`. Character frames use the corresponding
locked palette and binary alpha. Six bodies keep their original scale on 96×80
canvases; Treant keeps its scale on 128×112. `grounding-checks.json` verifies
that each Ready pose preserves every opaque source pixel after whole-pixel
translation to the common foot row. Original approved PNGs remain unchanged.

The actual scene was rendered for all seven enemies and the selected forest
obstacles. Portrait/UI checks cover 720×1280, 1080×1920, 1080×2400 and simulated
safe-area insets. Quiet test fixtures intentionally show large countdown values;
those values are not the production tuning.

- [Treant at the shared floor](ForestExpandedRoster/idle-ancient_treant.png)
- [Treant held Bough warning](ForestExpandedRoster/treant-channel.png)
- [Thorns, tier-two roots and dense vines](ForestExpandedRoster/selected-thorn-roots-vines.png)
- [Berserker](ForestExpandedRoster/idle-orc_berserker.png), [Bloomcaller](ForestExpandedRoster/idle-orc_bloomcaller.png), [Drummer](ForestExpandedRoster/idle-orc_drummer.png)
- [Thornkeeper](ForestExpandedRoster/idle-elven_thornkeeper.png), [Archer](ForestExpandedRoster/idle-briar_archer.png), [Snapvine](ForestExpandedRoster/idle-snapvine.png)

Visual inspection corrected oversized thorn-side markers and the empty lower
padding in Treant's reference. Final captures show bounded edge markers,
separate shield/HP tracks and grounded feet. Art source and technical checks
remain separate from visual judgment.

The final scene diff changes only four board sprite references. The selected
blocker importer updates those YAML bindings directly, avoiding unrelated
ExecuteAlways layout/background changes caused by opening and saving the scene.

Interactive local review pages: `MotionReview.html`, `BlockerReview.html` and
`VineMotionReview.html` in the production source folder. They preserve raw
alternatives, exact native comparisons, playback and individual frame stepping.

## Generation usage

**121 / 160 subscription generations charged; 39 unused.** The account moved
from 298 used / 1702 remaining to 419 used / 1581 remaining. Requested units total
122 because one failed Drummer cast was uncharged. All jobs settled; no credits
were purchased. Two reserve units adapted the newly approved dense vines.
Three accidental duplicate samples are included in the charged total and kept
separately under `UnusedDuplicates`.

## Limits

This is targeted Editor and automated scene evidence, not physical-device or
human balance approval. Encounter values remain provisional. The prior broad
baseline's 34 lifecycle/audio failures are documented separately in the forest
production records; this change does not claim to resolve them. Main menu,
opened settings, existing travel and player ability rules remain unchanged.
