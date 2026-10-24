# Locked Region Rules — Do Not Touch the Top HUD

## Locked region definition
Treat the following as **locked** and preserve them from the current Astra-generated forest screen:

- Top title/header widgets (`WAVE 1`, `GUIDE`, settings)
- Top hanging decorations / lanterns already present there
- Character portrait frame area on the upper left
- The upper magical forest battle background and atmosphere
- The enemy standing area / battle-stage composition
- Overall composition from the top edge of the screen down to just above the board frame

## What this means in practice
Do not redraw, replace, or stylistically reinterpret the locked region.
Do not ask PixelLab to regenerate that region.
Use the current implementation as the approved base.

## Allowed exceptions
The only allowed edits inside the locked region are:
- tiny seam corrections where newly revised side art meets the preserved area
- tiny cleanup fixes for misalignment if absolutely necessary

These exceptions must be minimal and must not change the look of the preserved top HUD.

## Main rule
**Protect what is already working. Improve only the weaker lower/surrounding areas.**
