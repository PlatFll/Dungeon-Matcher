"""Publish actual Unity captures as an animation and a local review page."""
from pathlib import Path
from PIL import Image
import json, shutil, sys

HERE=Path(__file__).resolve().parent; ROOT=HERE.parents[1]
source=ROOT/'.utmp/CourtFloodReview'
dest=Path(sys.argv[1]);dest.mkdir(parents=True,exist_ok=True)
frames=[Image.open(p).convert('RGB') for p in sorted((source/'Frames').glob('*.png'))]
assert len(frames)==210,len(frames)
assert all(im.size==(720,1280) for im in frames)
# Lossless animated WebP keeps the actual captured pixels, at the captured 30 fps.
frames[0].save(dest/'WaterFill.webp',save_all=True,append_images=frames[1:],lossless=True,quality=100,method=4,loop=0,
 duration=[33,33,34]*70)
# Portable compact animation; clearly secondary to the lossless Unity preview.
small=[im.resize((360,640),Image.Resampling.NEAREST).quantize(colors=256,dither=Image.Dither.NONE) for im in frames[::2]]
small[0].save(dest/'WaterFill.gif',save_all=True,append_images=small[1:],duration=[60,70,70]*35,loop=0,disposal=1)
for p in source.glob('*.png'):shutil.copy2(p,dest/p.name)
for name in ['WaterSurface.aseprite','WaterSurface-strip.png','WaterSurface-preview.gif','manifest.json']:
    shutil.copy2(HERE/name,dest/name)
for name in ['tests.xml','Result.txt']:shutil.copy2(source/name,dest/name)
frames[42].save(dest/'Rising-720x1280.png')
html='''<!doctype html><meta charset="utf-8"><title>Drowned Court · rising-water review</title>
<style>body{margin:0;background:#0c1721;color:#e3f0f4;font:16px system-ui}main{max-width:1180px;margin:28px auto;padding:0 24px}h1{font-size:26px}p{color:#acc6d2;line-height:1.6}.grid{display:flex;flex-wrap:wrap;gap:22px}.card{background:#152632;padding:18px;border-radius:12px;flex:1;min-width:280px}img{max-width:100%;image-rendering:pixelated}.phone{width:360px;display:block;margin:auto}.controls{display:flex;gap:12px;align-items:center;margin:12px 0}button,a{background:#294656;color:#e8f6ff;border:0;padding:9px 15px;border-radius:6px;text-decoration:none;cursor:pointer}small{color:#92afbc}select{padding:7px}label{display:block;margin-bottom:10px}</style>
<main><h1>Drowned Court — rising water</h1><p>Actual Unity Game scene. Gentle surface waves and small bubbles accompany the rise, followed by a quieter underwater tint. The approved environment and gameplay artwork are unchanged. This effect is awaiting your approval and has not been merged.</p>
<div class="grid"><section class="card"><h2>Rise · underwater · drain</h2><img class="phone" id="motion" src="WaterFill.webp"><div class="controls"><button onclick="document.querySelector('#motion').src='WaterFill.webp?replay='+Date.now()">Replay</button><a href="WaterFill.gif">GIF</a></div><small>7 seconds · 30 fps lossless Unity capture</small></section>
<section class="card"><h2>Inspect a still</h2><label><select id="state" onchange="show()"><option value="Dry">Dry</option><option value="Rising">Water rising</option><option value="Flooded" selected>Fully underwater</option><option value="SafeArea">Tall phone + safe area</option></select> <select id="size" onchange="show()"><option>720x1280</option><option>1080x1920</option><option>1080x2400</option></select></label><img class="phone" id="still" src="Flooded-720x1280.png"><small>Characters, controls, gems and the approved Court scenery remain intact.</small></section></div>
<section class="card" style="margin-top:22px"><h2>Native water surface</h2><img src="WaterSurface-preview.gif" width="384" height="192"><p>PixelLab-generated still and 16-frame motion, cleaned and verified through LibreSprite. Twelve generations used. Native surface: 128 × 64.</p><a href="WaterSurface.aseprite">Editable LibreSprite source</a> <a href="WaterSurface-strip.png">Frame strip</a><p>Unity checks passed at 720×1280, 1080×1920 and 1080×2400, plus a safe-area case. Pausing and reduced motion work; AIR, move count and health remain unchanged by the effect. No real-device test.</p></section>
<script>function show(){let s=document.querySelector('#state').value,z=document.querySelector('#size').value;if(s==='Rising')z='720x1280';if(s==='SafeArea')z='1080x2400';document.querySelector('#still').src=s+'-'+z+'.png'}</script></main>'''
(dest/'Review.html').write_text(html,encoding='utf-8')
print(json.dumps({'preview':str(dest/'Review.html'),'animation_bytes':(dest/'WaterFill.webp').stat().st_size,'frames':len(frames)},indent=2))
