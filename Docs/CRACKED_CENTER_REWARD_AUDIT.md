# Cracked center rewards — gameplay audit chunk 13

## Status / dependency

IMPLEMENTED — SOURCE/DIFF REVIEWED — UNITY VALIDATION DEFERRED — DO NOT MERGE.

Main inspected: `4e2e959186492636d44c91c33514d65aa1f724c1`.
Branch: `fix/cracked-center-energy-attribution`.
This branch is stacked on PR #142, `fix/run-upgrade-lifecycle-audit`, at
`93d0582ab4fadde089da606d9690ba0cc03063b9`. It edits the hooks with #142's
subscription/reset corrections already present. Include #142 before this group
in the final integration state. #143 is a sibling dependent on #142; this group
does not require #143 and does not edit its coordinator/UI files.

Compilation, NUnit discovery/execution, Play Mode and device testing have NOT
RUN. The user requested one combined validation stage after the source audit.

## Confirmed defect: Resonant Cracks counts combat dispatch, not detonations

The current `RunUpgrade_ResonantCracks.asset` describes a five-energy refund on
every third Cracked Gem detonation. In `RunUpgradeGameplayHooks`, its counter and
refund were inside `HandleBeforeGemDamage`. `CombatController.ResolveFixedGemDamage`
checks `CanResolveCombat` before publishing that event; an inactive encounter
therefore prevents the counter from seeing the center at all.

An already-accepted Cracked resolution may finish its bubble/shake after poison
or earlier effects finish the encounter. BoardController still owns the action
and reports rewardable centers through `BoardClearOutcomeResolved`; ordinary
special energy can still be granted, but the card's additional refund was lost.
The same defect can lose a third center after two previously counted centers.

The old implementation did NOT require a matching enemy to take HP damage when
the wave was active. Do not misdescribe this as a general no-matching-color bug.
The demonstrated source condition is the rejected active-encounter damage path.
Actual frequency and timed reproduction remain untested.

## Correction

- Add an opt-in immutable `BoardClearContext.IsFixedDamageExplosionCenter`
  classification. Existing constructor calls default to false. It changes
  neither clear source nor `GrantsSpecialEnergy` and grants no entitlement.
- The existing Cracked set reporter marks only its unique, sorted fixed-damage
  centers. Ordinary collateral and consumed Color Crystals are not centers.
- Count Resonant Cracks in the existing board-outcome subscriber, once per
  entitled one-gem center for the living initialized Cracked-Gems player owning
  the card. No combat hit or active encounter is required for that refund.
- Damage modifiers stay in `BeforeGemDamage`, but identify centers using that
  same explicit classification instead of comparing a requested damage number
  against a definition constant. Numeric coincidence must not turn collateral
  into a center. Different-number/collision tests are defensive coverage, not a
  claim that shipped balance currently causes frequent damage collisions.

No second reward dispatcher, board resolver, extra damage call or VFX-owned
reward path was added. Standard per-gem energy still has its existing producer;
this hook awards only the card's additional fixed refund. The existing shared
counter's wave/reset lifetime, frequency of three and refund of five remain
unchanged. This correction does not decide whether partial progress SHOULD
persist across waves, or whether fixed card bonuses should themselves receive
global energy multipliers. Keep those separate from this activation defect.

## Scope

Three incremental production files:
- `Assets/_Game/Scripts/Board/BoardClearContext.cs`
- `Assets/_Game/Scripts/Board/BoardController.CrackedGems.cs`
- `Assets/_Game/Scripts/RunUpgrades/RunUpgradeGameplayHooks.cs`

No changes to target selection, explosion areas, crystal conversion, utility
bomb shatter commitment, damage values, modifier arithmetic, energy storage/cap,
activation costs, card data, active-ability entitlement rules, reset policy,
shields, stagger, waves, mastery, scenes, prefabs, art or imports.

## Authored regression coverage — NOT RUN

`CrackedCenterRewardTests`: 22 NUnit cases. Synchronous EditMode fixtures call
actual board set-reporting and combat dispatch with disposable actors and gems.
Defensive inputs use controlled delivery of the existing outcome event. Card
and player Resources are read, not mutated; one temporary modifier asset tests
numeric classification. RNG is restored. No PlayerPrefs or authored scene edits.

Coverage: active/inactive encounter refunds; clear between the second/third
center; center/collateral/crystal tagging; wrong-source, unentitled and invalid
counts; default constructor compatibility; no owned card; damage calls alone
not counting; unchanged center modifier math and collateral-number collision;
energy caps; player defeat; repeated enable/reset; separate Conductor bonus.

These tests do not simulate actual shatter, enemy HP loss, poison timing, wave
advancement, board settling or UI energy updates. They intentionally omit the
ordinary energy producer when asserting the isolated card refund; real combined
energy totals MUST also be checked in Play Mode.

## Final combined validation backlog — issue group 12

1. Integrate #142 first and this group's delta, alongside #135–#145. Never
   overwrite #142's hook corrections with the old main version. #143 still has
   its independent #142 dependency.
2. Discover/run all 22 new cases, #142's 20 cases, existing upgrade/gameplay tests
   and relevant Bardley/crystal/energy tests. Fixtures that explicitly construct
   a fixed center must describe it with the new marker; ordinary collateral
   must remain unmarked. Do not weaken assertions to obtain PASS.
3. Equip the actual Resonant Cracks card. Detonate three and six real centers;
   inspect the extra five/ten energy separately from normal per-gem rewards.
4. Finish the last enemy and death cleanup during an already-accepted bubble/
   shake (including poison), then let the resolution finish. Eligible center
   refunds must still occur; no revival or next-wave enemy damage is permitted.
5. Test two counted centers followed by an encounter-ending third center, and
   test crystal-created centers versus bombs/crystals caught as collateral.
   Ordinary collateral and the crystal's hidden color must not advance resonance.
6. Combine actual Cracked damage upgrades, Conductor and ordinary energy cards.
   Preserve existing numeric rules, one ordinary reward per cleared gem, energy
   cap, source attribution and active-ability entitlement. A refund at cap is
   consumed, not queued to replay after spending.
7. Test defeat, recovery, rejected activation, all-special fallback, five hook
   enable cycles and new run reset with prior audit groups. Only actual reported
   centers count; damage attempts and planning must not award this card.
8. Run required `Tools/Validate-Unity.ps1` in the final authorized combined pass.
   No immediate per-PR validation or merge is requested now.

## Read-only persistence boundary review

Also read `CharacterSelectionSettings`, `PlayerDefinitionRegistry`,
`GemMasterySettings` and `GemMasteryRuntimeResolver`. These use stable stored
IDs/enum choices and explicit fallbacks; this pass established no new saving
failure in those inspected methods. No preferences were changed. This is not a
claim that full menu UI, cold-launch storage or device persistence was tested.

The full repository audit remains incomplete.
