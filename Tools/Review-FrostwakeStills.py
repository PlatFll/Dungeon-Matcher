"""Native PNG inspection and review-only integer enlargement; no sprite edits."""
from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
import json,hashlib
root=Path(__file__).resolve().parents[1];art=root/'ArtSource/Frostwake'
jobs=json.loads((art/'completed-stills.json').read_text())
names={j['id']:j['name'] for p in ['still-jobs.json','milestone-jobs.json'] for j in (json.loads((art/p).read_text())['jobs'] if p=='still-jobs.json' else json.loads((art/p).read_text()))}
canvas=Image.new('RGB',(4*350,4*340),'#303238');d=ImageDraw.Draw(canvas)
font=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',18)
checks=[];html=['<!doctype html><meta charset="utf-8"><title>Frostwake roster review</title><style>body{background:#25272c;color:#eee;font:16px system-ui}main{display:grid;grid-template-columns:repeat(3,1fr);gap:14px}article{background:#303238;padding:14px}img{image-rendering:pixelated}p{line-height:1.4}</style><h1>Frostwake Peaks: eleven still candidates</h1><p>Actual native PNGs at 1x, then nearest-neighbour 3x. No art rescaling. Candidates need visual approval before full animation production.</p><main>']
refs=[('Rattlebones','Docs/ArtDirection/Audit2026-10/media/skeleton-still.png'),('Farmer','Docs/ArtDirection/EnemyCohesion2026-10/farmer-still.png'),('Pan Villager','Docs/ArtDirection/EnemyCohesion2026-10/pan_villager-still.png')]
for n,j in enumerate(jobs):
    folder='palette-clean' if (art/'palette-clean'/(j['id']+'.png')).exists() else 'candidates'
    p=art/folder/(j['id']+'.png');im=Image.open(p).convert('RGBA');b=im.getbbox()
    c=dict(id=j['id'],native_size=im.size,occupied_bounds=b,occupied_size=[b[2]-b[0],b[3]-b[1]],colors=len({v for v in im.get_flattened_data() if v[3]}),alpha_values=sorted(set(im.getchannel('A').get_flattened_data())),sha256=hashlib.sha256(p.read_bytes()).hexdigest(),approved=False)
    checks.append(c);x=(n%4)*350;y=(n//4)*340;d.text((x+8,y+8),names[j['id']],font=font,fill='white')
    shown=im.resize((im.width*3,im.height*3),Image.Resampling.NEAREST);canvas.paste(shown,(x+7,y+36),shown)
    d.text((x+8,y+307),f'{im.width}x{im.height} | {c["colors"]} colors | bounds {c["occupied_size"]}',fill='#cfbf9a')
    html.append(f'<article><h2>{names[j["id"]]}</h2><img src="{folder}/{j["id"]}.png"><br><img width="336" height="264" src="{folder}/{j["id"]}.png"><p>Native {im.width} x {im.height}; occupied {c["occupied_size"]}; {c["colors"]} opaque colors; alpha {c["alpha_values"]}.</p></article>')
for n,(name,path) in enumerate(refs,12):
    im=Image.open(root/path).convert('RGBA');x=(n%4)*350;y=(n//4)*340;d.text((x+8,y+8),name+' / actual reference',font=font,fill='#c5ddf5')
    # Same native pixel scale, same floor in the review panel.
    shown=im.resize((im.width*3,im.height*3),Image.Resampling.NEAREST);canvas.paste(shown,(x+7,y+300-im.height*3),shown)
html.append('</main><h2>Same native pixel scale comparison</h2><img width="1120" src="roster-review.png"><p>References: Rattlebones primary; Farmer and Pan supporting. Crops/transparent padding only in generation reference panels. No new golden-reference membership is implied.</p>')
canvas.save(art/'roster-review.png');(art/'technical-checks.json').write_text(json.dumps(checks,indent=2));(art/'Review.html').write_text('\n'.join(html));print(json.dumps(checks,indent=2))
