# Temporary starting-zone picker — 2026-10-03

Implemented on `codex/testing-zone-picker` from main `8924ff3`. The user
explicitly requested implementation and merge. The original Unity checkout and
its three existing consumable metadata changes were preserved.

## Behavior

- New-run Play opens a temporary Dungeon / Magical Forest choice. Character
  select, free practice and unlocked challenges use the same screen.
- Only ready live zones are offered. Back returns to the originating menu and
  clears pending launch options without creating a run or spending supplies.
- The selected zone is a one-use scene handoff. Continue always restores its
  checkpoint and skips the picker. Existing isolated forest saves retain their
  recorded loop and timing profile.
- Forest starts use live encounters and crystal travel, with seconds for basics
  and effects and accepted moves for special abilities. Dungeon retains its
  original opening generator. Retry starts the current live zone at wave one.
- `RunLaunchOptions.TestingZonePickerEnabled` disables the temporary picker and
  same-zone Retry behavior for release. No new saved setting/schema is added.

## Executed validation

Unity **6000.3.19f1**, Windows Editor:

| Check | Result | Evidence |
| --- | --- | --- |
| New picker and affected launch/travel/continuation paths | **17 passed, 0 failed, 0 skipped** | [Test XML](TestingZonePicker/740d4478-b7b7-43fe-9723-3d0df473bd01.xml) |
| Required `Tools/Validate-Unity.ps1` | **Passed, exit 0** | [Unity log](TestingZonePicker/unity-validator.txt) |
| Actual menu pointer input and portrait rendering | Passed | [720×1280](TestingZonePicker/picker-720x1280.png), [1080×2400](TestingZonePicker/picker-1080x2400.png) |

Seven new cases cover cancel/duplicate clicks, character-select Back, both starting
zones, forest Continue with exact board carryover, same-zone Retry, practice with
a saved normal attempt, challenge preservation and an unavailable-zone fallback.
Buttons receive actual EventSystem raycast/pointer clicks. Layout checks verify
screen containment, no TMP overflow and the shared Thaleah font; captures were
also visually inspected.

Existing cases cover dungeon→forest→dungeon travel, old isolated forest saves,
seconds-based effects, channel continuation and unsupported-save preservation.
The original test XML lists each executed case. Tests use disposable accounts;
no real account progression was changed. There were no failed attempts in this
change's test run. Physical-device testing was not performed.

Runtime edits are limited to `MainMenuController`, `RunLaunchOptions` and
`RunSession`. Scene/prefab bindings, combat rules, art and random destination
selection are unchanged.
