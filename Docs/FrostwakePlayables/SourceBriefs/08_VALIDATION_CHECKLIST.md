# Validation Checklist

## Frostwake / Jarl Haldra
- Glacier Cleave selects a gem correctly.
- It force-freezes that gem and marks it.
- Response window is exactly 3 accepted player moves.
- Fully unfreezing in time cancels/fails the ability correctly.
- Partial response reduces resulting damage if that is how the implemented stage formula works.
- If left at Bifrost stage, the attack deals the heaviest damage and applies Frostbite.
- Save/resume preserves marked target, stage and remaining moves.

## Minotaur
- Ability leaves frame and re-enters via board marker flow correctly.
- Movement budget defaults to 7 spaces and is tunable.
- Prioritization prefers blockers/barricades first.
- He damages barricades using authoritative standard clear damage + player modifiers.
- He does not improperly one-shot stronger barricades.
- Repeat collision/stumble behavior works when an obstacle survives.
- He can interact with vines/freeze/barricades/other destructibles as intended.
- VFX/SFX feel heavy and readable.

## Kobold Gunner
- Final design uses one revolver and is not cowboy-coded.
- Ability chooses 6 legal targets.
- Ricochet order is readable.
- Shots can affect legal board objects and enemies appropriately.
- Repeated hits on same enemy increase damage according to the passive.
- No infinite recursion / ricochet loops.
- Bullet-impact VFX and audio timings are correct.
- Revolver spin SFX is present and tasteful.

## Art
- New sprites align with current art direction.
- Native pixel scale preserved.
- Transparent backgrounds correct.
- Outline/shading/face readability hold up at gameplay scale.
- Minotaur visually reads larger and more imposing than orcs.
- Kobold redesign feels like Dungeon Matcher, not a cowboy parody.

## Reporting
- selected/rejected art candidates recorded;
- generation lineage recorded;
- branch/HEAD/test results documented.
