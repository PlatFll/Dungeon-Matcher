# Frostwake eye and Skald color review — 11 October 2026

**Status: candidates awaiting the user's character-by-character visual approval.**
No Unity sprites have been replaced, no gameplay has changed, and nothing from
this pass may be merged until the user selects the candidates. The base is merged
main `c2869ddc09f6b7a021b7dbd492ba683303a651b1`; the isolated branch is
`codex/frostwake-eye-color-review`.

Open [FaceReview.html](../FaceReview.html) for native 1× stills, 3× full-sprite
views, 8× face details and synchronized before/after motion with frame stepping.
[BeforeAfter.png](BeforeAfter.png) is the static overview. Image enlargements use
nearest-neighbor and are inspection artifacts, never native import sources.

## Requested changes

- **Jarl Haldra:** one silver-gray eye fill, `#BFC6CC`, within her existing dark
  eye contours. Closed-eye reactions stay closed.
- **Rime Skald:** pale ivory skin (`#E8DDD0`, with `#BDAE9E` / `#938579` hand
  shadows), gray hair and beard (`#464C53`, `#737980`, `#969BA1`), solid dark eyes.
  His cream cap, horn, clothing and original pose geometry are retained.
- **Iceborn Raider, Frostblade Reaver and Hearthkeeper:** remove pale sclera/glint
  pixels so eyes read as a single existing dark color, including motion poses.
- **Frostfang Wolf:** unify its iris to existing ice blue `#27C6F2`; preserve fur
  around the eye and the existing socket outline.
- **Glacier Troll, Harpoon Hunter, Iceborn Champion, Rimecarver and Shieldmaiden:**
  all source poses inspected; existing dark eye designs retained without edits.

The latest user instruction supersedes permissive historical eye-highlight
wording for these candidates. Approval of the previous eleven still identities
does not approve these replacements. The canonical guide and golden register
are unchanged until the user selects a replacement.

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

`selected-receipts.json` owns the six selected provider jobs. `provider/` stores
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

## Verification and next action

`verification.json` records all changed-pixel checks and the 35 exported clips
across six candidates. Every cel remains 128×104, with byte-identical alpha,
native registration and unchanged pixels outside the explicit guarded regions.
Clips retain source frame selections and durations. Palette counts are measured
from PNG pixels in `comparison-metadata.json`; new eye/skin/hair colors are
intentional material additions, not a claim that all candidates remain 15 colors.

Source poses and final faces were visually inspected, including open/closed eyes,
channel poses, hit/death reactions and Skald's hand/head material boundaries.
The review page exposes every selected state for human approval. This art-source
review does not claim a new Unity runtime test; runtime and serialized assets
were not changed. The four pre-existing unrelated working changes are preserved.

After the user chooses characters: copy only approved still/sheet candidates
into their existing Unity PNG paths while preserving all `.meta` GUIDs, record
the approved material change in the canonical guide/source manifests, validate
affected import/animation bindings, review the diff and open the implementation
PR. Merge only the characters the user explicitly approves.
