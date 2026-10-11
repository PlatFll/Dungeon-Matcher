"""Read-only native asset checks; run from the repository root with Pillow installed."""
import hashlib
import json
from pathlib import Path
import struct
import zlib
from PIL import Image

root = Path(__file__).resolve().parent.parent
source = root / "ArtSource/MenuUI"
target = root / "Assets/_Game/Resources/UI/GlobalMenu"
report = []

def ase_pixels(path):
    data = path.read_bytes()
    magic, frames, width, height, depth = struct.unpack_from("<5H", data, 4)
    assert (magic, frames, depth) == (0xA5E0, 1, 32), path
    image = Image.new("RGBA", (width, height))
    offset, cels = 144, 0
    while offset < len(data):
        length, kind = struct.unpack_from("<IH", data, offset)
        if kind == 0x2005:
            payload = offset + 6
            layer, x, y, opacity, cel_type = struct.unpack_from("<HhhBH", data, payload)
            assert (layer, opacity, cel_type) == (0, 255, 2), path
            w, h = struct.unpack_from("<HH", data, payload + 16)
            raw = zlib.decompress(data[payload + 20:offset + length])
            image.paste(Image.frombytes("RGBA", (w, h), raw), (x, y))
            cels += 1
        offset += length
    assert cels == 1, path
    return image

for path in sorted((source / "native").glob("*.png")):
    im = Image.open(path).convert("RGBA")
    ase = ase_pixels(path.with_suffix(".aseprite"))
    assert im.size == ase.size and im.tobytes() == ase.tobytes(), path
    assert path.read_bytes() == (target / path.name).read_bytes(), path
    alpha = sorted(set(im.getchannel("A").tobytes()))
    assert set(alpha) <= {0, 255}, (path, alpha)
    meta = (target / (path.name + ".meta")).read_text()
    assert "overridden: 1" not in meta, path
    for field in ("enableMipMap: 0", "filterMode: 0", "nPOTScale: 0", "spritePixelsToUnits: 100", "textureCompression: 0"):
        assert field in meta, (path, field)
    report.append(dict(asset=path.stem, width=im.width, height=im.height,
                       visible_colors=sum(1 for _, p in im.getcolors(im.width * im.height) if p[3]), alpha=alpha,
                       sha256=hashlib.sha256(path.read_bytes()).hexdigest(),
                       ase_pixels_match=True, production_bytes_match=True))

starting = json.loads((source / "validation/starting-user-files.json").read_text())
for path, expected in starting.items():
    assert hashlib.sha256((root / path).read_bytes()).hexdigest() == expected, path
assert len(report) == 54, len(report)
(source / "validation/native-checks.json").write_text(json.dumps(report, indent=2) + "\n")
print(f"PASS: {len(report)} native ASE/PNG pairs, binary alpha, matching Unity exports/imports; four pre-existing files unchanged.")
