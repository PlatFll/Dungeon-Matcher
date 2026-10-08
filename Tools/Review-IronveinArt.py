"""Measure unchanged source PNGs and make nearest-neighbor review composites."""
from pathlib import Path
import hashlib
import json
import argparse
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[1]
parser=argparse.ArgumentParser()
parser.add_argument('--selected',action='store_true')
args=parser.parse_args()
SOURCE = ROOT / ('ArtSource/Ironvein/Selected' if args.selected else 'ArtSource/Ironvein/Candidates')
REVIEW = ROOT / '.utmp/Ironvein/ArtReview'
REVIEW.mkdir(parents=True, exist_ok=True)
paths = sorted(SOURCE.glob('*.png'))
records = []
for path in paths:
    im = Image.open(path).convert('RGBA')
    colors = sorted(set(p[:3] for p in im.getdata() if p[3]))
    alpha = sorted(set(im.getchannel('A').getdata()))
    record = dict(file=str(path.relative_to(ROOT)).replace('\\','/'), canvas=list(im.size),
                  solid_bounds=im.getchannel('A').getbbox(), opaque_colors=len(colors),
                  palette=['#%02X%02X%02X' % c for c in colors], alpha=alpha,
                  sha256=hashlib.sha256(path.read_bytes()).hexdigest(),
                  status='INTERNAL WORKING SELECTION; user review pending' if args.selected else 'GENERATED CANDIDATE; visual review pending')
    raw=ROOT/'ArtSource/Ironvein/Candidates'/path.name
    if args.selected and raw.exists():
        before=Image.open(raw).convert('RGBA')
        record['source_mask_equal']=before.size==im.size and before.getchannel('A').tobytes()==im.getchannel('A').tobytes()
    records.append(record)
refs = [ROOT/'ArtSource/LocalEnemies/Miner_Reference.png',
        ROOT/'ArtSource/Forest/Approved/Orc_Trailguard.png',
        ROOT/'ArtSource/Forest/Approved/Elven_Mender.png']
all_paths = refs + paths
columns=min(6,len(all_paths)); rows=(len(all_paths)+columns-1)//columns
composite = Image.new('RGB',(columns*145,rows*160),'#343941')
draw = ImageDraw.Draw(composite)
for i,path in enumerate(all_paths):
    im=Image.open(path).convert('RGBA'); cx=(i%columns)*145; cy=(i//columns)*160
    composite.paste(im,(cx+(145-im.width)//2,cy+16),im)
    draw.text((cx+4,cy+120),path.stem,fill='#FFFFFF')
    draw.text((cx+4,cy+137),f'{im.width} x {im.height} / native 1x',fill='#FFD477')
composite.save(REVIEW/'native-comparison.png')
composite.resize((composite.width*3,composite.height*3),Image.Resampling.NEAREST).save(REVIEW/'comparison-3x.png')
(SOURCE.parent/('Production/selected-technical.json' if args.selected else 'Production/pilot-technical.json')).write_text(json.dumps(records,indent=2)+'\n',encoding='utf-8')
html=['<!doctype html><meta charset="utf-8"><title>Ironvein native pilots</title>',
      '<style>body{background:#20242b;color:#eee;font:16px system-ui;margin:32px}img{image-rendering:pixelated}article{display:inline-block;vertical-align:top;background:#343941;padding:18px;margin:8px}h2{font-size:18px}small{display:block}</style>',
      '<h1>Ironvein native candidates</h1><p>PixelLab source stills with exact palette cleanup in separate working selections. Final user approval pending. Source files preserved.</p>',
      '<h2>Same native pixel scale · neutral background</h2><img src="native-comparison.png">']
for r in records:
    p=ROOT/r['file']; dest=REVIEW/p.name
    dest.write_bytes(p.read_bytes())
    html.append(f'<article><h2>{p.stem.replace("_"," ").title()}</h2><img src="{p.name}" width="{r["canvas"][0]*4}" height="{r["canvas"][1]*4}"><small>{r["canvas"]} · {r["opaque_colors"]} colors · alpha {r["alpha"]}</small><small>Bounds {r["solid_bounds"]}</small></article>')
    sprite=Image.open(p).convert('RGBA')
    light=Image.new('RGB',sprite.size,'#D2D1CB');light.paste(sprite,(0,0),sprite)
    light.save(REVIEW/(p.stem+'-light.png'))
    html.append(f'<article><h2>Light background</h2><img src="{p.stem}-light.png" width="{r["canvas"][0]*4}" height="{r["canvas"][1]*4}"></article>')
(REVIEW/'Review.html').write_text('\n'.join(html),encoding='utf-8')
print(json.dumps([{k:v for k,v in r.items() if k not in ('palette','sha256')} for r in records],indent=2))
