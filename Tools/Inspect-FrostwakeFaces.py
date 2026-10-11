"""Read-only source packing and enlarged review; no sprite pixels are painted."""
from pathlib import Path
from PIL import Image, ImageDraw
import hashlib, json

ROOT = Path(__file__).resolve().parents[1]
BASE = ROOT / 'ArtSource/Frostwake'
OUT = BASE / 'face-revision'
OUT.mkdir(exist_ok=True)
if (OUT/'sources.json').exists():
    raise SystemExit('Baseline already captured. Preserve sources.json and packed originals; use a new review directory for another audit.')
specs = json.loads((BASE / 'production/technical-checks.json').read_text())
actors = sorted({s['name'] for s in specs})
records = []
overview = Image.new('RGB', (384 * 4, 350 * 3), '#303238')
labels = ImageDraw.Draw(overview)
for i, actor in enumerate(actors):
    still_path = ROOT / f'Assets/_Game/Art/Frostwake/{actor}.png'
    still = Image.open(still_path).convert('RGBA')
    x, y = i % 4 * 384, i // 4 * 350
    zoom = still.resize((still.width * 3, still.height * 3), Image.Resampling.NEAREST)
    overview.paste(zoom, (x, y + 24), zoom)
    labels.text((x + 8, y + 5), actor, fill='white')
    paths = [still_path] + sorted({BASE / p for s in specs if s['name'] == actor for p in s['selectedSources']})
    packet = Image.new('RGBA', (128 * 8, 104 * ((len(paths) + 7) // 8)))
    review = Image.new('RGB', (128 * 3 * 9, 126 * 3 * ((len(paths) + 8) // 9)), '#303238')
    d = ImageDraw.Draw(review)
    items = []
    for index, path in enumerate(paths):
        im = Image.open(path).convert('RGBA')
        assert im.size == (128, 104), (path, im.size)
        px, py = index % 8 * 128, index // 8 * 104
        packet.paste(im, (px, py))
        enlarged = im.resize((384, 312), Image.Resampling.NEAREST)
        rx, ry = index % 9 * 384, index // 9 * 378
        review.paste(enlarged, (rx, ry + 24), enlarged)
        d.text((rx + 4, ry + 5), f'{index}: {path.parent.parent.name}/{path.name}', fill='white')
        item = dict(index=index, path=path.relative_to(ROOT).as_posix(), sha256=hashlib.sha256(path.read_bytes()).hexdigest(), offset=[px, py])
        items.append(item)
    packet.save(OUT / f'{actor}-source.png')
    review.save(OUT / f'{actor}-audit.png')
    records.append(dict(actor=actor, items=items))
overview.save(OUT / 'before-overview.png')
(OUT / 'sources.json').write_text(json.dumps(records, indent=2))
refs = ['Docs/ArtDirection/Dungeon_Matcher_Art_Direction.txt', 'Docs/ArtDirection/Golden_Reference_Register.md', 'Docs/ArtDirection/Audit2026-10/media/skeleton-still.png', 'Docs/ArtDirection/EnemyCohesion2026-10/farmer-still.png', 'Docs/ArtDirection/EnemyCohesion2026-10/pan_villager-still.png', 'Assets/_Game/Art/CombatIdles/Rattlebones_Idle.png', 'Assets/_Game/Art/CombatIdles/Farmer_Idle.png', 'Assets/_Game/Art/CombatIdles/PanVillager_Idle.png']
hashes = {p: hashlib.sha256((ROOT / p).read_bytes()).hexdigest() for p in refs}
assert hashes[refs[2]] == '705c4c8379d56a262e287d256664db8d180a79917f4a84289bbb813730b9616c'
assert hashes[refs[3]] == '6189bc2cfe01bab84bafada24a7b129a3fd8c2225f629eb8580386f461109e2d'
assert hashes[refs[4]] == 'e1be89f8231483df5db6739dd2cae7bef83e11aaa9b028e4cb1b60ea55aba0ca'
(OUT / 'reference-hashes.json').write_text(json.dumps(hashes, indent=2))
print(json.dumps({r['actor']: len(r['items']) for r in records}))

regions = {'frostblade_reaver': [57, 42, 81, 67], 'frostfang_wolf': [11, 44, 44, 73], 'glacier_troll': [31, 25, 72, 60], 'harpoon_hunter': [49, 39, 76, 68], 'hearthkeeper': [46, 36, 76, 66], 'iceborn_champion': [50, 32, 78, 63], 'iceborn_raider': [49, 48, 77, 77], 'jarl_haldra': [50, 26, 82, 59], 'rime_skald': [48, 44, 82, 76], 'rimecarver': [51, 47, 79, 75], 'shieldmaiden': [41, 39, 73, 70]}
face_review = Image.new('RGB', (460 * 4, 420 * 3), '#303238')
fd = ImageDraw.Draw(face_review)
for i, actor in enumerate(actors):
    im = Image.open(ROOT / f'Assets/_Game/Art/Frostwake/{actor}.png').convert('RGBA')
    region = regions[actor]
    crop = im.crop(region); enlarged = crop.resize((crop.width * 10, crop.height * 10), Image.Resampling.NEAREST)
    x, y = (i % 4) * 460, (i // 4) * 420
    face_review.paste(enlarged, (x + 30, y + 40), enlarged)
    fd.text((x + 5, y + 5), actor, fill='white')
    for xx in range(crop.width):
        if (xx + region[0]) % 2 == 0: fd.text((x + 29 + xx * 10, y + 24), str(xx + region[0]), fill='#aab1bb')
    for yy in range(crop.height):
        if (yy + region[1]) % 2 == 0: fd.text((x + 7, y + 38 + yy * 10), str(yy + region[1]), fill='#aab1bb')
face_review.save(OUT / 'face-detail-audit.png')
