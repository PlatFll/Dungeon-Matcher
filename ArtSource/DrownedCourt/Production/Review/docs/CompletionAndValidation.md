# Drowned Court implementation and validation

Branch: `codex/drowned-court-implementation`. Base main:
`9f8b5f7dc78761906743eec551a38b25c89dcaea`.
Unity: 6000.3.19f1, Windows graphics-enabled batch editor. Editor audio is muted.
Implementation and automated validation completed on 2026-10-05. The final
affected evidence contains **213 unique passing cases**, with no remaining failed
or skipped case after focused corrections. This is the reconciled result across
the broad run and its focused reruns, not a claim that the first run passed.
`Tools/Validate-Unity.ps1` also completed successfully on the final C# source.
Physical Android, full-visit human pacing, later art and music approval remain open.

## Executed evidence

| Evidence | Result | Record |
| --- | --- | --- |
| AIR arithmetic, Bardley reward regression and early Court runtime | 34/34 passed | `.utmp/ForestValidation/1fe5addf-c458-4244-b8fe-b94dae7a949f.xml` |
| Coffer owner cleanup, wet continuation, final drain | 3/3 passed | `12823c71-9296-42e1-bd74-78235a1bc1ac.xml` |
| Full roster basic sequences, Puffer, Queen, Court state tests | 18/19 passed; lease test corrected and rerun below | `42ac26bf-c7ea-40c8-a13d-ab92c7d02136.xml` |
| Conch expiry, strongest lease, owner death, pause | 1/1 passed | `89649449-d43a-453f-8b09-1b1d27517b2e.xml` |
| King, environment, picker and travel regressions | 36/37 passed; travel-soak fixture subsequently corrected below | `3add9c35-2cb7-4ea8-b541-f802e8f609d7.xml` |
| Snare restriction, expiry and reachable AIR | Passed after using a genuinely useful combat response in the placement fixture | `3e1d41ce-97de-4106-b5be-b71c19b8e365.xml` |
| Eighteen three-zone handoffs | Passed; largest durable account 103,448 bytes; final visit 18 | `348d0276-8cb1-4b86-8913-d2e553618529.xml` |
| Three portrait sizes, safe area, Bardley three beats | Passed (two cases in a four-case run) | `7c897da0-81f4-451a-82c8-75ccb5e0f884.xml` |
| Mandatory Unity validation, before final motion/snare changes | Passed | `%TEMP%/DungeonMatcher-UnityValidation-d0632717-29c1-4311-8678-954cb6b9d5c4.log` |
| Broad affected regression | 183/201 passed, 18 failed, 0 skipped; failures subsequently cleared by focused runs | `be18b508-57a9-4d74-ad45-c7c6695d3e59.xml` |
| Corrected fixtures, Queen continuation, portraits and water view | 24/25 passed; new control run exposed dungeon Miner/header overlap | `31a0065e-dbaf-4340-9dce-f25683fab621.xml` |
| Layout correction, continuation, final Court portraits and water view | 9/10 passed; combined 18-scenario control case exceeded the runner's 180-second case limit after 15 completed scenarios | `6fd9c495-3189-43ca-b1ad-a61a343c219d.xml` |
| First split control run | 1/3 passed; fixture completion/lifecycle corrected before the final run | `3ffe08d7-29cc-4281-8b6f-623de3a3048e.xml` |
| Final three-zone control matrix | 3/3 cases passed, covering all 18 scenarios | `12d0244d-6218-4997-93c3-912226e4ac38.xml` |
| Final mandatory Unity validation | Passed, process exit 0 | `%TEMP%/DungeonMatcher-UnityValidation-77222112-3e84-47d2-a072-f34c42146de3.log` |

Travel-soak setup was corrected to initialize the environment through a real move
and wait for scene replacement before closing the restored pause UI. Its eighteen
handoffs then preserved board identities/resources, cleared source effects and
kept active scene objects and the saved account bounded. This is a scripted
handoff soak, not eighteen natural apex victories or human pacing evidence.

Original XML and full logs are retained. Rows overlap; do not add these counts.
`Production/CollectValidation.py` reconciles case identities and records the
superseding focused run for each result. The combined control case is explicitly
retired in favor of its three equivalent zone cases. The offline package includes
the five final regression XML files, mandatory validator log, exact case mapping
and SHA-256 records for 642 source/assets. Earlier evidence remains in the local
`.utmp/ForestValidation` directory. Final code and imported assets match the
recorded source hashes.

The first Conch expiry test used a coroutine delay which this EditMode runner
returned from without advancing five seconds. The corrected test waits on actual
scaled `Time.time`, verifies pause separately, and passed with the same expiry
assertion. No gameplay timer was weakened to make the test pass.

Earlier fixes found by tests: normalize Unity's materialized empty coffer object
back to null; revalidate a surviving reachable AIR route when selecting a coffer
footprint; wait for restored portrait spawn presentation before visual capture.

The broad run's seventeen cracked-chain failures exposed a synchronous EditMode
fixture that had not installed the runtime/static upgrade owners and inherited
the real user's permanent Bardley level. Its fixture now uses a disposable level-1
account and explicitly invokes the same owner lifecycle used in play. All damage,
scope, exception and reward assertions remain. The travel-soak energy assertion
also now includes the legitimate once-per-new-wave Prepared Casting grant when
that card is selected. Production reward behavior was not changed for these tests.

The short-screen dungeon minimum battle height increased from 256 to 272 logical
pixels after the control run caught a one-pixel Miner canvas/wave-plaque overlap.
Actor scale and drawings are unchanged; forest and Court use their existing higher
theme minimums. The 112 screen/safe-area math combinations, Court portrait captures
and six continuation tests passed afterward. The control matrix is split by zone
to fit the runner's unchanged per-case time limit; no scenario or assertion is removed.

The final split control fixtures use iterator lifecycles and the supported
`Run.ExitTo` path at their measurement cap. Their in-run responsiveness,
save/resume resource checks and exit assertion remain. This corrects the fixture's
completion behavior without changing production combat to satisfy the test.

## Observed control runs and continuation

The control matrix uses all three players in all three zones at permanent levels
1 and 5, seed 3101, default mastery, no supplies, the existing greedy input policy,
6x time and a 60-game-second observation cap. All 18 runs reached the cap alive;
their outcome is recorded as censored, with no artificial defeat. The runs made
23–30 accepted moves and used 1–3 abilities each. Court runs observed 1–2 floods
and 1–7 low-AIR samples. Seventeen runs reached a safe suspend/resume opportunity;
Dungeon Bardley at level 5 did not. Exact per-scenario results are in `ControlRuns.csv`.

The final eighteen-handoff soak reached visit 18 with a largest durable account
of 102,934 bytes. Six continuation tests also passed, including accepted Bomb and
Bardley recovery exactly once, atomic item recovery, resources and board identity,
draft/refinement, owned obstacles and the King's raised-sword attack pause. Queen
telegraph and recovery snapshots retained their targets without duplicate effects.

These short scripted runs test integration and recovery. They do not establish
full-visit length, mixed-skill comprehension, ten-minute pacing or human difficulty.

## Native and audio checks

Selected motion sheets use binary alpha and at most 21 opaque colors per clip.
`NativeChecks.json` records every frame size, selection, duration and SHA-256.
The pre-existing Royal ability glyph retains its original 253/254 edge alpha;
other prepared materials are opaque or binary-alpha. This is a preserved glyph,
not evidence that all source files have binary alpha.

The final export audit checked 300 Unity assets, 154 textures and 2,653 GUID
references, including dependencies in the installed Unity packages. All references
resolved; Court GUIDs were unique. Point filtering, disabled mipmaps, uncompressed
textures and the documented alpha exception passed. `ExportChecks.json` contains
the exact exported hashes.

The warmed water-view microprofile measured 120 calls in 2.4105 ms on this Windows
Editor, with 122 board child objects before and after and active bubble overlays.
The allocation-counter probe returned no usable measurement, so allocations are
recorded as null. This is not whole-frame, GPU, physical-device or long-run memory
profiling. The travel soak separately checks bounded scene objects and save size.

The original temporary music is stereo PCM at 32 kHz, 76.8 seconds. Measured peak
0.16141, zero clipped samples, loop endpoint difference 0.000611 full scale.
These measurements do not establish listening approval. The runtime import streams
Vorbis, preserves sample rate and shares the existing music/pause/mute owners.

## Phase and approval matrix

| Phase | Deliverable | Current classification |
| --- | --- | --- |
| 01 | Current-source audit and readiness | Executed before spending |
| 02 | Four stills and material proof | User-approved, with solid-color eye correction |
| 03 | Bardley three beats, no bubbles | Implemented; regression evidence retained |
| 04 | Flood/AIR foundation | Implemented; arithmetic and runtime checks executed |
| 05 | Remaining ten stills | Director-selected; user visual review pending |
| 06 | Motion pilots | Reviewed and selected during authorized continuation |
| 07 | Fourteen kits and production motion | Implemented; affected automated checks passed |
| 08 | Three compositions, marine UI, water and audio | Implemented; three portrait sizes and safe area checked; human visual/music approval pending |
| 09 | Three-zone travel and continuation | Implemented; eighteen handoffs and continuation checks passed |
| 10 | Weighted encounter book and pacing | 32 recipes implemented; human pacing/comprehension pending |
| 11 | Regression, device and portable review | Automated regression and offline package complete; physical Android NOT RUN |
| 12 | Final acceptance and handoff | Implementation ready for PR/user review; final human acceptance pending; no merge |

The user authorized continued implementation. This supersedes intermediate pack
stop gates but does not invent visual, music, human-pacing or hardware approval.
An Android device was not made available through the current tools. Background,
touch/audio behavior on physical Android remains an open release gate.

## Access and review

Open this checkout in Unity 6000.3.19f1. In MainMenu choose Play, then the temporary
**The Drowned Court** starting-zone entry. Continue restores an existing run instead
of changing its zone. In-run crystal destinations stay random among eligible zones.
For the shipped game, disable `RunLaunchOptions.TestingZonePickerEnabled`.

The offline review entry is
`ArtSource/DrownedCourt/Production/Review/Review.html`; its neighboring ZIP contains
actual PNG/GIF media and manifests. Runtime capture files are under
`.utmp/DrownedCourtValidation`. Human pacing evidence is not inferred from scripted
test elapsed time or desired visit length.

Production used **182 of 300** approved PixelLab subscription generations across
82 accepted jobs, verified by the balance change from 1,542 to 1,360. This leaves
38 of the initial 220 unused and the entire 80-generation correction reserve
untouched. Phase 2's 39 generations were separate. No credit purchases were made.
The first four eyes were corrected with 16 native pixel changes; all other pixels
and alpha were preserved. The later ten stills and 104 motion roles are selected
production art, pending the user's visual review.

## Next-zone handoff

Before a release: review the later ten designs/motion and marine screen materials;
listen to the temporary cue and approve or replace it; run mixed-skill full visits
with all three players; validate physical Android touch, safe areas, suspend/audio
and performance; disable the temporary starting-zone picker. None of these is
inferred from native asset checks or accelerated Editor control runs.

Reuse the existing zone definition, theme slots, native import conventions,
data-selected enemy runtimes, board mutation queue and atomic travel/save boundary.
Declare flood compatibility conservatively for visitors and summons. Keep raw,
selected and user-approved art distinct; start any later art work with its own
budget. The regional roadmap remains concept-locked. No next-zone production,
merge or publishing is authorized by this implementation task.
