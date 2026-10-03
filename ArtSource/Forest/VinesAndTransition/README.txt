ROOTS, VINES AND CRYSTAL TRANSITION — 2026-10-03

Root_Level_1 and Root_Level_2 are separate 64x64 PixelLab sources. Root art
tracks remaining durability; existing pips retain original maximum durability.
Vine_Weave is 81.64% transparent with binary alpha and 15 opaque colors.

The first animation attempts did not sufficiently grow/retract and are retained
under Raw. Corrected growth interpolates from an empty canvas into the weave.
Selected hit uses the corrected impact lead-in followed by reversed generated
growth poses to recoil fully away. No pixel drawings were rescaled or repainted.
Growth: 9 x 45 ms. Hit: 12 x 35 ms. Neither adds a board-resolution wait.
Restore displays the static state without replaying growth. Reduced motion skips
the transient vine clips. Unity sprites are FullRect, 64 PPU, Point, uncompressed.

Transition_Smoke is the fourth generated still. The story gem comes unchanged
from Assets/_Game/Resources/UI/Finalized/SplitStoryGem.png.
Actual usage: 24 subscription generations, including revisions; no purchases.
See jobs.json, stills.json, motion.json and usage.json.
