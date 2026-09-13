# Crystal-aware board validity audit

Gameplay audit chunk 8 / validation backlog group 7.

Base: `4e2e959186492636d44c91c33514d65aa1f724c1` (main after PR #134).
Branch: `fix/crystal-aware-board-validity`.

**Implemented and source/diff reviewed. Unity compilation, test discovery/execution and Play Mode validation have NOT RUN.** The user requested one combined validation stage after the repository source audit. Do not start a separate validation pass or merge this branch now.

## Findings

### 1. Inaccessible crystals could manufacture a nonexistent ordinary move

The real match collector (`AddMatchesAt` / `CollectLine`) excludes Color Crystals from ordinary color lines. The hint system already uses a separate crystal mask for the same reason. However, live `HasAvailableMove()` fell back to a plain `GemType[,]` search after checking direct crystal swaps. That fallback treated a crystal as an ordinary gem of its retained hidden color.

When a crystal was pinned, or its adjacent swap partners were all pinned/blocked, the checker could still invent an ordinary match through it. A false positive could suppress an emergency reshuffle. `BuildSafePinnableGemList` also uses that checker, so it could approve pinning the last genuine crystal exit based on the same nonexistent ordinary move. Frozen gems and movable chains share this safety infrastructure, but ordinary pinned/frozen gems must still count toward real color matches; their inability to move is not an inability to match.

Minimal synthetic example, coordinates increasing left-to-right and bottom-to-top:

```text
Top row:    Topaz             Amethyst  Ruby
Bottom row: Crystal(Ruby)     Ruby      Sapphire
```

Pin the crystal. The right-column swap appears to form three Rubies to the old type-only checker, but the actual collector finds no ordinary match. Alternatively, leave the crystal movable and pin both of its neighbors: the same false match remains. The fixtures use small boards to isolate this rule; production dimensions are not changed.

### 2. Reshuffle acceptance ignored special identities

`TryCreateShuffledLayout` inspected only a candidate's colors. It could reject a perfectly usable crystal-only swap because no ordinary three-match was possible, or reject a colorless crystal line as though it were an already-existing ordinary match. This can waste shuffle attempts and unnecessarily enter the fallback that recolors gems. Fallback generation likewise did not know where the preserved crystals would be placed.

Candidate special positions must come from the candidate list in playable-cell order, not from the gems' current coordinates on the live board. Reshuffling keeps those identities while moving them.

These are source-confirmed inconsistencies, not claimed runtime reproductions.

## Correction

Only `Assets/_Game/Scripts/Board/BoardController.cs` changes production behavior:

- Live move search supplies the current crystal mask.
- A legal adjacent crystal swap is accepted only after the existing structural/pin checks, before ordinary color-equality rejection.
- Other prospective swaps and candidate match scans reuse the existing crystal-aware `HasHintMatchAt` predicate. They do not invoke the public random hint API, which is input/busy gated and inappropriate during board resolution.
- Each shuffled candidate supplies its own crystal mask in the same ordering as its type grid. Recolor fallback receives the mask of the chosen shuffled layout too.
- Existing private color-only helper signatures remain as delegating compatibility entry points. Generated color values remain valid GemType values; no sentinel is stored on a Gem.

The board remains the sole resolver. Queries temporarily swap type-array entries and restore them; they do not mutate live gem positions or grant rewards. No scene, prefab, dimensions, serialized fields, mastery preference, special identity, damage, shield, stagger or rendering changes. PR #134's shatter-time utility commitment is unchanged. Actual swap/clear/cascade/gravity animation code is unchanged.

This does not introduce a new policy for structurally impossible boards or promise that arbitrary dimensions/obstacle masks always admit a move. Existing shuffle attempt limits and failure fallback remain; do not silently remove persistent obstacles or spawn free crystals as a recovery policy. The intended contract here is correct crystal handling on filled live/candidate boards and blocked structural cells.

## Authored tests — NOT RUN

`Assets/_Game/Scripts/Editor/CrystalBoardValidityTests.cs`: **24 NUnit cases** (including parameterized cases). One case compares 64 deterministic small configurations with the actual match collector and hints; that loop has not run either.

Coverage:

- pinned crystal and trapped crystal hidden-color false positives, including pending pin reservations;
- ordinary fixed pins, movable chains and freezes remaining matchable;
- crystal + ordinary / Row Bomb / crystal swaps with equal hidden colors;
- either pinned endpoint rejecting a crystal swap;
- holes, barricades and Royal Standards remaining non-swap cells;
- pin safety rejecting closure of the final crystal exit, with temporary simulation state restored;
- candidate mask following destination positions and skipping blocked cells;
- hidden crystal triples ignored, real ordinary triples rejected;
- existing-identity shuffle acceptance on a crystal-only layout;
- fallback generation accepting a crystal-only move without invalid color values;
- prospective-swap array restoration, repeated read-only queries, no clear/reward events;
- seeded comparison to actual `FindMatchesFrom` after reversible fixture-only swaps and to existing hint validation.

Fixtures are inactive disposable GameObjects. They never enter Play Mode, write PlayerPrefs or change source assets. Unity RNG state is restored. They inspect logic, not animation, actual enemy placement coroutines, automatic reshuffle scheduling or whole-scene integration.

## Final combined validation backlog

1. Discover/run all 24 cases on the integrated audit state. Check the directed regressions against baseline where practical; do not weaken expectations to get PASS.
2. Reproduce a real dead board with an inaccessible crystal on the production-sized board. Ensure automatic recovery occurs and hints/move availability agree after settlement.
3. Exercise actual Crossbow, Court Mage and Captain placement requests near the last legal move. Unsafe placement must be refused; real matches through ordinary pinned/frozen gems must remain legal. Respect each enemy's existing eligibility rules: this does not authorize freezing a crystal.
4. Exercise normal and forced reshuffles with crystals, colored bombs, holes, barricades, standards and released pins. Inspect the final actual arrangement: same identities where no recolor was needed, no accidental ordinary starting matches, at least one real move in the designed solvable fixture, no loss/duplication/self-activation of specials.
5. Exercise fallback recoloring with the chosen candidate crystal positions, not stale live positions. Check no invalid sprite indexing, premature input unlock, rewards merely for inspection/reshuffle, or extra valid-player-turn events.
6. Recheck bomb/crystal/Cracked clearing, obstacle legal-move checks, shared gameplay and Royal regressions, earlier audit suites and required `Tools/Validate-Unity.ps1` in the final authorized pass. Preserve pixel presentation, source assets and user preferences.

This production file does not overlap the production files changed by PR #135–#140. All those branches still need a combined integration and validation pass. The full repository audit remains incomplete.
