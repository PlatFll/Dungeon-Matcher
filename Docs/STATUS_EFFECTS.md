# Canonical combat statuses

Approved specification: 2026-10-06. Implementation is tracked in
[Handoffs/ASTRA_USAGE_CHECKPOINT.md](Handoffs/ASTRA_USAGE_CHECKPOINT.md).
Values below are conservative, data-driven prototypes, not finalized balance.

## Implementation checkpoint

The backend is implemented by `PlayerStatusRuntime`, owned by `PlayerActor`, with
eight definitions in `Resources/PlayerStatuses`. Ironvein's Seismic Smith applies
Rattled; the original seven retain no production caster assignment.
New unified runs use dedicated 24px PixelLab icons with outlined runtime move
counters in the existing player panel. The original seven 16px assets remain for
legacy views. Enemy effects and Royal Decree share the new visual grammar; the
complete owner/lifetime manifest is [here](UNIFIED_MOVE_COMBAT.md). Tap a glyph for its meaning
and individual Fear sources/durations. `ApplyPlayerStatus` is the optional enemy
ability kind; `EnemyDefinition.appliedPlayerStatus` selects its data. Slippery's
board movement is implemented, with no production caster assigned.
The existing accepted-action coordinator expires statuses after the complete
enemy/environment response. Legacy dungeon runs expire them at the completed
manual board action. No independent timer or resolution coroutine was introduced.
Fear uses persistent enemy identities in both clock modes, never reusable slots.


| Status | Stable meaning | Initial tuning |
| --- | --- | --- |
| Weakened | Reduces outgoing player damage; excludes healing/shields | 0.75 multiplier, 3 accepted moves |
| Burn | Short accepted-move damage with explicit cleanse/extinguish hook | 5 damage per move, 3 moves |
| Sapped | Reduces new energy generation, never stored energy | 0.5 multiplier, 3 moves |
| Wounded | Reduces HP healing received, never shield grants | 0.5 multiplier, 3 moves |
| Fear | Reduces player damage specifically against its living source | 0.75 multiplier, 3 moves; source Stagger/death cleanses |
| Frostbite | Increases all incoming player damage | 1.25 multiplier, 3 moves |
| Slippery | Flooded Court manual swaps move the chosen gem one extra cell when possible | 3 moves; no production caster assigned |
| Rattled | Reduces only newly earned enemy Stagger buildup | 0.5 multiplier, 2 accepted moves; Seismic Smith |

Repeated applications refresh duration without repeatedly multiplying strength.
Fear retains individual source identity; multiple sources do not multiply against
one recipient. Numeric statuses use the strongest active value for their channel;
different canonical channels compose once before the existing final rounding.
All durations count accepted manual moves, including complete special-swap actions,
and pause when no such action completes. New enemy applications do not lose a move
on their birth action. Death/new-run resets clear statuses; continuation restores
remaining moves and necessary source IDs. Snapshot restoration never reapplies a
tick, damage, healing or energy grant.

Burn is distinct from the existing enemy Poison: it ticks on accepted player
moves, has a short fixed lifetime, and exposes an explicit extinguish operation.
Use the centralized player damage path so shields and Frostbite apply normally.
Sapped modifies generation at the generation boundary; spending, save restoration,
initial energy and stored amounts remain storage concerns.

Rattled leaves existing Stagger meter/duration, forced Stagger, damage and energy
unchanged. Hitting stones or firing drills does not cleanse it. The unified view uses its dedicated cracked-bell icon; the legacy definition
retains the previous glyph for compatibility.

Slippery previews a deterministic three-cell rotation before commitment:
`[A][B][C] -> [B][C][A]` in the swipe direction. Legality uses the final arrangement.
An unavailable extra destination/path falls back to an ordinary adjacent swap.
Cascades, gravity, refill, enemy mutation and abilities never slip. It has gameplay
effect only in flooded Court. Do not assign it to an existing enemy without an
explicitly approved source. A future Reef Clan boss is a possible source for review.

Drag while holding to preview all final positions, then release to commit. With
tap selection, the first destination tap previews and a second confirms. A swipe
released without a preceding preview stages that preview for confirmation. An
invalid final arrangement is marked with an X and reverses without consuming a
move. Color-crystal interactions use the chosen gem and the piece at its final
destination; a crystal displaced from the middle only moves. Bombs retain their
identity and normal activation semantics. Hints, available-move and useful-response
queries use the same final arrangement. When flood/status expiry changes the rule,
the board rechecks availability and uses its existing reshuffle if necessary.
Saving waits for that check; previews themselves are transient and are not saved.

## Enemy Warded (implemented by the roster revision)

Warded reduces incoming damage by 25% through EnemyActor's central damage path.
It is active while any living Warden has its structural Root. All living enemies,
including new summons, inherit the effect. Multiple providers contribute one
reduction; deleting one source cannot remove another. This is separate from
ordinary shield HP. The board saves root ownership/durability; continuation
rebuilds source predicates rather than restoring a stale independent duration.
Combat labels and inspection display the named buff. No duration or extra HP bar.

## Enemy Fortified (roster revision)

Queen Nacre's Tribute grants up to two Fortified stacks per recipient. One eligible
direct player damage packet consumes one stack and is reduced by 50%, composing
with Warded and other existing modifiers before final five-point rounding. Shield
gating remains unchanged. Intercepted damage consumes only the actual recipient's
pearl. Periodic/status damage through the explicit non-direct actor path does not
spend a pearl; neither does zero or fully mitigated damage. Multiple pearls never
reduce the same packet repeatedly. The snapshot saves count, with missing legacy
data meaning zero. New enemy initialization and death expose no stale stacks.

Combat status/inspection text names Fortified. One native pink pearl per stack
orbits the actor with quantized offsets and front/back sorting; a consumed pearl
pops. Reduced motion uses static separated pearls. Presentation does not own the
damage reduction, and missing art cannot prevent the buff from working.

## Other future buff vocabulary (not implemented by this specification)

- Regeneration: periodic healing through the ordinary HP path.
- Cleansing: explicit removal of eligible negative statuses.
- Leeching/life-steal: healing from actual HP damage, not requested damage.
- Damage-up and attack-speed-up: retain their existing distinct meanings and clocks.

Exact future durations, stacking, consumption and caster assignments remain gaps.
