import hashlib
import importlib.util
from pathlib import Path
import tempfile
import unittest

spec = importlib.util.spec_from_file_location("menu_art", Path(__file__).parents[1] / "Validate-MenuArt.py")
menu_art = importlib.util.module_from_spec(spec)
spec.loader.exec_module(menu_art)


class ProtectedMenuInputsTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        self.path = self.root / "Court.prefab"
        self.repository = b"%YAML 1.1\n  m_AllTileFlags: 0\n  guid: approved\n"
        self.user_copy = self.repository.replace(b"Flags: 0", b"Flags: 1")
        self.manifest = {"format": "unity-text-lf-v1", "files": [{"path": "Court.prefab", "variants": [
            {"name": name, "sha256_lf": hashlib.sha256(data).hexdigest()}
            for name, data in (("repository", self.repository), ("preserved_local_user_copy", self.user_copy))
        ]}]}

    def verify(self):
        return menu_art.verify_preserved_files(self.root, self.manifest)

    def test_both_exact_known_states_accept_lf_and_crlf(self):
        for state, data in (("repository", self.repository), ("preserved_local_user_copy", self.user_copy)):
            for newline in (b"\n", b"\r\n"):
                with self.subTest(state=state, newline=newline):
                    self.path.write_bytes(data.replace(b"\n", newline))
                    self.assertEqual(self.verify(), {"Court.prefab": state})

    def test_unexpected_tile_flag_is_rejected(self):
        self.path.write_bytes(self.repository.replace(b"Flags: 0", b"Flags: 2"))
        with self.assertRaisesRegex(AssertionError, "Protected file changed: Court.prefab"):
            self.verify()

    def test_changed_art_reference_is_rejected(self):
        self.path.write_bytes(self.repository.replace(b"approved", b"different"))
        with self.assertRaisesRegex(AssertionError, "matches neither"):
            self.verify()

    def test_yaml_content_and_whitespace_are_not_ignored(self):
        self.path.write_bytes(self.repository.replace(b"approved", b"approved "))
        with self.assertRaises(AssertionError):
            self.verify()

    def test_missing_file_is_reported(self):
        with self.assertRaisesRegex(AssertionError, "Missing protected file: Court.prefab"):
            self.verify()

    def test_unknown_format_is_rejected(self):
        self.manifest["format"] = "ignore-unknown-changes"
        with self.assertRaisesRegex(ValueError, "Unsupported"):
            self.verify()


if __name__ == "__main__":
    unittest.main()
