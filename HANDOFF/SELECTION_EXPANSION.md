# Selection repair and non-boss expansion

The feature is stacked on the published PR #234 source at
7e306f72b26ad5b9bb2458cf973bb73cc7dcbc07. Stages 00–07 produced the source
audit, pool repairs, phase selection and recurrence, ten enemy definitions,
Guild support mechanics, approved character art and Unity motion bindings.
K06 Road Escort is the Guild Bulwark.

The [approved animation update](../Docs/SelectionExpansion/GameScene/ApprovedMotion/README.md) binds the revised idles and actions in the Game scene and verifies a single Slinghunter hit against health and shield. The earlier build and natural-run results below describe the pre-animation source.

The final production code and definitions are at
f3ed12bd8806b7874749d4f05f3d523f14398266. The required Unity validator,
278 balance tests, the affected Crystal Resonance and Game-scene checks,
Windows and Android development builds, and ten natural policy runs completed.
Eight runs reached their planned observation limit alive and two ended in
defeat. Together they completed 21 visits. The native audit matched all 2,442
accepted actions. All owned runners have exited.

| Review | Evidence |
|---|---|
| Approved stills and motion | [Decision ledger](../ArtSource/SelectionExpansion/ART_STILL_REVIEW/decisions.json) and [animation review](../ArtSource/SelectionExpansion/Motion/Review/Review.html) |
| All ten enemies in Unity | [Game-scene lineup](../Docs/SelectionExpansion/GameScene/formation-gallery.png) |
| Enemy stats and assets | [Serialized roster](../Docs/SelectionExpansion/production-roster.csv) |
| Guild Taunt and Field Medicine | [Mechanics and counterplay](../Docs/SelectionExpansion/GUILD_SUPPORT_REVIEW.md) |
| Authored and measured selection | [Encounter matrix](../Docs/CrystalResonance/ENCOUNTER_MATRIX.md) and [per-enemy exposure](../Docs/SelectionExpansion/Validation/ordinary-exposure.csv) |
| Stage 08 checks | [Technical results](../Docs/SelectionExpansion/Validation/TECHNICAL.md) and [natural Game runs](../Docs/SelectionExpansion/Validation/NATURAL_RUNS.md) |

Stage 09 packages this evidence for a stacked draft PR. The remaining human
checks are phone readability of the shield call and staff heal, beginner Court
oxygen pressure, and the length of deep Dungeon cleanup. Both Guild specialists
were demonstrated in production Game-scene fixtures. A natural Guild return
included the Ranger and Swordsman.
