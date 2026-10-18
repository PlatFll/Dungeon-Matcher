# Dungeon modular environment — corrected review kit

The supplied full mockup is the visual target. The canonical guide governs native rendering. This is an additive, unmerged art-only scene; it does not replace the live Dungeon environment.

Open `Review/Review.html`, or the Unity scene `Assets/_Game/Scenes/ArtPreview_DungeonModular.unity`. The Unity menu is **Dungeon Matcher → Art Preview → Open Dungeon Modular Scene**. The component exposes viewport/safe area and editable sample labels.

150 separate editable LibreSprite modules are under `Editable/`; native exports are under `Exports/`. `AssetManifest.json` records sources/changes; `LibraryManifest.json` records current measurements. `Correction/Before/` preserves the rejected artwork and captures, and `Correction/BeforeReports/` preserves superseded assessments.

The preview has no characters, enemies, board gems or gameplay owners. Static UI includes the source heart track, crowned-skull ability emblem, lightning, energy fill, potion, bomb and resonance jewel. Corners and props remain native-sized; straight rails/materials tile; portrait and board interiors are strictly masked.

`Build.ps1 -Phase All` runs the historical base extraction and then the current `Correction/RunNative.ps1` recipes in LibreSprite. It uses archived generation results and never spends credits. The correction scripts are `FloorPolishCorrection.js`, `WallCorrection.js`, `DecorationCorrection.js`, and `HealthCorrection.js`. `Correction/UpdateManifest.py` refreshes source records. After importing/rebuilding through the Unity editor tool, run `Correction/ValidateCorrection.py` for current native/copy/seam checks. Earlier `ValidateFloor.py`/`ValidateKit.py` and their reports describe the superseded pass, not acceptance of these corrections.

See `VALIDATION_REPORT.md` for actual verification and limits, and `PixelLabUsage.json` for the 11-generation ledger. User visual approval remains pending; do not merge without it.

## Current lighting-only follow-up

The latest 19-module lighting pass preserves the existing composition and every sprite silhouette. See [lighting comparison](Review/Lighting.html) and [scoped report](Correction/Lighting/REPORT.md) for the immediate before/after, exact preservation checks and fresh Unity renders. No additional generation spending.
