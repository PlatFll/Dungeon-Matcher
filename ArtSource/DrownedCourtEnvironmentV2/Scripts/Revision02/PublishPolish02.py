from pathlib import Path
from PIL import Image,ImageDraw
import json,hashlib,csv,shutil
F=Path(r'C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher');R=F/'ArtSource/DrownedCourtEnvironmentV2';P=R/'Preview/Revision02';Q=R/'QA/Revision02';W=Path(__file__).parent
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
data=json.loads((R/'Native/Revision02/Manifest.json').read_text())
frames=[Image.open(p).convert('RGB') for p in sorted((P/'Motion').glob('Frame-*.png'))]
assert len(frames)==288
# The HTML uses original lossless Unity captures; GIF is a convenient indexed preview.
sample=Image.new('RGB',(360*4,640))
for i,n in enumerate([0,60,120,210]):sample.paste(frames[n],(360*i,0))
palette=sample.quantize(colors=256,method=Image.Quantize.MEDIANCUT)
gif=[im.quantize(palette=palette,dither=Image.Dither.NONE) for im in frames]
gif[0].save(P/'Underwater-Motion.gif',save_all=True,append_images=gif[1:],duration=[80,80,90]*96,loop=0,optimize=True,disposal=1)
board=Image.new('RGB',(744,668),(16,24,32));draw=ImageDraw.Draw(board)
draw.text((8,7),'BEFORE - Revision 01',fill='white');draw.text((380,7),'REVISED - native Unity render',fill='white')
board.paste(Image.open(P/'Unity-Before-360x640.png').convert('RGB'),(8,24));board.paste(Image.open(P/'Unity-Revised-360x640.png').convert('RGB'),(380,24));board.save(P/'Before-After.png')
atlas=Image.new('RGBA',(444,344),(13,28,36,255));draw=ImageDraw.Draw(atlas)
for i,a in enumerate(data['assets']):
 x=4+(i%5)*88;y=4+(i//5)*68;draw.text((x,y),a['name'],fill='white');im=Image.open(R/f"Exports/Revision02/{a['name']}.png").convert('RGBA')
 # Tall source pieces have their own previews below. This sheet contains only native-size assets.
 if im.height<=48:atlas.alpha_composite(im,(x,y+14))
 else:draw.text((x,y+20),f"{im.width} x {im.height}",fill='#74aeb7')
atlas.save(P/'Native-Assets.png')
html='''<!doctype html><meta charset="utf-8"><title>Drowned Court — underwater polish</title><style>
body{margin:24px;background:#0b151e;color:#dbe8ef;font:16px system-ui}h1{font-size:24px}p{max-width:1000px;line-height:1.5}.row{display:flex;gap:24px;flex-wrap:wrap}figure{margin:0}figcaption{margin:8px 0}img,canvas{image-rendering:pixelated}button,select{padding:9px;background:#183640;color:white;border:1px solid #386876}a{color:#93d7e1}.scene{width:360px;height:640px}input{width:360px}summary{cursor:pointer;padding:12px}section{margin:28px 0}.asset{display:inline-block;width:140px;vertical-align:top;margin:10px}.asset img{display:block;margin-top:8px}small{color:#99b9c6}</style>
<h1>Drowned Court — underwater polish</h1><p>Coral cleanup, darker portrait masonry, submerged stone backing, twelve varied bubble paths and a small opening clam. Existing actors, frame geometry, board cells and shared controls are preserved. Review scene only; not merged or installed into gameplay.</p>
<p><button id="play">Pause</button> <select id="zoom"><option value="1">Native 1×</option><option value="2">Nearest-neighbor 2×</option></select> <span id="clock"></span><br><input id="seek" type="range" min="0" max="287" value="0"></p>
<div class="row"><figure><figcaption>Before — Revision 01</figcaption><img class="scene" src="Unity-Before-360x640.png"></figure><figure><figcaption>Revised — actual Unity animation frames</figcaption><canvas id="motion" class="scene" width="360" height="640"></canvas></figure></div>
<p>The 24-second loop uses 288 lossless Unity renders at 12 frames/sec. Movement stays on whole native pixels; bubble rings rise, drift and pop. Clam releases occur while the shell is open.</p>
<p><a href="Unity-Revised-720x1280.png">720×1280 still</a> · <a href="Unity-Revised-1080x1920.png">1080×1920 still</a> · <a href="Unity-Revised-1080x2400.png">1080×2400 tall still</a> · <a href="Underwater-Motion.gif">GIF motion preview</a> · <a href="../../Revision02Report.md">Verification report</a></p>
<section><h2>Native source assets</h2><small>Shown at actual resolution. Click to inspect the original PNG. All have editable LibreSprite sources.</small><div>ASSETS</div></section>
<script>const frames=Array.from({length:288},(_,i)=>{let im=new Image();im.src='Motion/Frame-'+String(i).padStart(3,'0')+'.png';return im}),ctx=document.querySelector('canvas').getContext('2d');let frame=0,playing=true,last=0;ctx.imageSmoothingEnabled=false;const seek=document.querySelector('#seek'),clock=document.querySelector('#clock');function show(){const im=frames[frame];if(im.complete&&im.naturalWidth){ctx.clearRect(0,0,360,640);ctx.drawImage(im,0,0)}seek.value=frame;clock.textContent=(frame/12).toFixed(2)+' / 24 s'}function tick(t){if(playing&&t-last>=1000/12){frame=(frame+1)%288;last=t;show()}requestAnimationFrame(tick)}frames[0].onload=show;requestAnimationFrame(tick);document.querySelector('#play').onclick=e=>{playing=!playing;e.target.textContent=playing?'Pause':'Play'};seek.oninput=()=>{frame=+seek.value;show()};document.querySelector('#zoom').onchange=e=>document.querySelectorAll('.scene').forEach(n=>{n.style.width=(360*+e.target.value)+'px';n.style.height=(640*+e.target.value)+'px'});</script>'''
items=''.join(f'<div class="asset">{a["name"]}<br><small>{a["width"]}×{a["height"]}</small><a href="../../Exports/Revision02/{a["name"]}.png"><img src="../../Exports/Revision02/{a["name"]}.png"></a></div>' for a in data['assets'])
(P/'Review.html').write_text(html.replace('ASSETS',items),encoding='utf-8')
progress=R/'Progress/PIPELINE_STATE.json';state=json.loads(progress.read_text());state['total_pixellab_spending']=state['generation_spend']=94
state['focused_revision_02']={'status':'COMPLETE_PENDING_USER_REVIEW','scope':'Six-stage coral, portrait, masonry and ambient-life polish','report':'Revision02Report.md','preview':'Preview/Revision02/Review.html','generation_spend':5,'new_assets':21,'motion_tracks':13,'formal_later_stages_unchanged':True};progress.write_text(json.dumps(state,indent=2)+'\n')
catalog=R/'Progress/ASSET_CATALOG.csv';rows=list(csv.DictReader(catalog.open(newline='')));fields=list(rows[0]);rows=[r for r in rows if r['stage']!='Revision02']
for a in data['assets']:
 n=a['name'];im=Image.open(R/f'Exports/Revision02/{n}.png').convert('RGBA');colors=len({p for p in im.getdata() if p[3]})
 rows.append(dict(zip(fields,[f'DC-REV02-{n.upper()}','Revision02','AmbientClam' if n.startswith('Clam') else 'AmbientBubble' if n.startswith('Bubble') else 'Polish',n,'PixelLab + LibreSprite' if n.startswith('Clam') else 'LibreSprite native edit',a['width'],a['height'],colors,'binary','native canvas; existing layout unchanged','RawPixelLab/Revision02/ClamSeed.png' if n.startswith('Clam') else 'Native source / exact authored pixels',f'Native/Revision02/{n}.ase',f'Exports/Revision02/{n}.png',f'Assets/_Game/Art/DrownedCourtEnvironmentV2/Revision02/{n}.png','CANDIDATE','PASS','PENDING','Additive review copy. See Revision02Report.md.'])))
with catalog.open('w',newline='') as f:writer=csv.DictWriter(f,fieldnames=fields);writer.writeheader();writer.writerows(rows)
ledger=R/'Progress/PIXELLAB_JOB_LEDGER.csv';rows=list(csv.DictReader(ledger.open(newline='')));fields=list(rows[0]);rows=[r for r in rows if r['stage']!='Revision02']
rows.append(dict(zip(fields,['Revision02','2026-10-26','ClamSeed','PixelLab create_image_pro_flash','7a4a16e1-426b-43a9-837d-5d792460c7ba','RawPixelLab/Revision02/Lineage.json','705c4c8379d56a262e287d256664db8d180a79917f4a84289bbb813730b9616c',64,64,5,5,'RawPixelLab/Revision02/ClamSeed.png',sha(R/'RawPixelLab/Revision02/ClamSeed.png'),'CANDIDATE_AFTER_LIBRESPRITE','Small grounded clam; five native poses; full original source retained.','Native/Revision02/Clam4.ase','Assets/_Game/Art/DrownedCourtEnvironmentV2/Revision02/Clam4.png','553.20 -> 548.20 remaining; 5 revision / 94 cumulative. No other generations.'])))
with ledger.open('w',newline='') as f:writer=csv.DictWriter(f,fieldnames=fields);writer.writeheader();writer.writerows(rows)
(Q/'BalanceAfter.json').write_text(json.dumps({'remaining_generations':548.2,'remaining_before':553.2,'revision_spend':5,'cumulative_pack_spend':94,'subscription':'Pixel Artisan Tier 2'},indent=2)+'\n')
for n in ['PublishPolish02.py','PrepareQA02.py','VerifyPolish02.py']:
 if (W/n).resolve()!=(R/'Scripts/Revision02'/n).resolve():shutil.copy2(W/n,R/'Scripts/Revision02'/n)
print('Gallery, lossless motion preview, GIF, asset catalog and generation ledger written.')
