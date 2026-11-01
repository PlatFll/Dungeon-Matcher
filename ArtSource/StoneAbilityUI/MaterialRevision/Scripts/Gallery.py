from pathlib import Path
from PIL import Image
import json,sys
R=Path(sys.argv[1]);A=R/'ArtSource/StoneAbilityUI/MaterialRevision';P=A/'Preview'
characters={'skeleton':'Rattlebones','bardley':'Bardley','kobold_gunner':'Kobold Gunner','dungeon_attender':'Minotaur'}
for key in characters:
 d=P/'Characters'/key
 assert (d/'Ready.png').exists(),key
 for stem,out in [('Rise','ReadyCue.gif'),('Frame','Ability.gif')]:
  paths=sorted(d.glob(stem+'[0-9]*.png'));assert paths,(key,stem)
  frames=[Image.open(p).convert('RGB') for p in paths]
  palette=frames[0].quantize(colors=256,method=Image.Quantize.MEDIANCUT)
  frames=[im.quantize(palette=palette,dither=Image.Dither.NONE) for im in frames]
  durations=[33]*len(frames);durations[-1]=900
  frames[0].save(d/out,save_all=True,append_images=frames[1:],duration=durations,loop=0,optimize=True,disposal=1)
 Image.open(d/'Ready.png').crop((170,1110,560,1240)).save(d/'ReadyHUD.png')
zones=['drowned-court','dungeon','magical-forest','ironvein-excavation','frostwake-peaks']
css='''body{background:#171b23;color:#eee;font:16px system-ui;margin:24px auto;max-width:1500px;padding:0 20px}h1{font-size:27px}h2{margin-top:40px}p{max-width:1000px;color:#c8c8d4}img{image-rendering:pixelated;background:#202533;max-width:100%;height:auto}a{color:#c7d9ff}select,button{background:#30353f;color:white;border:1px solid #777;padding:9px;margin:6px}figure{margin:0}figcaption{margin:8px 0}.row{display:flex;gap:20px;flex-wrap:wrap}.row figure{flex:1;min-width:290px}.scenes img{width:540px}.small{font-size:13px}'''
html=f'''<!doctype html><meta charset="utf-8"><title>Dungeon Matcher — Carved Stone UI</title><style>{css}</style>
<h1>Carved stone controls · larger wave and Settings</h1><p>PixelLab-generated matte stone, finished in LibreSprite. Chipped rock edges replace polished bevels and rivets. The wave plaque is up to 176×48 native pixels; Settings is 56×56. Existing character engraving shapes, ready motion and gameplay are preserved. Not merged.</p>
<p>40 PixelLab generations used for this revision; 4,643 remain. Exact PNGs and editable ASE sources are retained. GIFs are palette-limited review encodings of actual Unity frames.</p>
<h2>Actual Game: before / revised</h2><select id="zone">{''.join(f'<option>{z}</option>' for z in zones)}</select><select id="size"><option>720x1280</option><option>1080x2340</option><option>360x640</option><option>1080x2400-safe</option></select>
<div class="row scenes"><figure><figcaption>Before — smaller, metallic controls</figcaption><a id="beforeLink"><img id="before"></a></figure><figure><figcaption>Revised — actual Game scene</figcaption><a id="afterLink"><img id="after"></a></figure></div>
<h2>Native artwork comparison</h2><p><a href="ControlsNative.png">Open native 1× sheet</a> · enlarged view below is exactly 2× nearest-neighbor.</p><img src="Controls2x.png" width="880" alt="Old metallic UI beside new matte carved stone assets">
<h2>Character plates and existing ready motion</h2><select id="character">{''.join(f'<option value="{k}">{v}</option>' for k,v in characters.items())}</select><button onclick="refreshCharacter()">Replay</button>
<div class="row"><figure><figcaption>Ready lift — actual Game capture</figcaption><img id="rise" width="360"></figure><figure><figcaption>Ability / ready glow — actual Game capture</figcaption><img id="ability" width="360"></figure><figure><figcaption>Exact ready HUD PNG</figcaption><img id="hud"><p id="timing"></p><a id="full">Open full screenshot</a></figure></div>
<p><a href="../README.md">Validation and source notes</a> · <a href="../QA/NativeValidation.json">Native checks</a></p>
<script>
function refreshCharacter(){{let d='Characters/'+document.getElementById('character').value+'/';document.getElementById('rise').src=d+'ReadyCue.gif?'+Date.now();document.getElementById('ability').src=d+'Ability.gif?'+Date.now();document.getElementById('hud').src=d+'ReadyHUD.png';document.getElementById('full').href=d+'Ready.png';document.getElementById('timing').textContent='';fetch(d+'Timing.txt').then(r=>r.ok?r.text():'').then(t=>document.getElementById('timing').textContent=t)}}
function scene(){{let n=document.getElementById('zone').value+'-'+document.getElementById('size').value+'.png';for(let k of ['before','after']){{let p=k[0].toUpperCase()+k.slice(1)+'/'+n;document.getElementById(k).src=p;document.getElementById(k+'Link').href=p}}}}
document.getElementById('character').onchange=refreshCharacter;document.getElementById('zone').onchange=scene;document.getElementById('size').onchange=scene;refreshCharacter();scene();
</script>'''
(P/'Review.html').write_text(html,encoding='utf-8')
(A/'.gitignore').write_text('Preview/Characters/**/Frame[0-9]*.png\nPreview/Characters/**/Rise[0-9]*.png\n')
(A/'Scripts/Gallery.py').write_bytes(Path(__file__).read_bytes())
print('Actual Unity frame GIFs and comparison gallery complete')
