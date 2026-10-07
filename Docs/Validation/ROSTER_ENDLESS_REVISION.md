# Roster / endless revision evidence — 2026-10-07

Branch `codex/roster-endless-revision`, based on merged main
`e572a88227d036af765e057831c46a2cc067630d`. This report covers completed
slices of [the approved worklist](../ROSTER_ENDLESS_REVISION.md), not the
whole requested revision. No merge is authorized.

## Channel lifecycle

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
