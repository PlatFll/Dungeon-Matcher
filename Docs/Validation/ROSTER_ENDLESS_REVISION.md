# Roster / endless revision evidence — 2026-10-07

Branch `codex/roster-endless-revision`, based on merged main
`e572a88227d036af765e057831c46a2cc067630d`. This report covers completed
slices of [the approved worklist](../ROSTER_ENDLESS_REVISION.md), not the
whole requested revision. No merge is authorized.

## Tribute and Fortified slice

**39 unique affected cases pass on their latest run**, including central damage,
shield/interception, all 0/1/2/3 Tribute survivors, actual bubble collection,
ordinary Stagger, legacy warning migration, live/saved targeting and buff counts,
Queen-first actual slot order (including a new left-slot replacement), cap two,
Muster independence, sigils, front/back orbit, pause, reduced motion and death.

Runs under `.utmp/ForestValidation`:
- `4a360a3b-d81d-4d25-9084-918613d77079.xml`: 6/11 passed. This caught a real
  sibling-index orbit bug (both pearls could move in front on later frames), plus
  an isolated actor fixture invoking missing portrait setup and a distribution
  fixture losing a bubble to an incidental cascade. Corrected ordering, used the
  established isolated actor fixture, and protected intended survivors as specials.
- `cf382258-5d0e-417c-8afe-d9bd5b3b4e60.xml`: all five affected reruns passed.
- `9341a60b-4ba8-4103-9d41-a21a5773698c.xml`: 37/38 passed; a second survivor
  fixture's incidental collection was exposed. Its intended target identities are
  now preserved through actual special conversion during the test's manual moves.
- `6e7567dd-bdc9-4ad3-a9e9-55fb97ddc4dc.xml`: all five survivor/slot-order checks
  passed. Counts above deduplicate repeated cases; no failed result is hidden.

Mandatory Unity validator passed, exit 0:
`C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-72854e61-2c8c-48ee-be94-41cf0abf11c7.log`.

No character art was regenerated. Tribute reuses the approved Queen release clip
(its historical controller key remains `DepthsRelease`); the old damage ability
is absent. Native orbit sprites were authored in tranche 7. Actual portrait
captures and broader final integration remain outstanding.

## Royal moving pearl evidence

Four focused tests passed (`93a1efb0-07fa-4376-b2a0-e86ec5b8e527.xml`), including
two theft/durability regressions. The Queen scenario captures all five resources,
breaks armor through the real player-area-clear pipeline, checks each shifted
physical special at the committed rotation, verifies no new gem/refill midway and
held input, then Continue restores the exposed art/coordinate and the second hit
pays once. A structural-obstruction fixture proves the no-route fallback preserves
every gem. All playback is muted in the Editor. Native scene review remains in the
final presentation pass; these automated checks do not establish animation feel.

Mandatory validator passed (exit 0, Unity 6000.3.19f1):
`C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-07434cf1-592d-4528-b47a-ebe1f1360ba2.log`.

## Court coffer and Pressure evidence

Eleven focused Unity checks passed (`9ab7e42b-0dbf-4142-8185-b4a476e142fb.xml`
under `.utmp/ForestValidation`), with no failures or skipped cases. They exercise
the actual muted scene, accepted-action contact timing, exact two/three captures,
no theft warning, special preservation, ordinary footprint removal, independent
caster-death/Continue durability, +4/full AIR and duplicate payout prevention,
shell-break events, insufficient-resource deferral, two physical Pressure targets,
Continue, either-answer cancellation, full unanswered damage and legacy migration.
Four AIR tests overlap tranche 6; do not add counts across slices blindly.

`Tools/Validate-Unity.ps1` completed successfully on Unity 6000.3.19f1 (exit 0):
`C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-c5ee3e53-b906-4d03-8656-5053960c0e90.log`.
The native source comparison is `ArtSource/DrownedCourt/RosterRevision/PearlReview.png`.
Seven candidates have binary alpha, 2–9 opaque colors, actual native dimensions
6–32 pixels, 64 PPU, Point/uncompressed/unmipped FullRect imports. No paid jobs.
Actual four-viewport pearl/Pressure presentation checks remain in the final pass.

## Channel lifecycle evidence

Unity 6000.3.19f1, actual Game scene, disposable account saves, Editor audio muted.

- Focused success/fizzle/Stagger/legacy recovery continuation: **10 passed**,
  zero failed/skipped. `.utmp/ForestValidation/bbf334b6-afad-423d-a361-67b6e8e964c9.xml`.
- Broader affected combat, sigils, announcements, continuation and eighteen-zone-handoff
  regression: **15 passed, 3 failed** initially.
  `.utmp/ForestValidation/1064d3cc-7859-4fa0-b2b3-e71d6946a1a9.xml`.
  Queen and Captain tests still expected the retired two-move wait. They now
  assert the unchanged three-move cadence and full response window, then immediate
  release. The photograph fixture supplied only an edge match, whereas root sites
  require four neighbors; it now supplies an explicit interior match before asking
  for two warnings. Gameplay placement safety was not changed.
- All three affected reruns **passed**, zero failed/skipped:
  `.utmp/ForestValidation/c333c219-e8a9-4bd9-9081-dca84a464b03.xml`.
  Across the focused and broader sets, **28 unique tests passed**.
- Required `Tools/Validate-Unity.ps1` **passed**, exit 0, Unity 6000.3.19f1.
  Log: `C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-a99b67aa-795c-425c-a8c9-f63a5f5204a4.log`.
- The four original user modifications still match their starting SHA-256 hashes.

No visual assets changed in this slice. Final four-viewport review and whole-revision
regression remain pending. Earlier PR #181 evidence is a baseline, not a claim that
this new revision has passed those same checks.

## Audit discrepancy

`PROGRESSION_PACING.md`'s historical “Initial connected forest visits (2026-10-03)”
paragraph still describes two destinations and unavailable aquatic travel. Actual
main has three eligible connected zones and the Court contract records that newer
behavior. Do not revert implementation to that older paragraph; reconcile it when
updating the final progression documentation.

## Shared endless HP

The serialized standard profile and new-profile HP defaults now use the approved
1/15/30/50/70/100 anchors at 1/1.4/2/3/4/6×. Provisional post-100 growth adds
1% of wave-100 HP per wave, reaching 9× at 150. Enemy definitions, damage curves,
attack speed and special cadence were not retuned. Player-power correction is off.

- Eight data checks passed across all 48 enemy definitions at the seven requested
  depths, with monotonic/finite continuation through wave 1,000,000, five-point
  rounding and a 1,000× player-power input having no effect. Initial run:
  `.utmp/ForestValidation/57f7fb78-4ea0-47db-9dc1-5bf585179ad5.xml`.
- That first run's three live fixtures failed due to coroutine-wrapper / Editor
  resize setup. Standard nested UnityTest wrappers, explicit existing-owner layout
  refresh and waiting for spawn effects corrected the fixtures. No validation
  errors were suppressed. Three live checks then passed:
  `.utmp/ForestValidation/c6bba1d9-4599-4c6a-98bf-17f1e4069257.xml`.
  They cover actual spawned stats, damaged HP preservation through Continue at
  every requested depth, and global 150→151 / local 1 on each zone's travel leg.
- All **88** affected balance, damage, wave lifecycle and King checks passed:
  `.utmp/ForestValidation/8a0596fa-829b-41c6-9a32-135d1a63c3fe.xml`.
- Capture review exposed a pre-existing wave-plaque bug: Continue restored the
  encounter correctly but its label retained WAVE 1 because no new-wave event fires
  during restore. The UI now refreshes when authoritative depth differs, without
  emitting a spawn/reward event. Added label and overflow assertions; all three
  reruns passed in `.utmp/ForestValidation/654d817e-4218-4b44-81c4-050b3699e629.xml`.
- [Measured representative scaling table](ENDLESS_SCALING_TABLE.md). Full 48-enemy
  tables, actual runtime TSVs and twelve portrait captures are in `.utmp/RosterEndless/`.
  The four shapes are 720×1280, 1080×1920, 1080×2400 and a safe-inset 1080×2400.
- Fresh captures show WAVE 150 and the four-digit HP values inside their existing
  containers. All twelve layout assertions passed. These are Editor graphics
  captures, not physical-device screenshots.
- Final required Unity validator passed with 6000.3.19f1, exit 0:
  `C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-a7c9acf4-0dbf-45f0-a466-f1c660a2e437.log`.
- Deduplicating channel and scaling regression names gives **126 unique passing
  cases** with no unresolved failures. Earlier fixture failures remain recorded above.

This verifies stats and continuation, not human time-to-kill or mobile-device
performance. Full revised kits, animations and their eventual visual/regression
gates remain pending.

## Dungeon Minister and Judgment

The Minister retains `royal_arcanist` and the original asset/GUID. Judgment now
consumes surviving physical gems in saved order under one board transaction,
with authored contacts for Strike1, Strike2 and the three-survivor-only finisher.
Normal base damage remains 12; empowered base 18 is provisional serialized data.

- Initial ten-case run: five passed and five failed due to fixture reflection
  selecting an overloaded method and a UnityTest coroutine setup boundary.
  `.utmp/ForestValidation/b0b84687-abd8-4f32-9068-23e3117d66f6.xml`.
  Tests now use the public legal-response query and standard nested coroutine
  wrapper. Gameplay assertions were retained.
- All **13** focused and affected King checks passed in
  `.utmp/ForestValidation/264de4b9-1f27-40f4-8519-498c440bd50c.xml`.
  Cases cover 0/1/2/3 survivors, special conversion, no intermediate grid changes,
  authored impact timing, duplicate prevention, pause, death cancellation,
  continuation, missing-motion fallback, full King cycles in both clock profiles,
  Restoration and existing Bombardment/Assault behavior.
- Physical-gem movement, persisted survivor order, Minister inspection and stable
  ID through Continue passed in
  `.utmp/ForestValidation/fb45572c-6148-41a1-93f6-74b22331ba12.xml`.
  Its portrait case found a real wave-plaque overlap with the first 112×96
  finisher canvas. The final native canvas is **112×80**; lateral space preserves
  pixel scale without raising the sprite rectangle into the plaque.
- Final finisher contact, asset and portrait reruns: **4 passed** in
  `.utmp/ForestValidation/4fa927cc-a22b-4e02-ad8a-eed385aa09ee.xml`.
  Visual inspection caught that Editor resizing could replace a paused frame
  with the ready sprite. The capture fixture now asserts the actual rendered
  finisher contact, freezes it, and reapplies that sprite after each layout rebuild.
  That final four-shape capture case passed in
  `.utmp/ForestValidation/a1476992-c317-44b1-a0dd-041c0f900b3b.xml`.
- **15 unique passing cases** in this Dungeon slice; counts overlap earlier
  evidence. Actual 720×1280, 1080×1920, 1080×2400 and safe-inset renders are in
  `.utmp/RosterEndless/Judgment/`. Finisher stays clear of the wave plaque and HP.
- Three native motion sheets, JSON timings, GIF previews and reproducible direct
  pixel authoring source are in `ArtSource/EnemyAttacks/`. All use 17 opaque colors,
  binary alpha, Point filtering, uncompressed full rectangles and 64 PPU. Existing
  Bombardment/idle files were preserved. **Zero PixelLab generations**.
- All four original user-edited files still match their starting hashes.
- Mandatory `Tools/Validate-Unity.ps1` passed, Unity 6000.3.19f1, exit 0:
  `C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-5c855417-2e1c-483a-9428-8f122364509a.log`.

New animation art remains a review candidate. This is muted Editor/runtime
evidence, not physical-device or human-feel approval. Forest/Court revision
tranches and final combined regression remain open.

## Forest revision — validated implementation, art awaiting review

- Initial `.utmp/ForestValidation/24087d55-a4b5-4517-b15b-2535d9415032.xml`:
  8 passed / 9 failed. Fixtures attempted casts outside the accepted-move owner;
  two new scene coroutines needed explicit wrappers. One paired-root fixture
  struck both roots because the selected side bordered both; it now isolates the
  intended target. A real placement gap allowed structures to bury physical marks;
  the shared placement candidate check now protects both physical and cell marks.
- Second `.utmp/ForestValidation/28b1146f-8378-41a9-8be1-3d6b9431e774.xml`:
  17 passed / 2 failed. Root contact and pre-contact interruption exposed a missing
  `timeSpecialAbilityFromAnimation` opt-in. The importer now sets both required
  flags; explicit asset/event checks and four viewport captures were added.
- Third `.utmp/ForestValidation/901dbc1c-7677-4205-be0e-af1af0aa9c20.xml`:
  10 passed / 2 failed. Warden's instant pending action needed to release its
  animation token on Stagger; its old handler covered only multi-move channels.
  Warded's 14-unit label was too short for native Thaleah at the small viewport;
  the final container is now 42×20 beside the weakness gem. Review also added a Bough guard so answering a
  long warning cannot recast on that same accepted move when readiness is full.

- `0818681c-24b2-4e05-a717-b5220a819555.xml`: 15/16 passed. The answered Bough
  reset ran before move readiness advanced; it now finishes within the accepted
  coordinator. `1eec1a72-9e23-4bbf-adae-3c702db98bca.xml`: all five reruns passed.
- `6c482073-fd33-4d14-9bf8-16905dc9c0b7.xml`: 10/12 passed. Render review caught
  Warded clipping below the battle mask, then extending outside the right slot.
  Final labels use the weakness lane; text, culling, glyph count and position are
  asserted. `40a09de2-adeb-487c-8b95-6c9734dbcbf2.xml`: both reruns passed.
- `e7ff7a69-b318-4d1c-969c-e87d498a40ed.xml`: 7/9 passed. The screenshot fixture
  froze a partial hit flash; it now waits for the actual renderer to reach zero.
  Matriarch's scene coroutine needed the same explicit wrapper as other new cases.
  `f65fb7e0-27f7-4eff-9193-cc9c10078a2b.xml`: both final reruns passed.
  One intermediate run failed compilation on an unqualified test `Color` name;
  it was corrected before these executed tests.
- **23 unique current affected cases passed**, including the four separate
  Rootbinder/Warden contact and legacy-warning cases that replace two earlier
  combined failing fixtures. Counts above overlap. No current failed case remains.
- All four actual viewport images are in `.utmp/RosterEndless/Forest/`.
  Warded is visible, root hit flashes settle, and native root art is crisp.
  The synthetic right-slot Matriarch still demonstrates the older tall-character
  counter/settings crowding at short portrait; final combined UI review must fix it.
- Four 64×64 root candidates preserve the approved palettes (17/26 opaque colors),
  binary alpha and Point/uncompressed imports. Zero PixelLab generations.
- Required Unity validator passed, exit 0:
  `C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-5cdfe417-8888-4cb4-9d28-d8e3a6b8f422.log`.
  Editor audio stayed muted. Original four user files match their starting hashes.

## Court finite AIR reserve — tranche 6

- All **14 focused checks passed** in
  `.utmp/ForestValidation/f748729e-432d-4434-9f21-3b60ec7774b5.xml`: first dry
  formation, actual 16–18-move flood and five starting bubbles, receipt arithmetic,
  separate rendered −1/+2 beats, no supply while free/captured reserve exists,
  three-move emergency timing, normal/critical caps, resource-preserving low-AIR
  relocation, current/legacy continuation, duplicate accounting and final drain.
- Mandatory Unity validator passed, exit 0:
  `C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-c1ea84f5-8fdc-411c-b3aa-b55e41d25dc4.log`.
- No paid generations or asset replacements. The new economy is provisional
  zone data. Theft/coffer/Pressure/Tribute revisions remain subsequent work.
