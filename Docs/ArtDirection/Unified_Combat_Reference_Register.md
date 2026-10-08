# Unified combat status art — implementation review

The approved Dungeon Matcher art guide, reference board and existing Rattlebones,
Farmer, Bardley material palettes govern this pass. This delivery does not replace
those character approvals. Generated icons are implemented candidates, not a claim
of later human visual approval.

- 23 native 24×24 transparent PNGs: 22 effects plus the ivory sword action glyph.
- Individual icon materials, broad shading clusters, dark outlines, binary alpha.
- 24 PixelLab creations plus 23 no-dither palette reductions: 26.3 generation units.
- Separate original sources, selected finals, job/prompt ledgers and free sword edit.
- [Native review](../../ArtSource/UnifiedCombat/NativeReview.png).
- [Independent technical checks](../../ArtSource/UnifiedCombat/FinalChecks.json).
- [Manifest and runtime semantics](../UNIFIED_MOVE_COMBAT.md).

Unity draws all move/stack/condition counters independently using the Thaleah
bitmap font with a one-logical-pixel dark outline. No text is baked into icons.
Legacy saved-profile views retain their old assets; new status views consume the
new Resources family. Native source files are never resized for import.
