# Ironvein Excavation — current-source audit

2026-10-08. Phase 01. Implementation authorized; merge is not authorized.

## Evidence and authority

- Fetched `origin/main`: `a2bcf86e7d8d92707ac3fa857e72305c2f115824` (merged #182).
  Branch: `codex/ironvein-excavation`. GitHub open-PR API returned zero PRs.
- Read current global RTK/AGENTS and project AGENTS, PROJECT_STATE, relevant
  design, progression, architecture, status, zone and balance records. No nested
  AGENTS was found in the project source scopes.
- The supplied ZIP's 85 manifest hashes verified. Extracted source is at
  `.utmp/IronveinPack/Dungeon_Matcher_Ironvein_Excavation_Astra_Pack/`.
  Its source SHA is orientation only; no reset was performed.
- Read the start documents, master contract, decision log, orchestration,
  phase instructions and stone/network/roster/boss/status/ownership/save/test
  contracts. Each phase will inspect its detailed source before implementation.
- Current Unity is **6000.3.19f1**. This audit has not run Unity validation.
  Validation runs must mute Editor audio without altering player preferences.

## Preserved starting changes

These four existing files remain outside Ironvein commits:

| File | Starting SHA256 |
| --- | --- |
| `Assets/_Game/Resources/BattleEnvironments/DrownedCourt_Throne.prefab` | `f2a3e6059c4df8ebce3279e7ac29ca5acd9a52e529425abe70c51ecab097bd10` |
| `Assets/_Game/Resources/UI/Consumables/SlotDisabled.png.meta` | `0f7e12e81377e9d521a4d132a17a3b3bfcf575daac8434f77bf20849575f5518` |
| `Assets/_Game/Resources/UI/Consumables/SlotHighlighted.png.meta` | `3a32303ab05166168f39ca8f6f06367234df30cb46393acb6fa5fde7331762f7` |
| `Assets/_Game/Resources/UI/Consumables/SlotPressed.png.meta` | `0443273d14468ccb1584abcf6ab204674debcae15cda563f6aba543e6dc7e88f` |

## Actual implementation and integration plan

| Existing owner inspected | Ironvein extension |
| --- | --- |
| `ZoneDefinition`, `ZoneRuntimeContext` | Opt-in mine tuning and additive theme. Keep current affiliation, seconds basics/move abilities, global difficulty and layout owners. |
| `BoardController.Barricades` | Reuse structural occupancy, capped legal placement, clear-hit deduplication, gravity and view lifecycle. Add independent material stage, stable identity and age; durability damage must not change stage into wood. |
| Board mutation queue and `CombatMoveClock` | Mine state remains board-owned. Age before enemy work; drain deterministic machine work under the existing queue/hold before wave progression. No second resolver. |
| `EnemyAutoAttack` | Existing keyed next-sequence modifiers already consume once per entire basic sequence. Add a saved ore-token owner rather than a new attack loop. |
| `EnemyActor`, `EnemyStagger`, `PlayerStatusRuntime` | Central damage/shield/rounding stays authoritative. Rattled affects new Stagger accumulation only; environmental actions never cleanse it. |
| Definition-selected abilities / wave summon service | Seven specialists, two minibosses, boss and independent turret use current actor IDs, guarded contact, safe board requests, real slots and continuation owners. |
| `RunContinuation`, board/actor snapshots | Version mine payloads, reject unknown future data, retain journal replay and settled checkpoints. Extend the current explicit supported-zone list. |
| `ZoneTravelController`, `PrepareZoneArrival` | Add fourth destination only when ready. Clean mine state on detached destination snapshot; preserve player resources, board, global depth and atomic failure behavior. |
| Current theme/importers and battle background owner | Add mine assets and bindings only. Do not run importers that rebuild the user-edited Court Throne. |

**Critical distinction:** the portable small drill skips protected specials,
clears ordinary gems without rewards, hits the first stone for exactly one
durability and stops even when it breaks. Environmental drills clear the full
row/column including all stone tiers. Intersecting launches settle once and
cannot recharge themselves. These require separate regression cases.

The existing source has no mine implementation. `FUTURE_ZONE_LOCKS.md` currently
describes a documentation-only cave concept; this user request authorizes its
implementation. Older PROJECT_STATE historical sections mention v1.5/v1.9 and
formerly unmerged PRs; the current top checkpoint and v1.11 guide take precedence.
The mockup is composition authority only: it does not replace current settings,
HP/shield, three-slot battle layout or potion/ability/bomb controls.

## Art authority and status

| Source | Inspection / status |
| --- | --- |
| `Docs/ArtDirection/Dungeon_Matcher_Art_Direction.txt` | Current v1.11: shared outline `#0A0D11`, broad material clusters, top-left light, crisp native pixels. |
| `Dungeon_Matcher_Art_References.png` | Visually inspected. Historical v1.0 inspection board; source labels retain working-reference status. Bardley is palette-only/placement pending on this board. |
| `ArtSource/LocalEnemies/Miner_Reference.png` | Actual individual native PNG visually inspected: useful human material/outline reference, not a dwarf design to copy. |
| `ArtSource/Forest/Approved/` | Approved race silhouettes and guide-colored stills; motion approvals remain separate. |
| Pack full-screen mockup | Visually inspected 941×1672 concept. Dark layered cave, timber, orange ore, warm lanterns, rails and minecart, restrained industrial frames. Recreate as modular native art. |
| Ironvein stills/motion/environment/UI/audio | Not produced. New outputs remain review candidates. Temporary music needs explicit provenance/listening status. |

Do not infer sprite dimensions, palette or alpha from a composite. Native PNGs
and all frames need individual measurements, hashes, integer-scale comparisons
and import checks. Four pilots precede expansion: Pickaxe Delver, Ore Hauler,
Bore Engineer and Grand Delver; also prove one reversible powered weapon.

## Tools, allowance and gates

PixelLab `get_balance` succeeded: active Pixel Apprentice subscription,
**1,360/2,000 generations remaining**, 640 used, **$0 purchased credits**;
reset 2026-10-29. Ironvein production usage: **zero**.

A help query was rejected by automatic approval review because it would disclose
project production details to PixelLab without authorization for that payload.
Requested explicit authorization for prompts/specifications/native dimensions,
mockup and selected approved references. Separately proposed an Ironvein ceiling
of **400 subscription generations: 300 initial + 100 correction reserve**.
Both requests are pending. Old Forest/Court allowances do not carry over; no
credit purchases. Local engineering can continue while art requests wait.

The last observed account window had 73% remaining. Check actual usage at phase
boundaries. At 5% stop starting phases, at 3% validate/handoff only, at 1% stop.
An unavailable reading is unavailable, never an estimated percentage.

## Validation plan

Use focused Unity tests per mechanical tranche, then the required
`Tools/Validate-Unity.ps1` before a completed PR. Cover stone maturation/hits,
both distinct drills, ore/death/summon exactly-once behavior, all kits, Rattled,
boss phase rewards, photographs, replay/save/travel, prior zones and player
abilities. Capture 720×1280, 1080×1920, 1080×2400 and a safe-inset shape; inspect
native assets and animation anchors. Report human pacing, art/music approval and
physical-device checks separately. The pack's acceptance matrix is the checklist,
not evidence that those checks have already passed.

Next: Phase 02 zone stub and maturing stones. See [WORKLIST.md](WORKLIST.md).
