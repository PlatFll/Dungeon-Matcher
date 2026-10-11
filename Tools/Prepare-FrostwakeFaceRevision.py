"""Inspect material regions and prepare guarded PixelLab decisions, never paint locally."""
from pathlib import Path
from PIL import Image, ImageDraw
import json, numpy as np

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'ArtSource/Frostwake/face-revision'
REGIONS = {'frostblade_reaver': [58, 54, 78, 66], 'frostfang_wolf': [17, 49, 33, 64], 'glacier_troll': [43, 28, 67, 49], 'harpoon_hunter': [52, 48, 71, 63], 'hearthkeeper': [52, 43, 73, 60], 'iceborn_champion': [55, 42, 75, 54], 'iceborn_raider': [53, 55, 73, 72], 'jarl_haldra': [53, 35, 76, 52], 'rime_skald': [52, 48, 79, 66], 'rimecarver': [56, 50, 75, 65], 'shieldmaiden': [47, 40, 66, 54]}
records = json.loads((OUT / 'sources.json').read_text())
locations = {}
for actor in records:
    name = actor['actor']; items = actor['items']; region = REGIONS[name]
    packed = Image.open(OUT / f'{name}-source.png').convert('RGBA')
    original = packed.crop((0,0,128,104))
    template = np.asarray(original.crop(region))
    h, w = template.shape[:2]
    mask = template[:, :, 3] > 0
    # Exact-color agreement proposes a translation only; inspect every contact sheet.
    packets = []
    atlas = Image.new('RGB', (220 * 9, 250 * ((len(items) + 8) // 9)), '#303238')
    draw = ImageDraw.Draw(atlas)
    for item in items:
        px,py=item['offset']; im = packed.crop((px,py,px+128,py+104)); arr = np.asarray(im)
        best = (-1, 0, 0)
        for y in range(max(0, region[1]-35), min(104-h, region[1]+39)+1):
            for x in range(max(0, region[0]-40), min(128-w, region[0]+40)+1):
                part = arr[y:y+h, x:x+w]
                score = np.mean(np.all(part == template, axis=2)[mask]) - .0001 * (abs(x-region[0])+abs(y-region[1]))
                if score > best[0]: best = (float(score), x, y)
        score, x, y = best
        left, top = max(0,x-5),max(0,y-7)
        crop = im.crop((left,top,min(128,x+w+5),min(104,y+h+8)))
        shown = crop.resize((crop.width*5, crop.height*5), Image.Resampling.NEAREST)
        ax, ay = item['index'] % 9 * 220, item['index'] // 9 * 250
        atlas.paste(shown,(ax+15,ay+35),shown)
        draw.text((ax+5,ay+4), f"{item['index']}: {Path(item['path']).parent.parent.name}", fill='white')
        draw.text((ax+5,ay+17), f"at {x},{y} match {score:.2f}",fill='#aab1bb')
        for yy in range(crop.height):
            if (top+yy)%4==0:draw.text((ax,ay+33+yy*5),str(top+yy),fill='#aab1bb')
        packets.append(dict(index=item['index'],position=[x,y],offset=[x-region[0],y-region[1]],score=score))
    atlas.save(OUT/f'{name}-faces.png')
    locations[name]=packets
(OUT/'face-locations.json').write_text(json.dumps(locations,indent=2))
print('Face translation suggestions and every-source face atlases saved for inspection.')
