# Hearth flame and full-tile Marine Net

Review candidate following the user's acceptance of the Hearth Ember still and
request for flame-only motion and a thicker, tile-covering Marine Net.
The prior approved still and previous PixelLab art remain in `../PixelLab/`.

`Review.html` presents native artwork, nearest-neighbor enlargement, the flame
loop and actual Unity Game captures. These are controlled combat fixtures,
not phone recordings or claims of device testing. This revision is not merged.

## Artwork

- Hearth: eight 32x32 cels, 120 ms each, 960 ms loop, seven opaque colors and
  binary alpha. Every original stone pixel and every pixel outside the flame
  edit mask is exact. Cel zero equals the approved still. No scaling, drift,
  palette expansion or detached sparks. The original still remains the fallback.
- Net: native 64x64, six opaque colors, binary alpha, knotted heavy ropes with
  transparent diamond windows. Occupied bounds are `[1,1,63,63)` for a one-pixel
  edge clearance. Its full sprite rectangle matches the cell; the visible ropes
  reach within one native pixel of each edge. It no longer shrinks toward expiry.

PixelLab flame job `366178e0-6228-42fc-864e-a7c3ca149fba` used the accepted
Hearth Ember as both endpoint inputs (one generation). Keep source poses 0–7;
pose 8 repeats the endpoint. Cleanup restores the exact stone pixels and palette
and removes one disconnected speck. PixelLab net job
`9ea6247c-5184-4205-92ed-4d896c4d78ac` used the actual Rattlebones style image
and produced 16 alternatives (10 generations). Candidate 04 was selected for
its clean square footprint and regular heavy rope clusters. Its native geometry
is unchanged. Total this revision: **11 subscription generations**.

`generation-requests.json` records prompts, references, settings and receipts.
`manifest.json` records exact source/reference hashes and native checks.
`build.py` reproduces the guarded flame cleanup and LibreSprite exports.
`HearthFlame.ase` and `MarineSnare.ase` are the editable LibreSprite documents.
`HearthFlame.gif` and `HearthFlame-strip.png` retain native scale and timing.

## Runtime boundaries

Only the hearth presenter and net size mapping change. BoardController still
owns placement, healing, destruction, pins and expiry. The flame respects pause
and Reduced Motion; its texture binding preserves existing hit-flash properties.
The original static hearth resource and its GUID are unchanged. The net keeps
its existing GUID and production theme reference. Other sprites, gems, scenery,
UI, gameplay rules and the main checkout are untouched.

Executed test details and limitations are in `validation.json`.
