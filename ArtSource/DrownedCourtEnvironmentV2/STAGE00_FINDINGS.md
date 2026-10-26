# Drowned Court V2 — Stage 00 preflight

Status: **COMPLETE, awaiting Stage 01 approval.** No new underwater art generated. No production binding changed. No merge.

## Branch and scope

- Branch: `feature/drowned-court-pixellab-environment-v2` in a fresh managed worktree.
- Base: main `00e9ebe9d074ecc7cf5e5989286b976f644c8436` (includes merged Dungeon architecture/torches).
- Current art guide/register and Court gameplay/reference documents inspected. Current Rattlebones, Farmer and Pan PNG hashes verified, actual pixels inspected. All 4 supplied references and 9 crops reviewed.
- Mockup supplies architecture, depth, teal stone and attached marine growth; its characters, gems and text are not production source art. Rattlebones establishes rendering discipline, not an underwater palette or anatomy template.
- Main's existing local prefab/metadata/recovery-file edits remain untouched in the original checkout.

## Verified tool readiness and budget

PixelLab read-only balance/capability calls succeeded. Active Pixel Artisan tier: **642.20 remaining / 3100**, 2457.80 used; cash balance $0; reset 2026-10-29. Subscription capacity is available despite the cash balance. **Stage 00 spend: 0; uploads: 0; generation jobs: 0.**

Actual advertised tools, not inferred endpoint names:

| Tool | Relevant reference support and size | Advertised/provisional cost |
|---|---|---|
| `create_image_pro` | `style_image_url`/`style_image_base64`, `style_copy`, up to four labeled `reference_images`; square max 512, wide max 688x384 | 10–25 equivalents per call; 128x128 listing offers four candidates for 10 |
| Pro Flash create/edit | Style input supported; create 16–256 dimensions in steps of 4; edit/inpaint 32–256, one reference, no canvas growth | Read-only 128x128 create quote: 6; quote again before spending |
| `create_sidescroller_tileset` | 16/32 square tiles, lower/transition descriptions, base-tile seam input; **no style-image field** | 2–3 |
| `create_image_pixen` | 16–768 multiples of 4 within area limits; transparency area <=65,536; **no golden-image style field** | 1, for secondary ideation only |
| `edit_image` | Existing input plus instruction/reference; max 512x512 | 10–25; prefer zero-credit native edits for precise cleanup |
| `animate_image` | Approved first-frame image, optional last frame; even 4–16 frames, max 256x256 subject to total area | Example 64x64 x8 = 1, 128x128 x16 = 4 |

Raw schema and service receipts: `QA/ToolEvidence.json`. Tool availability/cost can change. A filename or prompt saying Rattlebones is not a reference-image input. Upload approval and actual hashes must be recorded when generation is authorized; Stage 00 sends nothing.

LibreSprite **1.1-dev** successfully saved a disposable copy to `.ase`, reopened and exported it, and produced **identical RGBA pixels and dimensions**. Native batch workflow is verified. Interactive LibreSprite MCP bridge/editor state was not tested and is not claimed connected. Initial version-only invocation could not write sandboxed preferences; the subsequent correctly sequenced native batch run succeeded. Evidence: `QA/LibreSpriteCheck.json`.

Planning envelope across all future art stages: **73–154 equivalents**, not spending authorization. See `Progress/FamilyPlan.csv` for every family, dimensions, reuse and cleanup route. Run one representative asset before expanding each family; re-quote and record actual billing. Assembly/QA should need no paid generation by default.

## Current ownership and reuse

Court is already playable. `Docs/DROWNED_COURT.md` and `Resources/Zones/drowned-court.asset` own its 14-enemy roster, encounters, flooding and AIR. These are protected, not part of a new gameplay implementation.

- `BattlePresentationSkin` maps Court to `Court`; its active scenery comes from `Resources/UI/BattlePresentation/ReferenceCourt`. `DungeonPresentationArt.BuildReferenceKit` creates the existing scenery, including its historical floor crop. Court currently does **not** select `DungeonPixelLabBattleArt.Current`; Dungeon and Forest select their modular resource manifests.
- Existing `Presentation_Court`/Court zone-theme and Reefgate/Cloister/Throne prefab resources remain intact. Do not run an old importer or silently overwrite them. The source kit's older candidate labels are historical: the canonical register records the later 2026-10-17 presentation approval.
- Keep all current Court characters/animation/VFX, six gem drawings, cell art, HP/shield/AIR UI, shared controls, save/resume and combat logic. Court's `DC_M02_Marine_Cell` historical candidate does not authorize changing today's shared cells.
- Reuse shared controls and code: `GameplayPixelLayoutController` for sections/safe area/board/portrait; `BoardLayoutController` for the 64px-cell footprint; `TopBattleLayoutController` and `TopBattlePresentationController` for actors and `TopHUD/BattleFloorAnchor`; `ResponsiveModularFrameFitter` for corners/rails. No second runtime owner was added.
- Existing Court marine props/materials are a reuse pool, not automatic wholesale replacement or a new golden set. `QA/CourtAssetInventory.json` records exact current dimensions, alpha/colors and hashes; `Preview/Existing-Court-Native.png` shows representative bound sources at 1x.

## Actual Unity baseline findings

Six actual **Game scene, play-mode** captures: Court, Dungeon, Forest at **720x1280 and 720x1600**, with Rattlebones, three test-fixture enemies, live gem board, HP and shared HUD. Disposable accounts and temporary character selection isolate user saves. These are desktop Unity captures, not real-device tests or an Android build.

- All six existing layout-validator reports returned **zero errors**. Cross-zone board and portrait dimensions are identical at each size.
- At 720x1280 all three playable grids measure **545.099 x 545.099 screen pixels**; portraits **195 x 406**. At 720x1600 the board/portrait retain those dimensions while regions reposition.
- The existing board source footprint is 512x512 (8 x 64), displayed at the intentional shared **1.064646** fractional ratio at this size. Keep that established common board size. Do not shrink gems or alter rules to fit new art.
- **Visual defect recorded, not fixed during setup:** Court far scenery is currently ~1.515x horizontally / 1.669x vertically; its frame and side art also stretch nonuniformly. Passing the existing validator does not certify native artwork fidelity. New Court art should use uniform native scale, tiling/crops and separate modules through the existing presentation architecture.
- Court has **no explicit portrait RectMask2D** in the current top-HUD route; Dungeon/Forest do. Later Court integration needs the existing overfilled, clipped-recess technique while retaining the shared portrait dimensions.
- Floor anchors: Court screen y=865 and Dungeon y=864 at 720x1280. Preserve the live floor owner and check actual soles/HP clearance when adding a flat modular platform. Do not position actors from a painted mockup row.
- Safe-area authority is identified; full hardware notch/Android/device validation is deferred to optional Stage 12. The baseline intentionally does not change current scale or mask behavior.

## Blank art-only scaffold

`Assets/_Game/Scenes/ArtPreview_DrownedCourtV2.unity` contains a **720x1280 screen-pixel scaffold**, copied references to existing shared UI graphics, plain upper/lower/portrait placeholders, a clipped overfilled blank portrait recess, and an 8x8 guide over the measured playable footprint. The guide is not replacement cell art. No actors/gems or mockup raster are baked into any background. No board/combat owner exists in the scene. It is **excluded from build settings**.

The scene preserves baseline control positions; it is a Stage 00 staging aid, not new art and not final visual approval. Live UI remains driven by existing owners in `Game.unity`. `Scaffold-Native.png` is an actual Unity 1x render; `Scaffold-2x.png` is an integer 2x Unity render. Native reference contact sheets and nearest-neighbor inspection are included separately.

## Preservation and checks

- **7,839 existing Assets/ProjectSettings files verified byte-identical** after removing incidental Unity serialization-only changes in the new worktree. GUIDs included. See `QA/ProtectedBefore.json` and `QA/ProtectedAfter.json`.
- Required `Tools/Validate-Unity.ps1`: succeeded, Unity6000.3.19f1; final receipt in `QA/UnityValidation.log`.
- Explicit Unity preflight test: **1 passed**, six captures, cross-zone dimension assertions, scaffold build/exclusion checks. `QA/PreflightTests.xml`; logs retained. Warning about the scene generated by the test is expected: that scene is the required Stage 00 deliverable, deliberately retained.
- LibreSprite exact RGBA round-trip passed. No production art import, regeneration, recolor, or gameplay edit.

Next allowed action only after user approval: Stage 01 deep-ocean background layers. All Stage01–12 entries remain NOT_STARTED. No merge.
