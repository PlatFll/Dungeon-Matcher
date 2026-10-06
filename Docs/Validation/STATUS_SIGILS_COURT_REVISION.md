# Status, caster presentation and Court revision

Revision date: 2026-10-06. Branch: `codex/status-sigils-court-revision`.
Base: `2783df8` (merged PR #180). **This revision must remain unmerged for review.**

## Implemented changes / changelog

- Seven shared player debuffs use data definitions and the existing centralized
  damage, healing and energy paths. Accepted complete actions expire durations;
  reapplying a status refreshes one channel. Fear retains its source identity and
  clears when that source dies or Staggers. Continue restores durations without
  replaying resource changes. Native 16px icons and move counts sit in player UI.
- Slippery previews `[A B C] → [B C A]` before commitment in flooded Court only.
  Final-arrangement legality drives acceptance, hints and response checks. Blocked
  extra steps fall back to an ordinary adjacent swap. Cascades, refill and ability
  clears retain their existing behavior. It is exposed in enemy data but unassigned.
- Successful casts show their shared inspection name in rising, fading off-white
  Thaleah. Readiness, failed placement, canceled pre-casts, ticks and Continue do
  not announce a fresh cast.
- Actual occupied slots supply triangle/square/ring identity beside enemy HP and
  on targets. Gem marks follow gravity; cell marks remain fixed; row/column marks
  sit outside the corresponding edge. Overlapping sources stack in slot order.
  Native glyphs render above the board frame and below modals/travel smoke.
- Court uses the existing faction field: ten Reef identities and four Nacre
  identities. The 32 recipes comprise 24 Reef, six Court and two deliberate mixed
  templates. Queen starts with Court escorts. Bands, weights and kits remain intact.
- Direct pixel additions give Hammerhead and Captain compact tails across three
  source still canvases and 13 motion sheets (118 frames). Original opaque body
  pixels, palette, canvas and floor remain intact. No paid generations were used.
- Durable cross-zone variation/lore rules and the future cave, snow and Reef boss
  concepts are documented. Those future zones/boss are not implemented.

## Tranche commits

| Commit | Scope |
| --- | --- |
| `7c972f1` | Shared design rules and future-only locks |
| `df8b06f` | Status backend, central hooks and source-aware persistence |
| `391bcf2` | Native status UI and optional generic status cast |
| `cd138e9` | Deterministic flooded Slippery input, preview and legality |
| `e9f0687` | Court culture taxonomy and encounter recipes |
| `ed1ced5` | Direct shark-tail source/motion revisions and review package |
| `73e06e1` | Successful cast announcements and inspection names |
| `9dd7742` | Slot sigils, target lifetimes and fixed-cell Slippery correction |
| `cac4068` | Forecast freshness and final gem-readable target presentation |

## Executed focused evidence

XMLs below are under `.utmp/ForestValidation/`. Repeated tests are not additive
unique coverage. Graphics-enabled cases enter the actual Game scene. Editor audio
is muted; player audio preferences are preserved.

| Scope | Result | XML |
| --- | --- | --- |
| Status/resource/continuation checks | 38/38 | `d68008c9-6c36-4714-bf0b-ae8aaca5dd2e.xml` |
| Legacy Fear and complete-action save/continue | 2/2 | `79c1bdb3-9dbe-446d-993f-36dd2df0fded.xml` |
| Status asset and generic cast cases | 2 passed; separate layout failure corrected below | `6df3a9ba-5f29-456e-b051-58f99eb9fb4d.xml` |
| Seven-status panel: three portrait sizes + safe inset | 1/1 | `9534cc12-9e10-4b33-84d6-07b97d4822c7.xml` |
| Slippery/board/continuation regression | 58/58 | `606527d1-20c1-40dc-81ec-a854be2e10bc.xml` |
| Culture assets, flood and Queen continuation | 4/4 | `a3cd72f7-e4ef-4d73-b291-b425cf4689b2.xml` |
| Nine dungeon cast kinds, readiness and canceled casts | 1 scenario passed | `cf064bf2-fc6f-41e9-a160-601df2d529a8.xml` |
| Court cast pause/fade/continue without replay | 1/1 | `d8d11246-a5b0-4a85-afeb-46b9a2f14f1a.xml` |
| Slippery final cells and sigil lifecycle/portrait cases | 13/13 | `e258a170-bfcc-4d43-95d4-60bf48e08d20.xml` |
| Sigil overlay correction: lifecycle and all portraits | 4/4 | `74589947-3719-4ebc-a72f-a12e43e4a6cc.xml` |

The focused pass found and corrected a three-pixel status-row overflow, a paused
Slippery rule-check deadlock, and fixed-cell response prediction that still assumed
a two-piece swap. Screenshot inspection caught row/column marks hidden by the
board frame despite correct world positions; the overlay correction made them
visible. An invalid new test metadata GUID was corrected and the skipped Court
case was then actually executed. Initial headless live tests could not obtain a
portrait viewport; graphics-enabled runs supply the live evidence above.

## Broader regression and validator

Broad affected regression **passed 335/335, with no failed or skipped tests**:
`.utmp/ForestValidation/a47a252f-85c8-472b-a443-b3fbae0a372e.xml`.
It covers status arithmetic/lifecycles, board input/crystals/rewards, all forest and
Court live fixtures, culture data, cast/sigil checks, King/royal regressions,
continuation, and the eighteen-handoff three-zone soak. These are executed cases,
not 335 independent gameplay features.

Review after that run added forecast color/special freshness guards. The affected
rerun passed **18/18** in `d5528f07-eb52-487f-9b73-54ea13ce727d.xml`, including two
new freshness cases. Visual review then removed the extra neutral target overlay,
which obscured a gem. Final targets use only slot glyphs, leaving the center clear;
existing actor countdowns and lane flashes communicate timing. Its final focused
rerun passed **6/6** in `534ed03f-b278-481c-8277-ae7175e84631.xml`, including a
screenshot pixel assertion at every required viewport. The three runs contain
**337 unique passing test cases**; repeated cases are counted once. The packaged
`ArtSource/SharedStatus/Review/Evidence.json` records each case's latest result.
The mandatory `Tools/Validate-Unity.ps1` passed after these final changes using
Unity **6000.3.19f1**. Log:
`C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-86545328-c3ff-4a7c-9735-a73631b43591.log`.

No repository-wide green claim: the earlier production baseline's 34 lifecycle/
audio failures remain documented in [the Court record](DROWNED_COURT.md). These
checks do not establish physical Android behavior, human pacing, or listening approval.

## Art and visual evidence

- `ArtSource/SharedStatus/Review/Review.html`: portable gallery embedding all six
  final Unity captures and native status/sigil sheets. Adjacent `Evidence/` keeps
  the three complete XML records and portrait layout reports; `Evidence.json`
  records capture dimensions/hashes and the deduplicated case results.
- `ArtSource/DrownedCourt/TailRevision/Review/Review.html`: offline before/after,
  native cast comparisons and every revised motion.
- `ArtSource/DrownedCourt/TailRevision/TechnicalChecks.json`: individual source
  dimensions, alpha, palette and hashes. `ValidateTailRevision.py` passed for
  16 source PNGs and 15 matching Unity exports; all 118 frames retain old body pixels.
- `ArtSource/SharedStatus/NativeIcons.png` and `NativeSigils.png`: native glyphs.
- `.utmp/StatusRevision/Visual/sigils-*.png`: 720×1280, 1080×1920,
  1080×2400 and 1080×1920 with safe inset. Actual status/shield UI and simultaneous
  gem/cell/lane sigils. The fixture forces overlap; normal fair selection avoids it.
- `.utmp/StatusRevision/Visual/slippery-preview.png`: actual deterministic forecast.
- `.utmp/StatusRevision/Visual/cast-announcement.png`: committed Siphon label.

## Documentation ownership

`PROJECT_STATE.md` summarizes the implemented revision. `GAME_DESIGN.md` and
`ARCHITECTURE.md` link the canonical contracts and system owners. `STATUS_EFFECTS.md`
and `CASTER_PRESENTATION.md` specify shared runtime semantics. `DROWNED_COURT.md`,
`DROWNED_COURT_ENCOUNTERS.md` and `ArtDirection/Drowned_Court_Reference_Register.md`
record cultures, recipes and art status. `ZONE_DESIGN_RULES.md` contains the overlap
audit and lore checklist; `FUTURE_ZONE_LOCKS.md` keeps future concepts separate.
The checkpoint and this report provide resumption, change and validation records.

## Provisional values and review decisions

All seven initial durations are **three accepted moves**. Weakened and source-bound
Fear multiply damage by **0.75**; Sapped generation and Wounded healing by **0.5**;
Frostbite incoming damage by **1.25**; Burn deals **5 per accepted move**, with an
explicit extinguish hook. Existing final combat rounding remains in place.
Same-status reapplication uses the stronger value and longer remaining duration,
not repeated multiplication. Details: [STATUS_EFFECTS.md](../STATUS_EFFECTS.md).

Status balance, mixed-formation frequency and new Court escort pressure need human
play. The two mixed templates are not a measured probability. No new status caster
is assigned in production; a future Reef Clan boss is a possible Slippery source,
subject to a separate kit decision. Tail additions await user visual review.
Needlefin also has weak tail readability, but its silhouette is intentionally
unchanged because only Hammerhead/Captain edits were authorized.

Current warnings cancel on caster death. Stagger behavior stays mechanic-specific:
forest/Court interrupted channels clear their marks, while delayed dungeon warnings
remain marked. Persistent completed board objects are not treated as active casts.

## Safety and scope

The four pre-existing modified files (Court throne prefab and three consumable
sprite metadata files) retain their starting SHA-256 hashes and are excluded from
the branch commits. No full Court importer was run. No music changes, paid art,
credit purchases, model reset, future-zone implementation or merge occurred.
