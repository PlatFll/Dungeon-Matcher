"""Validate native menu assets and documented protected-file states; emit a report."""
import argparse
import hashlib
import json
from pathlib import Path
import struct
import zlib
from PIL import Image

ROOT = Path(__file__).resolve().parent.parent


def verify_preserved_files(root, manifest):
    """Accept only the exact recorded Git or original user-local YAML state.

    Git checks out Unity text with CRLF on Windows and LF elsewhere. Normalize
    that encoding difference only; flags, references and all other bytes matter.
    The original task-local snapshot remains immutable provenance, not the sole
    valid baseline for every future clone.
    """
    if manifest.get("format") != "unity-text-lf-v1":
        raise ValueError("Unsupported protected-file checksum format")
    matched = {}
    for entry in manifest["files"]:
        path = root / entry["path"]
        if not path.is_file():
            raise AssertionError(f"Missing protected file: {entry['path']}")
        actual = hashlib.sha256(path.read_bytes().replace(b"\r\n", b"\n")).hexdigest()
        variant = next((v for v in entry["variants"] if v["sha256_lf"] == actual), None)
        if variant is None:
            raise AssertionError(
                f"Protected file changed: {entry['path']}\n"
                f"SHA-256 (LF): {actual}\n"
                "It matches neither the committed nor the preserved user-local state. "
                "Inspect the diff; do not overwrite the asset or refresh hashes automatically."
            )
        matched[entry["path"]] = variant["name"]
    return matched

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

def validate_menu_art(root, report_path=None):
    source = root / "ArtSource/MenuUI"
    target = root / "Assets/_Game/Resources/UI/GlobalMenu"
    report = []
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

    # Load the versioned contract alongside this validator, even when validating
    # another checkout via --root. Do not learn expected values from that target.
    manifest = json.loads((ROOT / "ArtSource/MenuUI/validation/preserved-file-variants.json").read_text())
    matched = verify_preserved_files(root, manifest)
    assert len(matched) == 4, len(matched)
    assert len(report) == 54, len(report)
    destination = report_path or source / "validation/native-checks.json"
    destination.write_text(json.dumps(report, indent=2) + "\n")
    print(f"PASS: {len(report)} native ASE/PNG pairs, binary alpha, matching Unity exports/imports; "
          f"{len(matched)} protected files match documented states.")
    for path, variant in matched.items():
        print(f"  {variant}: {path}")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--root", type=Path, default=ROOT)
    parser.add_argument("--report", type=Path, help="Optional report destination outside the checkout")
    args = parser.parse_args()
    validate_menu_art(args.root, args.report)
