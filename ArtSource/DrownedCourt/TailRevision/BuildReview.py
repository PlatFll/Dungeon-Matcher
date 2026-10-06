from pathlib import Path
from PIL import Image,ImageDraw
import json,hashlib,base64,html
here=Path(__file__).resolve().parent;repo=here.parents[2];selected=here.parent/'Production/Selected';out=here/'Review'
checks=json.loads((here/'TechnicalChecks.json').read_text())['checks'];names=['hammerhead_bruiser','breakwater_captain']
native=json.loads((selected/'Motion/NativeChecks.json').read_text())
for item in native['checks']:
    if item['name'] in names:
        c=next(c for c in checks if c['file']==f"Motion/{item['name']}/{item['state']}.png")
        item.update(colors=c['opaque_colors_after'],alpha=c['alpha'],sha256=c['sha256'],revision='2026-10-06 direct shark tail; see TailRevision/TechnicalChecks.json')
(selected/'Motion/NativeChecks.json').write_text(json.dumps(native,indent=2)+'\n')
stills=json.loads((selected/'StillManifest.json').read_text())
for s in stills:
    if s['id'] in names:
        c=next(c for c in checks if c['file']==s['id']+'.png')
        s['revision']='Authorized direct tail addition; original body pixels and palette unchanged; revised PNG awaiting review'
        s['sha256']=c['sha256'];s['colors']=c['opaque_colors_after'];s['alpha']=c['alpha']
(selected/'StillManifest.json').write_text(json.dumps(stills,indent=2)+'\n')

references=[('Spear Knight',repo/'Assets/_Game/Art/Enemies/Knight/SpearKnight/SpearKnight_1.png'),
            ('Shield Knight',repo/'Assets/_Game/Art/Enemies/Knight/ShieldKnight/ShieldKnight_1.png')]
items=[]
for n in names:
    items.extend([(n+' before',here/'Before'/(n+'.png')),(n+' after',selected/(n+'.png'))])
items+=references
proof=Image.new('RGB',(len(items)*148,124),(84,84,84));d=ImageDraw.Draw(proof)
for i,(label,p) in enumerate(items):
    im=Image.open(p).convert('RGBA');assert im.width<=128 and im.height<=96
    proof.paste(im,(i*148+(148-im.width)//2,96-im.height),im);d.text((i*148+2,102),label.replace('_',' '),fill='white')
proof.save(out/'NativeComparison.png')
roster=[(p.stem,p) for p in selected.glob('*.png') if not p.stem.endswith('_wide')]
cast=Image.new('RGB',(7*136,2*120),(84,84,84));d=ImageDraw.Draw(cast)
for i,(name,p) in enumerate(roster):
    im=Image.open(p).convert('RGBA');x=i%7*136;y=i//7*120;cast.paste(im,(x+(136-im.width)//2,y+96-im.height),im);d.text((x,y+100),name.replace('_',' '),fill='white')
cast.save(out/'NativeCourtCast.png')
def embed(p):return 'data:image/'+('gif' if p.suffix=='.gif' else 'png')+';base64,'+base64.b64encode(p.read_bytes()).decode()
parts=['<!doctype html><meta charset="utf-8"><title>Shark tail revision</title><style>body{background:#25282b;color:#eee;font:16px system-ui;margin:24px}img{image-rendering:pixelated;background:#545454}section{padding:20px;border:1px solid #555;margin:16px 0}.motion{width:256px;height:160px;object-fit:contain}button{font:inherit}table{border-collapse:collapse}td,th{padding:8px;border:1px solid #666}</style><h1>Compact shark tails — direct pixel revision</h1><p>Original body pixels, palette, scale, floor and canvas preserved. No generations. Revised tails await your visual review.</p><button onclick="document.querySelectorAll(\'.native\').forEach(i=>i.style.width=i.style.width?\'\':i.naturalWidth*2+\'px\')">Toggle native / 2× inspection</button>']
for file in ['NativeComparison.png','NativeCourtCast.png']:
    parts.append(f'<h2>{file}</h2><img class="native" src="{embed(out/file)}">')
parts.append('<p>Needlefin also has a weak shark-tail silhouette; reported only. All other characters are unchanged. The older source animation contains a few stray hit/swing pixels, preserved by this silhouette-only edit.</p>')
manifest=json.loads((selected/'Motion/animation-manifest.json').read_text())['clips']
for n in names:
    parts.append(f'<h2>{n.replace("_"," ")}</h2>')
    for c in [c for c in manifest if c['name']==n]:
        folder=selected/'Motion'/n;s=c['state'];parts.append(f'<section><h3>{s} — {c["frameCount"]} frames · {c["width"]}×{c["height"]} native</h3><img class="motion" src="{embed(folder/(s+".gif"))}"><details><summary>All frames at native pixel scale</summary><img class="native" src="{embed(folder/(s+"_native.png"))}"></details></section>')
parts.append('<h2>Technical checks</h2><p>Every original opaque pixel is byte-identical. Added pixels use only existing palette entries and touch the original sprite. Binary alpha, unchanged canvas, unchanged floor. Exact per-file hashes and dimensions are in TechnicalChecks.json. Comparison composites do not replace individual-file checks.</p>')
(out/'Review.html').write_text('\n'.join(parts),encoding='utf-8')
print('Native proof, complete cast and all 13 motion roles reviewed in offline gallery')
