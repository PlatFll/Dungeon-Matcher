"""Read-only pose inspection. Does not create or edit sprite art.

Usage: python check_polish.py --candidate <Review/Polished> --baseline <old sources>
Optional --renders <temporary directory> writes labeled inspection composites.
"""
from pathlib import Path
import argparse
import json
import sys
from collections import deque

PROJECT = Path(__file__).resolve().parents[3]
sys.path.insert(0, str(PROJECT / "ArtSource/CombatIdles/Scripts"))
from inspect_idles import read_ase
from PIL import Image, ImageDraw

CLIPS = ["Miner_Idle", "BasketVillager_Idle", "BarricadeVillager_Idle",
         "BasketVillager_AutoAttack", "BarricadeVillager_AutoAttack",
         "BarricadeVillager_Ability"]


def components(im):
    remaining = {(x, y) for y in range(im.height) for x in range(im.width)
                 if im.getpixel((x, y))[3]}
    result = []
    while remaining:
        first = remaining.pop()
        queue = deque([first])
        group = [first]
        while queue:
            x, y = queue.popleft()
            for dx, dy in [(0, 1), (0, -1), (1, 0), (-1, 0),
                           (1, 1), (1, -1), (-1, 1), (-1, -1)]:
                near = x + dx, y + dy
                if near in remaining:
                    remaining.remove(near)
                    queue.append(near)
                    group.append(near)
        if len(group) >= 30:
            result.append({"pixels": len(group), "bounds": [
                min(x for x, y in group), min(y for x, y in group),
                max(x for x, y in group), max(y for x, y in group)]})
    return sorted(result, key=lambda c: c["pixels"], reverse=True)


def render(frames, durations, name, folder):
    w, h = frames[0].size
    tw, th = 3 * w + 12, 3 * h + 28
    image = Image.new("RGB", (4 * tw, ((len(frames) + 3) // 4) * th + 26), "#292632")
    draw = ImageDraw.Draw(image)
    draw.text((6, 5), name, fill="white")
    for n, (frame, ms) in enumerate(zip(frames, durations)):
        x, y = n % 4 * tw + 6, n // 4 * th + 48
        draw.text((x, y - 17), f"{n + 1}: {ms} ms", fill="white")
        scaled = frame.resize((3 * w, 3 * h), Image.Resampling.NEAREST)
        image.paste(scaled, (x, y), scaled)
        draw.line((x, y + 3 * h, x + 3 * w - 1, y + 3 * h), fill="#625573")
    image.save(folder / f"{name}.png")


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--candidate", type=Path, required=True)
    parser.add_argument("--baseline", type=Path, required=True)
    parser.add_argument("--renders", type=Path)
    args = parser.parse_args()
    palettes = json.loads((Path(__file__).parent / "palettes.json").read_text())
    report = {}
    failures = []
    if args.renders:
        args.renders.mkdir(parents=True, exist_ok=True)
    for name in CLIPS:
        frames, durations = read_ase(args.candidate / f"{name}.aseprite")
        before, old_durations = read_ase(args.baseline / f"{name}.aseprite")
        assert len(frames) == len(before) and durations == old_durations, name
        assert all(f.size == b.size for f, b in zip(frames, before)), name
        assert frames[0].tobytes() == before[0].tobytes(), name + " first ready"
        assert frames[-1].tobytes() == before[-1].tobytes(), name + " last ready"
        approved = set(palettes[name.split("_")[0]])
        changed, detached = {}, {}
        for n, (frame, old) in enumerate(zip(frames, before)):
            bad_colors = {"%02X%02X%02X" % p[:3] for p in frame.getdata()
                          if p[3] and "%02X%02X%02X" % p[:3] not in approved}
            assert not bad_colors, (name, n + 1, bad_colors)
            assert all(p[3] in (0, 255) for p in frame.getdata()), (name, n + 1)
            assert frame.getbbox()[3] == frame.height, (name, n + 1, "ground")
            cs = components(frame)
            if len(cs) > 1:
                detached[n + 1] = cs
                failures.append(f"{name} frame {n + 1}: detached solid component")
            diffs = [(x, y) for y in range(frame.height) for x in range(frame.width)
                     if frame.getpixel((x, y)) != old.getpixel((x, y))]
            if diffs:
                changed[n + 1] = {"count": len(diffs), "bounds": [
                    min(x for x, y in diffs), min(y for x, y in diffs),
                    max(x for x, y in diffs), max(y for x, y in diffs)]}
            if name == "Miner_Idle":
                assert all(n in (2, 7) and 33 <= x <= 41 and y == 30 for x, y in diffs)
            if name == "BarricadeVillager_Idle":
                assert all(2 <= n <= 7 and 24 <= x <= 44 and 56 <= y <= 59 for x, y in diffs)
            if name == "BarricadeVillager_AutoAttack":
                assert all(n in (1, 3, 6) and 40 <= x <= 60 and 56 <= y <= 59 for x, y in diffs)
            if name == "BarricadeVillager_Ability":
                assert all(2 <= n <= 6 and 49 <= x <= 65 and 58 <= y <= 63 for x, y in diffs)
            if name == "BasketVillager_Idle":
                assert all(2 <= n <= 7 and x <= 24 and y >= 44 for x, y in diffs)
                # Boots retain their exact native pixels while the basket may lift.
                assert frame.crop((25, 58, 64, 64)).tobytes() == old.crop((25, 58, 64, 64)).tobytes()
        report[name] = {"frames": len(frames), "duration_ms": sum(durations),
                        "changes": changed, "detached": detached}
        if args.renders:
            render(frames, durations, name, args.renders)
    result = {"passed": not failures, "failures": failures, "clips": report}
    print(json.dumps(result, indent=2))
    if args.renders:
        (args.renders / "checks.json").write_text(json.dumps(result, indent=2))
    if failures:
        raise SystemExit(1)


if __name__ == "__main__":
    main()
