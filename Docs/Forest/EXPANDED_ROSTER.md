# Expanded forest roster

The user authorized animation and implementation of these seven locked designs
on 2026-10-03. Earlier still-only restrictions are superseded for this batch.
Treant uses variant C, Old Stump. Faces, equipment, proportions, material palettes
and original pixel scale remain the identity references.

## Final behavior

| Enemy | Rank | Ability and response |
| --- | --- | --- |
| Elven Thornkeeper | Special | A one-hit Bramble Barricade has three thorned sides and one safe side. Only an ordinary gem in the opening deliberate player clear can provoke retaliation. Cascades, ability clears and special blasts are harmless. A clear touching the safe side is a safe answer even when it also touches another side. Placement searches for a currently usable safe side and preserves a useful board response. |
| Orc Berserker | Special | Bloodrage activates once, strictly below half HP after surviving health damage. It increases basic damage and speed. Healing cannot reset or stack rage. No board mechanic. |
| Orc Bloomcaller | Special | Call Snapvine uses a free enemy slot, never replaces an enemy, and waits when full. Each caller tracks at most one living owned summon by persistent actor ID. Dead callers cannot summon; an existing Snapvine survives independently. A dead summon permits a later replacement. |
| Snapvine | Normal / summon | Low HP and fast bite basics; no special. |
| Orc Drummer | Special | War Rhythm buffs only other living enemies. Recasting refreshes duration. Multiple Drummers share the strongest rhythm modifier without multiplying it. Each living caster maintains a lease; removing one leaves other active leases intact. An alone Drummer waits. |
| Briar Archer | Special | Marks up to three already-vined cells and gives two future accepted moves to respond. Each remaining vine contributes one small hit. Removing a marked vine permanently cancels its shot, including after regrowth or Continue. She creates no vines. |
| Ancient Treant | Miniboss | Alternates ordinary Bark Armor and Falling Bough. Breaking its armed shield applies ordinary EnemyStagger. Bough marks a compact group of physical gems for two future moves. Clearing any mark cancels the entire attack without consuming the others or granting Stagger. With no answer, its authored release hits once, consumes the marked gems, then settles the board normally. |

Overlapping warnings use existing deadline spacing and may allow extra response
moves. Volley and Bough pause their caster's basic countdown while the warning is
active. Ordinary stagger/death cancels it and releases the hold. Animation
completion does not advance a move or decide damage.

## First-pass values for testing

These values are tuning inputs, not finalized encounter pacing.

| Enemy | HP | Basic hit | Seconds | Special readiness |
| --- | ---: | ---: | ---: | ---: |
| Thornkeeper | 75 | 10 | 5 | 4 moves |
| Berserker | 100 | 15 | 4.5 | HP threshold |
| Bloomcaller | 70 | 5 | 6 | 4 moves |
| Snapvine | 20 | 5 | 2.5 | None |
| Drummer | 80 | 5 | 6 | 4 moves |
| Archer | 75 | 10 | 5 | 4 moves |
| Treant | 180 | 15 | 6 | 3 moves |

- Thorn retaliation: base 10, through player damage/shield resolution.
- Rage: 1.5× basic damage and 1.4× speed; damage uses shared five-point rounding.
- Rhythm: 1.4× speed for five seconds in live travel, or three future moves in
  isolated move-effect profiles. Settings pause freezes seconds.
- Volley: base 5 per surviving shot. Separate hits respect ordinary shield gating.
- Bark Armor: 30 ordinary shield, respecting existing capacity and the break gate.
- Bough: base 30 when unanswered; zero and no remaining-gem consumption when answered.
- Existing encounter damage scaling applies; no player-power scaling or forced wait.

## Ownership and continuation

EnemyDefinition selects ForestCombatAbility or ForestPressureAbility through the
existing runtime factory. Thornkeeper reuses BarricadeEnemyAbility. Snapvine uses
the ordinary enemy actor and basic attack runtime.

BoardController owns thorn sides, placement, structural damage, fixed Volley
response cells and physical Bough marks. Bough reuses saved gem-set targets with
compact selection and whole-set cancellation on any target loss. The opening player clear passes its provenance into the
existing barricade path. No secondary clear/reward pipeline exists. Response
marks are consumed before impact callbacks and cannot resolve twice.
Active fixed-cell warning targets are excluded from new structure placement and
root warnings, so a later enemy cannot bury an advertised response cell.

Continue saves rage, caller/summon identity, rhythm targets and remaining
duration, Treant cycle/armed shield, warning deadlines, physical gem order and answered Volley cells, and
thorn safe side/damage. Rebuilding UI never restarts these rules. Board photographs
do not rewind enemy state or warning answers. Old fixed-cell Bough warnings fizzle
on load without damage, clearing gems or granting Stagger; new marks retain identity.

The definitions join the existing zone roster. Nine development fixtures append
to the prior thirteen. Eight live formations extend existing temporary bands
without changing the apex, travel rules or eighteen-band anchor. Substantial
visit length still needs human pacing tests.

## Blocker art approval

The user selected Wood A, Stone B, Chain A, Thorn A, Roots B/B and Vines B
on 2026-10-03. The seven native 64×64 PNGs are imported unchanged. Thorn's smooth
top edge rotates toward its authoritative safe side; colored edge markers reinforce
that orientation. Dense vines retain 17.7% transparent area. The two root tiers
have different silhouettes. Spread and recoil use the selected vine design.
The review page, rejected alternatives and measured PNG ledger remain in
ArtSource/Forest/RosterProduction.
