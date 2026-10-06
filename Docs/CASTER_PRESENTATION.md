# Enemy cast presentation

## Cast names — implemented

Gameplay owners publish `EnemyActor.AbilityCastCommitted` through
`AnnounceCommittedCast` only after a cast succeeds: an instant effect applies,
an active response warning/channel is established, or the first commanded strike
is accepted. Readiness, animation start, failed placement, capped cadence resets,
cancelled pre-casts, restoration and individual channel ticks stay silent.

`EnemyAbilityNames` supplies the shared display vocabulary for announcements and
tap-to-inspect. Existing inspection descriptions remain intact. Multi-part enemies
announce their current action (for example Shellguard versus Boarding Order).
Existing authored animation/contact ownership remains in the ability runtime.

`EnemyCastAnnouncement` draws off-white Thaleah text above the caster. It rises
10 UI units over 1.25 scaled seconds and fades after 0.4 seconds. Pause freezes
it. It has no damage, counter or board authority. Death/disable/scene teardown
removes it, and continuation never replays the transient label.

## Slot sigils — implemented

Left triangle, middle square, right ring. Slot identity is independent of species,
rank, persistent actor ID and formation order. The authoritative WaveController
slot mapping supplies identity; replacement occupants inherit their new slot.
Target marks must follow gem identity or remain on fixed cells as applicable.
Rows place their mark outside the right edge; columns above the top edge.
Simultaneous marks stack in triangle/square/ring order. No effect-type pictograms.

`BoardController.CollectCasterTargets` projects the existing gem-pair, gem-set,
fixed-cell and lane threats, plus committed pin reservations. Sequential gem-set
resolution keeps the source mark on surviving targets after consuming its warning.
`BoardCasterSigilView` adds the Court runtime's current cells, bubbled gems and
coffer-site targets. It never creates deadlines, selects targets, mutates the board
or stores a second warning state. Environmental vine warnings have no caster.

The three 12×12 two-color glyphs are direct pixel assets with a dark one-pixel
outline. Board images render above the board frame so outside lane marks stay
visible; menus and travel smoke remain above them. Images do not intercept input.
Actor badges sit beside the HP track, separate from weakness and shield indicators.
Neutral blinking corners replace effect pictograms. Existing countdowns, lane
flashes, impact art and actual bubble/snare/coffer objects retain their roles.

Views use live authoritative targets and the actual slot occupant. Death, resolved
targets, cancelled warnings, interrupted channels and scene teardown remove marks.
Continue restores existing target identities and deadlines, then reconstructs the
view. Summons inherit their occupied slot. No current warning explicitly persists
after caster death. Resolved persistent barricades/banners are board objects, not
active casting warnings. Existing dungeon warnings that survive a delaying Stagger
remain visible; forest/Court channels cancelled by Stagger lose their marks.

Focused checks cover three overlapping sources, actual gravity, fixed cells, rows,
columns, source death, replacement, sequential clear, interruption, Continue and
four portrait/safe-area layouts. Final evidence is in the revision validation record.
