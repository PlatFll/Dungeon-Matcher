# Animation review

[Open the before-and-after player](Review.html). It opens on the Dungeon Miner in the Game scene. Choose a character and action to compare both versions on the same clock. Pause to step through the exposures or zoom in on the Miner’s waist.

## The Miner

The previous repair closed a gap on the pick. The lower torso still changed independently of the trousers. The new idle holds the same shirt hem, pelvis, grip and legs through the entire breathing loop. The neck, head, candle and eyelids carry the movement. His raised pick now stays in front of the shoulder with both hands on its shaft in the basic and special swings.

Two independent measurements cover the reported fault. Across the ten idle exposures, the old lower-body region varied by up to 487 native pixels from the neutral pose. The same region is pixel-identical throughout the new loop. On the 1080 × 1920 Game canvas, the 54 × 36-pixel hip patch changed by up to 1,215 pixels before the repair and by zero pixels afterward. The review includes all 30 Game exposures on each side. The debugger steps the production animator to capture its exact poses. Combat event delivery is exercised separately in the Unity tests.

The underground Pickaxe Delver is a different character. His apron stays connected across his 36 drawings. [Ironvein belt close-ups](media/ironvein-waist.png)

## Scope and verification

The controller pass covered 465 clips and 4,343 timed sprite bindings across 90 actor identities or forms, including the hidden playable and conditional forms. The supplementary pass reconciled the other 34 Unity animation clips, inspected 78 environment and effect bindings with 2,529 keys, and traced 124 procedural presentation components. The full corpus contains 499 Unity clips. References and retained clips are listed in the ledgers. Device frame pacing and subjective motion quality need hands-on review.

The final changes touch 48 bound clips on 27 actors. Examples include the Elven Mender’s split collar, Queen Nacre’s held trident, the Reef Spearman’s late jab, the Stonewright’s detached steel, the Champion’s hammer socket and stray material colors. Every repaired clip has its own note in the player.

LibreSprite reopened all 48 editable masters. Their 489 native cels matched the Unity textures at all 510 exposure intervals, including registration on the expanded canvases. The clip keys, events and import metadata retain their baseline hashes. [Repairs](verification.json) · [Native checks](native-checks.json) · [Actor coverage](actor-coverage.csv) · [Effect coverage](effects-coverage.csv) · [Other Unity clips](other-clips.csv)
