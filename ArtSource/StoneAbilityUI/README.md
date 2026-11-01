# Stone battle controls — review handoff

**Latest revision:** [matte carved stone and larger top controls](MaterialRevision/README.md).
The first material pass below is preserved as history; the user rejected its metallic read.

Implemented on `codex/stone-ability-controls`, stacked on the existing shared-UI
branch/PR #220. No merge. The expanded approved Ironvein cave remains bound in
the actual Game scene through the preceding integration work.

Open [the review gallery](Preview/Review.html) for native art, before/after Game
screenshots in all five zones, four character plates, ready motion, rapid shots,
the bull charge and the four-character menu.

## Delivered behavior

- One PixelLab stone supply tile, 40×40 square Settings button and solid
  176×66 ability slab across Dungeon, Forest, Court, Ironvein and Frostwake.
- Crowned skull, angry bull head, six-chamber revolver cylinder and flute
  engravings. Ready uses a fixed dark socket, a 1-pixel sideways / 3-pixel upward
  slab lift, and restrained white engraving glow. No source stretching.
- Guide is removed in every live/legacy UI creation path. Enemy tap inspection,
  Settings, potion/bomb callbacks and existing ability activation still work.
- Kobold: six authoritative impacts in 0.5 seconds from first to last at 60 Hz;
  90 energy spent once, no player move consumed. Normal per-tile flash/hold and
  post-burst wait are skipped only for this rapid ordered sequence. Damage and
  repeat-recipient passive, specials, locks, cancellation and final settlement
  retain their existing owners and semantics.
- Minotaur: original portrait leap and seven-step collision behavior; a small
  colored bull head moves over the board. The duplicate full-body landing marker
  is absent. Seven impacts, 2.1 seconds first-to-last, 90 energy, zero player moves.
- Gideon is hidden from new selection, with progression/assets and existing
  saved-run identity retained for a future return.

## Art lineage and native QA

95 subscription generations / 9 PixelLab requests; 4,683 generations remain.
No USD credit balance was spent. The first hollow plate and oversized bull set
were rejected; the replacement plate has a full stone face and the bull is
generated at 32×32 rather than resampled. See `QA/Generation.json`,
`QA/ReferenceLineage.json`, `QA/PixelLabUsage.json` and all raw candidates.

The current wave plate, actual Rattlebones and Minotaur imagery supplied the
reference inputs. The canonical guide/register hashes were refreshed. Cleaned
assets use native palette mapping, a chisel lip and separate ready artwork;
no regenerated character bodies or existing environment sprites were changed.

13 exports: plate/socket 176×66, supply 64×64, Settings 40×40, eight engraving
states 48×40 and colored Minotaur icon 40×40 (original 32×32 artwork padded).
All have binary alpha, point/uncompressed imports, 64 PPU and exact
Prepared → LibreSprite ASE → export → reopened ASE PNG equality. Opaque color
counts are 4–12. `Native/` retains editable ASEs; `Scripts/FinishStone.js` is the
actual LibreSprite finishing script. `QA/NativeValidation.json` records every
size, occupied bound, color count and SHA-256.

## Executed verification

- `Tools/Validate-Unity.ps1`: PASS, Unity 6000.3.19f1. Full log in QA.
- Actual Game: five zones × 720×1280, 1080×2340, 360×640 and 1080×2400 with
  a simulated safe area = 20 before/after cases. Board cells, feet, environment
  placements, protected UI/art remain exact; shared control scale, clearances,
  long wave label, Settings, inspection, potion, bomb cancel and ability use pass.
- Four character captures: correct definition-bound engraving, raised/white
  ready state, pause freeze, Reduced Motion static state, both ability effects
  and a real four-choice menu all pass (6 test cases).
- Five existing combat regressions pass: per-recipient damage/reset/spend,
  blocked activation and pause, durable-obstacle charge/card damage, safe Bifrost
  and special chaining, cancellation/one settlement.
- Initial failures caught the raised energy bar touching the compact board;
  its original position was restored. The new recorder also initially resumed
  before its player fixture existed; moving play-mode entry to UnitySetUp fixed
  it. Historical XML receipts retain those failures rather than hiding them.
  Final affected tests pass. Overall: 12 final targeted tests / 20 layout cases,
  plus the separate passing before-capture test and required compilation gate.
- All nine protected starting files were restored byte-for-byte after Unity;
  six were already dirty when this work began and are excluded from the commit.

Screenshots and GIFs come from automated Unity play mode, not a physical phone.
No Android build or real-device test was performed for this change. The existing
PixelPerfectCamera warning at 360×640 remains; the layout validator and native
shared-control checks pass at that size. Exact PNGs are the visual evidence;
GIFs are palette-limited review encodings. Full PNG animation frames remain
locally beside them and are excluded from Git to avoid unnecessary bulk.
