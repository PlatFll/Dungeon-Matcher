"""Build an approval gallery from exact native PixelLab outputs and measured PNG data."""
from pathlib import Path
from PIL import Image
import json, base64, hashlib, html
ROOT=Path(__file__).resolve().parents[1]
ART=ROOT/'ArtSource/Forest/RosterProduction'
def uri(path):return 'data:image/png;base64,'+base64.b64encode(path.read_bytes()).decode()
rows=[];checks=[]
titles={'Wood':'Dungeon wooden barricade','Stone':'Dungeon stone barricade','Chain':'Dungeon chain','Thorn':'Forest thorn barricade','Root1':'Forest root level 1','Root2':'Forest root level 2','Vines':'Forest dense vines'}
gem=uri(ROOT/'Assets/_Game/Art/Gems/Small gems/Gems32/Ruby32.png')
for family,title in titles.items():
    cards=[]
    for variant in 'AB':
        name=family+'_'+variant;path=ART/'Raw'/name/'Concept/00.png';im=Image.open(path).convert('RGBA')
        pixels=list(im.getdata());opaque=[p for p in pixels if p[3]>0];alpha=sorted({p[3] for p in pixels})
        check=dict(name=name,width=im.width,height=im.height,bounds=im.getbbox(),opaque_colors=len(set(opaque)),
                   transparent_percent=round(100*(len(pixels)-len(opaque))/len(pixels),1),alpha_values=alpha,sha256=hashlib.sha256(path.read_bytes()).hexdigest())
        checks.append(check);src=uri(path)
        overlay=f'<div class="over"><img class="gem" src="{gem}"><img src="{src}"></div>' if family in ['Vines','Chain'] else ''
        cards.append(f'<article><h3>{variant}</h3><div class="examples"><img class="large" src="{src}">{overlay}<img class="native" src="{src}"></div><p>64 × 64 native · {check["opaque_colors"]} visible RGBA colors · {check["transparent_percent"]}% fully transparent</p><a download="{name}.png" href="{src}">Exact PNG</a></article>')
    note='Top edge is safe; the other three carry spikes. Final placement will rotate to its safe side.' if family=='Thorn' else 'Large previews are 3× nearest-neighbor. Small previews are the unchanged 64×64 output.'
    rows.append(f'<section><h2>{title}</h2><p>{note}</p><div class="pair">{"".join(cards)}</div></section>')
page='''<!doctype html><meta charset="utf-8"><title>Forest roster · blocker approval</title>
<style>body{margin:0;padding:28px;background:#20212a;color:#f2e9d7;font:16px system-ui}main{max-width:1120px;margin:auto}h1{margin:0}p{line-height:1.5;color:#cfccbe}h2{margin-top:32px}.pair{display:grid;grid-template-columns:1fr 1fr;gap:16px}article{padding:18px;background:#30323b;border-radius:12px}.examples{display:flex;gap:18px;align-items:end;flex-wrap:wrap}img{image-rendering:pixelated;object-fit:contain}.large,.over{width:192px;height:192px;background:#777}.native{width:64px;height:64px;background:#777}.over{position:relative;background:#4c3647}.over img{position:absolute;inset:0;width:192px;height:192px}.over .gem{inset:24px;width:144px;height:144px}a{color:#e8c879}h3{font-size:24px;margin:0 0 12px}code{color:#ebc673}</style>
<main><h1>Blocker concept choices</h1><p>Two PixelLab concepts for each family. These are review candidates; existing game art is still in place. Choose A or B for each family, or name a specific revision. Vines and chains also appear over a ruby gem to show coverage.</p>'''
page+=''.join(rows)+'</main>'
(ART/'BlockerReview.html').write_text(page,encoding='utf-8')
(ART/'concept-technical-checks.json').write_text(json.dumps(checks,indent=2),encoding='utf-8')
print(json.dumps(checks))
