# Playable still revisions

Status: candidates pending user approval, 2026-10-11. No animation or Unity
replacement is authorized by the user's request to revise these stills.

The user liked the first designs and requested a kind, less angry Minotaur
with keys on his belt, and a more distinctive Kobold. The exact previous
PNGs remain preserved in `../review/`.

## Direction

- Minotaur: relaxed brows, gentle muzzle expression and a ring of brass keys.
  His bulky frame, horns, fur, apron, stance and native drawing size remain.
- Kobold A: asymmetric teal coat, cream collar and a crossbody pouch. Its first
  output lost the revolver, so a separate correction restores it before review.
- Kobold B: short teal capelet, oversized brass clasp and a compact belt pouch.
  Both proposals retain the eye wrap, listening gesture, reptile anatomy,
  curled tail and single revolver. Neither introduces a cowboy hat.

## Provenance and review

The canonical art guide/register and current individual golden images were
read and inspected again. `verified-production-hashes.json` confirms the
current bound reference sources. The original lineage is in
`../input-manifest.json` and the first generation requests, which supplied
actual Rattlebones and supporting image inputs.

These edits use the exact previous target PNG as PixelLab Pixen's supported
image input. That endpoint does not offer a separate style-reference input.
`requests.json`, `receipts.json`, `cleanup.json` and the completed result
records preserve the prompts, image URLs, seeds, jobs and charges. Cleanup
reuses each character's previous palette with no dithering.

Individual PNG measurements are in `technical-checks.json`. The review boards
show 1x pixels, nearest-neighbour 3x inspection and light/dark backgrounds.
Only transparent margins are cropped for review layout and common-floor
alignment; the source PNGs retain their native canvas and pixel scale.

No golden-reference approval, animation approval or game integration is
implied by this source-art revision.

## Final proposed files and checks

| Candidate | Native canvas | Occupied pixels | Opaque colors |
|---|---|---|---|
| `minotaur-final.png` | 80 x 80 | 62 x 65 | 13 |
| `kobold-a-final.png` | 64 x 64 | 49 x 59 | 15 |
| `kobold-b-final.png` | 64 x 64 | 51 x 59 | 15 |

The `-final` suffix identifies the cleaned review output, not user approval.
All three have binary alpha and reuse their original candidate palette,
including the canonical #0A0D11 outline. The Minotaur's occupied bounds are
unchanged and exceed the comparison orcs (56 x 57 and 55 x 56). No sprites
were resized. Technical checks do not replace the user's visual assessment.

Recommendation: Kobold B. Its capelet and large clasp give a clearer silhouette
and retain a more readable revolver. A remains an alternative for review.
The first A, which lost the gun, is a superseded intermediate, not a proposal
for production. Raw generated and palette-cleaned files are both retained.

## Actual generation usage

Four Pixen edits at 1 generation each and three palette operations at 0.1
each: **4.3 subscription generations** for this revision. Exact account
display before: 198 remaining / 1801 used; after: 194 remaining / 1806 used.
The service rounds these account fields to integers; per-job charges provide
the fractional total. Credits remain $0; no purchase occurred. The complete
playable still work now totals **29.5 reported generations** (25.2 + 4.3).
Every job is complete; no animation job has been requested.
