# HUD candidate source lineage

Stage 07 authored source without running Unity. Stage 10 subsequently captured
the actual Game HUD in five zones and five portrait/safe-area profiles, with
separate charge/motion cases. Candidate overlays are native UI rectangles/pooled
pixels in CrystalResonanceHud.cs; no raster replacement or generation occurred.
See the validation report for exact coverage; listening/device approval remains open.

| Source | SHA-256 | Read-only inspection |
|---|---|---|
| Canonical art guide | 4122181015ec3442b830e846c605de94d54374f1cde44af893d07b0f5528cce7 | Current instructions, including user-edited silver sources |
| Golden register | e95eae0c386ba33f573bd0d70749e74076360021aeab95e53d3ab0a54b279958 | Primary/supporting authority |
| Audit2026-10/media/skeleton-still.png | 705c4c8379d56a262e287d256664db8d180a79917f4a84289bbb813730b9616c | Actual 64x64 primary image viewed; hash matches register |
| CombatIdles/Rattlebones_Idle.png | 59b04929e9fdc15ae9f1d70fd3c85c6ea5362bdac479eb12f45cfcbb0efe4306 | Current production hash matches register |
| UI/Finalized/SplitStoryGem.png | 026194cbe4095abd4d71d8a148a16a2e84844c43634e26f78b53f8a0ef0da5bf | Exact approved 40x36 split crystal viewed, binary alpha |
| UI/BattlePresentation/Silver/WavePlaqueSilver.png | 39f36486535958053ed2f2a031a07233b8618c4d1614ed0c8e5eeaea7642eb8f | Exact user-edited 264x99 plaque viewed, binary alpha |

Retained crystal palette: #D63850, #FFB6B9, #FFE0FF, #653285, #151423.
Native pixels use integer snapped placement and the crystal's fitted source pixel
scale; the existing UIPixelParticle has optional bounded-reuse/no-resample mode.
Its default consumers retain their prior behavior. Original audio cue resources
are reused through CombatAudioController's volume/cooldown/voice/preferences flow.
Actual Frostwake captures exposed low contrast; an opaque native dark backing
now keeps milestone/capstone feedback readable over snow. Automated bounds and
pause checks pass; visual evidence remains a candidate for user acceptance.
Audio routing was tested, but human listening approval has not been obtained.
No new approved sprite or new golden reference is claimed.
