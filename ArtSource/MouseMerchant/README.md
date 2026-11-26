# Mouse merchant

![Shopkeeper expressions and inventory art](Merchant-and-supplies.png)

The approved mouse leans on the counter with both paws. Happy, Helpful and Playful share one registration point, so the hat, ears and counter contact stay put when dialogue changes. The banded faceted orb represents the shop Color Bomb. The alternate gem cluster is retained here for comparison.

The art came from four PixelLab jobs using the supplied mockup. The image hashes, seeds, job IDs, reference roles and final sprite dimensions are in [PixelLab.json](PixelLab.json). The UI plaques and icons were sliced at native resolution in LibreSprite. Every runtime PNG has a corresponding editable file in [Native](Native).

The alcove and foreground counter meet at row 146 of a 320 × 164 composition. Unity renders the mouse between them. Prices, inventory counts, comments and button labels are live text. The shared board Color Crystal uses its existing star.

The shop uses the existing Main Menu entry point. The five stock counts and two equipment choices are saved by AccountProgression. Game-scene tests use disposable accounts.

## Unity verification

![Mouse merchant in the Unity Game view at three portrait sizes](Shop-in-Unity.png)

The Unity 6000.3.19f1 validator passed. All 47 tests across the merchant, Balance V1, continuation and menu review suites passed. The shop was captured at 540 × 960, 720 × 1280 and 1080 × 2340. The UI test also checked a simulated asymmetric safe area, the fifth item after scrolling, a purchase, equipping and returning to the menu. The Game scene tests exercised supply use, cancellation, temporary shield expiry and resuming a confirmed Color Bomb. Physical-device touch and cutout testing remains outstanding.
