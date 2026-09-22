# Grounded enemy idle family — 2026-09-22

The user accepted the restored guard motion and requested that style for the
remaining enemies from Miner through King, plus repairs to Crossbow Guard, Miner
and Basket Villager. This pass covers **19 idles / 171 poses**.

## Artwork and scope

- Miner, Basket Villager and Barricade Villager now share the accepted guard
  settle/blink/recovery pattern above fixed boots. Heads retain their shapes and
  follow the body. Held tools, basket and log load move as rigid pieces.
- Crossbow Guard's lower wood/string contour was partly assigned to the leg/body
  selection, causing the visible tear. The complete prop now moves together;
  collar/sleeve overlap closes exposed seams.
- Town Marshal, four knights, six royal units and King use the restrained guard
  offsets, without the rejected deeper motion or extra head pitch. Continuous
  body joins avoid row cuts. Larger Royal Lancer/Arbalist helmets are retained.
- Barricade Guard, Spear Guard and Siege Sergeant retain their exact accepted
  native files. Rattlebones, Farmer, Pan Villager, Bardley and Minotaur are unchanged.
- All ready poses, palettes, canvases and nine × 130 ms exposures are preserved.
  Miner remains 96×80; other idles remain 64×64. Native authoring and PNG/GIF/JSON
  exports ran in LibreSprite. Local action drawings and impact timings are unchanged.

Sources are documented in `ArtSource/RemainingCast/SelectedIdles/README.md`.
Earlier local/crossbow files are preserved beside the production source folders.
The temporary LibreSprite script entry was restored after native authoring.

## Executed art checks

`ArtSource/RemainingCast/Scripts/verify_family.py --unity` passed:

- all 171 native/PNG/GIF frames agree, including binary alpha and timing;
- all frames stay in their existing approved material palettes;
- exact static-ready / final-ready pose identity;
- fixed source contact and explicitly checked local boot pixels;
- complete, translation-only exposed prop pixels and on-canvas part bounds for
  the repaired crossbow and the newly selected Marshal/knight/royal/King sources;
- the three unchanged guards match their preserved native backups exactly;
- Unity PNGs equal the source exports byte for byte.

`ArtSource/LocalEnemies/Scripts/validate.py` also passed all 71 local idle/action
frames, including the existing action timing and ready-pose continuity. The local
manifest was refreshed while verifying that supplied originals were unchanged.
`ArtSource/GuardIdles/verify_sources.py` checks the repaired bow separately from
the three exact restored guards. Current fingerprints and geometry evidence are
in `ArtSource/RemainingCast/FamilyVerification.json` and the source manifests.

Four source contact boards were visually inspected across the cast, including
lift, low and recovery poses. The synchronized browser preview includes native
size and enlarged views beside unchanged Rattlebones. The low pose was inspected
on light and dark surroundings during source/browser review. These visual checks
remain distinct from the automated pixel assertions.

## Unity integration and runtime evidence

The existing `CombatIdleImporter.ImportEnemyFamily` imported all nineteen idles.
It uses Point filtering, no compression/mipmaps, 64 PPU, FullRect sprites, fixed
bottom-center pivots and sprite-only clips. Historical definition-name mappings
are explicit: SwordKnight → Knight, RoyalMage → CourtMage, RoyalArcanist →
RoyalArchbishop. No gameplay definition values are changed beyond visual references.
Existing local attack/ability states remain in their original controllers.

Graphics-enabled **Unity 6000.3.19f1** execution of
`CombatIdleValidation.ImportEnemyFamilyAndRun` **passed, exit 0**. It exercised
the production `Game` scene with disposable profiles:

- live playback and pause for every selected enemy;
- all nine poses per character at **1080×1920 and 1080×2400** — 342
  character/pose/resolution combinations;
- exact selected frame, stable rectangle/center/floor, uniform integer texel scale,
  portrait and UI-mask containment, health-bar clearance and shared enemy ground;
- serialized sprite/controller references, source bytes, complete clip durations,
  no idle combat/transform events, and retained authored action states/flags.

Frames 1, 6 and 9 were captured for each test group at each resolution. Selected
production captures were visually reviewed for Miner/Basket Villager,
Barricade Villager/Crossbow Guard, Royal Lancer/Swordsman and King. No centering,
floor or UI clipping issue was observed in those captures.

`Tools/Validate-Unity.ps1` passed on the final C# source. Execution artifacts:

- `.utmp/IdleFamily/unity-play.log`
- `.utmp/CombatIdles/family-asset-validation.txt`
- `.utmp/CombatIdles/family-play-validation.txt`
- `.utmp/CombatIdles/familys-*-frame*.png`
- Compile log:
  `C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-ca1b071f-1560-43ba-b888-8395d929e099.log`

The assertions establish source preservation constraints and runtime alignment;
they do not substitute for the user's final art judgment. No Android/iOS device
test was performed. This pass changes idle presentation only; combat behavior,
action events, balance, board resolution and character selection are unchanged.
