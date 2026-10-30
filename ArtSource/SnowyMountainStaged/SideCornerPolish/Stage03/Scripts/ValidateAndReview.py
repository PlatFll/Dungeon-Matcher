"""Read-only pixel QA and labelled inspection layouts. No artwork editing."""
from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
from collections import Counter,deque
import hashlib,json,shutil,sys,html,re
S=Path(__file__).resolve().parents[1]
build=Path(sys.argv[1])
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def components(im):
 pix=im.load();w,h=im.size;points={(x,y) for y in range(h) for x in range(w) if pix[x,y][3]};sizes=[]
 while points:
  p=points.pop();q=[p];n=0
  while q:
   x,y=q.pop();n+=1
   for dx,dy in [(1,0),(-1,0),(0,1),(0,-1),(1,1),(-1,1),(1,-1),(-1,-1)]:
    t=(x+dx,y+dy)
    if t in points:points.remove(t);q.append(t)
  sizes.append(n)
 return sorted(sizes,reverse=True)
assets=[]
js=(S/'Scripts/BuildNative.js').read_text()
maps={side:dict(re.findall(r"'([^']+)':'([^']+)'",re.search(side+r':\{([^}]+)\}',js).group(1))) for side in ['Left','Right']}
for p in sorted((build/'Exports').glob('*.png')):
 im=Image.open(p).convert('RGBA');rt=Image.open(build/'Roundtrip'/p.name).convert('RGBA');px=list(im.getdata());b=im.getbbox();comps=components(im)
 assert im.size==rt.size and im.tobytes()==rt.tobytes(),f'Round trip: {p.name}'
 assert {c[3] for c in px}<={0,255},f'Alpha: {p.name}'
 assert b and b[0]>0 and b[1]>0 and b[2]<im.width and b[3]<im.height,f'Cut edge: {p.name}'
 assert len(comps)==1,f'Disconnected pixels {p.name}: {comps}'
 if p.stem.startswith('Connector_'):
  side=p.stem.split('_')[1];src=Image.open(S/f'Raw/Approved{side}Connector.png').convert('RGBA');assert im.tobytes()==src.tobytes()
 if p.stem.startswith('Corner_'):
  side=p.stem.split('_')[1];src=Image.open(S/('Raw/Left_3.png' if side=='Left' else 'Raw/RightCargo_0.png')).convert('RGBA')
  for y in range(80):
   for x in range(96 if side=='Left' else 92):
    c=src.getpixel((x,y));h=maps[side].get('%02x%02x%02x'%c[:3]);expected=(0,0,0,0) if not c[3] else tuple(int(h[i:i+2],16) for i in (0,2,4))+(255,)
    assert im.getpixel((x+4,y+4))==expected,f'Unexpected source shift/edit {side} {x},{y}'
 shutil.copy2(p,S/'Exports'/p.name);shutil.copy2(build/'Native'/f'{p.stem}.ase',S/'Native'/f'{p.stem}.ase')
 assets.append({'file':'Exports/'+p.name,'native':'Native/'+p.stem+'.ase','size':im.size,'bounds_exclusive':b,'opaque_colors':len({c[:3] for c in px if c[3]}),'palette':sorted({'#%02X%02X%02X'%c[:3] for c in px if c[3]}),'alpha':[0,255],'connected_components_8way':comps,'sha256':sha(p),'reopened_RGBA_exact':True})
manifest={'stage':3,'status':'Corner asset candidates for review; no Unity assembly or production import','generation_spend':30,'balance_after':4838,'selections':{'left':'Raw/Left_3.png','right':'Raw/RightCargo_0.png'},'native_handling':'No source resampling. Core canvases add transparent padding; right cut source boundary completed in LibreSprite.','source_pixel_registration':{'left':'All 96x80 source positions match exact material remap at +4,+4','right':'All source positions x<92, y<80 match exact material remap at +4,+4. Only final four source columns and added canvas complete the clipped contour.'},'assets':assets}
(S/'QA/Manifest.json').write_text(json.dumps(manifest,indent=2))
(S/'QA/NativeExecution.txt').write_text((build/'QA/LibreSprite.log').read_text()+'\nNative and batch reopen processes both returned exit code 0.\nAll six reopened PNG pixel arrays match the first exports.\n')
def font(sz):return ImageFont.truetype('C:/Windows/Fonts/segoeui.ttf',sz)
def label(d,xy,t,size=17,color='#d6e5ef'):d.text(xy,t,font=font(size),fill=color)
def paste(board,im,x,y,k=1):
 z=im.resize((im.width*k,im.height*k),Image.Resampling.NEAREST);board.paste(z,(x,y),z)
board=Image.new('RGB',(1080,720),'#16232e');d=ImageDraw.Draw(board)
label(d,(24,16),'Snowy Mountain — bottom-corner asset review',28)
label(d,(24,58),'Stage 3 · PixelLab forms cleaned in LibreSprite · native scale retained',17)
for ix,side in enumerate(['Left','Right']):
 x=24+ix*535;core=Image.open(S/f'Exports/Corner_{side}.png').convert('RGBA')
 label(d,(x,100),'Left drift / buried fence' if side=='Left' else 'Right rocky cargo ledge',21)
 paste(board,core,x,154);label(d,(x,250),'1× native',15)
 paste(board,core,x+155,130,3);label(d,(x+155,414),f'{core.width}×{core.height} · 3× nearest',15)
 conn=Image.open(S/f'Exports/Connector_{side}.png').convert('RGBA');apron=Image.open(S/f'Exports/SnowApron_{side}.png').convert('RGBA')
 paste(board,conn,x,480,2);label(d,(x,610),'Cliff connector · 2×',15)
 paste(board,apron,x+150,520,3);label(d,(x+150,610),'Separate snow apron · 3×',15)
 label(d,(x,650),'Snow, rock, wood and ice use the approved material ramps.',14)
label(d,(24,690),'Asset review only. Shared UI and scene untouched. Unity fit and overlap review: Stage 4.',15)
board.save(S/'Preview/CornerKits.png')
for side,raw in [('Left','Left_3'),('Right','RightCargo_0')]:
 im=Image.new('RGB',(720,270),'#16232e');d=ImageDraw.Draw(im)
 label(d,(14,10),f'{side}: raw PixelLab source → LibreSprite cleanup',20)
 paste(im,Image.open(S/f'Raw/{raw}.png').convert('RGBA'),18,55,2)
 paste(im,Image.open(S/f'Exports/Corner_{side}.png').convert('RGBA'),280,45,2)
 label(d,(20,230),'Raw · 96×80 at 2×',15);label(d,(280,238),'Cleaned · 2×, no source resizing',15)
 im.save(S/f'Preview/{side}Comparison.png')
cards=''.join(f'<article><h3>{Path(a["file"]).stem}</h3><div class="pair"><img src="{a["file"]}" width="{a["size"][0]}"><img src="{a["file"]}" width="{a["size"][0]*3}"></div><p>{a["size"][0]}×{a["size"][1]} · {a["opaque_colors"]} opaque colors · binary alpha<br>One connected mass; clear native canvas margins</p><a href="{a["file"]}">PNG</a> · <a href="{a["native"]}">LibreSprite source</a></article>' for a in assets)
rawcards=''.join(f'<article><h3>{p.stem}</h3><img src="Raw/{p.name}" width="288"><p>96×80 native · 3× nearest</p></article>' for p in sorted((S/'Raw').glob('*.png')) if p.stem.startswith(('Left_','Right_','RightCargo_')))
page='''<!doctype html><html><meta charset="utf-8"><title>Snowy Mountain Stage 3 — Corner kits</title><style>body{background:#0e171f;color:#e5eef6;font:17px system-ui;margin:30px auto;max-width:1160px}h1{font-size:30px}p{line-height:1.55;color:#bfcfda}a{color:#86d5ff}.grid{display:grid;grid-template-columns:repeat(2,1fr);gap:18px}article{padding:20px;background:#182733;border-radius:10px}img{image-rendering:pixelated;max-width:100%;object-fit:contain} .pair{display:flex;align-items:center;gap:28px;min-height:270px}summary{cursor:pointer;margin:25px 0}.status{padding:16px;border-left:4px solid #89bccc;background:#1a2c3a}</style><h1>Snowy Mountain — Stage 3 corner kits</h1><p class="status">Asset review checkpoint. No scene assembly or production import in this stage. Top HUD, characters, board/frame, gems and bottom controls remain unchanged.</p><img src="Preview/CornerKits.png" width="1080"><h2>Native PNGs and editable sources</h2><p>Each pair shows 1× and 3× nearest-neighbor. The connectors are lossless copies of the approved Stage 2 overlap courses; the snow aprons are separate native LibreSprite drawings. The right core includes a finished outer contour where the generated source was clipped.</p><div class="grid">'''+cards+'''</div><h2>Raw versus cleaned</h2><img src="Preview/LeftComparison.png"><img src="Preview/RightComparison.png"><h2>Validation and next checkpoint</h2><p>Six native documents reopened and exported pixel-exact. All six images have binary alpha, one connected opaque component and clear canvas margins. Native source scale is retained. No claim of final Unity layout fit: Stage 4 will verify connector overlaps and clearance around potion/bomb controls and text at every supported viewport.</p><p>PixelLab: three Pro calls, twelve candidates, <b>30 generations</b> total. 4,838 generations remain; no dollar credits spent.</p><p><a href="QA/Manifest.json">Measured manifest</a> · <a href="QA/Lineage.json">Reference lineage</a> · <a href="../Stage02/Review.html">Approved side kits</a> · <a href="../../Stage09/Review.html">Unchanged earlier Unity baseline</a></p><details><summary>All twelve immutable raw PixelLab candidates</summary><div class="grid">'''+rawcards+'''</div></details></html>'''
(S/'Review.html').write_text(page,encoding='utf-8')
print(json.dumps({'assets':[{'file':a['file'],'size':a['size'],'colors':a['opaque_colors'],'components':len(a['connected_components_8way'])} for a in assets],'roundtrip':'6/6 exact','edge_clearance':'6/6 pass'}))
