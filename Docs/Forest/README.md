# Forest foundation — Phase 4

Internal development prototype, awaiting technical and playable review.
Base: `d334b5278748dcd303ac931eb71b0aa7328dd15b` (current main at start).
Work branch: `codex/forest-gameplay-foundation`. The original dirty checkout is untouched.

## Play

Open this branch's Unity project in **Unity 6000.3.19f1**. Choose
**Dungeon Matcher → Forest → Play new isolated test**. This creates a separate
save under `.utmp/ForestPlaytest`, with three test Potions and Bombs. Production
gold, supplies, run and character progression are untouched. Stop Play Mode,
then use **Resume isolated test** to reopen that save. Suspend to Menu also works
inside the test session. A new test gets a new file; older saves are retained.

The first formation is Trailguard + Mender + Scout. The next formation adds
Rootbinder, then a combined support/disruption formation. This small fixture
loops through the existing endless wave pipeline. It is not the final forest
roster, progression, zone travel or a claim of ten-minute content coverage.
Forest is explicitly ineligible for live selection. No public mode selector
or aquatic implementation was added.

## Inspect

- [Approved art and technical review](ART_REVIEW.md)
- [Resolution, timing and save contract](FOUNDATION_CONTRACT.md)
- [Timer migration audit](CLOCK_IMPLEMENTATION.md)
- [Executed validation and limits](../Validation/FOREST_FOUNDATION_PHASE_04.md)
- `ArtSource/Forest/Approved/manifest.json`: six approved sprite identities.
- `ArtSource/Forest/UI/Theme_Source_Record.json`: exact crops/repetition of
  accepted wood and health-frame art. No generated replacement designs.
- `Assets/_Game/Resources/Zones/magical-forest.asset`: explicit test eligibility,
  Emerald affiliation, roster and gameplay theme.

The four starter enemies have playable test kits. Warden and Matriarch are
imported approved still references; their milestone kits remain later work.
Vines currently use the existing chain overlay and a green warning marker as
**development presentation**, with canonical chain movement and removal.
The forest environment repeats exact native-size crops of the approved small
woodland study. Visible repeats and provisional vine markers remain development
presentation pending later production. Runtime text, gems, ranks and menu/settings skins
keep their existing ownership.

`Starter_Kits.txt` and `Milestones_and_Pilot.txt` preserve the Phase 3 review
specifications. Their historical implementation/approval notes describe that
earlier phase. Current status: all six still designs are approved; the four
starter kits are implemented here; milestone kits and full motion remain later
work. This approval does not automatically approve the earlier motion samples.

## Review gate

Review the move cadence, two-response heal, interruptions, vine counterplay,
readability and save/resume. These numbers are tuning proposals. Human fun,
Android hardware behavior and complete forest pacing are not established by
automated tests. Full animation production and subsequent phases require the
next approval. No new PixelLab generations or credits were used in Phase 4.
