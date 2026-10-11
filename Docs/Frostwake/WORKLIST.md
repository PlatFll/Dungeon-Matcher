# Frostwake Peaks production

2026-10-11. Branch `codex/frostwake-zone-production`, based on merged main
`3291cd968b53394d24f225fe16f48364996121e1` (PR #190).

## Authorization and current state

The latest request expands the prior Frostwake planning/foundation scope to the
complete mountain roster and its kits. All eleven still candidates in
`ArtSource/Frostwake/technical-checks.json` were explicitly approved for animation
in chat. This supersedes the earlier foundation-only scope. Rattlebones remains
the primary rendering authority; no new golden membership is implied.

The Minotaur eye-only revision uses PixelLab's free exact editor: darker amber,
near-black eye contours, unchanged smile/equipment/poses. Original approved
sources remain preserved. See `ArtSource/FrostwakePlayables/eye-revision`.

PixelLab start balance: 173 subscription generations, zero purchased credits.
First roster batch: 66 generations plus 1 for ten palette cleanups. All eleven
PNGs are 112x88, binary alpha, 15 opaque colors; occupied bounds vary. These are
actual measurements, not inferred from the composite. Actual golden PNG inputs,
prompts, job receipts and hashes are in `ArtSource/Frostwake`.

## Implementation plan

1. Preserve still approval and exact native identity; prepare action padding.
2. Generate/review native idles, attacks, ability actions and defeats. Lock each
   character's palette across motion. Retain optional-art gameplay fallbacks.
3. Author the eleven definitions with provisional five-step combat values.
   Reuse centralized shields, healing, status and accepted-move owners.
4. Add only the missing ice-placement and interruptible mountain strike kits.
   BoardController continues to own all safe placement and resolution.
5. Build weighted teaching/relief/leader encounters, continuation and random
   travel using existing zone architecture. No player-power scaling or HP padding.
6. Generate mountain scenery, quiet stone/timber gameplay materials and three
   readable ice stages; retain current main menu/settings and shared rank UI.
7. Integrate audio, verify native presentation and run focused Unity regressions
   plus `Tools/Validate-Unity.ps1` with audio muted. Review complete diff, preserve
   four pre-existing user edits, then open a PR.

## Provisional kit direction

| Enemy | Role | Kit |
| --- | --- | --- |
| Iceborn Raider | Normal | Single axe attack; simple teaching/relief actor. |
| Frostfang Wolf | Normal | Low HP, fast bite; no board effect. |
| Harpoon Hunter | Special | Marks one gem for two moves; clear it to cancel the harpoon strike and Stagger the caster. |
| Rimecarver | Special | Places one Frost encasement through the existing safe queue. |
| Shieldmaiden | Special/support | Grants ordinary shields to the party; central caps and shield gating. |
| Rime Skald | Special/support | Short non-stacking attack-speed rally for other living allies. |
| Hearthkeeper | Special/support | Interruptible two-move heal of a wounded ally. |
| Frostblade Reaver | Special | Applies short Frostbite; sword basic has two authored contacts. |
| Glacier Troll | Miniboss | Alternates breakable ordinary ice armor and an interruptible marked-gem slam. Shield break Staggers. |
| Iceborn Champion | Miniboss | Telegraphs a heavier hammer blow on two marked gems; clear either to cancel and Stagger. |
| Jarl Haldra | Boss | Locked Glacier Cleave: full Bifrost target, three response moves, full thaw cancels, partial thaw reduces damage, remaining Bifrost also applies Frostbite. |

All numerical values and encounter windows remain first-pass tuning. Latest
unified accepted-move combat is authoritative over stale timed-combat history.

## Checkpoint

All eleven stills are approved. Minotaur eye-only verification passes for 33
source cels; the six existing motion sheets and portrait are updated. All 416
generated motion cels were checked against each approved 15-color palette and
binary alpha, then selected into 70 Unity clips. Fixed native canvas is 128x104,
with a four-pixel floor margin. Actual native comparisons and GIFs are under
`ArtSource/Frostwake/production`.

The eleven kits, weighted encounters, five-zone travel registration, three
scenery variants, timber/stone UI, transparent ice and original temporary cue
are imported. Integration corrected native frame dimensions, floor pivots,
player-frame occlusion, scene coverage and short-screen icon scaling. All 24
affected checks pass on their latest executions, and the mandatory Unity
validator passed on final source. See [executed evidence](../Validation/FROSTWAKE_PRODUCTION.md).
The user authorized integration to main after validation. Physical-device
performance, human pacing and listening review remain separate.

## Production usage

PixelLab receipts: 66 for eleven stills, 1 for ten still palette passes,
56 for motion, 22 for environment/UI/effects including eight composition
revisions, 2.5 for 25 shared motion palette passes: **147.5 subscription generations**. Free exact eye, hit, shell, Champion
impact and modular-frame edits cost zero. No credit purchases. The balance tool
reported 26 remaining / 1973 used out of 2000 after production; it displays
rounded integers, so the itemized receipt sum is the precise task ledger.
Rejected source attempts remain marked in the lineage and are included in usage.
