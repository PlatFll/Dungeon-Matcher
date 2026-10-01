# Gideon Glass — production checkpoint

Status: **All nine implementation/review phases and the final defect sweep passed using the user's definitive design.** Final review, executed tests and the direct-Edit-Mode runner limitation are in [the validation record](../../Docs/Validation/GIDEON_GLASS.md). The earlier simplified sprite was rejected and its art gates reopened before gameplay work resumed. The current production files all use the selected second design.

## Definitive user design

The user explicitly selected the second supplied image: `References/Gideon_UserDesign.png` (unaltered copy). `Candidates/Gideon_Approved_03` is its 64×64 adaptation, copied to `Gideon_Ready`. It preserves the hat/feather, proportions, lens, brass face, split coat and cane. Native LibreSprite preparation normalizes alpha and the 16-color palette; manual pixel corrections recover the cane shaft, neutral feather shadow and square lens glint. Static review beside the exact working cast and on dark/light backgrounds passed: 1606 opaque pixels, one connected silhouette, bounds (8,3)–(56,64), no off-palette pixels, alpha 0/255. The new reference is the design authority; do not regenerate the superseded redraw.

`AnimateApproved.js` builds the revised idle directly from the approved source pixels. A neck gap found in the second frame was fixed by coordinating head/shoulder movement. All nine current frames have one connected silhouette, fixed feet/cane, native/export pixel agreement and a pixel-identical loop seam. Played the actual 9×130 ms loop beside Rattlebones in LibreSprite after closing the cached earlier draft; reviewed all revised frames individually.

`AnimateApprovedCast.js` then rebuilt the 850 ms cast, single-frame hold and 370 ms recovery from these same pixels. Audited all 15 frames and both transition boundaries; no equipment/foot drift or geometry tears. The 40 ms flash is local to the lens; steam occurs only once. Played the complete revised flow in LibreSprite, including the dedicated hold and release. The displayed review counter simulates turns and is not gameplay evidence. `Superseded` preserves the prior native drafts.

## Authority and inspection

Read the user's complete gated brief, current global/project instructions, `Docs/PROJECT_STATE.md`, art guide v1.9, and relevant design, architecture, pacing and balance sections before editing. Inspected the actual reference board, all four current cast sheets, current Rattlebones GIF playback in LibreSprite, JSON durations, Unity clip and player assignment. Current Rattlebones uses nine 64×64 frames, 130 ms each, with planted feet and restrained head/cape settling.

Inspected PlayerDefinition, PlayerAbilityController, IPlayerAbilityRuntime, board swap completion, typed continuation snapshots, GameplayRandom/SavedRandom and card definitions. Older architecture prose about Unity RNG is superseded by current continuation code. Current gameplay randomness is shared between board and enemies, so simply restoring that shared state would violate the requested RNG scope.

Base: main matched freshly fetched origin/main. Branch: `codex/gideon-glass`. Existing AGENTS.md, background sources and Dungeon_Default prefab edits are user work and must stay out of this feature's commits.

## Checklist

1. Static: 64×64, binary alpha, right-facing, broad connected material clusters, simple brass face and blue camera-eye, large theatrical hat, no chest camera/straps, connected thin cane, equivalent cast scale. Review native/enlarged on dark/light backgrounds beside exact cast.
2. Idle: grounded mechanical settling, coherent head/hat/grip, fixed feet/cane contact. Compare actual playback to Rattlebones; check every frame and seam.
3. Cast/hold/recovery: focus, shutter snap, short local flash, optional restrained steam, dedicated held pose, counter 5→0, rewind/recovery into idle. Review complete animation flow before gameplay work.
4. Runtime: generic board capture/restore operation; dedicated board/refill SavedRandom state; five accepted manual turns; restore only after complete settlement. Preserve combat history, energy, rewards, statuses, deaths and enemy cadence. Restore without emitting clear/reward events.
5. Edges: specials, deep cascades, deterministic reshuffles, dead obstacle owners, wave completion, player death, pause, cancellation, character/run changes, recast, invalid snapshots. Active ability must serialize through existing continuation.
6. Cards: choose 2–4 coherent ability interactions after base ability works; use current stable-ID eligibility and Epic character-card convention.
7. Validation: targeted board/lifecycle/replay tests, existing regressions, required Unity validator, real gameplay and final integrated art review. Fix all identified defects before proceeding.
8. Review full relevant diff, commit only feature files, open PR, stop before merge.

## Production files and reproduction

- `References/Gideon_UserDesign.png` is the unaltered user input. `Gideon_Ready.aseprite/.png` is the approved native drawing. The `.aseprite` files remain editable sources; PNG/JSON exports are Unity import inputs.
- `PrepareApprovedDesign.js` recreates the native candidate in LibreSprite. `AnimateApproved.js` and `AnimateApprovedCast.js` articulate its pixels. `BuildApprovedCast.mjs` regenerates the latter helper from the shared pose code. Earlier drawing/generation helpers and local superseded drafts are excluded from the production handoff.
- Run `Build.ps1 -Script AnimateApproved.js`, then `Build.ps1 -Script AnimateApprovedCast.js` through LibreSprite. `Retime.py` writes only authored ASE exposure metadata. `Export.ps1 -Names Gideon_Idle,Gideon_Cast,Gideon_Hold,Gideon_Recovery` exports native frames. The local LibreSprite executable path in the helpers may need adjusting on another machine.
- `ReviewArt.py`, `ReviewApproved.py`, `ReviewAnimation.py` and `ReviewFlow.py` inspect and display existing files; they do not redraw source cels. Reviews use nearest-neighbor enlargement. `ChronoShutter_Flow.gif` includes a simulated countdown and is only an animation review.
- `GideonGlassImporter.Run` imports production PNGs, verifies source dimensions/timing, authors clips, registers the player/ability and adds the two cards to the existing catalog. `Tools/Run-GideonValidation.ps1 -Mode Import` invokes it explicitly.
- `DrawLensIcon.js` draws the seven-color brass/blue lens emblem in LibreSprite. `DrawAbilityButton.js` places it in a copy of the game's native 176×64 ability-button frame and normalizes the old frame's near-opaque/faint alpha pixels. `Gideon_AbilityButton.aseprite/.png` supplies the production ability icon; the 64×64 lens source remains editable here.

The palette uses the project outline `#0A0D11`, muted brass, charcoal/plum cloth, burgundy accents, warm ivory and a small blue lens. All production pixels have alpha 0 or 255. Sources keep a fixed bottom contact and full 64×64 frame; animation clips change only `Image.m_Sprite`.
