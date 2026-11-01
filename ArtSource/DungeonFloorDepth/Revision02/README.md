# Connected Dungeon floor — revision 02

Candidate for visual review, not merged. Replaces the rejected stacked-course treatment in PR #222.

The Dungeon standing surface was only 12 native pixels deep. Repeating that edged strip upward looked like stacked ledges. The current revision removes those repeats and replaces the enemy-side surface with one new 128×32 PixelLab slab module, repeated horizontally only. There is no old course underneath or overlapping the new walking surface.

LibreSprite removes the generated side band, cleans deep speckles and makes the repeat joint consistent. Five opaque colors, binary alpha, no scaling, exact ASE→PNG reopen/export. Raw candidates, references, request/receipt and native source remain here. Cost: 10 generations; balance 4643→4633.

The doorway, rear column and grounded props now meet the rear floor edge, 20 native pixels farther back. The foreground portrait posts, old masked portrait-side course and front separator retain their positions. Enemy soles, health bars, countdowns, board, shared controls and gameplay remain unchanged. Original asset PNGs remain intact. The optional floor has the original production surface as its missing-art fallback.

[Actual Unity before/after review](Preview/Review.html) · [Native floor](Exports/Floor_Standing.png) · [Editable LibreSprite source](Native/Floor_Standing.ase)

## Executed verification

- Actual Game scene with Rattlebones, three enemies and 64 gems.
- Dungeon: 720×1280, 1080×1920, 1080×2400, 1080×2400 with safe-area insets, and 360×640.
- Underwater reference: 720×1280 and 1080×2400 safe-area.
- Exact frozen before/after pixel checks: zero changes outside the authorized floor/grounded-background region; zero changed portrait pixels in all five Dungeon profiles.
- Actor feet and all captured UI/board geometry identical before/after. Shared authoritative floor anchor retained, with native-pixel scenery rounding.
- 32-pixel connected surface, no horizontal module gaps/overlaps, no old enemy-floor course underneath, and fascia directly below.
- Existing native-art import/layout checks, accepted board swap and Settings opening/closing pass.
- Tools/Validate-Unity.ps1 passed, Unity 6000.3.19f1; receipt in QA/Compilation.json.
- Nine pre-existing local settings/assets preserved byte-for-byte. Golden and original-floor hashes unchanged.

Intermediate failures are retained in QA/*Pass: incorrect initial anchor expectation, portrait mask fringe, region bounds and float projection precision. The final solution preserves the original portrait area rather than altering its mask. Screenshot comparisons still have zero tolerance. World-to-screen rectangle readback allows 0.05 physical pixel for matrix arithmetic; native drawing scale and pixel equality are separately checked.

Artistic verdict: a clear, broad, quiet walking plane with the gate behind it, matching the underwater zone's depth logic while retaining Dungeon violet stone. Final approval remains with the user. No Android build or physical-device test was run for this correction.
