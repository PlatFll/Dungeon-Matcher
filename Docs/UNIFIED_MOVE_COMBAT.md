# Unified accepted-move combat — experimental revision

## Authority and starting state

User brief: `a6e1f030-90a1-4d95-b881-6b235484e23f/Pasted text.txt`.
The direct request authorizes merging after implementation and validation.
Base: `b625cbb61d9929a0281edecedda79b0eb3b58ec2` (Ironvein merged).
Branch: `codex/unified-move-combat`. Four pre-existing asset edits are excluded;
their baseline hashes remain in `Docs/IronveinExcavation/READINESS.md`.

## Current-source audit

- BoardController already emits exactly one acceptance/completion pair around
  the entire manual swap. CombatMoveClock already holds input and wave progression
  through environmental and enemy work. Extend this owner, never add a resolver.
- Existing profiles are seconds, seconds with move-coordinated abilities, hybrid
  forest effects, and the `accepted-moves-v1` forest prototype. Their float timing
  values cannot be silently reinterpreted as integer moves.
- Prototype basics subtract fractional haste each move; it orders actors by ID
  and treats special/basic opportunities as mutually exclusive. New requirements
  need integer intervals, all specials before basics, and slot/rank ordering.
- Prototype Stagger lasts one future move and drains 25% after a grace move.
  EnemyStagger consumes feedback-producing actor damage; periodic poison bypasses
  those events. Rattled multiplies only new buildup. Preserve that source boundary.
- Player's eight statuses already use accepted moves and birth-action protection.
  Poison, Marshal rally, Drummer rhythm, supplies and Decree have profile-aware
  paths. Conch rally still runs in seconds. Ore/Benediction consume one sequence,
  Warded depends on living roots, Fortified consumes eligible incoming hits,
  banner haste depends on surviving board standards, and rage lasts until death.
- Channel owners already save target identities/deadlines, cancel through ordinary
  Stagger and have zero extra recovery. Mine final-response warnings must remain
  live through environmental drills before they release.
- Current player status art is seven 16px icons; Rattled needs a dedicated glyph.
  Enemy status presentation is mostly text beside weakness gems. Existing action
  UI combines HIT IN text, a separate ability counter and slot sigil.

## New profile contract

`unified-accepted-moves-v2` applies to new normal/practice runs in all four zones.
Existing records keep their saved profile; explicit old forest test fixtures
retain their chosen prototype profile. No migration converts seconds into moves.
The new basic countdown is saved in a dedicated integer field. Missing/invalid
new-profile state is rejected without altering the durable save.

Basic and special countdowns are independent. All accepted actors advance before
special effects. Specials use Boss, Miniboss, Special, Normal priority, then slot
and persistent identity. Basics use slot order and only the original due set.
Commands consume participants' basic opportunities. New summons wait until the
next acceptance. Every action rechecks living state, holds and reservations.

Haste uses `max(2, ceil(base interval / strongest active speed))`. A stronger buff
can shorten an in-progress countdown by the interval difference, leaving at least
one future move. Expiry never adds back consumed progress. Damage modifiers keep
their existing ownership and composition.

## Status/art manifest (audit, pending integration)

| Family | Owner / actual lifetime | Planned glyph |
|---|---|---|
| Weakened | PlayerStatusRuntime / moves | Notched dull sword |
| Burn | PlayerStatusRuntime / moves, periodic player damage | Orange flame |
| Sapped | PlayerStatusRuntime / moves | Dim violet crystal |
| Wounded | PlayerStatusRuntime / moves | Cracked red heart |
| Fear | PlayerStatusRuntime / moves and source cleanup | Spectral mask |
| Frostbite | PlayerStatusRuntime / moves | Ice shard |
| Slippery | PlayerStatusRuntime / moves; currently no enemy source | Aqua drop with motion |
| Rattled | PlayerStatusRuntime / moves | Cracked metal bell |
| Poison | EnemyPoisonStatus / periodic moves in new profile | Green venom drop |
| Warded | EnemyActor / living Warden root predicate | Leaf/wood shield |
| Fortified | EnemyActor / incoming-hit stacks | Pink pearl |
| Ore-Powered | EnemyOrePower / next eligible whole basic | Orange ore housing |
| Bloodrage / Enrage | ForestCombatAbility / King / until defeat | Red snarling mask |
| War Rhythm | ForestCombatAbility / moves | War drum |
| Marshal rally | TownMarshalEnemyAbility / moves | Hand bell |
| Royal standard | RoyalBannerAuraRuntime / surviving banner | Small gold standard |
| Benediction | RoyalArchbishopEnemyAbility / next whole basic | Blessed blade |
| Rallying Conch | AquaticEnemyAbility / moves; damage only | Coral conch |
| Royal Decree | RoyalDecreeRuntime / five future moves (+ card) | Antique crown |
| Stagger | EnemyStagger / two future moves | Dazed stars |
| Stagger immunity | EnemyStagger / post-expiry moves | Guarded star |
| Marshal retreat | TownMarshalEnemyAbility / protector and moves | Figure behind shield |
| Basic counter | EnemyAutoAttack / integer readiness, not a status | Ivory sword |

HP/shield, ore-machine charges, blockers and channel deadlines are not invented
statuses. Native sources will remain separate from imported final PNGs. Runtime
text supplies counters; condition icons use a neutral marker rather than a fake
duration. Actual native dimensions, alpha and palette counts require file checks.

## Phases and evidence

1. Audit/profile; 2. integer basics; 3. global priority; 4. haste.
5. Stagger; 6. remaining timer owners; 7. Royal Decree.
8. two-row enemy UI; 9. manifest; 10. PixelLab icons; 11. status counters.
12. four-zone provisional balance; 13. save/regression/render/pacing evidence.
14. authoritative docs/art register; 15. review PR, validate and merge.

Implementation is in progress. The first focused Unity run passed 11/11 tests
with audio muted: integer countdown/reset, thinking time, haste/expiry and legacy
save interpretation. Results: `.utmp/ForestValidation/99e921ed-3924-4c20-9550-afc228d3b186.xml`.
Global priority and broader gameplay evidence are still pending. No pacing claim
has been made. Existing HP/damage curves, encounter counts, shields and drill
clear semantics are preserved until an explicit, evidenced balance decision.

## Art allowance

Approved: 60 subscription generations total, 30 initial and 30 corrections.
No credit purchase. Opening balance: 1064 generations, $0 credit. PixelLab help
confirmed native 24px Pixen output at one generation per image. The initial three
requests are Weakened, Burn and Royal Decree; source jobs are recorded separately.
