# Approved motion, corrected equipment

These frames supersede the earlier candidates for three auto-attacks:

- **Royal Lancer:** remove every invented shield and back rim; preserve the two-handed spear thrust.
- **Spear Knight:** remove the invented back shield and reuse the original narrow helmet slit at each frame's head position.
- **Shield Knight:** remove the sword behind his cape; preserve the shield bash.

The originals, exact PixelLab edit decisions and output IDs are recorded in `provenance.json`. Each character folder contains the reviewed `Edited.gif` and its twelve native RGBA frames. No motion was regenerated and no subscription generations were spent on this correction.

Run `Tools/Stage-EquipmentCorrections.ps1` from the repository to recreate native RGBA sources, production exports and the three Unity PNG sheets. The existing `selected.json` retains the approved frame exposures and contact frames. PNG, GIF and native source exports are checked against the exact edited pixels before integration.
