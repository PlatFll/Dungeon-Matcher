# Technical and Game-scene validation

Unity 6000.3.19f1 ran on Windows. The original no-compilation gate ended after
stage 07. The final production code and definitions are at
f3ed12bd8806b7874749d4f05f3d523f14398266.

| Check | Source | Result |
|---|---|---|
| Tools/Validate-Unity.ps1 | 84dcc0b5 and f3ed12bd | Passed both invocations |
| Tools/Test-Balance.ps1 | 84dcc0b5 and f3ed12bd | 278 passed on each invocation |
| Initial Crystal Resonance and expansion gallery | 84dcc0b5 | 36 passed, six gallery failures |
| Corrected gallery | a9a18c2b | Six passed |
| Expanded Crystal Resonance and gallery suite | a9a18c2b | 43 passed |
| Final gallery and enemy inspection | f3ed12bd | Six passed after changing inspection text |
| Windows development player | f3ed12bd | Succeeded, zero errors and six warnings |
| Android development APK | f3ed12bd | Succeeded, zero errors and seven warnings |
| Natural Game runs and durable journal audit | f3ed12bd | Ten finished cases, 2,442 accepted actions and zero audit failures |

The initial gallery released its scene-load subscription before Unity loaded
Game. Six fixtures therefore observed genuine ordinary spawns. Keeping the
subscription through scene initialization fixed all six. The first failing XML
and every retry are identified in the [technical gate ledger](technical-gates.json).
Inspector QA also caught internal prototype instructions in serialized enemy
descriptions. The final Game checks open the production inspection path for
every new enemy and assert that it displays the player's counterplay text.

## Selection

Each of the five zones was sampled at all five depth tiers with 10,000
independent encounter RNG seeds per context. The reference ordinary distribution
has 250,000 draws. Every eligible ordinary member appeared. Skittercrab, Rivet
Turret and Snapvine are summon-only and have zero ordinary draws at all tiers.
The [per-enemy table](ordinary-exposure.csv) marks them explicitly.

Packbeetle had no authored ordinary recipe at any tier on the base branch.
After repair it appeared in 1,559, 1,496, 1,365, 1,224 and 1,156 of 10,000
Ironvein ordinary draws at tiers zero through four. The previous authored gaps
for Barricade Guard, Crossbow Guard, Spear Guard and Orc Rootbinder also have
ordinary exposure. The late Dungeon Bulwark appeared in 683 and 830 draws, and
the Chirurgeon in 680 and 847. These samples start with prior mechanics taught.

The additional selection tests recorded 500,000 ordinary and capstone draws
and two million first-visit or post-capstone draws with exhausted leaders,
with and without the ten new enemy definitions. On a real Court board a further
300,000 selection calls checked dry, pending and flooded contexts across all
five tiers. Ten depth-45 diagnostic formations accepted a legal swap across
five zones. These are selection and fixture results. Visit duration and player
survival come from the separate Dungeon-first policy runs.

[Phase, role and formation counts](ordinary-distribution.csv) ·
[Ordinary and capstone ranks](ordinary-and-capstones.csv) ·
[Original roster and expanded roster contexts](selection-contexts.csv)

## Reproduction

From the feature worktree run Tools/Validate-Unity.ps1 and Tools/Test-Balance.ps1
with an unused output directory. The focused Unity Test Framework filter is
`CrystalResonance;ExpansionGallery` on EditMode. The gallery enters Play Mode
and loads the production Game scene. The narrower final filter is
`ExpansionGallery`. Use CrystalResonanceBuild.Windows and
CrystalResonanceBuild.Android for the development players. Neither build
publishes the game.

The Windows build recorded 462,608,823 bytes and 66.87 seconds. The six
warnings include obsolete Unity APIs and unused members. The original logs,
XMLs, captures and build reports stay under .utmp/SelectionExpansion. Source
hashes and gate outcomes are retained in the tracked ledger.

The Android player build took 301.79 seconds. Device installation, touch
readability and human judgment remain open. Actual local artifact sizes and
SHA-256 hashes are recorded in the ledger. The natural policy process exited
with code zero after 2 h 19 min. All tracked phases exited, and the final
process census found zero owned Unity editors or test runners.

An initial evidence-wrapper preflight could not find the PowerShell hash
cmdlet and exited before starting Unity. The retry used the system SHA-256
library. After builds, twelve inspected Unity-generated writes to existing
files were restored. These included 342 Drowned Court tile flag serialization
changes. The approved prefab remains at its repository version. Documentation
and empty YAML field whitespace are the only post-validation changes.
