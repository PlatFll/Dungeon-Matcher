# Battle Environment Authoring

Dungeon Matcher battle scenery is authored as reusable world-space Tilemap prefabs and positioned by the scene-level `BattleBackgroundTilemapController`.

## Ownership

- `BattleBackgroundTilemapController` remains the runtime authority for projecting the environment onto the responsive battle-area UI, aligning it to `BattleFloorAnchor`, snapping it to the 64-PPU pixel grid, clipping it to the frame interior, and suppressing invalid scenery.
- `BattleEnvironmentRoot` marks one authored environment prefab and contains the environment's Tilemap layers.
- Environment prefabs contain presentation only. They do not own gameplay state, enemy placement, board rules, or combat timing.

## Default environment

The fallback authored environment is:

`Assets/_Game/Resources/BattleEnvironments/Dungeon_Default.prefab`

When `BattleBackground` has no `BattleEnvironmentRoot` child, `BattleBackgroundTilemapController` loads this prefab from `Resources/BattleEnvironments/Dungeon_Default`.

In Edit Mode the controller instantiates it as a real prefab instance. In Play Mode it instantiates it normally. If an environment prefab instance is already present beneath `BattleBackground`, that instance is used instead of creating the default.

This allows future environments such as `Dungeon_Royal.prefab` or `Dungeon_Deep.prefab` to use the same placement/masking system without duplicating runtime logic.

## Prefab hierarchy

Each environment prefab uses a `Grid` on the prefab root and these four Tilemap children:

```text
Dungeon_Default
├── BackWall
├── Architecture
├── Floor
└── BackDecor
```

The intended renderer order is:

- `BackWall`: -100
- `Architecture`: -99
- `Floor`: -98
- `BackDecor`: -97

The Grid is rectangular with cell size `1 x 1`, zero cell gap, and transform scale `1 x 1 x 1`.

Prefab TilemapRenderers use Mask Interaction `None` so painted tiles remain visible in isolated Prefab Mode. The scene controller applies `Visible Inside Mask` to the environment instance when positioning it in the battle viewport. Do not apply that scene mask override back to the source prefab.

## Pixel-art scale invariant

Battle environment sprites must use `GameplayPixelLayoutController.AssetsPPU`, currently 64 PPU.

For the current environment workflow:

- one source tile is 64 x 64 pixels,
- one tile is imported at 64 PPU,
- one tile occupies exactly one `1 x 1` Grid cell,
- environment prefab and Tilemap transforms remain at scale 1.

Do not enlarge the Grid or Tilemap to visually match Canvas dimensions. The runtime controller is responsible for projecting world-space scenery into the responsive UI battle viewport.

## Artist workflow

Author scenery in Prefab Mode rather than painting the Tilemaps while viewing the large gameplay Canvas:

1. Open `Assets/_Game/Resources/BattleEnvironments/Dungeon_Default.prefab`.
2. Unity enters Prefab Mode and shows the environment at its natural world-space scale.
3. Open `Window > 2D > Tile Palette`.
4. Choose the intended Tilemap child (`BackWall`, `Architecture`, `Floor`, or `BackDecor`) as the active Tilemap.
5. Paint normally on the 1-unit Grid.
6. Save the prefab.
7. Return to `Game.unity`; the prefab instance uses the existing battle viewport alignment and mask automatically.

The authored origin `(0, 0)` is the battle-floor baseline after runtime placement. Back-wall cells normally extend upward from that line. Floor art can extend at or below the baseline according to the tile artwork.

## Adding another environment

Create another environment by duplicating an existing valid environment prefab, then edit the duplicate in Prefab Mode. Preserve:

- `BattleEnvironmentRoot` on the root,
- the 1-unit Grid,
- the four layer roles,
- the renderer ordering,
- 64-PPU source art,
- root transform scale 1.

A scene can use a different environment by placing that prefab beneath the scene's `BattleBackground` object. The controller resolves the active `BattleEnvironmentRoot` and scopes its Tilemap cache/masking to that environment.

Do not create a second placement/masking controller per environment.

## Legacy scene Tilemaps

`BattleBackgroundTilemapController` still falls back to direct descendant Tilemaps when no `BattleEnvironmentRoot` exists. This preserves compatibility with the pre-prefab scene hierarchy during migration.

Once an environment prefab is present, only Tilemaps beneath that `BattleEnvironmentRoot` are treated as the authored battle environment. The older scene-owned Grid can therefore be removed after local painted content, if any, has been migrated into the prefab.
