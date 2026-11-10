"""Package real Unity captures and native artwork for this focused review."""
from pathlib import Path
from PIL import Image
import shutil

here=Path(__file__).resolve().parent
root=here.parents[2]
out=here/'Preview';out.mkdir(exist_ok=True)
capture=root/'.utmp/ForestValidation/ZoneEnvironment'
for name in [f'hearth-flame-{i:02d}' for i in range(8)]+[
    'marine-net-full-tile-1080x1920','marine-net-full-tile-360x640','marine-net-full-tile-1080x2400']:
    shutil.copy2(capture/(name+'.png'),out/(name+'.png'))

# Integer enlargement for the chat preview; the native animation remains separate.
gif=Image.open(here/'HearthFlame.gif');frames=[]
for i in range(gif.n_frames):
    gif.seek(i);frames.append(gif.convert('RGBA').resize((192,192),Image.Resampling.NEAREST))
frames[0].save(out/'HearthFlame-6x.gif',save_all=True,append_images=frames[1:],duration=120,loop=0,disposal=2)
Image.open(here/'MarineSnare.png').resize((256,256),Image.Resampling.NEAREST).save(out/'MarineSnare-4x.png')

(here/'Review.html').write_text('''<!doctype html>
<html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
<title>Hearth flame &amp; full-tile Marine Net</title>
<style>
:root{color-scheme:dark}*{box-sizing:border-box}body{margin:0;background:#11151e;color:#edf0f5;font:16px/1.55 system-ui,sans-serif}
main{max-width:1300px;margin:auto;padding:32px}h1{font-size:32px;margin:0}h2{font-size:22px;margin:0 0 18px}p{color:#bac4d3}a{color:#a8caff}
.badge{font-size:13px;letter-spacing:.08em;color:#a4e6cc}.grid{display:grid;grid-template-columns:repeat(2,minmax(0,1fr));gap:24px;margin:24px 0}
section{background:#1b2230;border:1px solid #343f52;border-radius:12px;padding:24px}img{image-rendering:pixelated;max-width:100%}
.samples{display:flex;align-items:end;gap:28px;flex-wrap:wrap}.sample{text-align:center}.sample p{font-size:13px;margin:8px 0}
.stage{background:repeating-conic-gradient(#283141 0% 25%,#222a38 0% 50%) 0/16px 16px;display:grid;place-items:center;width:160px;height:160px}
.game{width:360px;display:block;margin:auto}.shots{display:flex;gap:20px;flex-wrap:wrap;align-items:start}.shots img{width:360px}.caption{font-size:13px}
button,select{padding:9px 14px;background:#2b3950;color:white;border:1px solid #657590;border-radius:6px;cursor:pointer}button{margin-bottom:14px}
@media(max-width:780px){main{padding:16px}.grid{grid-template-columns:1fr}.stage{width:128px;height:128px}}
</style><main>
<div class="badge">DUNGEON MATCHER · REVIEW CANDIDATE · NOT MERGED</div>
<h1>Moving flame. Heavier net.</h1><p>The approved hearth stays intact. The net covers its cell for its entire lifetime.</p>
<div class="grid"><section><h2>Hearth Ember — flame only</h2><div class="samples">
<div class="sample"><div class="stage"><img width="128" height="128" src="../PixelLab/HearthEmber.png"></div><p>Approved still · 4×</p></div>
<div class="sample"><div class="stage"><img width="128" height="128" src="HearthFlame.gif"></div><p>Animated flame · 4×</p></div>
<div class="sample"><img width="32" height="32" src="HearthFlame.gif"><p>Native 1×</p></div></div>
<p>8 native cels · 120 ms per cel · 7 colors · binary transparency.<br>Every stone pixel remains fixed. Pause freezes the flame; Reduced Motion restores the still.</p>
<a href="HearthFlame.ase">LibreSprite source</a> · <a href="HearthFlame-strip.png">Native frame strip</a></section>
<section><h2>Marine Net — full cell</h2><div class="samples">
<div class="sample"><div class="stage"><img width="128" height="128" src="../PixelLab/MarineSnare.png"></div><p>Previous · same cell display size</p></div>
<div class="sample"><div class="stage"><img width="128" height="128" src="MarineSnare.png"></div><p>Thicker ropes · same cell display size</p></div>
<div class="sample"><img width="64" height="64" src="MarineSnare.png"><p>Native 64×64</p></div></div>
<p>6 colors · binary transparency · one-pixel edge clearance. Diamond windows keep the gem visible. No expiry shrink; the existing three-move rule remains unchanged.</p>
<a href="MarineSnare.ase">LibreSprite source</a> · <a href="MarineNet-raw-sheet.png">All raw PixelLab candidates</a></section></div>
<div class="grid"><section><h2>Actual Unity Game — hearth</h2>
<button id="play">Pause preview</button><img id="hearth" class="game" src="Preview/hearth-flame-00.png">
<p class="caption">Real Game renderer sampled at the eight authored flame times. Automatic playback, pause and Reduced Motion were also tested separately.</p></section>
<section><h2>Actual Unity Game — net</h2><select id="size"><option>1080x1920</option><option>360x640</option><option>1080x2400</option></select>
<img id="net" class="game" src="Preview/marine-net-full-tile-1080x1920.png">
<p class="caption">Real Game captures. Full-cell bounds and alignment checked at all three sizes, with three, two and one moves remaining. The 360×640 capture retains the existing Unity Editor reference-resolution warning.</p></section></div>
<section><h2>Verification and sources</h2><p><strong>Unity compilation and six focused Game checks passed.</strong> PixelLab: <strong>11 generations</strong> for this revision. LibreSprite exports verified against native cels. Unity captures use controlled combat fixtures; no real-device test was performed.</p>
<a href="validation.json">Executed checks</a> · <a href="manifest.json">Pixel checks and hashes</a> · <a href="README.md">Production notes</a></section>
</main><script>
let playing=true,frame=0;const h=document.querySelector('#hearth');
for(let i=0;i<8;i++){const p=new Image();p.src=`Preview/hearth-flame-${String(i).padStart(2,'0')}.png`}
setInterval(()=>{if(playing){frame=(frame+1)%8;h.src=`Preview/hearth-flame-${String(frame).padStart(2,'0')}.png`}},120);
document.querySelector('#play').onclick=e=>{playing=!playing;e.target.textContent=playing?'Pause preview':'Play preview'};
document.querySelector('#size').onchange=e=>document.querySelector('#net').src=`Preview/marine-net-full-tile-${e.target.value}.png`;
</script></html>''',encoding='utf-8')
print('Review packaged with 11 actual Unity captures, native artwork, and animation.')
