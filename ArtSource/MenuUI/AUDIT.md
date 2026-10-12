# Menu and settings presentation audit

## Authority and scope

- Baseline: main `b3e1edd68d8423ef8dd9defaa8cd1ed160f2aeb9`, fetched 2026-10-12.
- User confirmed current canonical guide v1.15 and golden register because the
  pack's `(2)` files were absent. Supplied mockups are preserved byte-for-byte in
  `references/`, with source hashes and native dimensions in `lineage.json`.
- Rattlebones reference hash matches the current register. Its approved production
  sprite and idle remain the character source; no character regeneration.
- Existing throne prefab and three consumable importer edits were present before
  work. Their hashes are recorded separately and they are excluded from this pass.

## Existing owners

`MainMenu.unity` has a 480x854 width-scaled canvas and serialized Play/Mastery
controls. `MainMenuController.ConfigureHome` adds the other navigation controls.
`DungeonPresentationArt` installs the earlier doorway and logo. `RunControlsUI`
owns pause, board-input leases, settings, suspension and confirmed run exits.
`GameUi`, `FinalizedUiSkin`, `PixelTextFitter` and Thaleah provide existing styling.
The main-menu gear is new presentation access to the same preference owners.

## Plan

1. Capture production before screens using isolated account data and muted Unity.
2. Reconstruct scenery with the actual mockup as PixelLab image input. Author
   separate native controls, icons and title in LibreSprite. Verify PNG exports.
3. Attach native menu stages to existing navigation and modal owners. Preserve
   existing callbacks, save confirmations, zone testing picker and combat rules.
4. Validate actual screen rendering, native scale, text fit and functional paths
   at short/tall portrait sizes and safe insets. Review screenshots against targets.
5. Document reusable global menu assets and known deviations, run the mandatory
   validator, review the full scoped diff, and open a PR. No merge authorized.

## Production limits

PixelLab had 26 subscription generations and $0 credit. One Pro scene at 360x640
used 25, leaving one; no purchases or second paid request. Its prompt, exact image
reference and receipt are preserved in `generation.json`. LibreSprite cel scripts
author controls and finish the hall; original generated pixels remain in candidates.
All labels other than the brand wordmark remain live Thaleah text.

## Outcome

The baseline Unity graphics test captured both menus at 1080x1920 and 1080x2400.
Implementation and technical native-asset checks are complete. Six targeted
Unity graphics tests passed, covering layout, navigation, pause, continuation
and settings/recovery through crystal travel. The final home-modal check also
verifies coverage outside the serialized inset HomeScreen. Current screenshots,
exact validation records and known differences live in [REVIEW.md](REVIEW.md).
