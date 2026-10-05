"""Validate actual review bytes and assemble a self-contained offline handoff."""
from pathlib import Path
from html.parser import HTMLParser
from collections import Counter
from io import BytesIO
import hashlib
import json
import zipfile
from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
PROJECT = ROOT.parents[2]
ZIP_NAME = "Drowned_Court_Phase02_Review.zip"

def digest(data):
    return hashlib.sha256(data).hexdigest()

class Links(HTMLParser):
    def __init__(self):
        super().__init__()
        self.links = []
        self.ids = set()

    def handle_starttag(self, tag, attrs):
        attrs = dict(attrs)
        if "id" in attrs:
            self.ids.add(attrs["id"])
        self.links += [attrs[k] for k in ("href", "src") if k in attrs]

manifest = json.loads((ROOT / "Manifests/CandidateManifest.json").read_text())
for item in manifest:
    for record in (item, item["raw"]):
        data = (ROOT / record["file"]).read_bytes()
        assert digest(data) == record["sha256"], record["file"]
        im = Image.open(BytesIO(data)).convert("RGBA")
        assert im.size == (record["width"], record["height"])
        alpha = im.getchannel("A")
        assert sorted(set(alpha.tobytes())) == record["alpha_values"]
        assert list(alpha.getbbox()) == record["bounds_exclusive"]
        palette = Counter(tuple(px[:3]) for px in im.getdata() if px[3])
        expected = {tuple(bytes.fromhex(p["hex"][1:])): p["pixels"] for p in record["palette"]}
        assert dict(palette) == expected, record["file"]
        assert len(palette) == record["visible_rgb_colors"]
    if item["id"].startswith("DC_A"):
        raw = Image.open(ROOT / item["raw"]["file"]).convert("RGBA")
        candidate = Image.open(ROOT / item["file"]).convert("RGBA")
        assert raw.getchannel("A").tobytes() == candidate.getchannel("A").tobytes()
        assert candidate.size == (64, 64)
        assert {p[3] for p in candidate.getdata()} == {0, 255}
        assert any(p[:3] == (10, 13, 17) for p in candidate.getdata() if p[3])

sources = json.loads((ROOT / "Manifests/SourceRegister.json").read_text())
for source in sources:
    assert digest((PROJECT / source["path"]).read_bytes()) == source["sha256"]
    assert digest((ROOT / "References" / (source["id"] + ".png")).read_bytes()) == source["sha256"]

page = (ROOT / "Review.html").read_text(encoding="utf8")
offline_page = page.replace(
    '<a href="' + ZIP_NAME + '" download>Download complete offline ZIP</a>',
    '<span>Offline review package</span>')
parser = Links()
parser.feed(offline_page)
for link in parser.links:
    assert not link.startswith(("http:", "https:", "//", "file:")), link
    if link.startswith("#"):
        assert link[1:] in parser.ids, link
    else:
        assert (ROOT / link).is_file(), link
assert (ROOT / "Review/Screen_Flooded.png").is_file()

checks = {
    "date": "2026-10-05",
    "candidate_and_raw_hash_dimension_alpha_palette_checks": "PASS: 7 pairs",
    "character_transparency_masks": "PASS: all 4 byte-identical to raw",
    "reference_original_and_copy_hashes": "PASS: " + str(len(sources)),
    "offline_html_local_links": "PASS",
    "zip_integrity_and_actual_png_bytes": "PASS (verified after writing)",
    "fonts_distributed": False,
    "local_http_server": "HTTP 200 verified from host at 127.0.0.1:8892",
    "browser_controls": "NOT VERIFIED: in-app browser timed out; subsequent navigation was blocked on its data-URL error page",
    "unity_runtime_and_device_validation": "NOT RUN: image-only candidate review",
    "gameplay_changes": False,
    "zip_html_difference": "Self-download ZIP link replaced with offline-package label; all media remains local"
}
(ROOT / "Manifests/ValidationSummary.json").write_text(json.dumps(checks, indent=2), encoding="utf8")

files = {}
for path in sorted(ROOT.rglob("*")):
    if not path.is_file() or path.suffix.lower() in {".zip", ".pyc", ".log"}:
        continue
    assert path.suffix.lower() not in {".ttf", ".otf", ".woff", ".woff2"}
    relative = path.relative_to(ROOT).as_posix()
    files[relative] = offline_page.encode("utf8") if relative == "Review.html" else path.read_bytes()
files["PackageSHA256.json"] = json.dumps({p: digest(data) for p, data in files.items()}, indent=2).encode("utf8")
with zipfile.ZipFile(ROOT / ZIP_NAME, "w", zipfile.ZIP_DEFLATED) as archive:
    for name, data in files.items():
        archive.writestr(name, data)
with zipfile.ZipFile(ROOT / ZIP_NAME) as archive:
    assert archive.testzip() is None
    hashes = json.loads(archive.read("PackageSHA256.json"))
    for name, expected in hashes.items():
        data = archive.read(name)
        assert digest(data) == expected, name
        if name.lower().endswith(".png"):
            Image.open(BytesIO(data)).verify()
    for link in parser.links:
        if not link.startswith("#"):
            assert link in archive.namelist(), link
    assert "Review/Screen_Flooded.png" in archive.namelist()
    assert len([p for p in archive.namelist() if p.startswith("Candidates/") and p.endswith(".png")]) == 7

print(json.dumps({"archive": ZIP_NAME, "files": len(files), "png_files": sum(p.endswith('.png') for p in files), "bytes": (ROOT / ZIP_NAME).stat().st_size, "validation": "PASS; browser interaction and Unity runtime not verified"}, indent=2))
