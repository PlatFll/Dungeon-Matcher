"""Build the local review page from real Unity captures and native source PNGs."""
from pathlib import Path
from PIL import Image
import json, html
HERE=Path(__file__).resolve().parent
manifest=json.loads((HERE/'Manifest.json').read_text())
cases=[('dungeon-720x1280','Dungeon · 720 × 1280'),('dungeon-360x640','Dungeon · 360 × 640'),
       ('dungeon-1080x1920','Dungeon · 1080 × 1920'),('dungeon-1080x2400-safe','Dungeon · 1080 × 2400 · simulated safe area'),
       ('drowned-court-1080x2340','Drowned Court · 1080 × 2340')]
for key,_ in cases:
    for suffix in ('before','after'):
        p=HERE/'Preview'/f'{key}-{suffix}.png'
        assert p.exists(),p
        im=Image.open(p);im.crop((0,0,im.width,round(im.height*.31))).save(HERE/'Preview'/f'{key}-{suffix}-top.png')
cards=''
for name,row in manifest['assets'].items():
    w,h=row['dimensions']
    cards+=f'''<article class="asset"><h3>{name}</h3><div class="native"><img src="PNG/{name}.png" width="{w}" height="{h}" alt="{name} native pixels"></div>
      <div class="enlarged"><img src="Preview/{name}-4x.png" alt="{name} enlarged four times with nearest neighbor"></div>
      <p>{w} × {h} · {row['opaque_colors']} opaque colors · binary transparency</p>
      <a href="PNG/{name}.png">Native PNG</a> · <a href="Editable/{name}.ase">LibreSprite source</a></article>'''
options=''.join(f'<option value="{key}">{html.escape(label)}</option>' for key,label in cases)
page='''<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
<title>Royal bevel — Stage 2 top HUD</title><style>
:root{color-scheme:dark}*{box-sizing:border-box}body{margin:0;background:#101018;color:#e7e5ed;font:16px/1.6 system-ui,sans-serif}main{max-width:1250px;margin:auto;padding:32px 24px 64px}h1{font-size:32px;line-height:1.2}h2{margin-top:36px}p{color:#bdb8cb}.tag{color:#e8b84d;font-weight:650;letter-spacing:1px;text-transform:uppercase;font-size:12px}a{color:#c9b8f5}button,select{font:inherit;color:inherit;background:#292638;border:1px solid #777185;border-radius:6px;padding:7px 12px}button:hover{background:#3b3450}.toolbar{display:flex;gap:10px;flex-wrap:wrap;align-items:center;margin:24px 0}.compare{display:grid;grid-template-columns:1fr 1fr;gap:20px}figure{margin:0;background:#191824;padding:12px;border:1px solid #34313f;border-radius:10px;overflow:auto}figcaption{font-size:14px;margin-bottom:10px;color:#c4bdd3}img{image-rendering:pixelated;vertical-align:middle}.shot{display:block;width:100%;height:auto}.assets{display:grid;grid-template-columns:repeat(2,minmax(0,1fr));gap:16px}.asset{padding:18px;background:#1b1927;border:1px solid #34313f;border-radius:10px;min-width:0}.native{min-height:88px;display:flex;align-items:center;background:#252232;padding:12px;overflow:auto}.enlarged{overflow:auto;padding:12px 0;min-height:112px}h3{margin:0 0 12px}.note{padding:16px 20px;border-left:3px solid #e8b84d;background:#1c1925}.reference{display:flex;gap:24px;align-items:center;padding:16px;background:#252232;overflow:auto}.checks{display:flex;gap:14px;flex-wrap:wrap}.checks span{background:#222936;padding:8px 14px;border-radius:5px;font-size:14px}details{margin:20px 0}summary{cursor:pointer}footer{border-top:1px solid #393343;margin-top:32px;padding-top:16px}dialog{max-width:95vw;max-height:95vh;background:#17151f;border:1px solid #8b819d}dialog img{max-width:none}dialog button{position:sticky;left:0;top:0;display:block}@media(max-width:750px){.compare,.assets{grid-template-columns:1fr}main{padding:18px 12px}h1{font-size:26px}}
</style><main>
<div class="tag">A selected · Stage 2 · review checkpoint</div><h1>Royal bevel, on the real battle HUD</h1>
<p>LibreSprite-cleaned silver wave plaque and square Settings control. The screenshots below are actual Unity Game captures with Rattlebones, enemies, gems and the existing HUD.</p>
<div class="note">Temporary Editor preview only. Production skin routing, the bottom HUD and all zone artwork remain unchanged. Stage 3 has not started; nothing is merged.</div>
<div class="toolbar"><label for="view">Capture</label><select id="view">OPTIONS</select><button id="crop">Show full gameplay</button><button id="pixels">Open after at physical pixels</button></div>
<div class="compare"><figure><figcaption>Before — existing live controls</figcaption><img id="before" class="shot" alt="Before: real Unity combat"></figure><figure><figcaption>After — selected A top controls</figcaption><img id="after" class="shot" alt="After: real Unity combat with Royal bevel"></figure></div>
<p id="capture-note">Overview fitted to this page. Open the physical-pixel capture to judge exact rendering.</p>
<div class="checks"><span>5 gameplay layouts</span><span>Settings opens and pauses</span><span>Dynamic WAVE 1–99999 fit</span><span>0 new PixelLab generations</span></div>
<h2>Native source pieces</h2><p>Each card starts at native 1×, followed by an exact 4× inspection view. Separate ornaments keep the corners and decorations undistorted when the wave frame changes width. Settings remains 56 × 56, including transparent padding.</p><div class="assets">CARDS</div>
<details><summary>Selected A source and unchanged golden reference</summary><div class="reference"><img src="Inputs/A_Wave.png" alt="Selected A wave source"><img src="Inputs/A_Settings.png" alt="Selected A settings source"><img src="../Stage01/Inputs/Rattlebones.png" alt="Unchanged Rattlebones reference"></div><p>Same PixelLab source, palette and design. The straight wave rails are cleaned and repeated to the existing control footprint; the native diamond and skull are detached. The gear is repositioned by one pixel for centering.</p></details>
<footer><p><a href="README.md">Handoff and limits</a> · <a href="Manifest.json">Native sizes and source hashes</a> · <a href="QA/PreservationValidation.json">Preservation checks</a> · <a href="QA/UnityTests.xml">Unity test results</a></p><p>Unity 6000.3.19f1 compilation and Editor Play Mode checks. Simulated safe area; no Android build or real-device test. Stage 1 used 29 PixelLab generations; Stage 2 used 0.</p></footer></main>
<dialog id="full"><button id="close">Close native capture</button><img id="full-image" alt="Full physical pixel Unity capture"></dialog>
<script>const view=document.getElementById('view'),crop=document.getElementById('crop');let topOnly=true;function show(){for(const mode of ['before','after'])document.getElementById(mode).src='Preview/'+view.value+'-'+mode+(topOnly?'-top':'')+'.png';crop.textContent=topOnly?'Show full gameplay':'Show top HUD';}view.onchange=show;crop.onclick=()=>{topOnly=!topOnly;show()};document.getElementById('pixels').onclick=()=>{document.getElementById('full-image').src='Preview/'+view.value+'-after.png';document.getElementById('full').showModal()};document.getElementById('close').onclick=()=>document.getElementById('full').close();show();</script></html>'''
(HERE/'Review.html').write_text(page.replace('OPTIONS',options).replace('CARDS',cards),encoding='utf-8')
print('Built Stage 2 review from ten real Unity screenshots.')
