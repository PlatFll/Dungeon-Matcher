# Dungeon side-view reference rebuild

Branch: `feature/dungeon-top-hud-pixellab-rebuild`, based on current origin/main. Existing main and PR #200 worktrees and assets stay preserved.

1. Record the supplied reference and inventory. Work on a 360×640 native canvas matching its portrait composition, removing only the external screenshot mat.
2. Extract exact frame/prop contours into independent LibreSprite documents; rebuild clean hidden wall tiles and shallow platform connectors using actual-reference-conditioned PixelLab assets where useful. No large perspective floor.
3. Clean accepted assets by material in LibreSprite, preserve silhouettes and relative rectangles, and validate each family at 1× and integer zoom before assembly.
4. Assemble the six depth layers in an additive Unity art-only scene/prefab. Fixed reference proportions, integer pixel scale, masked frame interiors; no characters, board gems, gameplay owners or production replacement.
5. Run native source/export/import, seam, silhouette and layout checks; render reference-size, portrait and safe-area previews; run the mandatory Unity compiler validator. Commit small steps and open an unmerged review PR.

## Initial inventory and tool decisions

| Family | Reusable pieces | Method |
|---|---|---|
| Tiles | dark plum back/lower wall; blue portrait fill; narrow standing strip; platform face/caps; empty cell | PixelLab for clean unobstructed wall/platform options, LibreSprite for final course/seam and cell cleanup |
| Frame parts | player and board four corners/rails; Wave/Guide/gear shells; potion/bomb/ability corners/edges; progress track | Preserve exact reference profiles with native extraction plus LibreSprite masks and corrections |
| Props | barred opening, arch crown/piers, three upper torches, two side braziers, hanging chains, crest banners, barrels, side skulls | Reference contour extraction and native cleanup; fill hidden regions rather than bake unrelated wall into props |
| Signs/buttons | Wave, Guide, gear, hanging hardware and skull accent | Separate shells/decorations; editable preview labels |
| HUD pieces | heart/fill track, progress fill, potion, bomb, crowned skull, energy accents, resonance icon/support | Separate native decorations and frames; no gameplay behavior |
| Connectors | side piers, lower wall joints, platform supports | Native reconstruction from reference blocks, matching neighboring courses |

Numeric technical checks are distinct from the requested visual-fidelity threshold. Per-family visual acceptance must be justified against the actual reference, not fabricated as a measured similarity percentage.
