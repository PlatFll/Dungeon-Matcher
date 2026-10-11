# Frostwake eye and Skald color review — 11 October 2026

**Status: all final corrections approved; imported and validated for merge.**
The user explicitly approved every final correction and asked to add them to
the game, validate and merge. Seven Unity stills and 43 sheets now use the
approved pixels. No gameplay has changed. The base is merged
main `c2869ddc09f6b7a021b7dbd492ba683303a651b1`; the isolated branch is
`codex/frostwake-eye-color-review`.

Open [FaceReview.html](../FaceReview.html) for native 1× stills, 3× full-sprite
views, 8× face details and synchronized before/after motion with frame stepping.
[BeforeAfter.png](BeforeAfter.png) is the static overview. Image enlargements use
nearest-neighbor and are inspection artifacts, never native import sources.

## Requested changes

- **Jarl Haldra:** one black eye fill, `#101517`, within her existing dark
  eye contours. Closed-eye reactions stay closed.
- **Rime Skald:** warm light beige skin (`#DFA881`, with `#B58A6E` / `#9E694F` hand
  shadows), gray hair and beard (`#464C53`, `#737980`, `#969BA1`), solid dark eyes.
  His cream cap, horn, clothing and original pose geometry are retained.
- **Iceborn Raider, Frostblade Reaver and Hearthkeeper:** remove pale sclera/glint
  pixels so eyes read as a single existing dark color, including motion poses.
- **Frostfang Wolf:** unify its iris to existing ice blue `#27C6F2`; preserve fur
  around the eye and the existing socket outline.
- **Iceborn Champion:** the first pass missed white/gray eye pixels below his
  helmet. Sixty native pixels across 38 poses now use the existing black
  `#121314`. The seven closed-eye death poses stay unchanged.
- **Glacier Troll, Harpoon Hunter, Rimecarver and Shieldmaiden:** retained under
  the user's approval. Their source pixels remain unchanged.

`Revision02.png` compares the prior review with the three latest corrections.
Skald retains his gray hair and beard, and his skin now matches Haldra's existing
warm human skin color. Review 1's ivory face and silver eyes remain in `review-01/`
as superseded evidence. Raider, Reaver, Hearthkeeper and Wolf are approved and
byte-identical to their approved review. `approval-status.json` records the
latest all-character approval and explicit merge authorization.

The latest user instruction supersedes permissive historical eye-highlight
wording for these characters. The final replacements were separately approved.
Canonical guide v1.15 records that decision and preserves the prior history.

## Exact source and provider evidence

The current Rattlebones, Farmer and Pan Villager images were opened and their
production/extraction hashes verified against guide/register v1.14.
`reference-hashes.json` records their exact hashes, along with the guide/register.
No reference drawing changed. These are rendering references; the current bound
Frostwake stills and selected source cels remain the identity authority.

`sources.json` identifies all 417 inspected still/source-cel inputs and hashes.
The eleven `*-source.png` files are lossless native packing, with no rescaling.
PixelLab `pixelart_workbench edit` received their actual PNG bytes as data URLs
and the guarded assignments in `*-decisions.json`. It performed every sprite
color edit. Local Python only inspected, proposed guards, packed/unpacked native
pixels, verified provider outputs and composed labeled reviews.

`selected-receipts.json` owns the seven selected provider jobs. `provider/` stores
the downloaded outputs; `receipts.json` retains the first dispatch responses.
Two non-producing requests were rejected by tool bounds (1024-pixel side and
10,000 assignments per operation); repacking/splitting preserved every source
pixel. All corrections and rejected requests cost **0 paid generations**.
Balance was 26 remaining / 1973 used before and after; no credits purchased.

Earlier visual refinements are not selected:
`a3ce992b-b38a-466a-8e84-028af6a93fe2` (Skald, overbroad death material region),
`c0cd3841-7fde-43cc-a264-ed6c46ca4698` (Hearthkeeper, missed cast eye pixels),
`09d67907-8a14-41ec-968e-fb52374b251e` and
`9c83f046-46ad-4360-b25f-6a83781d6a19` (wolf inspection iterations).
Selected decisions use each original packed source, not accumulated drift.

## Verification and integration

`verification.json` records all changed-pixel checks and the 43 exported clips
across seven approved characters. Every cel remains 128×104, with byte-identical alpha,
native registration and unchanged pixels outside the explicit guarded regions.
Clips retain source frame selections and durations. Palette counts are measured
from PNG pixels in `comparison-metadata.json`; new eye/skin/hair colors are
intentional material additions, not a claim that all candidates remain 15 colors.

Source poses and final faces were visually inspected, including open/closed eyes,
channel poses, hit/death reactions and Skald's hand/head material boundaries.
The review page exposes every selected state against immutable before sheets.
`integration.json` records the installed PNGs, updated native sources and
unchanged animation/metadata hashes. The four unrelated user edits are preserved.

`Tools/Check-FrostwakeAssets.py` passes all 70 sheets and 501 slices. Unity's
required validator passed, as did five existing runtime/presentation checks
with Editor audio muted. See `Docs/Validation/FROSTWAKE_FACE_CORRECTIONS.md`.
There are no gameplay, controller, timing, event or GUID changes. Historical
source filenames in `sources.json` describe the pre-edit baseline, preserved
losslessly in the packed originals and in main commit c2869ddc. Selected native
palette cels and approved stills now carry the final colors for future exports.
