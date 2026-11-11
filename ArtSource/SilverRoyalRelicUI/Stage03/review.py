"""Build a clearly labelled asset review, never a substitute for a Unity capture."""
from pathlib import Path
from PIL import Image
import json,hashlib
HERE=Path(__file__).resolve().parent
manifest=json.loads((HERE/'Manifest.json').read_text())
def png(name):return Image.open(HERE/'PNG'/f'{name}.png').convert('RGBA')
family=Image.new('RGBA',(360,222),(21,20,35,255))
def put(im,x,y):family.alpha_composite(im,(x,y))
for name,x,y in [('ApprovedWaveBody',66,24),('ApprovedWaveDiamond',143,16),('ApprovedWavePendant',140,56),('ApprovedSettings',274,20)]:put(Image.open(HERE/'Inputs'/f'{name}.png').convert('RGBA'),x,y)
put(png('EnergyCasing'),106,108);fill=png('EnergyFill');put(fill.crop((0,0,70,8)),126,115)
put(png('AbilityFrame'),92,142);put(png('AbilityFlourish'),92,142);put(png('RattlebonesEmblem'),165,160)
for name,x in [('Potion',12),('Bomb',284)]:
 put(png('ItemFrame'),x,144)
 icon=Image.open(HERE/'Inputs'/f'{name}.png').convert('RGBA');assert icon.size==(24,24),icon.size
 put(icon.resize((48,48),Image.Resampling.NEAREST),x+8,152)
# Preview text is drawn on the review, never baked into production art.
glyphs={'W':['10001','10001','10001','10101','10101','11011','10001'],'A':['01110','10001','10001','11111','10001','10001','10001'],'V':['10001','10001','10001','10001','01010','01010','00100'],'E':['11111','10000','10000','11110','10000','10000','11111'],'1':['00100','01100','00100','00100','00100','00100','01110'],'3':['11110','00001','00001','01110','00001','00001','11110']}
def label(text,x,y,s):
 for ch in text:
  for yy,row in enumerate(glyphs.get(ch,['00000']*7)):
   for xx,b in enumerate(row):
    if b=='1':
     for dy in range(s):
      for dx in range(s):family.putpixel((x+xx*s+dx,y+yy*s+dy),(255,249,240,255))
  x+=6*s
label('WAVE 1',119,40,2)
for x in (58,330):
 for yy in range(188,208):
  for xx in range(x-3,x+12):family.putpixel((xx,yy),(17,16,27,255))
 label('3',x,191,2)
family.save(HERE/'Preview/Family-native.png')
family.resize((1080,666),Image.Resampling.NEAREST).save(HERE/'Preview/Family-3x.png')
cards=[]
for name,r in manifest['assets'].items():
 w,h=r['dimensions']
 cards.append(f'<article><h3>{name}</h3><div class="native"><img src="PNG/{name}.png" width="{w}" height="{h}" alt="{name} at native resolution"></div><div class="zoom"><img src="Preview/{name}-4x.png" alt="{name}, four times enlarged with nearest neighbor"></div><p>{w} × {h} · {r["opaque_colors"]} opaque colors · binary alpha</p><a href="PNG/{name}.png">PNG</a> · <a href="Editable/{name}.ase">LibreSprite source</a></article>')
html='''<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Royal bevel · Stage 3 bottom HUD</title>
<style>:root{color-scheme:dark}*{box-sizing:border-box}body{margin:0;color:#eceaf2;background:#101018;font:16px/1.6 system-ui,sans-serif}main{max-width:1180px;margin:auto;padding:32px 24px 64px}h1{font-size:34px;line-height:1.15}p{color:#bdb8cb}a{color:#d1baff}.tag{color:#e8b84d;font-size:12px;font-weight:700;letter-spacing:1px;text-transform:uppercase}.note{background:#201d2b;padding:16px 20px;border-left:3px solid #e8b84d}.toolbar{display:flex;gap:12px;align-items:center;flex-wrap:wrap;margin:20px 0}button,select,input{font:inherit}button,select{padding:7px 12px;background:#302b40;border:1px solid #70677d;color:inherit;border-radius:5px}button:hover{background:#4b3f60}input{accent-color:#bc52fa}input[type=number]{width:60px}label{display:flex;gap:8px;align-items:center}.stage{overflow:auto;border:1px solid #484153;border-radius:8px;padding:16px;background:#1b1927}canvas,img{image-rendering:pixelated;vertical-align:middle}canvas{display:block;margin:auto}.checks{display:flex;gap:10px;flex-wrap:wrap;margin-top:18px}.checks span{background:#252233;padding:8px 12px;border-radius:5px;font-size:14px}.grid{display:grid;grid-template-columns:1fr 1fr;gap:18px}article{background:#1c1927;padding:20px;border:1px solid #3b3549;border-radius:8px;overflow:auto}.native{padding:14px;background:#292536;min-height:96px;display:flex;align-items:center}.zoom{padding:18px 0;overflow:auto}h2{margin-top:36px}h3{margin:0 0 14px}.raw{display:flex;gap:20px;flex-wrap:wrap}figure{margin:0;background:#24202e;padding:18px}figcaption{font-size:14px;margin-bottom:10px}.reference{display:flex;align-items:center;gap:28px;overflow:auto;background:#24202e;padding:18px}footer{margin-top:32px;padding-top:16px;border-top:1px solid #484153}@media(max-width:760px){.stage{padding:0}main{padding:18px 12px}.grid{grid-template-columns:1fr}h1{font-size:27px}}</style>
<main><div class="tag">Selected A · Stage 3 · review checkpoint</div><h1>The bottom HUD joins the Royal bevel family</h1><p>Silver frames, restrained gold accents and dark violet recesses. The approved wave and Settings artwork above remains unchanged.</p>
<div class="note">Native asset assembly, not a Unity screenshot. Stage 3 stops here for your review; game integration follows in Stage 4. Nothing is merged.</div>
<div class="toolbar"><label>Zoom <select id="zoom"><option value="1">Native 1×</option><option value="2" selected>2×</option><option value="3">3×</option></select></label><label>Backdrop <select id="backdrop"><option value="#151423">Dark dungeon</option><option value="#163B35">Forest green</option><option value="#087E91">Underwater teal</option><option value="#49302C">Warm mine</option><option value="#C4DEE7">Pale snow</option></select></label><label>Charge <input id="charge" type="range" min="0" max="100" value="65"><output id="amount">65%</output></label></div>
<div class="stage"><canvas id="family" width="360" height="222" aria-label="Native assembly of the approved top controls and new bottom HUD"></canvas></div>
<div class="toolbar"><button id="empty">Empty charge</button><button id="full">Full charge</button><button id="emblem">Hide Rattlebones emblem</button><label>Item count <input id="count" type="number" min="0" max="99" value="3"></label></div>
<p>The charge and quantities above are live review controls. Potion and bomb reuse their current game icons and the exact same frame. The crowned skull is a separate Rattlebones sample; the universal ability frame has an empty center.</p>
<div class="checks"><span>6 separate native exports</span><span>18 PixelLab generations</span><span>Exact LibreSprite roundtrips</span><span>No production changes</span></div>
<h2>Native assets and editable sources</h2><p>Each asset is shown first at 1×, then at 4×. Straight frame runs were extended or shortened without resampling bevels, corners or jewelry.</p><div class="grid">'''+''.join(cards)+'''</div>
<h2>Original PixelLab sources</h2><p>Unedited generation sheets at 1×. The final exports above have corrected dimensions, a continuous ability rail, quieter interiors and separated live content.</p><div class="raw">'''+''.join(f'<figure><figcaption>{n} source · 192 × 128</figcaption><img src="Raw/{n}.png" width="192" height="128" alt="Raw PixelLab {n} source"></figure>' for n in ('Ability','Supply','Energy'))+'''</div>
<h2>Rendering reference</h2><div class="reference"><img src="Inputs/Rattlebones.png" width="64" height="64" alt="Unchanged current Rattlebones golden reference at 1x"><img src="Inputs/Rattlebones.png" width="256" height="256" alt="Unchanged Rattlebones at 4x"><img src="Inputs/ApprovedA_PixelLab.png" width="192" height="128" alt="Selected A source"></div><p>Rattlebones remains unchanged. Selected A was passed to PixelLab as the actual style-image input; its recorded parent input is the approved Rattlebones PNG.</p>
<footer><a href="README.md">Handoff notes</a> · <a href="Manifest.json">Asset measurements</a> · <a href="QA/Validation.json">Preservation and pixel checks</a> · <a href="Preview/Family-3x.png">Enlarged review image</a> · <a href="../Stage02/Review.html">Stage 2 Unity captures</a></footer></main>
<script>
const paths={wave:'Inputs/ApprovedWaveBody.png',diamond:'Inputs/ApprovedWaveDiamond.png',pendant:'Inputs/ApprovedWavePendant.png',settings:'Inputs/ApprovedSettings.png',energy:'PNG/EnergyCasing.png',fill:'PNG/EnergyFill.png',ability:'PNG/AbilityFrame.png',flourish:'PNG/AbilityFlourish.png',emblem:'PNG/RattlebonesEmblem.png',item:'PNG/ItemFrame.png',potion:'Inputs/Potion.png',bomb:'Inputs/Bomb.png'};
const ims={},c=document.getElementById('family'),ctx=c.getContext('2d');let showEmblem=true;
const glyphs='''+json.dumps(glyphs)+''';
document.getElementById('zoom').value=window.innerWidth<800?'1':'2';Object.assign(glyphs,{'0':['01110','10001','10011','10101','11001','10001','01110'],'2':['01110','10001','00001','00010','00100','01000','11111'],'4':['00010','00110','01010','10010','11111','00010','00010'],'5':['11111','10000','10000','11110','00001','00001','11110'],'6':['01110','10000','10000','11110','10001','10001','01110'],'7':['11111','00001','00010','00100','01000','01000','01000'],'8':['01110','10001','10001','01110','10001','10001','01110'],'9':['01110','10001','10001','01111','00001','00001','01110']});
function text(s,x,y,size){ctx.fillStyle='#FFF9F0';for(const ch of s){(glyphs[ch]||Array(7).fill('00000')).forEach((r,j)=>[...r].forEach((b,i)=>{if(b==='1')ctx.fillRect(x+i*size,y+j*size,size,size)}));x+=6*size}}
function draw(){ctx.imageSmoothingEnabled=false;ctx.fillStyle=document.getElementById('backdrop').value;ctx.fillRect(0,0,360,222);const put=(n,x,y)=>ctx.drawImage(ims[n],x,y);put('wave',66,24);put('diamond',143,16);put('pendant',140,56);put('settings',274,20);text('WAVE 1',119,40,2);put('energy',106,108);const amount=+document.getElementById('charge').value,w=Math.round(107*amount/100);if(w){ctx.save();ctx.beginPath();ctx.rect(126,115,w,8);ctx.clip();put('fill',126,115);ctx.restore()}document.getElementById('amount').value=amount+'%';put('ability',92,142);put('flourish',92,142);if(showEmblem)put('emblem',165,160);let count=Math.max(0,Math.min(99,parseInt(document.getElementById('count').value)||0)).toString();for(const [name,x] of [['potion',12],['bomb',284]]){put('item',x,144);ctx.drawImage(ims[name],x+8,152,48,48);const width=count.length*12+3;ctx.fillStyle='#11101B';ctx.fillRect(x+61-width,188,width,20);text(count,x+63-width,191,2)}const z=+document.getElementById('zoom').value;c.style.width=360*z+'px';c.style.height=222*z+'px'}
Promise.all(Object.entries(paths).map(([n,url])=>new Promise((resolve,reject)=>{const im=new Image;im.onload=()=>{ims[n]=im;resolve()};im.onerror=reject;im.src=url}))).then(()=>{document.querySelectorAll('select,input').forEach(el=>el.addEventListener('input',draw));document.getElementById('empty').onclick=()=>{document.getElementById('charge').value=0;draw()};document.getElementById('full').onclick=()=>{document.getElementById('charge').value=100;draw()};document.getElementById('emblem').onclick=()=>{showEmblem=!showEmblem;document.getElementById('emblem').textContent=showEmblem?'Hide Rattlebones emblem':'Show Rattlebones emblem';draw()};draw();document.body.dataset.ready='true'}).catch(()=>document.body.dataset.ready='error');
</script></html>'''
(HERE/'Review.html').write_text(html,encoding='utf8')
print('Built native family board and interactive Stage 3 asset review.')
