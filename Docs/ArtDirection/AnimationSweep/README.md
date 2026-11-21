# Animation review

[Open the before-and-after player](Review.html). It opens on the Dungeon Miner in the Game scene. Choose a character and action to compare both versions on the same clock. Pause to step through the exposures or zoom in on the Miner’s waist.

## The Miner

The original Miner idle is back in the game. Only the second of its ten timed poses changes. It had a thin stray band across the trousers and leftover pixels below both gloves. The correction removes or recolors 26 native pixels. The head, pick movement, breathing and footing follow the original loop.

LibreSprite exported the edited master. A full-sheet comparison with `70881ad5` found all differences in frame 2, within native coordinates x58–89 and y88–92. The clip still lasts 1,170 ms. Both versions were captured in the production Game scene at 1080 × 1920 using the same layout. Exactly 104 screen pixels changed on frame 2, at the 26 corrected texels rendered at 2×. The other nine actor captures matched exactly. Choose **2** under **Changed frames** and enable **Zoom to waist** to inspect the edit. [Frame 2 close-up](media/Miner-frame-2.png)

The underground Pickaxe Delver is a different character. His apron stays connected across his 36 drawings. [Ironvein belt close-ups](media/ironvein-waist.png)

## Scope and verification

The controller pass covered 465 clips and 4,343 timed sprite bindings across 90 actor identities or forms, including the hidden playable and conditional forms. The supplementary pass reconciled the other 34 Unity animation clips, inspected 78 environment and effect bindings with 2,529 keys, and traced 124 procedural presentation components. The full corpus contains 499 Unity clips. References and retained clips are listed in the ledgers. Device frame pacing and subjective motion quality need hands-on review.

The final changes touch 48 bound clips on 27 actors. Examples include the Elven Mender’s split collar, Queen Nacre’s held trident, the Reef Spearman’s late jab, the Stonewright’s detached steel, the Champion’s hammer socket and stray material colors. Every repaired clip has its own note in the player.

LibreSprite reopened all 48 editable masters. Their 489 native cels matched the Unity textures at all 510 exposure intervals, including registration on the expanded canvases. The clip keys, events and import metadata retain their baseline hashes. [Repairs](verification.json) · [Native checks](native-checks.json) · [Actor coverage](actor-coverage.csv) · [Effect coverage](effects-coverage.csv) · [Other Unity clips](other-clips.csv)
