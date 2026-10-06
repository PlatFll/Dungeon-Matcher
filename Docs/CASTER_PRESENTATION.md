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

## Slot sigils — implementation pending

Left triangle, middle square, right ring. Slot identity is independent of species,
rank, persistent actor ID and formation order. The authoritative WaveController
slot mapping supplies identity; replacement occupants inherit their new slot.
Target marks must follow gem identity or remain on fixed cells as applicable.
Rows place their mark outside the right edge; columns above the top edge.
Simultaneous marks stack in triangle/square/ring order. No effect-type pictograms.
