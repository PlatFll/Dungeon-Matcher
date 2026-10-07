# Roster/endless revision checkpoint — 2026-10-07

**Stopped new tranches at the user's 5% remaining-usage threshold.** No merge
authorized. No background implementation or Unity job remains running. Recheck
instructions and usage before resuming; do not consume a reset credit.

## Repository and source

- Branch: `codex/roster-endless-revision`.
- Exact validated implementation HEAD: `78c98cd6b1603e9a844ab3af973c6a0327db2129`.
- Base main: `e572a88227d036af765e057831c46a2cc067630d` (merged PR #181).
- A documentation-only checkpoint commit follows this implementation HEAD. Use
  `git rev-parse HEAD` for the current review HEAD; no gameplay changes follow it.
- Full user brief: `C:/Users/USER/.codex/attachments/6ce2a904-6cc6-46a9-acda-b262f66dcd5a/Pasted text.txt`.
- [Worklist](../ROSTER_ENDLESS_REVISION.md), [evidence](../Validation/ROSTER_ENDLESS_REVISION.md),
  [measured scaling table](../Validation/ENDLESS_SCALING_TABLE.md).

## Completed commits

1. `eafd0f8fdfd6aecde4aebe86677eda5a57ebb5d1` — tranche 1: remove extra channel
   recovery moves in Mender/Cantor, Forest milestones and Court abilities. Success
   and fizzle restart ordinary cadence; genuine interruption uses ordinary Stagger
   only. Legacy terminal states migrate without replaying effects/rotation.
2. `78c98cd6b1603e9a844ab3af973c6a0327db2129` — tranche 11: shared production HP
   anchors 1/15/30/50/70/100 → 1/1.4/2/3/4/6×, provisional linear continuation
   to 9× at 150. Damage, attack timing and special cadence remain unchanged;
   player-power correction stays off. Continue's wave plaque now follows restored
   global depth without replaying spawn/reward events.

Both slices are coherent and validated. No mechanic is partially coded inside
them. The complete requested revision is unfinished; keep its PR a draft.

## Verification

- **126 unique passing affected cases**, counting each name once after its latest
  rerun. Initial fixture failures and corrections are documented in the evidence.
- Channel slice: 28 unique passing checks, including an eighteen-handoff travel
  soak, legacy/current continuation, Stagger, target loss and announcements.
- HP: all 48 definitions at waves 1/15/30/50/70/100/150; three live-zone checks
  verify spawned stats and damaged HP across Continue at every depth, then global
  150→151 / local 1 on travel. Monotonic finite growth also tested through wave
  1,000,000; this does not establish arbitrary-integer-depth behavior.
- 88 affected balance, damage, wave lifecycle and King checks passed. They overlap
  one channel test. Final three live reruns also cover the wave-plaque fix.
- Final mandatory `Tools/Validate-Unity.ps1` passed, Unity 6000.3.19f1, exit 0.
  Log: `C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-a7c9acf4-0dbf-45f0-a466-f1c660a2e437.log`.
- Editor audio muted. Twelve captures cover all zones at 720×1280, 1080×1920,
  1080×2400 and safe-inset 1080×2400. Four-digit HP and WAVE 150 fit.
- Local gallery: `.utmp/RosterEndless/Review.html`; overview:
  `.utmp/RosterEndless/Scaling-Viewport-Review.png`. Same directory contains native
  captures, all-definition TSVs and live-runtime TSVs.
- No new sprite/animation art, no PixelLab calls, no credit purchase/reset.
- No physical-device, human pacing or completed-new-kit validation claim.

## Exact next task

Resume **tranche 2: Minister display rename and King Judgment runtime**. Read the
full brief and actual King/board owners first. Current display `Royal Arcanist`,
stable ID `royal_arcanist`, asset `Enemy_RoyalArchbishop`. Keep that ID, asset name
and references. Inspection and cast names already use definition/generic kit data;
update display-facing docs/reference records too.

Judgment must keep physical gem identities and stable saved order, hold the board
once, consume each surviving mark before its contact-timed strike, use the unique
empowered third hit only when all three survive, then settle once. Preserve normal
damage; empowered tuning is provisional serialized data. Tranche 3 authors
JudgmentStrike1/Strike2/Finisher from the approved King identity.

No new paid PixelLab ceiling was supplied; old Forest/Court allowances cannot be
reused. Prefer requested direct editing. Obtain a bounded allowance only if paid
generation is genuinely required.

## Remaining work

- 2–3: Minister display rename; sequential held-board Judgment and three authored
  motions, including a unique two-handed empowered finisher.
- 4: instant queued Rootbinder/Warden roots; readable nonstacking Warded (25%, all
  allies/summons, multiple valid sources); Treant physical marks, any answer fully
  cancels without Stagger, otherwise full hit/consume/settle.
- 5: direct wooden shield-root and Heartroot art, both durability states/pulse.
- 6: 16–18-move flood, five initial reachable bubbles, captured oxygen counts as
  reserve, emergency supply only after exhaustion, visible separate −1/+2 AIR.
- 7: instant exact-two Thief / exact-three Warden capture at authored beats;
  independent one/two-hit coffers, safe placement, +4/full-AIR payouts; Pressure
  Lance requires two bubbles and either answer completely cancels.
- 8: Queen steals all free bubbles; first armored hit exposes pearl and rotates a
  legal 2–3-cell cardinal line, preserving specials/blockers, no intermediate refill;
  second hit returns two AIR per captured charge, capped at five.
- 9: retire Crushing Depths; Nacre Tribute marks up to three bubbles for three moves.
  Survivors give Queen-first stable round-robin Fortified, cap two per enemy. One
  stack halves one direct-player damage packet; periodic damage does not consume it.
  Status/inspection, saved stacks and front/back elliptical native pink pearls.
- 10: restrained flooded swap/fall bubbles/ripple, stronger coffer wake, reduced
  motion; presentation only.
- 12: finish owner docs as mechanics land, native art gallery, all requested
  adversarial kit tests, final broad regression and four-viewport review. Preserve
  Court Muster and all specified merged systems. Do not merge.

## Preserve unrelated changes / current status

Implementation commits leave exactly these pre-existing modifications:

```text
 M Assets/_Game/Resources/BattleEnvironments/DrownedCourt_Throne.prefab
 M Assets/_Game/Resources/UI/Consumables/SlotDisabled.png.meta
 M Assets/_Game/Resources/UI/Consumables/SlotHighlighted.png.meta
 M Assets/_Game/Resources/UI/Consumables/SlotPressed.png.meta
```

All four retain starting SHA-256 hashes in
`.utmp/StatusRevision/UserChanges/manifest.json`. Never stage/reset them or run the
full Court/theme importer over the throne prefab. This checkpoint is the only
subsequent documentation edit before its own commit. No implementation work is
uncommitted; no job needs polling. Prior PR #181's checkpoint is in Git history.

An older pacing paragraph still says aquatic travel is unavailable. Actual main
and the newer Court contract support three connected zones. Reconcile that older
paragraph in final docs; do not revert travel behavior.

Last usage: **5% remaining**, shared weekly window. User thresholds: 5% no new
major tranche; 3% validation/commits/docs/handoff only; 1% stop all new work.
