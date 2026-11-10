# Approved enemy revisions — verification record

Feature branch: `codex/approved-enemy-ability-redesigns`, based on main
`dabfc718ca17a9637ae3095a6a63f70d77729013`. Not merged. Design provenance: PR #226
and the user's explicit implementation request of 2026-11-10.

Review PR: [#227](https://github.com/PlatFll/Dungeon-Matcher/pull/227).
Tested implementation commit: `2a9d5a60fe467af54194dad149b3c75c9e7e4a77`.
Subsequent handoff edits only add these PR/commit references to documentation.

## Implemented scope

| Enemy | Implemented change |
| --- | --- |
| Knight Captain | Chain Volley retained; Advance the Ranks advances other living allies' normal/special readiness, floor one, without immediate dispatch or shortening active response windows. King's command is unchanged. |
| Court Mage | Shared True Ice, durable caster ownership, three per owner, no Dungeon ambient seeding, safe release on death and migration of legacy frozen pins. |
| Glacier Troll | Armor retained; two-move column response, player-clear receipts distinguish counters from environmental removals, unanswered outcome adds at most two new Frost encasements. |
| Iceborn Champion | Two-move charge, independent 10/25 damage packets through ordinary shield gating, three-move recovery, Stagger/death cancellation. |
| Hearthkeeper | One board-wide one-hit Ember, fixed wounded recipient, exactly 5 HP per future move for three moves. Player break thaws up to two adjacent layers; death/expiry removes it without a reward. |
| Pearl Cantor | Flooded channel stores 20 pending HP and subtracts 10 per genuinely collected bubble. Dry healing, fixed recipient and interruptions preserved. |
| Reef Netweaver | Native marine-net overlay; Thorny Snare's identity, falling attachment, restriction, AIR cost, cap and duration preserved. |

The board owns placements, physical clearing, ice, structures and settlement.
Enemy runtimes own readiness, channel/recipient state and cleanup. No alternative
board resolver, damage path or encounter formation was introduced.

## Evidence and test method

Final combined gate on 2026-11-10: **38 passed, 0 failed, 0 skipped** with
Unity **6000.3.19f1**. This comprises 22 Game-scene coroutine scenarios that enter
Play Mode and 16 snapshot/rule cases. The compact [per-test results](EnemyRevisions/results.json)
retain every test name and outcome. Raw XML/log:
`.utmp/ForestValidation/ecfaae56-f635-428f-a89e-23c4b299d9b2.{xml,log}`.

Mandatory `powershell -ExecutionPolicy Bypass -File Tools/Validate-Unity.ps1`
also completed successfully on the final runtime/editor source. Its log is
`C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-cff2ce9c-0bbf-43d6-a273-459850b51498.log`.
All eight gallery captures and their local links were verified. No Android
build or physical-device testing was performed.

`Tools/Test-ForestFoundation.ps1 -Graphics` runs Unity Editor tests. The coroutine
fixtures enter Play Mode, load the production `Game` scene, execute accepted
swaps/ability impacts and suspend/reload actual disposable run saves. Pure
snapshot/rule tests run alongside them. These are automated Play Mode scenarios,
not a manual phone playtest or a difficulty/balance study.

The [review gallery](EnemyRevisions.html) contains 1080×1920 Game-view captures.
Fixtures deliberately control readiness, weakness, immunity, RNG and health to
isolate mechanics. Large counters or health in a capture are fixture values.
Hearth success fixtures temporarily prevent incidental cascade damage so healing
can be measured separately; player-break, recipient-death and caster-death
scenarios remain separate strict checks.

Coverage includes:

- Captain readiness floor, no immediate hits, no self advancement, leader
  recipients, preserved alternation and legacy timed-basic compatibility.
- Mage's actual cast, shared restriction count/cap, independent owners,
  escalation, Continue and caster death. Shared ice clearing checks exercise
  adjacency, deliberate Bifrost retaliation and safe indirect-clear policy.
- Troll warning geometry and two-move deadline, Continue, physical destruction
  without a player receipt, actual player ability cancellation and the bounded
  unanswered Frost outcome. The unanswered outcome fixture advances its saved
  deadline directly to test resolution; it is not an unassisted playthrough.
- Champion saved channel/recovery, shield absorbing the first packet while the
  second reaches HP, full recovery, Stagger and death cancellation.
- Ember's three future heals, idempotence on repeated move delivery, Continue,
  one-hit player break, at most two layer thaws, owner/recipient cleanup and no
  retargeting.
- Cantor's 20→10→0 flooded progression, saved pending amount, environmental
  removal exclusion, dry 20-HP heal, Stagger and recipient death.
- Existing snare expiry, placement/AIR-route safety, manual-vs-indirect AIR
  receipts, concurrent channels and King command regressions.
- Existing travel coverage performs seven actual handoffs across all five zones,
  with ice/flood cleanup and preserved run identity, player resources and gems.

Focused iterations revealed fixture assumptions (random recipient death,
unreachable AIR after fixture recoloring, obsolete hardcoded cadence, missing
screenshot directory, and an unanswered Troll fixture with no useful safe pin),
which were corrected without relaxing the outcome
assertions. Required obsolete Champion/Troll/Captain/healer expectations were
replaced with assertions for the explicitly approved redesigns.
The Troll success fixture now retains an independent useful Color Crystal route
outside its threatened column, so its strict 1–2 new-ice assertion tests the
success path rather than a permitted safety fizzle. Production placement checks
were not relaxed.

## Art and presentation

`ArtSource/ApprovedEnemyRevisions` contains the editable LibreSprite sources,
native PNGs, reproducible direct-pixel authoring script and hash manifest.
Both effects are 32×32, binary alpha, Point-filtered, uncompressed and retain
transparent edge clearance. Marine net: six opaque colors. Ember: eight.
LibreSprite export was checked for exact RGBA equality. Zero generation credits.

Actual screenshots exposed texture mixing between same-sized AIR and snare
overlays under the shared default sprite material. The view now binds each
overlay's own texture explicitly, and the new imports have their own sprite
identifiers. The final visual review verifies distinct blue bubbles and barbed
netting. No snare or AIR accounting rule changed.

Rattlebones, Farmer and Pan reference paths/hashes are recorded in the art
manifest. Existing character, gem, zone and approved menu artwork is untouched.
These two new effects remain review candidates, not new golden references.

## Tuning and limits

Champion's 10/25 base impacts and three recovery moves are editable provisional
values; see `../BALANCE_V1.md` for the rationale. Existing normal attacks, HP,
encounter weights, King's command, player shields and unrelated kits are retained.
Human balance feedback is still needed across defensive builds and simultaneous
enemy pressure. No Android build or real-device testing is claimed in this pass.
