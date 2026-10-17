"""Package source-layer inspection and actual Unity captures, without editing art."""
from pathlib import Path
import json,html,shutil
K=Path(__file__).resolve().parent;P=K.parents[2]
layout=json.loads((K/'layout.json').read_text())
checks=json.loads((K/'checks.json').read_text())
names={x['name']:x for x in checks['assets']}
modules=layout['modules'];composite=[x for x in modules if x['composite']]
cards=''.join(f'<article><h3>{html.escape(m["name"])}</h3><img loading="lazy" src="native/{m["name"]}.png"><p>{m["width"]} × {m["height"]} · {names[m["name"]]["opaque_colors"]} colors</p><p>{html.escape(m["role"])}</p><a href="native/{m["name"]}.aseprite">LibreSprite source</a> · <a href="native/{m["name"]}.png">PNG</a></article>' for m in modules)
captures=P/'.utmp/ThreeZonePresentation/after'
shots=[]
for suffix,label in [('1080','1080 × 1920'),('720x1280','720 × 1280'),('1080x2400','1080 × 2400 with safe area')]:
    source=captures/('dungeon-'+suffix+'.png')
    # Only accept captures newer than this revision's native build receipt.
    if source.exists() and source.stat().st_mtime>(K/'build-receipt.json').stat().st_mtime:
        dest=K/'review'/('Unity-'+suffix+'.png');shutil.copyfile(source,dest)
        shots.append(f'<figure><figcaption>{label} · actual Unity</figcaption><img src="review/{dest.name}" width="360"></figure>')
controls=''.join(f'<label><input type="checkbox" data-name="{m["name"]}" checked>{m["name"]}</label>' for m in composite)
page='''<!doctype html><meta charset="utf-8"><title>Dungeon · LibreSprite dissection</title>
<style>body{margin:28px;background:#15131e;color:#e7dfef;font:15px system-ui}h1{font-size:26px}h2{font-size:20px;margin-top:32px}a{color:#cfadff}p{max-width:1000px;line-height:1.5}button,select{background:#3b2d50;border:1px solid #83628f;color:white;padding:8px;margin:3px}img,canvas{image-rendering:pixelated}figure{margin:0}figcaption{margin:8px 0}.row{display:flex;gap:22px;align-items:flex-start;flex-wrap:wrap}.scroll{overflow:auto;max-width:100%;max-height:900px;background-color:#22202b;background-image:linear-gradient(45deg,#292533 25%,transparent 25%),linear-gradient(-45deg,#292533 25%,transparent 25%),linear-gradient(45deg,transparent 75%,#292533 75%),linear-gradient(-45deg,transparent 75%,#292533 75%);background-size:16px 16px;background-position:0 0,0 8px,8px -8px,-8px 0}aside{min-width:230px}label{display:block;padding:4px;cursor:pointer}.cards{display:grid;grid-template-columns:repeat(auto-fit,minmax(240px,1fr));gap:16px}.cards article{padding:16px;border:1px solid #493957;background:#201c2a}.cards img{max-width:100%;min-height:24px;object-fit:contain;object-position:left}.cards h3{margin:0 0 12px}.badge{display:inline-block;padding:5px 9px;background:#49315f;border-radius:5px}</style>
<h1>Dungeon · LibreSprite dissection and cleanup</h1>
<p><span class="badge">Integrated review candidate</span> Your new three-enemy mockup supplies the composition. Actors, gems, counters and labels remain live game content. No generation service was used.</p>
<p><a href="native/DungeonLayered.aseprite">Open layered LibreSprite master</a> · <a href="native/TopHudBackground.aseprite">Top HUD background source</a> · <a href="checks.json">Native validation</a></p>
<h2>Reference and cleaned artwork</h2><div class="row"><figure><figcaption>Supplied mockup</figcaption><img src="reference/RequestedDungeon.png" width="470"></figure><figure><figcaption>Cleaned art only · native 470 × 836</figcaption><img src="native/DungeonArtOnly.png" width="470"></figure></div>
<p>The concealed floor and masonry were reconstructed, then checked visually. Source sampling uses a consistent 2× grid; the generated mockup does not establish an original native resolution. Colors use 84 authored material shades. Character palette limits are not applied to this environment.</p>
<h2>Inspect the layers</h2><p>Toggle layers to inspect their transparent cutouts. Zoom uses nearest-neighbor display.</p><div class="row"><aside><button id="all">Show all</button><button id="none">Hide all</button><select id="zoom"><option value="1">Native 1×</option><option value="2">2×</option><option value="3">3×</option></select>__CONTROLS__</aside><div class="scroll"><canvas id="stage" width="470" height="836"></canvas></div></div>
<h2>Actual Unity comparison</h2><div class="row"><figure><figcaption>Previous Dungeon · 1080 × 1920</figcaption><img src="review/PreviousUnity.png" width="360"></figure>__SHOTS__</div>
<h2>Editable asset library</h2><div class="cards">__CARDS__</div>
<script>const layers=__LAYERS__;const images={};const stage=document.getElementById('stage'),ctx=stage.getContext('2d');ctx.imageSmoothingEnabled=false;function draw(){ctx.clearRect(0,0,470,836);for(const m of layers){const cb=document.querySelector('[data-name="'+m.name+'"]'),im=images[m.name];if(cb.checked&&im&&im.complete&&im.naturalWidth)ctx.drawImage(im,m.x,m.y)}}for(const m of layers){const im=new Image();im.onload=draw;im.src='native/'+m.name+'.png';images[m.name]=im}document.querySelectorAll('input').forEach(c=>c.onchange=draw);for(const [id,value] of [['all',true],['none',false]])document.getElementById(id).onclick=()=>{document.querySelectorAll('input').forEach(c=>c.checked=value);draw()};document.getElementById('zoom').onchange=e=>{stage.style.width=470*e.target.value+'px';stage.style.height=836*e.target.value+'px'};</script>'''
page=page.replace('__CONTROLS__',controls).replace('__SHOTS__',''.join(shots) or '<p>Unity capture pending.</p>').replace('__CARDS__',cards).replace('__LAYERS__',json.dumps(composite))
(K/'Review.html').write_text(page,encoding='utf-8')
print(f'Packaged {len(modules)} source modules and {len(shots)} fresh Unity captures.')
