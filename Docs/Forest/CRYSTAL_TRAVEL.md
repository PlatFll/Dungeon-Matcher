# Crystal travel and vine presentation

The user approved connecting dungeon and forest travel on 2026-10-03. New normal
runs default to the dungeon. The temporary testing picker can instead start a
fresh live run in the magical forest, with seconds effects and move-based
abilities. Continue retains its saved zone; travel destinations remain random.
Isolated Editor forest fixtures retain their separate
save and loop; they do not opt into live travel automatically.
Older forest checkpoints without travel metadata also retain that isolated loop;
schema zero distinguishes Unity's default nested JSON object from a travel save.

## Encounters and destinations

The initial kingdom progression and King window are unchanged. A zone specifies
its apex enemy explicitly. Travel begins only after the entire formation dies,
the board and enemy cleanup settle, rewards are recorded, and any card choice
finishes. Boss rank alone cannot trigger travel.

The destination stream is separate from encounter and card randomness. It picks
uniformly among eligible regions other than the current one and saves the result
before the presentation begins. With the two available regions this alternates
dungeon and forest. Aquatic is unavailable. No destination choice is shown.

Global wave/depth continues. Forest currently uses eighteen local encounters from
authored bands, with Warden checkpoints at 8 and 14 and a Matriarch formation at
18. The bands vary escorts and include relief encounters. Dungeon returns use
the existing depth-appropriate weighted pool, then a King rematch at local wave
18 with one of three escort formations. The initial kingdom is not compressed.
These are starter-content tuning anchors, not proof of a complete ten-minute
visit or a finalized full forest roster. There is no waiting timer, player-power
scaling, HP padding, run cap or automatic heal.

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
removes source-zone vines, roots and crumbling holes before reveal; it fills
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
