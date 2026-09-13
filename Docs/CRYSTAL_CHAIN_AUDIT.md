# Gameplay audit, chunk 2: crystal-chain trigger attribution

Base: `4e2e959186492636d44c91c33514d65aa1f724c1` (main after PR #134).
Branch: `fix/crystal-chain-trigger-attribution`.

## Scope and status

This focused source review follows PR #134. It covers the normal, converted-bomb and bomb-triggered crystal paths in `BoardController.SpecialGems.cs`, the shared utility-bomb expansion helpers in `BoardController.PoisonBombs.cs`, crystal request data, and the existing grid comparator. The two confirmed defects below have code fixes and new EditMode regression coverage. **Unity compilation, EditMode tests, Play Mode validators and visual tests have NOT been run in this GitHub-only environment.**

This is not a full-repository or all-combinations certification. Earlier PR #134 validation is evidence for that earlier commit, not a test result for this branch. The user has not authorized merging this new branch.

## 1. Converted chains give crystals the wrong trigger color

`BuildConvertedCrystalBombActivationSet` traverses collateral bombs using its existing FIFO. Each iteration knows the current `bomb`, but the converted clear helper previously received only `activatedBomb` (the sequence root). When a collateral bomb hit a crystal, the queued `BombTriggeredCrystalRequest.TriggerGemType` therefore came from the root, not the bomb whose footprint reached that crystal.

Example (zero-based board coordinates):

- Ruby Row Bomb at (0,0), acting as the current converted bomb.
- Sapphire Column Bomb at (3,0), hit by the Ruby row.
- Color Crystal at (3,3), reached only by the Sapphire column.

The converted path previously queued **Ruby**. The ordinary bomb expansion path for the same geometry queued **Sapphire**. Because the later crystal target set reads `TriggerGemType`, this changes which gems are selected and therefore can change later damage, energy, healing and chains.

### Fix

Pass both identities explicitly to `TryAddGemToConvertedBombClearSet`:

- `activatedBomb` retains its existing role in converted-bomb protection;
- `triggeringBomb` identifies the current blast and supplies the crystal's color.

Update both directional callers and the common area-bomb caller used by Poison, Healing and Shield Bombs. No bomb conversion, blast geometry, protection set, timing or utility commitment changes are made.

## 2. Simultaneous bomb seeds depend on collection enumeration

Normal bomb expansion previously enqueued initial seeds directly from `HashSet<Gem>`. Crystal requests keep the first arrival. For two different-color seed bombs reaching the same crystal, initial collection order could therefore decide its trigger color and alter subsequent gameplay on an otherwise identical board.

### Fix

Copy and sort initial seeds with the existing `CompareGemsByGridPosition` before adding them to the existing FIFO. This uses ascending internal row, then column, with nulls skipped. Descendant traversal and first-arrival deduplication remain unchanged. The caller's set is not mutated, and this adds no random draws.

This stabilizes simultaneous planning; it is not a claim that the game's random refill is replay-seeded. It does not replace FIFO traversal with a new global priority system or change the nearest-first order of a crystal's converted targets.

## Tests added

`Assets/_Game/Scripts/Editor/CrystalChainAttributionTests.cs` contains 15 EditMode cases:

- five collateral bomb types (Row, Column, Poison, Healing, Shield) give the actual impact color and agree with ordinary expansion;
- the later target set selects that color and excludes other colors and other crystals;
- five direct-hit bomb types retain their own color;
- pending converted bombs remain protected, their footprints do not expand early, and they can expand on their own turn;
- overlapping collateral blasts queue one crystal request; repeated planning preserves gem identity, type, special type, coordinates and clear/outcome report counts;
- both insertion orders of simultaneous seeds choose the same trigger in grid order, including null-seed handling;
- null input and non-preserving expansion retain their existing behavior.

The fixture constructs an inactive, disposable board and populates gem identity/type data without optional art. It invokes production planners rather than duplicating their algorithms. These tests intentionally do not assert live shatter timing, effect counts, coroutine completion or on-screen appearance: use the existing Play Mode validators and manual checks for those. No PlayerPrefs or serialized gameplay/art assets are edited.

## Validation handoff

Before merge:

1. Compile using the project's pinned Unity version. Run `CrystalChainAttributionTests` and verify all 15 cases are discovered and pass. The five mixed-color cases should fail against the pre-fix planners; use an isolated worktree if checking the baseline.
2. Run the existing Gameplay Bomb Rewards validator (PR #134), Gameplay Edge Cases, Gameplay Supplementary Cases, and Enemy Stagger Meter. Follow each fixture's setup requirements. Run `Tools/Validate-Unity.ps1` with Unity closed according to AGENTS.md.
3. In Play Mode, arrange the Ruby-row -> Sapphire-column -> crystal example above during a crystal conversion. Confirm the later crystal targets Sapphire, not Ruby. Repeat with one collateral utility bomb, with the crystal outside the original blast but inside the collateral blast. Observe actual target/clear data, not only color VFX.
4. Test multiple bombs hitting one crystal, multiple waiting converted bombs, a further crystal in the chain, and a double-crystal sweep. Verify one activation per eligible bomb/crystal, no premature activation of protected bombs, no duplicate rewards or softlock, and unchanged non-activating double-sweep semantics.
5. Recheck wave-ending Healing/Shield Bomb rewards and next-wave isolation. Make sure the board settles and waves resume. Preserve mastery settings, art, shield cap, Aegis Reservoir, stagger tuning, layout and pixel imports.
6. If anything fails, distinguish fixture failure from production failure; fix the root cause and rerun. Record the exact tested commit and results in the PR. Do not label unrun or blocked checks PASS and do not merge without approval.

## Files intentionally untouched

`BoardController.cs` (including PR #134's shatter fix), combat/HP/shield code, mastery settings, energy/healing modifiers, wave ownership, double-crystal orchestration, Bardley's ability implementation, art, scenes, prefabs and balance data remain unchanged in this chunk.
