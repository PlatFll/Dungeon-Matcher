# Canonical player statuses

Approved specification: 2026-10-06. Implementation is tracked in
[Handoffs/ASTRA_USAGE_CHECKPOINT.md](Handoffs/ASTRA_USAGE_CHECKPOINT.md).
Values below are conservative, data-driven prototypes, not finalized balance.

## Implementation checkpoint

The backend is implemented by `PlayerStatusRuntime`, owned by `PlayerActor`, with
seven definitions in `Resources/PlayerStatuses`. No production enemy applies
these definitions yet. Status icons/UI, the generic casting adapter and Slippery's
board movement are subsequent tranches; a Slippery duration alone does not move gems.
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

Slippery previews a deterministic three-cell rotation before commitment:
`[A][B][C] -> [B][C][A]` in the swipe direction. Legality uses the final arrangement.
An unavailable extra destination/path falls back to an ordinary adjacent swap.
Cascades, gravity, refill, enemy mutation and abilities never slip. It has gameplay
effect only in flooded Court. Do not assign it to an existing enemy without an
explicitly approved source. A future Reef Clan boss is a possible source for review.

## Future buff vocabulary (not implemented by this specification)

- Regeneration: periodic healing through the ordinary HP path.
- Fortified: temporary next-hit protection with a clear consumption condition.
- Cleansing: explicit removal of eligible negative statuses.
- Leeching/life-steal: healing from actual HP damage, not requested damage.
- Damage-up and attack-speed-up: retain their existing distinct meanings and clocks.

Exact future durations, stacking, consumption and caster assignments remain gaps.
