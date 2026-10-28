# Review checkpoint

Revision04 aligns the existing 32px floor around native enemy floor y204. Floor y188..220; separator y220..239. Both moved down 14 native pixels without redrawing/scaling. Actors, HP, countdowns, weakness gems, board, portrait and all other UI remain fixed. Same scenery-around-baseline ownership as Dungeon/Forest/Court. Prior scenes and all artwork preserved.

Actual art-only Unity previews and validation pass at 360x640, 720x1280 and 1080x1920. Source preservation and serialized delta checked. Zero generation spend. Review: http://127.0.0.1:8899/Revision04/Review.html while loopback gallery runs. Existing draft PR215. No production integration or merge; stop for user review.
