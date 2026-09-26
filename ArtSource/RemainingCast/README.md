# Remaining cast — recolors and idle family

**Current selection:** the user accepted the restored guard style. Nineteen enemy
idles from Miner through King now share that restrained rhythm and are integrated
in Unity. See `SelectedIdles/README.md` and `FamilyPreview.html` for the current
sources, scope and comparison. `Idles/` remains the rejected stronger candidate;
the historical production description below refers to that earlier pass.

Working art pass, 2026-09-22. All 17 static character tabs supplied alongside
`RattleBones_FluidIdle.ase` were recolored, then given a matching idle in LibreSprite.
The correction pass enlarges Royal Lancer/Arbalist helmets, repairs cut anatomy
and incomplete prop selections, and strengthens coordinated shoulder/body acting.
The user's Rattlebones animation and original sprite files were preserved.
This is an art-source delivery; these new files are not assigned in Unity yet.

Open `Preview.html` for synchronized playback against the unchanged Rattlebones
reference. It includes native-size views, frame stepping, faction groups, zoom,
and light/dark backgrounds. The Pass selector switches between the corrected
and preserved previous animation at the same frame. Open `Idles/*_Idle.aseprite`
to edit the animations.

## Files and identities

| Supplied source stem | Output stem |
|---|---|
| CrossBow_Guard | CrossbowGuard |
| BarricadeGuard | BarricadeGuard |
| spearGuard_1 | SpearGuard |
| Marshall_1 | TownMarshal |
| SiegeSergeant | SiegeSergeant |
| SwordKnight_1 | SwordKnight |
| SpearKnight_1 | SpearKnight |
| ShieldKnight_1 | ShieldKnight |
| KnightCaptain1 | KnightCaptain |
| SwordRoyalKnight1 | RoyalSwordsman |
| spearRoaylKnight1 | RoyalLancer |
| RoyalArbalist1 | RoyalArbalist |
| BannerRoyalKnight1 | RoyalStandardBearer |
| RoyalArcanist | RoyalArcanist |
| RoyalMage1 | RoyalMage |
| TheKing | King |
| Minotaur64x64 | Minotaur |

Names identify the supplied artwork; they do not establish new gameplay definitions.
In particular, Barricade Guard is distinct from the earlier Barricade Villager.

- `Originals/`: native copies captured from the 18 open LibreSprite tabs, including
  the unchanged Rattlebones motion reference. `SourceManifest.json` records hashes,
  dimensions, bounds, timing, and original colors.
- `Recolored/`: 17 native stills and transparent PNGs. Fifteen retain their original
  masks. Royal Lancer and Royal Arbalist have the requested larger helmets;
  their bodies, props, palettes and placement remain unchanged.
- `Idles/`: 17 native animations, untrimmed horizontal PNG sheets, JSON exposure
  tables, and transparent GIFs. JSON sheet paths are portable filenames.
- `Palettes.json`: the exact opaque palette of each recolored character.
- `Review/`: before/after boards, all-frame contact sheets, coordinate inspection
  boards, and the unchanged reference exports. These are inspection assets.
- `Validation.json`: final native-file hashes and executed export/palette checks.
- `BeforeCorrection/`: preserved previous idles and the two pre-enlargement stills.
- `Review/Correction/`: all-nine-frame boards with Rattlebones, before/after boards,
  and native part drawings. `RigParts` frames are diagnostic selections (ready,
  body, head, prop, shield, flag, then blanks), not production animations.

## Color and motion decisions

Colors come exclusively from the existing guide. Guards use cool iron, warm skin,
practical wood/leather and restrained blue/cloth accents. Knights retain their
recognizable armor, helmet, plume and weapon designs. Royal characters use brighter
steel, warm gold and the established deep red cloth family; bright metal accents
remain separate from fabric. The Mage keeps pale cloth and a cyan crystal staff.
The Arcanist keeps his gold staff head, wood shaft and warm face. The Minotaur keeps
brown fur, pale horns, gold details and red cloth. Top-left cluster lighting and
the shared `#0A0D11` outline carry across the cast.

Every idle uses nine 130 ms frames, **1170 ms per loop**, matching the exact open
Rattlebones reference. A small lift gives way to a compact dip, then recovery.
Head motion follows the torso; shoulders roll and knees compress over fixed soles.
Continuous torso compression replaces the earlier abrupt row/region deformation.
Head, neck and sleeve joins overlap so the rise cannot separate a hand or outline.
Visible eyes narrow on frame 5, close on frame 6 and reopen during recovery.
Full helmets keep their visor designs. The King and Minotaur use less compression
to retain weight. Capes and the banner tip have a small delayed response; plumes
stay attached to their complete helmet drawing.
Long planted tools keep their shape and grip; carried crossbows, swords and axes
follow the body as rigid objects.

Frames 1 and 9 match the recolored still exactly. Their combined 260 ms hold across
the seam is intentional. Each corrected idle has eight distinct drawings. All
canvases remain **64×64**, with fixed body soles on row 63 and hard, binary
transparency. Crossbows and the carried shield can overlap a foot; validation
checks the exact rigid prop pixels where they obscure it. Do not trim or
recenter individual frames when importing later; use full rectangles and a fixed
bottom-center anchor at the existing source pixel scale.

This is a reviewable extension of the current material and animation family, not
an automatic approval of a new global faction palette. Historical reference-board
assets remain unchanged. The open Rattlebones file is a motion reference here;
its own colors were expressly excluded from recoloring.

## Reproduce and verify

Native authoring uses LibreSprite's pixel/cel API. `cast_spec.js` contains material
assignments; concatenate it before `recolor_native.js` or `animate_native.js` and
run inside the LibreSprite GUI. The installed development build does not expose
an active sprite to batch scripts, so do not substitute `--batch --script`.
Set `ROOT` to this folder if using another checkout. Animation writing uses the
existing Pan idle solely as a nine-cel, full-canvas transparent storage template;
every pixel and palette entry is replaced. Its artwork is not a motion source.
The helmet correction always reads the preserved `BeforeCorrection/Recolored`
stills, making repeated animation authoring idempotent. Do not run the one-time
`preserve_open_idles.js` again over this archive. Do not rerun initial recoloring
over the corrected stills without then applying the helmet correction.

Export with PowerShell:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File ArtSource/RemainingCast/Scripts/export.ps1 -LibreSprite "path/to/libresprite.exe"
```

Read-only art checks require Python and Pillow:

```text
python ArtSource/RemainingCast/Scripts/review_recolors.py
python ArtSource/RemainingCast/Scripts/inspect_idles.py
python ArtSource/RemainingCast/Scripts/review_correction.py
python ArtSource/RemainingCast/Scripts/validate_exports.py
```

These check original-mask preservation (with two explicit helmet exceptions),
frame colors/alpha, connectivity, timing, fixed body soles, ready-pose return,
unchanged sources, exact native/PNG/GIF pixels, and translation-only exposed props.
The prop check excludes documented regions hidden by the head during compression;
it also checks every pixel of the complete part stays within the canvas.
The inspection scripts generate review layouts and JSON evidence; they never edit
the native artwork. Visual review remains necessary for material identity and
acting. See `Docs/Validation/REMAINING_CAST_ART.md` for the executed review and limits.
