# Menu-art preservation guard — 2026-11-08

The original `starting-user-files.json` is a snapshot of four **uncommitted**
files in the user's working project during the original menu task. It was not a
checksum of the committed asset library. Its four hashes still match the user's
preserved files in `C:/UnityProjects/Dungeon Matcher` exactly.

The committed `DrownedCourt_Throne.prefab` has not changed since `ba577b881811`.
Compared with it, the local user copy has four `m_AllTileFlags` values set to 1
instead of 0. The three local consumable metadata files retain Unity's spaces
after empty YAML fields. A clean checkout has the committed versions and may
also use CRLF instead of LF. The old byte-only, single-copy assertion therefore
failed even without any change to underwater art.

The user explicitly requested correction of the guard without editing approved
underwater art. `preserved-file-variants.json` records both exact, verified
pre-existing states with their provenance. The validator accepts only one of
these two states per file and normalizes **CRLF to LF only**. It does not ignore
flags, GUIDs, content, trailing spaces, missing files or unknown changes. It does
not learn/update expected hashes from the checkout being tested. The historical
snapshot is preserved unchanged. No prefab, texture, metadata or scene is edited.

Validation:

- The complete menu-art check passes all 54 native ASE/PNG/import contracts and
  all four protected-file contracts in both the clean branch and the existing
  user project (using `--root` and an external `--report`).
- Six `Tools/tests/test_validate_menu_art.py` regression tests pass: both known
  states under LF/CRLF, unexpected tile flags, altered art GUIDs, changed YAML
  whitespace, missing files, and unsupported manifest formats. Initial sandbox
  execution could not write temporary files; the unchanged tests passed when
  run with normal filesystem access.
- `git diff -- Assets` is empty. No approved underwater art changes.
- This is Python validation/documentation work only. No C# or Unity serialized
  assets changed, so Unity compilation/runtime tests are not required for this
  fix. The new flood-effect review is separate and is not part of this merge.

Run `python Tools/Validate-MenuArt.py` with Pillow installed. For a separate
checkout: `python Tools/Validate-MenuArt.py --root <checkout> --report <output>`.
Run regression tests with `python -m unittest discover -s Tools/tests -p test_validate_menu_art.py -v`.
