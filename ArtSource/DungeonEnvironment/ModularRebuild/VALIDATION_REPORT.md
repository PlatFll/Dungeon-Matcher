# Dungeon modular rebuild — validation and visual review

**Ready for user review. Not merged; not approved for production replacement.**

129 separately editable LibreSprite modules and native PNGs: 4 floor variants,
12 wall variants, 9 board-frame parts, 19 portrait/empty-health-frame parts,
48 HUD parts, 31 props/architectural parts, and 6 background/recess assets.

## Executed checks

- All 129 native PNGs match independently decoded LibreSprite cels exactly.
- All alpha is binary (0/255); floor/wall materials are fully opaque 64×64 tiles.
- Floor: all 16 horizontal and 16 vertical neighbor pairs pass. Each of the
  three wall families passes the same 32 edge pairings. Rail repeat edges pass.
- New Unity imports use Point filtering, 64 PPU, uncompressed textures and no
  mipmaps. PNG files compare byte-for-byte with native exports.
- Two EditMode tests pass, including actual Unity camera captures at 720×1280,
  1080×1920, 1080×2400, and 1080×2400 with safe area (24,80,1032,2200).
- Rendered oversized green-fill probes produce changed pixels only inside the
  portrait and board interior masks in all four configurations.
- Every submitted module appears in the preview; fixed sprites stay on integer
  physical pixels. The board has 64 empty cells; editable plaque labels fit on
  one line. No characters, enemies, gems or gameplay owners are instantiated.
- Tests run in Edit Mode, with no live run. Persistent files and release build
  settings compare unchanged across the test. The preview scene is excluded
  from the release scene list. This is Unity camera/render evidence, not a
  physical-phone gameplay test.

The final repository compiler-validator receipt is `Evidence/UnityCompiler.json`.
Exact test results are in `Evidence/UnityTests.xml`; clipping details are in
`Evidence/UnityChecks.txt`. Source/scene/capture hashes are in
`Evidence/ReviewProvenance.json`.

## Artistic review

Internal assessment: **9/10 compatibility**, ready for user review. This is a
subjective art-direction judgment, not a mathematical similarity percentage or
user approval. The kit keeps the approved muted purple masonry, cool portrait
recesses, dark native contours, connected material planes and warm torch colors.
Rattlebones and the approved cast informed pixel/cluster discipline; their
anatomy and character palettes were not imposed on the environment.

Source crops, native 1× and nearest-neighbor 3× views are provided for every
non-floor module in `Review/Assets.html`; `Review/FloorReview.png` covers all
four floor variants. Full-size actual captures and the supplied full reference
are presented in `Review/Review.html`. The review includes mixed tile repeats,
native corner shapes, foreground overlaps, complete torches, frame joins,
portrait containment, atmosphere and all four tested layouts.

Approved unique source pixels were retained. Tile and rail boundary repairs
are explicitly recorded. Two old flattened-source occlusions required bounded
reconstruction: the far-right chain's missing links behind Settings and the
right pillar's missing stone behind Guide. Their repair rectangles and exact
protected-pixel checks are recorded in the manifest and JSON evidence.
New isolation masks remove adjacent scenery; source-crop alpha equality is
reported honestly rather than equated with object preservation.

The new HUD uses reusable components from the newer approved mockup family.
Metal collars, amber jewels and energy endcaps are separate from plain rails,
so resizing does not duplicate or stretch ornaments. Empty health/recess
structures have no placeholder HP, gem, energy or inventory values.

## Scope and remaining action

Only additive artwork, the dedicated preview scene, preview code/tests and
documentation belong to this branch. Existing production scenes, assets,
GUIDs, combat, characters, gems, saves and progression remain outside the change.
No PixelLab, Sprite Fusion or paid image-generation calls were used.

The user must review the actual gallery/scene before any production integration
or merge. Production controls retain their existing behavior; preview plaques
and empty HUD structures are art-only. No physical-device test is claimed.

## Per-asset index

The complete source/crop/change and reasoning records are in
`Evidence/AssetValidation.csv` and `Evidence/VisualReview.json`.

| Asset | Native size | Opaque colors | Source-crop mask exact | Internal review |
|---|---:|---:|---|---|
| WallTiles/PortraitWallA | 64×64 | 10 | True | 9/10; ready for review |
| WallTiles/PortraitWallB | 64×64 | 10 | True | 9/10; ready for review |
| WallTiles/PortraitWallC | 64×64 | 10 | True | 9/10; ready for review |
| WallTiles/PortraitWallD | 64×64 | 10 | True | 9/10; ready for review |
| WallTiles/BattleWallA | 64×64 | 10 | True | 9/10; ready for review |
| WallTiles/BattleWallB | 64×64 | 10 | True | 9/10; ready for review |
| WallTiles/BattleWallC | 64×64 | 10 | True | 9/10; ready for review |
| WallTiles/BattleWallD | 64×64 | 10 | True | 9/10; ready for review |
| WallTiles/OuterWallA | 64×64 | 10 | True | 9/10; ready for review |
| WallTiles/OuterWallB | 64×64 | 10 | True | 9/10; ready for review |
| WallTiles/OuterWallC | 64×64 | 10 | True | 9/10; ready for review |
| WallTiles/OuterWallD | 64×64 | 10 | True | 9/10; ready for review |
| BoardFrames/BoardTL | 24×24 | 45 | True | 9/10; ready for review |
| BoardFrames/BoardTR | 24×24 | 46 | True | 9/10; ready for review |
| BoardFrames/BoardBL | 24×24 | 41 | True | 9/10; ready for review |
| BoardFrames/BoardBR | 24×24 | 44 | True | 9/10; ready for review |
| BoardFrames/BoardTopRail | 64×18 | 31 | True | 9/10; ready for review |
| BoardFrames/BoardBottomRail | 64×19 | 27 | True | 9/10; ready for review |
| BoardFrames/BoardLeftRail | 16×64 | 33 | True | 9/10; ready for review |
| BoardFrames/BoardRightRail | 20×64 | 42 | True | 9/10; ready for review |
| BoardFrames/BoardCenterFinial | 20×14 | 22 | True | 9/10; ready for review |
| BackgroundLayers/EmptyCell0 | 46×46 | 10 | True | 9/10; ready for review |
| BackgroundLayers/EmptyCell1 | 45×46 | 9 | True | 9/10; ready for review |
| BackgroundLayers/EmptyCell2 | 46×46 | 9 | True | 9/10; ready for review |
| CharacterFrames/PortraitTL | 18×18 | 43 | True | 9/10; ready for review |
| CharacterFrames/PortraitTR | 18×18 | 47 | True | 9/10; ready for review |
| CharacterFrames/PortraitBL | 18×18 | 45 | True | 9/10; ready for review |
| CharacterFrames/PortraitBR | 18×18 | 50 | True | 9/10; ready for review |
| CharacterFrames/PortraitTop | 16×13 | 23 | False | 9/10; ready for review |
| CharacterFrames/PortraitBottom | 16×13 | 11 | False | 9/10; ready for review |
| CharacterFrames/PortraitLeft | 11×32 | 18 | True | 9/10; ready for review |
| CharacterFrames/PortraitRight | 13×32 | 27 | True | 9/10; ready for review |
| CharacterFrames/PortraitCollarTopA | 12×13 | 28 | True | 9/10; ready for review |
| CharacterFrames/PortraitCollarTopB | 12×13 | 26 | True | 9/10; ready for review |
| CharacterFrames/PortraitCollarBottomA | 14×13 | 17 | True | 9/10; ready for review |
| CharacterFrames/PortraitCollarBottomB | 14×13 | 17 | True | 9/10; ready for review |
| CharacterFrames/PortraitCollarLeftA | 11×14 | 22 | True | 9/10; ready for review |
| CharacterFrames/PortraitCollarLeftB | 11×14 | 17 | True | 9/10; ready for review |
| CharacterFrames/PortraitCollarRightA | 13×14 | 19 | True | 9/10; ready for review |
| CharacterFrames/PortraitCollarRightB | 13×14 | 21 | True | 9/10; ready for review |
| HudFrames/WaveTL | 20×17 | 48 | True | 9/10; ready for review |
| HudFrames/WaveTR | 20×17 | 35 | True | 9/10; ready for review |
| HudFrames/WaveBL | 20×17 | 41 | True | 9/10; ready for review |
| HudFrames/WaveBR | 20×17 | 35 | True | 9/10; ready for review |
| HudFrames/WaveTop | 32×9 | 38 | True | 9/10; ready for review |
| HudFrames/WaveBottom | 32×9 | 34 | True | 9/10; ready for review |
| HudFrames/WaveLeft | 10×4 | 7 | True | 9/10; ready for review |
| HudFrames/WaveRight | 10×4 | 6 | True | 9/10; ready for review |
| HudFrames/WaveSkull | 27×12 | 27 | True | 9/10; ready for review |
| HudFrames/GuideTL | 13×13 | 29 | True | 9/10; ready for review |
| HudFrames/GuideTR | 13×13 | 22 | True | 9/10; ready for review |
| HudFrames/GuideBL | 13×13 | 24 | True | 9/10; ready for review |
| HudFrames/GuideBR | 13×13 | 25 | True | 9/10; ready for review |
| HudFrames/GuideTop | 32×8 | 28 | True | 9/10; ready for review |
| HudFrames/GuideBottom | 32×8 | 28 | True | 9/10; ready for review |
| HudFrames/GuideLeft | 9×13 | 12 | True | 9/10; ready for review |
| HudFrames/GuideRight | 9×13 | 10 | True | 9/10; ready for review |
| HudFrames/SettingsTL | 12×12 | 24 | True | 9/10; ready for review |
| HudFrames/SettingsTR | 12×12 | 24 | True | 9/10; ready for review |
| HudFrames/SettingsBL | 12×12 | 34 | True | 9/10; ready for review |
| HudFrames/SettingsBR | 12×12 | 27 | True | 9/10; ready for review |
| HudFrames/SettingsTop | 21×7 | 17 | True | 9/10; ready for review |
| HudFrames/SettingsBottom | 21×7 | 16 | True | 9/10; ready for review |
| HudFrames/SettingsLeft | 7×16 | 12 | True | 9/10; ready for review |
| HudFrames/SettingsRight | 7×16 | 15 | True | 9/10; ready for review |
| HudFrames/SettingsGear | 22×24 | 32 | True | 9/10; ready for review |
| HudFrames/AbilityTL | 16×16 | 35 | True | 9/10; ready for review |
| HudFrames/AbilityTR | 16×16 | 27 | True | 9/10; ready for review |
| HudFrames/AbilityBL | 16×16 | 31 | True | 9/10; ready for review |
| HudFrames/AbilityBR | 16×16 | 27 | True | 9/10; ready for review |
| HudFrames/AbilityTop | 32×10 | 28 | True | 9/10; ready for review |
| HudFrames/AbilityBottom | 32×12 | 17 | True | 9/10; ready for review |
| HudFrames/AbilityLeft | 14×8 | 10 | True | 9/10; ready for review |
| HudFrames/AbilityRight | 14×8 | 12 | True | 9/10; ready for review |
| HudFrames/AbilityLeftJewel | 14×16 | 33 | True | 9/10; ready for review |
| HudFrames/AbilityRightJewel | 14×16 | 31 | True | 9/10; ready for review |
| HudFrames/SupplyTL | 20×20 | 28 | True | 9/10; ready for review |
| HudFrames/SupplyTR | 20×20 | 28 | True | 9/10; ready for review |
| HudFrames/SupplyBL | 20×20 | 16 | True | 9/10; ready for review |
| HudFrames/SupplyBR | 20×20 | 16 | True | 9/10; ready for review |
| HudFrames/SupplyTop | 30×9 | 14 | True | 9/10; ready for review |
| HudFrames/SupplyBottom | 30×16 | 6 | True | 9/10; ready for review |
| HudFrames/SupplyLeft | 10×23 | 14 | True | 9/10; ready for review |
| HudFrames/SupplyRight | 10×23 | 22 | True | 9/10; ready for review |
| HudFrames/EnergyStart | 13×24 | 21 | True | 9/10; ready for review |
| HudFrames/EnergyEnd | 15×24 | 26 | True | 9/10; ready for review |
| HudFrames/EnergyTopRail | 64×6 | 14 | True | 9/10; ready for review |
| HudFrames/EnergyBottomRail | 64×6 | 7 | True | 9/10; ready for review |
| CharacterFrames/PlayerHealthStart | 16×24 | 9 | True | 9/10; ready for review |
| CharacterFrames/PlayerHealthMiddle | 1×24 | 4 | True | 9/10; ready for review |
| CharacterFrames/PlayerHealthEnd | 16×24 | 9 | True | 9/10; ready for review |
| BackgroundLayers/HudDark | 8×8 | 1 | False | 9/10; ready for review |
| BackgroundLayers/AbilityDark | 8×8 | 1 | False | 9/10; ready for review |
| BackgroundLayers/SilverInset | 8×8 | 1 | False | 9/10; ready for review |
| Props/Gateway | 115×121 | 51 | True | 9/10; ready for review |
| Props/LeftTorch | 25×65 | 35 | True | 9/10; ready for review |
| Props/RightTorch | 26×67 | 42 | True | 9/10; ready for review |
| Props/RearBarrels | 35×46 | 47 | True | 9/10; ready for review |
| Props/FrontBarrel | 38×46 | 53 | True | 9/10; ready for review |
| Props/LeftBones | 24×40 | 45 | True | 9/10; ready for review |
| Props/RightBones | 34×38 | 51 | True | 9/10; ready for review |
| Props/RearBones | 28×34 | 39 | True | 9/10; ready for review |
| Props/PortraitBanner | 49×65 | 48 | True | 9/10; ready for review |
| Props/LeftHangingBanner | 25×63 | 44 | False | 9/10; ready for review |
| Props/RightHangingBanner | 26×61 | 43 | False | 9/10; ready for review |
| Props/LeftHighChain | 34×103 | 54 | False | 9/10; ready for review |
| Props/RightHighChain | 28×83 | 45 | False | 9/10; ready for review |
| Props/FarRightChain | 21×180 | 55 | False | 9/10; ready for review |
| Props/LeftSideChain | 16×111 | 60 | True | 9/10; ready for review |
| Props/RightSideChain | 12×111 | 60 | True | 9/10; ready for review |
| Props/LeftSideTorch | 37×83 | 58 | False | 9/10; ready for review |
| Props/RightSideTorch | 33×83 | 59 | False | 9/10; ready for review |
| Props/LeftSideBanner | 30×96 | 48 | False | 9/10; ready for review |
| Props/RightSideBanner | 29×96 | 50 | False | 9/10; ready for review |
| Props/LeftSkullPillar | 37×114 | 42 | True | 9/10; ready for review |
| Props/RightSkullPillar | 33×114 | 40 | True | 9/10; ready for review |
| Props/LeftBottomButtress | 67×135 | 36 | False | 9/10; ready for review |
| Props/RightBottomButtress | 63×135 | 35 | False | 9/10; ready for review |
| Props/LeftTopPillar | 10×180 | 32 | True | 9/10; ready for review |
| Props/CenterTopPillar | 38×28 | 30 | True | 9/10; ready for review |
| Props/RightTopPillar | 23×180 | 57 | True | 9/10; ready for review |
| Props/FarRightPillar | 12×180 | 28 | True | 9/10; ready for review |
| Props/FoundationStart | 24×23 | 30 | True | 9/10; ready for review |
| Props/FoundationTopRail | 64×23 | 35 | True | 9/10; ready for review |
| Props/FoundationEnd | 30×23 | 30 | True | 9/10; ready for review |
| FloorTiles/FloorA | 64×64 | 11 | True | 9/10; ready for review |
| FloorTiles/FloorB | 64×64 | 11 | True | 9/10; ready for review |
| FloorTiles/FloorC | 64×64 | 11 | True | 9/10; ready for review |
| FloorTiles/FloorD | 64×64 | 11 | True | 9/10; ready for review |
