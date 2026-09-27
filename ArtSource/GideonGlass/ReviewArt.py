"""Read-only pixel audit and comparison renders. Never changes source sprites."""
from pathlib import Path
from collections import Counter
import json
import sys
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parent
PROJECT = ROOT.parents[1]
name = sys.argv[1] if len(sys.argv) > 1 else 'Gideon_Approved_03'
target = ROOT / 'Candidates' / (name + '.png')
im = Image.open(target).convert('RGBA')
palette = set(bytes.fromhex(c) for c in '0A0D11 755335 B18A4B DAC080 25212D 443949 685565 54283D 894254 24425C 397BAB A9E0FF B7A393 FDF5E5 5B3627 8E7868'.split())
pixels = list(im.get_flattened_data())
alpha = Counter(p[3] for p in pixels)
report = {'size': im.size, 'bounds': im.getbbox(), 'colors': sorted(set('#%02X%02X%02X' % p[:3] for p in pixels if p[3])),
          'alpha': dict(alpha), 'off_palette': sum(1 for p in pixels if p[3] and bytes(p[:3]) not in palette)}
out = Image.new('RGB', (1280, 740), '#24212d')
d = ImageDraw.Draw(out)
names = ['Rattlebones', 'Farmer', 'PanVillager', 'Bardley']
frames = [Image.open(PROJECT / 'Assets/_Game/Art/CombatIdles' / (n+'_Idle.png')).convert('RGBA').crop((0,0,64,64)) for n in names] + [im]
for i, f in enumerate(frames):
    d.text((i*256+6, 6), (names+['Gideon candidate'])[i], fill='white')
    for row, bg in enumerate(['#161420', '#ddd8ce']):
        tile = Image.new('RGBA', (64,64), bg)
        tile.alpha_composite(f)
        out.paste(tile.resize((256,256),Image.Resampling.NEAREST), (i*256,row*290+30))
        out.paste(tile, (i*256+12,620))
    d.text((i*256+12,690), 'Native 1x; identical 64px canvas', fill='white')
out.save(ROOT/'Candidates'/(name+'_review.png'))
(ROOT/'Candidates'/(name+'_audit.json')).write_text(json.dumps(report,indent=2))
print(json.dumps(report))
