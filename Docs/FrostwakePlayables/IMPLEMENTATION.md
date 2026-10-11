# Frostwake Peaks and new playable characters

Status: implementation in progress on `codex/frostwake-new-playables`, starting
from merged main `c8fec990545f92af14cd4e0a76b75a4fb9f65c43` (PR #189).
The user's task authorizes integration and merge after validation. Character
still approval remains required before full animation production by AGENTS.md.

## Audit and scope

- Main contains four playable zones and three player definitions. Frostwake is
  a documentation-only future-zone lock; there are no existing mountain enemies,
  scenes, encounter assets or Haldra runtime to amend.
- This pack authorizes a Frostwake direction and mechanic foundation. Its Phase 2
  explicitly permits documenting remaining full-zone production. This work will
  not silently expose an unfinished fifth zone through random crystal travel.
- Some older PROJECT_STATE paragraphs describe seconds-based basics and three
  zones. The current top-level milestone, serialized profiles and runtime use
  unified accepted moves in four zones. The additions follow that current model.
- PlayerDefinition / CharacterAbilityDefinition / IPlayerAbilityRuntime own the
  two characters. BoardController retains all occupancy, damage to objects,
  chaining, refill and settlement; no second board resolver will be introduced.
- Existing Cracked Gems has character-named board methods (a documented older
  boundary). New player actions use generic sequential board-impact operations.
- Accepted abilities are journaled before activation. In-flight recovery replays
  from the last stable snapshot with saved RNG; no serialized coroutine is needed.
- Four pre-existing user edits remain excluded: the Court throne prefab and the
  Disabled / Highlighted / Pressed consumable sprite metadata. Starting hashes
  are preserved in `.utmp/FrostwakePlayables/user-changes.json`.

## Frostwake direction

Iceborn human clans inhabit moving mountain halls. Glaciers are swallowing their
paths; they seek the wandering crystal to save their people. Viking-inspired
timber, rope, hide, fur and carved iron distinguish them from Crown soldiers.
Dark rock, cold deep blues, pale ice and sparse warm braziers keep actors legible.
Future scenery needs mountain depth and inhabited halls; gameplay UI needs its
own timber/iron/ice materials. Main menu and opened settings retain their design.

Starting roster: Iceborn Raider, Harpoon Hunter, Frostfang Wolf, Rimecarver,
Shieldmaiden, Rime Skald, Hearthkeeper, Frostblade Reaver, Glacier Troll,
Iceborn Champion and Jarl Haldra the Icebreaker. These are production briefs,
not eleven approved character designs or completed enemy kits.

Frost → Heavy Ice → Bifrost is gem encasement, separate from Ironvein stone
occupancy. Neglect grows the ice; no temperature meter or player hard-freeze.
Deliberate Bifrost destruction retaliates and applies the existing Frostbite
status. Cascades and ability/special clears are safe. Durability, cadence and
damage are provisional data.

Glacier Cleave selects a physical gem, encases it at Bifrost and marks it for
exactly three future accepted moves. Full thaw/destruction cancels the attack;
partial thaw reduces the hit. A surviving Bifrost target causes the heaviest
damage plus Frostbite. Target identity, remaining ice and deadline must survive
Continue; a replacement gem in the same cell cannot inherit the mark.

## Playable contracts and provisional decisions

Dungeon Attender uses the supplied warm-brown horned bull identity. Its native
drawing must be taller and broader than the current ~55–58px orcs, without
resampling. Jump → marked landing → seven collision/travel steps by default.
Landing consumes the first step. Each following step moves one orthogonal cell,
or retries a surviving obstacle without moving through it. Direction changes
occur only between straight segments. Blockers take the shared normal-clear
durability damage and eligible card modifiers, never unconditional destruction.
Priorities are meaningful blockers, specials, affinity, then ordinary targets.

Kobold Gunner is redesigned around one revolver, a listening pose, a reptilian
snout/tail and dungeon scavenger clothes. No cowboy hat or second weapon.
Six sequential legal board impacts can ricochet once each to a living enemy.
The same recipient's hit count persists across the whole cast (even with other
recipients between hits), resets next cast, and increases a bounded flat bonus.
Targets are revalidated before each impact; no recursive ricochet events.

## Art and gates

Guide v1.13.2 and current register hashes are in
`ArtSource/FrostwakePlayables/input-manifest.json`. Current Rattlebones, Farmer,
Pan Villager and Orc Trailguard hashes match the register and production sheets.
Orcs are anatomy/scale comparisons; proposed golden membership is not approval.
Both chat concepts are preserved unchanged. PowderFang contains an opaque
checkerboard and 675 opaque colors: inspiration only, not an importable sprite.

PixelLab began with 223 subscription generations and $0 credit. Two Pro calls
use actual Rattlebones style imagery and labelled identity references: Minotaur
80×80 / 15 generations, Kobold 64×64 / 10 generations. Requests and receipts are
saved beside the outputs. These are candidates pending review, not approvals.

## Worklist

1. Complete current-source audit and stage native character candidates.
2. Implement/test generic sequential impact and Frostwake mechanic foundations.
3. Implement both player runtimes, data, selection and continuation coverage.
4. Obtain still approval; produce and review identity-preserving motion, marker,
   impact art and original temporary SFX; import through existing UI owners.
5. Test accepted/rejected activation, obstruction damage/modifiers, chains,
   target legality, repeated damage, cancellation, pause, save/replay and Haldra.
6. Verify native sprites and portrait gameplay scale, run the required Unity
   validator with muted audio, review the complete diff, open PR and merge.

## Current checkpoint

The generic sequential resolver, both ability runtimes and the separate
Repeat Ricochet passive definition are implemented. Character selection code
recognizes both future definitions. Production PlayerDefinition assets,
approved motion, VFX/SFX and presentation wiring remain incomplete.

Ice encasement and Glacier Cleave are testable foundations. Current zones have
no FrostwakeRules assigned, so this does not add ice to their gameplay or expose
a fifth travel destination. The shared frozen-gem overlay is an explicit
presentation fallback; it is not approved Frostwake production art.

Both recommended stills are in `ArtSource/FrostwakePlayables/review/`. The
Minotaur is 80×80 (62×65 occupied, 13 opaque colors); the Kobold is 64×64
(50×59 occupied, 15 opaque colors). Alpha is binary. Actual golden images were
provided to PixelLab. Their approval request is pending; no animation has been
commissioned. PixelLab tool charges total 25.2 generations; the account reports
198 remaining after starting at 223 (whole-number balance display).

Executed validation so far, with batch Unity audio muted:

- `.utmp/ForestValidation/07b91650-8008-43c3-8a83-ecabd68cfff6.xml`:
  initial sequential-impact foundation, 5 passed.
- `.utmp/ForestValidation/70b7d4d2-f968-49e3-ad06-ade42b105920.xml`:
  foundation plus ice/channel/identity/cleanup checks, 10 passed.
- `.utmp/ForestValidation/c7f9dec9-bb82-4c90-b37f-0582f5dbd7d2.xml`:
  5 passed, 3 failed because fixtures assumed zero initial energy. Assertions
  were corrected to measure the actual spent amount.
- `.utmp/ForestValidation/d33d7f13-456b-4590-bb98-a82fbbd247ed.xml`:
  ricochet and pause/blocked-acceptance passed; Minotaur failed because the
  fixture attempted two stacks of a one-stack card. The revised fixture tests
  the live +1 modifier, reducing a level-three obstacle from three hits to two.

- `.utmp/ForestValidation/101586a6-bef4-4d35-a5f3-982d3edde9ed.xml`:
  the live Minotaur/card interaction passed. The full Continue fixture exposed
  its missing `hasSpecialAbility` data flag; enabling that flag allows the
  existing production factory to recreate the kit.
- `.utmp/ForestValidation/2f204301-b87f-4fc8-b35c-f2f4fee68b10.xml`:
  unified Haldra save/Continue passed, preserving physical gem identity, Heavy
  Ice stage, three response moves, the unified warning counter and basic hold.
- `Tools/Validate-Unity.ps1` completed successfully with Unity 6000.3.19f1;
  log `C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-dc1c7198-8c83-40df-a4e1-aafe54144606.log`.

Final review added a destroyed-caster guard to Haldra's queued callback.
The mandatory validator passed again on that source; final log:
`C:/Users/USER/AppData/Local/Temp/DungeonMatcher-UnityValidation-6e23cac1-1d95-4224-ac79-26c8f0bc8e30.log`.
No test or PixelLab job is still running. Resume from this branch after the
still approval; no automatic background continuation is installed.
These checks do not establish finished player presentation, human art approval,
full cross-zone regression, physical-device behavior or full-zone production.

## Next actions after still approval

1. Record the exact approved PNG hashes before producing animation. Keep both
   identities, direction, equipment and palette; expand transparent canvas for
   motion as needed, never resample anatomy. Measure Minotaur gameplay pixel
   scale against orcs again after import.
2. Generate/verify idle, ability, hit/death/utility states required by the player
   presenter, Minotaur landing marker and impact VFX. Compose the requested
   temporary heavy impacts and playful revolver shot/spin sounds locally.
3. Create production PlayerDefinition/ability/passive assets and animator clips;
   finish board marker/travel/stumble and ricochet presentation. Do not run older
   character importers over approved assets.
4. Verify selection, ability animation, specials/vines/freeze, enemy death during
   cast, in-flight journal recovery, all four current zones and portrait layouts.
   Re-run the mandatory validator after final implementation.
5. Review the full diff, open/attach the PR, then merge under the user's explicit
   authorization. No PR or merge has occurred at this checkpoint.
