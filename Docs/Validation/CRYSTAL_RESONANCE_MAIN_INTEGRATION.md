# Crystal Resonance and enemy expansion integration

Gameplay source `bf73a090` combines the approved merchant shop and rounded Color Bomb from `main`, Crystal Resonance, and the ten-enemy roster. The startup build loads the production Main Menu and Game scenes.

## Balance lineage

The last published refinement was `7e306f72`. The later recovery run on `90bc53b9` retained all 13 attempts, including 12 defeats and one living observation limit. It recorded 25 completed visits and 3,270 audited inputs. Across 485 ordinary-encounter transitions it recorded zero immediately repeated specials, and each of 22 post-capstone follow-ups offered relief. See the [recovery analysis](CRYSTAL_RESONANCE_RECOVERY.md).

The later `313c7722` Court correction keeps Moray and Spearman and substitutes Needlefin for Hammerhead in their patrol. Conch retains its Hammerhead. Its original retest was interrupted. The integrated director uses actual formation membership for recurrence, falls back to recipe labels when reading old saves, retains the Early/Mid/Late pools and reserves the normal-only relief draw for the first eligible encounter after a defeated capstone. A production asset check found every expansion enemy in two to five authored recipes for its home zone.

## Integration repairs

- Save normalization preserves earned milestones across visits and caps each equipped supply by its own inventory. An accepted use still records its replay boundary once.
- On Windows, if atomic backup rotation returns error 1175, checkpoint storage flushes and publishes the previous generation before atomically renaming the new primary. An unrecoverable write still rejects the transaction. A wallet restored from its backup retains the known-good backup.
- Damage telemetry records the attacking enemy and damage reason together with actual HP and shield losses. It remains opt-in.
- The saved-run summary shows recorded Resonance charge. A legacy save shows `SAVED RUN` and migrates through the existing Continue path.

## Verification

Unity 6000.3.19f1 passed the repository compiler validator on the integration source. The first focused run passed 72 of 81. After the storage and attribution corrections, all nine previously failing cases passed together and the complete 81-case suite passed. It exercises the ten production enemies, Guild Taunt, single-target healing, the Slinghunter's single contact, normal and reduced-motion HUDs, card and capstone gates, saves, the merchant and the board Color Bomb. The first balance run passed 279 of 280. The single board-settlement timeout passed in isolation and the full 280-case suite then passed without skipped cases.

The Android and Windows development builds succeeded with zero errors. Android reported seven warnings and Windows reported six. The APK is ARM64. Physical-device testing is still outstanding.

## Recorded Game-scene runs

The natural run uses a fresh Dungeon start. The separate Drowned Court diagnostic uses the explicit Editor entry. Both use legal inputs in the production Game scene at normal game speed. The durable action audit checked against each native journal. The original nine-case Court comparison had been stopped, so these two integration samples do not establish a change in beginner survival or pacing.

| Case | Seed | Outcome | Minutes | Completed visits | Journaled inputs | Card notches picked |
|---|---:|---|---:|---:|---:|---|
| diagnostic-court-skilled | 1293181219 | visit limit | 4.06 | 1 | 87 | 12, 40, 70 |
| natural-dungeon-typical | 762463853 | visit limit | 3.43 | 1 | 64 | 12, 40, 70 |

Dungeon defeated the Knight elite and traveled to Court. Court defeated a Veteran patrol and traveled to Dungeon. The Court sample rolled the heavy Conch patrol. The Moray escort was verified in the production asset and the first-flood eligibility regression. Its effect on low-level survival remains unmeasured on this source.

[Per-case results, input journal hashes and build fingerprint](CrystalResonanceMainIntegration.json).
