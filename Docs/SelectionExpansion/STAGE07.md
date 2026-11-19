# Combat animation production

All ten approved PNGs were verified before submission as the actual PixelLab first frames. Every returned first frame was byte-compared in RGBA against its approved sprite. The 27 jobs consumed 50 plan generations, including five targeted corrections. The original PNGs and individual receipts are listed in the [job ledger](../../ArtSource/SelectionExpansion/Motion/jobs.json).

There are ten idle loops, ten basic actions, a shield-thump Taunt for K06 Road Escort and a single staff flourish for the Chirurgeon. Five initial basic actions were superseded after inspection for an obscured mace, unreadable firing or sling contacts, an indistinct sword, or a lengthening ram horn. The selected frames, durations and source hashes are in the [clip ledger](../../ArtSource/SelectionExpansion/Motion/clips.json). Shorter idle loops retain useful drawings at the 1.17-second cadence. The Ranger and Slinghunter show a second firing pose 180 ms after the first. The basic action has one impact event. Existing follow-up damage owns the second hit.

Color cleanup used the approved character palette, registered the feet four pixels above the bottom and removed stray idle particle pixels. Each RGBA source was saved through LibreSprite as a timed editable document and exported as an untrimmed horizontal sheet. The exported RGBA and individual frame durations were verified on reopen. All controllers, clip GUIDs, sprite IDs and enemy definitions passed a static binding check. Existing combat hit/defeat feedback and weakness-facet effects retain their existing owners.

[Native and four-times playback](../../ArtSource/SelectionExpansion/Motion/Review/Review.html) · [Unity bindings](../../ArtSource/SelectionExpansion/Motion/unity-bindings.json)

Stage 08 includes actual Game-scene checks, regression tests, builds and policy runs. Diagnostic gallery fixtures use named formations and must be kept separate from naturally reached enemies.
