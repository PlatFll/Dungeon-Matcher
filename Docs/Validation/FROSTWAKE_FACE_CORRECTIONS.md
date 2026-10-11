# Approved Frostwake facial-color integration — 11 October 2026

The user approved all final corrections and explicitly authorized integration,
validation and merge. Seven stills and 43 sheets replace the existing Unity PNGs
without changing `.meta` files, GUIDs, pivots, animation clips, controllers,
events, serialized enemy definitions or gameplay code. The baseline is merged
main `c2869ddc09f6b7a021b7dbd492ba683303a651b1` (PR #191).

## Final appearance and sources

- Human eyes are solid dark: Raider, Reaver, Hearthkeeper, Champion and Haldra.
- Wolf's iris uses one existing ice blue; socket outline/fur remain intact.
- Skald uses warm beige `#DFA881` skin and gray hair/beard. His initial ivory skin
  and Haldra's silver eyes are superseded review proposals.
- Closed eyelids, native pose geometry, outfit and equipment are preserved.

`ArtSource/Frostwake/face-revision/` contains the exact PixelLab receipts,
guarded edits, before PNGs, selected candidates, approval and integration
records. All edits were made with PixelLab's free exact pixel tools: **zero
subscription generations or purchased credits**. The approved native sources
and selected palette cels now produce the corrected runtime sheets on re-export.
Golden-reference image hashes and the four unrelated user edits are preserved.

## Executed checks

- Exact guard verification: 11,840 changed source pixels across seven actors;
  alpha and pixels outside the selected regions are unchanged.
- `Tools/Check-FrostwakeAssets.py`: all **70 sheets / 501 exposures** match Unity
  exports, approved palettes, 128x104 cells, binary alpha, Point filtering and
  four-pixel floor pivots. Minotaur outputs remain unchanged.
- Preserved binding hashes in `face-revision/integration.json`: imported PNG
  `.meta` files and existing Frostwake animation assets retain their bytes.
- `Tools/Validate-Unity.ps1`: **passed, exit 0**, Unity **6000.3.19f1**.
  Log: `C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-a7b30f05-3d73-4077-b27b-6186db2f4325.log`.
- Existing Unity gameplay/presentation tests: **5 passed, 0 failed, 0 skipped**.
  XML: `.utmp/ForestValidation/3ecda389-5e21-4aa6-9cb5-1d666b5a319d.xml`.
  Covers all eleven basics and native sprites, support shield/rally/heal/Frostbite,
  Champion cancellation/Stagger, Haldra's three responses/single release, and
  three portrait layouts. The batch audio mute hook remained enabled.
- Actual Unity captures inspected for Skald/Hearthkeeper/Raider and
  Troll/Champion/Haldra. Evidence is in `FrostwakeFaces/`.

This is scoped Editor runtime, visual and native-asset validation. It is not a
new physical-device, performance, pacing or audio-listening certification.
