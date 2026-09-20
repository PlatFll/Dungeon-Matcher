# Local enemy animation sources

Native LibreSprite sources for Miner, Basket Villager (the berries farmer), and Barricade Villager. Original supplied files are preserved in `Originals/`.

| Character | Idle source | Auto attack source | Ability source |
| --- | --- | --- | --- |
| Miner | [Idle](Miner_Idle.aseprite) | [Side swing](Miner_AutoAttack.aseprite) | [Ground strike](Miner_Ability.aseprite) |
| Berries farmer | [Idle](BasketVillager_Idle.aseprite) | [Berry release](BasketVillager_AutoAttack.aseprite) | — |
| Barricade villager | [Idle](BarricadeVillager_Idle.aseprite) | [Axe strike](BarricadeVillager_AutoAttack.aseprite) | [Quick build](BarricadeVillager_Ability.aseprite) |

Each native file has a matching `.png` sheet, `.gif` preview and `.json` timing file in this directory.

Production exports are generated with `Scripts/export.ps1`. `Scripts/validate.py` checks every native frame against its PNG and GIF, approved material palette, timings, margins, ground contact, and ready-pose continuity. The Miner deliberately lifts his feet in the mining ability; the actor pivot stays fixed.

`*_Reference.png` is the first approved idle pose. `AssetManifest.json` fingerprints the final native files, exports and palette data; `Originals/Manifest.json` fingerprints the supplied files. The supplied Miner attack PNG and ASE contain different drawing revisions; both are preserved, and this pass uses the native ASE motion as its acting source.

`Scripts/build_native.js` contains the native LibreSprite pixel assembly and material correction commands. `assemble_native.py` only assembles script text; pixel editing happens inside LibreSprite. Final native layers must be converted with **Layer → Layer from Background**, then **File → Save** before exporting. The current development build does not reliably apply this conversion while the script transaction is active.

The action sources use frame 5 for gameplay contact: auto attacks at 320 ms, abilities at 360 ms. Recovery completes at 670/870 ms; the final exposure ends at 680/880 ms. Idles use nine 130 ms frames. Consult the project art direction for the full acting and palette rules.

The berry stays attached to the throwing hand until release; there is no travelling projectile. The build gesture sets a plank with a compact axe contact. The Miner uses a broad side swing for ordinary attacks and a lifted overhead ground strike for mining.

Open `Preview.html` through a local HTTP server for synchronized playback at native or integer zoom, or open the GIFs directly. Unity integration and executed evidence are described in `Docs/Validation/LOCAL_ENEMY_ANIMATIONS.md`. The production native files are in this directory; `Review/` is ignored scratch output. Rebuilding drafts requires the explicit native layer conversion above before promoting/exporting them.
