# Matte carved-stone UI — review revision

The user rejected the first controls' metallic appearance and requested larger
wave and Settings controls. The shared battle family now uses broad matte gray
stone planes, chipped contours, blunt chamfers and restrained cracks. Rivets,
thin polished rims and purple metal highlights are removed from these controls.
This revision is on `codex/stone-ability-controls`, PR #221; no merge.

[Open the before/after Game gallery](Preview/Review.html).

## Scope and dimensions

- Wave: new 176×48 native source, up from the 144×34 presentation. Runtime width
  is 171–176 in the tested profiles; protected 20/14/20/14-pixel corners and
  native center repeats fit the compact layout without distorting stone pixels.
  Live Thaleah text has a larger integer glyph size; WAVE 99999 also fits.
- Settings: 56×56 square source/control, up from 40×40. Its carved gear remains
  square and uses the same native scale as the other shared controls.
- Supply: 64×64. Ability plate/socket: 176×66. Four engravings and ready states:
  48×40, with exactly preserved alpha masks and cluster shapes; only their
  material ramp is matched to the new stone. Ready lift/glow remains unchanged.
- Board, portrait, enemies' floor positions, health UI, gems, zone artwork,
  colored bull board icon and all ability/gameplay logic are unchanged.

## PixelLab and LibreSprite

Four Pro jobs cost **40 subscription generations**, balance **4,683 → 4,643**.
USD balance remains $0.00. This is separate from the earlier UI pass's 95 uses.
Actual current Rattlebones and prior wave images were supplied through supported
style/reference inputs. `QA/Generation.json` retains prompts, input roles, jobs
and cost receipts; `QA/ReferenceLineage.json` records the exact guide/reference
hashes used before the subsequent scoped direction note was appended.

Selections: Ability 0, Wave 0, Supply 6, Settings 3. All raw candidates remain.
The broad quiet central rows/columns are repeated at native resolution to fill
the target UI bounds while retaining generated corner shapes. The wave's
transparent interior is filled with opaque charcoal. No resampling or blur.
`QA/Cleanup.json` records native edits and exact palette maps.

Thirteen revised PNGs have 4–9 meaningful opaque colors and binary alpha.
Every prepared PNG equals the LibreSprite export and reopened ASE export exactly.
Editable ASEs, exports, scripts and occupied bounds/hashes remain in this folder.
The colored Minotaur icon, original wave and original reference images are exact.
Existing Unity GUIDs are retained; the new wave has its own GUID. Point filtering,
64 PPU, FullRect and uncompressed/unmipped import settings are preserved.

## Verification actually performed

- Mandatory `Tools/Validate-Unity.ps1`: PASS, Unity 6000.3.19f1.
- Actual Game play-mode matrix: PASS, five zones × four profiles (20 cases):
  720×1280, 1080×2340, 360×640, and 1080×2400 with simulated safe area
  (24,96,1032,2184). Protected art/layout, board cells, feet, native control
  scale, control clearances, live long labels and actual callbacks are checked.
- Six character/menu cases PASS: four bound engravings, lift/white pulse,
  pause/Reduced Motion, rapid Kobold sequence, Minotaur icon and selection.
  This material revision changes no ability logic; the earlier five combat
  regression results remain recorded in the parent handoff, not counted as rerun.
- Initial layout test caught a 167px compact wave. The runtime gap next to
  Settings was reduced from 12px to 8px, yielding 171px. The strict >=170px test
  was retained. The full 20-case matrix then passed. Both receipts are retained.
- Nine protected starting files restored byte-for-byte after all Unity runs.
  Six were already dirty before this revision and are excluded from the commit.

Before PNGs are exact copies of the preceding commit's tested Game captures.
After PNGs are fresh automated Unity play-mode captures. GIFs encode actual
captured frames with a limited review palette; exact PNG frames remain locally.
No Android build or physical-device test was performed. The pre-existing
PixelPerfectCamera warning and compact health-label behavior at 360×640 remain
visible in the screenshots; this change does not alter their owners.

Art verdict: the chips, thicker edges and quieter gray planes read as carved
stone rather than hardware. The engraving silhouettes and white ready cue stay
readable. User visual approval of this revision is still pending.
