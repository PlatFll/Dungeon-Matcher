# Animation and gameplay edge-case validation

## Results

The first complete inventory exercised 909 ordinary NUnit cases across the source revisions listed below. 819 passed and 90 failed on that first pass. The later runs retested changed systems, added fixtures and original failures. Across 1100 executed case instances, the latest recorded result for each of 913 distinct cases is 913 passed and 0 failed. 90 previously failing cases now pass.

Each row in [the case ledger](case-results.csv) records the actual case, result and source. [Latest outcome by case](coverage.csv) pairs first and latest results. [Batch receipts](batches.csv) record Unity PIDs, native exits, source SHAs, wall-clock start/end and the SHA-256 of each NUnit XML. Native logs and complete XML are retained below the worktree’s `.utmp/AnimationSweep/<batch>/` directory.

## Confirmed defects and corrections

| Area | What failed | What changed |
|---|---|---|
| Miner and roster | The Miner had stray glove and trouser pixels on frame 2. Other clips had material noise, dropped details or discontinuous gear. | Corrected 48 production clips on 27 actors. The Miner idle now follows its original motion with 26 corrected pixels in the second pose. [Game comparison](../../ArtDirection/AnimationSweep/Review.html) |
| Account and wallet | Ending an already suspended run truncated milestone history. A locked primary file could roll back the wallet. Saving after recovering a damaged primary could replace the last good backup. | Preserve all completed milestones, distinguish an inaccessible file from a malformed one and protect the recoverable copy. |
| Board input | Opening a targeting mode or losing ownership with a gem held could strand its pressed tint and scale. | Release pressed visuals with the active pointer. Cases exercise takeover and stale release. |
| Flood presentation | An empty, paused or still reduced-motion surface repeatedly rebuilt its mesh. | Dirty the geometry when the level or active surface changes. The active rise and bubbles retain their animation. |
| Authoring reimports | Rebuilding the Court or Frostwake roster dropped registered expansion enemies. | Rebuild authored entries and preserve the registered same-zone additions. |
| Channel save/replay | A fresh idle Mender wrote version 0 and resumed version 1. Identical actions produced a different full checkpoint. | New channel state uses its current schema. The restore path still accepts older saves. The original full-checkpoint assertions remain. |
| Portrait and status presentation | Large standing art could enter the header, legacy ability counters overlapped controls, and short-screen status icons used fractional pixels. | Fit oversized standing art to the actual floor-to-header space at one stable integer scale, position legacy counters against the header and retain native integer status pixels. A ninth active effect had also put the last row 4.5 logical pixels below the inset panel. Three-row player layouts now use the transparent padding inside the native badges, preserving the full button size. See the individual portrait cases for measured outcomes. |

## Regression inventory and evidence

The [system map](system-map.md) pairs mechanics with normal and adverse paths. The ordinary inventory includes Edit Mode unit tests and tests that enter Play Mode and drive the real Game scene. Historical authoring and screenshot generators tagged Explicit have independent behavior and are listed outside the ordinary inventory.

An early assembly-wide invocation (`full-01`) also selected historical generators. It was stopped and produced no NUnit result; it contributes zero passes. All 401 generated paths were archived, checked against SHA-256 and restored before continuing. The replacement used explicit names for ordinary methods. The session interruption left eight Game cohorts complete and no test process active. Cohorts 9–12 resumed in new Unity processes with their own receipts. At closeout, 33 later generated capture or import files were archived with SHA-256 checks before the original tracked copies were restored.

Regression fixtures were updated where their assumptions had been superseded by approved mechanics. Examples include Dungeon-first Play, the score/Resonance plaque, silver ability engravings, physical foot pivots on padded canvases, move-owned enemy contacts, current Court and Frostwake roster membership, protected capstone travel, and legacy Warden warning migration. The Obsidian Sentinel can prepare its special on turn two, which legitimately withholds its basic. The contact fixture now observes a first accepted swap for each of the 14 Ironvein actors independently. All 14 reached the expected impact sprite, hit exactly once, paused with combat, and returned to idle.

All eight Chrono Shutter fixtures passed at a supported phone viewport. The 18-transition zone soak passed with separate per-zone object baselines and destruction checks for departing roots. Native ability contacts and recovery, the large-actor portrait fit, old-save counters and all eight simultaneous status icons have individual case receipts.

## Build and executable checks

The first standalone import gate found mixed historical Git line endings in the clip receipts. All 395 texture hashes and 2,951 sprite IDs had already passed. All 353 clip files matched their existing hashes in LF or CRLF form (42 LF receipts, 311 CRLF). The editor verifier permits that encoding difference only for clip text. PNGs retain raw-byte checks. The corrected standalone import gate passed 17,494 checks. A negative regression changes the impact time and must fail against both forms of the receipt.

| Gate | Tested source | Exit |
|---|---|---:|
| candidate-compile | `fc9e98c42161b537346341c768d3d7e564a1fbcf` | 0 |
| recovered-compile | `ea3f8eaf29ee5e08e388d578ef20518c914d837d` | 0 |
| final-compile | `0ff16bcfe6b8f1c1df2cf5a303bf9fff31150529` | 0 |
| post-fix-compile | `a48af4b3fc2e1d772eeb9f63bb115a598b0005fb` | 0 |
| final-ident | `a48af4b3fc2e1d772eeb9f63bb115a598b0005fb` | 0 |
| final-import | `a48af4b3fc2e1d772eeb9f63bb115a598b0005fb` | 1 |
| final-android | `a48af4b3fc2e1d772eeb9f63bb115a598b0005fb` | 0 |
| receipt-compile | `65c05d596ffe37e1898079bb7c0d8fbca9bf1eb8` | 0 |
| verified-native-import | `65c05d596ffe37e1898079bb7c0d8fbca9bf1eb8` | 0 |
| closing-compile | `60d56a1ec82bc96910a8a7c90273dfc113b8dc32` | 0 |
| closing-android | `60d56a1ec82bc96910a8a7c90273dfc113b8dc32` | 0 |
| contacts-final-compile | `5303249f94b418bf40ce7c91d69d911b558c2b01` | 0 |
| solo-actor-compile | `84081ffc9996ce616856fbb2462180aaa5516e47` | 0 |

The Android development build succeeded with zero errors and seven warnings.

The final Android development APK is 108,804,138 bytes. Build source `60d56a1ec82bc96910a8a7c90273dfc113b8dc32`. APK SHA-256: `8def9338e4e8538dd904f79815b79d0841711248ac7b7779fd89d76bd91f6ac8`.

## Scope of the evidence

The scene captures and scripted Game inputs cover the tested clocks, formations, saved states, viewports and input paths. Physical touch feel, thermal behavior, device frame pacing and subjective motion quality still require a phone. On a phone, check the repaired hip at normal play speed and the crowded status panel while tapping its bottom row. All test processes owned by this run exited.
