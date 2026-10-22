from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
import hashlib,json,shutil
P=Path(__file__).resolve().parents[4];R=P/'ArtSource/DungeonHybrid/Stage03'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def load(n):return Image.open(R/'Exports'/f'{n}.png').convert('RGBA')
def frame(f,w,h):
 a=Image.new('RGBA',(w,h),(21,20,35,255));c=9;t=5
 for x in range(4,w-4,16):
  for y,s in [(2,'Top'),(h-7,'Bottom')]:a.alpha_composite(load(f+'_Rail_'+s).crop((0,0,min(16,w-4-x),5)),(x,y))
 for y in range(4,h-4,16):
  for x,s in [(2,'Left'),(w-7,'Right')]:a.alpha_composite(load(f+'_Rail_'+s).crop((0,0,5,min(16,h-4-y))),(x,y))
 for s,pos in [('TL',(0,0)),('TR',(w-9,0)),('BL',(0,h-9)),('BR',(w-9,h-9))]:a.alpha_composite(load(f+'_Corner_'+s),pos)
 # Clear transparent outside corner/rail silhouette rather than filling the bounding box.
 mask=Image.new('L',(w,h));d=ImageDraw.Draw(mask);d.rectangle((4,4,w-5,h-5),fill=255)
 for s,pos in [('TL',(0,0)),('TR',(w-9,0)),('BL',(0,h-9)),('BR',(w-9,h-9))]:mask.paste(load(f+'_Corner_'+s).getchannel('A'),pos)
 for y in [2,h-7]:d.rectangle((4,y,w-5,y+4),fill=255)
 for x in [2,w-7]:d.rectangle((x,4,x+4,h-5),fill=255)
 a.putalpha(mask);return a
for f,w,h,x,y in [('Wave',103,32,12,8),('Control',47,47,8,8),('Resonance',118,44,5,10)]:
 a=Image.new('RGBA',(128 if f!='Control' else 64,64));a.alpha_composite(frame(f,w,h),(x,y))
 if f!='Control':
  for side,xx in [('Left',x-3),('Right',x+w-4)]:a.alpha_composite(load(f+'_Wing_'+side),(xx,y+(h-15)//2))
 if f=='Wave':a.alpha_composite(load('Wave_Skull'),(x+44,y+28))
 if f=='Resonance':
  for yy in [y+1,y+h-8]:a.alpha_composite(load('Resonance_Clamp'),(x+(w-7)//2,yy))
 a.save(R/'Previews'/f'{f}-clean-native.png');a.resize((a.width*4,a.height*4),Image.Resampling.NEAREST).save(R/'Previews'/f'{f}-clean-4x.png')
mods=json.loads((R/'Modules.json').read_text())['parts'];canvas=Image.new('RGBA',(384,180),(21,20,35,255));d=ImageDraw.Draw(canvas)
for i,m in enumerate(mods):
 x=(i%8)*48;y=(i//8)*36;im=load(m['name']);canvas.alpha_composite(im,(x+4,y+3));d.text((x+1,y+22),str(i+1),fill='#b5a9bc')
canvas.save(R/'Previews/Native-modules-1x.png')
retained=[]
for family in ['PortraitFrame','BoardFrame']:
 for p in sorted((P/'Assets/_Game/Art/DungeonPixelLab'/family).glob('*.png')):retained.append(dict(path=p.relative_to(P).as_posix(),sha256=sha(p),dimensions=Image.open(p).size))
(R/'Diagnostics/RetainedUI.json').write_text(json.dumps(retained,indent=2)+'\n')
# Gallery is a review artifact. No generated full-scene asset is imported.
html='''<!doctype html><meta charset="utf-8"><title>Hybrid Stage 03 — UI hero pieces</title><style>body{margin:32px;background:#11121b;color:#e0d8e3;font:16px system-ui}h1,h2{font-weight:600}section{padding:20px;margin:20px 0;background:#1e2030;border-radius:10px}img{image-rendering:pixelated;max-width:100%;object-fit:contain}a{color:#dd9bea}.row{display:flex;gap:28px;flex-wrap:wrap}.card{padding:12px;background:#151423}.muted{color:#b5a9bc}label{display:block}button{padding:8px}</style><h1>Stage 03 · UI hero pieces</h1><p>Candidate UI only. Stage 01 structure and Stage 02 gate are preserved. No merge or production integration.</p><p><a href="../Stage02/Review.html">Approved Stage 02</a> · <a href="Report.md">Validation / handoff</a></p><section><h2>Actual Unity top-HUD study</h2><p>Approved gate, floor, wall and cast retained. New wave / Guide / settings frames. Approved portrait frame reused.</p><label>Integer zoom <select onchange="document.getElementById('top').style.width=(360*this.value)+'px'"><option value="1">1×</option><option value="2" selected>2×</option><option value="3">3×</option></select></label><img id="top" src="Previews/Unity-Top-360x240.png" style="width:720px"></section><section><h2>Actual Unity board + lower-HUD study</h2><p>Separate native-scale study, not a new production layout. Approved 64×64 cells and gems; unchanged scalable board frame. Existing icons and progress frame. New resonance and consumable frames.</p><img src="Previews/Unity-BoardHud-576x688.png" width="576"></section><section><h2>PixelLab source → LibreSprite cleanup</h2><p>Native canvases shown below. No image scaling was used to create the asset files.</p>'''
for f,raw in [('Wave','Wave'),('Control','Controls'),('Resonance','Resonance')]:
 html+=f'<h3>{f}</h3><div class="row"><div class="card">Raw 1×<br><img src="Sources/{raw}_Raw.png"></div><div class="card">Clean assembly 1×<br><img src="Previews/{f}-clean-native.png"></div><div class="card">Clean 4×<br><img src="Previews/{f}-clean-4x.png"></div></div>'
html+='<h3>35 native modular pieces at 1×</h3><img src="Previews/Native-modules-1x.png"><p class="muted">Corners and rails are independent. Insets overfill behind masks. Ornaments are complete separate sprites, outside the clipped interior.</p></section>'
(R/'Review.html').write_text(html,encoding='utf-8')
print('Review boards and gallery written; retained UI files:',len(retained))
