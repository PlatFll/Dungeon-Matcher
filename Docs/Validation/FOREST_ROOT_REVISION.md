# Forest root revision — 2026-10-03

Branch: `codex/forest-roots-and-roster-concepts`, based on production #174.
The user's current **no publishing** instruction keeps this revision local:
no push, new pull request, merge, build publication or later-zone implementation.

## Implemented and reviewed

- Visual-only cell vines; ordinary movement, gravity, matches and bombs remain
  available. Normal edge/frontier growth has its own saved deadline.
- Rootbinder's one-hit nonspreading Root; Warden's two-hit spreading Root;
  Matriarch's linked two-hit Heartroots. Clearing a side vine opens the side;
  a later clear through it damages the Root. Structural safety/refill stay in
  BoardController. Root placement tries eligible cells within a bounded pass.
- Warden's 25% reduction covers every ally, including newly spawned allies
  before LateUpdate. Existing damage rounding, redirection and shield gates stay.
- Matriarch's two-move AoE Renew, independent Surge and three-move Harvest.
  Heartroots survive Harvest; breaking both invokes ordinary EnemyStagger.
- Exposure/weakness bonus removed. True warning interruption uses normal
  stagger; invalid targets fizzle. Existing Mender target identity is preserved.
- Versioned root/vine continuation, old vine-pin migration, preserved real
  chains, and photographs that cannot repair or resurrect solved roots.
- Guide, current design/architecture/timing/kit documents and project state updated.
  Future crumbling dungeon tiles and two-chain Special Royal Arbalist are notes only.

## Executed evidence

**Required final Unity validator passed, exit 0**, Unity 6000.3.19f1.
Command: `powershell -ExecutionPolicy Bypass -File Tools/Validate-Unity.ps1`.
Log: `C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-6c8bd94a-55c1-46ee-a7fd-7c3e9a5f087f.log`.

**Final forest suite: 30 passed, 0 failed, 0 skipped**, Unity 6000.3.19f1, graphics
enabled at 1080x1920. Command: `Tools/Test-ForestFoundation.ps1 -Graphics -Filter
"AcceptedMoveStateTests;ForestFoundationTests;ForestFoundationPlayTests"`.
XML: [final forest results](ForestRoots/897963a7-a595-4a85-9883-d1fdbdf9f20d.xml).
Coverage includes hybrid seconds basics, move deadlines, replay, fixed-target
healing, vine movement/clearing/cadence, root durability and photos, normal stagger
versus target fizzle, AoE heal, Harvest, legacy migration and immediate ally aura.

The preceding affected-system run passed **64 shared combat/board tests**:
EnemyDamageResult 24, CombatPolish 16, BarricadeBannerGravity 12,
ReshufflePinReservation 12. **Three rendered forest tests** also passed at
720x1280, 1080x1920 and 1080x2400, including a simulated safe area.
[That run's XML](ForestRoots/0798bc81-6f89-4132-b37a-ecf81d351e69.xml) records
71 passes and one forest pair-placement failure. The bounded placement correction
and immediate-spawn protection were subsequently checked by the final 30/30 run.
Shared non-forest behavior and visual geometry did not change after those checks.

Inspected [Warden warning](ForestRoots/08-warden.png) and
[Matriarch Heartroots](ForestRoots/09-matriarch.png). Vines retain readable gems;
Heartroots use the approved knot overlay plus two durability pips. Existing
character PNGs, animation sheets and clips have no changes. Optional art remains
non-authoritative. No physical-device, Android build or human balance claim.

Iteration logs remain in `.utmp/ForestValidation/`. Early failures included an
incorrect landscape test viewport, two stale test references, teardown access to
a destroyed board, incidental extra cascade hits in a single-hit fixture, and
root placement abandoning eligible candidates too early. These were corrected;
assertions were updated to the requested mechanics without ignoring errors.

The previous broad baseline's 34 unrelated lifecycle/audio failures are documented
in FOREST_PRODUCTION_PHASE_05_06.md; this task does not claim to resolve them.

## Art approval and usage

Six extra enemy designs are locked by SHA-256 in
`ArtSource/Forest/RosterConcepts/APPROVED_DESIGNS.json`. Their original PNGs were
verified unchanged. No animation, playable kit or Unity roster import was made
for the additional six. The original Ancient Treant is rejected and retained.

Three replacement Treants are unaltered PixelLab Pro Flash outputs: A Hollow Oak,
B Weeping Willow, C Old Stump. Each canvas is 96x96 with binary transparency;
opaque palettes are 11, 23 and 33 respectively. Exact bounds, jobs and hashes are
in `TreantVariants/manifest.json`; both native and 3x comparisons accompany them.
B/C have more internal texture than the current cast; selection and any later
light shading cleanup remain distinct from animation approval.

Actual still-batch usage is **50/60 subscription generations**, including 15 for
the three alternatives. Balance: 1776 ->1741 ->1726. Ten remain. No purchases,
wholesale regeneration of the six approved designs or active generation jobs.

Stop for the user's Treant selection. Existing starter animations are reused.
