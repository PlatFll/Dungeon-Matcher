# Enemy attack production and ability review

Produced with PixelLab on 2026-10-01 from the existing recolored cast. The account used 43 generations during this task (2,000 to 1,957 remaining). No paid credit top-up was made.

## Production attacks

All sixteen previously missing basic attacks are now in the existing definition-selected idle controllers. Farmer, Pan Villager, Miner, Basket Villager and Barricade Villager retain their existing authored attacks.

Each new attack lasts 680 ms. `selected.json` records the selected source frames, exposures and visible impact frame. Nine- and ten-pose sequences retain anticipation, a short contact pose and recovery. Damage uses the existing actor animation event; independent follow-up hits keep their established gameplay timing. Generic UI lunges are disabled for these authored attacks.

PixelLab supplied the native poses. LibreSprite reduced each frame to the original character palette, optionally adding one cream impact color, and exported binary transparency. Body pixels are never rescaled. Most canvases are 64 x 64; King, Knight Captain and Royal Swordsman use symmetric 96 x 80 padding to retain weapon motion and the shared bottom-center anchor. Point filtering, Full Rect, 64 PPU, no compression and no mipmaps are preserved.

`*_AutoAttack.aseprite` are editable production sources; matching PNG, JSON and GIF files are exports. Runtime copies and clips live in `Assets/_Game/Art/CombatActions` and `Assets/_Game/Animations/CombatActions`. `manifest.json` records hashes, native geometry, frame timing, lineage and rejected drafts. `prompts.json` records the PixelLab prompts and job IDs. Original candidate frames and review sheets remain in `Candidates/`.

Rebuild with `Tools/Finish-EnemyAttacks.ps1`, then `Tools/Review-EnemyAttacks.ps1`. The latter imports through `CombatActionImporter` and exercises actual attacks in the Game scene at both portrait sizes. `Tools/Fetch-PixelLabFrames.py` can recover the original frames from the explicit service download URLs in the manifest.

## Ability concepts awaiting selection

Open `AbilityConcepts.html` in a browser. It embeds all eleven PixelLab motion studies and offers two King impact treatments: Royal gold and Steel flash. Pause/replay and per-card choices produce a summary to send in chat. Choices are not saved or transmitted automatically.

The King's diagram demonstrates blinking warnings, a column strike, a held board, a second thrust and row strike, then refill. The board and VFX diagram are review illustrations. These special-ability timings and drawings are **not integrated into Unity**; the user requested selection first. Existing ability mechanics remain authoritative until a direction is chosen. The Siege Sergeant's selected, shortened hammer poses are also used for his basic attack; this does not activate the proposed Hammer Time ability presentation.

After selection, finish the approved ability palettes, contact/recovery poses and effects, connect them to existing actor/board ownership, and verify interrupted casts and the King's single held-board sequence in Unity.
