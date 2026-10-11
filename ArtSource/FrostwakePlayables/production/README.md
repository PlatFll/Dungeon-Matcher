# Locked playable production

The user approved Minotaur's friendly expression, fully yellow eyes and belt
keys, and Kobold B's body/clothing, full blindfold and free hand behind his back.
Exact final stills and hashes are in `../locked/APPROVED_DESIGNS.json`. This latest
instruction explicitly authorizes the animation set; earlier pending-review
labels in the initial galleries are historical.

## Source and selection

- Canonical guide v1.13.2 and current golden-image hashes: `../input-manifest.json`.
  Original Pro generations actually received Rattlebones plus applicable identity
  and supporting image inputs. Edits and motion use that reference-conditioned
  approved identity, not a text-only style guess.
- Native stills: Minotaur 80×80, occupied 62×65, 13 opaque colors; Kobold 64×64,
  occupied 51×59, 15 colors. Both binary alpha. No drawn pixels were resized.
- Motion padding: Minotaur 96×96 at offset 8,8 (floor 16 pixels above bottom);
  Kobold 96×80 at offset 16,12 (floor 4). FullRect/64 PPU/Point/uncompressed.
- `../motion/requests.json` and receipts preserve actual input bytes, prompts,
  seeds and jobs. `raw/` keeps original outputs; `palette-locked/` is production.
  A first palette pass incorrectly included transparency's black RGB in the
  provider's palette. The second pass uses an opaque strip containing only the
  approved colors; the rejected intermediate `clean/` files remain provenance.
- Kobold idle generated frames 5–6 added an unrequested flame. Reject those
  frames. The selected first-five poses settle and return in reverse order.
  No pixels were erased or painted to hide the error.
- All 72 selected character cels pass exact palette membership, binary alpha,
  constant native canvas and occupied-edge clearance checks. The manifest records
  every selected source index and exposure. Idle cadence is 9×130 ms.
- Minotaur ability uses a 400 ms departure, board landing/travel/rebound and
  320 ms recovery. Kobold uses a 540 ms flourish and individually replayed firing
  poses; the flash begins at 120 ms, matching each board strike callback.
- Effects are separate PixelLab outputs: one 32×32 identity-based board avatar,
  five 48×32 heavy contact cels and five 32×32 spark cels. Their shared palettes
  and requests are under `../effects/`. Effects never own damage or board state.

`Tools/Export-PlayableMotion.py` only selects/packs provider cels and produces
review images; it does not draw, rescale, quantize or paint character art.
PNG frames + exact timing JSON remain the editable native sources.

## Audio and spending

Six original mono cues are synthesized by `Tools/Render-PlayableAudio.py`.
They use the existing combat bus, independent SFX volume, pause and replay
gating. They are temporary authored sounds for listening review; validation
keeps Unity muted. `audio.json` records format, duration, headroom and hashes.

Reported PixelLab charges for the complete task: **49.8 subscription generations**
(29.5 before the design lock; 20.3 for final edits, motion, effects and palette
corrections). One rate-limited Kobold death request was not charged; its retry
is recorded. Final balance read: 173 generations remaining, $0.00 credits.
No credit purchase. All generation jobs completed.

`Review.html` and GIFs show exact selected timing at 3× nearest-neighbor display.
Runtime layout, gameplay and continuation evidence is tracked in
`Docs/FrostwakePlayables/IMPLEMENTATION.md` separately from source-art checks.
