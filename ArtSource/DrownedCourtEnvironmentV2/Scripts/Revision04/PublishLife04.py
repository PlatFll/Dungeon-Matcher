from pathlib import Path
from PIL import Image,ImageDraw,ImageChops
import json,subprocess,hashlib,shutil
F=Path(r'C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher');R=F/'ArtSource/DrownedCourtEnvironmentV2';P=R/'Preview/Revision04';Q=R/'QA/Revision04';E=R/'Exports/Revision04'
protected=0
allowed=[(0,182,360,214),(0,259,31,355),(329,419,360,515),(31,576,95,640)]
for phase in ['Closed','Open']:
 for scale in [1,2,3]:
  old=Image.open(P/f'Unity-Before{phase}-{360*scale}x{640*scale}.png').convert('RGBA');new=Image.open(P/f'Unity-Revised{phase}-{360*scale}x{640*scale}.png').convert('RGBA')
  mask=Image.new('1',old.size,1);d=ImageDraw.Draw(mask)
  for x,y,X,Y in allowed:d.rectangle((x*scale,y*scale,X*scale-1,Y*scale-1),fill=0)
  diff=ImageChops.difference(old,new).convert('RGB');diff.paste((0,0,0),(0,0),ImageChops.invert(mask.convert('L')))
  assert not diff.getbbox(),(phase,scale,'Unexpected changed pixels',diff.getbbox())
  protected+=sum(mask.get_flattened_data())
assert Image.open(P/'LoopStart.png').tobytes()==Image.open(P/'LoopEnd.png').tobytes()
frames=[Image.open(p).convert('RGBA') for p in sorted((P/'Motion').glob('Frame-*.png'))];assert len(frames)==288
# Lossless animated WebP and exact individual PNGs are authoritative animation previews.
frames[0].save(P/'Unity-Animation.webp',save_all=True,append_images=frames[1:],duration=[83,83,84]*96,loop=0,lossless=True,method=6)
for name in ['Closed','Open']:
 b=Image.new('RGBA',(744,678),'#0d1d26');d=ImageDraw.Draw(b);d.text((8,8),'Before - Unity',fill='white');d.text((380,8),'Revised - Unity',fill='white')
 b.alpha_composite(Image.open(P/f'Unity-Before{name}-360x640.png').convert('RGBA'),(8,30));b.alpha_composite(Image.open(P/f'Unity-Revised{name}-360x640.png').convert('RGBA'),(380,30));b.save(P/f'Compare-{name}.png')
assets=[]
for path in sorted(E.glob('*.png')):
 im=Image.open(path).convert('RGBA');alpha=set(im.getchannel('A').get_flattened_data());assert alpha<={0,255},path
 assets.append({'name':path.name,'dimensions':im.size,'opaque_colors':len(set(c for c in im.get_flattened_data() if c[3])),'bounds':im.getbbox(),'binary_alpha':True,'sha256':hashlib.sha256(path.read_bytes()).hexdigest()})
closed=Image.open(E/'ClamA.png').convert('RGBA')
for letter in 'ABCDEFGHI':
 im=Image.open(E/f'Clam{letter}.png').convert('RGBA');assert im.size==(64,64);bounds=im.getbbox();assert bounds[0]>=3 and bounds[1]>=3 and bounds[2]<=61 and bounds[3]<=61,(letter,bounds)
 assert im.crop((0,47,64,64)).tobytes()==closed.crop((0,47,64,64)).tobytes(),letter
assert closed.tobytes()==Image.open(E/'ClamClosed.png').convert('RGBA').tobytes()
changed=subprocess.check_output(['git','diff','--name-only','HEAD'],cwd=F,text=True).splitlines()
assert not [n for n in changed if Path(n).suffix.lower() in ['.png','.ase','.aseprite','.anim','.controller','.prefab','.unity']],changed
record={'status':'PASS','protected_pixels_compared':protected,'native_assets':assets,'clam_canvas':[64,64],'clam_ground_row':59,'clam_lower_17_rows_locked':True,'closed_seed_exact':True,'loop_seconds':24,'loop_frames':288,'loop_exact':True,'old_art_and_scenes_unchanged':True,'production_scene_and_runtime_unchanged':True,'pixellab_generations':13,'validation':'Tools/Validate-Unity.ps1 passed; actual art-only Unity captures; no new production gameplay or device test.'}
(Q/'Verification.json').write_text(json.dumps(record,indent=2)+'\n')
html='''<!doctype html><html><head><meta charset="utf-8"><title>Drowned Court - floor and living details</title><style>body{background:#09171f;color:#d8e8e9;font:16px system-ui;margin:24px}h1{font-size:24px}p{max-width:1000px;line-height:1.5}button,input,select{margin:8px;padding:8px;background:#19323d;color:white;border:1px solid #537381} .pair{display:flex;gap:20px;flex-wrap:wrap}.pair>div{background:#10242c;padding:12px}canvas,img{image-rendering:pixelated;max-width:100%} .small{font-size:13px;color:#9ab8c0} .assets{display:flex;gap:24px;align-items:flex-end;flex-wrap:wrap}a{color:#88d6df}</style></head><body><h1>Drowned Court - continuous floor, natural clam, gentle coral sway</h1><p>The two floor courses now read as one surface. The clam has a rounded closed shell and a connected opening hinge. Two corals sway occasionally by one native pixel while their stone bases stay fixed.</p><p class="small">Actual Unity art-only scene captures. Original assets, old scenes, characters, UI and gameplay remain preserved. No production import or merge. PixelLab: 13 generations (still plus three animation trials); rejected trials are retained.</p><button id="play">Pause</button><label>Zoom<select id="zoom"><option value="1">Native 1x</option><option value="2">2x</option></select></label><input id="seek" type="range" min="0" max="287" value="0"><span id="time"></span><div class="pair"><div>Before - actual Unity frames<br><canvas id="before" width="360" height="640"></canvas></div><div>Revised - actual Unity frames<br><canvas id="after" width="360" height="640"></canvas></div></div><h2>Native clam and PixelLab motion cleaned in LibreSprite</h2><div class="assets"><div>Closed at 1x<br><img src="../../Exports/Revision04/ClamA.png" width="64"></div><div>Closed at 4x<br><img src="../../Exports/Revision04/ClamA.png" width="256"></div><div>Open at 4x<br><img src="../../Exports/Revision04/ClamI.png" width="256"></div><div>Hinge loop at 4x<br><img src="Clam-Preview.gif" width="256"></div></div><p><a href="Unity-RevisedClosed-1080x1920.png">1080x1920 closed</a> · <a href="Unity-RevisedOpen-1080x1920.png">1080x1920 open</a> · <a href="Unity-Animation.webp">Lossless Unity animation</a> · <a href="../../QA/Revision04/Verification.json">Validation</a></p><img src="Surfaces-Review.png"><script>let running=true,f=0,t0=performance.now(),offset=0;const n=288,store=[[],[]],cs=[before,after],ctx=cs.map(c=>c.getContext('2d'));ctx.forEach(c=>c.imageSmoothingEnabled=false);for(let j=0;j<2;j++)for(let i=0;i<n;i++){let im=new Image();im.src=(j?'Motion/':'../Revision03/Motion/')+'Frame-'+String(i).padStart(3,'0')+'.png';store[j].push(im)}function draw(){ctx.forEach((c,j)=>{let im=store[j][f];if(im.complete)c.drawImage(im,0,0)});seek.value=f;time.textContent=(f/12).toFixed(2)+' s';}play.onclick=()=>{running=!running;play.textContent=running?'Pause':'Play';offset=f;t0=performance.now()};seek.oninput=()=>{f=+seek.value;offset=f;t0=performance.now();draw()};zoom.onchange=()=>cs.forEach(c=>{c.style.width=(360*+zoom.value)+'px';c.style.height=(640*+zoom.value)+'px'});function tick(t){if(running)f=(offset+Math.floor((t-t0)*.012))%n;draw();requestAnimationFrame(tick)}requestAnimationFrame(tick);</script></body></html>'''
(P/'Review.html').write_text(html,encoding='utf-8')
print(json.dumps({k:v for k,v in record.items() if k!='native_assets'},indent=2))
