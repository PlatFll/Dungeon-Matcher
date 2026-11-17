# Crystal travel and vine presentation

The user approved connecting dungeon and forest travel on 2026-10-03. New normal
runs default to the dungeon. The temporary testing picker can instead start a
fresh live run in any ready zone, with the unified accepted-move profile. Continue retains its saved zone; travel destinations remain random.
Isolated Editor forest fixtures retain their separate
save and loop; they do not opt into live travel automatically.
Older forest checkpoints without travel metadata also retain that isolated loop;
schema zero distinguishes Unity's default nested JSON object from a travel save.

## Encounters and destinations

Live travel-enabled runs use Crystal Resonance in all five regions: Dungeon,
Magical Forest, Drowned Court, Ironvein Excavation and Frostwake Peaks. A complete
selected current-zone capstone and 100 banked charge are both required. Board,
enemy actions, move clock, victory journal and sequential card drafts must settle.
Legacy apex/local-wave triggers are bypassed for these runs. No forced King,
Dungeon visit or major-boss-per-visit requirement remains.

Global depth continues. The saved destination stream picks uniformly among ready
regions other than the current one. It is separate from encounters, cards,
board refill and capstone selection. The destination is saved before cover.
Depth/member/formation data lives in ZoneDefinition; see
[the current contract](../PROGRESSION_PACING.md). Existing fixtures without live
travel retain their historical loops. No travel heal, run cap or minimum timer.

Nested Resonance schema 1 carries score and receipt cursor through arrival; only
the detached destination snapshot resets local charge/notches/capstone/timers.
Failed checkpoint writes preserve the source. A legacy already-pending trip is
honored once. Other old live saves start local Resonance at zero and preserve
combat/clock/resources; see [migration](../CrystalResonance/SAVE_MIGRATION.md).

## Durable handoff

`ZoneTravelController` holds the existing wave-progression gate. Its snapshot
stores current zone, visit, visit start wave, random state, destination and stage.

1. Pending: save the completed source encounter and selected destination.
2. Cover: enlarge/shake the approved split pink story gem and emit pink motes;
   opaque smoke covers gameplay. Combat time and input are held.
3. Commit: validate destination assets, then atomically store a destination
   checkpoint with the same player, board, build, supplies and run identity.
4. Restore: reload the existing Game scene under the persistent smoke canvas.
   Normal scene initialization binds destination environment, UI and music;
   the existing continuation owners restore gameplay.
5. Reveal: fade the smoke and destination title, save the completed transition,
   and release the wave gate for the next global wave.

Settings and opened options remain above the smoke. Opening settings pauses
presentation. Suspend preserves pending or committed travel; a pending trip uses
the saved destination, and a committed trip resumes its reveal. Neither repeats
the reward. Destination validation failure leaves the source run playable. A save
failure retains the gate and existing Retry Save control. A restore failure uses
the existing recovery UI while the durable destination checkpoint is preserved.

HP, shield, energy, cards, supply counts/cooldowns, ordinary gems, player-created
specials and their board identities carry over. Environmental vines persist
through ordinary waves and perk choices. The atomic destination checkpoint
removes source-zone vines, roots, frost, mine/court hazards, warnings and crumbling holes before reveal; it fills
reopened cells with safe ordinary gems from the refill stream. That stream is
otherwise unchanged. Existing producer cleanup removes old enemies and their
owned hazards. Failed writes leave the source board untouched.
Gideon's photograph retains its existing encounter-end cancellation rule.

The run retains its effect clock profile. Existing forest saves keep move-based
effects. A legacy dungeon run gains accepted-move ability coordination on first
travel using `seconds-effects-move-abilities-v1`: ordinary attacks, buffs,
stagger, poison and supplies keep seconds; forest channels and readiness count
accepted moves. No seconds value is interpreted as a move count.

## Art

Treant C (Old Stump) and the other six expanded-roster designs now have their
approved motion and kits; see [the roster contract](EXPANDED_ROSTER.md).

The two root sprites track remaining durability, retaining the existing pips.
The selected dense Vines B weave is 17.7% transparent. Native spread/recoil clips consume board
presentation cues and never own damage, occupancy, growth or refill. Restore
shows static vines; reduced motion skips the transient clips and crystal shake.
Transition sources remain in `ArtSource/Forest/VinesAndTransition`; selected
blockers and dense vine motion are in `ArtSource/Forest/RosterProduction`.

Validation evidence is recorded separately after the implementation gates run.
Final forest music approval, expanded content and physical-device testing remain
open production items.
