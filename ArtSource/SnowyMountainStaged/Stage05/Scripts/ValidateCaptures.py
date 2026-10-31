from pathlib import Path
from PIL import Image,ImageDraw,ImageChops,ImageFont
import json,math,shutil,hashlib
F=Path(__file__).resolve().parents[4];S=F/'ArtSource/SnowyMountainStaged/Stage05'
rows=json.loads((S/'Preview/Measurements.json').read_text())['items']
assert len(rows)==5
wallcolors=set(Image.open(S/'Exports/CobbleWall_Muted.png').convert('RGB').get_flattened_data())
oldcolors=set(Image.open(S/'Raw/OriginalCobbleWall.png').convert('RGB').get_flattened_data())
def near(c,palette):return any(max(abs(a-b) for a,b in zip(c,q))<=1 for q in palette)
results=[]
for row in rows:
 key=row['size'];a=Image.open(S/f'Preview/Before/{key}.png').convert('RGB');b=Image.open(S/f'Preview/After/{key}.png').convert('RGB');w,h=a.size;p=row['nativePixelScale'];d=ImageChops.difference(a,b)
 # The portrait source and interior remain untouched despite removal of the outer HUD enclosure.
 r=row['geometry']['portrait'];pad=10*p
 rect=(math.ceil(r['x']+pad),math.ceil(h-r['y']-r['height']+pad),math.floor(r['x']+r['width']-pad),math.floor(h-r['y']-pad))
 assert not d.crop(rect).getbbox(),(key,'Portrait interior changed')
 # New wall reaches every lower screen edge, including safe-area gutters.
 low=math.ceil(h-row['wallTop'])
 edge=[b.getpixel((x,y)) for y in range(low,h) for x in (0,w-1)]+[b.getpixel((x,h-1)) for x in range(w)]
 uncovered=sum(not near(c,wallcolors) for c in edge);assert not uncovered,(key,uncovered)
 # Quantized rim follows the unchanged board opening. Only the backing may differ within it.
 fr=row['boardFrame'];pad=13*p+2
 box=(math.ceil(fr['x']+pad),math.ceil(h-fr['y']-fr['height']+pad),math.floor(fr['x']+fr['width']-pad),math.floor(h-fr['y']-pad))
 unexpected=sum(x!=y and not(near(x,oldcolors) and near(y,wallcolors)) for x,y in zip(a.crop(box).get_flattened_data(),b.crop(box).get_flattened_data()))
 assert not unexpected,(key,'Gem/cell pixel changed',unexpected)
 results.append(dict(viewport=key,native_pixel_scale=p,portrait_exact=True,board_pixels_unchanged_except_backing=True,uncovered_lower_edge_pixels=uncovered))
(S/'QA/ScreenChecks.json').write_text(json.dumps(results,indent=2))
font=ImageFont.truetype('C:/Windows/Fonts/segoeui.ttf',20)
a=Image.open(S/'Preview/Before/720x1280.png').convert('RGB');b=Image.open(S/'Preview/After/720x1280.png').convert('RGB')
board=Image.new('RGB',(1464,1320),'#111720');d=ImageDraw.Draw(board);d.text((8,6),'Before: bright wall / legacy enclosures and board rim',font=font,fill='white');d.text((752,6),'Stage 5: muted wall / open HUD / native snowy timber frame',font=font,fill='white');board.paste(a,(0,40));board.paste(b,(744,40));board.save(S/'Preview/Unity-BeforeAfter.png')
html='''<!doctype html><meta charset="utf-8"><title>Snowy Mountain — Stage 5 review</title><style>body{background:#111720;color:#e1eaf3;font:16px system-ui;margin:24px}h1{font-size:24px}button,select{background:#263848;color:white;padding:8px;border:1px solid #59748a;margin:6px}img{image-rendering:pixelated;max-width:100%;object-fit:contain;object-position:top}.pair{display:flex;gap:20px;align-items:flex-start}.pair figure{margin:0;width:48%}.pair img{width:100%}details{margin:20px 0}a{color:#a2d7ef}.native{display:flex;gap:24px;flex-wrap:wrap}.native figure{background:#25313d;padding:16px;margin:0}.native img{max-width:none}figcaption{margin:7px 0}p{max-width:1000px}</style>
<h1>Snowy Mountain — Stage 5 board-frame kit</h1><p>Actual Unity Game captures with three live enemies. Lower masonry is darker and muted; the two outer HUD enclosures are removed. Approved scenery, snow ledge, portrait, gameplay board, gems and controls are preserved. Review only — not merged or registered as the production zone art.</p>
<label>Viewport <select id="size"><option>720x1280</option><option>1080x1920</option><option>1080x2400</option><option>1080x2400-safe</option><option>360x640</option></select></label><button id="actual">Toggle actual pixels / fit</button>
<div class="pair"><figure><figcaption>Before</figcaption><img id="before" src="Preview/Before/720x1280.png"></figure><figure><figcaption>Revised</figcaption><img id="after" src="Preview/After/720x1280.png"></figure></div>
<p>Native art scale: 1× / 2× / 3× depending on viewport. The existing low-resolution board sampling and 360×640 HUD crowding are retained; this stage does not change gameplay layout. No phone/device testing is claimed.</p>
<details open><summary>Native exported assets — 1× and 4×</summary><p>13 editable LibreSprite modules. Snowy timber rails, four iron corners, one centered connector, separate snow caps/icicles, and a recolored copy of the cobble tile. Snow remains outside the playable opening.</p><div class="native">ASSETS</div></details>
<details><summary>PixelLab candidates / source</summary><p>One Pro source job, four 128×128 candidates, 10 generations spent; candidate 2 selected. 4,920 generations remaining. No reference image uploads.</p><img src="QA/Candidates.png"></details>
<p><a href="QA/NativeAssets.json">Native checks</a> · <a href="QA/ScreenChecks.json">Screenshot checks</a> · <a href="Preview/Measurements.json">Unity measurements</a> · <a href="README.md">Handoff notes</a></p>
<script>size.onchange=()=>{before.src='Preview/Before/'+size.value+'.png';after.src='Preview/After/'+size.value+'.png'};let fit=true;actual.onclick=()=>{fit=!fit;document.querySelectorAll('.pair figure').forEach(f=>f.style.width=fit?'48%':'auto');document.querySelectorAll('.pair img').forEach(i=>{i.style.maxWidth=fit?'100%':'none';i.style.width=fit?'100%':'auto'})};</script>'''
cards=[]
for p in sorted((S/'Exports').glob('*.png')):
 im=Image.open(p);w,h=im.size;cards.append(f'<figure><figcaption>{p.stem} · {w}×{h}</figcaption><img src="Exports/{p.name}" width="{w}" height="{h}"><br><img src="Exports/{p.name}" width="{w*4}" height="{h*4}"></figure>')
(S/'Review.html').write_text(html.replace('ASSETS',''.join(cards)),encoding='utf-8')
print(json.dumps(results))
