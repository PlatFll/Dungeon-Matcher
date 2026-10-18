"""Build review material from actual Unity captures, without editing production pixels."""
from pathlib import Path
from PIL import Image,ImageDraw,ImageChops
import json,hashlib,math,xml.etree.ElementTree as ET
K=Path(__file__).resolve().parents[1];P=K.parents[2];F=K/'Correction/FlagstoneRebuild';V=F/'Review'
qa=json.loads((F/'Validation.json').read_text());test=ET.parse(K/'Evidence/UnityTests.xml').getroot()
assert test.attrib['result']=='Passed' and test.attrib['passed']=='2'
def bounds(width,height,inset):
 sx,sy,sw,sh=(24,80,1032,2200) if inset else (0,0,width,height)
 s=max(1,math.floor(sw/456));vx=math.ceil(sx/s)*s+4*s;vy=math.ceil(sy/s)*s+4*s
 w=math.floor((sx+sw-4*s-vx)/(2*s))*2;h=math.floor((sy+sh-4*s-vy)/(2*s))*2
 fit=min(w*.85*s/568,(h-156-12-320)*s/568);board=math.floor(fit*568/s);remaining=h-156-board
 bh=min(max(320,min(480,remaining-12)),remaining-12);bottom=height-(vy+(h-bh)*s);top=bottom-bh*s
 return (vx+round(137*w/470)*s,top+round(177*bh/298)*s,vx+w*s,bottom-23*s),bottom,s
layouts={}
for f in sorted((K/'Review/Unity').glob('*.png')):
 before=Image.open(F/'BeforeUnity'/f.name).convert('RGB');after=Image.open(f).convert('RGB');box=ImageChops.difference(before,after).getbbox();limit,bottom,scale=bounds(*after.size,'safe' in f.stem)
 assert box and box[0]>=limit[0] and box[1]>=limit[1] and box[2]<=limit[2] and box[3]<=limit[3],(f.name,box,limit)
 layouts[f.stem]={'changed_bounds':box,'authorized_floor_bounds':limit,'non_floor_render_pixels_identical':True,'sha256':hashlib.sha256(f.read_bytes()).hexdigest()}
 for label,im in [('Before',before),('After',after)]:im.crop((0,0,im.width,bottom+8)).save(V/f'{f.stem}-{label}-Upper.png')
 if f.stem=='1080x1920':
  board=Image.new('RGB',(1096,376),'#171824');d=ImageDraw.Draw(board)
  for x,label,im in [(0,'Before',before),(556,'New PixelLab flagstone tiles',after)]:
   d.text((x+8,8),label,fill='white');crop=im.crop((0,0,1080,bottom+8));board.paste(crop.resize((540,(bottom+8)//2),Image.Resampling.NEAREST),(x,28))
  board.save(V/'BeforeAfterUpper.png')
(F/'UnityRenderChecks.json').write_text(json.dumps({'tests':dict(test.attrib),'layouts':layouts},indent=2)+'\n',encoding='utf-8')
style='<style>body{margin:24px;background:#151621;color:#eee;font:16px system-ui}a{color:#ccb6ff}p{max-width:1100px;line-height:1.5}header{position:sticky;top:0;background:#151621ee;padding:12px;z-index:2}button,select{padding:10px;background:#30283e;color:white;border:1px solid #77618d}.compare{display:grid;grid-template-columns:1fr 1fr;gap:14px}figure{margin:0;background:#212230;overflow:auto}figcaption{padding:10px}img{image-rendering:pixelated;max-width:100%}.exact{display:block}.exact img{max-width:none}.tiles{width:768px}details{margin:24px 0}@media(max-width:800px){.compare{grid-template-columns:1fr}}</style>'
page='<!doctype html><meta charset="utf-8"><title>Dungeon flagstone floor</title>'+style+'''<h1>Dungeon floor - reference-based PixelLab tileset</h1>
<p>Broad, quieter blue-violet flagstones replace the dense cracked paving. Twelve native 64x64 floor modules cover rear, middle and near depth. Walls, torch lighting, props, frames and UI placement are preserved.</p>
<details open><summary>Your supplied floor reference</summary><img src="../Correction/FlagstoneRebuild/UserFloorReference.png" style="width:810px"></details>
<header><select id="view"><option>1080x1920</option><option>720x1280</option><option>1080x2400</option><option>1080x2400-safe-area</option></select> <button id="zoom">Toggle actual pixels</button> <a href="Review.html">Full reconstruction gallery</a></header>
<div class="compare" id="compare"><figure><figcaption>Before this floor replacement</figcaption><img id="before" src="../Correction/FlagstoneRebuild/Review/1080x1920-Before-Upper.png"></figure><figure><figcaption>Updated floor - actual Unity render</figcaption><img id="after" src="../Correction/FlagstoneRebuild/Review/1080x1920-After-Upper.png"></figure></div>
<details open><summary>Native 1x scene comparison</summary><img src="../Correction/FlagstoneRebuild/Review/BeforeAfterUpper.png"></details>
<details open><summary>Final tileset - rear / middle / near rows, A-D variants</summary><p>Native sheet 256x192, displayed at nearest-neighbor 3x. Each tile remains a separate editable LibreSprite source and 64x64 Unity PNG.</p><img class="tiles" src="../Correction/FlagstoneRebuild/FloorTileset.png"></details>
<details><summary>Repeated floor materials</summary>'''
for name in ['FloorBack','FloorMiddle','Floor']:page+=f'<p>{name}</p><img class="tiles" src="../Correction/FlagstoneRebuild/Review/{name}-Repeat.png">'
page+='''</details><p>Verified native cels, PNGs and Unity copies; binary alpha; all tile-edge pairings; fresh two-test Unity run across four layouts. Render changes are confined to the floor. Visual approval remains pending.</p>
<script>const v=document.querySelector('#view');v.onchange=()=>{for(const id of ['before','after'])document.querySelector('#'+id).src='../Correction/FlagstoneRebuild/Review/'+v.value+'-'+id[0].toUpperCase()+id.slice(1)+'-Upper.png'};document.querySelector('#zoom').onclick=()=>document.querySelector('#compare').classList.toggle('exact');</script>'''
(K/'Review/Floor.html').write_text(page,encoding='utf-8')
# Keep the full gallery's current captures and material comparisons consistent.
for name,box in {'Floor':(315,407,1080,674),'Walls':(315,0,1080,407),'Props':(315,150,1080,675),'PlayerFrame':(30,70,320,630),'BoardFrame':(80,680,995,1590),'UI':(150,1610,940,1885)}.items():Image.open(K/'Review/Unity/1080x1920.png').crop(box).save(K/'Correction/Review'/f'{name}-After.png')
p=K/'Review/Review.html';s=p.read_text(encoding='utf-8')
if 'href="Floor.html"' not in s:s=s.replace('<header>','<p><a href="Floor.html">Latest floor replacement: supplied reference / before / updated tileset</a></p><header>')
p.write_text(s,encoding='utf-8')
print(json.dumps({'gallery':'Review/Floor.html','fresh_unity_tests':2,'floor_only_render_checks':len(layouts)}))
