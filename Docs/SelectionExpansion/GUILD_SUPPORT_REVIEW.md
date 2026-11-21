# Guild support in combat

The Road Escort is the Guild Bulwark. The approved sprite is bound to the same
enemy definition that owns Taunt. The Chirurgeon uses Field Medicine. Both act
through the production move clock and deliver one effect at the staff or shield
contact of their approved special clip.

| Interaction | Gameplay result | Evidence |
|---|---|---|
| Bulwark starts Taunt | Requires a living ally, an immediately matchable Bulwark weakness and an available special opportunity. The next three accepted moves are protected. | Guild Game-scene diagnostic, move-count regressions |
| Same or different ally weakness | Each protected ally keeps its gem type. Matching an ally's color does not convert damage to a different color. If the Bulwark shares it, only its ordinary hit is applied. | Two fixed-clear combat regressions and the Game-scene formation |
| Row, column, color and chained clears | CombatController selects eligible recipients for each clear. A later clear can see the release after the Bulwark dies. | Bomb-source regression and production clear-path inspection. Every chain permutation has not been separately played against the Bulwark. |
| Targeted and sweeping attacks | Cracked Gems, Royal Decree and Ricochet consult actor eligibility. Direct damage checks protection again at contact. | Production call paths and the blocked direct hit in the Game scene |
| Existing versus new poison | A protected ally rejects new poison. Status damage already applied resolves through the existing periodic damage route. | Poison application and quiet-damage regression |
| Actor death and Stagger | The Bulwark releases its allies when defeated or staggered. A lone Bulwark clears its own Taunt. | Source death in the Game scene, Stagger and wave cleanup handlers |
| Continue | The Bulwark persists its remaining moves and the last settled move. Allies keep their original saved gem types. | Restored snapshots at three, two and one move remaining |
| Taunt presentation | Ally weakness facets disappear in their existing slots. The attacking party, health bars and underlying board stay in place. | Actual [before](GameScene/bulwark-idle.png) and [during](GameScene/bulwark-taunt.png) captures |
| Field Medicine | Chooses the other living ally with the lowest HP fraction, breaking ties by slot and identity. At contact it heals up to 10 missing HP. It keeps its charge when nobody needs healing. | Wounded-target and clamping regression, accepted-swap Game-scene diagnostic |
| Cancelled heal | Losing the recipient before contact fizzles the reserved heal. Successful contact resets cooldown and consumes the commanded basic opportunity. | Production action, target validation and charge reset inspected. No separate natural cancellation sample is claimed. |

The gallery exercises the fallback sprite, pause, resumed playback, reduced motion,
hit and defeat on actual actors. Its forced formations and prepared board are
diagnostic evidence. Natural run measurements are recorded separately.
